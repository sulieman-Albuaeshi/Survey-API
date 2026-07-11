using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Interface
{
    public interface IAuthService
    {
        public Task<string?> Login(UserLoginDto userLoginDto);
        //public Task Logout(string userId);
    }
}
