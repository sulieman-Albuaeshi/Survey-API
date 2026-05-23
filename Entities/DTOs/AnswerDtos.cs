using Entities;

namespace DTOs;

public class AnswerQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; }
    public object Value { get; set; }
    public QuestionType AnswerType { get; set; }
}

public class AnswerSelectionDto
{
    public int ChoiceId { get; set; }
    public int RankOrder { get; set; }
}
