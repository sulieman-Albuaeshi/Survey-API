using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;


namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/Response")]
public class ResponseController : ControllerBase
{
    readonly IResponseService _responseService;
    readonly IValidator<ResponseCreateDto> _createResponseValidator;

    public ResponseController(IResponseService responseService, IValidator<ResponseCreateDto> createResponseValidator)
    {
        _responseService = responseService;
        _createResponseValidator = createResponseValidator;
    }
    
    [HttpGet("All",  Name = "GetAllResponses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public  async Task<ActionResult<IEnumerable<ResponseDto>>> GetResponsesBySurveyId(int surveyId, int pageSize, int pageNumber)
    {
        if (pageSize <= 0 || pageNumber <= 0) return BadRequest("Page size and page number must be greater than zero");
        if (surveyId <= 0) return BadRequest("Invalid Survey ID");

        var responses = await _responseService.GetResponsesBySurveyIdAsync(surveyId, pageSize, pageNumber);
            
        if(responses == null || !responses.Any())
            return NotFound($"No responses found for survey ID {surveyId}.");
            
        return Ok(responses);

    }
    
    [HttpGet("user/{userId}", Name = "GetResponsesByUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ResponseDto>>> GetResponsesByUserId(string userId, int pageSize, int pageNumber)
    {
        if (pageSize <= 0 || pageNumber <= 0) return BadRequest("Page size and page number must be greater than zero");

        var responses = await _responseService.GetResponsesByUserIdAsync(userId, pageSize, pageNumber);
            
        if(responses == null || !responses.Any())
            return NotFound($"No responses found for user ID {userId}.");
            
        return Ok(responses);
       
    }
    
    [HttpGet("{responseId}", Name = "GetResponseById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]    
    public async Task<ActionResult<ResponseDto>> GetResponseById(int responseId)
    {
        if (responseId <= 0) return BadRequest("Invalid Response ID");

        var response = await _responseService.GetResponseByIdAsync(responseId);
            
        if(response == null)
            return NotFound($"No response found with ID {responseId}.");
            
        return Ok(response);
    }

    
    [HttpPost("survey/{surveyId}", Name = "CreateResponse")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SubmitResponse([FromBody] ResponseCreateDto responseCreateDto)
    { 
        var validationResult = _createResponseValidator.Validate(responseCreateDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var createdResponse = await _responseService.SubmitResponseAsync(responseCreateDto);
        return CreatedAtRoute("GetResponseById", new { responseId = createdResponse.ResponseId}, createdResponse);
    }

    [HttpGet("survey/{surveyId}/Count", Name = "GetResponsesCount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResponseCountBySurveyId(int surveyId)
    {
        if (surveyId <= 0) return BadRequest("Invalid Survey ID");

        var count = await _responseService.GetResponsesCountAsync(surveyId);
        return Ok(new { SurveyId = surveyId, ResponseCount = count });
    }

}