using FluentValidation;
using SurveyBusinessLayer.DTOs;

namespace SurveyApplication.Validation.User
{
    public class UserUpdate : BaseUserValidation<UpdateUserDto>
    {
        public UserUpdate() : base()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User ID is required.");

        }
    }
}
