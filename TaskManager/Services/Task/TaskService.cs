using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;
using TaskManager.DTOs.Task;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Services.Task
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetTasksAsync(int userId, string userRole, int? departmentId, TaskStatusEnum? status, TaskPriorityEnum? priority);
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(int currentUserId, CreateTaskDto dto);
        Task<bool> UpdateStatusAsync(int taskId, int currentUserId, string currentUserRole, TaskStatusEnum newStatus);
        Task<bool> UpdatePriorityAsync(int taskId, int currentUserId, string currentUserRole, TaskPriorityEnum newPriority);
        Task<bool> AssignTaskAsync(int taskId, int currentUserId, string currentUserRole, int newAssigneeId);
        Task<bool> DeleteTaskAsync(int taskId, int currentUserId, string currentUserRole);
    }

    public class TaskService : ITaskService
    {
        private readonly IRepository<TaskItem> _taskRepo;
        private readonly IRepository<User> _userRepo;

        public TaskService(IRepository<TaskItem> taskRepo, IRepository<User> userRepo)
        {
            _taskRepo = taskRepo;
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksAsync(int userId, string userRole, int? departmentId, TaskStatusEnum? status, TaskPriorityEnum? priority)
        {
            var query = _taskRepo.GetAll();

            // Применяем фильтрацию по правам
            if (userRole == "Employee")
            {
                query = query.Where(t => t.AssignedToId == userId);
            }
            else if (userRole == "Chief" || userRole == "Observer")
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user != null)
                {
                    int deptId = departmentId ?? user.DepartmentId;
                    query = query.Where(t => t.DepartmentId == deptId);
                }
                else
                    return new List<TaskItem>();
            }

            if (status.HasValue) query = query.Where(t => t.Status == status.Value);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);

            // Разделяем Include и выполнение запроса
            var resultList = await query
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Department)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return resultList;
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepo.GetAll()
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.Id == id);

            return task;
        }

        public async Task<TaskItem> CreateTaskAsync(int currentUserId, CreateTaskDto dto)
        {
            var currentUser = await _userRepo.GetByIdAsync(currentUserId);
            if (currentUser == null) throw new Exception("User not found");

            var assignee = await _userRepo.GetByIdAsync(dto.AssignedToId);
            if (assignee == null) throw new Exception("Assignee not found");

            // Загружаем роль пользователя
            await _userRepo.GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            var currentUserRole = currentUser.Role?.Name ?? string.Empty;

            if (currentUserRole == "Employee" && dto.AssignedToId != currentUserId)
                throw new UnauthorizedAccessException("Employee can only assign task to themselves");

            if (currentUserRole == "Chief" && assignee.DepartmentId != currentUser.DepartmentId)
                throw new UnauthorizedAccessException("Chief can assign only within own department");

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                CreatedById = currentUserId,
                AssignedToId = dto.AssignedToId,
                DepartmentId = assignee.DepartmentId,
                Priority = dto.Priority,
                Status = TaskStatusEnum.New,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _taskRepo.AddAsync(task);
            await _taskRepo.SaveChangesAsync();
            return task;
        }

        public async Task<bool> UpdateStatusAsync(int taskId, int currentUserId, string currentUserRole, TaskStatusEnum newStatus)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return false;

            if (currentUserRole == "Chief")
            {
                task.Status = newStatus;
                task.UpdatedAt = DateTime.UtcNow;
                _taskRepo.Update(task);
                await _taskRepo.SaveChangesAsync();
                return true;
            }
            else if (currentUserRole == "Employee" && task.AssignedToId == currentUserId)
            {
                task.Status = newStatus;
                task.UpdatedAt = DateTime.UtcNow;
                _taskRepo.Update(task);
                await _taskRepo.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> UpdatePriorityAsync(int taskId, int currentUserId, string currentUserRole, TaskPriorityEnum newPriority)
        {
            if (currentUserRole != "Chief") return false;

            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return false;

            task.Priority = newPriority;
            task.UpdatedAt = DateTime.UtcNow;
            _taskRepo.Update(task);
            await _taskRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTaskAsync(int taskId, int currentUserId, string currentUserRole, int newAssigneeId)
        {
            if (currentUserRole != "Chief") return false;

            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return false;

            var newAssignee = await _userRepo.GetByIdAsync(newAssigneeId);
            if (newAssignee == null) return false;

            var currentUser = await _userRepo.GetByIdAsync(currentUserId);
            if (currentUser != null && newAssignee.DepartmentId != currentUser.DepartmentId)
                return false;

            task.AssignedToId = newAssigneeId;
            task.DepartmentId = newAssignee.DepartmentId;
            task.UpdatedAt = DateTime.UtcNow;
            _taskRepo.Update(task);
            await _taskRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int currentUserId, string currentUserRole)
        {
            if (currentUserRole != "Chief") return false;

            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return false;

            _taskRepo.Delete(task);
            await _taskRepo.SaveChangesAsync();
            return true;
        }
    }
}