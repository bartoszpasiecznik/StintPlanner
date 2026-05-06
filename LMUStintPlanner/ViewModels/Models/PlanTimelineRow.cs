using System.Collections.ObjectModel;

namespace LMUStintPlanner.ViewModels;

public sealed class PlanTimelineRow : BindableBase
{
    private static readonly (string Card, string Chip, string Soft, string Strong)[] DriverPalette =
    [
        ("#E6EFE7", "#2E6A43", "#F4F8F2", "#1F4B31"),
        ("#E4ECF2", "#315F86", "#F3F7FA", "#223F5C"),
        ("#F1E5D1", "#946327", "#FBF4E8", "#664215"),
        ("#ECE5F0", "#705287", "#F7F2FA", "#4D3762"),
        ("#F2E1DF", "#8F3D32", "#FBF0ED", "#642B24"),
        ("#DFEFEC", "#267267", "#F1F8F6", "#1C4D46")
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

        if (SourceStint.Driver is null)
        {
            return DriverPalette[0];
        }

        int index = SourceStint.Driver.ColorIndex;
        return index < DriverPalette.Length
            ? DriverPalette[index]
            : BuildDriverColorSet(index);
    }

    private static (string Card, string Chip, string Soft, string Strong) BuildDriverColorSet(int index)
    {
        double hue = index * 137.508 % 360;

        return (
            HslToHex(hue, 0.32, 0.91),
            HslToHex(hue, 0.48, 0.36),
            HslToHex(hue, 0.38, 0.97),
            HslToHex(hue, 0.55, 0.24)
        );
    }

    private static string HslToHex(double hue, double saturation, double lightness)
    {
        double chroma = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        double huePrime = hue / 60;
        double x = chroma * (1 - Math.Abs(huePrime % 2 - 1));

        (double red, double green, double blue) = huePrime switch
        {
            >= 0 and < 1 => (chroma, x, 0d),
            >= 1 and < 2 => (x, chroma, 0d),
            >= 2 and < 3 => (0d, chroma, x),
            >= 3 and < 4 => (0d, x, chroma),
            >= 4 and < 5 => (x, 0d, chroma),
            _ => (chroma, 0d, x)
        };

        double match = lightness - chroma / 2;
        int r = ToRgbChannel(red + match);
        int g = ToRgbChannel(green + match);
        int b = ToRgbChannel(blue + match);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static int ToRgbChannel(double value)
    {
        return Math.Clamp((int)Math.Round(value * 255), 0, 255);
    }
}
