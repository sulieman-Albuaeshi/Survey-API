using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;



namespace StudentApi.Controllers
{
    // This controller is responsible for authentication-related actions,
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        readonly private IAuthService _authService;
        readonly private ILogger<AuthController> _logger;
        readonly private IValidator<UserLoginDto> _loginValidator;
        public AuthController(
            IAuthService authService, 
            IValidator<UserLoginDto> loginValidator,
            ILogger<AuthController> logger
            )
        {
            _authService = authService;
            _loginValidator = loginValidator;
            _logger = logger;
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            var validationResult = _loginValidator.Validate(request);

            if(!validationResult.IsValid)
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                _logger.LogWarning($"Login attempt failed with Email: {request.Email}, IP: {ip}");
                return BadRequest(validationResult.ToDictionary());
            }

            var loginToken = await _authService.Login(request);
            return Ok(new
            {
                accessToken = loginToken?.AccessToken,
                refreshToken = loginToken?.RefreshToken
            });
        }

        [HttpPost("refresh-token")]
        [EnableRateLimiting("AuthLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]

        public async Task<ActionResult<RefreshTokenDto?>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if(string.IsNullOrEmpty(request.RefreshToken))
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                _logger.LogWarning($"Login attempt failed with Email: {request.Email}, IP: {ip}");
                return BadRequest(new { error = "Refresh token is required" });
            }

            if(string.IsNullOrEmpty(request.Email))
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                _logger.LogWarning($"Login attempt failed with Email: {request.Email}, IP: {ip}");
                return BadRequest(new { error = "Email is required" });
            }
            
            var newAccessToken = await _authService.RefreshToken(request);

            return Ok(new RefreshTokenResponseDto
            {
                RefreshToken = newAccessToken?.RefreshToken,
            });
        
        }

        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if(emailClaim == null || emailClaim.Value != refreshTokenRequestDto.Email)
                return Ok(); // returning 200 OK even if the email doesn't match, to avoid revealing information about the user's session
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
               return Ok(); 
               
             var logoutSuccessful = await _authService.Logout(userId, refreshTokenRequestDto.RefreshToken);
            if (logoutSuccessful == null || logoutSuccessful == false)
                return Ok();    
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
