using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakingRoses.Controllers;
using SpeakingRoses.Models;
using Task = SpeakingRoses.Models.Task;

namespace Tests
{
    public class TasksControllerTests
    {
        private DbContextOptions<TaskContext> _options;

        public TasksControllerTests()
        {
            _options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private TaskContext GetContext()
        {
            var context = new TaskContext(_options);
            context.Database.EnsureCreated();
            return context;
        }

        private void SeedDatabase(TaskContext context)
        {
            if (!context.Tasks.Any())
            {
                context.Tasks.AddRange(
                    new Task { Id = 1, Title = "Task 1", Description = "Description 1", Priority = Priority.Low, DueDate = DateTime.Now, Status = Status.Pending },
                    new Task { Id = 2, Title = "Task 2", Description = "Description 2", Priority = Priority.Medium, DueDate = DateTime.Now, Status = Status.Completed }
                );
                context.SaveChanges();
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTasks_ReturnsAllTasks()
        {
            using (var context = GetContext())
            {
                SeedDatabase(context);
                var controller = new TasksController(context);

                var result = await controller.GetTasks();

                var actionResult = Assert.IsType<ActionResult<IEnumerable<Task>>>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<Task>>(actionResult.Value);
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTask_ReturnsTaskById()
        {
            using (var context = GetContext())
            {
                SeedDatabase(context);
                var controller = new TasksController(context);

                var result = await controller.GetTask(1);

                var actionResult = Assert.IsType<ActionResult<Task>>(result);
                var model = Assert.IsAssignableFrom<Task>(actionResult.Value);
                Assert.Equal(1, model.Id);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_ReturnsCreatedTask()
        {
            using (var context = GetContext())
            {
                var controller = new TasksController(context);
                var task = new Task
                {
                    Title = "Task 3",
                    Description = "Description 3",
                    Priority = Priority.High,
                    DueDate = DateTime.Now,
                    Status = Status.Pending
                };

                var result = await controller.CreateTask(task);

                var actionResult = Assert.IsType<ActionResult<Task>>(result);
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var model = Assert.IsAssignableFrom<Task>(createdAtActionResult.Value);
                Assert.Equal(task.Title, model.Title);
                Assert.Equal(task.Description, model.Description);
                Assert.Equal(task.Priority, model.Priority);
                Assert.Equal(task.DueDate, model.DueDate);
                Assert.Equal(task.Status, model.Status);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_ReturnsNoContent()
        {
            using (var context = GetContext())
            {
                SeedDatabase(context);
                var controller = new TasksController(context);
                var task = new Task { Id = 1, Title = "Updated Task 1", Description = "Updated Description 1", Priority = Priority.Low, DueDate = DateTime.Now, Status = Status.Pending };

                var existingTask = await context.Tasks.FindAsync(1);
                context.Entry(existingTask).State = EntityState.Detached;

                var result = await controller.UpdateTask(1, task);

                Assert.IsType<NoContentResult>(result);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_ReturnsNoContent()
        {
            using (var context = GetContext())
            {
                SeedDatabase(context);
                var controller = new TasksController(context);

                var result = await controller.DeleteTask(1);

                Assert.IsType<NoContentResult>(result);
            }
        }
    }
}
