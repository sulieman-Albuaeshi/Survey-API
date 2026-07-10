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
    private readonly IUserRepository _userRepository;
    public ResponseService(IResponseRepository responseRepository, IUserRepository userRepository)
    {
        _responseRepository = responseRepository;
        _userRepository = userRepository;
    }
    
    private static void VerifyBusinessRules(ResponseCreateDto dto, ResponseValidationDataDto? validationData)
    {
        // Check if the survey exists and is valid
        if (validationData?.IsAnonymous == null)
            throw new KeyNotFoundException($"Survey with ID {dto.SurveyId} not found.");

        // Check if the survey is anonymous and if the userId is provided
        if (validationData?.IsAnonymous == false && string.IsNullOrEmpty(dto.UserId))
            throw new InvalidOperationException("Survey is not anonymous, userId must be provided.");

        ValidateRequiredQuestions(dto, validationData.RequiredQuestionIds);
        ValidateChoiceIds(dto, validationData.ValidChoiceIds);
    }
    private static void ValidateRequiredQuestions(ResponseCreateDto dto, IEnumerable<int> requiredQuestionIds)
    {
        var missingRequiredIds = requiredQuestionIds
            .Where(id => !dto.Answers.Select(a => a.QuestionId)
            .Contains(id))
            .Count();
        if (missingRequiredIds != 0)
            throw new InvalidOperationException("All required questions must be answered.");
    }
    private static void ValidateChoiceIds(ResponseCreateDto dto, IEnumerable<int> validChoiceIds)
    {
        var validChoiceSet = validChoiceIds.ToHashSet();
        var submittedChoiceIds = dto.Answers
            .SelectMany(a => a.RankedChoices?.Select(s => s.ChoiceId) ?? Enumerable.Empty<int>())
            .ToHashSet();

        if (!submittedChoiceIds.All(id => validChoiceSet.Contains(id)))
            throw new InvalidOperationException("One or more selected choices are invalid.");
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

        var responseDtos = responses.Select(r => r.ToDto()).ToList();

        return responseDtos;
    }
    
    public async Task<List<ResponseDto>> GetResponsesByUserIdAsync(Guid userId, int pageSize, int pageNumber)
    {
        if(await _userRepository.IsUserExist(userId))
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        var responses = await _responseRepository.GetResponsesByUserIdAsync(userId, pageSize, pageNumber);
        
        if(responses.Count == 0)
            throw new InvalidOperationException($"No responses found for user ID {userId}.");

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
        var validationData = await _responseRepository.GetValidationDataForSurveyAsync(dto.SurveyId);

        VerifyBusinessRules(dto, validationData);

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