using Database.Models;
using Database.Services;

namespace Database.Seed;

public static class TaskSeedData
{
    public static (IReadOnlyList<TaskDb> Tasks, IReadOnlyList<SubtaskDb> Subtasks, IReadOnlyList<GoalDb> Goals) GetSeedData()
    {
        var tasks = new List<TaskDb>
        {
            CreateTask(
                1,
                "Согласовать срочный договор с поставщиком",
                new DateTime(2026, 3, 18),
                15,
                TaskImportance.High,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                2,
                "Устранить критическую ошибку в отчёте",
                new DateTime(2026, 3, 20),
                40,
                TaskImportance.Critical,
                TaskDelayRisk.Medium,
                TaskDifficulty.Medium,
                TaskUrgency.Medium),
            CreateTask(
                3,
                "Подготовить ответ регулятору",
                new DateTime(2026, 3, 22),
                5,
                TaskImportance.Critical,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                4,
                "Создать первую задачу в новом проекте",
                new DateTime(2026, 3, 25),
                30,
                TaskImportance.Medium,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                5,
                "Согласовать бюджет на второй квартал",
                new DateTime(2026, 3, 28),
                55,
                TaskImportance.High,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Medium),
            CreateTask(
                6,
                "Обновить презентацию для совещания",
                new DateTime(2026, 4, 1),
                70,
                TaskImportance.High,
                TaskDelayRisk.Medium,
                TaskDifficulty.Low,
                TaskUrgency.Medium),
            CreateTask(
                7,
                "Проверить входящие заявки от клиентов",
                new DateTime(2026, 4, 3),
                20,
                TaskImportance.Critical,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                8,
                "Разобрать архив переписки за месяц",
                new DateTime(2026, 4, 10),
                45,
                TaskImportance.Medium,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Medium),
            CreateTask(
                9,
                "Актуализировать список контактов",
                new DateTime(2026, 4, 12),
                60,
                TaskImportance.High,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                10,
                "Подготовить шаблоны ответов для поддержки",
                new DateTime(2026, 4, 15),
                35,
                TaskImportance.Low,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.High),
            CreateTask(
                11,
                "Изучить новые возможности Material Design",
                new DateTime(2026, 5, 5),
                10,
                TaskImportance.Low,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                12,
                "Навести порядок в папках проекта",
                new DateTime(2026, 5, 12),
                0,
                TaskImportance.Low,
                TaskDelayRisk.Low,
                TaskDifficulty.Medium,
                TaskUrgency.Medium),
            CreateTask(
                13,
                "Составить план обучения команды",
                new DateTime(2026, 5, 20),
                25,
                TaskImportance.Medium,
                TaskDelayRisk.Low,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                14,
                "Обновить документацию по процессам",
                new DateTime(2026, 6, 1),
                80,
                TaskImportance.Low,
                TaskDelayRisk.Medium,
                TaskDifficulty.Low,
                TaskUrgency.Low),
            CreateTask(
                15,
                "Провести ретроспективу квартала",
                new DateTime(2026, 6, 15),
                50,
                TaskImportance.Medium,
                TaskDelayRisk.Medium,
                TaskDifficulty.Low,
                TaskUrgency.Low),
        };

        var subtasks = new List<SubtaskDb>
        {
            new() { Description = "Запросить правки у юридического отдела", TaskId = 1 },
            new() { Description = "Согласовать сумму с финансовым директором", TaskId = 1 },
            new() { Description = "Отправить финальную версию поставщику", TaskId = 1 },
            new() { Description = "Воспроизвести ошибку на тестовых данных", TaskId = 2 },
            new() { Description = "Исправить расчёт итоговой строки", TaskId = 2 },
        };

        var goals = new List<GoalDb>
        {
            new() { TaskId = 1, Text = "Подписать договор до конца недели" },
            new() { TaskId = 1, Text = "Сохранить условия поставки" },
            new() { TaskId = 2, Text = "Вернуть корректный отчёт пользователям" },
            new() { TaskId = 2, Text = "Исключить регрессию в итогах" },
        };

        return (tasks, subtasks, goals);
    }

    private static TaskDb CreateTask(
        int id,
        string title,
        DateTime dueDateUtc,
        int progressPercent,
        TaskImportance importance,
        TaskDelayRisk delayRisk,
        TaskDifficulty difficulty,
        TaskUrgency urgency) =>
        new()
        {
            Id = id,
            Title = title,
            DueDateUtc = dueDateUtc,
            ProgressPercent = progressPercent,
            Importance = importance,
            DelayRisk = delayRisk,
            Difficulty = difficulty,
            Urgency = urgency,
            Priority = PriorityCalculator.Calculate(importance, delayRisk, difficulty, urgency),
        };
}
