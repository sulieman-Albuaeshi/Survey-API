using Entities;
using SurveyDataAccessLayer.rowDTO;

namespace SurveyBusinessLayer.Interface;

public interface IResponseService
{
    public Task<List<SurveyResponseRow>> GetAllResponsesDetailsAsync();
    
    public Task<List<SurveyResponseRow>> GetResponsesBySurveyIdAsync(int surveyId);
    
    public Task<List<SurveyResponseRow>> GetResponsesByUserIdAsync(string userId);
    
    public Task<int> GetResponsesCountAsync();
    public Task<int> DeleteResponsesAsync(int surveyId);
}   