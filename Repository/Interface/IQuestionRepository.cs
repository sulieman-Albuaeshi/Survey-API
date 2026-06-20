using Repository.Models;

namespace Repository.Interface;
public interface IQuestionRepository
{
    Task<List<Question>> GetAllQuestions(int  surveyId);
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<Survey> SaveQuestionWithSurveyAsync(Survey survey);
}