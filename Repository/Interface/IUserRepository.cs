using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interface;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(User user);
    Task<int> DeleteUserAsync(Guid userId);
    Task<bool> IsUserExist(Guid userId);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<User?> GetUserByEmailAndPasswordAsync(string email, string passwordHash);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime);
    Task<bool> RevokeRefreshTokenAsync(Guid userId);
}
