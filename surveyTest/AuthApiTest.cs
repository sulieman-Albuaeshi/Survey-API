using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Repository.Data;
using Repository.Models;
using SurveyBusinessLayer.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using SurveyBusinessLayer.utility;
using SurveyBusinessLayer;

using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;
using Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace surveyTest
{
    public class AuthApiTest
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly TestAuthHandler _authHandler;

        public AuthApiTest(ITestOutputHelper output)
        {
            var application = new SurveyWebApplicationFactory();
            _client = application.CreateClient();
            _output = output;
            _configuration = application.Server.Services.GetRequiredService<IConfiguration>();
            _authHandler = new TestAuthHandler(_configuration, _client);
            
            // Ensure database is created
            var connectionString = GetTestConnectionString();
        }

        private static string GetTestConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.Test.json", optional: false, reloadOnChange: true)
                .Build();
            return config.GetConnectionString("TestConnection") ?? "";
        }

        private AppDbContext CreateTestDbContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.Test.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("TestConnection");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        private User CreateTestUserInDatabase(string email = "test@example.com", string password = "Password123!", string role = "Creator")
        {
            var dbContext = CreateTestDbContext();
            
            // Clean up any existing test user with this email
            var existingUser = dbContext.User.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                dbContext.User.Remove(existingUser);
                dbContext.SaveChanges();
            }

            var passwordHash = MyUtility.hashPassword(password);
            
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                Role = role,
                RefreshTokenHash = "",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1),
                RefreshTokenRevokedAt = null
            };

            dbContext.User.Add(user);
            dbContext.SaveChanges();

            return user;
        }


        private void ReadResponseContent(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            _output.WriteLine($"Response Content: {content}");
        }

        private string GenerateValidRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
        {
            // Arrange
            var email = "login_test@example.com";
            var password = "Password123!";
            CreateTestUserInDatabase(email, password, "Creator");

            var loginDto = new UserLoginDto
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(responseContent);
            Assert.True(responseContent.ContainsKey("accessToken"));
            Assert.True(responseContent.ContainsKey("refreshToken"));
            Assert.False(string.IsNullOrEmpty(responseContent["accessToken"]));
            Assert.False(string.IsNullOrEmpty(responseContent["refreshToken"]));
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var email = "invalid_pass_test@example.com";
            var password = "Password123!";
            CreateTestUserInDatabase(email, password, "Creator");

            var loginDto = new UserLoginDto
            {
                Email = email,
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithMissingEmail_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "",
                Password = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithMissingPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var email = "refresh_test@example.com";
            var password = "Password123!";

            // First login to get refresh token
            var loginDto = new UserLoginDto { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var refreshToken = loginContent["refreshToken"];

            var refreshRequest = new RefreshTokenRequestDto
            {
                Email = email,
                RefreshToken = refreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();
            Assert.NotNull(responseContent);
            Assert.False(string.IsNullOrEmpty(responseContent?.RefreshToken));
        }

        [Fact]
        public async Task RefreshToken_WithInvalidRefreshToken_ReturnsUnauthorized()
        {
            // Arrange
            var email = "invalid_refresh@example.com";
            var password = "Password123!";
            CreateTestUserInDatabase(email, password, "Creator");

            var refreshRequest = new RefreshTokenRequestDto
            {
                Email = email,
                RefreshToken = "invalid_refresh_token"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithExpiredRefreshToken_ReturnsUnauthorized()
        {
            // Arrange
            var email = "expired_refresh@example.com";

            // Manually set an expired refresh token in database
            var dbContext = CreateTestDbContext();
            var dbUser = await dbContext.User.FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser != null)
            {
                
                dbUser.RefreshTokenHash = MyUtility.hashPassword(GenerateValidRefreshToken());
                dbUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1); // Expired
                await dbContext.SaveChangesAsync();
            }

            var refreshRequest = new RefreshTokenRequestDto
            {
                Email = email,
                RefreshToken = GenerateValidRefreshToken()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithRevokedRefreshToken_ReturnsUnauthorized()
        {
            // Arrange
            var email = "revoked_refresh@example.com";
            var password = "Password123!";
            var user = CreateTestUserInDatabase(email, password, "Creator");

            // First login to get a valid refresh token
            var loginDto = new UserLoginDto { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var refreshToken = loginContent["refreshToken"];

            // Revoke the refresh token in database
            var dbContext = CreateTestDbContext();
            var dbUser = await dbContext.User.FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser != null)
            {
                dbUser.RefreshTokenRevokedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }

            var refreshRequest = new RefreshTokenRequestDto
            {
                Email = email,
                RefreshToken = refreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithMissingEmail_ReturnsBadRequest()
        {
            // Arrange
            var refreshRequest = new RefreshTokenRequestDto
            {
                Email = "",
                RefreshToken = "some_token"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithMissingRefreshToken_ReturnsBadRequest()
        {
            // Arrange
            var refreshRequest = new RefreshTokenRequestDto
            {
                Email = "test@example.com",
                RefreshToken = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Logout_WithValidTokenAndRefreshToken_ReturnsOk()
        {
            // Arrange
            var email = "logout_test@example.com";
            var password = "Password123!";
            var user = CreateTestUserInDatabase(email, password, "Creator");

            // Login to get tokens
            var loginDto = new UserLoginDto { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var accessToken = loginContent["accessToken"];
            var refreshToken = loginContent["refreshToken"];

            // Add auth header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var logoutRequest = new RefreshTokenRequestDto
            {
                Email = email,
                RefreshToken = refreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/logout", logoutRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(responseContent);
            Assert.True(responseContent.ContainsKey("message"));
        }

        [Fact]
        public async Task Logout_WithoutAuthHeader_ReturnsUnauthorized()
        {
            // Arrange
            var logoutRequest = new RefreshTokenRequestDto
            {
                Email = "test@example.com",
                RefreshToken = "some_token"
            };

            // Ensure no auth header
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/logout", logoutRequest);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_WithInvalidRefreshToken_ReturnsOk()
        {
            // Arrange
            var email = "logout_invalid_refresh@example.com";
            var password = "Password123!";
            var user = CreateTestUserInDatabase(email, password, "Creator");

            // Login to get access token
            var loginDto = new UserLoginDto { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var accessToken = loginContent["accessToken"];

            // Add auth header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var logoutRequest = new RefreshTokenRequestDto
            {
                Email = email,
                RefreshToken = "invalid_refresh_token"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/logout", logoutRequest);
            ReadResponseContent(response);

            // Assert - should return OK even with invalid refresh token (security by obscurity)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Logout_WithEmailMismatch_ReturnsOk()
        {
            // Arrange
            var email = "logout_mismatch@example.com";
            var password = "Password123!";
            var user = CreateTestUserInDatabase(email, password, "Creator");

            // Login to get access token
            var loginDto = new UserLoginDto { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var accessToken = loginContent["accessToken"];

            // Add auth header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Use different email in logout request
            var logoutRequest = new RefreshTokenRequestDto
            {
                Email = "different@example.com",
                RefreshToken = "some_token"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/logout", logoutRequest);
            ReadResponseContent(response);

            // Assert - should return OK to avoid revealing information
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithAdminRole_ReturnsAdminToken()
        {
            // Arrange
            var email = "admin_test@example.com";
            var password = "Password123!";
            CreateTestUserInDatabase(email, password, "Admin");

            var loginDto = new UserLoginDto
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(responseContent);
            Assert.False(string.IsNullOrEmpty(responseContent["accessToken"]));

            // Verify the token contains admin role
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(responseContent["accessToken"]);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            Assert.NotNull(roleClaim);
            Assert.Equal("Admin", roleClaim.Value);
        }

        [Fact]
        public async Task Login_WithRespondentRole_ReturnsRespondentToken()
        {
            // Arrange
            var email = "respondent_test@example.com";
            var password = "Password123!";
            CreateTestUserInDatabase(email, password, "Respondent");

            var loginDto = new UserLoginDto
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(responseContent);
            Assert.False(string.IsNullOrEmpty(responseContent["accessToken"]));

            // Verify the token contains respondent role
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(responseContent["accessToken"]);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            Assert.NotNull(roleClaim);
            Assert.Equal("Respondent", roleClaim.Value);
        }

        [Fact]
        public async Task RefreshToken_RotatesRefreshToken()
        {
            // Arrange
            var email = "Admin@koko.com";
            var password = "123456";  

            // Login to get initial tokens
            var loginDto = new UserLoginDto { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<RefreshTokenDto>();
            var oldRefreshToken = loginContent?.RefreshToken;

            var refreshRequest = new RefreshTokenRequestDto
            {
                Email = email,
                RefreshToken = oldRefreshToken
            };

            // Act - First refresh
            var response1 = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            var content1 = await response1.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();
            var newRefreshToken1 = content1?.RefreshToken;
            // Try to use the old refresh token again
            var response2 = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
            
            // Assert - second attempt should fail because token was rotated
            Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);
        }
    }
}