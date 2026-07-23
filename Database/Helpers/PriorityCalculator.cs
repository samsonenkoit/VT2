using Database.Models;

namespace Database.Helpers;

public static class PriorityCalculator
{
    public static TaskPriority Calculate(
        TaskImportance importance,
        TaskDelayRisk delayRisk,
        TaskDifficulty difficulty,
        TaskUrgency urgency)
    {
        var basePriority = GetBasePriority(importance, urgency);
        var bump = 0;
        if (delayRisk == TaskDelayRisk.High)
            bump++;
        if (difficulty == TaskDifficulty.High)
            bump++;

        var rank = Math.Min(3, ToRank(basePriority) + bump);
        return FromRank(rank);
    }

    private static TaskPriority GetBasePriority(TaskImportance importance, TaskUrgency urgency) =>
        (importance, urgency) switch
        {
            (TaskImportance.Low, TaskUrgency.Low) => TaskPriority.NotUrgent,
            (TaskImportance.Low, TaskUrgency.Medium) => TaskPriority.NotUrgent,
            (TaskImportance.Low, TaskUrgency.High) => TaskPriority.Medium,

            (TaskImportance.Medium, TaskUrgency.Low) => TaskPriority.NotUrgent,
            (TaskImportance.Medium, TaskUrgency.Medium) => TaskPriority.Medium,
            (TaskImportance.Medium, TaskUrgency.High) => TaskPriority.Urgent,

            (TaskImportance.High, TaskUrgency.Low) => TaskPriority.Medium,
            (TaskImportance.High, TaskUrgency.Medium) => TaskPriority.Urgent,
            (TaskImportance.High, TaskUrgency.High) => TaskPriority.Critical,

            (TaskImportance.Critical, TaskUrgency.Low) => TaskPriority.Urgent,
            (TaskImportance.Critical, TaskUrgency.Medium) => TaskPriority.Critical,
            (TaskImportance.Critical, TaskUrgency.High) => TaskPriority.Critical,

            _ => TaskPriority.NotUrgent,
        };

    // Severity rank (TaskPriority enum order is Critical=0 .. NotUrgent=3).
    private static int ToRank(TaskPriority priority) => priority switch
    {
        TaskPriority.NotUrgent => 0,
        TaskPriority.Medium => 1,
        TaskPriority.Urgent => 2,
        TaskPriority.Critical => 3,
        _ => 0,
    };

    private static TaskPriority FromRank(int rank) => rank switch
    {
        0 => TaskPriority.NotUrgent,
        1 => TaskPriority.Medium,
        2 => TaskPriority.Urgent,
        _ => TaskPriority.Critical,
    };
}
