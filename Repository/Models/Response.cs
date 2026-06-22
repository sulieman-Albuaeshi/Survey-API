using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Response
{
    public int Id { get; set; }

    public int SurveyId { get; set; }

    public string? UserId { get; set; }

    public bool IsActive { get; set; }

    public DateTime SubmittedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Survey Survey { get; set; } = null!;
}

public class ResponseValidationDataDto
{
    public bool? IsAnonymous { get; set; }
    public List<int> RequiredQuestionIds { get; set; } = new List<int>();
    public List<int> ValidChoiceIds { get; set; } = new List<int>();
}