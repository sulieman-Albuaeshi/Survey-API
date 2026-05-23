using DTOs;
using Entities;

namespace SurveyDataAccessLayer.Interface;

public interface IAnswerRepository
{
    public  Task<List<AnswerQuestionDto>> GetAllAnswersBySurveyAsync(int surveyId, int  userId);
    public  Task<bool> CreateAnswerAsync(Answer choice); 
}