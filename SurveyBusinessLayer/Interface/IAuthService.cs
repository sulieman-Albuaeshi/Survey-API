using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Interface
{
    public interface IAuthService
    {
        public Task<RefreshTokenDto?> Login(UserLoginDto userLoginDto);
        public Task<RefreshTokenDto?> RefreshToken(RefreshTokenRequestDto request);
        public Task Logout(Guid userId);
    }
}
