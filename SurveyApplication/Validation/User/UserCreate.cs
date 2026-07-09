using FluentValidation;
using SurveyBusinessLayer.DTOs;

namespace SurveyApplication.Validation.User
{
    public class UserCreate : BaseUserValidation<CreateUserDto>
    {
        public UserCreate() : base()
        {
            // All validation rules are inherited from BaseUserValidation
        }
    }
}
