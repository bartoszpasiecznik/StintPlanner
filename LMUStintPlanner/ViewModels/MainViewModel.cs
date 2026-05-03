using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LMUStintPlanner.ViewModels;

public sealed class MainViewModel : BindableBase
{
    private bool _isApplyingCalculatedValues;

    public MainViewModel()
    {
        TimeZones = TimeZoneInfo.GetSystemTimeZones();
        Drivers = [];
        Stints = [];
        Tracks = new ObservableCollection<TrackOption>(CreateTrackOptions());
        CarClasses = new ObservableCollection<CarClassOption>(CreateCarClasses());

        Drivers.CollectionChanged += OnDriversCollectionChanged;
        Stints.CollectionChanged += OnStintsCollectionChanged;

        SelectedTrack = Tracks.FirstOrDefault(track => track.Name == "Circuit de la Sarthe") ?? Tracks.FirstOrDefault();
        SelectedCarClass = CarClasses.FirstOrDefault(carClass => carClass.Name == "Hypercar") ??
                           CarClasses.FirstOrDefault();
        SelectedRaceTimeZone = TimeZones.FirstOrDefault(tz => tz.Id == RaceTimeZoneId) ?? TimeZones.FirstOrDefault();

        AddDriverInternal(new DriverPlan("Driver 1", "03:30.500",
            TimeZones.FirstOrDefault()?.Id ?? TimeZoneInfo.Local.Id, 4, 3.3));
        AddDriverInternal(new DriverPlan("Driver 2", "03:31.200",
            TimeZones.FirstOrDefault()?.Id ?? TimeZoneInfo.Local.Id, 4, 3.2));
        AddDriverInternal(new DriverPlan("Driver 3", "03:29.800",
            TimeZones.FirstOrDefault()?.Id ?? TimeZoneInfo.Local.Id, 4, 3.4));

        AddDriverCommand = new RelayCommand(_ => AddDriver());
        RemoveDriverCommand =
            new RelayCommand(driver => RemoveDriver(driver as DriverPlan), driver => driver is DriverPlan);
        GenerateStrategyCommand = new RelayCommand(_ => GenerateStrategy());
        AddManualStintCommand = new RelayCommand(_ => AddManualStint());
        RemoveSelectedStintCommand = new RelayCommand(_ => RemoveSelectedStint(), _ => SelectedStint is not null);

        GenerateStrategy();
    }

    public ObservableCollection<TrackOption> Tracks { get; }

    public ObservableCollection<CarClassOption> CarClasses { get; }

    public ObservableCollection<DriverPlan> Drivers { get; }

    public ObservableCollection<StintPlan> Stints { get; }

    public ObservableCollection<PlanTimelineRow> PlanTimelineRows { get; } = [];

    public ReadOnlyCollection<TimeZoneInfo> TimeZones { get; }

    public ICommand AddDriverCommand { get; }

    public ICommand RemoveDriverCommand { get; }

    public ICommand GenerateStrategyCommand { get; }

    public ICommand AddManualStintCommand { get; }

    public ICommand RemoveSelectedStintCommand { get; }

