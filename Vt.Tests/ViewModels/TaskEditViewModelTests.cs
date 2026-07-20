using Database.Models;
using Database.Repositories;
using VtApp.Models;
using VtApp.Services;
using VtApp.ViewModels;
using Xunit;

namespace Vt.Tests.ViewModels;

public class TaskEditViewModelTests
{
    [Fact]
    public async Task CanSave_IsFalseWhenTitleIsEmpty()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        await viewModel.PrepareForCreateAsync();

        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task CanSave_IsTrueWhenTitleHasValue()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        await viewModel.PrepareForCreateAsync();
        viewModel.Title = "Задача";

        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task SaveAsync_Create_CallsAddAsync()
    {
        var repository = new FakeTaskRepository([]);
        var saved = false;
        var viewModel = CreateViewModel(repository, onSaved: () => saved = true);

        await viewModel.PrepareForCreateAsync();
        viewModel.Title = "  Новая задача  ";
        viewModel.Description = "  Комментарий  ";
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(saved);
        var added = Assert.Single(repository.AddedTasks);
        Assert.Equal("Новая задача", added.Title);
        Assert.Equal("Комментарий", added.Description);
        Assert.Equal(TaskImportance.Medium, added.Importance);
        Assert.Equal(TaskDelayRisk.Low, added.DelayRisk);
        Assert.Equal(TaskDifficulty.Low, added.Difficulty);
        Assert.Equal(TaskUrgency.Medium, added.Urgency);
        Assert.Equal(TaskPriority.Medium, added.Priority);
        Assert.Equal(0, added.ProgressPercent);
        Assert.Equal(TaskEditViewModel.ToDueDateUtc(DateTime.Today.AddDays(3)), added.DueDateUtc);
    }

