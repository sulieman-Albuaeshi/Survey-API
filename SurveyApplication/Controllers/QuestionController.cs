using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.Interface;
using DTOs;
using Entities;
using SurveyApplication.Mapper;

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
            var questions = await _questionService.GetAllQuestionsAsync(surveyId);
            var questionDtos = questions.Select(QuestionMapper.ToQuestionDetailsDto).ToList();
            foreach (var queDto in questionDtos)
            {
                if (queDto.QuestionType is QuestionType.Radio or QuestionType.Checkbox or QuestionType.Matrix or QuestionType.Rank)
                {
                    var choices = await _choiceService.GetChoicesByQuestionIdAsync(queDto.Id);
                    queDto.Choices = ChoiceMapper.ToChoiceDto(choices);
                }
            }
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
    public async Task<ActionResult<QuestionDetailsDto>> GetQuestionById(int id, int surveyId)
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

    [HttpPost("", Name = "CreateQuestion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> CreateQuestion(int surveyId, [FromBody] QuestionDetailsDto questionDetailsDto)
    {
        try
        {
            var question = QuestionMapper.ToQuestionEntity(questionDetailsDto);
            question.SurveyId = surveyId;
            var choices = ChoiceMapper.ToChoice(questionDetailsDto);


            var createdQuestionId = await _questionService.CreateQuestionAsync(surveyId, question, choices);
            return CreatedAtRoute("GetQuestionById", new { id = createdQuestionId, surveyId }, createdQuestionId);
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

    // The "/" at the beginning ignores the class route completely!
    [HttpPut("/api/questions", Name = "UpdateQuestion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateQuestion([FromBody] QuestionDto questionDto)
    {
        try
        {
            var question = QuestionMapper.ToQuestionEntity(questionDto);
            
            var updatedQuestionId = await _questionService.UpdateQuestionAsync(question);
            return Ok(updatedQuestionId);
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

    
    [HttpDelete("/api/questions/{id}", Name = "DeleteQuestion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteQuestion(int id)
    {
        try
        {
            var deletedQuestionId = await _questionService.DeleteQuestionAsync(id);
            return Ok(deletedQuestionId);
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