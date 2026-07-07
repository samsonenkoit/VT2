using Database;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VtApp.ViewModels;

namespace VtApp.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVtAppServices(this IServiceCollection services)
    {
        var databasePath = new AppDataDatabasePathProvider().GetDatabaseFilePath();

        services.AddDbContext<VtDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddTransient<TasksViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        return services;
    }
}
