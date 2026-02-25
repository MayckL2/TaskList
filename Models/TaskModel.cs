using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskList.Models;

public class TaskModel
{
    public TaskModel(string title, string description)
    {
        this.Title = title;
        this.Description = description;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required!")]
    [StringLength(
        200,
        MinimumLength = 3,
        ErrorMessage = "The title should be between 3 and 200 characters."
    )]
    public string Title { get; set; }

    [StringLength(1000, ErrorMessage = "The description must be a maximum of 1000 characters.")]
    public string Description { get; set; }

    [Required]
    public bool Done { get; set; } = false;

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Creation Date")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Edition Date")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime DateEdition { get; set; } = DateTime.Now;
}