    public TrackOption? SelectedTrack
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(TrackName));
            }
        }
    }

    public string TrackName => SelectedTrack?.Name ?? "Track not selected";

    public CarClassOption? SelectedCarClass
    {
        get;
        init
        {
            if (!SetProperty(ref field, value)) return;
            OnPropertyChanged(nameof(ShowFuelCapacity));
            OnPropertyChanged(nameof(ShowFuelPerLap));
        }
    }

    public bool ShowFuelCapacity => SelectedCarClass?.FuelMode == FuelFieldMode.CapacityAndPerLap;

    public bool ShowFuelPerLap =>
        SelectedCarClass?.FuelMode is FuelFieldMode.CapacityAndPerLap or FuelFieldMode.PerLapOnly;

    public int RaceLengthHours
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 6;

    public int RaceLengthMinutes
    {
        get;
        set => SetProperty(ref field, Math.Clamp(value, 0, 59));
    }

    public string RaceStartText
    {
        get;
        set => SetProperty(ref field, value);
    } = "2026-06-13 16:00";

    public string RaceTimeZoneId
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                SelectedRaceTimeZone = TimeZones.FirstOrDefault(tz => tz.Id == value) ?? TimeZones.FirstOrDefault();
            }
        }
    } = TimeZoneInfo.Local.Id;

    public TimeZoneInfo? SelectedRaceTimeZone
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                RaceTimeZoneId = value?.Id ?? TimeZoneInfo.Local.Id;
            }
        }
    }

    public bool AutoGeneratePlan
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public double FuelCapacity
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 90;

    public double FuelPerLap
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 3.1;

    public double EnergyCapacity
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 100;

    public double EnergyPerLap
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 3.3;

    public double PitLaneSeconds
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 44;

    public double BasePitSeconds
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 18;

    public double FullRefillSeconds
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 35;

    public double TyreChangeSeconds
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    } = 24;

    public int AvailableTyres
    {
        get;
        set => SetProperty(ref field, Math.Max(1, value));
    } = 7;

    public double DefaultRepairSeconds
    {
        get;
        set => SetProperty(ref field, Math.Max(0, value));
    }

    public string SummaryText
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string WarningText
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string RaceStartSummary
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string CurrentDryLapTimeText
    {
        get;
        set => SetProperty(ref field, value);
    } = "03:30.000";

    public string CurrentWetLapTimeText
    {
        get;
        set => SetProperty(ref field, value);
    } = "03:40.000";

    public int RemainingStintLaps
    {
        get;
        set
        {
            if (SetProperty(ref field, Math.Max(0, value)))
            {
                OnPropertyChanged(nameof(RainCrossoverSummary));
                OnPropertyChanged(nameof(RainCrossoverRecommendation));
            }
        }
    } = 5;

    public double RemainingStintFuel
    {
        get;
        set
        {
            if (SetProperty(ref field, Math.Max(0, value)))
            {
                OnPropertyChanged(nameof(RainCrossoverSummary));
                OnPropertyChanged(nameof(RainCrossoverRecommendation));
            }
        }
    } = 15.5;

    public string RainCrossoverSummary
    {
        get
        {
            if (!TryParseFlexibleLapTime(CurrentDryLapTimeText, out TimeSpan dryLap))
            {
                return "Enter a valid dry lap time.";
            }

            if (!TryParseFlexibleLapTime(CurrentWetLapTimeText, out TimeSpan wetLap))
            {
                return "Enter a valid wet lap time.";
            }

            if (RemainingStintLaps <= 0)
            {
                return "No laps left in the stint.";
            }

            TimeSpan delta = wetLap - dryLap;
            TimeSpan totalDelta = TimeSpan.FromTicks(delta.Ticks * RemainingStintLaps);
            string fuelWindow = FuelPerLap > 0
                ? $"{Math.Floor(RemainingStintFuel / FuelPerLap):0} dry laps of fuel at current burn"
                : "fuel burn not set";

            return
                $"Per-lap crossover delta: {FormatSignedTime(delta)} | " +
                $"Over {RemainingStintLaps} laps: {FormatSignedTime(totalDelta)} | " +
                $"Fuel window: {fuelWindow}";
        }
    }

    public string RainCrossoverRecommendation
    {
        get
        {
            if (!TryParseFlexibleLapTime(CurrentDryLapTimeText, out TimeSpan dryLap) ||
                !TryParseFlexibleLapTime(CurrentWetLapTimeText, out TimeSpan wetLap) ||
                RemainingStintLaps <= 0)
            {
                return "Add valid lap times and laps left to calculate the crossover.";
            }

            TimeSpan delta = wetLap - dryLap;
            if (delta < TimeSpan.Zero)
            {
                return $"Wet pace is quicker by {FormatSignedTime(-delta)} per lap. Crossing now favors wets.";
            }

            return delta == TimeSpan.Zero
                ? "Dry and wet pace are equal. Cross based on traffic, radar, and pit window."
                : $"Dry pace is quicker by {FormatSignedTime(delta)} per lap. Staying out favors dries for now.";
        }
    }

    public StintPlan? SelectedStint
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CommandManagerImpl.InvalidateRequerySuggested();
            }
        }
    }

    public PlanTimelineRow? SelectedTimelineRow
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                SelectedStint = value?.SourceStint;
            }
        }
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (_isApplyingCalculatedValues || propertyName is null)
        {
            return;
        }

        switch (propertyName)
        {
            case nameof(SummaryText)
                or nameof(WarningText)
                or nameof(RaceStartSummary)
                or nameof(RainCrossoverSummary)
                or nameof(RainCrossoverRecommendation)
                or nameof(SelectedStint)
                or nameof(TrackName)
                or nameof(ShowFuelCapacity)
                or nameof(ShowFuelPerLap):
                return;
            case nameof(CurrentDryLapTimeText) or nameof(CurrentWetLapTimeText):
                OnPropertyChanged(nameof(RainCrossoverSummary));
                OnPropertyChanged(nameof(RainCrossoverRecommendation));
                break;
            case nameof(AutoGeneratePlan):
                break;
        }

        RefreshPlan();
    }

    private void RefreshPlan()
    {
        if (AutoGeneratePlan)
        {
            GenerateStrategy();
        }
        else
        {
            RecalculateStrategy();
        }
    }

    private void AddDriver()
    {
        string defaultZone = TimeZones.FirstOrDefault()?.Id ?? TimeZoneInfo.Local.Id;
        AddDriverInternal(new DriverPlan($"Driver {Drivers.Count + 1}", "03:30.000", defaultZone, 4, 3.3));
        RefreshPlan();
    }

    private void AddDriverInternal(DriverPlan driver)
    {
        Drivers.Add(driver);
    }

    private void RemoveDriver(DriverPlan? driver)
    {
        if (driver is null)
        {
            return;
        }

        Drivers.Remove(driver);

        foreach (StintPlan stint in Stints.Where(s => ReferenceEquals(s.Driver, driver)))
        {
            stint.Driver = Drivers.FirstOrDefault();
        }

        RefreshPlan();
    }

    private void AddManualStint()
    {
        DriverPlan? driver = Stints.LastOrDefault()?.Driver ?? Drivers.FirstOrDefault();
        const int laps = 10;

        StintPlan newStint = new()
        {
            Number = Stints.Count + 1,
            Driver = driver,
            Laps = laps,
            RefuelAmount = FuelPerLap * laps,
            EnergyAmount = EnergyPerLap * laps,
            AutoFillEnergyForNextStint = true,
            RepairSeconds = DefaultRepairSeconds
        };

        Stints.Add(newStint);
        SelectedStint = newStint;
        AutoGeneratePlan = false;
        RecalculateStrategy();
    }

    private void RemoveSelectedStint()
    {
        if (SelectedStint is null)
        {
            return;
        }

        Stints.Remove(SelectedStint);
        SelectedStint = null;
        RecalculateStrategy();
    }

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

        int maxFuelLaps = GetCapacityLaps(FuelCapacity, FuelPerLap);
        int maxEnergyLaps = activeDrivers.Max(driver => GetCapacityLaps(EnergyCapacity, driver.EnergyPerLap));
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
            int driverEnergyLaps = GetCapacityLaps(EnergyCapacity, driver.EnergyPerLap);
            int stintCap = Math.Min(maxFuelLaps, driverEnergyLaps);
            int stintLaps = Math.Max(1, Math.Min(stintCap, remainingLaps));

            if (remainingLaps > stintLaps && remainingLaps - stintLaps < Math.Min(stintCap, 3))
            {
                stintLaps = Math.Max(1, Math.Min(stintCap, remainingLaps / 2));
            }

            StintPlan stint = new StintPlan
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

            DriverPlan nextDriver = availableDrivers[(driverIndex) % availableDrivers.Count];
            int nextStintCap = Math.Min(maxFuelLaps, GetCapacityLaps(EnergyCapacity, nextDriver.EnergyPerLap));
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
            Dictionary<DriverPlan, int> driverStintCounts = new Dictionary<DriverPlan, int>();
            int totalTyreSetsUsed = Math.Max(0, tyreSet);

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
                double serviceSeconds = BasePitSeconds
                                        + GetRefillTimeSeconds(stint.RefuelAmount, FuelCapacity)
                                        + GetRefillTimeSeconds(stint.EnergyAmount, EnergyCapacity)
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

                stint.DriverLocalWindowText = TryConvertDriverWindow(stint.Driver.TimeZoneId, stint.RaceStart, stint.RaceEnd,
                    out string localWindow) ? localWindow : "Invalid time zone";

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

            totalTyreSetsUsed = Math.Max(0, tyreSet);

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

    private void UpdateAvailableDrivers(Dictionary<DriverPlan, int> driverStintCounts)
    {
        foreach (StintPlan stint in Stints)
        {
            List<DriverPlan> availableDrivers = Drivers
                .Where(driver =>
                {
                    int assigned = driverStintCounts.GetValueOrDefault(driver);
                    if (ReferenceEquals(driver, stint.Driver))
                    {
                        assigned--;
                    }

                    return driver.MaxStints <= 0 || assigned < driver.MaxStints;
                })
                .ToList();

            stint.SetAvailableDrivers(availableDrivers);
        }
    }

    private void RebuildTimelineRows()
    {
        StintPlan? selectedStint = SelectedTimelineRow?.SourceStint ?? SelectedStint;

        PlanTimelineRows.Clear();

        foreach (StintPlan stint in Stints)
        {
            PlanTimelineRows.Add(PlanTimelineRow.ForStint(stint));

            if (stint != Stints.Last())
            {
                PlanTimelineRows.Add(PlanTimelineRow.ForPitStop(stint));
            }
        }

        SelectedTimelineRow = PlanTimelineRows.FirstOrDefault(row => ReferenceEquals(row.SourceStint, selectedStint));
    }

    private double GetRefillTimeSeconds(double amount, double capacity)
    {
        if (capacity <= 0 || amount <= 0)
        {
            return 0;
        }

        return Math.Min(1, amount / capacity) * FullRefillSeconds;
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
        List<string> notes = new List<string>();

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

    private void ReplaceStints(IEnumerable<StintPlan> stints)
    {
        foreach (StintPlan stint in Stints)
        {
            stint.PropertyChanged -= OnStintPropertyChanged;
        }

        Stints.Clear();

        foreach (StintPlan stint in stints)
        {
            stint.PropertyChanged += OnStintPropertyChanged;
            Stints.Add(stint);
        }
    }

    private static bool TryConvertDriverWindow(
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

    private static int GetCapacityLaps(double capacity, double perLap)
    {
        return perLap <= 0 ? int.MaxValue : Math.Max(0, (int)Math.Floor(capacity / perLap));
    }

    private static bool TryParseFlexibleLapTime(string? text, out TimeSpan lapTime)
    {
        lapTime = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] formats =
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

        return TimeSpan.TryParseExact(text.Trim(), formats, CultureInfo.InvariantCulture, out lapTime);
    }

    private static string FormatSignedTime(TimeSpan value)
    {
        string sign = value < TimeSpan.Zero ? "-" : string.Empty;
        TimeSpan absolute = value.Duration();

        return absolute.Hours > 0 ? $@"{sign}{absolute:hh\:mm\:ss\.fff}" : $@"{sign}{absolute:mm\:ss\.fff}";
    }

    private static IEnumerable<TrackOption> CreateTrackOptions()
    {
        return
        [
            new TrackOption("Bahrain International Circuit", "Base game 2023 WEC"),
            new TrackOption("Circuit de la Sarthe", "Base game 2023 WEC"),
            new TrackOption("Fuji Speedway", "Base game 2023 WEC"),
            new TrackOption("Sebring International Raceway", "Base game 2023 WEC"),
            new TrackOption("Autodromo Nazionale Monza", "Base game 2023 WEC"),
            new TrackOption("Circuit de Spa-Francorchamps", "Base game 2023 WEC"),
            new TrackOption("Algarve International Circuit", "Base game 2023 WEC"),
            new TrackOption("Autodromo Internazionale Enzo e Dino Ferrari", "2024 Pack 1"),
            new TrackOption("Circuit of the Americas", "2024 Pack 2"),
            new TrackOption("Interlagos", "2024 Pack 3"),
            new TrackOption("Lusail International Circuit", "2024 season content"),
            new TrackOption("Silverstone International Circuit", "ELMS Pack 1"),
            new TrackOption("Circuit Paul Ricard", "ELMS Pack 2"),
            new TrackOption("Circuit de Barcelona-Catalunya", "ELMS Pack 3")
        ];
    }

    private static IEnumerable<CarClassOption> CreateCarClasses()
    {
        return
        [
            new CarClassOption("Hypercar", FuelFieldMode.Hidden),
            new CarClassOption("LMGT3", FuelFieldMode.Hidden),
            new CarClassOption("LMP2", FuelFieldMode.PerLapOnly),
            new CarClassOption("LMP3", FuelFieldMode.PerLapOnly),
            new CarClassOption("GTE", FuelFieldMode.PerLapOnly)
        ];
    }

    private TimeSpan GetRaceDuration()
    {
        return TimeSpan.FromHours(Math.Max(0, RaceLengthHours)) +
               TimeSpan.FromMinutes(Math.Clamp(RaceLengthMinutes, 0, 59));
    }

    private bool TryGetRaceStart(out DateTimeOffset raceStart)
    {
        raceStart = default;

        if (!DateTime.TryParse(RaceStartText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces,
                out DateTime parsed))
        {
            return false;
        }

        try
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(RaceTimeZoneId);
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

    private void OnDriversCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (DriverPlan driver in e.NewItems)
            {
                driver.PropertyChanged += OnDriverPropertyChanged;
            }
        }

        if (e.OldItems is null) return;

        foreach (DriverPlan driver in e.OldItems)
        {
            driver.PropertyChanged -= OnDriverPropertyChanged;
        }
    }

    private void OnStintsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (StintPlan stint in e.NewItems)
            {
                stint.PropertyChanged += OnStintPropertyChanged;
            }
        }

        if (e.OldItems is null) return;
        
        foreach (StintPlan stint in e.OldItems)
        {
            stint.PropertyChanged -= OnStintPropertyChanged;
        }
    }

    private void OnDriverPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isApplyingCalculatedValues || e.PropertyName == nameof(DriverPlan.AssignedStints))
        {
            return;
        }

        RefreshPlan();
    }

    private void OnStintPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isApplyingCalculatedValues)
        {
            return;
        }

        if (e.PropertyName is nameof(StintPlan.Number)
            or nameof(StintPlan.DrivingTime)
            or nameof(StintPlan.PitLaneTime)
            or nameof(StintPlan.ServiceTime)
            or nameof(StintPlan.TotalPitTime)
            or nameof(StintPlan.CanChangeTyres)
            or nameof(StintPlan.RaceStart)
            or nameof(StintPlan.RaceEnd)
            or nameof(StintPlan.DrivingTimeText)
            or nameof(StintPlan.PitLaneTimeText)
            or nameof(StintPlan.ServiceTimeText)
            or nameof(StintPlan.TotalPitTimeText)
            or nameof(StintPlan.RaceStart)
            or nameof(StintPlan.RaceEnd)
            or nameof(StintPlan.RaceStartClockText)
            or nameof(StintPlan.RaceEndClockText)
            or nameof(StintPlan.DriverLocalWindowText)
            or nameof(StintPlan.ValidationNote)
            or nameof(StintPlan.TyreSetNumber))
        {
            return;
        }

        AutoGeneratePlan = false;
        RecalculateStrategy();
    }
}

