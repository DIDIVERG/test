using System.Drawing.Text;
using WebApplication4.Database;
using WebApplication4.Database.Helpers;

namespace WebApplication4.Middleware;

public class EnsureDatabaseCreatedMiddleware : IMiddleware
{
    private readonly IDatabaseHelper _helper;
    private readonly UserContext? _context;
    public EnsureDatabaseCreatedMiddleware(IDatabaseHelper helper, IServiceProvider provider)
    {
        _helper = helper;
        _context = provider.GetService<UserContext>();
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_context != null)
        {
          await  _helper.ReCreateDatabase(_context);
        }
        await next(context);
    }
}