namespace TaskList.Models;

public class Report
{
    public DateTime Date { get; set; }
    public required string Data { get; set; }
}

public class DataRepost
{
    public DateTime Date { get; set; }
    public int CreatedTasks { get; set; }
    public int DoneTasks { get; set; }

    public int TotalTasks { get; set; }
}
