using System.Text.Json;
using Entities;

namespace DTOs;

public class AnswerQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; }
    public object Value { get; set; }
    public QuestionType AnswerType { get; set; }
}

public class AnswerCreateDto
{
    public int QuestionId { get; set; }
    public QuestionType AnswerType { get; set; }
    public JsonElement Value { get; set; }
}