namespace Entities;

public class AnswerSelection
{
    public int Id { get; set; }
    public int AnswerId { get; set; }
    public int ChoiceId { get; set; }
    public int RankOrder { get; set; } // Used for ranking, 0 for checkboxes
}