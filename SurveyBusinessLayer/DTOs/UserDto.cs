 using System;

namespace SurveyBusinessLayer.DTOs;

public interface IUserBaseDto
{
    string Email { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string Role { get; set; }
    string Password { get; set; }
}

public class UserDto 
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; }
}

public class UserDetailsDto : UserDto
{
    public DateTime CreatedAt { get; set; }
    
}

public class CreateUserDto : IUserBaseDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class UpdateUserDto : CreateUserDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}

public class UserLoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RefreshTokenDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class RefreshTokenResponseDto
{
    public string RefreshToken { get; set; } = null!;
}
