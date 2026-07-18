using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Repository.Models;
using SurveyBusinessLayer.Interface;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.utility;
using System.Security.Cryptography;

namespace SurveyBusinessLayer
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository authRepository, IConfiguration configuration)
        {
            _userRepository = authRepository;
            _configuration = configuration;

        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				// Role (User or Creator) used later for authorization
				new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RefreshTokenDto?> Login(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            bool isValidPassword = MyUtility.verifyPassword(userLoginDto.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new UnauthorizedAccessException("Invalid credentials");

            var refreshTokenHash = MyUtility.hashPassword(GenerateRefreshToken());
            var refreshTokenExpiryDays = Convert.ToInt32(_configuration["JwtSettings:RefreshTokenExpiryDays"]);
            var expiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshTokenHash, expiryTime);

            var accessToken = GenerateJwtToken(user);

            return new RefreshTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenHash
            };
        }

        public async Task<RefreshTokenDto> RefreshToken(RefreshTokenRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired");

            if (user.RefreshTokenRevokedAt != null)
                throw new UnauthorizedAccessException("Refresh token has been revoked");
            
            var isValidRefreshToken = MyUtility.verifyPassword(request.RefreshToken, user.RefreshTokenHash);

            if (!isValidRefreshToken)   
                throw new UnauthorizedAccessException("Invalid refresh token");

            var newRefreshToken = MyUtility.hashPassword(GenerateRefreshToken());
            var refreshTokenExpiryDays = Convert.ToInt32(_configuration["JwtSettings:RefreshTokenExpiryDays"]);
            var expiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            await _userRepository.UpdateRefreshTokenAsync(user.Id, newRefreshToken, expiryTime);

            return new RefreshTokenDto{
                AccessToken = GenerateJwtToken(user),
                RefreshToken = newRefreshToken
            };
        
        }

        public async Task Logout(Guid userId)
        {
            await _userRepository.RevokeRefreshTokenAsync(userId);
        }
    }
}
