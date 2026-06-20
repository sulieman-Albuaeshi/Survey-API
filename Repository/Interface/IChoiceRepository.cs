using Repository.Models;

namespace Repository.Interface;

public interface IChoiceRepository
{
    public  Task<List<Choice>> GetChoicesByQuestionIdAsync(int questionId);
    public Task<bool> CreateChoiceAsync(Choice choice);
    public  Task<bool> UpdateChoiceAsync(Choice choice);
    public  Task<bool> DeleteChoiceAsync(int id);
}