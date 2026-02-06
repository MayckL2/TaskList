using System.ComponentModel.DataAnnotations;

namespace TaskList.Entity;

public class TaskEntity
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool Done { get; set; } = false;
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime DateEdition { get; set; } = DateTime.Now;
    
}
