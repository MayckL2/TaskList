using System.ComponentModel.DataAnnotations;

namespace TaskList.DTOs;

public class ShowTaskDTO
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public bool Done { get; set; } = false;
    public DateTime DateCreation { get; set; }
    public DateTime DateEdition { get; set; }
}

public class CreateTaskDTO
{
    [Required(ErrorMessage = "Title is required...")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Title must be between 3 and 100 characteres"
    )]
    public required string Title { get; set; }

    [Required(ErrorMessage = "Title is required...")]
    [StringLength(
        255,
        MinimumLength = 10,
        ErrorMessage = "Title must be between 10 and 255 characteres"
    )]
    public required string Description { get; set; }

    // public DateTime DateCreation { get; set; } = DateTime.Now;
    // public DateTime DateEdition { get; set; } = DateTime.Now;
}

public class UpdateTaskDTO
{
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "Title must be between 3 and 100 characteres"
    )]
    public required string Title { get; set; }

    [StringLength(
        255,
        MinimumLength = 10,
        ErrorMessage = "Title must be between 10 and 255 characteres"
    )]
    public required string Description { get; set; }
}
