// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Curves
{
    public partial class CurveToolsPopover : EditorPopover
    {
        [Resolved] private EditorBeatmap beatmap { get; set; } = null!;
        [Resolved] private IEditorChangeHandler changes { get; set; } = null!;
        private Bindable<bool> anchorMode = null!;
        private Bindable<bool> keepDuration = null!;
        private Bindable<bool> curvaturePen = null!;
        private Bindable<bool> classicSymmetry = null!;
        private Bindable<EditorClassicSymmetryAxis> classicSymmetryAxis = null!;
        private readonly Bindable<CurveOperation> operation = new Bindable<CurveOperation>(CurveOperation.Union);
        private readonly BindableDouble offset = new BindableDouble(8) { MinValue = -128, MaxValue = 128, Precision = 1 };
        private readonly BindableBool replace = new BindableBool(false);
        private OsuTextFlowContainer status = null!;
        private Container preview = null!;
        private List<List<Vector2>>? pending;
        private Slider[] sources = Array.Empty<Slider>();
        private (int version, Vector2 position, double time, double duration)[] sourceVersions = Array.Empty<(int, Vector2, double, double)>();

        public CurveToolsPopover() : base(false) { }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            anchorMode = config.GetBindable<bool>(OsuSetting.EditorAnchorMode);
            keepDuration = config.GetBindable<bool>(OsuSetting.EditorAnchorKeepDuration);
            curvaturePen = config.GetBindable<bool>(OsuSetting.EditorAnchorCurvaturePen);
            classicSymmetry = config.GetBindable<bool>(OsuSetting.EditorClassicSymmetry);
            classicSymmetryAxis = config.GetBindable<EditorClassicSymmetryAxis>(OsuSetting.EditorClassicSymmetryAxis);

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };
            var flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(8),
                Padding = new MarginPadding(8)
            };
            Child = new OsuScrollContainer { Width = 360, Height = 560, Child = flow };

            flow.Add(new EditorToolPages(
                ("Editing settings", new Drawable[] { workflowGroup() }),
                ("Contour operations", new Drawable[] { contourGroup() }),
                ("Shortcuts & gestures", new Drawable[] { shortcutGroup() })));
            offset.BindValueChanged(_ => clear());
            operation.BindValueChanged(_ => clear());
        }

        private EditorToolboxGroup workflowGroup()
            => new EditorToolboxGroup("Curve workflow")
            {
                Spacing = new Vector2(0, 6),
                Children = new Drawable[]
                {
                    new FormCheckBox
                    {
                        Caption = "Unified curve controls",
                        Current = anchorMode,
                        HintText = "Use anchor and handle editing for pure Bézier sliders. Other slider types remain in classic control-point mode."
                    },
                    new FormCheckBox
                    {
                        Caption = "Keep duration when editing",
                        Current = keepDuration,
                        HintText = "Preserve slider duration while editing by compensating slider velocity."
                    },
                    new FormCheckBox
                    {
                        Caption = "Curvature pen",
                        Current = curvaturePen,
                        HintText = "Automatically smooths clicked points while placing a slider. Turn off for straight click-to-click segments; drag a point to create handles."
                    },
                    new FormCheckBox
                    {
                        Caption = "Classic live symmetry",
                        Current = classicSymmetry,
                        HintText = "When a single classic slider is selected, show a non-destructive mirrored preview that follows edits."
                    },
                    new FormEnumDropdown<EditorClassicSymmetryAxis>
                    {
                        Caption = "Classic symmetry axis",
                        Current = classicSymmetryAxis
                    },
                    description("Round/bevel trim fraction is kept in the top bar: UTAU_Editor → Workflow.")
                }
            };

        private EditorToolboxGroup shortcutGroup()
            => new EditorToolboxGroup("Shortcuts & gestures")
            {
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    description("Alt+P: switch classic / anchor editing. C: corner/smooth; Shift+C/S: force corner/smooth; S: symmetric; A: auto smooth; H: retract handles; R/B: round/bevel; I: midpoint; O: open/close; V: reverse."),
                    description("Alt+H/V: flip; Alt+X/Y: align; Alt+Shift+X/Y: distribute; Alt+Q: simplify linear; Alt+arrows nudge 1 px (Shift: 10 px). Shift+right-click a handle removes only that handle."),
                    description("Pen: click for straight segments; drag while placing a point for handles; Ctrl-drag moves the last anchor; Enter/right-click finishes; Backspace removes the last point. Double-click inserts; Ctrl-click selects; Shift-drag constrains; Alt-drag breaks handles."),
                    description("Right-click an anchor for node, geometry and arrange tools. Holding Alt while moving the whole slider enables distance snap.")
                }
            };

        private EditorToolboxGroup contourGroup()
            => new EditorToolboxGroup("Contour operations")
            {
                Spacing = new Vector2(0, 6),
                Children = new Drawable[]
                {
                    description("Create sampled outlines from selected pure Bézier sliders. Preview is non-destructive until Apply."),
                    new FormSliderBar<double> { Caption = "Offset / stroke half-width (px)", Current = offset, TabbableContentContainer = this },
                    new FormEnumDropdown<CurveOperation> { Caption = "Contour operation", Current = operation },
                    button("Preview operation", () => calculate(operation.Value)),
                    new FormCheckBox { Caption = "Replace source sliders on Apply", Current = replace,
                        HintText = "Off keeps originals. Output contours become separate linear sliders, ordered from the earliest source time." },
                    preview = new Container { RelativeSizeAxes = Axes.X, Height = 150, Masking = true },
                    status = new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 12))
                    {
                        RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y,
                        Text = "Select pure Bézier sliders. Boolean operations use sampled closed contours; preview never changes the source."
                    },
                    button("Apply preview", commit),
                    button("Cancel preview", clear)
                }
            };

        private static RoundedButton button(string text, Action action)
            => new RoundedButton { Text = text, RelativeSizeAxes = Axes.X, Height = 28, Action = action };

        private static OsuTextFlowContainer description(string text)
            => new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 13))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Text = text
            };

        private Slider[] selection() => beatmap.SelectedHitObjects.OfType<Slider>()
                                      .Where(s => AnchorPath.TryRead(s.Path, out _))
                                      .OrderBy(s => s.StartTime).ToArray();

        private void calculate(CurveOperation operation)
        {
            clear();
            sources = selection();
            try
            {
                pending = CurveGeometryOperations.Execute(sources, operation, offset.Value);
                sourceVersions = sources.Select(s => (s.Path.Version.Value, s.Position, s.StartTime, s.Duration)).ToArray();
                if (pending.Count == 0) { status.Text = "Empty result. Source sliders are unchanged."; return; }
                var all = pending.SelectMany(p => p).ToArray();
                float minX = all.Min(p => p.X), minY = all.Min(p => p.Y);
                float scale = Math.Min(Math.Max(1, preview.DrawWidth - 8) / Math.Max(1, all.Max(p => p.X) - minX),
                    Math.Max(1, preview.DrawHeight - 8) / Math.Max(1, all.Max(p => p.Y) - minY));
                foreach (var contour in pending)
                {
                    var path = new SmoothPath { PathRadius = 1, Colour = Colour4.Cyan };
                    foreach (var p in contour) path.AddVertex((p - new Vector2(minX, minY)) * scale + new Vector2(4));
                    path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
                    preview.Add(path);
                }
                status.Text = $"{operation}: {pending.Count} contour(s).\nSampled geometry; holes become separate slider outlines.";
            }
            catch (ArgumentException e) { pending = null; status.Text = e.Message; }
        }

        private void commit()
        {
            if (pending == null || pending.Count == 0) return;
            if (sources.Where((s, i) => !beatmap.HitObjects.Contains(s)
                || (s.Path.Version.Value, s.Position, s.StartTime, s.Duration) != sourceVersions[i]).Any())
            {
                clear(); status.Text = "Sources changed. Calculate a fresh preview."; return;
            }
            var output = new List<Slider>();
            double time = sources[0].StartTime;
            foreach (var contour in pending)
            {
                var origin = contour[0];
                var contourAnchors = contour.Take(contour.Count - 1)
                                             .Select(p => new CurveAnchor(p - origin)).ToList();
                var contourPath = new AnchorPath { Closed = true };
                contourPath.Anchors.AddRange(contourAnchors);
                var path = new SliderPath(contourPath.ToControlPoints().ToArray());
                path.ExpectedDistance.Value = path.CalculatedDistance;
                var slider = new Slider
                {
                    Position = origin,
                    StartTime = time,
                    Path = path,
                    NewCombo = output.Count == 0,
                    SliderVelocityMultiplier = sources[0].SliderVelocityMultiplier
                };
                foreach (var sample in sources[0].Samples)
                    slider.Samples.Add(sample.With());
                slider.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
                output.Add(slider);
                double step = beatmap.ControlPointInfo.TimingPointAt(time).BeatLength / beatmap.BeatDivisor;
                time += Math.Ceiling(Math.Max(slider.Duration, step) / step) * step;
            }
            changes.BeginChange();
            try
            {
                if (replace.Value) beatmap.RemoveRange(sources);
                beatmap.AddRange(output);
                beatmap.SelectedHitObjects.Clear();
                beatmap.SelectedHitObjects.AddRange(output);
            }
            finally { changes.EndChange(); }
            clear(); status.Text = $"Created {output.Count} ordinary slider(s). Undo available.";
        }

        private void clear()
        {
            pending = null;
            preview.Clear();
            status.Text = "Preview cleared. No beatmap changes.";
        }
    }
}
