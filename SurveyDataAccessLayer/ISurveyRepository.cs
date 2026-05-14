namespace SurveyDataAccessLayer;
using Entities;

public interface ISurveyRepository
{
    Task<List<Survey>> GetAllSurveysAsync();
    Task<Survey?> GetSurveyByIdAsync(int surveyId);
    Task<int> CreateSurveyAsync(Survey survey);
    Task<int> UpdateSurveyAsync(Survey survey);
    Task<int> DeleteSurveyAsync(int surveyId);
}