# UI Contract

This file describes the current intended UI behavior that future agents should preserve unless the user asks otherwise.

## Overall Layout

- Left side: setup tabs
- Right side: summary, checks, and compact planning timeline

## Left Side

Tabs:

- `Race`
- `Car`
- `Weather`
- `Drivers`

These tabs are the only primary configuration navigation on the left side.

## Right Side

The right side should remain stable and focused on plan output:

- listed summary
- warnings/checks
- compact card timeline

Do not replace it with a large spreadsheet-style layout unless explicitly requested.

## Stint Cards

Stint cards should show:

- stint number
- driver
- start and end timing
- local timing
- laps
- drive time
- tyre set
- first-stint `Get Quali Tyres` toggle
- notes

Stint cards should not show:

- `Change All Tyres`
- pit-only service controls

## Pit Cards

Pit cards should show:

- pit identifier
- stop window
- pit lane time
- service time
- total pit time
- fuel add when applicable
- VE add
- VE auto-fill toggle
- repair time
- `Change All Tyres`
- notes

Pit cards should not show tyre set ownership as the primary tyre label. Tyre set is shown on the stint using it.

## Compactness

The user explicitly requested smaller cards.

Preserve these characteristics:

- tight padding
- reduced spacing
- compact info chips
- narrow editor widths where practical
- readable but dense layout
- different drivers' stint cards must use distinct colors
- driver colors are assigned from driver list order and should not collide for visible configured drivers

## Drivers Tab

The drivers tab must remain easy to use in the constrained left panel.

Avoid layouts where rows or controls become partially hidden or hard to scroll.

It must include per-driver `VE Per Lap` input because energy planning now depends on driver-specific values.

New drivers should default their time zone selector to the user's PC time zone.

## Visibility Rules

### Fuel

Fuel input visibility on pit cards must match the car tab fuel rules.

### Tyres

`Change All Tyres` is only available on pit cards.

### VE

VE manual entry must disable when auto-fill is enabled.

VE planning must be derived from the next stint driver’s own `VE Per Lap` value.
