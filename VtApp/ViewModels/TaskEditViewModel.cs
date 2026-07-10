using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Models;
using Database.Repositories;

namespace VtApp.ViewModels;

public partial class TaskEditViewModel : ObservableObject
{
    private readonly ITaskRepository _taskRepository;
    private Action? _onSaved;
    private Action? _onCancelled;
    private int? _taskId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    public string PageTitle => IsEditMode ? "Редактирование задачи" : "Новая задача";

    public TaskEditViewModel(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
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
    }

    public async Task<bool> PrepareForEditAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task is null || task.DeletedAtUtc is not null)
            return false;

        _taskId = taskId;
        IsEditMode = true;
        Title = task.Title;
        return true;
    }

    partial void OnIsEditModeChanged(bool value) => OnPropertyChanged(nameof(PageTitle));

    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var trimmedTitle = Title.Trim();

        if (_taskId is null)
        {
            await _taskRepository.AddAsync(new TaskDb
            {
                Title = trimmedTitle,
                DueDateUtc = DateTime.Today,
                ProgressPercent = 0,
                Priority = TaskPriority.Medium,
            });
        }
        else
        {
            var task = await _taskRepository.GetByIdAsync(_taskId.Value);
            if (task is null)
                return;

            task.Title = trimmedTitle;
            await _taskRepository.UpdateAsync(task);
        }

        _onSaved?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => _onCancelled?.Invoke();
}
