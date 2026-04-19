using TaskManager.Domain;

namespace TaskManager.DTOs.Task
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string CreatedByUsername { get; set; } = string.Empty;
        public string AssignedToUsername { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AssignedToId { get; set; }
        public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.Medium;
    }

    public class UpdateStatusDto
    {
        public TaskStatusEnum Status { get; set; }
    }

    public class UpdatePriorityDto
    {
        public TaskPriorityEnum Priority { get; set; }
    }

    public class AssignTaskDto
    {
        public int UserId { get; set; }
    }
}