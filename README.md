# LMU Stint Planner

LMU Stint Planner is a WPF desktop tool for planning Le Mans Ultimate team races.

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
- WPF
- C#
- single-view-model desktop architecture

## Project Layout

- `StintPlanner.sln`: solution file
- `LMUStintPlanner/`: application project
- `LMUStintPlanner/MainWindow.xaml`: main UI
- `LMUStintPlanner/MainViewModel.cs`: planner logic, state, and models

## Current Feature Set

- Left-side setup tabs for `Race`, `Car`, `Weather`, and `Drivers`
- Right-side planning area with:
  - plan summary
  - checks and warnings
  - a modern timeline of stint cards and pit-stop cards
- Driver assignment filtered by stint-cap availability
- Car-class dependent fuel fields
- `Available Tyres` race limit
- `Change All Tyres` only on pit stops
- Tyre-set display on stint cards
- Optional `Add All Needed VE` per pit stop, with manual VE override

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
