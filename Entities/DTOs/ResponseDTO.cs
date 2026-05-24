namespace DTOs;
using Entities;
public class SurveyResponsesDto
{
    public int SurveyId { get; set; }
    public List<ResponseDto> Responses { get; set; } = new();
}

public class ResponseDto
{
    public int ResponseId { get; set; }
    public string Title { get; set; }   
    public DateTime SubmittedAt { get; set; }
    public List<AnswerQuestionDto> Answers { get; set; } = new();
}