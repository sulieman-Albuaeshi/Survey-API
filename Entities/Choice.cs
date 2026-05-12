namespace Entities;

public class Choice
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public required string ChoiceText { get; set; }
    public int OrderIndex { get; set; }
}