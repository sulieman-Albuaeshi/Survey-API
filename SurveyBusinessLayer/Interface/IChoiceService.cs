namespace SurveyBusinessLayer.Interface;
using Entities;

public interface IChoiceService
{
    public Task<List<Choice>> GetChoicesByQuestionIdAsync(int questionId);
    public Task<bool> CreateChoiceAsync(Choice choice); 
    public Task<bool> UpdateChoiceAsync(Choice choice);
    public Task<bool> DeleteChoiceAsync(int id);
}