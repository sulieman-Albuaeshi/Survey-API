using DTOs;
using Entities;
namespace SurveyApplication.Mapper;


public class SurveyMapper
{
    public static SurveyDetailsDto ToSurveyDetailsDto(Survey survey)
    {
        return new SurveyDetailsDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            IsActive = survey.IsActive,
            CreatedDate = survey.CreatedDate,
            IsAnonymous = survey.IsAnonymous,
            UserId = survey.UserId,
            Status = survey.Status.ToString(),
            // TODO : Questions = survey.Questions.Select(q => QuestionMapper.ToQuestionDto(q)).ToList()
        };
    }
    public static Survey ToSurveyEntity(SurveyDto surveyDto)
    {
        return new Survey
        {
            Id = surveyDto.Id,
            Title = surveyDto.Title,
            Description = surveyDto.Description,
            IsActive = surveyDto.IsActive,
            IsAnonymous = surveyDto.IsAnonymous,
            UserId = surveyDto.UserId,
            Status = Enum.TryParse<SurveyStatus>(surveyDto.Status, out var status) ? status : SurveyStatus.Draft,
            // TODO : Questions = surveyDto.Questions.Select(q => QuestionMapper.ToQuestionEntity(q)).ToList()
        };
    }
    public static SurveyTableDto ToSurveyTableDtos(Survey survey)
    {
        return new SurveyTableDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            CreatedDate = survey.CreatedDate,
            Status = survey.Status.ToString()
        };
    }
    public static SurveyDto ToSurveyDto(Survey survey)
    {
        return new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            IsActive = survey.IsActive,
            IsAnonymous = survey.IsAnonymous,
            UserId = survey.UserId,
            Status = survey.Status.ToString()
        };
    }
}