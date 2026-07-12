using System;
using System.Collections.Generic;

namespace Repository.Models;

public  class AnswerSelection
{
    public int Id { get; set; }

    public int AnswerId { get; set; }

    public int ChoiceId { get; set; }

    public int RankOrder { get; set; }

    public virtual Answer Answer { get; set; } = null!;

    public virtual Choice Choice { get; set; } = null!;
}
