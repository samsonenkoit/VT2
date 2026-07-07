using System.Windows;
using Database;
using Microsoft.Extensions.DependencyInjection;
using VtApp.DependencyInjection;

namespace VtApp;

public partial class App : Application
{
    private IServiceScope? _scope;

    protected override void OnStartup(StartupEventArgs e)
    {
        DatabaseInitializer.Initialize();

        var services = new ServiceCollection();
        services.AddVtAppServices();

        var serviceProvider = services.BuildServiceProvider();
        _scope = serviceProvider.CreateScope();

        var mainWindow = _scope.ServiceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _scope?.Dispose();
        base.OnExit(e);
    }
}
