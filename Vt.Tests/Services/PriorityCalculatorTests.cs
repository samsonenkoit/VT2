using Database.Models;
using Database.Helpers;
using Xunit;

namespace Vt.Tests.Services;

public class PriorityCalculatorTests
{
    [Theory]
    [InlineData(TaskImportance.Low, TaskUrgency.Low, TaskPriority.NotUrgent)]
    [InlineData(TaskImportance.Low, TaskUrgency.Medium, TaskPriority.NotUrgent)]
    [InlineData(TaskImportance.Low, TaskUrgency.High, TaskPriority.Medium)]
    [InlineData(TaskImportance.Medium, TaskUrgency.Low, TaskPriority.NotUrgent)]
    [InlineData(TaskImportance.Medium, TaskUrgency.Medium, TaskPriority.Medium)]
    [InlineData(TaskImportance.Medium, TaskUrgency.High, TaskPriority.Urgent)]
    [InlineData(TaskImportance.High, TaskUrgency.Low, TaskPriority.Medium)]
    [InlineData(TaskImportance.High, TaskUrgency.Medium, TaskPriority.Urgent)]
    [InlineData(TaskImportance.High, TaskUrgency.High, TaskPriority.Critical)]
    [InlineData(TaskImportance.Critical, TaskUrgency.Low, TaskPriority.Urgent)]
    [InlineData(TaskImportance.Critical, TaskUrgency.Medium, TaskPriority.Critical)]
    [InlineData(TaskImportance.Critical, TaskUrgency.High, TaskPriority.Critical)]
    public void Calculate_BaseMatrix_ReturnsExpectedPriority(
        TaskImportance importance,
        TaskUrgency urgency,
        TaskPriority expected)
    {
        var result = PriorityCalculator.Calculate(
            importance,
            TaskDelayRisk.Low,
            TaskDifficulty.Low,
            urgency);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_HighDelayRisk_BumpsPriorityByOne()
    {
        var result = PriorityCalculator.Calculate(
            TaskImportance.Medium,
            TaskDelayRisk.High,
            TaskDifficulty.Low,
            TaskUrgency.Medium);

        Assert.Equal(TaskPriority.Urgent, result);
    }

    [Fact]
    public void Calculate_HighDifficulty_BumpsPriorityByOne()
    {
        var result = PriorityCalculator.Calculate(
            TaskImportance.Medium,
            TaskDelayRisk.Low,
            TaskDifficulty.High,
            TaskUrgency.Medium);

        Assert.Equal(TaskPriority.Urgent, result);
    }

    [Fact]
    public void Calculate_BothHighBumps_CanReachCritical()
    {
        var result = PriorityCalculator.Calculate(
            TaskImportance.Medium,
            TaskDelayRisk.High,
            TaskDifficulty.High,
            TaskUrgency.Medium);

        Assert.Equal(TaskPriority.Critical, result);
    }

    [Fact]
    public void Calculate_BumpDoesNotExceedCritical()
    {
        var result = PriorityCalculator.Calculate(
            TaskImportance.High,
            TaskDelayRisk.High,
            TaskDifficulty.High,
            TaskUrgency.High);

        Assert.Equal(TaskPriority.Critical, result);
    }
}
