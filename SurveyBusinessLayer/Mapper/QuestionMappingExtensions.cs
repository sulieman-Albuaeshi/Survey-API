using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Mapper
{
    public static class QuestionMappingExtensions
    {
        public static QuestionDetailsDto ToDetailDto(this Question question)
        { 
            return new QuestionDetailsDto
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                IsRequired = question.IsRequired,
                OrderIndex = question.OrderIndex,
                QuestionTypeName = question.QuestionTypeEnum.ToString(),
                Choices = question.Choices.Select(c => new ChoiceDto
                {
                    Id = c.Id,
                    ChoiceText = c.ChoiceText,
                    OrderIndex = c.OrderIndex
                }).ToList(),
            };
        }
    }
}
