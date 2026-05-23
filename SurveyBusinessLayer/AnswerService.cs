
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
        var result  = await _answerRepository.GetAllAnswersBySurveyAsync(surveyId, userId);
        if (result == null || result.Count == 0) throw new KeyNotFoundException("No answers found for the given survey ID and user ID");
        return result;
    }

    public async Task<bool> CreateAnswerAsync(Answer choice)
    {
        throw new NotImplementedException();
    }
}