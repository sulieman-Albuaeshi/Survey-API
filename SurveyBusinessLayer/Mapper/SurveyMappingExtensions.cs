using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Mapper
{
    public static class SurveyMappingExtensions
    {
        public static Survey ToDominEntity(this SurveyDto dto)
        {
            return new Survey
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                IsAnonymous = dto.IsAnonymous,
                Status = Enum.TryParse<SurveyStatus>(dto.Status, out var status) ? status : SurveyStatus.Draft,
                CreatedAt = dto.CreatedAt
            };
        }

        public static Survey ToDominEntity(this CreateSurveyDto dto)
        {
            return new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                IsAnonymous = dto.IsAnonymous,
                Status = Enum.TryParse<SurveyStatus>(dto.Status, out var status) ? status : SurveyStatus.Draft,
                Questions = dto.Questions.Select(q => new Question
                {
                    QuestionText = q.QuestionText,
                    IsRequired = q.IsRequired,
                    QuestionTypeEnum = Enum.Parse<enQuestionType>(q.QuestionType),
                    Choices = q.Choices.Select(c => new Choice
                    {
                        ChoiceText = c.ChoiceText,
                    }).ToList()
                }).ToList(),
                UserId = Guid.Parse(dto.userId)
            };
        }

        public static Survey ToDominEntity(this UpdaatSurveyDto dto)
        {
            return new Survey
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                IsAnonymous = dto.IsAnonymous,
                Status = Enum.TryParse<SurveyStatus>(dto.Status, out var status) ? status : SurveyStatus.Draft,
                Questions = dto.Questions.Select(q => new Question
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    IsRequired = q.IsRequired,
                    QuestionTypeEnum = Enum.Parse<enQuestionType>(q.QuestionType),
                    Choices = q.Choices.Select(c => new Choice
                    {
                        Id = c.Id,
                        ChoiceText = c.ChoiceText,
                    }).ToList()
                }).ToList()
            };
        }

        public static SurveyDto ToDto(this Survey entity)
        {
            return new SurveyDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                IsAnonymous = entity.IsAnonymous,
                Status = entity.Status.ToString(),
                CreatedAt = entity.CreatedAt
            };
        }

        public static SurveyDetailsDto ToDetailsDto(this Survey entity)
        {
            return new SurveyDetailsDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                IsAnonymous = entity.IsAnonymous,
                Status = entity.Status.ToString(),
                CreatedAt = entity.CreatedAt,
                Questions = entity.Questions.Select(q => new QuestionDetailsDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    IsRequired = q.IsRequired,
                    OrderIndex = q.OrderIndex,
                    QuestionType = q.QuestionTypeEnum.ToString(),
                    Choices = q.Choices.Select(c => new ChoiceDto
                    {
                        Id = c.Id,
                        ChoiceText = c.ChoiceText,
                        OrderIndex = c.OrderIndex
                    }).ToList()
                }).ToList()
            };
        }
    }
}
