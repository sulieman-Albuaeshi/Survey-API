using Entities;
namespace SurveyBusinessLayer.Interface;

public interface ISurveyService
{
    Task<List<Survey>> GetAllSurveysAsync();
    Task<Survey?> GetSurveyByIdAsync(int surveyId);
    Task<int> CreateSurveyAsync(Survey survey);
    Task<int>  UpdateSurveyAsync(Survey survey); 
    Task<bool> DeleteSurveyAsync(int surveyId);
}