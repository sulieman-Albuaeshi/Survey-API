namespace Entities;


public enum QuestionType : byte
{
    Text = 0,
    Radio = 1,
    Checkbox = 2,
    Rating = 3,
    Rank = 4,
    Matrix = 5
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
    public string SettingsJSON { get; set; } 
    
}