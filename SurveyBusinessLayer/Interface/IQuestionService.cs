namespace SurveyBusinessLayer.Interface;
using Entities; 

public interface IQuestionService
{
    Task<List<Question>> GetAllQuestionsAsync();
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<int> CreateQuestionAsync(Question que);
    Task<int>  UpdateQuestionAsync(Question que); 
    Task<bool> DeleteQuestionAsync(int id);
}