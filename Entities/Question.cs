using System.Text.Json.Serialization;

namespace Entities;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestionType : int
{
    Text = 3,
    Radio = 4,
    Checkbox = 5,
    Rating = 6,
    Rank = 7,
    Matrix = 8
}

public class Question
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public required string QuestionText { get; set; }
    public QuestionType QuestionType { get; set; } 
    public bool IsRequired { get; set; }
    public int OrderIndex { get; set; }

    // Store configuration like {"min":1, "max":5} here
    public object? SettingsJSON { get; set; } 
    
}