public sealed class TrackOption(string name, string source)
{
    public string Name { get; } = name;

    public string Source { get; } = source;

    public string DisplayName => $"{Name} ({Source})";
}

public sealed class CarClassOption(string name, FuelFieldMode fuelMode)
{
    public string Name { get; } = name;

    public FuelFieldMode FuelMode { get; } = fuelMode;
}

public enum FuelFieldMode
{
    Hidden,
    PerLapOnly,
    CapacityAndPerLap
}

public sealed class DriverPlan(string name, string lapTimeText, string timeZoneId, int maxStints, double energyPerLap)
    : BindableBase
{
    private string _name = name;
    private string _lapTimeText = lapTimeText;
    private string _timeZoneId = timeZoneId;
    private TimeZoneInfo? _selectedTimeZone;
    private int _maxStints = maxStints;
    private int _assignedStints;
    private double _energyPerLap = energyPerLap;

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

    public TimeSpan ParsedLapTime => TryGetLapTime(out TimeSpan lapTime) ? lapTime : TimeSpan.Zero;

    public bool TryGetLapTime(out TimeSpan lapTime)
    {
        lapTime = TimeSpan.Zero;
        string text = LapTimeText.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] formats = [
            @"m\:ss\.fff",
            @"mm\:ss\.fff",
            @"h\:mm\:ss",
            @"hh\:mm\:ss",
            @"h\:mm\:ss\.fff",
            @"hh\:mm\:ss\.fff"
        ];

        return TimeSpan.TryParseExact(text, formats, CultureInfo.InvariantCulture, out lapTime);
    }
}

