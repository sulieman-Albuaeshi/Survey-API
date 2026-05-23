
using DTOs;
using Entities;
using SurveyBusinessLayer.Interface;
using SurveyDataAccessLayer.Interface;

namespace SurveyBusinessLayer;

public class AnswerService : IAnswerService
{
    private readonly IAnswerRepository _answerRepository;
    public AnswerService(IAnswerRepository answerRepository)
    {
        _answerRepository = answerRepository;
    }

    public async Task<List<AnswerQuestionDto>> GetAllAnswersBySurveyAsync(int surveyId, int userId)
    {
            
        if(surveyId < 0)  throw new ArgumentException("Survey ID cannot be negative");
        
        // TODO : userId should not be 1
        return await _answerRepository.GetAllAnswersBySurveyAsync(surveyId, userId);
    }

    public async Task<bool> CreateAnswerAsync(Answer choice)
    {
        throw new NotImplementedException();
    }
}