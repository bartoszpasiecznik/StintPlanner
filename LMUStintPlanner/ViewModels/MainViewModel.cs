using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LMUStintPlanner.ViewModels;

public sealed partial class MainViewModel : BindableBase
{
    private bool _isApplyingCalculatedValues;

    public MainViewModel()
    {
        TimeZones = TimeZoneInfo.GetSystemTimeZones();
        Drivers = [];
        Stints = [];
        Tracks = new ObservableCollection<TrackOption>(PlanningCatalog.CreateTrackOptions());
        CarClasses = new ObservableCollection<CarClassOption>(PlanningCatalog.CreateCarClasses());

        Drivers.CollectionChanged += OnDriversCollectionChanged;
        Stints.CollectionChanged += OnStintsCollectionChanged;

        SelectedTrack = Tracks.FirstOrDefault(track => track.Name == "Circuit de la Sarthe") ?? Tracks.FirstOrDefault();
        SelectedCarClass = CarClasses.FirstOrDefault(carClass => carClass.Name == "Hypercar") ??
                           CarClasses.FirstOrDefault();
        SelectedRaceTimeZone = TimeZones.FirstOrDefault(tz => tz.Id == RaceTimeZoneId) ?? TimeZones.FirstOrDefault();

        string localTimeZoneId = TimeZones.FirstOrDefault(tz => tz.Id == TimeZoneInfo.Local.Id)?.Id ?? TimeZoneInfo.Local.Id;
        AddDriverInternal(new DriverPlan("Driver 1", "03:30.500", localTimeZoneId, 4, 3.3));
        AddDriverInternal(new DriverPlan("Driver 2", "03:31.200", localTimeZoneId, 4, 3.2));
        AddDriverInternal(new DriverPlan("Driver 3", "03:29.800", localTimeZoneId, 4, 3.4));

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
            return RainCrossoverCalculator.BuildSummary(
                CurrentDryLapTimeText,
                CurrentWetLapTimeText,
                RemainingStintLaps,
                RemainingStintFuel,
                FuelPerLap);
        }
    }

    public string RainCrossoverRecommendation
    {
        get
        {
            return RainCrossoverCalculator.BuildRecommendation(
                CurrentDryLapTimeText,
                CurrentWetLapTimeText,
                RemainingStintLaps);
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

}

