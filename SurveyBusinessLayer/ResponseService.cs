using DTOs;
using Entities;
using SurveyBusinessLayer.Interface;
using SurveyDataAccessLayer.Interface;

namespace SurveyBusinessLayer;

public class ResponseService : IResponseService
{
    private readonly IResponseRepository _responseRepository;
    public ResponseService(IResponseRepository responseRepository)
    {
        _responseRepository = responseRepository;
    }
    
    public Task<List<ResponseDto>> GetAllResponsesDetailsAsync()
    {
        return _responseRepository.GetAllResponsesDetailsAsync();
    }

    public async Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId)
    {
        if (surveyId == 0)
            throw new ArgumentException("Survey ID must be a non-zero value.", nameof(surveyId));
        
        var responses = await _responseRepository.GetResponsesBySurveyIdAsync(surveyId);
        
        if(responses.Count == 0)
            throw new KeyNotFoundException($"No responses found for survey ID {surveyId}.");
        
        return responses;
    }
    
    public async Task<List<ResponseDto>> GetResponsesByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID must be a non-empty value.", nameof(userId));
        
        int userid = int.Parse(userId);
        if(userid <= 0)
            throw new ArgumentException("user ID not found", nameof(userId));
        
        var responses = await _responseRepository.GetResponsesByUserIdAsync(userId);
        
        if(responses.Count == 0)
            throw new KeyNotFoundException($"No responses found for user ID {userId}.");
        
        return responses;
    }

    public async Task<int> GetResponsesCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<int> DeleteResponsesAsync(int surveyId)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Survey ID must be a positive integer.", nameof(surveyId));
        
        return await _responseRepository.DeleteResponseAsync(surveyId);
    }
}