using FluentValidation;
using SurveyBusinessLayer.DTOs;
using Repository.Models;

namespace SurveyApplication.Validation.Survey
{
    public class SurveyUpdateDtoValidation : BaseSurveyValidator<UpdaatSurveyDto>
    {
        public SurveyUpdateDtoValidation() : base() 
        {
            RuleFor(s => s.Id)
                .Must(id => id > 0).WithMessage("Invalid Survey ID.");

            RuleFor(q => q.Questions)
                .NotEmpty().WithMessage("Survey must contain at least one question.")
                .Must(questions => questions.Count >= 1).WithMessage("Survey must contain at least one question.");

        }
    }

    public class QuestionUpdateDtoValidation : BaseQuestionValidator<UpdateQuestionDto>
    {
        public QuestionUpdateDtoValidation() : base()
        {
            RuleFor(q => q.Id)
                .Must(id => id > 0).WithMessage("Invalid Question ID.");

            When(q => q.QuestionType == enQuestionType.Radio.ToString() ||
                      q.QuestionType == enQuestionType.Checkbox.ToString() ||
                      q.QuestionType == enQuestionType.Rank.ToString(),
                      () =>
                      {
                          RuleFor(q => q.Choices)
                              .NotEmpty().WithMessage("Choices are required for multiple choice, Radio choice and Rank questions.")
                              .Must(choices => choices.Count >= 2).WithMessage("At least two choices are required for multiple choice and single choice questions.");

                          RuleForEach(q => q.Choices)
                          .Must(choice => choice.Id > 0).WithMessage("Invalid Choice ID");

                      });
        }
    }
}
