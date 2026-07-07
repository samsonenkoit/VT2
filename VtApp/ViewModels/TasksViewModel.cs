using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Models;
using Database.Repositories;
using VtApp.Models;
using VtApp.Services;

namespace VtApp.ViewModels;

public partial class TasksViewModel : ObservableObject
{
    private readonly ITaskRepository _taskRepository;

    public ObservableCollection<TaskItem> CriticalTasks { get; } = [];
    public ObservableCollection<TaskItem> UrgentTasks { get; } = [];
    public ObservableCollection<TaskItem> MediumTasks { get; } = [];
    public ObservableCollection<TaskItem> NotUrgentTasks { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    public TasksViewModel(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
        _ = LoadTasksAsync();
    }

    public async Task LoadTasksAsync()
    {
        IsLoading = true;

        try
        {
            var tasks = await _taskRepository.GetAllActiveAsync();
            ClearCollections();

            foreach (var task in tasks)
                GetCollectionForPriority(task.Priority).Add(TaskMapper.ToTaskItem(task));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddTask()
    {
    }

    private void ClearCollections()
    {
        CriticalTasks.Clear();
        UrgentTasks.Clear();
        MediumTasks.Clear();
        NotUrgentTasks.Clear();
    }

    private ObservableCollection<TaskItem> GetCollectionForPriority(TaskPriority priority) =>
        priority switch
        {
            TaskPriority.Critical => CriticalTasks,
            TaskPriority.Urgent => UrgentTasks,
            TaskPriority.Medium => MediumTasks,
            TaskPriority.NotUrgent => NotUrgentTasks,
            _ => NotUrgentTasks,
        };
}
