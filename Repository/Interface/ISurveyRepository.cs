namespace Repository.Interface;

using Repository.Models;

public interface ISurveyRepository
{
    Task<List<Survey>> GetAllSurveysAsync();
    Task<Survey?> GetSurveyByIdAsync(int surveyId);
    Task<Survey> CreateSurveyAsync(Survey survey);
    Task<Survey> UpdateSurveyAsync(Survey survey);
    Task<int> DeleteSurveyAsync(int surveyId);
    Task<bool> ChangeSurveyStatusAsync(int surveyId, string statusText);
}