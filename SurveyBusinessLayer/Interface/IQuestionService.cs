using Repository.Models;
using SurveyBusinessLayer.DTOs;
namespace SurveyBusinessLayer.Interface;

public interface IQuestionService
{
    Task<List<QuestionDto>> GetAllQuestionsAsync(int surveyId);
    Task<QuestionDto?> GetQuestionByIdAsync(int id);

    Task<SurveyDetailsDto> SaveQuestionAsync(SurveyDetailsDto questionDtos);
    Task<QuestionDto> CreateQuestionAsync(Question que);
    Task<bool> DeleteQuestionAsync(int id);
}