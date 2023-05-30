using System.ComponentModel.DataAnnotations;

namespace WebApplication4.DTOs;

public class UserDto
{
    
    [RegularExpression("^[a-zA-Z0-9]+$", 
        ErrorMessage = "Login have to contain only latin symbols or numerics")]
    public string Login { get; set; }
    
    [RegularExpression("^[a-zA-Z0-9]+$", 
        ErrorMessage = "Password have to contain only latin symbols or numerics")]
    public string Password { get; set; }
    public string Name { get; set; }
    [Range(0,2)]
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsAdmin { get; set; }
    
}