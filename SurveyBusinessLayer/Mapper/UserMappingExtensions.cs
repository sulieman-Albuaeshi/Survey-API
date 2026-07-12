using System;
using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Mapper;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    public static UserDetailsDto ToDetailsDto(this User user)
    {
        return new UserDetailsDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public static User ToDomainEntity(this CreateUserDto dto)
    {
        return new User
        {
            Email = dto.Email,
            PasswordHash = dto.Password, 
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public static User ToDomainEntity(this UpdateUserDto dto)
    {
        return new User
        {
            Id = dto.Id,
            Email = dto.Email,
            PasswordHash = dto.Password, 
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,
            IsActive = dto.IsActive
        };
    }
}
