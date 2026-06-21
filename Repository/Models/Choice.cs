using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Choice
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string ChoiceText { get; set; } = null!;

    public int OrderIndex { get; set; }

    public bool IsRandomized { get; set; }

    public virtual ICollection<AnswerSelection> AnswerSelections { get; set; } = new List<AnswerSelection>();

    public virtual Question Question { get; set; } = null!;
}
