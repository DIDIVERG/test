using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models;

public class SenderCredentials
{
    [RegularExpression("^[a-zA-Z0-9]+$", 
        ErrorMessage = "Login have to contain only latin symbols or numerics")]
    public string SenderLogin { get; set; }
    
    [RegularExpression("^[a-zA-Z0-9]+$", 
        ErrorMessage = "Password have to contain only latin symbols or numerics")]
    public string SenderPassword { get; set; }
}