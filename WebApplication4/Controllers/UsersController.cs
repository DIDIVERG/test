using System;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Database;
using WebApplication4.Database.Models;
using WebApplication4.DTOs;
using WebApplication4.Models;
using WebApplication4.Services.ValidationService;

namespace WebApplication4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;
        private readonly Validator _validator;
        public UsersController(IConfiguration configuration, UserContext userContext, 
            IMapper mapper, Validator validator)
        {
            _configuration = configuration;
            _userContext = userContext;
            _mapper = mapper;
            _validator = validator;
        }

        private async Task<ActionResult> Validate(SenderCredentials sender, UserDto request)
        {
            if (await _validator.isUserExist(request.Login) == true)
            {
                return BadRequest($"Пользователь с ником {request.Login} уже существует");
            }
            if (await _validator.isUserExist(sender.SenderLogin) == false)
            {
                return BadRequest($"Пользователя с ником {sender.SenderLogin} не существует");
            }
            return Ok();
        }
        [HttpPost("Create")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<ActionResult<User>> Register
            ([FromBody] SenderCredentials sender, [FromQuery] UserDto request)
        {
            if (await _validator.isUserExist(request.Login) == true)
            {
                return BadRequest($"User {request.Login} alreay exist");
            }
            if (await _validator.isUserExist(sender.SenderLogin) == false)
            {
                return BadRequest($"User {sender.SenderLogin} doesen't exist");
            }
            var newUser = _mapper.Map<User>(request);
            newUser.Birthday = request.Birthday;
            newUser.Gender = request.Gender;
            newUser.IsAdmin = request.IsAdmin;
            newUser.Name = request.Name;
            newUser.CreatedOn = DateTime.UtcNow;
            newUser.CreatedBy = sender.SenderLogin;
            /*await  _userContext.Users.AddAsync(newUser);
            await _userContext.SaveChangesAsync();*/
            return Ok(newUser);
        }
        
        [HttpPatch("Update1")]
        [Authorize(Policy = "All")]
        public async Task<ActionResult<User>> UpdatePassword(UserDto request, string name, string password, string login)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            
            user.Password = password;
            user.ModifiedBy = request.Login;
            user.ModifiedOn = DateTime.UtcNow;
            _userContext.Users.Update(user);
            await _userContext.SaveChangesAsync();
            return Ok(user);
        }
        
        [HttpGet("Get1")]
        public async Task<ActionResult<List<User>>> GetActiveUsersSortedByCreatedDate(UserDto request)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
          
            return await _userContext.Users.Where(i => i.RevokedOn == default)
                .OrderBy(i => i.CreatedOn).ToListAsync();
        }
        
        [HttpGet("Get2")]
        public async Task<ActionResult<User>> GetUserByLogin(UserDto request, string login)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            var userInfo = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
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
        public async Task<ActionResult<User>> GetUserByLoginAndPassword(UserDto request, string login, string password)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }
            var userInfo = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            if (userInfo == null || userInfo.RevokedOn == default)
            {
                return BadRequest($"Пользователь с логином {login} не существует");
            }
            return Ok(userInfo);
        }
        
        [HttpGet("Get4")]
        public async Task<ActionResult<List<User>>> GetUserOlderAge(UserDto request,int targetAge)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }
            var result = _userContext.Users.Where(u 
                => u.Birthday.HasValue && DateTime.Today.Year - u.Birthday.Value.Year > targetAge).ToList();
            return Ok(result);
        }
        
        [HttpDelete("Delete1")]
        public async Task<ActionResult<User>> DeleteUserByLogin(UserDto request,string login)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            var userToDelete = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            userToDelete.RevokedOn = DateTime.UtcNow;
            userToDelete.RevokedBy = request.Login;
            _userContext.Update(userToDelete);
            await _userContext.SaveChangesAsync();
            return Ok(userToDelete);
        }
        
        [HttpPut("Recovery")]
        public async Task<ActionResult<User>> Recover(UserDto request,string login)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            var userToRecover = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            userToRecover.RevokedOn = default;
            userToRecover.RevokedBy = "";
            _userContext.Update(userToRecover);
            await _userContext.SaveChangesAsync();
            return Ok(userToRecover);
        }
    }
    
}
