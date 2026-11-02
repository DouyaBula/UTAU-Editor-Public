# UTAU_Editor v13 Changelog

Release date: 2026-09-28

This update adds editor-side difficulty visualization and a toggle for slider body rendering.

## Difficulty Display

The editor can now show the beatmap's overall star rating and a local difficulty/strain curve across the bottom summary timeline. Difficulty is calculated on demand in the background rather than recalculated after every edit, avoiding the edit and undo stutters caused by continuous calculation.

### Display and Controls

- Overall star rating is shown in the upper-right of the editor playfield; its position can be moved to the upper-left.
- The local difficulty curve is drawn on the bottom summary timeline.
- The curve can be shown above or below other timeline markers; it is above them by default.
- Curve visibility, opacity, height, sample interval, and rating visibility/position are configurable.
- Difficulty can be refreshed manually. An option also refreshes it after saving with `Ctrl+S`.

### UI Entry

- Left sidebar: `UTAU Editor > Difficulty display`

### Implementation Notes

- Overall rating uses the existing osu! difficulty calculator.
- The timeline profile is built from per-skill local difficulty intervals, resampled into time buckets, and combined into a continuous curve.
- Long maps widen the sampling interval as needed to bound the number of plotted points.

## Slider Body Rendering Toggle

Added an editor-only switch to hide or show slider body fill while preserving the slider border. It is enabled by default and does not change gameplay rendering.

### UI Entry

- Top menu: `UTAU Editor > Render slider body`

## Verification

- Difficulty timeline visual test: verifies curve geometry, default top rendering order, and live order switching.
- Difficulty timeline sampling tests: verify local peak resampling, short-interval visibility, and bounded long-map sampling.
- Slider body tests: verify fill toggling while keeping the border visible.
