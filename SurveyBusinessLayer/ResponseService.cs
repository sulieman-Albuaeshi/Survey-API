using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Models;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;
using SurveyBusinessLayer.Mapper;

namespace SurveyBusinessLayer;

public class ResponseService : IResponseService
{
    private readonly IResponseRepository _responseRepository;
    public ResponseService(IResponseRepository responseRepository)
    {
        _responseRepository = responseRepository;
    }
    
    public async Task<List<ResponseDto>> GetAllResponsesDetailsAsync(int pageSize, int pageNumber)
    {
        if (pageSize <= 0 || pageNumber <= 0)
            throw new ArgumentException("Page size and page number must be greater than zero.");

        var response = await _responseRepository.GetAllResponsesDetailsAsync(pageSize, pageNumber);
        
        var responseDtos = response.Select(r => r.ToDto()).ToList();

        return responseDtos;
    }

    public async Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId, int pageSize, int pageNumber)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Survey ID must be a positive integer.", nameof(surveyId));
        
        var responses = await _responseRepository.GetResponsesBySurveyIdAsync(surveyId, pageSize, pageNumber);
        
        if(responses.Count == 0)
            throw new KeyNotFoundException($"No responses found for survey ID {surveyId}.");

        var responseDtos = responses.Select(r => r.ToDto()).ToList();

        return responseDtos;
    }
    
    public async Task<List<ResponseDto>> GetResponsesByUserIdAsync(string userId, int pageSize, int pageNumber)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID must be a non-empty value.", nameof(userId));
               
        var responses = await _responseRepository.GetResponsesByUserIdAsync(userId, pageSize, pageNumber);
        
        if(responses.Count == 0)
            throw new KeyNotFoundException($"No responses found for user ID {userId}.");

        var responseDtos = responses.Select(r => r.ToDto()).ToList();

        return responseDtos;

    }

    public async Task<ResponseDto> GetResponseByIdAsync(int responseId)
    {
        if (responseId <= 0)
            throw new ArgumentException("Response ID must be a positive integer.", nameof(responseId));
        
        var response = await _responseRepository.GetResponseByIdAsync(responseId);
        
        if(response == null)
            throw new KeyNotFoundException($"No response found with ID {responseId}.");

        return response.ToDto();
    }
    
    public async Task<ResponseDetailsDto> SubmitResponseAsync(ResponseCreateDto dto)
    {
        // survey validation
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Response data cannot be null.");

        if(dto.Answers == null || !dto.Answers.Any())
            throw new InvalidOperationException("required questions must be answered");

        var validationData = await _responseRepository.GetValidationDataForSurveyAsync(dto.SurveyId);

        if (validationData?.IsAnonymous == null)
            throw new KeyNotFoundException($"Survey with ID {dto.SurveyId} not found.");

        if ( validationData.IsAnonymous  == false && string.IsNullOrEmpty(dto.UserId))
            throw new InvalidOperationException("Survey is not anonymous, userId must be provided.");

        var missingRequiredIds = validationData.RequiredQuestionIds
            .Where(id => !dto.Answers.Select(a => a.QuestionId)
            .Contains(id))
            .Count();

        if (missingRequiredIds != 0)
            throw new InvalidOperationException("All required questions must be answered.");

        var submittedChoiceIds = dto.Answers
            .SelectMany(a => a.RankedChoices?.Select(s => s.ChoiceId) ?? Enumerable.Empty<int>())
            .ToHashSet();

        if (!submittedChoiceIds.All(id => validationData.ValidChoiceIds.Contains(id)))
            throw new InvalidOperationException("One or more selected choices are invalid.");

        if (string.IsNullOrWhiteSpace(dto.UserId))
            dto.UserId = null;

        var response = dto.ToDominEntity();

        var createdResponse = await _responseRepository.SubmitResponseAsync(response);

        return createdResponse.ToDetailsDto();
    }

    public async Task<int> GetResponsesCountAsync(int surveyId)
    {
        var count = await _responseRepository.GetResponsesCountAsync(surveyId);
        if(count == 0) throw new InvalidOperationException("No responses found for the specified survey.");
            return count;
    }
}