using DTOs;
using Entities;
using SurveyBusinessLayer.Interface;
using SurveyDataAccessLayer.Interface;

namespace SurveyBusinessLayer;

public class ResponseService : IResponseService
{
    private readonly IResponseRepository _responseRepository;
    private readonly IQuestionRepository _questionRepository;
    public ResponseService(IResponseRepository responseRepository, IQuestionRepository questionRepository)
    {
        _responseRepository = responseRepository;
        _questionRepository = questionRepository;
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
    
    public async Task<ResponseDto> GetResponseByIdAsync(int responseId)
    {
        if (responseId <= 0)
            throw new ArgumentException("Response ID must be a positive integer.", nameof(responseId));
        
        var response = await _responseRepository.GetResponseByIdAsync(responseId);
        
        if(response == null)
            throw new KeyNotFoundException($"No response found with ID {responseId}.");
        
        return response;
    }
    
    public async Task<ResponseCreateDto> CreateResponseAsync(ResponseCreateDto responseCreateDto)
    {
        if (responseCreateDto == null)
            throw new ArgumentNullException(nameof(responseCreateDto), "Response data cannot be null.");

        var questions = _questionRepository.GetAllQuestionsAsync(responseCreateDto.SurveyId);
        var questionIds = questions.Result
            .Where(q => q.IsRequired)
            .Select(q => q.Id)
            .ToHashSet();
        
        var sentIds = responseCreateDto.Answers.Select(a => a.QuestionId).ToHashSet();
        
        if( sentIds.Count > questionIds.Count)  
            throw new ArgumentException("Too many answers provided. Please try again.");
        
        var missingRequired = questionIds.Except(sentIds).ToList();
        if (missingRequired.Any())
            throw new ArgumentException($"Missing answers for required questions: {string.Join(", ", missingRequired)}");
        
        return await _responseRepository.CreateResponseAsync(responseCreateDto);
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