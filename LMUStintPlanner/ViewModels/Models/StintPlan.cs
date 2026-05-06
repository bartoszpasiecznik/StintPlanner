using System.Collections.ObjectModel;

namespace LMUStintPlanner.ViewModels;

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
