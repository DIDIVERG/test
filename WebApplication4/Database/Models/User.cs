using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace WebApplication4.Database.Models;

public class User
{
    [Key]
    public Guid Guid { get; set; } = Guid.NewGuid();
    [RegularExpression("^[a-zA-Z0-9]+$", 
        ErrorMessage = "Login have to contain only latin symbols or numerics")]
    public string Login { get; set; }
    [RegularExpression("^[a-zA-Z0-9]+$", 
        ErrorMessage = "Password have to contain only latin symbols or numerics")]
    public string Password { get; set; }
    [RegularExpression("^[a-zA-Zа-яА-Я]+$", 
        ErrorMessage = "Name have to contain only cyrillic or latin symbols")]
    public string Name { get; set; }
    [Range(0,2)]
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime  CreatedOn { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime  ModifiedOn { get; set; }
    public string ModifiedBy { get; set; } = "";
    public DateTime  RevokedOn { get; set; }
    public string RevokedBy { get; set; } = "";
}