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

        public async Task<string?> Login(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            bool isValidPassword =  MyUtility.verifyPassword(userLoginDto.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new UnauthorizedAccessException("Invalid credentials");

            return GenerateJwtToken(user);
        }

        public async Task Logout(string userId)
        {
            //await _userRepository.Logout(userId);
        }
    }
}
