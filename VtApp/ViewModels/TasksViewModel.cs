using System.Collections.ObjectModel;
using System.Windows;
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
    private readonly ITaskFileService _taskFileService;
    private readonly TaskEditViewModel _taskEditViewModel;

    public ObservableCollection<TaskItem> CriticalTasks { get; } = [];
    public ObservableCollection<TaskItem> UrgentTasks { get; } = [];
    public ObservableCollection<TaskItem> MediumTasks { get; } = [];
    public ObservableCollection<TaskItem> NotUrgentTasks { get; } = [];

    [ObservableProperty]
    private object _currentContent;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Overridable in tests; production uses MessageBox confirmation.
    /// </summary>
    public Func<string, string, MessageBoxResult> ConfirmDelete { get; set; } =
        static (message, caption) =>
            MessageBox.Show(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Warning);

    public TasksViewModel(
        ITaskRepository taskRepository,
        ITaskFileService taskFileService,
        TaskEditViewModel taskEditViewModel)
    {
        _taskRepository = taskRepository;
        _taskFileService = taskFileService;
        _taskEditViewModel = taskEditViewModel;
        _currentContent = this;

        _taskEditViewModel.Configure(
            onSaved: () => _ = ReturnToListAsync(reload: true),
            onCancelled: () => ReturnToList(reload: false));
    }

    public void ResetToBoard() => CurrentContent = this;

    public async Task LoadTasksAsync()
    {
        IsLoading = true;

        try
        {
            var tasks = await _taskRepository.GetAllNotDeletedAsync();
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
    private async Task AddTaskAsync()
    {
        CurrentContent = _taskEditViewModel;
        await _taskEditViewModel.PrepareForCreateAsync();
    }

    [RelayCommand]
    private Task EditTask(TaskItem task) => OpenEditAsync(task.Id);

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskItem task)
    {
        var result = ConfirmDelete(
            $"Удалить задачу \"{task.Title}\"?",
            "Удаление");

        if (result != MessageBoxResult.OK)
            return;

        await _taskRepository.SoftDeleteAsync(task.Id);
        await _taskFileService.DeleteTaskDirectoryAsync(task.Id);
        GetCollectionForPriority(task.Priority).Remove(task);
    }

    private async Task OpenEditAsync(int taskId)
    {
        CurrentContent = _taskEditViewModel;

        if (!await _taskEditViewModel.PrepareForEditAsync(taskId))
            CurrentContent = this;
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
