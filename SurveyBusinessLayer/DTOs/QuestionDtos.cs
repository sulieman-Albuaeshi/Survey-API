namespace SurveyBusinessLayer.DTOs;
using System.Text.Json;
using Repository.Models;

public class UpdateQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public bool IsRequired { get; set; }
    public string QuestionType { get; set; } = null!;
    public List<updateChoiceDto> Choices { get; set; } = new();
}

public class QuestionDetailsDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public bool IsRequired { get; set; }

    public int OrderIndex { get; set; }
    public string QuestionTypeName { get; set; } = null!;
    public List<ChoiceDto> Choices { get; set; } = new();
}

public class CreateQuestionDto
{
    public string QuestionText { get; set; } = null!;
    public bool IsRequired { get; set; }
    public string QuestionType { get; set; } = null!;
    public List<CreateChoiceDto> Choices { get; set; } = new();
}