namespace Repository.Interface;

using Repository.Models;

public interface ISurveyRepository
{
    Task<List<Survey>> GetAllSurveysAsync(int skipage, int takepage);
    Task<Survey?> GetSurveyByIdAsync(int surveyId);
    Task<string?> GetUserIdBySurveyIdAsync(int surveyId);
    Task<Survey> CreateSurveyAsync(Survey survey);
    Task<Survey> UpdateSurveyAsync(Survey survey);
    Task<int> DeleteSurveyAsync(int surveyId);
    Task<bool> ChangeSurveyStatusAsync(int surveyId, string statusText);
    public Task<bool?> IsSurveyAnonymousAsync(int surveyId);
}