using Repository.Interface;
using SurveyBusinessLayer.Interface;
using SurveyBusinessLayer.DTOs;
using Repository.Models;
using SurveyBusinessLayer.Mapper;

namespace SurveyBusinessLayer;
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    
    public QuestionService(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }
    public async Task<List<QuestionDetailsDto>> GetAllQuestionsAsync(int  surveyId)
    {
        var questions = await _questionRepository.GetAllQuestions(surveyId);

        return questions.Select(q => q.ToDetailDto()).ToList()
;
    }
    public async Task<QuestionDetailsDto?> GetQuestionByIdAsync(int id)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id);

        if(question == null)
            throw new KeyNotFoundException("No question found");

        return question.ToDetailDto();
    }
}