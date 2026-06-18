namespace SurveyBusinessLayer.DTOs;

public class SurveyDto
{
    public int Id { get; set; }
    public required string  Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public int QuestionCount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

}

public class CreateSurveyDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; } 
    public bool IsActive { get; set; }
    public string userId { get; set; } = string.Empty;
}
