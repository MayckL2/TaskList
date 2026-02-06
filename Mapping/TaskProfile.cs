using AutoMapper;
using TaskList.Entity;
using TaskList.DTOs;

namespace TaskList.Mapping;

public class TaskProfile : Profile
{
    public TaskProfile()
    {
        CreateMap<TaskEntity, TaskDTO>();
    }
    
}
