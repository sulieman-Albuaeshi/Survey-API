using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.Interface;
using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/surveys/{surveyId}/questions")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IChoiceService _choiceService;

    public QuestionController(IQuestionService questionService,  IChoiceService choiceService)
    {
        _questionService = questionService;
        _choiceService = choiceService;
    }

    [HttpGet("All", Name = "GetAllQuestionsForSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuestionDetailsDto>>> GetAllQuestionsForSurvey(int surveyId)
    {
        try
        {
            var questionDtos = await _questionService.GetAllQuestionsAsync(surveyId);
            
            return Ok(questionDtos);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    // The "/" at the beginning ignores the class route completely!
    [HttpGet("/api/questions/{id}", Name = "GetQuestionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionDetailsDto>> GetQuestionById(int id)
    {
        try
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null)
                return NotFound("Question not found");
            return Ok(question);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
    
    [HttpDelete("/api/questions/{id}", Name = "DeleteQuestion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
}