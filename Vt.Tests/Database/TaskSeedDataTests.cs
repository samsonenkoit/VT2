using Database.Seed;
using Xunit;

namespace Vt.Tests.Database;

public class TaskSeedDataTests
{
    [Fact]
    public void GetSeedData_EachTaskHasDescriptionSubtasksAndGoals()
    {
        var (tasks, subtasks, goals) = TaskSeedData.GetSeedData();

        Assert.NotEmpty(tasks);

        foreach (var task in tasks)
        {
            Assert.False(string.IsNullOrWhiteSpace(task.Description),
                $"Task {task.Id} must have a description.");

            var taskSubtasks = subtasks.Where(s => s.TaskId == task.Id).ToList();
            Assert.InRange(taskSubtasks.Count, 1, 6);
            Assert.All(taskSubtasks, s => Assert.False(string.IsNullOrWhiteSpace(s.Description)));

            var taskGoals = goals.Where(g => g.TaskId == task.Id).ToList();
            Assert.InRange(taskGoals.Count, 1, 3);
            Assert.All(taskGoals, g => Assert.False(string.IsNullOrWhiteSpace(g.Text)));
        }

        Assert.All(subtasks, s => Assert.Contains(tasks, t => t.Id == s.TaskId));
        Assert.All(goals, g => Assert.Contains(tasks, t => t.Id == g.TaskId));
    }
}
