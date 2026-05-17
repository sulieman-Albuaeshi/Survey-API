using Entities;
using DTOs;

namespace SurveyApplication.Mapper;

public class ChoiceMapper
{
    public static Choice ToEntity(CreateChoiceDto dto)
    {
        return new Choice
        {
            ChoiceText = dto.ChoiceText,
            OrderIndex = dto.OrderIndex,
        };
    }
    
    public static Choice ToEntity(ChoiceDto dto)
    {
        return new Choice
        {
            Id = dto.Id,
            ChoiceText = dto.ChoiceText,
            OrderIndex = dto.OrderIndex,
        };
    }
    
    public static List<ChoiceDto> ToChoiceDto(List<Choice> choices)
    {
        var choiceDtoList = new List<ChoiceDto>();
        
        choiceDtoList = choices.Select(c => new ChoiceDto
        {
            Id = c.Id,
            ChoiceText = c.ChoiceText,
            OrderIndex = c.OrderIndex
        }).ToList();
        return choiceDtoList;
    }
    
    public static List<Choice> ToChoice(QuestionDetailsDto dto)
    {
        var queList = new List<Choice>();
        queList = dto.Choices.Select(c => ChoiceMapper.ToEntity(c)).ToList();
        return queList;

    }
}