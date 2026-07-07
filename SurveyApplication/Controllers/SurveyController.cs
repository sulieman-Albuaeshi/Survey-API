using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SurveyApplication.Validation;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;


namespace SurveyApplication.Controllers;

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
    
    [HttpGet("All", Name = "GetAllSurveys")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SurveyDto>>> GetAllSurveys(int pageSize, int pageNumber)
    {
        if(pageSize <= 0 || pageNumber <= 0) return BadRequest("Page size and page number must be greater than zero");

        var surveyList = await _surveyService.GetAllSurveysAsync(pageSize, pageNumber);
            
        return Ok(surveyList);        
    }
    
    [HttpGet("{id}", Name = "GetSurveyById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    // need to return SurveyDetailsDto but for now we return surveyDto
    public async Task<ActionResult<SurveyDto>> GetSurveyByIdAsync(int id)
    {
        if(id <= 0) return BadRequest("Invalid Survey ID");

        var survey = await _surveyService.GetSurveyByIdAsync(id);
        //var (questionList, choiceList) = await _surveyService.GetQuestionsForSurveyAsync(id);
        //var surveyDetailsDto = SurveyMapper.ToSurveyDetailsDto(survey, questionList, choiceList);
        return Ok(survey);
    }
    
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

        var  survey = await _surveyService.CreateSurveyWithQuestionsAsync(surveyDto);
        if (survey.Id > 0)
        {
            return CreatedAtRoute("GetSurveyById", new { id = survey.Id }, survey);
        }
        return BadRequest("Failed to create survey");

    }
    
    [HttpPut("{id}", Name = "UpdateSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SurveyDto>> UpdateSurvey(int id, UpdaatSurveyDto dto)
    {
        if(id <= 0) return BadRequest("Invalid Survey ID");
        dto.Id = id;

        var validationResult = _updateValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            // Fail Fast: 400 Bad Request if syntax/structure is wrong
            return BadRequest(validationResult.ToDictionary());
        }

        var survey  = await _surveyService.UpdateSurveyWithQuestionsAsync(dto);

        if (survey == null) return BadRequest("Failed to update survey");

        return Ok(survey);

    }
    
    
    [HttpDelete("{id}", Name = "DeleteSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteSurvey(int id)
    {
        if (id <= 0) return BadRequest("Invalid Survey ID");

        var deleted = await _surveyService.DeleteSurveyAsync(id);
        if (!deleted)
            return NotFound("Failed to delete survey");

        return Ok($"Survey with id {id} has been deleted");

    }


    [HttpPatch("{id}/status", Name = "ChangeSurveyStatus")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeSurveyStatus(int id, SurveyStatusDto status)
    {
        var updated = await _surveyService.ChangeSurveyStatusAsync(id, status.StatusText);

        if (!updated)
            return NotFound("Failed to update survey status");

        return NoContent();
    }
}