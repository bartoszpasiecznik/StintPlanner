using System.Collections.Specialized;
using System.ComponentModel;

namespace LMUStintPlanner.ViewModels;

public sealed partial class MainViewModel
{
    private void AddDriver()
    {
        string defaultZone = TimeZones.FirstOrDefault(tz => tz.Id == TimeZoneInfo.Local.Id)?.Id ?? TimeZoneInfo.Local.Id;
        AddDriverInternal(new DriverPlan($"Driver {Drivers.Count + 1}", "03:30.000", defaultZone, 4, 3.3));
        RefreshPlan();
    }

    private void AddDriverInternal(DriverPlan driver)
    {
        Drivers.Add(driver);
        UpdateDriverColorIndexes();
    }

    private void RemoveDriver(DriverPlan? driver)
    {
        if (driver is null)
        {
            return;
        }

        Drivers.Remove(driver);
        UpdateDriverColorIndexes();

        foreach (StintPlan stint in Stints.Where(s => ReferenceEquals(s.Driver, driver)))
        {
            stint.Driver = Drivers.FirstOrDefault();
        }

        RefreshPlan();
    }

    private void UpdateDriverColorIndexes()
    {
        for (int index = 0; index < Drivers.Count; index++)
        {
            Drivers[index].ColorIndex = index;
        }
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
