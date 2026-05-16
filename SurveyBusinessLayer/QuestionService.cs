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

    public async Task<Question?> GetQuestionByIdAsync(int id, int surveyId)
    {
        if (id < 1 || surveyId < 1)
            throw new ArgumentException("Invalid question id or survey id");
        
        var question = await _questionRepository.GetQuestionByIdAsync(id, surveyId);
        if(question == null)
            throw new KeyNotFoundException("No question found");
        
        return question;
    }

    public async Task<int> CreateQuestionAsync(int surveyid, Question que,  IEnumerable<Choice> choice)
    {
        ValidateSurveyId(surveyid);
        ValidateQuestion(que);
        
        // TODO : HOW to habdel Matrix`
        if(que.QuestionType == QuestionType.Radio || que.QuestionType == QuestionType.Checkbox || que.QuestionType == QuestionType.Matrix)
        {
            if(choice == null || !choice.Any())
                throw new ArgumentException("No choice found");
        }
        if(choice != null && choice.Any() && (que.QuestionType == QuestionType.Rating || que.QuestionType == QuestionType.Text))
            throw new ArgumentException("this type of question does not support choices");
        
        if(choice != null && choice.Any(ch => string.IsNullOrWhiteSpace(ch.ChoiceText)))
            throw new ArgumentException("No choice found");

        var survey = await ValidateSurveyExists(surveyid);
        ValidateSurveyNotPublished(survey);

        que.SurveyId = surveyid;

        que.Id = await _questionRepository.CreateQuestionAsync(surveyid, que, choice);
        return que.Id;
    }

    public async Task<int> UpdateQuestionAsync(Question que)
    {
        ValidateSurveyId(que.SurveyId);
        ValidateQuestion(que);
        
        var existingQuestion = await _questionRepository.GetQuestionByIdAsync(que.Id, que.SurveyId);
        if(existingQuestion == null)
            throw new KeyNotFoundException("No question found");
        
        var survey = await ValidateSurveyExists(que.SurveyId);
        ValidateSurveyNotPublished(survey);
        
        return await _questionRepository.UpdateQuestionAsync(que);
    }

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        if(id < 1)
            throw new ArgumentException("Invalid question id or survey id");
        
        return await _questionRepository.DeleteQuestionAsync(id);
    }
    

    private void ValidateSurveyId(int surveyId)
    {
        if (surveyId < 1)
            throw new ArgumentException("Invalid survey id");
    }

    private void ValidateQuestion(Question question)
    {
        if (question == null || string.IsNullOrEmpty(question.QuestionText))
            throw new ArgumentException("No question found");
    }
    private async Task<Survey> ValidateSurveyExists(int surveyId)
    {
        var survey = await _surveyRepository.GetSurveyByIdAsync(surveyId);
        if (survey == null)
            throw new KeyNotFoundException("Survey not found.");
        return survey;
    }

    private void ValidateSurveyNotPublished(Survey survey)
    {
        if (survey.Status == SurveyStatus.Published)
            throw new InvalidOperationException("Cannot modify questions in a published survey.");
    }
}