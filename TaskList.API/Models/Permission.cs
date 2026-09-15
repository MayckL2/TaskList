namespace TaskList.Models;

public static class Permissions
{
    // User permissions
    public const string UsersView = "users:view";
    public const string UsersCreate = "users:create";
    public const string UsersEdit = "users:edit";
    public const string UsersDelete = "users:delete";

    // Task permissions
    public const string TasksView = "tasks:view";
    public const string TasksCreate = "tasks:create";
    public const string TasksEdit = "tasks:edit";
    public const string TasksDelete = "tasks:delete";
    public const string TasksAssign = "tasks:assign";

    // Report permissions
    public const string ReportsView = "reports:view";
    public const string ReportsExport = "reports:export";
    public const string ReportsSchedule = "reports:schedule";

    // Admin permissions
    public const string AdminAccess = "admin:access";
    public const string AuditView = "audit:view";
    public const string SystemConfig = "system:config";

    // Group all permissions for easy reference
    public static readonly IReadOnlyList<string> All = new[]
    {
        UsersView,
        UsersCreate,
        UsersEdit,
        UsersDelete,
        TasksView,
        TasksCreate,
        TasksEdit,
        TasksDelete,
        TasksAssign,
        ReportsView,
        ReportsExport,
        ReportsSchedule,
        AdminAccess,
        AuditView,
        SystemConfig,
    };

    // Group permissions by module
    public static class User
    {
        public static readonly string[] All = { UsersView, UsersCreate, UsersEdit, UsersDelete };
    }

    public static class Task
    {
        public static readonly string[] All =
        {
            TasksView,
            TasksCreate,
            TasksEdit,
            TasksDelete,
            TasksAssign,
        };
    }

    public static class Report
    {
        public static readonly string[] All = { ReportsView, ReportsExport, ReportsSchedule };
    }
}
