using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurveyBusinessLayer.Interface;
using Repository.Interface;
using Repository.Models;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Mapper;

namespace SurveyBusinessLayer
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
       
        public async Task<UserDetailsDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return user.ToDetailsDto();
        }

        public async Task<UserDetailsDto> CreateUserAsync(CreateUserDto dto)
        {
            var mappedUser = dto.ToDomainEntity();
            // hash the password

            if (await _userRepository.IsEmailUniqueAsync(mappedUser.Email))
            {
                throw new ArgumentException("Email is already in use.");
            }

            var createdUser = await _userRepository.CreateUserAsync(mappedUser);

            if (createdUser == null)
                throw new KeyNotFoundException("User was not created.");

            return createdUser.ToDetailsDto();
        }

        public async Task<UserDetailsDto> UpdateUserAsync(UpdateUserDto dto)
        {
            var mappedUser = dto.ToDomainEntity();

            var updatedUser = await _userRepository.UpdateUserAsync(mappedUser);

            if (updatedUser == null)
                throw new KeyNotFoundException("User was not updated.");

            return updatedUser.ToDetailsDto();
        }
        
        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            return await _userRepository.DeleteUserAsync(userId) == 1;
        }
        public async Task<User?> GetUserByEmailAndPasswordAsync(string email, string password)
        {
            // the password should be hashed before calling this method

            return await _userRepository.GetUserByEmailAndPasswordAsync(email, password);
        }
    }
}
