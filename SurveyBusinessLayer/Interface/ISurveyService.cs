namespace SurveyBusinessLayer.Interface;

using Repository.Models;
using DTOs;

public interface ISurveyService
{
    Task<List<SurveyDto>> GetAllSurveysAsync(int pageSize, int pageNumber);
    Task<SurveyDetailsDto?> GetSurveyByIdAsync(int surveyId, string userAuthId, string? userRole);
    Task<string?> GetUserIdBySurveyIdAsync(int survceyId);
    Task<SurveyDetailsDto> CreateSurveyWithQuestionsAsync(CreateSurveyDto survey);
    Task<SurveyDetailsDto> UpdateSurveyWithQuestionsAsync(UpdaatSurveyDto survey);
    Task<bool> DeleteSurveyAsync(int surveyId);
    Task<bool> ChangeSurveyStatusAsync(int surveyId, string statusText);
    public Task<bool?> IsSurveyAnonymous(int surveyId);
}