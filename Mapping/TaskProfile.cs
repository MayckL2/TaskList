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
    }
}
