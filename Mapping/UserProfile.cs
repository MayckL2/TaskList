using AutoMapper;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<AuthUser, User>() // DTO to User
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        CreateMap<User, AuthUser>(); // User to DTO
    }
}
