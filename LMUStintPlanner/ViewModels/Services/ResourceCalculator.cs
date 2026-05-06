namespace LMUStintPlanner.ViewModels;

public static class ResourceCalculator
{
    public static int GetCapacityLaps(double capacity, double perLap)
    {
        return perLap <= 0 ? int.MaxValue : Math.Max(0, (int)Math.Floor(capacity / perLap));
    }

    public static double GetRefillTimeSeconds(double amount, double capacity, double fullRefillSeconds)
    {
        if (capacity <= 0 || amount <= 0)
        {
            return 0;
        }

        return Math.Min(1, amount / capacity) * fullRefillSeconds;
    }
}
