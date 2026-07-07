using FluentValidation;
using SurveyBusinessLayer.DTOs;
using Repository.Models;

namespace SurveyApplication.Validation.Survey
{
    public class SurveyCreaterDtoValidation : BaseSurveyValidator<CreateSurveyDto>
    {
        public SurveyCreaterDtoValidation() : base()
        {
            RuleFor(q => q.Questions)
                .NotEmpty().WithMessage("Survey must contain at least one question.")
                .Must(questions => questions.Count >= 1).WithMessage("Survey must contain at least one question.");

            RuleForEach(s => s.Questions).SetValidator(new QuestionCreateDtoValidation());
        }
    }
    public class QuestionCreateDtoValidation : BaseQuestionValidator<CreateQuestionDto>
    {
        public QuestionCreateDtoValidation() : base()
        {
            When(q => q.QuestionType == enQuestionType.Radio.ToString() ||
                      q.QuestionType == enQuestionType.Checkbox.ToString() ||
                      q.QuestionType == enQuestionType.Rank.ToString(), 
                      () => {
                RuleFor(q => q.Choices)
                    .NotEmpty().WithMessage("Choices are required for multiple choice, Radio choice and Rank questions.")
                    .Must(choices => choices.Count >= 2).WithMessage("At least two choices are required for multiple choice and single choice questions.");
            });
        }
    }
}
