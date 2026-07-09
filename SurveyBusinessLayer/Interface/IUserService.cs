using SurveyBusinessLayer.DTOs;
using Repository.Models;

namespace SurveyBusinessLayer.Interface;

public interface IUserService
{
    Task<UserDetailsDto?> GetUserByIdAsync(Guid userId);
    Task<UserDetailsDto> CreateUserAsync(CreateUserDto user);
    Task<UserDetailsDto> UpdateUserAsync(UpdateUserDto user);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<User?> GetUserByEmailAndPasswordAsync(string email, string password);
}
