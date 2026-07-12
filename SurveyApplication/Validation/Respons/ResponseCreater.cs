using FluentValidation;
using Repository.Models;
using SurveyBusinessLayer.DTOs;

namespace SurveyApplication.Validation.Respons
{
    public class ResponseCreaterDtoValidation : AbstractValidator<ResponseCreateDto>
    {
        public ResponseCreaterDtoValidation() 
        {
            RuleFor(r => r.SurveyId)
                .GreaterThan(0).WithMessage("Survey ID must be greater than zero.");

            RuleForEach(r => r.Answers).SetValidator(new AnswerCreateDtoValidation());
        }
    }

    public class AnswerCreateDtoValidation : AbstractValidator<AnswerCreateDto>
    {
        public AnswerCreateDtoValidation()
        {
            RuleFor(a => a.QuestionId)
                .GreaterThan(0).WithMessage("Question ID must be greater than zero.");
            
            RuleFor(a => a.AnswerType)
                .NotEmpty().WithMessage("Answer type is required.")
                .Must(a => Enum.IsDefined(typeof(enQuestionType), a)).WithMessage("Invalid answer type.");

            When(a => a.AnswerType == enQuestionType.Checkbox ||
                a.AnswerType == enQuestionType.Radio ||
                a.AnswerType == enQuestionType.Rank,
                () =>
                {
                    RuleFor(a => a.RankedChoices)
                        .NotEmpty().WithMessage("Choice IDs are required for multiple choice, Radio choice and Rank questions.")
                        .Must(choiceIds => choiceIds?.Count >= 1).WithMessage("At least one choice ID is required for multiple choice, Radio choice and Rank questions.");

                    RuleFor(a => a.AnswerValue)
                        .Empty().WithMessage("Answer value should be empty for multiple choice, Radio choice and Rank questions.");

                    RuleForEach(a => a.RankedChoices).ChildRules(choice =>
                    {
                        choice.RuleFor(c => c.ChoiceId)
                            .GreaterThan(0).WithMessage("Choice ID must be greater than zero.");

                        choice.RuleFor(c => c.RankOrder)
                            .GreaterThan(0).WithMessage("Rank order must be greater than zero.");
                    });

                    RuleFor(a => a.RankedChoices)
                        .Must(choiceIds => choiceIds?.Select(c => c.ChoiceId).Distinct().Count() == choiceIds?.Count)
                        .WithMessage("Duplicate choice IDs are not allowed in RankedChoices.");
                });

            When(a => a.AnswerType == enQuestionType.Rating ||
                a.AnswerType == enQuestionType.Text,
                () =>
                {
                    RuleFor(a => a.AnswerValue)
                        .NotEmpty().WithMessage("Answer value is required for Rating and Text questions.")
                        .MaximumLength(500).WithMessage("Answer value cannot exceed 500 characters.");


                    RuleFor(a => a.RankedChoices)
                        .Empty().WithMessage("RankedChoices should be empty for Rating and Text questions.");
                });


        }
    }
}
