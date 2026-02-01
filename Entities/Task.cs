using System.ComponentModel.DataAnnotations;

namespace TaskList.Entities;

public class TaskEntitie
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }
    public bool Done { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime DateEdition { get; set; }
    
}
