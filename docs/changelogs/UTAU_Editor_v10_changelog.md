# UTAU_Editor v10 Changelog

Release date: 2026-06-19

This version focuses on expanding `Slider Toolbox` shape coverage, improving mathematical symbol generation, and adding visual symbol reference documentation.

## Slider Toolbox Shape Expansion

`Slider Toolbox` now supports a broader set of generated slider shapes.

- The old `Polygon` category has been reorganized into `Basic`.
- Added common closed/basic shapes such as rectangle, diamond, parallelogram, rhombus, trapezoid, kite, star polygon, starburst, cross, chevron, and arrowhead.
- Added more function-style curves, including probability distributions, wavelets, decay/sigmoid/activation curves, and additional parametric/spiral variants.
- Shape category tabs were reorganized to reduce overcrowding:
  - `BASIC`
  - `SYMBOLS`
  - `WAVES`
  - `FUNCTIONS`
  - `PARAMETRIC`
  - `SPIRALS`
  - `FRACTALS`
  - `3D`

### UI Entry

- Left sidebar: `Generate > Slider Toolbox`

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/ParametricSliderGenerator.cs`
  - Adds the new shape enum entries, category routing, parameter names, and shape sampling logic.
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderAnimationPopover.cs`
  - Updates the shape grid layout and category tabs for the expanded shape set.

## Mathematical Symbol Generation

`Slider Toolbox` can now generate symbols from an embedded mathematical font instead of relying on hand-made vector paths.

- Added `NotoSansMath-Regular.ttf` as the symbol outline source.
- Added a TrueType outline reader for converting glyph contours into slider paths.
- Symbol paths support multi-contour glyphs by allowing connecting/retrace path segments, so non-one-stroke symbols keep their overall visual form.
- Closed symbol-like paths support `Start head` adjustment.

### Supported Symbol Groups

- Latin uppercase / lowercase
- Italic uppercase / lowercase
- Bold uppercase / lowercase
- Bold italic uppercase / lowercase
- Script uppercase / lowercase
- Fraktur uppercase / lowercase
- Sans-serif uppercase / lowercase
- Sans-serif bold uppercase / lowercase
- Monospace uppercase / lowercase
- Blackboard letters
- Greek uppercase / lowercase
- Hebrew letters
- Named math letters
- Calculus
- Operators
- Relations
- Set / logic
- Arrows
- Delimiters

### UI Entry

- `Generate > Slider Toolbox > SYMBOLS`
- Select a symbol group on the left, then enter the target token in the right-side `Symbol` field.

Examples:

```text
A
R
alpha
aleph
int
subset
to
```

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/MathSymbolFontPath.cs`
  - Loads TrueType glyph outlines and samples them into slider path points.
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderGenerator.cs`
  - Maps symbol group + input token to Unicode codepoints.
  - Routes symbol generation through `MathSymbolFontPath`.
- `osu.Game.Rulesets.Osu/osu.Game.Rulesets.Osu.csproj`
  - Embeds the symbol font resources.

### Font Resource

- `osu.Game.Rulesets.Osu/Edit/Resources/Fonts/NotoSansMath-Regular.ttf`
- `osu.Game.Rulesets.Osu/Edit/Resources/Fonts/NotoSansMath-OFL.txt`

## Symbol Sequence Generation

Added a non-animation symbol sequence mode for quickly generating a group of symbol sliders.

- Enter space-separated tokens in `Sequence`.
- Each token generates one symbol slider.
- Time placement reuses `Frame duration` and `Frame gap`.
- Canvas placement uses a configurable grid:
  - `Columns`
  - `Spacing X`
  - `Spacing Y`
- If `Sequence` is empty, `Slider Toolbox` keeps the normal single-frame / animation behavior.
- If `Sequence` is not empty:
  - generated count is the number of tokens
  - `Frames` is ignored
  - end-frame parameter values are ignored
  - each symbol uses start-side parameters plus its grid offset

### UI Entry

- `Generate > Slider Toolbox > SYMBOLS > Sequence`
- Layout controls are shown with the symbol controls:
  - `Columns`
  - `Spacing X`
  - `Spacing Y`

### Config Entries

- `EditorParametricSliderSymbolText`
- `EditorParametricSliderSymbolSequenceText`
- `EditorParametricSliderSymbolSequenceColumnSpacing`
- `EditorParametricSliderSymbolSequenceRowSpacing`
- `EditorParametricSliderSymbolSequenceColumns`

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/ParametricSliderAnimationPopover.cs`
  - Parses sequence tokens.
  - Generates one slider per token.
  - Applies grid offsets for canvas layout.
  - Saves symbol sequence and layout settings.
- `osu.Game/Configuration/OsuConfigManager.cs`
  - Adds defaults for symbol text and sequence layout settings.

## Symbol Reference Documentation

Added documentation for symbol input tokens.

- Text reference:
  - `.docs/slider-toolbox-symbol-reference.md`
- Visual reference:
  - `.docs/slider_symbols/README.md`
- Preview images:
  - `.docs/slider_symbols/images/`

The visual reference renders each token next to its actual glyph preview using the embedded `NotoSansMath-Regular.ttf` font.

## UI Compactness Updates

The `Slider Toolbox` panel was adjusted to handle the expanded shape and symbol set.

- Wider panel layout.
- Category tabs are placed above the two-column content area.
- Left side shows shape/category choices.
- Right side contains parameter controls.
- Shape grid uses more compact multi-column rows.
- Symbol controls are grouped with sequence and layout controls.

### Code Entry

- `osu.Game.Rulesets.Osu/Edit/ParametricSliderAnimationPopover.cs`

## Verification Status

Per current development convention, this version documentation round did not run build or runtime tests.

Completed checks:

- Generated and opened a sample symbol preview image.
- Ran `git diff --check` for the new symbol documentation files.
