using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication4.Database;
using WebApplication4.Database.Models;
using WebApplication4.DTOs;
// 
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
            IMapper mapper )
        {
            _configuration = configuration;
            _userContext = userContext;
            _mapper = mapper;
        }

        [HttpPost("Create")]
        public async Task<ActionResult<User>> Register([FromBody] UserDto request, string name, int gender,
            DateTime birthday, bool isAdmin)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (user != null)
            {
                return BadRequest($"Пользователь с логином {request.Login} уже существует");
            }
            var newUser = _mapper.Map<User>(request);
            newUser.Birthday = birthday;
            newUser.Gender = gender;
            newUser.IsAdmin = isAdmin;
            newUser.Name = name;
            newUser.CreatedOn = DateTime.UtcNow;
            newUser.CreatedBy = request.Login;
            await  _userContext.Users.AddAsync(newUser);
            await _userContext.SaveChangesAsync();
            return Ok(newUser);
        }

        [HttpPut("Update1")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> UpdateName(UserDto request,string name)
        {
            
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }

            user.Name = name;
            user.ModifiedBy = request.Login;
            user.ModifiedOn = DateTime.UtcNow;
            _userContext.Users.Update(user);
            await _userContext.SaveChangesAsync();
            return Ok(user);
        }
        // проверить на сработает ли регекс когда обновляю пароль
        [HttpPut("Update2")]
        public async Task<ActionResult<User>> UpdatePassword(UserDto request, string password)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }
            user.Password = password;
            user.ModifiedBy = request.Login;
            user.ModifiedOn = DateTime.UtcNow;
            _userContext.Users.Update(user);
            await _userContext.SaveChangesAsync();
            return Ok(user);
        }

        
        [HttpPut("Update3")]
        public async Task<ActionResult<User>> UpdateLogin(UserDto request,string login)
        {
            if (await _userContext.Users.AnyAsync(i => i.Login == login))
            {
                return BadRequest($"Пользователь с логином {login} уже существует");
            }
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }
            user.Login = login;
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
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }
            return await _userContext.Users.Where(i => i.RevokedOn == default)
                .OrderBy(i => i.CreatedOn).ToListAsync();
        }
        
        [HttpGet("Get2")]
        public async Task<ActionResult<User>> GetUserByLogin(UserDto request, string login)
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == request.Login);
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }

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
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }
            var userToDelete = await _userContext.Users.FirstOrDefaultAsync(i => i.Login == login);
            if (userToDelete == null)
            {
                return BadRequest($"Пользователь с логином {login} не существует");
            }
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
            if (user == null)
            {
                return BadRequest($"Пользователь с логином {request.Login} не существует");
            }
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
