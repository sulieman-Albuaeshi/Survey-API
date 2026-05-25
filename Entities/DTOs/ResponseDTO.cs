namespace DTOs;

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

public class ResponseCreateDto
{
    public int ResponseId { get; set; }
    public int SurveyId { get; set; }
    public string UserId { get; set; }
    public List<AnswerCreateDto> Answers { get; set; } = new();
}