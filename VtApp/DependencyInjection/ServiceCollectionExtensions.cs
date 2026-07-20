using Database;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VtApp.Services;
using VtApp.ViewModels;

namespace VtApp.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVtAppServices(this IServiceCollection services)
    {
        var pathProvider = new AppDataPathProvider();
        services.AddSingleton<IAppDataPathProvider>(pathProvider);

        services.AddDbContext<VtDbContext>(options =>
            options.UseSqlite($"Data Source={pathProvider.GetDatabaseFilePath()}"));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ISubtaskRepository, SubtaskRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<ITaskFileRepository, TaskFileRepository>();
        services.AddScoped<ITaskFileService, TaskFileService>();
        services.AddTransient<TaskEditViewModel>();
        services.AddTransient<TasksViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        return services;
    }
}
