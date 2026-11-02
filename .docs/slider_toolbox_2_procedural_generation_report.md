# Slider Toolbox 2 Procedural Generation Notes

> Historical algorithm reference. The current UI, locked insertion-time workflow, and beat-domain timing across BPM changes are documented in the [editor guide](../docs/editor-guide.md). In particular, equal snap durations need not have equal millisecond lengths across BPM changes.

This document records the first implementation pass for `Slider Toolbox 2` and the procedural slider ideas that can be added later.

## Implemented Procedural Families

`Slider Toolbox 2` now contains multiple procedural generation families. Its UI follows the main Slider Toolbox's animation model: editable start/end parameter columns, frame count, easing, frame duration, frame gap, swap, and copy-to-end controls.

Code entry points:

- UI entry: `osu.Game.Rulesets.Osu/Edit/GenerateToolboxGroup.cs`
- Popover: `osu.Game.Rulesets.Osu/Edit/NoiseSliderToolboxPopover.cs`
- Path generator: `osu.Game.Rulesets.Osu/Edit/NoiseSliderGenerator.cs`
- Defaults: `osu.Game/Configuration/OsuConfigManager.cs`

Current families:

- Vector fields: streamlines integrated through Gaussian, value, fractal value, curl, sine-warp, spiral, or orbital vector fields.
- Implicit contours: superellipse and blobby contour paths.
- Graph paths: Hilbert path and maze-walk paths.
- Attractors: Lorenz attractor and Clifford map projections.
- Stroke grammar: loop chain, zigzag ribbon, and arc chain paths.
- Mask tracing: procedural noise-mask contour tracing.
- Beat driven: beat-pulse wave paths.

For the vector-field family, the generator works in four stages:

1. Choose a seed point.
   `Seed pattern` selects a deterministic seed distribution, such as centre, playfield edges, ring, spiral, or random. `Seed position` chooses the concrete starting point inside that distribution. This is intentionally separate from `Frames`: frames decide how many sliders are generated, while the seed controls where each streamline starts in the vector field.

2. Evaluate the vector field.
   The selected field returns a direction at any playfield coordinate. Noise fields convert scalar noise into direction fields; curl fields use finite-difference curl; spiral and orbital fields combine radial/tangential components with deterministic noise.

3. Integrate a streamline.
   Each generated slider is an integral curve of the vector field. The implementation uses midpoint integration: sample the field at the current point, step halfway, sample again, then advance. Vector-field paths are traced forward from the seed point until their arc length reaches `Flow length`; the seed point remains the slider head and is not re-centered or scaled away. `Field bias` blends a user-selected direction into the sampled field when a more directed flow is needed.

4. Normalize, smooth, and clean.
   The field integration happens in an independent field domain. `Field width` and `Field height` control that domain, not direct screen placement. Non-vector generated paths are normalized to `Source length`; vector-field paths already use `Flow length` during forward tracing. Smoothing and duplicate/invalid point cleanup are then applied.

The popover then creates actual slider objects using the same duration model as the main Slider Toolbox: every generated frame has equal time length in units of the current snap divisor, and the slider velocity multiplier is calculated from the generated path length. Each frame creates exactly one slider, so `Frames` is also the final slider count.

## Current UI Parameters

- Category: top-level procedural family.
- Type: concrete generator inside the selected family. The dropdown is filtered by the active category to avoid a long flat list.
- Seed pattern: distribution used to choose the streamline starting point.
- Noise: vector-field construction mode for the noise-field family.
- Motion curve: easing preset used to interpolate start parameters to end parameters.
- Frames: number of generated timeline frames and final slider count.
- Seed position: continuous position inside the selected seed pattern.
- Seed angle: direction used by field bias and by non-field generators that still use a rotation-like parameter.
- Field bias: amount of user-directed flow blended into vector-field generation.
- Source length / Flow length: target generated path length before placement transforms. In vector fields this is the forward arc length traced from the seed point.
- Samples: control point density per slider path.
- Field width / Field height: field sampling domain size or generator aspect domain.
- Amplitude: field step strength / visual scale.
- Frequency: spatial frequency of the noise field.
- Octaves / Persistence: fractal noise detail controls.
- Smooth: number of smoothing passes after generation.
- Seed: deterministic random seed.
- Scale / Offset / Rotation turns / Rotation degrees: final object transform.
- Frame duration / Frame gap: time spacing in current snap-divisor units.
- Opacity: preview panel opacity, shared with the main Slider Toolbox.

When `Frames` is `1`, end-frame controls are visually dimmed and input-blocked. Their values are preserved, but generation uses only the start-frame values.

The parameter panel uses two compact columns of start/end controls. Captions are adjusted per category so the same stored parameter can read as "flow strength", "contour force", "order", "time step", or "repeats" depending on the active generator.

