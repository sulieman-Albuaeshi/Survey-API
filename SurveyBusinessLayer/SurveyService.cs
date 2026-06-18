using SurveyBusinessLayer.Interface;
using Repository.Interface;
using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer;

public class SurveyService : ISurveyService
{
    private readonly ISurveyRepository _surveyRepository;
    
    public SurveyService(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }
    
    public async Task<List<SurveyDto>> GetAllSurveysAsync()
    {
        var surveyList = await _surveyRepository.GetAllSurveysAsync();
        if (surveyList == null || !surveyList.Any())
            throw new KeyNotFoundException("Survey not found.");

        List<SurveyDto> dto = surveyList.Select(s => new SurveyDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            IsAnonymous = s.IsAnonymous,
            QuestionCount = s.QuestionCount,
            Status = s.Status.ToString(),
            CreatedAt = s.CreatedAt
        }).ToList();

        return dto;
    }
    
    public async Task<SurveyDto?> GetSurveyByIdAsync(int surveyId)
    {
        var survey = await _surveyRepository.GetSurveyByIdAsync(surveyId);
        if (survey == null)
            throw new KeyNotFoundException("Survey not found.");
        
        var surveyDto = new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            IsAnonymous = survey.IsAnonymous,
            QuestionCount = survey.QuestionCount,
            Status = survey.Status.ToString(),
            CreatedAt = survey.CreatedAt
        };

        return surveyDto;
    }
    
    public async Task<SurveyDto> CreateSurveyAsync(CreateSurveyDto surveyDto)
    {

        Survey survey = new Survey
        {
            Id = surveyDto.Id,
            Title = surveyDto.Title,
            Description = surveyDto.Description,
            IsAnonymous = surveyDto.IsAnonymous,
            Status = SurveyStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            QuestionCount = 0,
            UserId = surveyDto.userId
        };

        ValidateSurveyArgument(surveyDto);
        
        var createdSurvey = await _surveyRepository.CreateSurveyAsync(survey);

        if (createdSurvey == null)
            throw new KeyNotFoundException("Survey was not created.");

        var createdSurveyDto = new SurveyDto
        {
            Id = createdSurvey.Id,
            Title = createdSurvey.Title,
            Description = createdSurvey.Description,
            IsAnonymous = createdSurvey.IsAnonymous,
            QuestionCount = createdSurvey.QuestionCount,
            Status = createdSurvey.Status.ToString(),
            CreatedAt = createdSurvey.CreatedAt
        };

        return createdSurveyDto;
    }

    public async Task<SurveyDto> UpdateSurveyAsync(CreateSurveyDto surveyDto)
    {
        ValidateSurveyArgument(surveyDto);

        Survey survey = new Survey
        {
            Id = surveyDto.Id,
            Title = surveyDto.Title,
            Description = surveyDto.Description,
            IsAnonymous = surveyDto.IsAnonymous,
            Status = SurveyStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            QuestionCount = 0,
            UserId = surveyDto.userId
        };

        if (survey.Status == SurveyStatus.Published)
            throw new InvalidOperationException("Cannot update a published survey.");
        
        if(survey.Id <= 0)
            throw new ArgumentException("Invalid survey ID.");
        

        var createdSurvey = await _surveyRepository.UpdateSurveyAsync(survey);

        if (createdSurvey == null)
            throw new KeyNotFoundException("Survey was not updated.");

        var createdSurveyDto = new SurveyDto
        {
            Id = createdSurvey.Id,
            Title = createdSurvey.Title,
            Description = createdSurvey.Description,
            IsAnonymous = createdSurvey.IsAnonymous,
            QuestionCount = createdSurvey.QuestionCount,
            Status = createdSurvey.Status.ToString(),
            CreatedAt = createdSurvey.CreatedAt
        };

        return createdSurveyDto;
    }
    
    public async Task<bool> DeleteSurveyAsync(int surveyId)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");

        return await _surveyRepository.DeleteSurveyAsync(surveyId) == 1;
    }

    // TODO what is this Method ? returning a tuple of List<Question> and List<Choice> for a given surveyId ???? where to put ?
    public async Task<(List<Question>, List<Choice>)> GetQuestionsForSurveyAsync(int surveyId)
    {
        return await _surveyRepository.GetQuestionsForSurveyAsync(surveyId);
    }
    
    private static void ValidateSurveyArgument(CreateSurveyDto survey)
    {
        if (survey.Title == null || survey.Title.Trim() == "")
            throw new ArgumentException("Survey title is required.");
    }
}