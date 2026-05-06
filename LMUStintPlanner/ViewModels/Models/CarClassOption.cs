namespace LMUStintPlanner.ViewModels;

public sealed class CarClassOption(string name, FuelFieldMode fuelMode)
{
    public string Name { get; } = name;

    public FuelFieldMode FuelMode { get; } = fuelMode;
}
