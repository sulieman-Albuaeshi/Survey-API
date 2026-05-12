namespace Entities;

public class Responses
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public int QuestionId { get; set; }
    public required string Response { get; set; }
}