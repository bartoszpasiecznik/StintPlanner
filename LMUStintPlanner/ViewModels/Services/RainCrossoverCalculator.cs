namespace LMUStintPlanner.ViewModels;

public static class RainCrossoverCalculator
{
    public static string BuildSummary(
        string currentDryLapTimeText,
        string currentWetLapTimeText,
        int remainingStintLaps,
        double remainingStintFuel,
        double fuelPerLap)
    {
        if (!LapTimeParser.TryParseFlexibleLapTime(currentDryLapTimeText, out TimeSpan dryLap))
        {
            return "Enter a valid dry lap time.";
        }

        if (!LapTimeParser.TryParseFlexibleLapTime(currentWetLapTimeText, out TimeSpan wetLap))
        {
            return "Enter a valid wet lap time.";
        }

        if (remainingStintLaps <= 0)
        {
            return "No laps left in the stint.";
        }

        TimeSpan delta = wetLap - dryLap;
        TimeSpan totalDelta = TimeSpan.FromTicks(delta.Ticks * remainingStintLaps);
        string fuelWindow = fuelPerLap > 0
            ? $"{Math.Floor(remainingStintFuel / fuelPerLap):0} dry laps of fuel at current burn"
            : "fuel burn not set";

        return
            $"Per-lap crossover delta: {LapTimeParser.FormatSignedTime(delta)} | " +
            $"Over {remainingStintLaps} laps: {LapTimeParser.FormatSignedTime(totalDelta)} | " +
            $"Fuel window: {fuelWindow}";
    }

    public static string BuildRecommendation(
        string currentDryLapTimeText,
        string currentWetLapTimeText,
        int remainingStintLaps)
    {
        if (!LapTimeParser.TryParseFlexibleLapTime(currentDryLapTimeText, out TimeSpan dryLap) ||
            !LapTimeParser.TryParseFlexibleLapTime(currentWetLapTimeText, out TimeSpan wetLap) ||
            remainingStintLaps <= 0)
        {
            return "Add valid lap times and laps left to calculate the crossover.";
        }

        TimeSpan delta = wetLap - dryLap;
        if (delta < TimeSpan.Zero)
        {
            return $"Wet pace is quicker by {LapTimeParser.FormatSignedTime(-delta)} per lap. Crossing now favors wets.";
        }

        return delta == TimeSpan.Zero
            ? "Dry and wet pace are equal. Cross based on traffic, radar, and pit window."
            : $"Dry pace is quicker by {LapTimeParser.FormatSignedTime(delta)} per lap. Staying out favors dries for now.";
    }
}
