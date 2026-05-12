namespace Entities;
public class Answer
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }

    // Sparse Fields
    public string? TextValue { get; set; }
    public decimal? NumberValue { get; set; }
    public DateTime? DateValue { get; set; }
    
    public QuestionType AnswerType { get; set; } 

}