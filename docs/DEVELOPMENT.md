# Development Notes

## Architecture

The app currently uses a pragmatic single-view-model structure rather than a full multi-layer MVVM breakdown.

Main pieces:

- `MainWindow.xaml`: all UI layout and bindings
- `MainWindow.xaml.cs`: sets `DataContext`
- `MainViewModel.cs`: application state, planner logic, commands, and model types

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
- timezone
- max stints
- assigned stints

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
- tyre set assignment
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

- `MainViewModel.cs` is large and mixes UI-facing state with planning logic
- timeline rows proxy back into `StintPlan`, which is simple but tightly coupled
- summary and warnings are text-based rather than structured view models

## Good Next Refactors

- move planning logic into a dedicated service
- split domain models from UI models
- replace summary strings with typed summary data
- add unit tests for:
  - driver availability filtering
  - tyre-limit enforcement
  - VE auto-fill behavior
  - pit timing calculations
