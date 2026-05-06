namespace LMUStintPlanner.ViewModels;

public sealed class DriverPlan(string name, string lapTimeText, string timeZoneId, int maxStints, double energyPerLap)
    : BindableBase
{
    private string _name = name;
    private string _lapTimeText = lapTimeText;
    private string _timeZoneId = timeZoneId;
    private TimeZoneInfo? _selectedTimeZone = TryFindTimeZone(timeZoneId);
    private int _maxStints = maxStints;
    private int _assignedStints;
    private double _energyPerLap = energyPerLap;
    private int _colorIndex;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string LapTimeText
    {
        get => _lapTimeText;
        set => SetProperty(ref _lapTimeText, value);
    }

    public string TimeZoneId
    {
        get => _timeZoneId;
        set
        {
            if (SetProperty(ref _timeZoneId, value))
            {
                try
                {
                    SelectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(value);
                }
                catch
                {
                    SelectedTimeZone = null;
                }
            }
        }
    }

    public TimeZoneInfo? SelectedTimeZone
    {
        get => _selectedTimeZone;
        set
        {
            if (SetProperty(ref _selectedTimeZone, value))
            {
                TimeZoneId = value?.Id ?? TimeZoneInfo.Local.Id;
            }
        }
    }

    public int MaxStints
    {
        get => _maxStints;
        set => SetProperty(ref _maxStints, Math.Max(0, value));
    }

    public int AssignedStints
    {
        get => _assignedStints;
        set => SetProperty(ref _assignedStints, Math.Max(0, value));
    }

    public double EnergyPerLap
    {
        get => _energyPerLap;
        set => SetProperty(ref _energyPerLap, Math.Max(0, value));
    }

    public int ColorIndex
    {
        get => _colorIndex;
        set => SetProperty(ref _colorIndex, Math.Max(0, value));
    }

    public TimeSpan ParsedLapTime => TryGetLapTime(out TimeSpan lapTime) ? lapTime : TimeSpan.Zero;

    public bool TryGetLapTime(out TimeSpan lapTime)
    {
        return LapTimeParser.TryParseDriverLapTime(LapTimeText, out lapTime);
    }

    private static TimeZoneInfo? TryFindTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return null;
        }
    }
}
