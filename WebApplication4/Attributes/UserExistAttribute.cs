using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Database;
using WebApplication4.DTOs;

namespace WebApplication4.Attributes;

public class UserExistsAttribute :Attribute, IAsyncActionFilter
{
  
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.TryGetValue("request", out var argument) && argument is UserDto request)
        {
            var userContext = context.HttpContext.RequestServices.GetService(typeof(UserContext)) as UserContext;
            var existingUser = await userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (existingUser != null)
            {
                context.Result = new BadRequestObjectResult($"User {request.Login} already exists.");
                return;
            }
        }
        await next();
    }
}