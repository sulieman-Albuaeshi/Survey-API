namespace SurveyDataAccessLayer.rowDTO;
using Entities;
public class SurveyResponseRow
{
    public int ResponseId { get; set; }
    public int SurveyId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public int AnswerId { get; set; }
    public int QuestionId { get; set; }
    public QuestionType AnswerType { get; set; }

    public string? TextAnswer { get; set; }
    public string? RatingAnswer { get; set; }

    public int? ChoiceId { get; set; }
    public int? RankOrder { get; set; }
}