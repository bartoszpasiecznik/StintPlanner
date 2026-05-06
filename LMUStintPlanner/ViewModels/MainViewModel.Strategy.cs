namespace LMUStintPlanner.ViewModels;

public sealed partial class MainViewModel
{
    private void GenerateStrategy()
    {
        List<DriverPlan> activeDrivers = Drivers
            .Where(d => !string.IsNullOrWhiteSpace(d.Name) && d.TryGetLapTime(out _) && d.EnergyPerLap > 0)
            .ToList();

        if (activeDrivers.Count == 0)
        {
            SummaryText = "Add at least one driver with a valid lap time and VE per lap to generate the stint plan.";
            WarningText = string.Empty;
            ReplaceStints([]);
            UpdateRaceStartSummary();
            return;
        }

        int maxFuelLaps = ResourceCalculator.GetCapacityLaps(FuelCapacity, FuelPerLap);
        int maxEnergyLaps = activeDrivers.Max(driver => ResourceCalculator.GetCapacityLaps(EnergyCapacity, driver.EnergyPerLap));
        int maxResourceLaps = Math.Min(maxFuelLaps, maxEnergyLaps);

        if (maxResourceLaps < 1)
        {
            SummaryText = "Fuel and VE settings must cover at least one lap.";
            WarningText = string.Empty;
            ReplaceStints([]);
            UpdateRaceStartSummary();
            return;
        }

        TimeSpan raceDuration = GetRaceDuration();
        TimeSpan averageLap = TimeSpan.FromTicks((long)activeDrivers.Average(d => d.ParsedLapTime.Ticks));
        int estimatedLaps = Math.Max(1, (int)Math.Ceiling(raceDuration.TotalSeconds / averageLap.TotalSeconds));

        List<StintPlan> generated = [];
        int remainingLaps = estimatedLaps;
        int driverIndex = 0;
        const int tyreSetNumber = 1;
        Dictionary<DriverPlan, int> driverStintCounts = activeDrivers.ToDictionary(driver => driver, _ => 0);

        while (remainingLaps > 0)
        {
            List<DriverPlan> availableDrivers = activeDrivers
                .Where(driver => driver.MaxStints <= 0 || driverStintCounts[driver] < driver.MaxStints)
                .ToList();

            if (availableDrivers.Count == 0)
            {
                break;
            }

            DriverPlan driver = availableDrivers[driverIndex % availableDrivers.Count];
            int driverEnergyLaps = ResourceCalculator.GetCapacityLaps(EnergyCapacity, driver.EnergyPerLap);
            int stintCap = Math.Min(maxFuelLaps, driverEnergyLaps);
            int stintLaps = Math.Max(1, Math.Min(stintCap, remainingLaps));

            if (remainingLaps > stintLaps && remainingLaps - stintLaps < Math.Min(stintCap, 3))
            {
                stintLaps = Math.Max(1, Math.Min(stintCap, remainingLaps / 2));
            }

            StintPlan stint = new()
            {
                Number = generated.Count + 1,
                Driver = driver,
                Laps = stintLaps,
                AutoFillEnergyForNextStint = true,
                GetQualiTyres = generated.Count == 0,
                RepairSeconds = DefaultRepairSeconds,
                TyreSetNumber = tyreSetNumber
            };

            generated.Add(stint);
            driverStintCounts[driver]++;
            remainingLaps -= stintLaps;
            driverIndex++;

            if (remainingLaps <= 0)
            {
                continue;
            }

            DriverPlan nextDriver = availableDrivers[driverIndex % availableDrivers.Count];
            int nextStintCap = Math.Min(maxFuelLaps, ResourceCalculator.GetCapacityLaps(EnergyCapacity, nextDriver.EnergyPerLap));
            int nextStintLaps = Math.Max(1, Math.Min(nextStintCap, remainingLaps));

            stint.RefuelAmount = Math.Min(FuelCapacity, nextStintLaps * FuelPerLap);
            stint.EnergyAmount = Math.Min(EnergyCapacity, nextStintLaps * nextDriver.EnergyPerLap);
        }

        ReplaceStints(generated);
        RecalculateStrategy();
    }

