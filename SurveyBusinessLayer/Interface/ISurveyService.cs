namespace SurveyBusinessLayer.Interface;

using Repository.Models;
using DTOs;

public interface ISurveyService
{
    Task<List<SurveyDto>> GetAllSurveysAsync();
    Task<SurveyDetailsDto?> GetSurveyByIdAsync(int surveyId);
    Task<SurveyDetailsDto> CreateSurveyWithQuestionsAsync(CreateSurveyDto survey);
    Task<SurveyDetailsDto> UpdateSurveyWithQuestionsAsync(UpdaatSurveyDto survey);
    Task<bool> DeleteSurveyAsync(int surveyId);
    // TODO what is this Method ? returning a tuple of List<Question> and List<Choice> for a given surveyId ????
    Task<(List<Question>, List<Choice>)> GetQuestionsForSurveyAsync(int surveyId);
}