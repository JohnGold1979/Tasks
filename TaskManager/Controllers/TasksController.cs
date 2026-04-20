using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Domain;
using TaskManager.DTOs.Task;
using TaskManager.Services.Task;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] int? departmentId,
            [FromQuery] TaskStatusEnum? status,
            [FromQuery] TaskPriorityEnum? priority)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var tasks = await _taskService.GetTasksAsync(userId, userRole, departmentId, status, priority);

            var result = tasks.Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CreatedByUsername = t.CreatedBy?.Username ?? string.Empty,
                AssignedToUsername = t.AssignedTo?.Username ?? string.Empty,
                DepartmentId = t.DepartmentId,
                DepartmentName = t.Department?.Name ?? string.Empty,
                CreatedAt = t.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            var result = new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                Priority = task.Priority.ToString(),
                CreatedByUsername = task.CreatedBy?.Username ?? string.Empty,
                AssignedToUsername = task.AssignedTo?.Username ?? string.Empty,
                DepartmentId = task.DepartmentId,
                DepartmentName = task.Department?.Name ?? string.Empty,
                CreatedAt = task.CreatedAt
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "TaskCreate")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var userId = int.Parse(userIdClaim.Value);

            try
            {
                var task = await _taskService.CreateTaskAsync(userId, dto);
                
                var result = new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status.ToString(),
                    Priority = task.Priority.ToString(),
                    CreatedByUsername = task.CreatedBy?.Username ?? string.Empty,
                    AssignedToUsername = task.AssignedTo?.Username ?? string.Empty,
                    DepartmentId = task.DepartmentId,
                    DepartmentName = task.Department?.Name ?? string.Empty,
                    CreatedAt = task.CreatedAt
                };

                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "TaskChangeStatus")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var success = await _taskService.UpdateStatusAsync(id, userId, userRole, dto.Status);
            if (!success) return Forbid();

            return NoContent();
        }

        [HttpPut("{id}/priority")]
        [Authorize(Policy = "TaskCreate")]
        public async Task<IActionResult> ChangePriority(int id, [FromBody] UpdatePriorityDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var success = await _taskService.UpdatePriorityAsync(id, userId, userRole, dto.Priority);
            if (!success) return Forbid();

            return NoContent();
        }

        [HttpPut("{id}/assign")]
        [Authorize(Policy = "TaskCreate")]
        public async Task<IActionResult> AssignTask(int id, [FromBody] AssignTaskDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var success = await _taskService.AssignTaskAsync(id, userId, userRole, dto.UserId);
            if (!success) return Forbid();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "TaskCreate")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var success = await _taskService.DeleteTaskAsync(id, userId, userRole);
            if (!success) return Forbid();

            return NoContent();
        }
    }
}