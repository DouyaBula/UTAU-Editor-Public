# UTAU Editor

**English** · [简体中文](README.zh-CN.md)

An unofficial, personal [osu!lazer](https://github.com/ppy/osu) fork for experimental mapping, slider art and animation. Shared for anyone who finds the tools useful; this is not an official osu! release and has no affiliation with or support from ppy.

The custom features were developed with substantial LLM assistance and tested through focused automated checks and personal use. They have not had a comprehensive independent code review. Back up important maps.

[Download portable builds](https://github.com/DouyaBula/UTAU-Editor-Public/releases) · [Usage guide (中文)](docs/editor-guide.md) · [Build / release guide](docs/releasing.md) · [Report an issue](https://github.com/DouyaBula/UTAU-Editor-Public/issues)

## What is added?

Most additions target the **osu!standard editor**. The original lazer editor remains the foundation; the following are the custom additions and changes.

| Area | Additional tools |
| --- | --- |
| **Slider drawing** | Switch between Classic control points and Pen-style anchors/direction handles for pure Bézier paths (`Alt+P`). Click for straight segments, drag to create handles; optional Curvature pen. Corner/smooth/symmetric nodes, independent handle editing/retraction, insertion/deletion, subdivision, open/close, reverse, mirror, alignment and distribution. |
| **Classic curves** | Bevel/round corners, smooth/symmetric/retract handles and related node operations on mixed paths, with segment starts distinguished from inherited points. Linear-segment simplification, configurable corner trim, live mirrored preview of the selected slider, and restored Catmull selection. |
| **Contour operations** | Preview union, intersection, difference, XOR and offset/stroke outlines from sampled closed Bézier contours; output ordinary sliders. |
| **Duration and SV** | Keep the full edited path while compensating SV to preserve duration, including control-point edits and scaling. Inherit placement SV from selected/preceding sliders, with manual override. Fixed-duration SV/scale shortcuts and a variable-velocity slider tool with acceleration, easing and detour controls. |
| **Slider Toolbox** | Parametric geometric curves, waves, spirals, fractals, 3D projections and mathematical symbols. Symbol sequences, configurable head/path type, scale/rotation/offset, start/end parameter interpolation, easing and multi-slider animation with snap-based frame duration and gaps. |
| **Slider Toolbox 2** | Procedural vector-field streamlines, noise, implicit contours, graph/maze paths, attractors, stroke grammars, procedural mask contours and beat-driven shapes. Seeded variation, sampling/smoothing and the same animation workflow. |
| **Slider Toolbox 3** | Turn a pose JSON sequence into dancing sliders: one sampled stick figure per native slider. Locked timeline range, source-frame density, empty snaps between poses, positioning and scale. Drag/drop or browse import; a separate [Python companion](tools/pose-slider-companion/README.md) recognises videos and previews the skeleton. |
| **Pattern Gallery** | Save one slider or a selection, browse multi-column previews, search/tag/sort and reuse across maps. Preserve beat spacing, gaps, shapes and slider tails across BPM changes by recalculating SV. JSON library import/export; reject impossible SV rather than silently changing timing. |
| **Selection and transforms** | Numeric movement/rotation/scaling with selectable origins, configurable nudge distance, optional out-of-bounds placement, seeded random perturbation, accelerated/mirrored Convert to stream, insert/split at the timeline cursor and tangent glitches. |
| **Timing and difficulty** | Shift a whole difficulty's timing/effect lines, objects, breaks, bookmarks and preview time by one global offset, with Undo. Overall native star rating plus a continuous local-strain curve on the bottom summary timeline. Manual/save-triggered calculation, configurable position, opacity, height and foreground/background layer. |
| **Hitsounds** | Copy/merge hitsounds between difficulties; mute slider tails, batch volume edits and 5% silent samples. Export MIDI or note/mania references; import MIDI and mania-column hitsounds with mappings and matching tolerance. |
| **Canvas and appearance** | Canvas zoom/pan; precise grid opacity; slider body toggle (on by default); configurable selection outlines, colours, bounding-box buttons/handles, control-point display and hit area, control lines and tail arcs. Coarse/detail display for dense sliders, quieter hit/selection effects and fine background dimming. |
| **Workflow** | Persistent sidebar ordering, quick two-value AR toggle, configurable snap targets, lower tick-rate range and mapping-oriented defaults. Bounded scrollable tool panels, task pages, responsive layouts and fixed Apply/Cancel for generated previews. |

### Where to find them

- **Left sidebar → UTAU Editor:** canvas, AR, slider detail/curves, difficulty display, Hitsound tools.
- **Right sidebar → Generate:** Polygon, Slider Toolbox 1/2/3, Pattern Gallery. **Transform / GRID:** precise transforms and grid controls.
- **Top bar → UTAU_Editor:** appearance, snapping, workflow parameters and sidebar ordering. **Timing page:** Global offset.
- **Object / node context menus:** local curve operations, stream conversion, perturbation, SV and split tools.

Preview tools use **Apply / Cancel** and Undo. Finish a preview before saving. Shape generators lock their insertion time until **Use timeline cursor** is clicked. Difficulty calculation runs only when requested or on save, avoiding background recalculation during every edit.

## Portable builds

The Release workflow targets Windows x64/ARM64, Linux x64/ARM64 and macOS Intel/Apple Silicon. Download the asset matching your system, extract it fully into a writable folder, then run `osu!.exe` on Windows or `./launch.sh` on Linux/macOS. No separate .NET installation is needed. These are unsigned personal builds; macOS builds are not notarised. Linux still requires desktop, graphics and audio system libraries.

Keep `utau-editor.portable` beside the executable. Maps, skins, bindings, editor configuration and Gallery are stored in the package's **`data/`** directory; framework settings may also live beside the executable. There is no official auto-update or file-association takeover. Update by extracting a new release and moving your `data/` and optional `framework.ini` while both versions are closed. See [portable usage](docs/portable.md).

Use `.osz` / `.osk` imports to copy content from an existing installation. **Do not point this build at the official client's live data directory or copy its Realm database**: their schema and binding IDs can differ. The separate pose companion is optional and requires Python plus its own dependencies.

## Compatibility and scope

Maps remain native osu! data and use the standard compatibility `.osz` export. Gallery and pose JSON are auxiliary files, not a new beatmap format. Pen handles are for pure Bézier paths; Classic supports other and mixed path types. Dense artwork, very high SV and integer-coordinate conversion can behave differently in stable—check the exported map in the intended client. The retired “slider optimization” exporter is not included.

This fork is intended for editing and visual experimentation, not as a supported replacement for official online play. No promise of ranked-map acceptance or identical visuals in every client is made.

## Build and upstream updates

Install the SDK selected by `global.json` (**.NET 10**, C# 14-capable IDE), then:

```sh
dotnet run --project osu.Desktop -c Debug
# Example self-contained portable build:
dotnet publish osu.Desktop -c Release -r win-x64 --self-contained true -o artifacts/portable
```

The public history retains official commits and introduces the custom editor as one initial snapshot. Future updates use normal `git fetch upstream` / `git merge upstream/master`; see [release and upstream maintenance](docs/releasing.md). The [maintainer guide](docs/maintainer-guide.md) describes shared UI, previews/Undo and focused tests.

## Credits and licence

Based on [osu!lazer](https://github.com/ppy/osu) and [osu!framework](https://github.com/ppy/osu-framework). Pattern Gallery and strain visualisation were informed by [Mapping Tools](https://github.com/OliBomby/Mapping_Tools) and [MapsetVerifier](https://github.com/Naxesss/MapsetVerifier); the editor workflows are adapted for this fork.

Code retains the upstream [MIT licence](LICENCE). Bundled resources, fonts and native libraries have their own terms; see [third-party notices](THIRD_PARTY_NOTICES.md). In particular, osu!resources is largely **CC BY-NC 4.0**, and osu!/ppy branding is not covered by the code licence. This project is shared non-commercially.
