namespace TaskList.Models;

public class TaskModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public bool Done { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime DateEdition { get; set; }
}
