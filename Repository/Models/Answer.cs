using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int ResponseId { get; set; }

    public int QuestionId { get; set; }

    public enQuestionType AnswerType { get; set; }

    public string? AnswerValue { get; set; }

    public virtual ICollection<AnswerSelection> AnswerSelections { get; set; } = new List<AnswerSelection>();

    public virtual Question Question { get; set; } = null!;

    public virtual Response Response { get; set; } = null!;
}
