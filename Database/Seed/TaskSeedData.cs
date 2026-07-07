using Database.Models;

namespace Database.Seed;

public static class TaskSeedData
{
    public static IReadOnlyList<TaskDb> GetTasks() =>
    [
        new()
        {
            Title = "Согласовать срочный договор с поставщиком",
            DueDate = new DateTime(2026, 3, 18),
            ProgressPercent = 15,
            Priority = TaskPriority.Critical,
        },
        new()
        {
            Title = "Устранить критическую ошибку в отчёте",
            DueDate = new DateTime(2026, 3, 20),
            ProgressPercent = 40,
            Priority = TaskPriority.Critical,
        },
        new()
        {
            Title = "Подготовить ответ регулятору",
            DueDate = new DateTime(2026, 3, 22),
            ProgressPercent = 5,
            Priority = TaskPriority.Critical,
        },
        new()
        {
            Title = "Создать первую задачу в новом проекте",
            DueDate = new DateTime(2026, 3, 25),
            ProgressPercent = 30,
            Priority = TaskPriority.Urgent,
        },
        new()
        {
            Title = "Согласовать бюджет на второй квартал",
            DueDate = new DateTime(2026, 3, 28),
            ProgressPercent = 55,
            Priority = TaskPriority.Urgent,
        },
        new()
        {
            Title = "Обновить презентацию для совещания",
            DueDate = new DateTime(2026, 4, 1),
            ProgressPercent = 70,
            Priority = TaskPriority.Urgent,
        },
        new()
        {
            Title = "Проверить входящие заявки от клиентов",
            DueDate = new DateTime(2026, 4, 3),
            ProgressPercent = 20,
            Priority = TaskPriority.Urgent,
        },
        new()
        {
            Title = "Разобрать архив переписки за месяц",
            DueDate = new DateTime(2026, 4, 10),
            ProgressPercent = 45,
            Priority = TaskPriority.Medium,
        },
        new()
        {
            Title = "Актуализировать список контактов",
            DueDate = new DateTime(2026, 4, 12),
            ProgressPercent = 60,
            Priority = TaskPriority.Medium,
        },
        new()
        {
            Title = "Подготовить шаблоны ответов для поддержки",
            DueDate = new DateTime(2026, 4, 15),
            ProgressPercent = 35,
            Priority = TaskPriority.Medium,
        },
        new()
        {
            Title = "Изучить новые возможности Material Design",
            DueDate = new DateTime(2026, 5, 5),
            ProgressPercent = 10,
            Priority = TaskPriority.NotUrgent,
        },
        new()
        {
            Title = "Навести порядок в папках проекта",
            DueDate = new DateTime(2026, 5, 12),
            ProgressPercent = 0,
            Priority = TaskPriority.NotUrgent,
        },
        new()
        {
            Title = "Составить план обучения команды",
            DueDate = new DateTime(2026, 5, 20),
            ProgressPercent = 25,
            Priority = TaskPriority.NotUrgent,
        },
        new()
        {
            Title = "Обновить документацию по процессам",
            DueDate = new DateTime(2026, 6, 1),
            ProgressPercent = 80,
            Priority = TaskPriority.NotUrgent,
        },
        new()
        {
            Title = "Провести ретроспективу квартала",
            DueDate = new DateTime(2026, 6, 15),
            ProgressPercent = 50,
            Priority = TaskPriority.NotUrgent,
        },
    ];
}
