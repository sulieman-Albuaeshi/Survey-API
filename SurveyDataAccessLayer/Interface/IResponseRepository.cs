using Entities;
using SurveyDataAccessLayer.rowDTO;

namespace SurveyDataAccessLayer.Interface;

public interface IResponseRepository
{
     public Task<List<SurveyResponseRow>> GetAllResponsesDetailsAsync(); 
     public Task<Responses?> GetResponseByIdAsync(int responseId);  
     public Task<List<Responses>> GetResponsesBySurveyIdAsync(int surveyId);
     public Task<List<Responses>> GetResponsesByUserIdAsync(string userId);
     public Task<int> CreateResponseAsync(Responses response);
     public Task<int> DeleteResponseAsync(int responseId);
     
} 