public sealed class StintPlan : BindableBase
{
    private int _number;
    private DriverPlan? _driver;
    private int _laps;
    private double _refuelAmount;
    private double _energyAmount;
    private bool _autoFillEnergyForNextStint = true;
    private bool _getQualiTyres;
    private bool _changeTyres;
    private double _repairSeconds;
    private TimeSpan _drivingTime;
    private TimeSpan _pitLaneTime;
    private TimeSpan _serviceTime;
    private TimeSpan _totalPitTime;
    private DateTimeOffset _raceStart;
    private DateTimeOffset _raceEnd;
    private string _driverLocalWindowText = string.Empty;
    private string _validationNote = string.Empty;
    private int _tyreSetNumber = 1;
    private ObservableCollection<DriverPlan> _availableDrivers = [];
    private bool _canChangeTyres = true;

    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public DriverPlan? Driver
    {
        get => _driver;
        set => SetProperty(ref _driver, value);
    }

    public int Laps
    {
        get => _laps;
        set => SetProperty(ref _laps, Math.Max(0, value));
    }

    public double RefuelAmount
    {
        get => _refuelAmount;
        set => SetProperty(ref _refuelAmount, Math.Max(0, value));
    }

    public double EnergyAmount
    {
        get => _energyAmount;
        set => SetProperty(ref _energyAmount, Math.Max(0, value));
    }

