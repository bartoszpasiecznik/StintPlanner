namespace LMUStintPlanner.ViewModels;

public sealed class TrackOption(string name, string source)
{
    public string Name { get; } = name;

    public string Source { get; } = source;

    public string DisplayName => $"{Name} ({Source})";
}
