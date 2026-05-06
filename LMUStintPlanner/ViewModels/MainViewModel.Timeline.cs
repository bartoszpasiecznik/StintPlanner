namespace LMUStintPlanner.ViewModels;

public sealed partial class MainViewModel
{
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
}
