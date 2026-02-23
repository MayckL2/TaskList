namespace TaskList.DTOs;

public class ShowTaskDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool Done { get; set; } = false;
    public DateTime DateCreation { get; set; }
    public DateTime DateEdition { get; set; }
}

public class CreateTaskDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class UpdateTaskDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}
