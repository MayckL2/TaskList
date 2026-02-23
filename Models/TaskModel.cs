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
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool Done { get; set; } = false;
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime DateEdition { get; set; } = DateTime.Now;

    // Edit the task Title to a new Title if have value
    public bool EditTitle(string newTitle)
    {
        if (newTitle != "")
        {
            this.Title = newTitle;
            this.DateEdition = DateTime.Now;
            return true;
        }
        else
        {
            return false;
        }
    }

    // Edit the task description to a new description if have value
    public bool EditDescription(string newDescription)
    {
        if (newDescription != "")
        {
            this.Description = newDescription;
            this.DateEdition = DateTime.Now;
            return true;
        }
        else
        {
            return false;
        }
    }

    // toggle done status for true or false
    public void ToggleDone(bool done)
    {
        this.Done = done;
    }

    internal bool GetTask(int v)
    {
        throw new NotImplementedException();
    }

    // Return task data
    public TaskModel GetTask()
    {
        return new TaskModel(this.Title!, this.Description!)
        {
            Id = this.Id,
            Title = this.Title,
            Description = this.Description,
            Done = this.Done,
            DateCreation = this.DateCreation,
            DateEdition = this.DateEdition,
        };
    }
}
