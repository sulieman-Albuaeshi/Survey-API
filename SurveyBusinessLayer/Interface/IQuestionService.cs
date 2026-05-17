namespace SurveyBusinessLayer.Interface;
using Entities; 

public interface IQuestionService
{
    Task<List<Question>> GetAllQuestionsAsync(int surveyId);
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<int> CreateQuestionAsync(int surveyid, Question que, IEnumerable<Choice> choice);
    Task<int>  UpdateQuestionAsync(Question que); 
    Task<bool> DeleteQuestionAsync(int id);
}