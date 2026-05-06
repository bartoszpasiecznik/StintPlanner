namespace LMUStintPlanner.ViewModels;

/// <summary>
/// Avalonia-compatible replacement for WPF CommandManager.
/// Provides a simple RequerySuggested event that commands can hook into.
/// </summary>
public static class CommandManagerImpl
{
    public static event EventHandler? RequerySuggested;

    public static void InvalidateRequerySuggested()
    {
        RequerySuggested?.Invoke(null, EventArgs.Empty);
    }
}
