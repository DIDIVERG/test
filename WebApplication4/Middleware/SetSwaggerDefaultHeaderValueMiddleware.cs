using Microsoft.EntityFrameworkCore;
using WebApplication4.Database;
using WebApplication4.Services.JwtTokenGeneratorService;

namespace WebApplication4.Middleware;

public class SetSwaggerDefaultHeaderValueMiddleware : IMiddleware
{
    private readonly UserContext? _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public SetSwaggerDefaultHeaderValueMiddleware(IJwtTokenGenerator jwtTokenGenerator, UserContext? context)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _context = context;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
        {
           var user = await _context.Users.FirstOrDefaultAsync(i => i.Login == "Master");
           var _defaultToken = _jwtTokenGenerator.GenerateToken(user);
           context.Request.Headers["Authorization"] = $"Bearer {_defaultToken}";
        }
        next(context);
    }
}