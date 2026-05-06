using Avalonia.Controls;
using Avalonia;

namespace LMUStintPlanner.Views.Controls;

public partial class PitStopTimelineCard : UserControl
{
    public static readonly StyledProperty<bool> ShowFuelPerLapProperty =
        AvaloniaProperty.Register<PitStopTimelineCard, bool>(nameof(ShowFuelPerLap));

    public PitStopTimelineCard()
    {
        InitializeComponent();
    }

    public bool ShowFuelPerLap
    {
        get => GetValue(ShowFuelPerLapProperty);
        set => SetValue(ShowFuelPerLapProperty, value);
    }
}
