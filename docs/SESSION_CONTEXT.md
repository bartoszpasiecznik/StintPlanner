# Session Context

This file summarizes the major product decisions already made in this repository so future agents have stable context.

## Product Direction

The planner is intended to feel like a practical race-engineering tool, not a generic form app.

The UI direction is:

- compact
- readable
- task-focused
- modern card-based planning on the right side

## Important Decisions Already Made

### Driver Planning

- driver limits are based on stint count, not lap count
- unavailable drivers should not appear as assignable options once capped
- new and default drivers use the user's PC time zone

### Tyres

- tyre-set lap limit was removed
- `Available Tyres` is the only tyre limiter
- tyre changes are configured on pit stops only
- stint cards show the tyre set being used
- the first stint can optionally use qualifying tyres
- when qualifying tyres are used, that first stint is tyre set 1
- when they are not used, the first race stint starts on tyre set 2 because the qualifying set still counts in the limit

### Pit Logic

- pit stops appear between stints
- pit timing includes pit lane, refill or VE time, tyre change time, repair time, and total time
- one full-refill-time input is used instead of separate rates
- there is no separate base service time

### Fuel And VE

- fuel visibility depends on car class
- VE capacity is configured at car level
- VE per lap is configured per driver
- each pit stop can auto-fill VE for the next stint or accept manual VE input
- VE auto-fill for a stop uses the next stint driver’s own VE-per-lap value

### Visual Direction

- the right-side cards were intentionally made smaller
- different drivers' stint cards use unique colors assigned from driver list order
- tag/chip styling should stay more angular and not drift back toward circle-like pills

### Summary

- plan summary is intentionally listed rather than paragraph-style
- it includes total race laps

## Current Main Files To Inspect

- `LMUStintPlanner/Views/MainWindow.axaml`
- `LMUStintPlanner/Views/Controls/`
- `LMUStintPlanner/Styles/PlannerControls.axaml`
- `LMUStintPlanner/ViewModels/MainViewModel*.cs`
- `LMUStintPlanner/ViewModels/Models/`
- `LMUStintPlanner/ViewModels/Services/`

## Safe Default Workflow For Future Agents

1. Read `AGENTS.md`
2. Read `docs/PLANNING_RULES.md`
3. Read `docs/UI_CONTRACT.md`
4. Inspect the relevant XAML and view-model bindings
5. Build with `dotnet build /p:UseAppHost=false`
