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
    public QuestionController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet("All", Name = "GetAllQuestionsForSurvey")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QuestionDetailsDto>>> GetAllQuestionsForSurvey(int surveyId)
    {
        if(surveyId <= 0 ) return BadRequest("Invalid survey ID");
        
        var questionDtos = await _questionService.GetAllQuestionsAsync(surveyId);
            
        return Ok(questionDtos);
   
    }

    // The "/" at the beginning ignores the class route completely!
    [HttpGet("/api/questions/{id}", Name = "GetQuestionById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuestionDetailsDto>> GetQuestionById(int id)
    {
        if(id <= 0 ) return BadRequest("Invalid question ID");

        var question = await _questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound("Question not found");
        return Ok(question);

    }
}