using DTOs;
using Microsoft.AspNetCore.Mvc;
using SurveyApplication.Mapper;
using SurveyBusinessLayer.Interface;

namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/Response")]
public class ResponseController : ControllerBase
{
    readonly IResponseService _responseService;

    public ResponseController(IResponseService responseService)
    {
        _responseService = responseService;
    }
    
    [HttpGet("All",  Name = "GetAllResponses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ResponseDto>>> GetAllResponses()
    {
        try
        {
            var responses = await _responseService.GetAllResponsesDetailsAsync();
            
            if(responses == null || !responses.Any())
                return NotFound("No responses found.");
            
            return Ok(responses);
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }
    
    [HttpGet("survey/{surveyId}", Name = "GetResponsesBuSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public  async Task<ActionResult<IEnumerable<ResponseDto>>> GetResponsesBySurveyId(int surveyId)
    {
        try
        {
            var responses = await _responseService.GetResponsesBySurveyIdAsync(surveyId);
            
            if(responses == null || !responses.Any())
                return NotFound($"No responses found for survey ID {surveyId}.");
            
            return Ok(responses);

        }
        catch (ArgumentException ex)
        {
            // Log the exception (not implemented here)
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            // Log the exception (not implemented here)
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }
    
    [HttpGet("user/{userId}", Name = "GetResponsesByUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ResponseDto>>> GetResponsesByUserId(string userId)
    {
        try
        {
            var responses = await _responseService.GetResponsesByUserIdAsync(userId);
            
            if(responses == null || !responses.Any())
                return NotFound($"No responses found for user ID {userId}.");
            
            return Ok(responses);
        }
        catch (ArgumentException ex)
        {
            // Log the exception (not implemented here)
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            // Log the exception (not implemented here)
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }
    
    [HttpGet("{responseId}", Name = "GetResponseById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]    
    public async Task<ActionResult<ResponseDto>> GetResponseById(int responseId)
    {
        try
        {
            var response = await _responseService.GetResponseByIdAsync(responseId);
            
            if(response == null)
                return NotFound($"No response found with ID {responseId}.");
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            // Log the exception (not implemented here)
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            // Log the exception (not implemented here)
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }
    
    
    [HttpPost("survey/{surveyId}", Name = "CreateResponse")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateResponse([FromBody] ResponseCreateDto responseCreateDto)
    {
        try
        {
            var createdResponse = await _responseService.CreateResponseAsync(responseCreateDto);
            return CreatedAtRoute("GetResponseById", new { responseId = createdResponse }, createdResponse);
        }
        catch (ArgumentException ex)
        {
            // Log the exception (not implemented here)
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }
    
    
    [HttpDelete("survey/{surveyId}", Name = "DeleteResponsesBySurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteResponsesBySurveyId(int surveyId)
    {
        try
        {
            var deletedCount = await _responseService.DeleteResponsesAsync(surveyId);
            if(deletedCount == 0)
                NotFound($"No responses found for survey ID {surveyId} to delete.");
            return Ok($"number of  responses deleted: {deletedCount}");
        }
        catch (ArgumentException ex)
        {
            // Log the exception (not implemented here)
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }
}