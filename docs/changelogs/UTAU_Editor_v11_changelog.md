# UTAU_Editor v11 Changelog

Release date: 2026-06-25

This version adds `Slider Toolbox 2`, a procedural slider generator focused on noise, vector-field, contour, graph, attractor, and stroke-based paths. It also refines the tool's animation workflow, preview behavior, and parameter layout.

## Slider Toolbox 2

Added a second slider generation tool for procedural paths that are not covered well by the shape/formula-oriented `Slider Toolbox`.

### Supported Categories

- `Vector fields`
  - `Noise sampled path`
  - `Curl flow line`
  - `Spiral flow line`
  - `Orbital flow line`
- `Implicit contours`
  - `Superellipse contour`
  - `Blobby contour`
- `Graph paths`
  - `Hilbert path`
  - `Maze walk`
- `Attractors`
  - `Lorenz attractor`
  - `Clifford map`
- `Stroke grammar`
  - `Loop chain`
  - `Zigzag ribbon`
  - `Arc chain`
- `Mask tracing`
  - `Noise mask trace`
- `Beat driven`
  - `Beat pulse wave`

### UI Entry

- Left sidebar: `Generate > Slider Toolbox 2`

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/GenerateToolboxGroup.cs`
  - Adds the `Slider Toolbox 2` button.
- `osu.Game.Rulesets.Osu/Edit/NoiseSliderToolboxPopover.cs`
  - Implements the tool panel, preview, generation command, animation controls, and config binding.
- `osu.Game.Rulesets.Osu/Edit/NoiseSliderGenerator.cs`
  - Implements procedural path generation.

## Vector Field Streamline Workflow

The vector-field generator now follows a clearer seed-to-flow model.

- `Seed pattern` chooses where the start point comes from.
- `Seed position` selects the exact start point within that pattern.
- `Flow length` controls how far the slider travels forward from the seed.
- The seed point is treated as the slider head and remains fixed when changing `Flow length`.
- Vector-field paths now integrate forward along the field direction instead of behaving like a centered/bidirectional path.
- Non-vector procedural types keep the older normalized-length behavior.

### Main Vector Field Parameters

- `Field`
  - Selects the underlying field/noise model.
- `Seed angle`
  - Direction used by the bias control.
- `Field bias`
  - Blends the generated field toward the `Seed angle`.
- `Field width` / `Field height`
  - Defines the working field area.
- `Field scale`
  - Controls spatial frequency of the generated field.
- `Field detail`
  - Increases both preview vector density and integration precision.
- `Field pull`
  - Controls how strongly the current vector-field direction pulls the streamline away from its previous direction.
- `Samples`
  - Controls output control point count.

## Animation And Frame Controls

`Slider Toolbox 2` supports both single-slider generation and multi-frame slider sequences.

- `Frames = 1` generates one slider.
- `Frames > 1` interpolates start/end parameters across a slider sequence.
- `Frame duration` and `Frame gap` use snap-count semantics.
- Easing is shared with the existing slider generation workflow.
- Added helper buttons:
  - `Swap`
    - Swaps all start-frame and end-frame parameter values.
  - `Copy > End`
    - Copies all start-frame parameter values to the end frame.

### UI Layout

- Parameters are grouped by frame instead of alternating start/end controls row by row.
- The panel now uses:
  - left column: `Start frame`
  - right column: `End frame`
- Each frame column contains:
  - `Source / field`
  - `Placement`
- End-frame controls are visually disabled and input-blocked when `Frames = 1`.

### Shared UI Utilities

- `osu.Game.Rulesets.Osu/Edit/SliderToolboxUiUtils.cs`
  - Adds shared interpolation, easing, swap/copy helpers, snap-count formatting, and single-frame inactive containers.

## Preview Updates

The left preview panel now better reflects the generation model.

- Vector-field preview keeps the seed/head position stable when changing `Flow length`.
- Field vector density increases with `Field detail`.
- The generated preview path is drawn together with lightweight field/sample visualization.
- Preview behavior prioritizes responsiveness over exact full-resolution simulation.

## Config Entries

Added persistent defaults for `Slider Toolbox 2`.

- `EditorNoiseSliderCategory`
- `EditorNoiseSliderType`
- `EditorNoiseSliderSeedPattern`
- `EditorNoiseSliderNoiseType`
- `EditorNoiseSliderSeedPosition`
- `EditorNoiseSliderSeedAngle`
- `EditorNoiseSliderFieldBias`
- `EditorNoiseSliderStreamlineLength`
- `EditorNoiseSliderSamples`
- `EditorNoiseSliderWidth`
- `EditorNoiseSliderHeight`
- `EditorNoiseSliderAmplitude`
- `EditorNoiseSliderFrequency`
- `EditorNoiseSliderOctaves`
- `EditorNoiseSliderPersistence`
- `EditorNoiseSliderSmoothing`
- `EditorNoiseSliderSeed`
- `EditorNoiseSliderScale`
- `EditorNoiseSliderRotationTurns`
- `EditorNoiseSliderRotationDegrees`
- `EditorNoiseSliderOffsetX`
- `EditorNoiseSliderOffsetY`
- `EditorNoiseSliderAnimationEasing`
- `EditorNoiseSliderAnimationFrames`
- `EditorNoiseSliderAnimationEndSeedPosition`
- `EditorNoiseSliderAnimationEndSeedAngle`
- `EditorNoiseSliderAnimationEndFieldBias`
- `EditorNoiseSliderAnimationEndStreamlineLength`
- `EditorNoiseSliderAnimationEndSamples`
- `EditorNoiseSliderAnimationEndWidth`
- `EditorNoiseSliderAnimationEndHeight`
- `EditorNoiseSliderAnimationEndAmplitude`
- `EditorNoiseSliderAnimationEndFrequency`
- `EditorNoiseSliderAnimationEndOctaves`
- `EditorNoiseSliderAnimationEndPersistence`
- `EditorNoiseSliderAnimationEndSmoothing`
- `EditorNoiseSliderAnimationEndSeed`
- `EditorNoiseSliderAnimationEndScale`
- `EditorNoiseSliderAnimationEndRotationTurns`
- `EditorNoiseSliderAnimationEndRotationDegrees`
- `EditorNoiseSliderAnimationEndOffsetX`
- `EditorNoiseSliderAnimationEndOffsetY`
- `EditorNoiseSliderFrameDurationSnaps`
- `EditorNoiseSliderFrameGapSnaps`

### Config Code Entry

- `osu.Game/Configuration/OsuConfigManager.cs`

## Documentation

Added an algorithm note for the procedural generator:

- `.docs/slider_toolbox_2_procedural_generation_report.md`

## Verification Status

This changelog-writing pass did not run build or runtime tests.
