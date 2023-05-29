using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Database.Models;

namespace WebApplication4.Database.Helpers;

public class DatabaseHelper : IDatabaseHelper
{
    private async Task FillInitialData(UserContext context)
    {
        var user = new User()
        {
            Guid = Guid.NewGuid(),
            Login = "Master",
            Password = "MasterPass",
            Name = "None",
            Gender = 2,
            Birthday = DateTime.UtcNow,
            IsAdmin = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = nameof(DatabaseHelper),
        };
       await context.Users.AddAsync(user);
       await context.SaveChangesAsync();
    }
    
    public  async Task EnsureDatabaseCreated(UserContext context)
    {
        if (await context.Database.EnsureCreatedAsync())
        {
            await FillInitialData(context);
        }
    }

    public async Task ReCreateDatabase(UserContext context)
    {
        if (await context.Database.EnsureDeletedAsync())
        {
            await EnsureDatabaseCreated(context);
        }
    }
    
}