    private void RecalculateStrategy()
    {
        _isApplyingCalculatedValues = true;
        try
        {
            for (int index = 0; index < Stints.Count; index++)
            {
                Stints[index].Number = index + 1;
            }

            DateTimeOffset raceStart = TryGetRaceStart(out DateTimeOffset parsedRaceStart)
                ? parsedRaceStart
                : new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            UpdateRaceStartSummary();

            List<string> warnings = [];
            TimeSpan totalDrive = TimeSpan.Zero;
            TimeSpan totalPit = TimeSpan.Zero;
            int totalLaps = 0;
            int tyreSet = GetInitialTyreSetNumber();
            Dictionary<DriverPlan, int> driverStintCounts = new();

            foreach (DriverPlan driver in Drivers)
            {
                driver.AssignedStints = 0;
            }

            foreach (StintPlan stint in Stints)
            {
                totalLaps += Math.Max(0, stint.Laps);
                stint.TyreSetNumber = tyreSet;
                stint.CanChangeTyres = stint.ChangeTyres || tyreSet < AvailableTyres;

                if (stint.AutoFillEnergyForNextStint)
                {
                    StintPlan? nextStint = Stints.SkipWhile(current => !ReferenceEquals(current, stint)).Skip(1)
                        .FirstOrDefault();
                    double nextStintEnergyNeed = nextStint is null || nextStint.Driver is null
                        ? 0
                        : nextStint.Laps * nextStint.Driver.EnergyPerLap;
                    stint.EnergyAmount = Math.Min(EnergyCapacity, Math.Max(0, nextStintEnergyNeed));
                }

                if (stint.Driver is null || !stint.Driver.TryGetLapTime(out TimeSpan lapTime))
                {
                    stint.ValidationNote = "Set a valid driver and lap time.";
                    continue;
                }

                if (stint.Driver.EnergyPerLap <= 0)
                {
                    stint.ValidationNote = $"Driver {stint.Driver.Name} needs a valid VE per lap value.";
                    continue;
                }

                if (!driverStintCounts.TryAdd(stint.Driver, 1))
                {
                    driverStintCounts[stint.Driver]++;
                }

                stint.Driver.AssignedStints = driverStintCounts[stint.Driver];

                TimeSpan driveTime = TimeSpan.FromTicks(lapTime.Ticks * Math.Max(0, stint.Laps));
                double serviceSeconds = ResourceCalculator.GetRefillTimeSeconds(stint.RefuelAmount, FuelCapacity, FullRefillSeconds)
                                        + ResourceCalculator.GetRefillTimeSeconds(stint.EnergyAmount, EnergyCapacity, FullRefillSeconds)
                                        + stint.RepairSeconds
                                        + (stint.ChangeTyres ? TyreChangeSeconds : 0);

                TimeSpan serviceTime = TimeSpan.FromSeconds(Math.Max(0, serviceSeconds));
                TimeSpan pitLaneTime = TimeSpan.FromSeconds(Math.Max(0, PitLaneSeconds));
                TimeSpan totalPitTime = pitLaneTime + serviceTime;

                stint.DrivingTime = driveTime;
                stint.PitLaneTime = pitLaneTime;
                stint.ServiceTime = serviceTime;
                stint.TotalPitTime = totalPitTime;
                stint.RaceStart = raceStart + totalDrive + totalPit;
                stint.RaceEnd = stint.RaceStart + driveTime;

                stint.DriverLocalWindowText = RaceTimeService.TryConvertDriverWindow(
                    stint.Driver.TimeZoneId,
                    stint.RaceStart,
                    stint.RaceEnd,
                    out string localWindow)
                    ? localWindow
                    : "Invalid time zone";

                stint.ValidationNote = BuildValidationNote(stint, tyreSet, driverStintCounts[stint.Driver]);
                totalDrive += driveTime;
                totalPit += stint == Stints.Last() ? TimeSpan.Zero : totalPitTime;

                if (stint.ChangeTyres)
                {
                    tyreSet++;
                }
            }

            UpdateAvailableDrivers(driverStintCounts);
            RebuildTimelineRows();

            int totalTyreSetsUsed = Math.Max(0, tyreSet);
            if (totalTyreSetsUsed > AvailableTyres)
            {
                warnings.Add("Planned tyre changes exceed the available tyre sets for this race.");
            }

            List<string> overAssignedDrivers = driverStintCounts
                .Where(entry => entry.Key.MaxStints > 0 && entry.Value > entry.Key.MaxStints)
                .Select(entry => $"{entry.Key.Name} {entry.Value}/{entry.Key.MaxStints}")
                .ToList();
            if (overAssignedDrivers.Count > 0)
            {
                warnings.Add($"Driver stint cap exceeded: {string.Join(", ", overAssignedDrivers)}.");
            }

            TimeSpan raceDuration = GetRaceDuration();
            TimeSpan projectedFinish = totalDrive + totalPit;
            if (projectedFinish > raceDuration)
            {
                warnings.Add($@"Projected finish overruns the race by {(projectedFinish - raceDuration):hh\:mm\:ss}.");
            }
            else if (raceDuration - projectedFinish > TimeSpan.FromMinutes(3))
            {
                warnings.Add($@"Plan leaves {(raceDuration - projectedFinish):hh\:mm\:ss} unused.");
            }

            SummaryText =
                $"Track: {TrackName}\n" +
                $"Mode: {(AutoGeneratePlan ? "Auto-generate" : "Live strategy")}\n" +
                $"Race Length: {raceDuration:hh\\:mm}\n" +
                $"Total Race Laps: {totalLaps}\n" +
                $"Planned Stints: {Stints.Count}\n" +
                $"Tyre Sets Used: {totalTyreSetsUsed}/{AvailableTyres}\n" +
                $"Projected Finish: {projectedFinish:hh\\:mm\\:ss}";

            WarningText = warnings.Count > 0
                ? string.Join(" ", warnings)
                : string.Join(" ",
                    Stints.Where(s => !string.IsNullOrWhiteSpace(s.ValidationNote))
                        .Select(s => $"S{s.Number}: {s.ValidationNote}").Take(3));
        }
        finally
        {
            _isApplyingCalculatedValues = false;
        }
    }

