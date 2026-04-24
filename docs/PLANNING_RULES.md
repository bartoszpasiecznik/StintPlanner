# Planning Rules

This file describes the race-planning rules currently implemented in the app.

## 1. Driver Caps

- Each driver has a `Max Stints` limit
- A driver with `Assigned Stints >= Max Stints` is not offered for new stint assignments
- A currently assigned driver remains selectable on their own stint

## 2. Fuel Visibility By Car Class

The UI exposes fuel fields based on `Car Class`.

### Hypercar and LMGT3

- hide fuel capacity
- hide fuel per lap

### LMP2, LMP3, and GTE

- show fuel per lap
- fuel stop inputs on pit cards are available

## 3. Refill Timing

The planner does not use refuel rate or VE rate.

Instead it uses one shared value:

- `Full Refill Time (s)`

This is treated as the time needed to refill from `0` to `100`.

Service time uses:

- base pit time
- scaled fuel refill time
- scaled VE refill time
- repair time
- tyre change time

## 4. Tyre Limits

- `Available Tyres` is the only tyre limiter
- The starting tyre set counts as the first set
- Additional tyre changes are limited by remaining available sets
- When the tyre limit is reached, extra `Change All Tyres` toggles are disabled

## 5. Tyre Set Tracking

- Each stint is tagged with a tyre set number
- The tyre set advances only when the previous pit stop changes all tyres
- The active tyre set is displayed on the stint card

## 6. VE Stop Logic

Each pit stop stores a VE amount.

Each stop also has a mode:

- `Add All Needed VE = true`
- `Add All Needed VE = false`

When enabled:

- VE add is automatically set to the VE needed for the next stint
- the value is capped by VE capacity

When disabled:

- VE add is manually editable by the user

## 7. Pit Stops In Timeline

Pit stops are represented between stints.

A pit-stop row is built from the stint that just ended and shows the service required before the next stint begins.

## 8. Notes And Warnings

Validation notes are written in descriptive language rather than shorthand.

Warnings may include:

- missing or invalid driver setup
- driver cap exceeded
- fuel requirement above configured capacity
- VE requirement above configured capacity
- tyre usage above available sets
- projected finish shorter or longer than race target
