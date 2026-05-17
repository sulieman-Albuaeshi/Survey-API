namespace SurveyDataAccessLayer.Interface;
using Entities;

public interface ISurveyRepository
{
    Task<List<Survey>> GetAllSurveysAsync();
    Task<Survey?> GetSurveyByIdAsync(int surveyId);
    Task<int> CreateSurveyAsync(Survey survey);
    Task<int> UpdateSurveyAsync(Survey survey);
    Task<int> DeleteSurveyAsync(int surveyId);
    Task<(List<Question>, List<Choice>)> GetQuestionsForSurveyAsync(int surveyId);
}