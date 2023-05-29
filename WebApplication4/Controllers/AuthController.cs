using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Database;
using WebApplication4.Database.Helpers;
using WebApplication4.Services.JwtTokenGeneratorService;

namespace WebApplication4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _userContext;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        public AuthController(UserContext userContext, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userContext = userContext;
            _jwtTokenGenerator = jwtTokenGenerator;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(string username, string password)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == username && i.Password == password);
            if (user is null)
            {
                return BadRequest("Uset doesen't exist");
            }
            return _jwtTokenGenerator.GenerateToken(user);
        }
        
    }
}