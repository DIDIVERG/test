using Microsoft.EntityFrameworkCore;
using WebApplication4.Database;
using WebApplication4.DTOs;

namespace WebApplication4.Services.ValidationService;

public class Validator
{

    private readonly UserContext _userContext;

    public Validator(UserContext userContext)
    {
        _userContext = userContext;
    }

    public async Task<bool> isUserExist(string login)
    {
        var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
        if (user == null)
        {
            return false;
        }
        return true;
    }
    
}