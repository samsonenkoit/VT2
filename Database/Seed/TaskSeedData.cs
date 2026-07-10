using Database.Models;

namespace Database.Seed;

public static class TaskSeedData
{
    public static (IReadOnlyList<TaskDb> Tasks, IReadOnlyList<SubtaskDb> Subtasks) GetSeedData()
    {
        var tasks = new List<TaskDb>
        {
            new()
            {
                Id = 1,
                Title = "Согласовать срочный договор с поставщиком",
                DueDate = new DateTime(2026, 3, 18),
                ProgressPercent = 15,
                Priority = TaskPriority.Critical,
            },
            new()
            {
                Id = 2,
                Title = "Устранить критическую ошибку в отчёте",
                DueDate = new DateTime(2026, 3, 20),
                ProgressPercent = 40,
                Priority = TaskPriority.Critical,
            },
            new()
            {
                Id = 3,
                Title = "Подготовить ответ регулятору",
                DueDate = new DateTime(2026, 3, 22),
                ProgressPercent = 5,
                Priority = TaskPriority.Critical,
            },
            new()
            {
                Id = 4,
                Title = "Создать первую задачу в новом проекте",
                DueDate = new DateTime(2026, 3, 25),
                ProgressPercent = 30,
                Priority = TaskPriority.Urgent,
            },
            new()
            {
                Id = 5,
                Title = "Согласовать бюджет на второй квартал",
                DueDate = new DateTime(2026, 3, 28),
                ProgressPercent = 55,
                Priority = TaskPriority.Urgent,
            },
            new()
            {
                Id = 6,
                Title = "Обновить презентацию для совещания",
                DueDate = new DateTime(2026, 4, 1),
                ProgressPercent = 70,
                Priority = TaskPriority.Urgent,
            },
            new()
            {
                Id = 7,
                Title = "Проверить входящие заявки от клиентов",
                DueDate = new DateTime(2026, 4, 3),
                ProgressPercent = 20,
                Priority = TaskPriority.Urgent,
            },
            new()
            {
                Id = 8,
                Title = "Разобрать архив переписки за месяц",
                DueDate = new DateTime(2026, 4, 10),
                ProgressPercent = 45,
                Priority = TaskPriority.Medium,
            },
            new()
            {
                Id = 9,
                Title = "Актуализировать список контактов",
                DueDate = new DateTime(2026, 4, 12),
                ProgressPercent = 60,
                Priority = TaskPriority.Medium,
            },
            new()
            {
                Id = 10,
                Title = "Подготовить шаблоны ответов для поддержки",
                DueDate = new DateTime(2026, 4, 15),
                ProgressPercent = 35,
                Priority = TaskPriority.Medium,
            },
            new()
            {
                Id = 11,
                Title = "Изучить новые возможности Material Design",
                DueDate = new DateTime(2026, 5, 5),
                ProgressPercent = 10,
                Priority = TaskPriority.NotUrgent,
            },
            new()
            {
                Id = 12,
                Title = "Навести порядок в папках проекта",
                DueDate = new DateTime(2026, 5, 12),
                ProgressPercent = 0,
                Priority = TaskPriority.NotUrgent,
            },
            new()
            {
                Id = 13,
                Title = "Составить план обучения команды",
                DueDate = new DateTime(2026, 5, 20),
                ProgressPercent = 25,
                Priority = TaskPriority.NotUrgent,
            },
            new()
            {
                Id = 14,
                Title = "Обновить документацию по процессам",
                DueDate = new DateTime(2026, 6, 1),
                ProgressPercent = 80,
                Priority = TaskPriority.NotUrgent,
            },
            new()
            {
                Id = 15,
                Title = "Провести ретроспективу квартала",
                DueDate = new DateTime(2026, 6, 15),
                ProgressPercent = 50,
                Priority = TaskPriority.NotUrgent,
            },
        };

        var subtasks = new List<SubtaskDb>
        {
            new() { Title = "Запросить правки у юридического отдела", TaskId = 1 },
            new() { Title = "Согласовать сумму с финансовым директором", TaskId = 1 },
            new() { Title = "Отправить финальную версию поставщику", TaskId = 1 },
            new() { Title = "Воспроизвести ошибку на тестовых данных", TaskId = 2 },
            new() { Title = "Исправить расчёт итоговой строки", TaskId = 2 },
        };

        return (tasks, subtasks);
    }
}
