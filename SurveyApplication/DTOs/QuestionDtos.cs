namespace DTOs;
using Entities;
public class QuestionDto
{
    public int Id { get; set; }
    public  string QuestionText { get; set; }
    public bool IsRequired { get; set; }
    public int OrderIndex { get; set; }
    public object? SettingsJSON { get; set; }
    public QuestionType QuestionType { get; set; }
    // public List<ChoiceDto> Choices { get; set; } = new();
}

public class QuestionDetailsDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; }
    public bool IsRequired { get; set; }
    public int OrderIndex { get; set; }
    public object? SettingsJSON { get; set; }
    public QuestionType QuestionType { get; set; }
    public List<ChoiceDto> Choices { get; set; } = new();
}

public class ChoiceDto
{
    public int Id { get; set; }
    public string ChoiceText { get; set; }
    public int OrderIndex { get; set; }
    public bool IsRandomized { get; set; }
}