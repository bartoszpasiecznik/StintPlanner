namespace LMUStintPlanner.ViewModels;

public sealed partial class MainViewModel
{
    private TimeSpan GetRaceDuration()
    {
        return RaceTimeService.GetRaceDuration(RaceLengthHours, RaceLengthMinutes);
    }

    private bool TryGetRaceStart(out DateTimeOffset raceStart)
    {
        return RaceTimeService.TryGetRaceStart(RaceStartText, RaceTimeZoneId, out raceStart);
    }

    private void UpdateRaceStartSummary()
    {
        if (!TryGetRaceStart(out DateTimeOffset raceStart))
        {
            RaceStartSummary = "Enter a valid race start and race time zone.";
            return;
        }

        RaceStartSummary =
            $"Race start: {raceStart:yyyy-MM-dd HH:mm zzz} | UTC: {raceStart.UtcDateTime:yyyy-MM-dd HH:mm}";
    }
}
