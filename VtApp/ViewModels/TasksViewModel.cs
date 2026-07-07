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
    private readonly TaskEditViewModel _taskEditViewModel;

    public ObservableCollection<TaskItem> CriticalTasks { get; } = [];
    public ObservableCollection<TaskItem> UrgentTasks { get; } = [];
    public ObservableCollection<TaskItem> MediumTasks { get; } = [];
    public ObservableCollection<TaskItem> NotUrgentTasks { get; } = [];

    [ObservableProperty]
    private object _currentContent;

    [ObservableProperty]
    private bool _isLoading;

    public TasksViewModel(ITaskRepository taskRepository, TaskEditViewModel taskEditViewModel)
    {
        _taskRepository = taskRepository;
        _taskEditViewModel = taskEditViewModel;
        _currentContent = this;

        _taskEditViewModel.Configure(
            onSaved: () => _ = ReturnToListAsync(reload: true),
            onCancelled: () => ReturnToList(reload: false));

        _ = LoadTasksAsync();
    }

    public void ResetToBoard() => CurrentContent = this;

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
        _taskEditViewModel.PrepareForCreate();
        CurrentContent = _taskEditViewModel;
    }

    [RelayCommand]
    private async Task EditTask(TaskItem task) => await OpenEditAsync(task.Id);

    private async Task OpenEditAsync(int taskId)
    {
        if (!await _taskEditViewModel.PrepareForEditAsync(taskId))
            return;

        CurrentContent = _taskEditViewModel;
    }

    private void ReturnToList(bool reload)
    {
        CurrentContent = this;

        if (reload)
            _ = LoadTasksAsync();
    }

    private async Task ReturnToListAsync(bool reload)
    {
        CurrentContent = this;

        if (reload)
            await LoadTasksAsync();
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
