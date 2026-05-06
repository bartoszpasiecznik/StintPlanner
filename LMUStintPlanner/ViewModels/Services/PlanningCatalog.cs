namespace LMUStintPlanner.ViewModels;

public static class PlanningCatalog
{
    public static IEnumerable<TrackOption> CreateTrackOptions()
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

    public static IEnumerable<CarClassOption> CreateCarClasses()
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
}
