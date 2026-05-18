using Entities;
using SurveyDataAccessLayer.rowDTO;

namespace SurveyDataAccessLayer.Interface;

public interface IResponseRepository
{
     public Task<List<SurveyResponseRow>> GetAllResponsesDetailsAsync(); 
     public Task<List<SurveyResponseRow>> GetResponsesBySurveyIdAsync(int surveyId);
     public Task<List<SurveyResponseRow>> GetResponsesByUserIdAsync(string userId);
     public Task<int> CreateResponseAsync(Responses response);
     public Task<int> DeleteResponseAsync(int responseId);
     
} 