# UTAU_Editor v14 Changelog

Date: 2026-10-18

## Slider Toolbox 3

- Added a Generate sidebar tool to import external dance pose sequences as native slider animation frames.
- Added locked timeline start/end positions, source-frame sampling density, configurable snap gaps, positioning, scaling, and live preview.
- Optimised skeleton traversal to reduce retracing and validate required slider velocity.
- Added JSON drag import and an embedded file browser.
- Added the Python pose extraction companion with frame-by-frame playback and confidence diagnostics. Its versioned source is in `tools/pose-slider-companion`.
- Fixed preview callback recursion, file-selector popup dismissal, Tk worker-thread access, and slider range binding constraints.

## Verification

- Focused pose generation tests: 7 passed.
- Companion tests: 5 passed; Python syntax checks passed.
- Editor project build passed. Visual acceptance remains manual.
