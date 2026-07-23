using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Models;
using Database.Repositories;
using Database.Services;
using Microsoft.Win32;
using VtApp.Models;
using VtApp.Services;

namespace VtApp.ViewModels;

public partial class TaskEditViewModel : ObservableObject
{
    private readonly ITaskRepository _taskRepository;
    private readonly ISubtaskRepository _subtaskRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly ITaskFileService _taskFileService;

    private readonly List<int> _removedSubtaskIds = [];

    private Action? _onSaved;
    private Action? _onCancelled;

    private int? _taskId;

    #region properties
    public ObservableCollection<SubtaskEditItem> Subtasks { get; } = [];

    public ObservableCollection<GoalEditItem> Goals { get; } = [];

    public ObservableCollection<TaskFileItem> Files { get; } = [];

    public IReadOnlyList<EnumOption<TaskImportance>> ImportanceOptions { get; } =
    [
        new() { Value = TaskImportance.Low, Display = TaskFactorDisplay.Importance(TaskImportance.Low) },
        new() { Value = TaskImportance.Medium, Display = TaskFactorDisplay.Importance(TaskImportance.Medium) },
        new() { Value = TaskImportance.High, Display = TaskFactorDisplay.Importance(TaskImportance.High) },
        new() { Value = TaskImportance.Critical, Display = TaskFactorDisplay.Importance(TaskImportance.Critical) },
    ];

    public IReadOnlyList<EnumOption<TaskDelayRisk>> DelayRiskOptions { get; } =
    [
        new() { Value = TaskDelayRisk.Low, Display = TaskFactorDisplay.DelayRisk(TaskDelayRisk.Low) },
        new() { Value = TaskDelayRisk.Medium, Display = TaskFactorDisplay.DelayRisk(TaskDelayRisk.Medium) },
        new() { Value = TaskDelayRisk.High, Display = TaskFactorDisplay.DelayRisk(TaskDelayRisk.High) },
    ];

    public IReadOnlyList<EnumOption<TaskDifficulty>> DifficultyOptions { get; } =
    [
        new() { Value = TaskDifficulty.Low, Display = TaskFactorDisplay.Difficulty(TaskDifficulty.Low) },
        new() { Value = TaskDifficulty.Medium, Display = TaskFactorDisplay.Difficulty(TaskDifficulty.Medium) },
        new() { Value = TaskDifficulty.High, Display = TaskFactorDisplay.Difficulty(TaskDifficulty.High) },
    ];

    public IReadOnlyList<EnumOption<TaskUrgency>> UrgencyOptions { get; } =
    [
        new() { Value = TaskUrgency.Low, Display = TaskFactorDisplay.Urgency(TaskUrgency.Low) },
        new() { Value = TaskUrgency.Medium, Display = TaskFactorDisplay.Urgency(TaskUrgency.Medium) },
        new() { Value = TaskUrgency.High, Display = TaskFactorDisplay.Urgency(TaskUrgency.High) },
    ];

