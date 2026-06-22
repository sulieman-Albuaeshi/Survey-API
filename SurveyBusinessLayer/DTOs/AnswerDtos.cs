using Repository.Models;
using System.Text.Json.Serialization;

namespace SurveyBusinessLayer.DTOs;

public class AnswerQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string AnswerType { get; set; } = null!;
    public string? AnswerValue { get; set; }
    public List<ChoiceRankingDto>? RankedChoices { get; set; }
}

public class AnswerCreateDto
{
    public int QuestionId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enQuestionType AnswerType { get; set; }
    public string? AnswerValue { get; set; }
    // For multiple choice questions, Rank Question and single choice questions
    public List<ChoiceRankingDto>? RankedChoices { get; set; }
}