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
    public void CanSave_IsFalseWhenTitleIsEmpty()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        viewModel.PrepareForCreate();

        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void CanSave_IsTrueWhenTitleHasValue()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        viewModel.PrepareForCreate();
        viewModel.Title = "Задача";

        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task SaveAsync_Create_CallsAddAsync()
    {
        var repository = new FakeTaskRepository([]);
        var saved = false;
        var viewModel = CreateViewModel(repository, onSaved: () => saved = true);

        viewModel.PrepareForCreate();
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
        Assert.Equal(DateTime.Today, added.DueDateUtc.Date);
    }

    [Fact]
    public async Task SaveAsync_Create_PersistsComputedPriorityFromFactors()
    {
        var repository = new FakeTaskRepository([]);
        var viewModel = CreateViewModel(repository);

        viewModel.PrepareForCreate();
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
    public void ChangingFactors_RecalculatesPriority()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        viewModel.PrepareForCreate();

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
            DueDateUtc = new DateTime(2026, 4, 1),
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
            new SubtaskDb { Id = 10, Title = "Подзадача 1", TaskId = 1 },
            new SubtaskDb { Id = 11, Title = "Подзадача 2", TaskId = 1 },
        ]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), subtaskRepository);

        await viewModel.PrepareForEditAsync(1);

        Assert.Equal(2, viewModel.Subtasks.Count);
        Assert.Equal("Подзадача 1", viewModel.Subtasks[0].Title);
        Assert.Equal(10, viewModel.Subtasks[0].Id);
        Assert.Equal("Подзадача 2", viewModel.Subtasks[1].Title);
    }

    [Fact]
    public void AddSubtaskCommand_AddsToCollection()
    {
        var subtaskRepository = new FakeSubtaskRepository([]);
        var viewModel = CreateViewModel(new FakeTaskRepository([]), subtaskRepository);
        viewModel.PrepareForCreate();
        viewModel.NewSubtaskTitle = "  Новая подзадача  ";

        viewModel.AddSubtaskCommand.Execute(null);

        var subtask = Assert.Single(viewModel.Subtasks);
        Assert.Equal("Новая подзадача", subtask.Title);
        Assert.Equal(0, subtask.Id);
        Assert.Equal(string.Empty, viewModel.NewSubtaskTitle);
        Assert.Empty(subtaskRepository.AddedSubtasks);
    }

    [Fact]
    public async Task SaveAsync_Create_PersistsSubtasks()
    {
        var taskRepository = new FakeTaskRepository([]);
        var subtaskRepository = new FakeSubtaskRepository([]);
        var viewModel = CreateViewModel(taskRepository, subtaskRepository);

        viewModel.PrepareForCreate();
        viewModel.Title = "Задача";
        viewModel.Subtasks.Add(new SubtaskEditItem { Title = "Шаг 1" });
        viewModel.Subtasks.Add(new SubtaskEditItem { Title = "Шаг 2" });
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(2, subtaskRepository.AddedSubtasks.Count);
        Assert.All(subtaskRepository.AddedSubtasks, s => Assert.Equal(1, s.TaskId));
        Assert.Equal("Шаг 1", subtaskRepository.AddedSubtasks[0].Title);
        Assert.Equal("Шаг 2", subtaskRepository.AddedSubtasks[1].Title);
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
            new SubtaskDb { Id = 20, Title = "Старая", TaskId = 5 },
        ]);
        var viewModel = CreateViewModel(new FakeTaskRepository([task]), subtaskRepository);

        await viewModel.PrepareForEditAsync(5);
        viewModel.Subtasks[0].Title = "Обновлённая";
        viewModel.Subtasks.Add(new SubtaskEditItem { Title = "Новая" });
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Single(subtaskRepository.UpdatedSubtasks);
        Assert.Equal("Обновлённая", subtaskRepository.UpdatedSubtasks[0].Title);
        Assert.Equal(20, subtaskRepository.UpdatedSubtasks[0].Id);

        var added = Assert.Single(subtaskRepository.AddedSubtasks);
        Assert.Equal("Новая", added.Title);
        Assert.Equal(5, added.TaskId);
    }

    [Fact]
    public void PrepareForCreate_DisablesFileManagement()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        viewModel.PrepareForCreate();

        Assert.False(viewModel.CanManageFiles);
        Assert.False(viewModel.AddFileCommand.CanExecute(null));
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
    public void PrepareForCreate_ClearsSubtasks()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        viewModel.Subtasks.Add(new SubtaskEditItem { Title = "Остаток" });
        viewModel.NewSubtaskTitle = "Черновик";

        viewModel.PrepareForCreate();

        Assert.Empty(viewModel.Subtasks);
        Assert.Equal(string.Empty, viewModel.NewSubtaskTitle);
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

        public Task<IReadOnlyList<TaskDb>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskDb>>(tasks.Where(t => t.DeletedAtUtc is null).ToList());

        public Task<TaskDb?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
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

        public Task<IReadOnlyList<SubtaskDb>> GetNotDeletedAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SubtaskDb>>(
                subtasks.Where(s => s.TaskId == taskId && s.DeletedAtUtc is null).ToList());

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
