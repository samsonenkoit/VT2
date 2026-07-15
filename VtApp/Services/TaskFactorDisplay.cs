using Database.Models;

namespace VtApp.Services;

public static class TaskFactorDisplay
{
    public static string Importance(TaskImportance value) => value switch
    {
        TaskImportance.Low => "Низкая",
        TaskImportance.Medium => "Средняя",
        TaskImportance.High => "Высокая",
        TaskImportance.Critical => "Критическая",
        _ => value.ToString(),
    };

    public static string DelayRisk(TaskDelayRisk value) => value switch
    {
        TaskDelayRisk.Low => "Низкий",
        TaskDelayRisk.Medium => "Средний",
        TaskDelayRisk.High => "Высокий",
        _ => value.ToString(),
    };

    public static string Difficulty(TaskDifficulty value) => value switch
    {
        TaskDifficulty.Low => "Низкая",
        TaskDifficulty.Medium => "Средняя",
        TaskDifficulty.High => "Высокая",
        _ => value.ToString(),
    };

    public static string Urgency(TaskUrgency value) => value switch
    {
        TaskUrgency.Low => "Низкая",
        TaskUrgency.Medium => "Средняя",
        TaskUrgency.High => "Высокая",
        _ => value.ToString(),
    };

    public static string Priority(TaskPriority value) => value switch
    {
        TaskPriority.Critical => "Критический",
        TaskPriority.Urgent => "Срочный",
        TaskPriority.Medium => "Средний",
        TaskPriority.NotUrgent => "Несрочный",
        _ => value.ToString(),
    };
}
