using Entities;
using SurveyBusinessLayer.Interface;
using SurveyDataAccessLayer.Interface;

namespace SurveyBusinessLayer;

// TODO the survey need to populated with question 

public class SurveyService : ISurveyService
{
    private readonly ISurveyRepository _surveyRepository;
    
    public SurveyService(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }
    
    public async Task<List<Survey>> GetAllSurveysAsync()
    {
        var SurveyList = await _surveyRepository.GetAllSurveysAsync();
        if (SurveyList == null || !SurveyList.Any())
            throw new KeyNotFoundException("Survey not found.");
        
        return SurveyList;
    }
    
    public async Task<Survey?> GetSurveyByIdAsync(int surveyId)
    {
        var survey = await _surveyRepository.GetSurveyByIdAsync(surveyId);
        if (survey == null)
            throw new KeyNotFoundException("Survey not found.");
        
        return survey;
    }
    
    public async Task<int> CreateSurveyAsync(Survey survey)
    {
        ValidateSurveyArgument(survey);
        survey.Status = SurveyStatus.Draft;
        survey.CreatedDate = DateTime.UtcNow;
        survey.QuestionCount = 0;
        return await _surveyRepository.CreateSurveyAsync(survey);
    }
    
    public async Task<int> UpdateSurveyAsync(Survey survey)
    {
        ValidateSurveyArgument(survey);
        
        if(survey.Status == SurveyStatus.Published)
            throw new InvalidOperationException("Cannot update a published survey.");
        
        if(survey.Id < 0)
            throw new ArgumentException("Invalid survey ID.");
        
        var exsitingSurvey = await GetSurveyByIdAsync(survey.Id);
        
        if(exsitingSurvey == null)
            throw new KeyNotFoundException("Survey not found.");
        
        // TODO if the user authorized
        if(survey.UserId !=  exsitingSurvey?.UserId)
            throw new ArgumentException("User IDs do not match.");
        //throw new UnauthorizedAccessException("You are not authorized to update this survey.");
        
        return await _surveyRepository.UpdateSurveyAsync(survey);
    }
    
    public async Task<bool> DeleteSurveyAsync(int surveyId)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");

        return await _surveyRepository.DeleteSurveyAsync(surveyId) == 1;
    }

    private static void ValidateSurveyArgument(Survey survey)
    {
        if (survey.Title == null || survey.Title.Trim() == "")
            throw new ArgumentException("Survey title is required.");
        if (survey.UserId == null || survey.UserId.Trim() == "")
            throw new ArgumentException("User ID is required.");
    }
}