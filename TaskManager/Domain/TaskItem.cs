using System;

namespace TaskManager.Domain
{
    public enum TaskStatusEnum { New, InProgress, Done }
    public enum TaskPriorityEnum { Low, Medium, High }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.New;
        public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.Medium;
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
        public int AssignedToId { get; set; }
        public User AssignedTo { get; set; } = null!;
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}