using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Models;
using Database.Repositories;
using Microsoft.Win32;
using VtApp.Models;
using VtApp.Services;

namespace VtApp.ViewModels;

public partial class TaskEditViewModel : ObservableObject
{
    private readonly ITaskRepository _taskRepository;
    private readonly ISubtaskRepository _subtaskRepository;
    private readonly ITaskFileService _taskFileService;
    private Action? _onSaved;
    private Action? _onCancelled;
    private int? _taskId;

    public ObservableCollection<SubtaskEditItem> Subtasks { get; } = [];

    public ObservableCollection<TaskFileItem> Files { get; } = [];

    public bool CanManageFiles => _taskId is not null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSubtaskCommand))]
    private string _newSubtaskTitle = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    public string PageTitle => IsEditMode ? "Редактирование задачи" : "Новая задача";

    public TaskEditViewModel(
        ITaskRepository taskRepository,
        ISubtaskRepository subtaskRepository,
        ITaskFileService taskFileService)
    {
        _taskRepository = taskRepository;
        _subtaskRepository = subtaskRepository;
        _taskFileService = taskFileService;
    }

    public void Configure(Action onSaved, Action onCancelled)
    {
        _onSaved = onSaved;
        _onCancelled = onCancelled;
    }

    public void PrepareForCreate()
    {
        _taskId = null;
        IsEditMode = false;
        Title = string.Empty;
        Subtasks.Clear();
        NewSubtaskTitle = string.Empty;
        Files.Clear();
        NotifyFilesStateChanged();
    }

    public async Task<bool> PrepareForEditAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task is null || task.DeletedAtUtc is not null)
            return false;

        _taskId = taskId;
        IsEditMode = true;
        Title = task.Title;

        var subtasks = await _subtaskRepository.GetNotDeletedAsync(taskId);
        Subtasks.Clear();
        foreach (var subtask in subtasks)
        {
            Subtasks.Add(new SubtaskEditItem
            {
                Id = subtask.Id,
                Title = subtask.Title,
            });
        }

        NewSubtaskTitle = string.Empty;

        var files = await _taskFileService.GetFilesAsync(taskId);
        Files.Clear();
        foreach (var file in files)
            Files.Add(file);

        NotifyFilesStateChanged();
        return true;
    }

    partial void OnIsEditModeChanged(bool value) => OnPropertyChanged(nameof(PageTitle));

    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);

    private bool CanAddSubtask() => !string.IsNullOrWhiteSpace(NewSubtaskTitle);

    private bool CanAddFile() => CanManageFiles;

    [RelayCommand(CanExecute = nameof(CanAddSubtask))]
    private void AddSubtask()
    {
        Subtasks.Add(new SubtaskEditItem { Title = NewSubtaskTitle.Trim() });
        NewSubtaskTitle = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanAddFile))]
    private async Task AddFileAsync()
    {
        if (_taskId is null)
            return;

        var sourcePath = PickFile();
        if (sourcePath is null)
            return;

        var file = await _taskFileService.AddFileAsync(_taskId.Value, sourcePath);
        Files.Add(file);
    }

    [RelayCommand(CanExecute = nameof(CanAddFile))]
    private async Task DeleteFileAsync(TaskFileItem file)
    {
        await _taskFileService.DeleteFileAsync(file.Id);
        Files.Remove(file);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var trimmedTitle = Title.Trim();

        if (_taskId is null)
        {
            var task = await _taskRepository.AddAsync(new TaskDb
            {
                Title = trimmedTitle,
                DueDateUtc = DateTime.Today,
                ProgressPercent = 0,
                Priority = TaskPriority.Medium,
            });

            await SaveSubtasksAsync(task.Id);
        }
        else
        {
            var task = await _taskRepository.GetByIdAsync(_taskId.Value);
            if (task is null)
                return;

            task.Title = trimmedTitle;
            await _taskRepository.UpdateAsync(task);
            await SaveSubtasksAsync(task.Id);
        }

        _onSaved?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => _onCancelled?.Invoke();

    private async Task SaveSubtasksAsync(int taskId)
    {
        foreach (var subtask in Subtasks)
        {
            var trimmedTitle = subtask.Title.Trim();
            if (string.IsNullOrWhiteSpace(trimmedTitle))
                continue;

            if (subtask.Id == 0)
            {
                await _subtaskRepository.AddAsync(new SubtaskDb
                {
                    Title = trimmedTitle,
                    TaskId = taskId,
                });
            }
            else
            {
                await _subtaskRepository.UpdateAsync(new SubtaskDb
                {
                    Id = subtask.Id,
                    Title = trimmedTitle,
                    TaskId = taskId,
                });
            }
        }
    }

    private static string? PickFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите файл",
            CheckFileExists = true,
            Multiselect = false,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void NotifyFilesStateChanged()
    {
        OnPropertyChanged(nameof(CanManageFiles));
        AddFileCommand.NotifyCanExecuteChanged();
        DeleteFileCommand.NotifyCanExecuteChanged();
    }
}
