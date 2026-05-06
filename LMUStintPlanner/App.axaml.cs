using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace LMUStintPlanner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public override void Initialize()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.MainWindow();
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}