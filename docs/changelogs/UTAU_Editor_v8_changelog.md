# UTAU_Editor v8 Changelog

Release date: 2026-06-16

This version focuses on high-control-point slider usability and continued improvements to Shape slider / Shape animation generation.

## High-Control-Point Slider Display

Large sliders with thousands of control points now automatically use a rough display mode while selected.

- Rough mode draws a downsampled control line.
- Rough mode hides intermediate editable control points and keeps only lightweight slider selection visuals.
- Detailed mode remains available when precise point editing is needed.
- The switch and all parameters are exposed in the editor sidebar, not the global settings screen.

### UI Entry

- Left sidebar: `UTAU Editor > Slider detail`

### Sidebar Controls

- `Detailed high-density sliders`
  - Enables full editable control point rendering even for dense sliders.
- `Detailed point limit`
  - Sliders at or below this control point count use detailed editable mode automatically.
  - Sliders above this value use rough mode unless detailed mode is enabled.
- `Rough line samples`
  - Maximum number of sampled points used to draw the rough control line.

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/SliderDetailPopover.cs`
  - Sidebar popover for the display mode and thresholds.
- `osu.Game.Rulesets.Osu/Edit/UTAUEditorToolboxGroup.cs`
  - Adds the `Slider detail` sidebar button.
- `osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/SliderSelectionBlueprint.cs`
  - Chooses detailed or rough visuals for selected sliders.
  - Hides `Add control point` in rough mode because intermediate control points are not editable there.
- `osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/Components/PathControlPointConnection.cs`
  - Adds downsampled rough control line rendering.

### Config Entries

- `EditorSliderDetailedControlPointMode`
- `EditorSliderDetailedControlPointLimit`
- `EditorSliderRoughControlLineMaxPoints`

## Performance Analysis Document

Added a dedicated analysis note for the selected-slider slowdown caused by massive control point counts.

- `docs/UTAU_Editor_slider_control_point_performance_analysis.md`

The report identifies the main bottleneck as selection blueprint / control point visual rendering rather than geometric transform calculation.

## Shape Slider Head Position

Closed-path `Head position` is no longer limited to `0-100%`.

- Values are now shown as turns.
- `1 turn` means one full loop around the closed path.
- Values above `1` support multi-loop animation.
- Example: `Start head = 0`, `End head = 3` animates the head through three full loops.

### UI Entry

- `Generate > Shape slider > Head position`
- `Generate > Shape animation > Start head / End head`

### Config Entries

- `EditorParametricSliderHeadPosition`
- `EditorParametricSliderAnimationEndHeadPosition`

Both now support `0-8 turns`.

## Shape Animation Frame Gap

`Frame gap` now uses the current snap divisor, matching `Frame duration`.

- Old behavior: gap was measured in beats.
- New behavior: gap is measured in snaps.
- Calculation now uses:

```text
TimingPoint.BeatLength / current BeatDivisor * FrameGapSnaps
```

### UI Entry

- `Generate > Shape animation > Frame gap`

### Config Entry

- `EditorParametricSliderAnimationFrameGapSnaps`

This replaces the old beat-based `EditorParametricSliderAnimationFrameGap` config entry to avoid reinterpreting old saved beat values as snap counts.

## Shape Parameter Range

The shared Shape slider primary and secondary parameters now support larger values.

- Old range: `1-12`
- New range: `1-32`

This affects parameters such as:

- `Cycles`
- `Petals`
- `Cusps`
- `Lobes`
- `Symmetry`
- Frequency-style parameters
- 3D winding parameters

### Affected UI

- `Generate > Shape slider`
- `Generate > Shape animation`

### Config Entries

- `EditorParametricSliderPrimaryParameter`
- `EditorParametricSliderSecondaryParameter`
- `EditorParametricSliderAnimationEndPrimaryParameter`
- `EditorParametricSliderAnimationEndSecondaryParameter`

## New Shape Slider Types

Added 16 new parameterized Shape slider types.

### Curves

- `Damped sine wave`
- `Sinc curve`
- `Superellipse`
- `Limacon`
- `Nephroid`
- `Deltoid`
- `Epispiral`
- `Gear curve`
- `Maurer rose`
- `Chebyshev curve`
- `Harmonograph`
- `Golden spiral`
- `Lituus spiral`
- `Spiral wave`

### 3D

- `Torus knot`
- `Spherical helix`

The new types appear automatically in both:

- `Generate > Shape slider`
- `Generate > Shape animation`

## 3D Shape Controls

3D parameter controls are now available to all 3D shape types.

- `Quaternion X / Y / Z / W`
  - Controls 3D rotation.
- `Perspective`
  - Controls projection depth distortion.
- `Faces`
  - Still only appears for `Regular polyhedron`.

## Core Code Entry

- `osu.Game.Rulesets.Osu/Edit/ParametricSliderGenerator.cs`
  - Shape enum expansion.
  - New curve and 3D generation formulas.
  - Parameter metadata and category routing.
  - Larger primary / secondary parameter clamp.
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderGenerationPopover.cs`
  - Shape slider UI ranges.
  - New shape display names.
  - Head position turn display.
  - 3D control visibility.
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderAnimationPopover.cs`
  - Animation UI ranges.
  - New shape display names.
  - Head position turn display.
  - Frame gap snap handling.
  - 3D control visibility.
- `osu.Game/Configuration/OsuConfigManager.cs`
  - New defaults and persistent config entries.

## Verification Status

Per current development convention, this round did not run build or runtime tests.

Completed lightweight static checks:

- Confirmed old `0-100%` head-position UI limits were replaced for Shape slider and Shape animation.
- Confirmed old beat-based frame gap config name is no longer referenced.
- Confirmed old `1-12` Shape primary / secondary parameter limits were removed from Shape generation paths.
- Confirmed brace counts remain balanced in the touched files.
- `git diff --check` only reports line-ending warnings.
