using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskList.Models;

namespace TaskList.Contexts;

public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options)
        : base(options) { }

    public DbSet<TaskModel> Tasks { get; set; }

    // specify the id as the primary key in the Model
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskModel>().HasKey(t => t.Id);
    }
}
