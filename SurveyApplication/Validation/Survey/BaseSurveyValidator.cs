using FluentValidation;
using SurveyBusinessLayer.DTOs;
using Repository.Models;

namespace SurveyApplication.Validation.Survey
{
    public abstract class BaseSurveyValidator<T> : AbstractValidator<T> where T : ISurveyBaseDto
    {
        public BaseSurveyValidator() 
        {
            RuleFor(s => s.Title)
            .NotEmpty().WithMessage("Survey title is required.")
            .MaximumLength(200).WithMessage("Survey title cannot exceed 200 characters.");

            RuleFor(s => s.userId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(s => s.Status)
                .NotEmpty().WithMessage("Survey status is required.")
                .Must((status => status == "Draft" || status == "Published" || status == "Archived"))
                .WithMessage("Survey status must be either 'Draft' or 'Published' or 'Archived'.");
        }
    }
    public abstract class BaseQuestionValidator<T> : AbstractValidator<T> where T : IQuestionBaseDto
    {
        public BaseQuestionValidator()
        {
            RuleFor(q => q.QuestionText)
                .NotEmpty().WithMessage("Question text is required.")
                .MaximumLength(500).WithMessage("Question text cannot exceed 500 characters.");

            var QuestionTypes = Enum.GetNames<enQuestionType>();
            
            RuleFor(q => q.QuestionType)
                .NotEmpty().WithMessage("Question type is required.")
                .Must(type => QuestionTypes.Contains(type))
                .WithMessage($"Question type must be either {string.Join(", ", QuestionTypes)} .");
        }
    }
}
