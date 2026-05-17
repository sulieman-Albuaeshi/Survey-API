using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer.Interface;
using Entities;
using DTOs;
using SurveyApplication.Mapper;

namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/questions/{questionId}/choices")]
public class ChoiceController : ControllerBase
{
    private readonly IChoiceService _choiceService;
    public ChoiceController(IChoiceService choiceService)
    {
        _choiceService = choiceService;
    }
    
    
    [HttpGet("", Name = "GetChoicesByQuestionId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Choice>>> GetChoicesByQuestionIdAsync(int questionId)
    {
        try
        {
            var choices = await _choiceService.GetChoicesByQuestionIdAsync(questionId);
            var choiceDto = choices.Select(c => new DTOs.ChoiceDto
            {
                Id = c.Id,
                ChoiceText = c.ChoiceText,
                OrderIndex = c.OrderIndex,
            }).ToList();
            return Ok(choiceDto);
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

    [HttpPost("", Name = "AddChoice")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateChoiceAsync(int questionId, [FromBody] CreateChoiceDto choiceDto)
    {
        try
        {
            var choice = ChoiceMapper.ToEntity(choiceDto);
            choice.QuestionId = questionId;
            var result = await _choiceService.CreateChoiceAsync(choice);
            return CreatedAtRoute("GetChoicesByQuestionId", new { questionId = questionId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)       
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)        {
            return StatusCode(500, ex.Message);
        }
    }   
    
    
    [HttpPut("", Name = "UpdateChoice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateChoiceAsync(int questionId, [FromBody] ChoiceDto choiceDto)
    {
        try
        {
            var choice = ChoiceMapper.ToEntity(choiceDto);
            choice.QuestionId = questionId;
        
            var result = await _choiceService.UpdateChoiceAsync(choice);
            return Ok(result);

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

    [HttpDelete("/api/choice/{id}", Name = "DeleteChoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteChoiceAsync(int id)
    {
        try
        {
            var result = await _choiceService.DeleteChoiceAsync(id);
            if (result)
                return Ok(result);
            return NotFound("Choice not found for deletion");
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