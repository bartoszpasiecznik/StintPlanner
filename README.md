# LMU Stint Planner

LMU Stint Planner is an Avalonia desktop tool for planning Le Mans Ultimate team races.

It focuses on:

- race-length based stint generation
- driver stint-cap management
- pit stop timing
- tyre allocation across the race
- VE and fuel stop planning
- timezone-aware driver windows
- a combined stint and pit-stop timeline

## Tech Stack

- .NET 10
- Avalonia
- C#
- pragmatic Avalonia/MVVM desktop architecture

## Project Layout

- `StintPlanner.sln`: solution file
- `LMUStintPlanner/`: application project
- `LMUStintPlanner/App.axaml`: application theme/style bootstrap
- `LMUStintPlanner/Views/MainWindow.axaml`: main window shell
- `LMUStintPlanner/Views/Controls/`: setup panels and timeline/status controls
- `LMUStintPlanner/Styles/PlannerControls.axaml`: shared UI styles
- `LMUStintPlanner/ViewModels/MainViewModel*.cs`: planner state, commands, and strategy logic
- `LMUStintPlanner/ViewModels/Models/`: UI-facing planner models
- `LMUStintPlanner/ViewModels/Services/`: calculation and catalog helpers

## Current Feature Set

- Left-side setup tabs for `Race`, `Car`, `Weather`, and `Drivers`
- Right-side planning area with:
  - plan summary
  - checks and warnings
  - a compact modern timeline of stint cards and pit-stop cards
- Driver assignment filtered by stint-cap availability
- Car-class dependent fuel fields
- `Available Tyres` race limit
- `Change All Tyres` only on pit stops
- Tyre-set display on stint cards
- Qualifying-tyre handling for the first stint
- Unique per-driver stint-card colors, assigned from the driver list order
- Optional `Add All Needed VE` per pit stop, with manual VE override
- Per-driver `VE Per Lap` inputs that determine energy-limited stint length and VE estimates
- Driver time zones default to the user's PC time zone

## Build

From the repository root:

```powershell
dotnet build /p:UseAppHost=false
```

`UseAppHost=false` is useful when the app is already running and the normal executable output is locked by Windows.

## Run

```powershell
dotnet run --project .\LMUStintPlanner\
```

## Documentation

- [User Guide](docs/USER_GUIDE.md)
- [Planning Rules](docs/PLANNING_RULES.md)
- [Development Notes](docs/DEVELOPMENT.md)
- [Domain Reference](docs/DOMAIN_REFERENCE.md)
- [UI Contract](docs/UI_CONTRACT.md)
- [Session Context](docs/SESSION_CONTEXT.md)
- [Agent Instructions](AGENTS.md)
