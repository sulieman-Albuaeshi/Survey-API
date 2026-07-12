namespace SurveyBusinessLayer.DTOs;

public interface ISurveyBaseDto
{
    string Title { get; set; }
    string? Description { get; set; }
    bool IsAnonymous { get; set; }
    string Status { get; set; }
    public string userId { get; set; }
}

public class SurveyDto
{
    public int Id { get; set; }
    public required string  Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

}

public class CreateSurveyDto : ISurveyBaseDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public string Status { get; set; } = null!;
    public string userId { get; set; } = null!;
    public List<CreateQuestionDto> Questions { get; set; } = new List<CreateQuestionDto>();
}

public class UpdaatSurveyDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsAnonymous { get; set; }
    public string? Status { get; set; } = null!;
    public string? userId { get; set; } = string.Empty;
    public List<UpdateQuestionDto> Questions { get; set; } = new List<UpdateQuestionDto>();
}

public class SurveyDetailsDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<QuestionDetailsDto> Questions { get; set; } = new List<QuestionDetailsDto>();
}

public class SurveyStatusDto
{
    public string StatusText { set; get; } = null!;
}
