using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Repository.Models;

public enum enQuestionType : byte
{
    Text = 1,
    Radio = 2,
    Checkbox = 3,
    Rating = 4,
    Rank = 5,
    Matrix = 6,
}

public partial class Question
{
    public int Id { get; set; }

    public int SurveyId { get; set; }

    public int QuestionTypeId { get; set; }

    public string QuestionText { get; set; } = null!;

    public enQuestionType QuestionTypeEnum
    {
        get => (enQuestionType)QuestionTypeId;
        set => QuestionTypeId = (int)value;
    }

    public bool IsRequired { get; set; }

    public int OrderIndex { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();
    public virtual Survey Survey { get; set; } = null!;
}
