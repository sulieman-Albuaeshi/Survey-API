using FluentValidation;
using SurveyBusinessLayer.DTOs;

namespace SurveyApplication.Validation.Survey
{
    public class SurveyStatuse : AbstractValidator<SurveyStatusDto>
    {
        public SurveyStatuse() 
        {
            RuleFor(s => s.StatusText)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => s == "Draft" || s == "Published" || s == "Archived")
                .WithMessage("Status must be either 'Draft', 'Published', or 'Archived'.");
        }
    }
}
