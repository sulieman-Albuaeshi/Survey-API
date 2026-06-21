using Repository.Models;
namespace Repository.Interface;

public interface IResponseRepository
{
     public Task<List<Response>> GetAllResponsesDetailsAsync(); 
     public Task<List<Response>> GetResponsesBySurveyIdAsync(int surveyId);
     public Task<List<Response>> GetResponsesByUserIdAsync(string userId);
     public Task<Response> GetResponseByIdAsync(int responseId);
     public Task<int> DeleteResponseAsync(int responseId);
     public Task<Response> SubmitResponseAsync(Response response);
} 