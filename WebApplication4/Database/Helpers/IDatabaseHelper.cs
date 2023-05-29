namespace WebApplication4.Database.Helpers;

public interface IDatabaseHelper
{
    public Task EnsureDatabaseCreated(UserContext context); 
    public Task ReCreateDatabase(UserContext context); 
    
}