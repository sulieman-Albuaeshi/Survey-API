using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SurveyBusinessLayer.DTOs;
namespace SurveyApplication.Validation.User
{
    public class BaseUserValidation<T> : AbstractValidator<T>  where T : IUserBaseDto
    {
        public BaseUserValidation() : base()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => role == "Respondent" || role == "Admin" || role == "Creator" )
                .WithMessage("Role must be either 'User' or 'Admin' or 'Respondent'.");
        }
    }
}
