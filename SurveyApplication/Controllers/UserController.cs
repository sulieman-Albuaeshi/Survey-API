using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;

namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserDto> _createValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;

    public UserController(
        IUserService userService, 
        IValidator<CreateUserDto> createValidator, 
        IValidator<UpdateUserDto> updateValidator)
    {
        _userService = userService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsDto>> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound("User not found");

        return Ok(user);
    }

    [HttpPost("Create", Name = "CreateUser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto>> CreateUser(CreateUserDto userDto)
    {
        var validationResult = _createValidator.Validate(userDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var createdUser = await _userService.CreateUserAsync(userDto);
        if (createdUser != null && createdUser.Id != Guid.Empty)
        {
            return CreatedAtRoute("GetUserById", new { id = createdUser.Id }, createdUser);
        }

        return BadRequest("Failed to create user");
    }

    [HttpPut("{id:guid}", Name = "UpdateUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailsDto>> UpdateUser(Guid id, UpdateUserDto userDto)
    {
        userDto.Id = id;

        var validationResult = _updateValidator.Validate(userDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var updatedUser = await _userService.UpdateUserAsync(userDto);
        if (updatedUser == null) return BadRequest("Failed to update user");

        return Ok(updatedUser);
    }

    [HttpDelete("{id:guid}", Name = "DeleteUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound("Failed to delete user");

        return Ok($"User with id {id} has been deleted");
    }
}
