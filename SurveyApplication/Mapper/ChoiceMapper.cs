using Entities;
using DTOs;

namespace SurveyApplication.Mapper;

public class ChoiceMapper
{
    public static Choice ToChoice(ChoiceDto dto)
    {
        return new Choice
        {
            Id = dto.Id,
            ChoiceText = dto.ChoiceText,
            OrderIndex = dto.OrderIndex,
        };
    }
    
    public static List<Choice> ToChoice(QuestionDetailsDto dto)
    {
        var queList = new List<Choice>();
        queList = dto.Choices.Select(c => ChoiceMapper.ToChoice(c)).ToList();
        return queList;

    }
}