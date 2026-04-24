# Domain Reference

This file defines the intended meaning of the main planning data used by the app.

## Main Concepts

### Driver

A driver has:

- name
- lap time
- VE per lap
- time zone
- max stints
- assigned stints

`AssignedStints` is a calculated value and should not be treated as primary user input.

### Stint

A stint represents one driver run on track.

It stores:

- stint number
- driver
- laps
- drive timing
- race start and end
- driver local window
- tyre set used during that stint
- validation notes

### Pit Stop

Pit stops are not separate underlying domain objects.

Instead:

- a pit-stop timeline row is derived from a `StintPlan`
- that row represents the service after that stint and before the next stint

Pit-stop editable values currently live on the associated `StintPlan`.

These include:

- fuel add
- VE add
- auto-fill VE toggle
- repair time
- change all tyres

## Resource Semantics

### Fuel

Fuel add is a pit-stop input only.

Its visibility depends on car class.

### VE

VE add is a pit-stop input only.

It supports two modes:

- auto-fill all VE needed for next stint
- manual entry

The VE need for a stint is calculated from the assigned driver’s `VE Per Lap`.

### Tyres

`Available Tyres` is the race-wide limiter.

The app treats tyre sets as a race resource:

- the qualifying set is included in the tyre limit
- if the first stint uses qualifying tyres, that first race stint runs on tyre set 1
- if the first stint does not use qualifying tyres, the first race stint starts on tyre set 2
- enabling `Change All Tyres` consumes another set
- each stint displays the tyre set it runs on

## Timeline Semantics

The right-side plan is a mixed sequence:

- stint card
- pit-stop card
- stint card
- pit-stop card

There is no pit card after the final stint.

## Summary Semantics

The summary is intended to present high-level planning status:

- race setup
- lap projection
- stint count
- tyre usage
- projected finish

Warnings are for conflicts or invalid assumptions, not general narration.
