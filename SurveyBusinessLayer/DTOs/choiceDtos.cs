using System.Runtime.CompilerServices;

namespace SurveyBusinessLayer.DTOs;

public class ChoiceDto
{
    public int Id { get; set; }
    public string ChoiceText { get; set; } = null!;
    public int OrderIndex { get; set; } 
}

public class CreateChoiceDto
{
    public string ChoiceText { get; set; } = null!;
}
public class updateChoiceDto
{
    public int Id { get; set; }

    public string ChoiceText { get; set; } = null!;
}
