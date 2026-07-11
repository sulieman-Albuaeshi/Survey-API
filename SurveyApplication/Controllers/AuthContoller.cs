using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;


namespace StudentApi.Controllers
{
    // This controller is responsible for authentication-related actions,
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        readonly private IAuthService _authService;
        readonly private IValidator<UserLoginDto> _loginValidator;
        public AuthController(IAuthService authService, IValidator<UserLoginDto> userValidator)
        {
            _authService = authService;
            _loginValidator = userValidator;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            var validationResult = _loginValidator.Validate(request);

            if(!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var SecurityToken = await _authService.Login(request);

            return Ok(new
            {
                token = SecurityToken,
            });
        }
    }
}