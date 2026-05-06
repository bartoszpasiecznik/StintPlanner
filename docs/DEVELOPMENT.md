# Development Notes

## Architecture

The app uses a pragmatic Avalonia/MVVM structure rather than a full multi-layer architecture.

Main pieces:

- `App.axaml`: Avalonia application theme and style includes
- `Views/MainWindow.axaml`: main window shell and top-level layout
- `Views/MainWindow.axaml.cs`: sets `DataContext`
- `Views/Controls/`: setup panels, status cards, and timeline cards
- `Styles/PlannerControls.axaml`: shared UI styles
- `ViewModels/MainViewModel*.cs`: application state, planner orchestration, commands, timing, and strategy logic
- `ViewModels/Models/`: UI-facing planner models
- `ViewModels/Services/`: calculation and catalog helpers

## Key Types

### `MainViewModel`

Owns:

- setup inputs
- collections of drivers and stints
- generated timeline rows
- commands
- summary and warning text
- recalculation and generation logic

### `DriverPlan`

Stores:

- name
- lap time
- VE per lap
- timezone
- max stints
- assigned stints
- color index used by timeline card styling

### `StintPlan`

Stores:

- driver
- laps
- fuel add
- VE add
- auto-fill VE flag
- tyre change flag
- repair time
- calculated timings
- tyre set number
- validation notes
- available drivers

### `PlanTimelineRow`

Wraps a `StintPlan` into either:

- a stint card row
- a pit-stop card row

This lets the UI show one mixed timeline while still editing the underlying stint model.

## Recalculation Model

`RecalculateStrategy()` is the central update path for:

- timing calculations
- driver color indexes
- tyre set assignment
- qualifying-tyre first-stint handling
- VE auto-fill for next stint
- warnings
- summary values
- filtered driver availability
- rebuilt timeline rows

Because the UI is highly reactive, property-change loops need to be controlled carefully. Calculated values are guarded so they do not recursively trigger full refreshes.

## Build Notes

Preferred verification command:

```powershell
dotnet build /p:UseAppHost=false
```

This avoids Windows executable locking issues when the app is already running.

## Current Tradeoffs

- `MainViewModel*.cs` still mixes UI-facing state with planning orchestration, even though it is split across partial files
- timeline rows proxy back into `StintPlan`, which is simple but tightly coupled
- summary and warnings are text-based rather than structured view models
- driver-level energy usage is embedded directly into `DriverPlan`, so generation logic and pit VE logic are coupled to driver setup

## Good Next Refactors

- move planning logic into a dedicated service
- split domain models from UI models
- replace summary strings with typed summary data
- add unit tests for:
  - driver availability filtering
  - driver-specific VE stint limits
  - tyre-limit enforcement
  - qualifying-tyre first-stint numbering
  - VE auto-fill behavior
  - pit timing calculations
