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
            QuestionType = Enum.Parse<QuestionType>(dto.QuestionType.ToString())
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
            QuestionType = Enum.Parse<QuestionType>(dto.QuestionType.ToString()),
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
            QuestionType = Enum.Parse<QuestionType>(question.QuestionType.ToString()),
            // TODO : Choices = question.Choices.Select(c => ChoiceMapper.ToChoiceDto(c)).ToList()
        };
    }
}