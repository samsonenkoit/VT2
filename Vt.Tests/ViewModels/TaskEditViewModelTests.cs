using Database.Models;
using Database.Repositories;
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
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(saved);
        var added = Assert.Single(repository.AddedTasks);
        Assert.Equal("Новая задача", added.Title);
        Assert.Equal(TaskPriority.Medium, added.Priority);
        Assert.Equal(0, added.ProgressPercent);
        Assert.Equal(DateTime.Today, added.DueDate.Date);
    }

    [Fact]
    public async Task SaveAsync_Edit_CallsUpdateAsync()
    {
        var existing = new TaskDb
        {
            Id = 7,
            Title = "Старое",
            DueDate = new DateTime(2026, 4, 1),
            ProgressPercent = 10,
            Priority = TaskPriority.Urgent,
        };
        var repository = new FakeTaskRepository([existing]);
        var saved = false;
        var viewModel = CreateViewModel(repository, onSaved: () => saved = true);

        await viewModel.PrepareForEditAsync(7);
        viewModel.Title = "Новое";
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(saved);
        Assert.Equal("Новое", existing.Title);
        Assert.Equal(10, existing.ProgressPercent);
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
            DueDate = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        var viewModel = CreateViewModel(new FakeTaskRepository([existing]));

        var prepared = await viewModel.PrepareForEditAsync(3);

        Assert.True(prepared);
        Assert.True(viewModel.IsEditMode);
        Assert.Equal("Редактируемая", viewModel.Title);
        Assert.Equal("Редактирование задачи", viewModel.PageTitle);
    }

    private static TaskEditViewModel CreateViewModel(
        FakeTaskRepository repository,
        Action? onSaved = null,
        Action? onCancelled = null)
    {
        var viewModel = new TaskEditViewModel(repository);
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
            Task.FromResult<IReadOnlyList<TaskDb>>(tasks.Where(t => t.DeletedAt is null).ToList());

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
}
