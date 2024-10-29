using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakingRoses.Models;

namespace SpeakingRoses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    public class TasksController : ControllerBase
    {
        private readonly TaskContext _taskContext;

        public TasksController(TaskContext taskContext)
        {
            _taskContext = taskContext;
        }

        // Get
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpeakingRoses.Models.Task>>> GetTasks()
        {
            return await _taskContext.Tasks.ToListAsync();
        }

        // Get by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<SpeakingRoses.Models.Task>> GetTask(int id)
        {
            var task = await _taskContext.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }
            return task;
        }

        // Create task
        [HttpPost]
        public async Task<ActionResult<SpeakingRoses.Models.Task>> CreateTask([FromBody] SpeakingRoses.Models.Task task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _taskContext.Tasks.Add(task);
            await _taskContext.SaveChangesAsync();

            // Test comment

            return CreatedAtAction("GetTask", new { id = task.Id }, task);
        }

        // Update task
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] SpeakingRoses.Models.Task updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return BadRequest();
            }

            var existingTask = await _taskContext.Tasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.Status = updatedTask.Status;

            try
            {
                await _taskContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the task.");
            }

            return NoContent();
        }


        // Delete task
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _taskContext.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _taskContext.Tasks.Remove(task);
            await _taskContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
