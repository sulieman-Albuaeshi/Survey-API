using FluentValidation;
using SurveyBusinessLayer.DTOs;
using Repository.Models;

namespace SurveyApplication.Validation.Survey
{
    public class SurveyUpdateDtoValidation : AbstractValidator<UpdaatSurveyDto>
    {
        public SurveyUpdateDtoValidation() 
        {
            RuleFor(s => s.Title)
            .MaximumLength(200).WithMessage("Survey title cannot exceed 200 characters.")
            .When(s => !string.IsNullOrEmpty(s.Title));


            RuleFor(s => s.Status)
                .Must((status => status == "Draft" || status == "Published" || status == "Archived"))
                .WithMessage("Survey status must be either 'Draft' or 'Published' or 'Archived'.")
                .When(s => !string.IsNullOrEmpty(s.Status));

            RuleFor(s => s.Id)
                .Must(id => id > 0).WithMessage("Invalid Survey ID.");

            RuleFor(q => q.Questions)
                .Must(questions => questions.Count >= 1).WithMessage("Survey must contain at least one question.")

        }
    }

    public class QuestionUpdateDtoValidation : AbstractValidator<UpdateQuestionDto>
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
                              .Must(choices => choices.Count >= 2).WithMessage("At least two choices are required for multiple choice and single choice questions.")
                              .When(q => q.Choices != null); // <-- Added condition

                          When(q => q.Choices != null, () =>
                          {
                              RuleForEach(q => q.Choices)
                              .Must(choice => choice.Id > 0).WithMessage("Invalid Choice ID");
                          };                   

                      });
        }
    }
}