    [Fact]
    public async Task PrepareForCreateAsync_SetsDueDateToTodayPlusThreeDays()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));

        await viewModel.PrepareForCreateAsync();

        Assert.Equal(DateTime.Today.AddDays(3), viewModel.DueDate);
        Assert.False(viewModel.IsLoading);
        Assert.True(viewModel.IsUiEnabled);
    }

    [Fact]
    public async Task PrepareForEditAsync_LoadsDueDateAsLocalDate()
    {
        var localDate = new DateTime(2026, 6, 15);
        var dueDateUtc = TaskEditViewModel.ToDueDateUtc(localDate);
        var existing = new TaskDb
        {
            Id = 3,
            Title = "Редактируемая",
            DueDateUtc = dueDateUtc,
            ProgressPercent = 0,
            Importance = TaskImportance.High,
            DelayRisk = TaskDelayRisk.Medium,
            Difficulty = TaskDifficulty.High,
            Urgency = TaskUrgency.Low,
            Priority = TaskPriority.Urgent,
        };
        var viewModel = CreateViewModel(new FakeTaskRepository([existing]));

        var prepared = await viewModel.PrepareForEditAsync(3);

        Assert.True(prepared);
        Assert.Equal(localDate, viewModel.DueDate);
    }

    [Fact]
    public async Task SaveAsync_Edit_PersistsDueDateUtcAsEndOfLocalDay()
    {
        var existing = new TaskDb
        {
            Id = 7,
            Title = "Старое",
            Description = "Старое описание",
            DueDateUtc = TaskEditViewModel.ToDueDateUtc(new DateTime(2026, 4, 1)),
            ProgressPercent = 10,
            Importance = TaskImportance.Medium,
            DelayRisk = TaskDelayRisk.Low,
            Difficulty = TaskDifficulty.Low,
            Urgency = TaskUrgency.High,
            Priority = TaskPriority.Urgent,
        };
        var repository = new FakeTaskRepository([existing]);
        var viewModel = CreateViewModel(repository);

        await viewModel.PrepareForEditAsync(7);
        var newLocalDate = new DateTime(2026, 5, 20);
        viewModel.DueDate = newLocalDate;
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(TaskEditViewModel.ToDueDateUtc(newLocalDate), existing.DueDateUtc);
    }

    [Fact]
    public async Task SaveAsync_Create_PersistsComputedPriorityFromFactors()
    {
        var repository = new FakeTaskRepository([]);
        var viewModel = CreateViewModel(repository);

        await viewModel.PrepareForCreateAsync();
        viewModel.Title = "Срочная";
        viewModel.Importance = TaskImportance.High;
        viewModel.Urgency = TaskUrgency.High;
        viewModel.DelayRisk = TaskDelayRisk.Low;
        viewModel.Difficulty = TaskDifficulty.Low;
        await viewModel.SaveCommand.ExecuteAsync(null);

        var added = Assert.Single(repository.AddedTasks);
        Assert.Equal(TaskPriority.Critical, added.Priority);
        Assert.Equal(TaskPriority.Critical, viewModel.Priority);
    }

    [Fact]
    public async Task ChangingFactors_RecalculatesPriority()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        await viewModel.PrepareForCreateAsync();

        viewModel.Importance = TaskImportance.Critical;
        viewModel.Urgency = TaskUrgency.High;
        viewModel.DelayRisk = TaskDelayRisk.High;
        viewModel.Difficulty = TaskDifficulty.High;

        Assert.Equal(TaskPriority.Critical, viewModel.Priority);
        Assert.Equal("Критический", viewModel.PriorityDisplay);
    }

    [Fact]
    public async Task SaveAsync_Edit_CallsUpdateAsync()
    {
        var existing = new TaskDb
        {
            Id = 7,
            Title = "Старое",
            Description = "Старое описание",
            DueDateUtc = TaskEditViewModel.ToDueDateUtc(new DateTime(2026, 4, 1)),
            ProgressPercent = 10,
            Importance = TaskImportance.Medium,
            DelayRisk = TaskDelayRisk.Low,
            Difficulty = TaskDifficulty.Low,
            Urgency = TaskUrgency.High,
            Priority = TaskPriority.Urgent,
        };
        var repository = new FakeTaskRepository([existing]);
        var saved = false;
        var viewModel = CreateViewModel(repository, onSaved: () => saved = true);

        await viewModel.PrepareForEditAsync(7);
        viewModel.Title = "Новое";
        viewModel.Description = "Новое описание";
        viewModel.Importance = TaskImportance.High;
        viewModel.Urgency = TaskUrgency.High;
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(saved);
        Assert.Equal("Новое", existing.Title);
        Assert.Equal("Новое описание", existing.Description);
        Assert.Equal(10, existing.ProgressPercent);
        Assert.Equal(TaskImportance.High, existing.Importance);
        Assert.Equal(TaskUrgency.High, existing.Urgency);
        Assert.Equal(TaskPriority.Critical, existing.Priority);
        Assert.Equal(TaskEditViewModel.ToDueDateUtc(new DateTime(2026, 4, 1)), existing.DueDateUtc);
        Assert.Single(repository.UpdatedTasks);
    }

    [Fact]
    public void Cancel_InvokesCallback()
    {
        var cancelled = false;
        var viewModel = CreateViewModel(new FakeTaskRepository([]), onCancelled: () => cancelled = true);

        viewModel.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }

    [Fact]
    public async Task PrepareForEditAsync_SetsEditMode()
    {
        var existing = new TaskDb
        {
            Id = 3,
            Title = "Редактируемая",
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Importance = TaskImportance.High,
            DelayRisk = TaskDelayRisk.Medium,
            Difficulty = TaskDifficulty.High,
            Urgency = TaskUrgency.Low,
            Priority = TaskPriority.Urgent,
        };
        var viewModel = CreateViewModel(new FakeTaskRepository([existing]));

        var prepared = await viewModel.PrepareForEditAsync(3);

        Assert.True(prepared);
        Assert.True(viewModel.IsEditMode);
        Assert.False(viewModel.IsLoading);
        Assert.True(viewModel.IsUiEnabled);
        Assert.Equal("Редактируемая", viewModel.Title);
        Assert.Equal("Редактирование задачи", viewModel.PageTitle);
        Assert.Equal(TaskImportance.High, viewModel.Importance);
        Assert.Equal(TaskDelayRisk.Medium, viewModel.DelayRisk);
        Assert.Equal(TaskDifficulty.High, viewModel.Difficulty);
        Assert.Equal(TaskUrgency.Low, viewModel.Urgency);
        Assert.Equal(TaskPriority.Urgent, viewModel.Priority);
        Assert.Equal(0, viewModel.ProgressPercent);
        Assert.Equal("Выполнено (0%):", viewModel.ProgressLabel);
    }

    [Fact]
    public async Task PrepareForEditAsync_LoadsSubtasks()
    {
        var task = new TaskDb
        {
            Id = 1,
            Title = "Задача",
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var subtaskRepository = new FakeSubtaskRepository(
        [
            new SubtaskDb { Id = 10, Description = "Подзадача 1", TaskId = 1 },
            new SubtaskDb { Id = 11, Description = "Подзадача 2", TaskId = 1 },
        ]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), subtaskRepository);

        await viewModel.PrepareForEditAsync(1);

        Assert.Equal(2, viewModel.Subtasks.Count);
        Assert.Equal("Подзадача 1", viewModel.Subtasks[0].Description);
        Assert.Equal(10, viewModel.Subtasks[0].Id);
        Assert.Equal("Подзадача 2", viewModel.Subtasks[1].Description);
    }

    [Fact]
    public async Task AddSubtaskCommand_AddsToCollection()
    {
        var subtaskRepository = new FakeSubtaskRepository([]);
        var viewModel = CreateViewModel(new FakeTaskRepository([]), subtaskRepository);
        await viewModel.PrepareForCreateAsync();
        viewModel.NewSubtaskDescription = "  Новая подзадача  ";

        viewModel.AddSubtaskCommand.Execute(null);

        var subtask = Assert.Single(viewModel.Subtasks);
        Assert.Equal("Новая подзадача", subtask.Description);
        Assert.Equal(0, subtask.Id);
        Assert.Equal(0, subtask.ProgressPercent);
        Assert.Null(subtask.DueDate);
        Assert.Equal(string.Empty, viewModel.NewSubtaskDescription);
        Assert.Equal("Подзадачи · 0/1", viewModel.SubtasksProgressLabel);
        Assert.Empty(subtaskRepository.AddedSubtasks);
    }

    [Fact]
    public async Task PrepareForEditAsync_LoadsSubtaskExtendedFields()
    {
        var localDue = new DateTime(2026, 7, 20);
        var task = new TaskDb
        {
            Id = 1,
            Title = "Задача",
            DueDateUtc = TaskEditViewModel.ToDueDateUtc(DateTime.Today.AddDays(3)),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var subtaskRepository = new FakeSubtaskRepository(
        [
            new SubtaskDb
            {
                Id = 10,
                Description = "Подзадача 1",
                TaskId = 1,
                DueDateUtc = TaskEditViewModel.ToDueDateUtc(localDue),
                ProgressPercent = 100,
            },
        ]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), subtaskRepository);

        await viewModel.PrepareForEditAsync(1);

        var subtask = Assert.Single(viewModel.Subtasks);
        Assert.Equal("Подзадача 1", subtask.Description);
        Assert.Equal(localDue, subtask.DueDate);
        Assert.Equal(100, subtask.ProgressPercent);
    }

    [Fact]
    public async Task SaveAsync_Create_PersistsSubtasks()
    {
        var taskRepository = new FakeTaskRepository([]);
        var subtaskRepository = new FakeSubtaskRepository([]);
        var viewModel = CreateViewModel(taskRepository, subtaskRepository);

        await viewModel.PrepareForCreateAsync();
        viewModel.Title = "Задача";
        viewModel.Subtasks.Add(new SubtaskEditItem { Description = "Шаг 1" });
        viewModel.Subtasks.Add(new SubtaskEditItem { Description = "Шаг 2" });
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(2, subtaskRepository.AddedSubtasks.Count);
        Assert.All(subtaskRepository.AddedSubtasks, s => Assert.Equal(1, s.TaskId));
        Assert.Equal("Шаг 1", subtaskRepository.AddedSubtasks[0].Description);
        Assert.Equal("Шаг 2", subtaskRepository.AddedSubtasks[1].Description);
    }

    [Fact]
    public async Task SaveAsync_Edit_UpdatesAndAddsSubtasks()
    {
        var task = new TaskDb
        {
            Id = 5,
            Title = "Задача",
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var subtaskRepository = new FakeSubtaskRepository(
        [
            new SubtaskDb { Id = 20, Description = "Старая", TaskId = 5 },
        ]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), subtaskRepository);

        await viewModel.PrepareForEditAsync(5);
        viewModel.Subtasks[0].Description = "Обновлённая";
        viewModel.Subtasks.Add(new SubtaskEditItem { Description = "Новая" });
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Single(subtaskRepository.UpdatedSubtasks);
        Assert.Equal("Обновлённая", subtaskRepository.UpdatedSubtasks[0].Description);
        Assert.Equal(20, subtaskRepository.UpdatedSubtasks[0].Id);

        var added = Assert.Single(subtaskRepository.AddedSubtasks);
        Assert.Equal("Новая", added.Description);
        Assert.Equal(5, added.TaskId);
    }

    [Fact]
    public async Task PrepareForCreateAsync_DisablesFileManagement()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        await viewModel.PrepareForCreateAsync();

        Assert.False(viewModel.CanManageFiles);
        Assert.False(viewModel.AddFileCommand.CanExecute(null));
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task PrepareForEditAsync_LoadsFiles()
    {
        var task = new TaskDb
        {
            Id = 2,
            Title = "Задача",
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var fileService = new FakeTaskFileService(
        [
            new TaskFileItem { Id = 1, FileName = "report.pdf" },
        ]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), fileService: fileService);

        await viewModel.PrepareForEditAsync(2);

        Assert.True(viewModel.CanManageFiles);
        var file = Assert.Single(viewModel.Files);
        Assert.Equal("report.pdf", file.FileName);
    }

    [Fact]
    public async Task DeleteFileCommand_RemovesFileFromCollection()
    {
        var task = new TaskDb
        {
            Id = 4,
            Title = "Задача",
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var file = new TaskFileItem { Id = 9, FileName = "notes.txt" };
        var fileService = new FakeTaskFileService([file]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), fileService: fileService);

        await viewModel.PrepareForEditAsync(4);
        await viewModel.DeleteFileCommand.ExecuteAsync(file);

        Assert.Empty(viewModel.Files);
        Assert.Single(fileService.DeletedFileIds);
        Assert.Equal(9, fileService.DeletedFileIds[0]);
    }

    [Fact]
    public async Task PrepareForCreateAsync_ResetsGoalsToThreeEmptySlots()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        viewModel.Goals.Add(new GoalEditItem { Text = "Старая" });

        await viewModel.PrepareForCreateAsync();

        Assert.Equal(3, viewModel.Goals.Count);
        Assert.All(viewModel.Goals, g => Assert.Equal(string.Empty, g.Text));
    }

    [Fact]
    public async Task PrepareForEditAsync_ResetsGoalsToThreeEmptySlots()
    {
        var existing = new TaskDb
        {
            Id = 12,
            Title = "Задача",
            DueDateUtc = TaskEditViewModel.ToDueDateUtc(DateTime.Today.AddDays(3)),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var viewModel = CreateViewModel(new FakeTaskRepository([existing]));
        viewModel.Goals.Clear();
        viewModel.Goals.Add(new GoalEditItem { Text = "A" });
        viewModel.Goals.Add(new GoalEditItem { Text = "B" });

        await viewModel.PrepareForEditAsync(12);

        Assert.Equal(3, viewModel.Goals.Count);
        Assert.All(viewModel.Goals, g => Assert.Equal(string.Empty, g.Text));
    }

    [Fact]
    public async Task PrepareForCreateAsync_ClearsSubtasks()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        viewModel.Subtasks.Add(new SubtaskEditItem { Description = "Остаток" });
        viewModel.NewSubtaskDescription = "Черновик";

        await viewModel.PrepareForCreateAsync();

        Assert.Empty(viewModel.Subtasks);
        Assert.Equal(string.Empty, viewModel.NewSubtaskDescription);
        Assert.Equal("Подзадачи · 0/0", viewModel.SubtasksProgressLabel);
    }

    [Fact]
    public async Task SaveAsync_Create_PersistsSubtaskExtendedFields()
    {
        var taskRepository = new FakeTaskRepository([]);
        var subtaskRepository = new FakeSubtaskRepository([]);
        var viewModel = CreateViewModel(taskRepository, subtaskRepository);
        var due = new DateTime(2026, 8, 1);

        await viewModel.PrepareForCreateAsync();
        viewModel.Title = "Задача";
        viewModel.Subtasks.Add(new SubtaskEditItem
        {
            Description = "  Шаг с комментарием  ",
            DueDate = due,
            ProgressPercent = 67,
        });
        await viewModel.SaveCommand.ExecuteAsync(null);

        var added = Assert.Single(subtaskRepository.AddedSubtasks);
        Assert.Equal("Шаг с комментарием", added.Description);
        Assert.Equal(TaskEditViewModel.ToDueDateUtc(due), added.DueDateUtc);
        Assert.Equal(67, added.ProgressPercent);
    }

    [Fact]
    public async Task SaveAsync_Edit_SoftDeletesRemovedSubtasks()
    {
        var task = new TaskDb
        {
            Id = 5,
            Title = "Задача",
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var subtaskRepository = new FakeSubtaskRepository(
        [
            new SubtaskDb { Id = 20, Description = "Старая", TaskId = 5 },
            new SubtaskDb { Id = 21, Description = "Удаляемая", TaskId = 5 },
        ]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), subtaskRepository);

        await viewModel.PrepareForEditAsync(5);
        viewModel.RemoveSubtaskCommand.Execute(viewModel.Subtasks[1]);
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal([21], subtaskRepository.SoftDeletedIds);
        Assert.Single(viewModel.Subtasks);
        Assert.Equal("Подзадачи · 0/1", viewModel.SubtasksProgressLabel);
    }

    [Fact]
    public void SubtaskIsDone_AndSetProgress_UpdateProgressPercent()
    {
        var item = new SubtaskEditItem { Description = "Шаг", ProgressPercent = 0 };

        item.SetProgressCommand.Execute(67);
        Assert.Equal(67, item.ProgressPercent);
        Assert.False(item.IsDone);

        item.IsDone = true;
        Assert.Equal(100, item.ProgressPercent);
        Assert.True(item.IsDone);

        item.IsDone = false;
        Assert.Equal(0, item.ProgressPercent);
    }

    [Fact]
    public async Task SubtasksProgressLabel_UpdatesWhenProgressChanges()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        await viewModel.PrepareForCreateAsync();
        viewModel.Subtasks.Add(new SubtaskEditItem { Description = "A", ProgressPercent = 0 });
        viewModel.Subtasks.Add(new SubtaskEditItem { Description = "B", ProgressPercent = 100 });

        Assert.Equal("Подзадачи · 1/2", viewModel.SubtasksProgressLabel);

        viewModel.Subtasks[0].ProgressPercent = 100;
        Assert.Equal("Подзадачи · 2/2", viewModel.SubtasksProgressLabel);
    }

    private static TaskEditViewModel CreateViewModel(
        FakeTaskRepository repository,
        FakeSubtaskRepository? subtaskRepository = null,
        FakeTaskFileService? fileService = null,
        Action? onSaved = null,
        Action? onCancelled = null)
    {
        var viewModel = new TaskEditViewModel(
            repository,
            subtaskRepository ?? new FakeSubtaskRepository([]),
            fileService ?? new FakeTaskFileService([]));
        viewModel.Configure(
            onSaved ?? (() => { }),
            onCancelled ?? (() => { }));
        return viewModel;
    }

    private sealed class FakeTaskRepository(IReadOnlyList<TaskDb> tasks) : ITaskRepository
    {
        public List<TaskDb> AddedTasks { get; } = [];
        public List<TaskDb> UpdatedTasks { get; } = [];

        public Task<IReadOnlyList<TaskDb>> GetAllNotDeletedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskDb>>(tasks.Where(t => t.DeletedAtUtc is null).ToList());

        public Task<TaskDb?> GetAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(tasks.FirstOrDefault(t => t.Id == id));

        public Task<TaskDb> AddAsync(TaskDb task, CancellationToken cancellationToken = default)
        {
            task.Id = tasks.Count + AddedTasks.Count + 1;
            AddedTasks.Add(task);
            return Task.FromResult(task);
        }

        public Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default)
        {
            UpdatedTasks.Add(task);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSubtaskRepository(IReadOnlyList<SubtaskDb> subtasks) : ISubtaskRepository
    {
        public List<SubtaskDb> AddedSubtasks { get; } = [];
        public List<SubtaskDb> UpdatedSubtasks { get; } = [];
        public List<int> SoftDeletedIds { get; } = [];

        public Task<IReadOnlyList<SubtaskDb>> GetNotDeletedAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SubtaskDb>>(
                subtasks.Where(s => s.TaskId == taskId && s.DeletedAtUtc is null && !SoftDeletedIds.Contains(s.Id)).ToList());

        public Task<SubtaskDb> AddAsync(SubtaskDb subtask, CancellationToken cancellationToken = default)
        {
            subtask.Id = subtasks.Count + AddedSubtasks.Count + 1;
            AddedSubtasks.Add(subtask);
            return Task.FromResult(subtask);
        }

        public Task UpdateAsync(SubtaskDb subtask, CancellationToken cancellationToken = default)
        {
            UpdatedSubtasks.Add(subtask);
            return Task.CompletedTask;
        }

        public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            SoftDeletedIds.Add(id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTaskFileService(IReadOnlyList<TaskFileItem> files) : ITaskFileService
    {
        public List<int> DeletedFileIds { get; } = [];

        public Task<IReadOnlyList<TaskFileItem>> GetFilesAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskFileItem>>(files.ToList());

        public Task<TaskFileItem> AddFileAsync(int taskId, string sourceFilePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteFileAsync(int fileId, CancellationToken cancellationToken = default)
        {
            DeletedFileIds.Add(fileId);
            return Task.CompletedTask;
        }
    }
}
