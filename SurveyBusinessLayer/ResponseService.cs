using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Models;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Interface;

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
        
        var responseDtos = response.Select(r => new ResponseDto
        {
            ResponseId = r.Id,
            Title = r.Survey.Title,
            SubmittedAt = r.SubmittedAt,
            Answers = r.Answers.Select(a => new AnswerQuestionDto
            {
                Id = a.Id,
                QuestionText = a.Question.QuestionText,
                AnswerType = a.Question.QuestionTypeEnum.ToString(),
                AnswerValue = a.AnswerValue,
                RankedChoices = a.AnswerSelections?.Select(s => new ChoiceRankingDto
                {
                    ChoiceId = s.ChoiceId,
                    RankOrder = s.RankOrder
                }).ToList() ?? new List<ChoiceRankingDto>()
            }).ToList()
        }).ToList();

        return responseDtos;
    }

    public async Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId, int pageSize, int pageNumber)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Survey ID must be a positive integer.", nameof(surveyId));
        
        var responses = await _responseRepository.GetResponsesBySurveyIdAsync(surveyId, pageSize, pageNumber);
        
        if(responses.Count == 0)
            throw new KeyNotFoundException($"No responses found for survey ID {surveyId}.");

        var responseDtos = responses.Select(r => new ResponseDto
        {
            ResponseId = r.Id,
            Title = r.Survey.Title,
            SubmittedAt = r.SubmittedAt,
            Answers = r.Answers.Select(a => new AnswerQuestionDto
            {
                Id = a.Id,
                QuestionText = a.Question.QuestionText,
                AnswerType = a.Question.QuestionTypeEnum.ToString(),
                AnswerValue = a.AnswerValue,
                RankedChoices = a.AnswerSelections?.Select(s => new ChoiceRankingDto
                {
                    ChoiceId = s.ChoiceId,
                    RankOrder = s.RankOrder
                }).ToList() ?? new List<ChoiceRankingDto>()
            }).ToList()
        }).ToList();

        return responseDtos;
    }
    
    public async Task<List<ResponseDto>> GetResponsesByUserIdAsync(string userId, int pageSize, int pageNumber)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID must be a non-empty value.", nameof(userId));
               
        var responses = await _responseRepository.GetResponsesByUserIdAsync(userId, pageSize, pageNumber);
        
        if(responses.Count == 0)
            throw new KeyNotFoundException($"No responses found for user ID {userId}.");

        var responseDtos = responses.Select(r => new ResponseDto
        {
            ResponseId = r.Id,
            Title = r.Survey.Title,
            SubmittedAt = r.SubmittedAt,
            Answers = r.Answers.Select(a => new AnswerQuestionDto
            {
                Id = a.Id,
                QuestionText = a.Question.QuestionText,
                AnswerType = a.Question.QuestionTypeEnum.ToString(),
                AnswerValue = a.AnswerValue,
                RankedChoices = a.AnswerSelections?.Select(s => new ChoiceRankingDto
                {
                    ChoiceId = s.ChoiceId,
                    RankOrder = s.RankOrder
                }).ToList() ?? new List<ChoiceRankingDto>()
            }).ToList()
        }).ToList();

        return responseDtos;

    }

    public async Task<ResponseDto> GetResponseByIdAsync(int responseId)
    {
        if (responseId <= 0)
            throw new ArgumentException("Response ID must be a positive integer.", nameof(responseId));
        
        var response = await _responseRepository.GetResponseByIdAsync(responseId);
        
        if(response == null)
            throw new KeyNotFoundException($"No response found with ID {responseId}.");

        var responseDtos = new ResponseDto
        {
            ResponseId = response.Id,
            Title = response.Survey.Title,
            SubmittedAt = response.SubmittedAt,
            Answers = response.Answers.Select(a => new AnswerQuestionDto
            {
                Id = a.Id,
                QuestionText = a.Question.QuestionText,
                AnswerType = a.Question.QuestionTypeEnum.ToString(),
                AnswerValue = a.AnswerValue,
                RankedChoices = a.AnswerSelections?.Select(s => new ChoiceRankingDto
                {
                    ChoiceId = s.ChoiceId,
                    RankOrder = s.RankOrder
                }).ToList() ?? new List<ChoiceRankingDto>()
            }).ToList()
        };

        return responseDtos;

    }
    
    public async Task<ResponseDetailsDto> SubmitResponseAsync(ResponseCreateDto responseCreateDto)
    {
        // survey validation
        if (responseCreateDto == null)
            throw new ArgumentNullException(nameof(responseCreateDto), "Response data cannot be null.");

        if(responseCreateDto.Answers == null || !responseCreateDto.Answers.Any())
            throw new InvalidOperationException("required questions must be answered");

        var validationData = await _responseRepository.GetValidationDataForSurveyAsync(responseCreateDto.SurveyId);

        if (validationData?.IsAnonymous == null)
            throw new KeyNotFoundException($"Survey with ID {responseCreateDto.SurveyId} not found.");

        if ( validationData.IsAnonymous  == false && string.IsNullOrEmpty(responseCreateDto.UserId))
            throw new InvalidOperationException("Survey is not anonymous, userId must be provided.");

        var missingRequiredIds = validationData.RequiredQuestionIds
            .Where(id => !responseCreateDto.Answers.Select(a => a.QuestionId)
            .Contains(id))
            .Count();

        if (missingRequiredIds != 0)
            throw new InvalidOperationException("All required questions must be answered.");

        var submittedChoiceIds = responseCreateDto.Answers
            .SelectMany(a => a.RankedChoices?.Select(s => s.ChoiceId) ?? Enumerable.Empty<int>())
            .ToHashSet();

        if (!submittedChoiceIds.All(id => validationData.ValidChoiceIds.Contains(id)))
            throw new InvalidOperationException("One or more selected choices are invalid.");

        var response = new Response
        {
            SurveyId = responseCreateDto.SurveyId,
            UserId = responseCreateDto.UserId,
            SubmittedAt = responseCreateDto.SubmittedAt,
            Answers = responseCreateDto.Answers.Select(a => new Answer
            {
                QuestionId = a.QuestionId,
                AnswerType = a.AnswerType,
                AnswerValue = a.AnswerValue,
                AnswerSelections = a.RankedChoices?.Select(s => new AnswerSelection
                {
                    ChoiceId = s.ChoiceId,
                    RankOrder = s.RankOrder
                }).ToList() ?? new List<AnswerSelection>()
            }).ToList()
        };
        var createdResponse = await _responseRepository.SubmitResponseAsync(response);

        var responseDetailsDto = new ResponseDetailsDto
        {
            ResponseId = createdResponse.Id,
            Title = createdResponse.Survey.Title,
            SubmittedAt = createdResponse.SubmittedAt,
            Answers = createdResponse.Answers.Select(a => new AnswerQuestionDto
            {
                QuestionText= a.Question.QuestionText,
                AnswerType = a.Question.QuestionTypeEnum.ToString(),
                AnswerValue = a.AnswerValue,
                RankedChoices = a.AnswerSelections?.Select(s => new ChoiceRankingDto
                {
                    ChoiceId = s.ChoiceId,
                    RankOrder = s.RankOrder
                }).ToList() ?? new List<ChoiceRankingDto>()
            }).ToList()
        };
        return responseDetailsDto;
    }

    public async Task<int> GetResponsesCountAsync()
    {
        throw new NotImplementedException();
    }
}