The right parameter panel separates source-generation controls from placement controls. The left preview intentionally ignores placement transforms (`Scale`, `Offset`, and `Rotation`) so it stays focused on the generated source path and field behavior. Placement is applied only when creating the actual slider objects.

## Visual Preview

The left preview pane is intentionally approximate and lightweight:

- pale short line segments show a coarse vector-field sample when the active family uses a vector field;
- non-field families show lightweight construction guides such as bounds, grids, axes, baselines, or repeat markers;
- blue dots/lines show the start-frame generated streamline or path;
- orange dots/lines show the end-frame generated streamline or path when `Frames > 1`.

The preview limits the number of displayed samples so parameter edits stay responsive. It is meant to show the broad direction of parameter changes, not exact in-editor rendering or stable-export results.

## Stable Export Considerations

Dense generated control points look good in lazer, but stable export quantizes coordinates to integers. For noise-generated sliders this can make high-frequency noise look jagged after export.

Recommended usage for stable-targeted work:

- Prefer lower frequency and moderate smoothing.
- Increase samples only when the shape needs it; dense high-frequency noise is the worst case after integer quantization.
- Keep amplitude large enough that integer rounding does not erase the intended field motion.
- The experimental optimized exporter was removed. Use the normal compatibility export and inspect highly variable slider velocity cases in the target client.

## Additional Procedural Families Worth Adding

Most of the initially researched families are now represented in code. Remaining possible extensions are listed below.

### Vector Field / Flow Lines

Current implementation includes curl, spiral, and orbital flow lines. Possible future additions:

- Perlin/simplex/curl flow fields.
- Spiral sink/source fields.
- Magnetic or orbital fields around configurable attractors.
- Blended fields where each attractor has position, strength, and radius.

This would be useful for natural swirls, streamlines, and turbulence-like slider groups.

### Signed Distance Field Contours

Current implementation includes direct sampled superellipse and blobby contours. Possible future additions:

- Blobby metaballs.
- Superellipses and rounded polygons.
- Boolean unions/intersections of circles, boxes, and capsules.
- Offset contours around an existing symbol or generated shape.

This is good for closed shapes and layered outlines.

### Graph And Maze Paths

Current implementation includes Hilbert paths and maze walks. Possible future additions:

- Maze solution paths.
- Hilbert/Peano space-filling curves.
- Voronoi ridge walks.
- Delaunay edge tours.
- Minimum spanning tree or traveling-salesman-like paths through points.

This gives structured complexity without needing thousands of random control points.

### Attractors And Dynamical Systems

Current implementation includes Lorenz and Clifford trajectories. Possible future additions:

- Lorenz, Rossler, Halvorsen, Thomas, and other strange attractors.
- Clifford, De Jong, Ikeda, and Gumowski-Mira maps.
- Pendulum or spring simulation traces.

These are visually rich and expose well as seed, step count, scale, and projection parameters.

### Image Or Mask Tracing

Current implementation includes a procedural noise-mask trace. Possible future additions:

- Edge detection followed by contour tracing.
- Thresholded grayscale masks.
- Skeletonized strokes.
- Posterized image layers.

This can eventually support logo-like or handwriting-like slider generation, but it needs stronger UI and validation than the current generator.

### Parametric Stroke Grammars

Current implementation includes loop chain, zigzag ribbon, and arc chain strokes. Possible future additions:

- Repeated arcs, hooks, loops, and zigzags.
- Calligraphic stroke templates.
- Radial or bilateral symmetry modifiers.
- L-system variants beyond the current fractal set.

This is likely the best direction for usable "designed" shapes because the output stays predictable and editable.

### Audio Or Beat-Driven Curves

Current implementation includes a beat-pulse wave generator. Possible future additions:

- Waveform/envelope curves.
- Frequency-band traces.
- Beat-synced radial pulses.
- Control-point density driven by local timing or SV intent.

This could pair well with animation/sequence generation, but needs careful timeline integration.

## Implementation Recommendations

- Keep `Slider Toolbox 2` separate from the existing Slider Toolbox while the procedural models are experimental.
- Keep common popover mechanics in reusable helpers where practical. `SliderToolboxUiUtils` currently contains shared easing, interpolation, snap formatting, swap/copy, and single-frame input-blocking utilities.
- Prefer deterministic generators with explicit seeds so generated work is reproducible.
- For any generator that can create very dense paths, expose both sample count and smoothing.
- For stable compatibility, add optional post-generation simplification as a user-controlled operation rather than hiding it in the generator.
- When adding families with many subtypes, use the same grouped UI pattern already used by the main Slider Toolbox instead of adding a long flat dropdown.
