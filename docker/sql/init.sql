IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TaskListDb')
BEGIN
    CREATE DATABASE TaskListDb;
END;
GO