using System.Runtime.CompilerServices;

namespace SurveyBusinessLayer.DTOs;

public interface IChoiceDto
{
    string ChoiceText { get; set; }
}

public class ChoiceDto
{
    public int Id { get; set; }
    public string ChoiceText { get; set; } = null!;
    public int OrderIndex { get; set; } 
}

public class CreateChoiceDto : IChoiceDto
{
    public string ChoiceText { get; set; } = null!;
}
public class updateChoiceDto : IChoiceDto
{
    public int Id { get; set; }

    public string ChoiceText { get; set; } = null!;
}

public class ChoiceRankingDto
{
    public int ChoiceId { get; set; }
    public int RankOrder { get; set; }
}