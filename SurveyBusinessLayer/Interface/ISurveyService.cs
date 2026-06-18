namespace SurveyBusinessLayer.Interface;

using Repository.Models;
using DTOs;

public interface ISurveyService
{
    Task<List<SurveyDto>> GetAllSurveysAsync();
    Task<SurveyDto?> GetSurveyByIdAsync(int surveyId);
    Task<SurveyDto> CreateSurveyAsync(CreateSurveyDto survey);
    Task<SurveyDto>  UpdateSurveyAsync(CreateSurveyDto survey); 
    Task<bool> DeleteSurveyAsync(int surveyId);
    // TODO what is this Method ? returning a tuple of List<Question> and List<Choice> for a given surveyId ????
    Task<(List<Question>, List<Choice>)> GetQuestionsForSurveyAsync(int surveyId);
}