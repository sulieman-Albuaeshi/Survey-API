using DTOs;
using Entities;

namespace SurveyBusinessLayer.Interface;

public interface IAnswerService 
{
    public Task<List<AnswerQuestionDto>> GetAllAnswersBySurveyAsync(int surveyId, int  userId); 
}