using SurveyBusinessLayer.Interface;
using Repository.Interface;
using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer;

public class SurveyService : ISurveyService
{
    private readonly ISurveyRepository _surveyRepository;
    
    public SurveyService(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }
    
    public async Task<List<SurveyDto>> GetAllSurveysAsync(int pageSize, int pageNumber)
    {
        if (pageSize <= 0 || pageNumber <= 0)
            throw new ArgumentException("Page size and page number must be greater than zero.");

        var surveyList = await _surveyRepository.GetAllSurveysAsync(pageSize, pageNumber);
        if (surveyList == null || !surveyList.Any())
            throw new KeyNotFoundException("Survey not found.");

        List<SurveyDto> dto = surveyList.Select(s => new SurveyDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            IsAnonymous = s.IsAnonymous,
            QuestionCount = s.QuestionCount,
            Status = s.Status.ToString(),
            CreatedAt = s.CreatedAt
        }).ToList();

        return dto;
    }
    
    public async Task<SurveyDetailsDto?> GetSurveyByIdAsync(int surveyId)
    {
        var survey = await _surveyRepository.GetSurveyByIdAsync(surveyId);
        if (survey == null)
            throw new KeyNotFoundException("Survey not found.");

        var surveyDtoDTOs = new SurveyDetailsDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            IsAnonymous = survey.IsAnonymous,
            QuestionCount = survey.QuestionCount,
            Status = survey.Status.ToString(),
            CreatedAt = survey.CreatedAt,
            Questions = survey.Questions.Select(q => new QuestionDetailsDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                IsRequired = q.IsRequired,
                OrderIndex = q.OrderIndex,
                QuestionTypeName = q.QuestionTypeEnum.ToString(),
                Choices = q.Choices.Select(c => new ChoiceDto
                {
                    Id = c.Id,
                    ChoiceText = c.ChoiceText,
                    OrderIndex = c.OrderIndex
                }).ToList()
            }).ToList()
        };

        return surveyDtoDTOs;
    }
    
    public async Task<SurveyDetailsDto> CreateSurveyWithQuestionsAsync(CreateSurveyDto SurveydetailsDto)
    {

        var MapedSurvey = new Survey
        {
            Title = SurveydetailsDto.Title,
            Description = SurveydetailsDto.Description,
            IsAnonymous = SurveydetailsDto.IsAnonymous,
            Status = Enum.Parse<SurveyStatus>(SurveydetailsDto.Status),
            Questions = SurveydetailsDto.Questions.Select(q => new Question
            {
                QuestionText = q.QuestionText,
                IsRequired = q.IsRequired,
                QuestionTypeEnum = Enum.Parse<enQuestionType>(q.QuestionType),
                Choices = q.Choices.Select(c => new Choice
                {
                    ChoiceText = c.ChoiceText,
                }).ToList()
            }).ToList(),
            UserId = SurveydetailsDto.userId,

        };

        if (string.IsNullOrEmpty(SurveydetailsDto.Title))
            throw new ArgumentException("Survey title is required.");

        foreach (var question in MapedSurvey.Questions)
        {
            if (!question.IsValid())
            {
                throw new ArgumentException("Invalid question data.");
            }
        }

        var createdSurvey = await _surveyRepository.CreateSurveyAsync(MapedSurvey);

        if (createdSurvey == null)
            throw new KeyNotFoundException("Survey was not created.");

        var createdSurveyDto = new SurveyDetailsDto
        {
            Id = createdSurvey.Id,
            Title = createdSurvey.Title,
            Description = createdSurvey.Description,
            IsAnonymous = createdSurvey.IsAnonymous,
            QuestionCount = createdSurvey.QuestionCount,
            Status = createdSurvey.Status.ToString(),
            CreatedAt = createdSurvey.CreatedAt,
            Questions = createdSurvey.Questions.Select(q => new QuestionDetailsDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                IsRequired = q.IsRequired,
                OrderIndex = q.OrderIndex,
                QuestionTypeName = q.QuestionTypeEnum.ToString(),
                Choices = q.Choices.Select(c => new ChoiceDto
                {
                    Id = c.Id,
                    ChoiceText = c.ChoiceText,
                    OrderIndex = c.OrderIndex
                }).ToList()
            }).ToList()
        };

        return createdSurveyDto;
    }

    public async Task<SurveyDetailsDto> UpdateSurveyWithQuestionsAsync(UpdaatSurveyDto SurveydetailsDto)
    {
        // TODO : need to check if the same user trying to update the same survey
        var MapedSurvey = new Survey
        {
            Id = SurveydetailsDto.Id,
            Title = SurveydetailsDto.Title,
            Description = SurveydetailsDto.Description,
            IsAnonymous = SurveydetailsDto.IsAnonymous,
            Status = Enum.Parse<SurveyStatus>(SurveydetailsDto.Status),
            QuestionCount = SurveydetailsDto.Questions.Count(),
            Questions = SurveydetailsDto.Questions.Select(q => new Question
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                IsRequired = q.IsRequired,
                QuestionTypeId = (int)Enum.Parse<enQuestionType>(q.QuestionType),
                Choices = q.Choices.Select(c => new Choice
                {
                    Id = c.Id,
                    ChoiceText = c.ChoiceText,
                }).ToList()
            }).ToList()
        };

        if (MapedSurvey.Status == SurveyStatus.Published)
            throw new InvalidOperationException("Cannot update a published survey.");
        
        if(MapedSurvey.Id <= 0)
            throw new ArgumentException("Invalid survey ID.");


        if (string.IsNullOrEmpty(SurveydetailsDto.Title))
            throw new ArgumentException("Survey title is required.");

        foreach (var question in MapedSurvey.Questions)
        {
            if (!question.IsValid())
            {
                throw new ArgumentException("Invalid question data.");
            }
        }

        if (string.IsNullOrEmpty(SurveydetailsDto.Title))
            throw new ArgumentException("Survey title is required.");

        foreach (var question in MapedSurvey.Questions)
        {
            if (!question.IsValid())
            {
                throw new ArgumentException("Invalid question data.");
            }
        }

        if (string.IsNullOrEmpty(SurveydetailsDto.Title))
            throw new ArgumentException("Survey title is required.");

        foreach (var question in MapedSurvey.Questions)
        {
            if (!question.IsValid())
            {
                throw new ArgumentException("Invalid question data.");
            }
        }

        var createdSurvey = await _surveyRepository.UpdateSurveyAsync(MapedSurvey);

        if (createdSurvey == null)
            throw new KeyNotFoundException("Survey was not updated.");

        var UpdatedSurveyDto = new SurveyDetailsDto
        {
            Id = createdSurvey.Id,
            Title = createdSurvey.Title,
            Description = createdSurvey.Description,
            IsAnonymous = createdSurvey.IsAnonymous,
            Status = createdSurvey.Status.ToString(),
            Questions = createdSurvey.Questions.Select(q => new QuestionDetailsDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                IsRequired = q.IsRequired,
                QuestionTypeName = q.QuestionTypeEnum.ToString(),
                Choices = q.Choices.Select(c => new ChoiceDto
                {
                    Id = c.Id,
                    ChoiceText = c.ChoiceText,
                }).ToList()
            }).ToList()
        };

        return UpdatedSurveyDto;
    }
    
    public async Task<bool> DeleteSurveyAsync(int surveyId)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");

        return await _surveyRepository.DeleteSurveyAsync(surveyId) == 1;
    }

    public async Task<bool> ChangeSurveyStatusAsync(int surveyId, string statusText)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");
        var status = await _surveyRepository.ChangeSurveyStatusAsync(surveyId, statusText);
        if (!status)
            throw new KeyNotFoundException("there is no survey with the specified ID.");
        return status;
    }

    public async Task<bool?> IsSurveyAnonymous(int surveyId)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");

        return await _surveyRepository.IsSurveyAnonymousAsync(surveyId);
    }
}