using Entities;

namespace SurveyBusinessLayer;

using SurveyDataAccessLayer.Interface;
using SurveyBusinessLayer.Interface;
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    
    public QuestionService(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }
    public async Task<List<Question>> GetAllQuestionsAsync(int  surveyId)
    {
        if(surveyId < 1)
            throw new ArgumentException("Invalid survey id");
        
        var questions = await _questionRepository.GetAllQuestionsAsync(surveyId);
        
        if(questions == null)
            throw new KeyNotFoundException("No questions found");
        
        return questions;
    }

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CreateQuestionAsync(Question que)
    {
        throw new NotImplementedException();
    }

    public async Task<int> UpdateQuestionAsync(Question que)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        throw new NotImplementedException();
    }
}