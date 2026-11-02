# UTAU_Editor v12 Changelog

Release date: 2026-06-25

This version completes a deeper `Slider Toolbox 2` pass: non-vector procedural families are expanded, slider head path type is configurable, and parameter behavior is cleaned up so the UI is easier to test and reason about.

## Slider Toolbox 2 Shape Expansion

Expanded the non-vector procedural families that were previously only rough first-pass implementations.

### New / Expanded Types

`Implicit contours`

- `Rounded polygon`
- `Metaball contour`
- `Boolean field contour`

`Graph paths`

- `Peano path`
- `Maze solution`
- `Voronoi ridge walk`
- `Point network tour`

`Attractors`

- `Rossler attractor`
- `Halvorsen attractor`
- `Thomas attractor`
- `De Jong map`
- `Ikeda map`
- `Gumowski-Mira map`
- `Spring pendulum`

`Stroke grammar`

- `Hook chain`
- `Calligraphy stroke`
- `Radial rosette`
- `L-system stroke`

`Mask tracing`

- `Layered mask contour`

`Beat driven`

- `Envelope wave`
- `Radial pulse`

### Design Notes

- Similar planned graph/network styles were consolidated into `Point network tour` instead of exposing many near-duplicate route types.
- Each new type is deterministic through the existing `Seed` parameter where randomness is involved.
- The existing grouped `Category > Type` UI is reused; no new sidebar entry was added.

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/NoiseSliderGenerator.cs`
  - Adds enum entries, category routing, and generation functions for the new types.

## Slider Head Path Type

`Slider Toolbox 2` now exposes the same first-control-point path type option as the main `Slider Toolbox`.

Available options:

- `Linear`
- `Bezier`
- `Perfect curve`
- `B-spline`
- `Catmull`

### UI Entry

- Left sidebar: `Generate > Slider Toolbox 2`
- Top control row: `Head`

### Config Entry

- `EditorNoiseSliderHeadControlPointType`

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/NoiseSliderToolboxPopover.cs`
  - Adds the `Head` dropdown and applies it when building the first `PathControlPoint`.
- `osu.Game/Configuration/OsuConfigManager.cs`
  - Adds the default persisted value.

## Parameter Semantics Cleanup

The `Slider Toolbox 2` parameter model was adjusted so common controls behave more predictably across categories.

### Sampling Model

- Shape generation now uses a fixed high-detail internal source path.
- `Smooth` is applied to that internal path.
- `Samples` is applied last by arc-length resampling.
- This means `Samples` should control control-point density only, not change the intended source shape.

This specifically fixes cases such as attractor generators where changing `Samples` previously changed the trajectory shape.

### Attractor Parameters

- `Warm offset`
  - Now changes how much simulated trajectory is skipped before collecting the displayed path.
- `Projection`
  - Now changes the 2D projection of 3D attractors.
  - For 2D maps, it applies a projection skew.
- `Time step`, `Parameter`, and `Parameter B`
  - Continue to control system-specific dynamics.

### Non-Attractor Parameters

- `Phase` / `Route offset`
  - Now shifts the generated path start point.
  - Closed paths rotate around the loop.
  - Open paths offset by walking from the offset point to the end, then back toward the start, avoiding a direct tail-to-head jump.
- `Contour skew`, `Route bend`, `Stroke slant`, `Mask skew`, and `Wave tilt`
  - Now apply visible category-specific post-generation bias.
- `Path length`
  - Normalizes generated source length before placement transforms.

### UI Caption Updates

Parameter names were revised per category so they better match actual behavior.

Examples:

- `Field width / height` becomes category-specific names such as `Source width`, `Shape width`, `Grid width`, `Output width`, `Stroke width`, `Mask width`, or `Wave width`.
- Attractor `Projection bias` is now shown as `Projection`.
- Non-field categories use `Path length` instead of field-oriented wording.

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/NoiseSliderGenerator.cs`
  - Separates internal source generation from final output sampling.
  - Adds path offset, non-field bias, and attractor projection helpers.
- `osu.Game.Rulesets.Osu/Edit/NoiseSliderToolboxPopover.cs`
  - Updates category-specific captions.

## Testing / Debug Documentation

Added one-off local debugging documentation for parameter testing.

The one-off guide explained categories with SVG diagrams. It was local debugging material and is not distributed. Current references are indexed in [docs](../README.md).

## UI Entry Summary

- Left sidebar: `Generate > Slider Toolbox 2`
- Top row:
  - `Category`
  - `Type`
  - `Head`
  - `Curve`
  - `Opacity`
- Source columns:
  - `Start frame`
  - `End frame`
- Bottom row:
  - `Frames`
  - `Duration`
  - `Gap`
  - `Swap`
  - `Copy > End`
  - `Generate`

## Verification Status

Per current instruction, no build or runtime test was run for this version.

Completed lightweight checks:

- `git diff --check` for modified source files.
- XML parse check for the local debugging SVG diagrams.
