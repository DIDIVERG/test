using System;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Attributes;
using WebApplication4.Database;
using WebApplication4.Database.Models;
using WebApplication4.DTOs;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;
        public UsersController(IConfiguration configuration, UserContext userContext, 
            IMapper mapper)
        {
            _configuration = configuration;
            _userContext = userContext;
            _mapper = mapper;
        }
        
        [HttpPost("Create")]
        [Authorize(Policy = "OnlyAdmin")]
        [UserExists]
        [UserNotExist]
        public async Task<ActionResult<User>> Register
            ([FromBody] SenderCredentials sender, [FromQuery] UserDto request)
        {
            var newUser = _mapper.Map<User>(request);
            var birthday = request.Birthday ?? default;
            newUser.Birthday = birthday.ToUniversalTime();
            newUser.Gender = request.Gender;
            newUser.IsAdmin = request.IsAdmin;
            newUser.Name = request.Name;
            newUser.CreatedOn = DateTime.UtcNow;
            newUser.CreatedBy = sender.SenderLogin;
            newUser.ModifiedOn = newUser.ModifiedOn.ToUniversalTime();
            newUser.RevokedOn = newUser.RevokedOn.ToUniversalTime();
            await  _userContext.Users.AddAsync(newUser);
            await _userContext.SaveChangesAsync();
            return Ok(newUser);
        }
        
        [HttpPatch("Update1")]
        [Authorize(Policy = "All")]
        [UserNotExist]
        public async Task<ActionResult<User>> UpdatePassword(SenderCredentials sender ,
            string? loginToChange,string? name, string? password, string login)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            if (user is null)
            {
                return BadRequest($"User {login} doesn't exist");
            }
            if (name != null)
            {
                user.Name = name;
            }
            if (password != null)
            {
                user.Password = password;
            }

            if (loginToChange != null && !_userContext.Users.Any(i => i.Login == loginToChange))
            {
                user.Login = loginToChange;
            }
            user.ModifiedBy = sender.SenderLogin;
            user.ModifiedOn = DateTime.UtcNow;
            _userContext.Users.Update(user);
            await _userContext.SaveChangesAsync();
            return Ok(user);
        }
        
        [HttpGet("Get1")]
        [Authorize(Policy = "OnlyAdmin")]
        [UserNotExist]
        public async Task<ActionResult<List<User>>> GetActiveUsersSortedByCreatedDate
            ([FromQuery] SenderCredentials sender)
        {
            return await _userContext.Users.Where(i => i.RevokedOn == default)
                .OrderBy(i => i.CreatedOn).ToListAsync();
        }
        
        [HttpGet("Get2")]
        [UserNotExist]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<ActionResult<User>> GetUserByLogin([FromQuery]SenderCredentials sender, string login)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == sender.SenderLogin);
            var userInfo = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            if (userInfo == null)
            {
                return BadRequest($"Пользователь с логином {login} не существует");
            }
            var result = new
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                RevokedOn = user.RevokedOn
            };
            return Ok(result);
        }
        
        [HttpGet("Get3")]
        [UserNotExist]
        [Authorize(Policy = "OnlyUser")]
        public async Task<ActionResult<User>> GetUserByLoginAndPassword([FromQuery] SenderCredentials sender, 
            string login, string password)
        {
            var userInfo = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            if (userInfo == null || userInfo.RevokedOn != default)
            {
                return BadRequest($"Пользователь с логином {login} не существует");
            }
            return Ok(userInfo);
        }
        
        [HttpGet("Get4")]
        [UserNotExist]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<ActionResult<List<User>>> GetUserOlderAge([FromQuery] SenderCredentials sender,int targetAge)
        {
            var result = _userContext.Users.Where(u 
                => u.Birthday.HasValue && DateTime.Today.Year - u.Birthday.Value.Year > targetAge).ToListAsync();
            return Ok(result);
        }
        
        [HttpDelete("Delete1")]
        [UserNotExist]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<ActionResult<User>> DeleteUserByLogin(SenderCredentials sender,string login)
        {
            var userToDelete = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            if (userToDelete == null)
            {
                return BadRequest($"Пользователь с логином {login} не существует");
            }
            userToDelete.RevokedOn = DateTime.UtcNow;
            userToDelete.RevokedBy = sender.SenderLogin;
            _userContext.Update(userToDelete);
            await _userContext.SaveChangesAsync();
            return Ok(userToDelete);
        }
        
        [HttpPatch("Recovery")]
        [UserNotExist]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<ActionResult<User>> Recover(SenderCredentials sender,string login)
        {
            var userToRecover = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            if (userToRecover == null)
            {
                return BadRequest($"Пользователь с логином {login} не существует");
            }
            userToRecover.RevokedOn = default;
            userToRecover.RevokedBy = "";
            _userContext.Update(userToRecover);
            await _userContext.SaveChangesAsync();
            return Ok(userToRecover);
        }
    }
}
