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

        // New implementation
        CreateMap<RegisterDTO, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

        CreateMap<User, UserResponseDto>().ForMember(dest => dest.Roles, opt => opt.Ignore());
    }
}
