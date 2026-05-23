using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.Interface;
using DTOs;

namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/survey/{surveyId}/answers")]
public class AnswerController : ControllerBase
{
    private readonly IAnswerService _answerService;
    public AnswerController(IAnswerService answerService)
    {
        _answerService = answerService;
    }
    
    [HttpGet("", Name = "GetAnswersBySurveyId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AnswerQuestionDto>>> GetAnswersBySurveyIdAsync(int surveyId)
    {
        try
        {
            var answers = await _answerService.GetAllAnswersBySurveyAsync(surveyId, 1); // TODO : For now let assume the authorized userId is 1
            return Ok(answers);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}