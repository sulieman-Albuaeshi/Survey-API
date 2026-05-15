namespace DTOs;

using Entities;
public class SurveyTableDto
{ 
    public int Id { get; set; }
    public required string  Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public required string Status { get; set; }
    
    // TODO QuestionCount, ResponseCount
}

public class SurveyDto
{
    public int Id { get; set; }
    public required string  Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsActive { get; set; }
    
    public string Status { get; set; }
    public string UserId { get; set; }
}

public class SurveyDetailsDto
{
    public int Id { get; set; }
    public  string Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string UserId { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}
