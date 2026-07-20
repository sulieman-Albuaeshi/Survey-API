using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Repository.Models;
using SurveyBusinessLayer;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;
using System.Security.Claims;


namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/Response")]
[Authorize]
public class ResponseController : ControllerBase
{
    readonly IResponseService _responseService;
    readonly ISurveyService _surveyService;
    readonly IValidator<ResponseCreateDto> _createResponseValidator;

    public ResponseController(IResponseService responseService, IValidator<ResponseCreateDto> createResponseValidator, ISurveyService surveyService)
    {
        _responseService = responseService;
        _createResponseValidator = createResponseValidator;
        _surveyService = surveyService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("All",  Name = "GetAllResponses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ResponseDto>>> GetAllResponses(int pageSize, int pageNumber)
    {
        if (pageSize <= 0 || pageNumber <= 0) return BadRequest("Page size and page number must be greater than zero");

        var responses = await _responseService.GetAllResponsesDetailsAsync(pageSize, pageNumber);
            
        if(responses == null || !responses.Any())
            return NotFound("No responses found.");
            
        return Ok(responses);
       
    }
    
    [HttpGet("survey/{surveyId}", Name = "GetResponsesBuSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public  async Task<ActionResult<IEnumerable<ResponseDto>>> GetResponsesBySurveyId(int surveyId, int pageSize, int pageNumber,
        [FromServices] IAuthorizationService authorizationService)
    {
        if (pageSize <= 0 || pageNumber <= 0) return BadRequest("Page size and page number must be greater than zero");
        if (surveyId <= 0) return BadRequest("Invalid Survey ID");

        var userId = await _surveyService.GetUserIdBySurveyIdAsync(surveyId);
        var Authservice = await authorizationService.AuthorizeAsync(User, userId, "EditDeleteResuorse");

        if(!Authservice.Succeeded)
            return Forbid("You are not authorized to view responses for this survey.");

        var responses = await _responseService.GetResponsesBySurveyIdAsync(surveyId, pageSize, pageNumber);
            
        if(responses == null || !responses.Any())
            return NotFound($"No responses found for survey ID {surveyId}.");
            
        return Ok(responses);

    }

    [Authorize(Roles = "Creator")]
    [HttpGet("user/{userId}", Name = "GetResponsesByUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ResponseDto>>> GetResponsesByUserId(int pageSize, int pageNumber,
        [FromServices] IAuthorizationService service)
    {
        if (pageSize <= 0 || pageNumber <= 0) return BadRequest("Page size and page number must be greater than zero");

        var userId = Guid.Empty;
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        var responses = await _responseService.GetResponsesByUserIdAsync(userId, pageSize, pageNumber);
            
        if(responses == null || !responses.Any())
            return NotFound($"No responses found for user ID {userId}.");
            
        return Ok(responses);
       
    }

    [Authorize(Roles = "Creator")]
    [HttpGet("{responseId}", Name = "GetResponseById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]    
    public async Task<ActionResult<ResponseDto>> GetResponseById(int responseId)
    {
        if (responseId <= 0) return BadRequest("Invalid Response ID");

        var userId = Guid.Empty;
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        var response = await _responseService.GetResponseByIdAsync(responseId, userId);
            
        if(response == null)
            return NotFound($"No response found with ID {responseId}.");
            
        return Ok(response);
    }

    [AllowAnonymous]
    [EnableRateLimiting("ResponseLimiter")]
    [HttpPost("survey/{surveyId}", Name = "CreateResponse")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult> SubmitResponse([FromBody] ResponseCreateDto responseCreateDto)
    { 
        var validationResult = _createResponseValidator.Validate(responseCreateDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

        var createdResponse = await _responseService.SubmitResponseAsync(responseCreateDto, isAuthenticated);
        return CreatedAtRoute("GetResponseById", new { responseId = createdResponse.ResponseId}, createdResponse);
    }

    [Authorize(Roles = "Admin, Creator")]
    [HttpGet("survey/{surveyId}/Count", Name = "GetResponsesCount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResponseCountBySurveyId(int surveyId, [FromServices] IAuthorizationService authorizationService)
    {
        if (surveyId <= 0) return BadRequest("Invalid Survey ID");

        var userId = await _surveyService.GetUserIdBySurveyIdAsync(surveyId);
        var authservice = await authorizationService.AuthorizeAsync(User, userId, "EditDeleteResuorse");

        if (!authservice.Succeeded)
            return Forbid();


        var count = await _responseService.GetResponsesCountAsync(surveyId);
        return Ok(new { SurveyId = surveyId, ResponseCount = count });
    }

}