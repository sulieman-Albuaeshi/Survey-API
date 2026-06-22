using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.Interface;
using SurveyBusinessLayer.DTOs;


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
    public async Task<ActionResult<IEnumerable<SurveyDto>>> GetAllSurveys(int pageSize, int pageNumber)
    {
        try
        {
            var surveyList = await _surveyService.GetAllSurveysAsync(pageSize, pageNumber);
            
            return Ok(surveyList);
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

    // need to return SurveyDetailsDto but for now we return surveyDto
    public async Task<ActionResult<SurveyDto>> GetSurveyByIdAsync(int id)
    {
        try
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id);
            //var (questionList, choiceList) = await _surveyService.GetQuestionsForSurveyAsync(id);
            //var surveyDetailsDto = SurveyMapper.ToSurveyDetailsDto(survey, questionList, choiceList);
            return Ok(survey);

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
    public async Task<ActionResult<SurveyDetailsDto>> CreateSurvey(CreateSurveyDto surveyDto)
    {
        try
        {
            // TODO : Get userId from the authenticated user context instead of hardcoding it
            surveyDto.userId = "Test";
            var  survey = await _surveyService.CreateSurveyWithQuestionsAsync(surveyDto);
            if (survey.Id > 0)
            {
                return CreatedAtRoute("GetSurveyById", new { id = survey.Id }, survey);
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
    public async Task<ActionResult<SurveyDto>> UpdateSurvey(int id, UpdaatSurveyDto dto)
    {
        try
        {
            dto.Id = id;
            var survey  = await _surveyService.UpdateSurveyWithQuestionsAsync(dto);

            if (survey == null) return BadRequest("Failed to update survey");

            return Ok(survey);
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
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }


    [HttpPatch("{id}/status", Name = "ChangeSurveyStatus")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeSurveyStatus(int id, SurveyStatusDto status)
    {
        if(string.IsNullOrEmpty(status.StatusText))
        {
            return BadRequest("Status text cannot be null or empty");
        }

        try
        {
            var updated = await _surveyService.ChangeSurveyStatusAsync(id, status.StatusText);

            if (!updated)
                return NotFound("Failed to update survey status");

            return NoContent();
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
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }

    }
}