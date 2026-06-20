using Repository.Models;
using SurveyBusinessLayer.DTOs;
namespace SurveyBusinessLayer.Interface;

public interface IQuestionService
{
    Task<List<QuestionDetailsDto>> GetAllQuestionsAsync(int surveyId);
    Task<QuestionDetailsDto?> GetQuestionByIdAsync(int id);
}