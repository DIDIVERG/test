using AutoMapper;
using WebApplication4.Database.Models;
using WebApplication4.DTOs;

namespace WebApplication4.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
    }
}