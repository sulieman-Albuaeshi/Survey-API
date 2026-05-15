using DTOs;
using Entities;

namespace SurveyApplication.Mapper;

public class QuestionMapper
{
    public static Question ToQuestionEntity(QuestionDto dto)
    {
        return new Question
        {
            Id = dto.Id,
            QuestionText = dto.QuestionText,
            IsRequired = dto.IsRequired,
            OrderIndex = dto.OrderIndex,
            SettingsJSON = dto.SettingsJSON,
            QuestionType = dto.QuestionType
        };
    }
    
    public static Question ToQuestionEntity(QuestionDetailsDto dto)
    {
        return new Question
        {
            Id = dto.Id,
            QuestionText = dto.QuestionText,
            IsRequired = dto.IsRequired,
            OrderIndex = dto.OrderIndex,
            SettingsJSON = dto.SettingsJSON,
            QuestionType = dto.QuestionType
        };
    }
    
    public static QuestionDetailsDto ToQuestionDetailsDto(Question question)
    {
        return new QuestionDetailsDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            IsRequired = question.IsRequired,
            OrderIndex = question.OrderIndex,
            SettingsJSON = question.SettingsJSON,
            QuestionType = question.QuestionType,
            // TODO : Choices = question.Choices.Select(c => ChoiceMapper.ToChoiceDto(c)).ToList()
        };
    }
}