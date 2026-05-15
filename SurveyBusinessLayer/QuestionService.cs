using Entities;

namespace SurveyBusinessLayer;

using SurveyDataAccessLayer.Interface;
using SurveyBusinessLayer.Interface;
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ISurveyRepository _surveyRepository;
    
    public QuestionService(IQuestionRepository questionRepository,  ISurveyRepository surveyRepository)
    {
        _questionRepository = questionRepository;
        _surveyRepository = surveyRepository;
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
        if(id < 1)
            throw new ArgumentException("Invalid question id");
        
        var question = await _questionRepository.GetQuestionByIdAsync(id);
        if(question == null)
            throw new KeyNotFoundException("No question found");
        
        return question;
    }

    public async Task<int> CreateQuestionAsync(int surveyid, Question que,  IEnumerable<Choice> choice)
    {   
        if(surveyid < 1)
            throw new ArgumentException("Invalid surveyid");
        
        if(que == null || string.IsNullOrEmpty(que.QuestionText) )
            throw new ArgumentException("No question found");
        
        if(choice == null || !choice.Any())
            throw new ArgumentException("No choice found");
        
        if(choice.Any(ch => string.IsNullOrWhiteSpace(ch.ChoiceText)))
            throw new ArgumentException("No choice found");
        
        var survey = await _surveyRepository.GetSurveyByIdAsync(surveyid);
        if (survey == null)
            throw new KeyNotFoundException("Survey not found.");

        if (survey.Status == SurveyStatus.Published)
            throw new InvalidOperationException("Cannot add questions to a published survey.");

        que.SurveyId = surveyid;

        que.Id = await _questionRepository.CreateQuestionAsync(surveyid, que, choice);
        return que.Id;
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