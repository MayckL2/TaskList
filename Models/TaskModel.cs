using Microsoft.Extensions.WebEncoders.Testing;

namespace TaskList.Models;

public class TaskModel
{
    public TaskModel(string name, string description)
    {
        this.ID = 0;
        this.Name = name;
        this.Description = description;
        this.Done = false;
        this.DateCreation = DateTime.Now;
        this.DateEdition = DateTime.Now;

    }

    public int ID { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool Done { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime DateEdition { get; set; }

    // Edit the task name to a new name if have value
    public bool EditName(string newName)
    {
        if (newName != "")
        {
            this.Name = newName;
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
        return new TaskModel(this.Name!, this.Description!)
        {
            ID = this.ID,
            Name = this.Name,
            Description = this.Description,
            Done = this.Done,
            DateCreation = this.DateCreation,
            DateEdition = this.DateEdition
        };
    }

}
