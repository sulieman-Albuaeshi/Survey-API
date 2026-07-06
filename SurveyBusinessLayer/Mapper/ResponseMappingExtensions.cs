using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Mapper
{
    public static class ResponseMappingExtensions
    {
        public static ResponseDto ToDto(this Response response)
        {
            return new ResponseDto
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
        }
        public static ResponseDetailsDto ToDetailsDto(this Response response)
        {
            return new ResponseDetailsDto
            {
                ResponseId = response.Id,
                Title = response.Survey.Title,
                SubmittedAt = response.SubmittedAt,
                Answers = response.Answers.Select(a => new AnswerQuestionDto
                {
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
        }

        public static Response ToDominEntity(this ResponseCreateDto Dto)
        {
            return new Response
            {
                SurveyId = Dto.SurveyId,
                UserId = Dto.UserId,
                SubmittedAt = Dto.SubmittedAt,
                Answers = Dto.Answers.Select(a => new Answer
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
        }

    }
}
