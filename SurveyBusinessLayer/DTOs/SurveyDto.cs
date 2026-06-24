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
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public string Status { get; set; } = null!;

    public string userId { get; set; } = string.Empty;
    public List<CreateQuestionDto> Questions { get; set; } = new List<CreateQuestionDto>();
}

public class UpdaatSurveyDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public string Status { get; set; } = null!;
    public List<UpdateQuestionDto> Questions { get; set; } = new List<UpdateQuestionDto>();
}

public class SurveyDetailsDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public int QuestionCount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<QuestionDetailsDto> Questions { get; set; } = new List<QuestionDetailsDto>();
}

public class SurveyStatusDto
{
    public string StatusText { set; get; } = null!;
}
