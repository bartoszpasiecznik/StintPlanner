# AGENTS

This file is for future coding agents working in this repository.

## Project Summary

LMU Stint Planner is an Avalonia desktop application for planning Le Mans Ultimate team races.

Core concerns:

- race duration based stint planning
- driver rotation with stint caps
- pit stop timing
- tyre-set usage across the race
- VE and fuel planning
- timezone-aware driver windows
- a mixed stint and pit-stop timeline

## Repository Layout

- `StintPlanner.sln`: solution
- `LMUStintPlanner/LMUStintPlanner.csproj`: Avalonia application project
- `LMUStintPlanner/App.axaml`: application theme/style bootstrap
- `LMUStintPlanner/Styles/PlannerControls.axaml`: shared control styling
- `LMUStintPlanner/Views/MainWindow.axaml`: main window shell and top-level layout
- `LMUStintPlanner/Views/Controls/`: setup panels, status cards, and timeline cards
- `LMUStintPlanner/ViewModels/MainViewModel*.cs`: planner state, commands, and strategy logic
- `LMUStintPlanner/ViewModels/Models/`: UI-facing planner models
- `LMUStintPlanner/ViewModels/Services/`: calculation and catalog helpers
- `docs/`: project documentation

## Build And Verification

Run from repo root:

```powershell
dotnet build /p:UseAppHost=false
```

Use `UseAppHost=false` by default because the app executable is often locked if the user has the program open.

If you need to run the app:

```powershell
dotnet run --project .\LMUStintPlanner\
```

## Current Architecture

This project is intentionally simple and centralized, but the original single-file UI and view-model have been split into Avalonia views, controls, styles, partial view-model files, models, and services.

- UI is in `Views/MainWindow.axaml` and `Views/Controls/*.axaml`
- shared UI styling is in `Styles/PlannerControls.axaml`
- planner state and orchestration are in `ViewModels/MainViewModel*.cs`
- domain/UI models are in `ViewModels/Models/`
- calculation helpers are in `ViewModels/Services/`
- `StintPlan` is both a domain object and a UI-editable model
- `PlanTimelineRow` is a UI adapter over `StintPlan`

Do not assume there is a deep layered architecture beyond this pragmatic Avalonia/MVVM split.

## Critical Behavior Invariants

Preserve these unless the user explicitly asks to change them:

1. Driver availability is filtered by `MaxStints`
2. `Available Tyres` is the only tyre-limit setting
3. `Change All Tyres` appears only on pit stops
4. Tyre-set number is shown on the stint card, not the pit card
5. Fuel fields depend on selected car class
6. Pit-stop fuel input follows the same car-class visibility rules as the car menu
7. VE can be auto-filled for the next stint or manually entered
8. VE-per-lap planning is driver-specific, not car-global
9. Pit stops are shown between stints in the timeline
10. Summary includes total race laps
11. The right side should stay compact and card-based, not revert to a large grid
12. The first stint can use qualifying tyres, and tyre-set numbering must reflect that exactly
13. Pit timing has no separate base service time; it is pit lane plus refill/VE time, tyre change time, and repair time
14. New/default driver time zones use the user's PC time zone unless changed manually
15. Driver stint-card colors are unique per configured driver and assigned from driver list order

## High-Risk Areas

### Property-change recursion

This app previously had stack overflows caused by calculated property changes re-triggering recalculation.

When editing:

- `OnStintPropertyChanged`
- `OnDriverPropertyChanged`
- `RefreshPlan()`
- `GenerateStrategy()`
- `RecalculateStrategy()`

be careful not to reintroduce recursive update loops.

### Timeline bindings

`PlanTimelineRow` proxies editable values back into `StintPlan`.

If timeline editing stops working, inspect both:

- the XAML binding
- the corresponding `PlanTimelineRow` property proxy

### Car-class conditional UI

Fuel visibility is controlled centrally by:

- `ShowFuelCapacity`
- `ShowFuelPerLap`

Reuse these rules instead of introducing competing logic in XAML.

### Tyre numbering

The first-stint qualifying-tyre option changes the actual tyre-set numbering.

Preserve these meanings:

- `Get Quali Tyres = true` means first stint uses tyre set 1
- `Get Quali Tyres = false` means first stint uses tyre set 2
- the qualifying set still counts in the race tyre limit either way

## Editing Guidance

- Prefer small, targeted changes
- Keep the current visual direction compact and modern
- Avoid making the right-side cards larger unless explicitly requested
- Do not reintroduce unused settings that were already removed
- Keep docs in sync when behavior changes
- Preserve unique per-driver stint colors unless the user asks for a redesign

## Required Docs To Read First

Before major changes, read:

- `README.md`
- `docs/USER_GUIDE.md`
- `docs/PLANNING_RULES.md`
- `docs/DEVELOPMENT.md`
- `docs/DOMAIN_REFERENCE.md`
- `docs/UI_CONTRACT.md`

## When Adding Features

Update the relevant docs if you change:

- race rules
- pit logic
- tyre logic
- car-class behavior
- timeline UI structure
- summary/check output

## When Refactoring

If you refactor `MainViewModel*.cs`, preserve:

- current binding names used by XAML
- current timeline editing behavior
- current driver filtering behavior
- current pit-stop placement in the timeline
