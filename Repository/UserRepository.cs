using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interface;
using Repository.Models;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.User.FindAsync(userId);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            _context.User.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<int> DeleteUserAsync(Guid userId)
        {
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return 0;
            }

            _context.User.Remove(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByEmailAndPasswordAsync(string email, string passwordHash)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.User.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsUserExist(Guid userId)
        {
            return await _context.User.AnyAsync(u => u.Id == userId);
        }
    }
}
