using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Repository.Models;

public enum enQuestionType 
{
    Text = 1,
    Radio = 2,
    Checkbox = 3,
    Rating = 4,
    Rank = 5,
    Matrix = 6,
}

public partial class Question
{
    public int Id { get; set; }

    public int SurveyId { get; set; }

    public int QuestionTypeId { get; set; }

    public string QuestionText { get; set; } = null!;

    public enQuestionType QuestionTypeEnum
    {
        get => (enQuestionType)QuestionTypeId;
        set => QuestionTypeId = (int)value;
    }

    public bool IsRequired { get; set; }

    public int OrderIndex { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();
    public virtual Survey Survey { get; set; } = null!;

    private bool IsValidateQuestionType() => Enum.IsDefined(typeof(enQuestionType), QuestionTypeId);

    private bool IsValidateChoices()
    {
        if (QuestionTypeEnum == enQuestionType.Radio || QuestionTypeEnum == enQuestionType.Checkbox || QuestionTypeEnum == enQuestionType.Rank)
        {
            if (Choices == null || !Choices.Any())
                throw new ArgumentException("Question must have choices.");
        }
        else if (QuestionTypeEnum == enQuestionType.Text)
        {
            if (Choices != null && Choices.Any())
                throw new ArgumentException("Text question must not have choices.");
        }
        return true;
    }

    private bool IsValidateQuestionText() => !string.IsNullOrWhiteSpace(QuestionText);
   
    private bool IsValidateOrderIndex() => OrderIndex > 0;

    public bool IsValid()
    {
        return IsValidateQuestionType() && IsValidateChoices() && IsValidateQuestionText();
    }

}
