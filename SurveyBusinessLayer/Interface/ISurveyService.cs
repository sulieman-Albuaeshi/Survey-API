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
    Task<bool> ChangeSurveyStatusAsync(int surveyId, string statusText);

}