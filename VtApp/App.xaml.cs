using System.Windows;
using Database;

namespace VtApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DatabaseInitializer.Initialize();
        base.OnStartup(e);
    }
}
