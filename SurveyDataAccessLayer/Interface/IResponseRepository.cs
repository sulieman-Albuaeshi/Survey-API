using Entities;
using DTOs;
namespace SurveyDataAccessLayer.Interface;

public interface IResponseRepository
{
     public Task<List<ResponseDto>> GetAllResponsesDetailsAsync(); 
     public Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId);
     public Task<List<ResponseDto>> GetResponsesByUserIdAsync(string userId);
     public Task<int> CreateResponseAsync(Responses response);
     public Task<int> DeleteResponseAsync(int responseId);
     
} 