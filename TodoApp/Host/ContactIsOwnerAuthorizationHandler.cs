using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Templates.TodoApp.Host
{
    public class ContactIsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement>
    {
        
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,OperationAuthorizationRequirement requirement
                                   )
        {
            if (context.User == null )
            {
                return Task.CompletedTask;
            }

            // If not asking for CRUD permission, return.

           

            return Task.CompletedTask;
        }
    }
}
