using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;
using System.Security.Claims;


namespace SurveyApplication.Controllers;

[Authorize]
[ApiController]
[Route("api/surveys")]
public class SurveyController : ControllerBase
{
    private readonly ISurveyService _surveyService;
    private readonly IValidator<CreateSurveyDto> _createValidator;
    private readonly IValidator<UpdaatSurveyDto> _updateValidator;
    public SurveyController(ISurveyService surveyService, IValidator<CreateSurveyDto> CreateValidator, IValidator<UpdaatSurveyDto> UpdateValidator)
    {
        _surveyService = surveyService;
        _createValidator = CreateValidator;
        _updateValidator = UpdateValidator;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("All", Name = "GetAllSurveys")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SurveyDto>>> GetAllSurveys(int pageSize, int pageNumber)
    {
        if(pageSize <= 0 || pageNumber <= 0) return BadRequest("Page size and page number must be greater than zero");

        var surveyList = await _surveyService.GetAllSurveysAsync(pageSize, pageNumber);
            
        return Ok(surveyList);        
    }

    [AllowAnonymous]
    [HttpGet("{id}", Name = "GetSurveyById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public async Task<ActionResult<SurveyDetailsDto>> GetSurveyByIdAsync(int id)
    {
        if(id <= 0) return BadRequest("Invalid Survey ID");

        var userAuthId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var survey = await _surveyService.GetSurveyByIdAsync(id, userAuthId, userRole);
        return Ok(survey);
    }

    [Authorize(Roles = "Creator")]
    [HttpPost("Create", Name = "CreateSurvey")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SurveyDetailsDto>> CreateSurvey(CreateSurveyDto surveyDto)
    {
        var validationResult = _createValidator.Validate(surveyDto);

        if (!validationResult.IsValid)
        {
            // Fail Fast: 400 Bad Request if syntax/structure is wrong
            return BadRequest(validationResult.ToDictionary());
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        surveyDto.userId = userId ?? throw new UnauthorizedAccessException("");

        var  survey = await _surveyService.CreateSurveyWithQuestionsAsync(surveyDto);
        if (survey.Id > 0)
        {
            return CreatedAtRoute("GetSurveyById", new { id = survey.Id }, survey);
        }
        return BadRequest("Failed to create survey");

    }

    [Authorize(Roles = "Creator")]
    [HttpPut("{id}", Name = "UpdateSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SurveyDto>> UpdateSurvey(int id, UpdaatSurveyDto dto, [FromServices] IAuthorizationService authorizationService)
    {
        if(id <= 0) return BadRequest("Invalid Survey ID");
        dto.Id = id;

        var validationResult = _updateValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            // Fail Fast: 400 Bad Request if syntax/structure is wrong
            return BadRequest(validationResult.ToDictionary());
        }


        var userId = await _surveyService.GetUserIdBySurveyIdAsync(id);
        dto.userId = userId;

        var authService = await authorizationService.AuthorizeAsync(User, dto.userId, "EditDeleteResuorse");

        if(!authService.Succeeded)
            return Forbid();

        var survey  = await _surveyService.UpdateSurveyWithQuestionsAsync(dto);

        if (survey == null) return BadRequest("Failed to update survey");

        return Ok(survey);
    }

    [Authorize(Roles = "Creator")]
    [HttpDelete("{id}", Name = "DeleteSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteSurvey(int id, [FromServices] IAuthorizationService authorizationService)
    {
        if (id <= 0) return BadRequest("Invalid Survey ID");

        var userId = await _surveyService.GetUserIdBySurveyIdAsync(id);
        var AuthService = await authorizationService.AuthorizeAsync(User, userId, "EditDeleteResuorse");

        if(!AuthService.Succeeded)
            return Forbid();

        var deleted = await _surveyService.DeleteSurveyAsync(id);
        if (!deleted)
            return NotFound("Failed to delete survey");

        return Ok($"Survey with id {id} has been deleted");

    }

    [Authorize(Roles = "Creator")]
    [HttpPatch("{id}/status", Name = "ChangeSurveyStatus")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeSurveyStatus(int id, SurveyStatusDto status, [FromServices] IAuthorizationService authorizationService)
    {
        var updated = await _surveyService.ChangeSurveyStatusAsync(id, status.StatusText);

        var userId = await _surveyService.GetUserIdBySurveyIdAsync(id);
        var authService = await authorizationService.AuthorizeAsync(User, userId, "EditDeleteResuorse");

        if (!authService.Succeeded)
            return Forbid();

        if (!updated)
            return NotFound("Failed to update survey status");

        return NoContent();
    }
}