    public bool AutoFillEnergyForNextStint
    {
        get => _autoFillEnergyForNextStint;
        set => SetProperty(ref _autoFillEnergyForNextStint, value);
    }

    public bool GetQualiTyres
    {
        get => _getQualiTyres;
        set => SetProperty(ref _getQualiTyres, value);
    }

    public bool ChangeTyres
    {
        get => _changeTyres;
        set => SetProperty(ref _changeTyres, value);
    }

    public double RepairSeconds
    {
        get => _repairSeconds;
        set => SetProperty(ref _repairSeconds, Math.Max(0, value));
    }

    public TimeSpan DrivingTime
    {
        get => _drivingTime;
        set
        {
            if (SetProperty(ref _drivingTime, value))
            {
                OnPropertyChanged(nameof(DrivingTimeText));
            }
        }
    }

    public TimeSpan PitLaneTime
    {
        get => _pitLaneTime;
        set
        {
            if (SetProperty(ref _pitLaneTime, value))
            {
                OnPropertyChanged(nameof(PitLaneTimeText));
            }
        }
    }

    public TimeSpan ServiceTime
    {
        get => _serviceTime;
        set
        {
            if (SetProperty(ref _serviceTime, value))
            {
                OnPropertyChanged(nameof(ServiceTimeText));
            }
        }
    }

