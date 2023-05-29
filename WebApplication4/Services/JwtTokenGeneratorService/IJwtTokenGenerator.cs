using WebApplication4.Database.Models;
using WebApplication4.DTOs;

namespace WebApplication4.Services.JwtTokenGeneratorService;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}