    private int GetInitialTyreSetNumber()
    {
        if (Stints.Count == 0)
        {
            return 0;
        }

        return Stints[0].GetQualiTyres ? 1 : 2;
    }

    private string BuildValidationNote(StintPlan stint, int tyreSet, int driverAssignedStints)
    {
        List<string> notes = [];

        if (stint.Driver is null)
        {
            notes.Add("No driver is assigned to this stint.");
        }
        else if (stint.Driver.MaxStints > 0 && driverAssignedStints > stint.Driver.MaxStints)
        {
            notes.Add(
                $"Driver {stint.Driver.Name} is assigned to {driverAssignedStints} stints, above the allowed cap of {stint.Driver.MaxStints}.");
        }

        if (stint.Driver is not null && stint.Driver.EnergyPerLap <= 0)
        {
            notes.Add($"Driver {stint.Driver.Name} needs a VE per lap value above zero.");
        }

        double fuelNeed = stint.Laps * FuelPerLap;
        if (FuelPerLap > 0 && fuelNeed > FuelCapacity)
        {
            notes.Add(
                $"Fuel needed for this stint is {fuelNeed:F1}, which exceeds the configured fuel capacity of {FuelCapacity:F1}.");
        }

        double energyNeed = stint.Driver is null ? 0 : stint.Laps * stint.Driver.EnergyPerLap;
        if (stint.Driver is not null && stint.Driver.EnergyPerLap > 0 && energyNeed > EnergyCapacity)
        {
            notes.Add(
                $"VE needed for this stint is {energyNeed:F1}, which exceeds the configured VE capacity of {EnergyCapacity:F1}.");
        }

        if (tyreSet > AvailableTyres)
        {
            notes.Add("This tyre change would use more tyre sets than are available for the race.");
        }

        return string.Join(", ", notes);
    }
}
