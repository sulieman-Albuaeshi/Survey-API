using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SurveyApplication.Authorization
{
    public class EditDeleteResuorseHandler : AuthorizationHandler<EditDeleteResuorseRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditDeleteResuorseRequirement requirement, string resuorseUserId)
        {
            if(context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == resuorseUserId)
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }
}
