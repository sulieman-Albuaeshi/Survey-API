using System;
using System.Collections.Generic;

namespace Repository.Models;

public enum SurveyStatus : Byte
{
    Draft = 0,
    Published = 1,
    Archived = 2
}
public partial class Survey
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsAnonymous { get; set; }

    public SurveyStatus Status { get; set; }

    public int QuestionCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
}
