using System.Globalization;

namespace LMUStintPlanner.ViewModels;

public static class RaceTimeService
{
    public static TimeSpan GetRaceDuration(int raceLengthHours, int raceLengthMinutes)
    {
        return TimeSpan.FromHours(Math.Max(0, raceLengthHours)) +
               TimeSpan.FromMinutes(Math.Clamp(raceLengthMinutes, 0, 59));
    }

    public static bool TryGetRaceStart(string raceStartText, string raceTimeZoneId, out DateTimeOffset raceStart)
    {
        raceStart = default;

        if (!DateTime.TryParse(raceStartText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces,
                out DateTime parsed))
        {
            return false;
        }

        try
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(raceTimeZoneId);
            raceStart = new DateTimeOffset(parsed, zone.GetUtcOffset(parsed));
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }

    public static bool TryConvertDriverWindow(
        string timeZoneId,
        DateTimeOffset raceStart,
        DateTimeOffset raceEnd,
        out string localWindow)
    {
        localWindow = string.Empty;

        try
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTimeOffset localStart = TimeZoneInfo.ConvertTime(raceStart, zone);
            DateTimeOffset localEnd = TimeZoneInfo.ConvertTime(raceEnd, zone);
            localWindow = $"{localStart:ddd HH:mm} - {localEnd:HH:mm}";
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