    public TimeSpan TotalPitTime
    {
        get => _totalPitTime;
        set
        {
            if (SetProperty(ref _totalPitTime, value))
            {
                OnPropertyChanged(nameof(TotalPitTimeText));
            }
        }
    }

    public DateTimeOffset RaceStart
    {
        get => _raceStart;
        set
        {
            if (SetProperty(ref _raceStart, value))
            {
                OnPropertyChanged(nameof(RaceStartClockText));
            }
        }
    }

    public DateTimeOffset RaceEnd
    {
        get => _raceEnd;
        set
        {
            if (SetProperty(ref _raceEnd, value))
            {
                OnPropertyChanged(nameof(RaceEndClockText));
            }
        }
    }

    public string DriverLocalWindowText
    {
        get => _driverLocalWindowText;
        set => SetProperty(ref _driverLocalWindowText, value);
    }

    public string ValidationNote
    {
        get => _validationNote;
        set => SetProperty(ref _validationNote, value);
    }

    public int TyreSetNumber
    {
        get => _tyreSetNumber;
        set => SetProperty(ref _tyreSetNumber, value);
    }

    public ObservableCollection<DriverPlan> AvailableDrivers
    {
        get => _availableDrivers;
        private set => SetProperty(ref _availableDrivers, value);
    }

