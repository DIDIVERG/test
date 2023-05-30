using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using WebApplication4.Database;
using WebApplication4.Models;

namespace WebApplication4.Attributes;

public class UserNotExistAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.TryGetValue("sender", out var argument) && argument is SenderCredentials sender)
        {
            var userContext = context.HttpContext.RequestServices.GetService(typeof(UserContext)) as UserContext;
            var existingUser = await userContext.Users.FirstOrDefaultAsync(i => i.Login == sender.SenderLogin);
            if (existingUser == null)
            {
                context.Result = new BadRequestObjectResult($"User {sender.SenderLogin} not exists.");
                return;
            }
        }
        await next();
    }
}