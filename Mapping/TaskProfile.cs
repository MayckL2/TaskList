using AutoMapper;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Mapping;

public class TaskProfile : Profile
{
    public TaskProfile()
    {
        CreateMap<TaskModel, ShowTaskDTO>();
        CreateMap<ShowTaskDTO, TaskModel>();
        CreateMap<CreateTaskDTO, TaskModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Done, opt => opt.Ignore())
            .ForMember(dest => dest.DateCreation, opt => opt.Ignore())
            .ForMember(dest => dest.DateEdition, opt => opt.Ignore());
    }
}
