namespace SurveyDataAccessLayer.Interface;

using Entities;
public interface IQuestionRepository
{
    Task<List<Question>> GetAllQuestionsAsync();
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<int> CreateQuestionAsync(Question que);
    Task<int>  UpdateQuestionAsync(Question que); 
    Task<bool> DeleteQuestionAsync(int id);
}