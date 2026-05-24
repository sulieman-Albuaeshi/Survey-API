using DTOs;

namespace SurveyBusinessLayer.Interface;

public interface IResponseService
{
    public Task<List<ResponseDto>> GetAllResponsesDetailsAsync();
    
    public Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId);
    
    public Task<List<ResponseDto>> GetResponsesByUserIdAsync(string userId);
    
    public Task<int> GetResponsesCountAsync();
    public Task<int> DeleteResponsesAsync(int surveyId);
}   