    public bool CanManageFiles => _taskId is not null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _dueDate = DateTime.Today.AddDays(3);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSubtaskCommand))]
    private string _newSubtaskDescription = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUiEnabled))]
    private bool _isLoading;

    public bool IsUiEnabled => !IsLoading;

    [ObservableProperty]
    private TaskImportance _importance = TaskImportance.Medium;

    [ObservableProperty]
    private TaskDelayRisk _delayRisk = TaskDelayRisk.Low;

    [ObservableProperty]
    private TaskDifficulty _difficulty = TaskDifficulty.Low;

    [ObservableProperty]
    private TaskUrgency _urgency = TaskUrgency.Medium;

    [ObservableProperty]
    private TaskPriority _priority = TaskPriority.Medium;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressLabel))]
    private int _progressPercent;

    public string PriorityDisplay => TaskFactorDisplay.Priority(Priority);

    public string ProgressLabel => $"Выполнено ({ProgressPercent}%):";

    public string SubtasksProgressLabel
    {
        get
        {
            var total = Subtasks.Count;
            var done = Subtasks.Count(s => s.IsDone);
            return $"Подзадачи · {done}/{total}";
        }
    }

    #endregion

    public TaskEditViewModel(
        ITaskRepository taskRepository,
        ISubtaskRepository subtaskRepository,
        IGoalRepository goalRepository,
        ITaskFileService taskFileService)
    {
        _taskRepository = taskRepository;
        _subtaskRepository = subtaskRepository;
        _goalRepository = goalRepository;
        _taskFileService = taskFileService;
        Subtasks.CollectionChanged += OnSubtasksCollectionChanged;
    }

    public void Configure(Action onSaved, Action onCancelled)
    {
        _onSaved = onSaved;
        _onCancelled = onCancelled;
    }

    public async Task PrepareForCreateAsync()
    {
        IsLoading = true;
        try
        {
            await Task.Yield();

            _taskId = null;
            IsEditMode = false;
            Title = string.Empty;
            Description = string.Empty;
            DueDate = DateTime.Today.AddDays(3);
            Importance = TaskImportance.Medium;
            DelayRisk = TaskDelayRisk.Low;
            Difficulty = TaskDifficulty.Low;
            Urgency = TaskUrgency.Medium;
            ProgressPercent = 0;
            RecalculatePriority();
            ClearSubtasks();
            NewSubtaskDescription = string.Empty;
            ResetGoals();
            Files.Clear();
            NotifyFilesStateChanged();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> PrepareForEditAsync(int taskId)
    {
        IsLoading = true;
        try
        {
            var task = await _taskRepository.GetAsync(taskId);
            if (task is null || task.DeletedAtUtc is not null)
                return false;

            _taskId = taskId;
            IsEditMode = true;
            Title = task.Title;
            Description = task.Description;
            DueDate = ToDueDateLocal(task.DueDateUtc);
            Importance = task.Importance;
            DelayRisk = task.DelayRisk;
            Difficulty = task.Difficulty;
            Urgency = task.Urgency;
            ProgressPercent = task.ProgressPercent;
            RecalculatePriority();

            ClearSubtasks();
            var subtasks = await _subtaskRepository.GetNotDeletedAsync(taskId);
            foreach (var subtask in subtasks)
            {
                Subtasks.Add(new SubtaskEditItem
                {
                    Id = subtask.Id,
                    Description = subtask.Description,
                    DueDate = ToDueDateLocal(subtask.DueDateUtc),
                    ProgressPercent = SubtaskProgressOptions.Normalize(subtask.ProgressPercent),
                });
            }

            NewSubtaskDescription = string.Empty;
            await LoadGoalsAsync(taskId);

            var files = await _taskFileService.GetFilesAsync(taskId);
            Files.Clear();
            foreach (var file in files)
                Files.Add(file);

            NotifyFilesStateChanged();
            return true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #region handlers

    partial void OnImportanceChanged(TaskImportance value) => RecalculatePriority();

    partial void OnDelayRiskChanged(TaskDelayRisk value) => RecalculatePriority();

    partial void OnDifficultyChanged(TaskDifficulty value) => RecalculatePriority();

    partial void OnUrgencyChanged(TaskUrgency value) => RecalculatePriority();

    partial void OnPriorityChanged(TaskPriority value) => OnPropertyChanged(nameof(PriorityDisplay));

    partial void OnDueDateChanged(DateTime value)
    {
        var date = value.Date;
        if (date != value)
            DueDate = date;
    }

    partial void OnProgressPercentChanged(int value)
    {
        var clamped = Math.Clamp(value, 0, 100);
        if (clamped != value)
            ProgressPercent = clamped;
    }

    private void OnSubtasksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (SubtaskEditItem item in e.OldItems)
                item.PropertyChanged -= OnSubtaskPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (SubtaskEditItem item in e.NewItems)
                item.PropertyChanged += OnSubtaskPropertyChanged;
        }

        NotifySubtasksProgressChanged();
    }

    #endregion

    private void RecalculatePriority() =>
        Priority = PriorityCalculator.Calculate(Importance, DelayRisk, Difficulty, Urgency);

    #region commands

    private bool CanAddSubtask() => !string.IsNullOrWhiteSpace(NewSubtaskDescription);

    private bool CanAddFile() => CanManageFiles;

    [RelayCommand(CanExecute = nameof(CanAddSubtask))]
    private void AddSubtask()
    {
        Subtasks.Add(new SubtaskEditItem
        {
            Description = NewSubtaskDescription.Trim(),
            DueDate = null,
            ProgressPercent = 0,
        });
        NewSubtaskDescription = string.Empty;
    }

    [RelayCommand]
    private void RemoveSubtask(SubtaskEditItem? subtask)
    {
        if (subtask is null)
            return;

        if (subtask.Id != 0)
            _removedSubtaskIds.Add(subtask.Id);

        Subtasks.Remove(subtask);
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

    #region save task commnad
    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var trimmedTitle = Title.Trim();
        RecalculatePriority();
        var dueDateUtc = ToDueDateUtc(DueDate);

        if (_taskId is null)
        {
            var task = await _taskRepository.AddAsync(new TaskDb
            {
                Title = trimmedTitle,
                Description = Description.Trim(),
                DueDateUtc = dueDateUtc,
                ProgressPercent = ProgressPercent,
                Importance = Importance,
                DelayRisk = DelayRisk,
                Difficulty = Difficulty,
                Urgency = Urgency,
                Priority = Priority,
            });

            await SaveSubtasksAsync(task.Id);
            await SaveGoalsAsync(task.Id);
        }
        else
        {
            var task = await _taskRepository.GetAsync(_taskId.Value);
            if (task is null)
                return;

            task.Title = trimmedTitle;
            task.Description = Description.Trim();
            task.DueDateUtc = dueDateUtc;
            task.Importance = Importance;
            task.DelayRisk = DelayRisk;
            task.Difficulty = Difficulty;
            task.Urgency = Urgency;
            task.Priority = Priority;
            task.ProgressPercent = ProgressPercent;
            await _taskRepository.UpdateAsync(task);
            await SaveSubtasksAsync(task.Id);
            await SaveGoalsAsync(task.Id);
        }

        _onSaved?.Invoke();
    }

    #endregion

    [RelayCommand]
    private void Cancel() => _onCancelled?.Invoke();

    private async Task SaveSubtasksAsync(int taskId)
    {
        foreach (var removedId in _removedSubtaskIds.Distinct())
            await _subtaskRepository.SoftDeleteAsync(removedId);
        _removedSubtaskIds.Clear();

        foreach (var subtask in Subtasks)
        {
            var trimmedDescription = subtask.Description.Trim();
            if (string.IsNullOrWhiteSpace(trimmedDescription))
                continue;

            var dueDateUtc = ToDueDateUtc(subtask.DueDate);

            if (subtask.Id == 0)
            {
                await _subtaskRepository.AddAsync(new SubtaskDb
                {
                    TaskId = taskId,
                    Description = trimmedDescription,
                    DueDateUtc = dueDateUtc,
                    ProgressPercent = subtask.ProgressPercent,
                });
            }
            else
            {
                await _subtaskRepository.UpdateAsync(new SubtaskDb
                {
                    Id = subtask.Id,
                    TaskId = taskId,
                    Description = trimmedDescription,
                    DueDateUtc = dueDateUtc,
                    ProgressPercent = subtask.ProgressPercent,
                });
            }
        }
    }

    private async Task SaveGoalsAsync(int taskId)
    {
        for (var i = 0; i < Goals.Count && i < 3; i++)
        {
            var goal = Goals[i];
            var trimmed = goal.Text.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                if (goal.Id != 0)
                    await _goalRepository.SoftDeleteAsync(goal.Id);
                continue;
            }

            if (goal.Id == 0)
            {
                var added = await _goalRepository.AddAsync(new GoalDb
                {
                    TaskId = taskId,
                    Text = trimmed,
                });
                goal.Id = added.Id;
                goal.Text = trimmed;
            }
            else
            {
                await _goalRepository.UpdateAsync(new GoalDb
                {
                    Id = goal.Id,
                    TaskId = taskId,
                    Text = trimmed,
                });
                goal.Text = trimmed;
            }
        }
    }

    #endregion

    private void ClearSubtasks()
    {
        foreach (var item in Subtasks)
            item.PropertyChanged -= OnSubtaskPropertyChanged;

        Subtasks.Clear();
        _removedSubtaskIds.Clear();
        NotifySubtasksProgressChanged();
    }

    private void OnSubtaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SubtaskEditItem.ProgressPercent) or nameof(SubtaskEditItem.IsDone))
            NotifySubtasksProgressChanged();
    }

    private void NotifySubtasksProgressChanged() =>
        OnPropertyChanged(nameof(SubtasksProgressLabel));

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

    private async Task LoadGoalsAsync(int taskId)
    {
        var goals = await _goalRepository.GetNotDeletedAsync(taskId);
        ApplyGoals(goals);
    }

    private void ResetGoals() => ApplyGoals([]);

    private void ApplyGoals(IReadOnlyList<GoalDb> goals)
    {
        //Move to config
        const int maxGoalsCount = 3;
        Goals.Clear();
        foreach (var goal in goals.OrderBy(g => g.Id).Take(maxGoalsCount))
            Goals.Add(new GoalEditItem { Id = goal.Id, Text = goal.Text });

        while (Goals.Count < maxGoalsCount)
            Goals.Add(new GoalEditItem());
    }

    public static DateTime ToDueDateUtc(DateTime localDate)
    {
        var endOfLocalDay = DateTime.SpecifyKind(localDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local);
        return endOfLocalDay.ToUniversalTime();
    }

    public static DateTime? ToDueDateUtc(DateTime? localDate) =>
        localDate is null ? null : ToDueDateUtc(localDate.Value);

    public static DateTime ToDueDateLocal(DateTime dueDateUtc)
    {
        var utc = dueDateUtc.Kind == DateTimeKind.Utc
            ? dueDateUtc
            : DateTime.SpecifyKind(dueDateUtc, DateTimeKind.Utc);
        return utc.ToLocalTime().Date;
    }

    public static DateTime? ToDueDateLocal(DateTime? dueDateUtc) =>
        dueDateUtc is null ? null : ToDueDateLocal(dueDateUtc.Value);
}
