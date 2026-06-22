using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VtApp.Models;

namespace VtApp.ViewModels;

public partial class TasksViewModel : ObservableObject
{
    public ObservableCollection<TaskItem> CriticalTasks { get; } = [];
    public ObservableCollection<TaskItem> UrgentTasks { get; } = [];
    public ObservableCollection<TaskItem> MediumTasks { get; } = [];
    public ObservableCollection<TaskItem> NotUrgentTasks { get; } = [];

    public TasksViewModel()
    {
        SeedDemoData();
    }

    private void SeedDemoData()
    {
        AddTasks(CriticalTasks,
        [
            new TaskItem
            {
                Title = "Согласовать срочный договор с поставщиком",
                DueDate = new DateTime(2026, 3, 18),
                ProgressPercent = 15,
                Priority = TaskPriority.Critical,
                EmailCount = 12,
                BadgeCounts = [23],
            },
            new TaskItem
            {
                Title = "Устранить критическую ошибку в отчёте",
                DueDate = new DateTime(2026, 3, 20),
                ProgressPercent = 40,
                Priority = TaskPriority.Critical,
                EmailCount = 5,
                BadgeCounts = [8, 2],
            },
            new TaskItem
            {
                Title = "Подготовить ответ регулятору",
                DueDate = new DateTime(2026, 3, 22),
                ProgressPercent = 5,
                Priority = TaskPriority.Critical,
                EmailCount = 31,
                BadgeCounts = [1],
            },
        ]);

        AddTasks(UrgentTasks,
        [
            new TaskItem
            {
                Title = "Создать первую задачу в новом проекте",
                DueDate = new DateTime(2026, 3, 25),
                ProgressPercent = 30,
                Priority = TaskPriority.Urgent,
                EmailCount = 7,
                BadgeCounts = [285, 1],
            },
            new TaskItem
            {
                Title = "Согласовать бюджет на второй квартал",
                DueDate = new DateTime(2026, 3, 28),
                ProgressPercent = 55,
                Priority = TaskPriority.Urgent,
                EmailCount = 14,
                BadgeCounts = [42],
            },
            new TaskItem
            {
                Title = "Обновить презентацию для совещания",
                DueDate = new DateTime(2026, 4, 1),
                ProgressPercent = 70,
                Priority = TaskPriority.Urgent,
                EmailCount = 3,
                BadgeCounts = [6, 11],
            },
            new TaskItem
            {
                Title = "Проверить входящие заявки от клиентов",
                DueDate = new DateTime(2026, 4, 3),
                ProgressPercent = 20,
                Priority = TaskPriority.Urgent,
                EmailCount = 19,
                BadgeCounts = [9],
            },
        ]);

        AddTasks(MediumTasks,
        [
            new TaskItem
            {
                Title = "Разобрать архив переписки за месяц",
                DueDate = new DateTime(2026, 4, 10),
                ProgressPercent = 45,
                Priority = TaskPriority.Medium,
                EmailCount = 64,
                BadgeCounts = [17],
            },
            new TaskItem
            {
                Title = "Актуализировать список контактов",
                DueDate = new DateTime(2026, 4, 12),
                ProgressPercent = 60,
                Priority = TaskPriority.Medium,
                EmailCount = 2,
                BadgeCounts = [4, 3],
            },
            new TaskItem
            {
                Title = "Подготовить шаблоны ответов для поддержки",
                DueDate = new DateTime(2026, 4, 15),
                ProgressPercent = 35,
                Priority = TaskPriority.Medium,
                EmailCount = 8,
                BadgeCounts = [12],
            },
        ]);

        AddTasks(NotUrgentTasks,
        [
            new TaskItem
            {
                Title = "Изучить новые возможности Material Design",
                DueDate = new DateTime(2026, 5, 5),
                ProgressPercent = 10,
                Priority = TaskPriority.NotUrgent,
                EmailCount = 1,
                BadgeCounts = [3],
            },
            new TaskItem
            {
                Title = "Навести порядок в папках проекта",
                DueDate = new DateTime(2026, 5, 12),
                ProgressPercent = 0,
                Priority = TaskPriority.NotUrgent,
                EmailCount = 0,
                BadgeCounts = [],
            },
            new TaskItem
            {
                Title = "Составить план обучения команды",
                DueDate = new DateTime(2026, 5, 20),
                ProgressPercent = 25,
                Priority = TaskPriority.NotUrgent,
                EmailCount = 6,
                BadgeCounts = [5, 2],
            },
            new TaskItem
            {
                Title = "Обновить документацию по процессам",
                DueDate = new DateTime(2026, 6, 1),
                ProgressPercent = 80,
                Priority = TaskPriority.NotUrgent,
                EmailCount = 4,
                BadgeCounts = [7],
            },
            new TaskItem
            {
                Title = "Провести ретроспективу квартала",
                DueDate = new DateTime(2026, 6, 15),
                ProgressPercent = 50,
                Priority = TaskPriority.NotUrgent,
                EmailCount = 11,
                BadgeCounts = [14, 6],
            },
        ]);
    }

    private static void AddTasks(ObservableCollection<TaskItem> collection, IEnumerable<TaskItem> tasks)
    {
        foreach (var task in tasks)
            collection.Add(task);
    }
}
