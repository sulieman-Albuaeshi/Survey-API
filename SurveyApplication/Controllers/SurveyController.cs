using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using SurveyBusinessLayer;

namespace SurveyApplication.Controllers;

[ApiController]
[Route("api/surveys")]
public class SurveyController : ControllerBase
{
    [HttpGet("All", Name = "GetAllSurveys")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<SurveyDto>> GetAllSurveys()
    {
        var surveys = SurveyBL.GetAllSurveys();
        if (surveys == null)
            return NotFound("No surveys found");

        var surveyDtos = surveys.Select(survey => new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            CreatedDate = survey.CreatedDate,
            // You can easily convert Enums to strings for the frontend here!
            Status = survey.Status.ToString()
        }).ToList();
    return Ok(surveyDtos);
    }
}