    public bool CanChangeTyres
    {
        get => _canChangeTyres;
        set => SetProperty(ref _canChangeTyres, value);
    }

    public string DrivingTimeText => DrivingTime.ToString(@"hh\:mm\:ss");

    public string PitLaneTimeText => PitLaneTime.ToString(@"mm\:ss");

    public string ServiceTimeText => ServiceTime.ToString(@"mm\:ss");

    public string TotalPitTimeText => TotalPitTime.ToString(@"mm\:ss");

    public string RaceStartClockText => RaceStart == default ? string.Empty : RaceStart.ToString("ddd HH:mm");

    public string RaceEndClockText => RaceEnd == default ? string.Empty : RaceEnd.ToString("ddd HH:mm");

    public void SetAvailableDrivers(IEnumerable<DriverPlan> drivers)
    {
        AvailableDrivers = new ObservableCollection<DriverPlan>(drivers);
    }
}

public sealed class PlanTimelineRow : BindableBase
{
    private static readonly (string Card, string Chip, string Soft, string Strong)[] DriverPalette =
    [
        ("#E7F3EC", "#2F6B45", "#F5FBF7", "#1E4A30"),
        ("#EAF2FB", "#2E5D93", "#F6FAFE", "#1E3E63"),
        ("#FBEEDC", "#9A5A1A", "#FFF8F0", "#6B3D10"),
        ("#F5EAF8", "#7A3E8E", "#FBF6FD", "#542A63"),
        ("#FCEBEC", "#9A3D47", "#FFF7F8", "#6C2931"),
        ("#EAF7F6", "#24766C", "#F5FCFB", "#184F49")
    ];

    private PlanTimelineRow(StintPlan sourceStint, bool isPitStop)
    {
        SourceStint = sourceStint;
        IsPitStop = isPitStop;
    }

    public StintPlan SourceStint { get; }

    public bool IsPitStop { get; }

    public static PlanTimelineRow ForStint(StintPlan stint) => new(stint, false);

    public static PlanTimelineRow ForPitStop(StintPlan stint) => new(stint, true);

    public bool IsStint => !IsPitStop;

    public string RowType => IsPitStop ? "Pit Stop" : "Stint";

    public string NumberText => IsPitStop ? $"P{SourceStint.Number}" : SourceStint.Number.ToString();

    public bool IsFirstStint => SourceStint.Number == 1;

