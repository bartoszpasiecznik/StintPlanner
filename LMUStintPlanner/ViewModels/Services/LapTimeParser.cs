using System.Globalization;

namespace LMUStintPlanner.ViewModels;

public static class LapTimeParser
{
    private static readonly string[] FlexibleFormats =
    [
        @"m\:ss\.fff",
        @"mm\:ss\.fff",
        @"m\:ss",
        @"mm\:ss",
        @"h\:mm\:ss",
        @"hh\:mm\:ss",
        @"h\:mm\:ss\.fff",
        @"hh\:mm\:ss\.fff"
    ];

    private static readonly string[] DriverFormats =
    [
        @"m\:ss\.fff",
        @"mm\:ss\.fff",
        @"h\:mm\:ss",
        @"hh\:mm\:ss",
        @"h\:mm\:ss\.fff",
        @"hh\:mm\:ss\.fff"
    ];

    public static bool TryParseFlexibleLapTime(string? text, out TimeSpan lapTime)
    {
        lapTime = TimeSpan.Zero;
        return !string.IsNullOrWhiteSpace(text) &&
               TimeSpan.TryParseExact(text.Trim(), FlexibleFormats, CultureInfo.InvariantCulture, out lapTime);
    }

    public static bool TryParseDriverLapTime(string? text, out TimeSpan lapTime)
    {
        lapTime = TimeSpan.Zero;
        return !string.IsNullOrWhiteSpace(text) &&
               TimeSpan.TryParseExact(text.Trim(), DriverFormats, CultureInfo.InvariantCulture, out lapTime);
    }

    public static string FormatSignedTime(TimeSpan value)
    {
        string sign = value < TimeSpan.Zero ? "-" : string.Empty;
        TimeSpan absolute = value.Duration();

        return absolute.Hours > 0 ? $@"{sign}{absolute:hh\:mm\:ss\.fff}" : $@"{sign}{absolute:mm\:ss\.fff}";
    }
}
