using Repository.Interface;
using SurveyBusinessLayer.Interface;
using SurveyBusinessLayer.DTOs;
using Repository.Models;

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
        if(surveyId < 1)
            throw new ArgumentException("Invalid survey id");

        var questions = await _questionRepository.GetAllQuestions(surveyId);


        var questionDtos = questions.Select(q => new QuestionDetailsDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            IsRequired = q.IsRequired,
            OrderIndex = q.OrderIndex,
            QuestionTypeName = q.QuestionTypeEnum.ToString(),
            Choices = q.Choices.Select(c => new ChoiceDto
            {
                Id = c.Id,
                ChoiceText = c.ChoiceText,
                OrderIndex = c.OrderIndex
            }).ToList(),
        }).ToList();

        if(questionDtos == null)
            throw new KeyNotFoundException("No questions found");

        return questionDtos;
    }
    public async Task<QuestionDetailsDto?> GetQuestionByIdAsync(int id)
    {
        if (id < 1)
            throw new ArgumentException("Invalid question id or survey id");
        
        var question = await _questionRepository.GetQuestionByIdAsync(id);

        if(question == null)
            throw new KeyNotFoundException("No question found");

        var questionDto = new QuestionDetailsDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            IsRequired = question.IsRequired,
            OrderIndex = question.OrderIndex,
            QuestionTypeName = question.QuestionTypeEnum.ToString(),
            Choices = question.Choices.Select(c => new ChoiceDto
            {
                Id = c.Id,
                ChoiceText = c.ChoiceText,
                OrderIndex = c.OrderIndex
            }).ToList()
        };  

        return questionDto;
    }
}