    public DriverPlan? Driver
    {
        get => SourceStint.Driver;
        set
        {
            if (IsPitStop)
            {
                return;
            }

            SourceStint.Driver = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DriverPlan> AvailableDrivers => SourceStint.AvailableDrivers;

    public int Laps
    {
        get => SourceStint.Laps;
        set
        {
            if (IsPitStop)
            {
                return;
            }

            SourceStint.Laps = value;
            OnPropertyChanged();
        }
    }

    public bool GetQualiTyres
    {
        get => SourceStint.GetQualiTyres;
        set
        {
            if (IsPitStop || !IsFirstStint)
            {
                return;
            }

            SourceStint.GetQualiTyres = value;
            OnPropertyChanged();
        }
    }

    public double RefuelAmount
    {
        get => SourceStint.RefuelAmount;
        set
        {
            if (!IsPitStop)
            {
                return;
            }

            SourceStint.RefuelAmount = value;
            OnPropertyChanged();
        }
    }

    public double EnergyAmount
    {
        get => SourceStint.EnergyAmount;
        set
        {
            if (!IsPitStop)
            {
                return;
            }

            SourceStint.EnergyAmount = value;
            OnPropertyChanged();
        }
    }

    public bool ChangeTyres
    {
        get => SourceStint.ChangeTyres;
        set
        {
            if (!IsPitStop)
            {
                return;
            }

            SourceStint.ChangeTyres = value;
            OnPropertyChanged();
        }
    }

    public bool CanChangeTyres => SourceStint.CanChangeTyres;

    public double RepairSeconds
    {
        get => SourceStint.RepairSeconds;
        set
        {
            if (!IsPitStop)
            {
                return;
            }

            SourceStint.RepairSeconds = value;
            OnPropertyChanged();
        }
    }

    public string DriverName => IsPitStop
        ? $"{SourceStint.Driver?.Name ?? "No Driver"} pit stop"
        : SourceStint.Driver?.Name ?? "No Driver";

    public string StintCardBackground => GetDriverColorSet().Card;

    public string StintBadgeBackground => GetDriverColorSet().Chip;

    public string StintBadgeForeground => "#FFFDFD";

    public string StintChipBackground => GetDriverColorSet().Soft;

    public string StintAccentForeground => GetDriverColorSet().Strong;

    public string MainTimeText => IsPitStop ? SourceStint.TotalPitTimeText : SourceStint.DrivingTimeText;

    public string RaceStartText => IsPitStop
        ? SourceStint.RaceEnd.ToString("ddd HH:mm")
        : SourceStint.RaceStartClockText;

    public string RaceEndText => IsPitStop
        ? (SourceStint.RaceEnd + SourceStint.TotalPitTime).ToString("ddd HH:mm")
        : SourceStint.RaceEndClockText;

    public string DriverLocalText => IsPitStop ? string.Empty : SourceStint.DriverLocalWindowText;

    public string PitLaneText => IsPitStop ? SourceStint.PitLaneTimeText : string.Empty;

    public string ServiceText => IsPitStop ? SourceStint.ServiceTimeText : string.Empty;

    public string TotalPitText => IsPitStop ? SourceStint.TotalPitTimeText : string.Empty;

    public string TyreSetText => SourceStint.TyreSetNumber.ToString();

    public bool AutoFillEnergyForNextStint
    {
        get => SourceStint.AutoFillEnergyForNextStint;
        set
        {
            if (!IsPitStop)
            {
                return;
            }

            SourceStint.AutoFillEnergyForNextStint = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEnergyInputEnabled));
        }
    }

    public bool IsEnergyInputEnabled => !AutoFillEnergyForNextStint;

    public string Notes => SourceStint.ValidationNote;

    private (string Card, string Chip, string Soft, string Strong) GetDriverColorSet()
    {
        if (IsPitStop)
        {
            return ("#F3E1E4", "#8B1E2D", "#FDF5F6", "#5E2A31");
        }

        string? key = SourceStint.Driver?.Name;
        if (string.IsNullOrWhiteSpace(key))
        {
            return DriverPalette[0];
        }

        int index = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(key)) % DriverPalette.Length;
        return DriverPalette[index];
    }
}

public abstract class BindableBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManagerImpl.RequerySuggested += value;
        remove => CommandManagerImpl.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => execute(parameter);
}

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
