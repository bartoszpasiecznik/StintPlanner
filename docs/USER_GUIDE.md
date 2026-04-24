# User Guide

## Purpose

This app helps plan a full LMU team race by combining race setup, car setup, driver rotation, pit timing, tyre usage, and stint-by-stint timeline editing.

## Main Layout

The window is split into two areas:

- Left side: setup tabs
- Right side: generated plan, warnings, and timeline cards

## Left-Side Tabs

### Race

Use this tab for race-wide settings:

- track
- race start
- race duration
- race timezone
- full refill time
- tyre change time
- available tyres
- auto-generate toggle

`Full Refill Time (s)` means the time needed to refill from `0` to `100`. The planner scales that time based on how much fuel or VE is added at a stop.

`Available Tyres` is the tyre-set limiter for the race.

### Car

Use this tab for car and pit configuration:

- car class
- fuel capacity
- fuel per lap
- VE capacity
- VE per lap
- pit lane time
- base service time
- default repair time

Fuel fields depend on the selected class:

- `Hypercar`, `LMGT3`: fuel inputs are hidden
- `LMP2`, `LMP3`, `GTE`: fuel per lap is shown

### Weather

Use this tab for wet/dry crossover calculation.

### Drivers

Use this tab to configure:

- driver names
- lap times
- time zones
- max stints

`Max Stints` is the driver cap used by the planner.

## Right-Side Plan Area

### Plan Summary

The summary shows:

- track
- mode
- race length
- total race laps
- planned stints
- tyre sets used
- projected finish

### Checks

This area lists planning warnings such as:

- tyre overuse
- driver cap over-assignment
- plan overruns
- invalid stint setup

### Timeline

The timeline alternates between:

- stint cards
- pit-stop cards

#### Stint Card

A stint card shows:

- stint number
- driver selector
- start and end time
- driver local time
- laps
- drive time
- tyre set in use
- descriptive notes

#### Pit-Stop Card

A pit-stop card shows:

- pit number
- stop window
- pit lane time
- service time
- total pit time
- fuel add, when applicable for the selected car class
- VE add
- repair time
- `Change All Tyres`
- descriptive notes

## VE Pit-Stop Behavior

Each pit stop supports two VE modes:

- `Add All Needed VE` enabled: VE is automatically filled for the next stint
- `Add All Needed VE` disabled: VE amount can be edited manually

## Tyre Behavior

- Tyre changes are configured only on pit stops
- When all available tyre sets are already used, additional tyre changes cannot be enabled
- The active tyre set is shown on the stint card, not on the pit card

## Driver Availability

Drivers who already reached their stint cap are hidden from assignment choices, unless they are already assigned to that stint.

## Auto vs Manual Planning

When `Auto-generate stint plan` is enabled, setup changes regenerate the plan.

When you edit individual stints or pit stops, the planner switches to live recalculation using your current timeline.
