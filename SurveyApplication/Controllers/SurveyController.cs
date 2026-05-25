using DTOs;
using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.Interface;
using Entities;
using SurveyApplication.Mapper;

namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/surveys")]
public class SurveyController : ControllerBase
{
    private readonly ISurveyService _surveyService;
    public SurveyController(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }
    
    [HttpGet("All", Name = "GetAllSurveys")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SurveyTableDto>>> GetAllSurveys()
    {
        try
        {
            var surveyList = await _surveyService.GetAllSurveysAsync();
            
            var surveyTableDtos = surveyList.Select(survey => SurveyMapper.ToSurveyTableDtos(survey)).ToList();
            return Ok(surveyTableDtos);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }
    
    [HttpGet("{id}", Name = "GetSurveyById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SurveyDetailsDto>> GetSurveyByIdAsync(int id)
    {
        try
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id);
            var (questionList, choiceList) = await _surveyService.GetQuestionsForSurveyAsync(id);
            var surveyDetailsDto = SurveyMapper.ToSurveyDetailsDto(survey, questionList, choiceList);
            return Ok(surveyDetailsDto);

        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch(Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    [HttpPost("Create", Name = "CreateSurvey")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SurveyDto>> CreateSurvey(SurveyDto surveyDto)
    {
        try
        {
            var survey = SurveyMapper.ToSurveyEntity(surveyDto);
            var  surveyId = await _surveyService.CreateSurveyAsync(survey);
            surveyDto.Id = surveyId;
            if (surveyId > 0)
            {
                return CreatedAtRoute("GetSurveyById", new { id = surveyId }, surveyDto);
            }
            return BadRequest("Failed to create survey");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch(Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    [HttpPut("{id}", Name = "UpdateSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SurveyDto>> UpdateSurvey(int id, SurveyDto dto)
    {
        try
        {
            var survey = new Survey
            {
                Id = id,
                Title = dto.Title,
                Description = dto.Description,
                IsAnonymous = dto.IsAnonymous,
                IsActive = dto.IsActive,
                UserId = dto.UserId,
                Status = Enum.TryParse<SurveyStatus>(dto.Status, out var status) ? status : SurveyStatus.Draft,
            };
        
            var  numberOfRowEffected  = await _surveyService.UpdateSurveyAsync(survey);

            if (numberOfRowEffected > 0)
            {
                var srv = SurveyMapper.ToSurveyDto(survey);
                return Ok(srv);
            }
            return BadRequest("Failed to update survey");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return Conflict(e.Message);
        }
        catch(Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    
    [HttpDelete("{id}", Name = "DeleteSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteSurvey(int id)
    {
        try
        {
            var deleted = await _surveyService.DeleteSurveyAsync(id);
            if (!deleted)
                return NotFound("Failed to delete survey");

            return Ok($"Survey with id {id} has been deleted");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch(Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
}