// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using osu.Game.Database;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.DancePose
{
    /// <summary>
    /// Imports externally recognised poses and previews one standard osu! slider per sampled pose.
    /// The popover follows the live-preview / commit interaction used by the other slider toolboxes.
    /// </summary>
    public partial class DancePoseSliderPopover : EditorPreviewPopover, ICanAcceptFiles
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        [Resolved]
        private HitObjectComposer composer { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private IEditorChangeHandler? changeHandler { get; set; }

        private readonly Bindable<FileInfo?> poseFile = new Bindable<FileInfo?>();
        private Container filePicker = null!;
        private readonly Bindable<DancePosePlacementMode> placementMode = new Bindable<DancePosePlacementMode>(DancePosePlacementMode.PreserveMotion);
        [Resolved] private OsuGameBase game { get; set; } = null!;
        private readonly BindableDouble rangeStart = new BindableDouble(0) { MinValue = 0, MaxValue = 86400000 };
        private readonly BindableDouble rangeEnd = new BindableDouble(4000) { MinValue = 0, MaxValue = 86400000 };
        private readonly BindableInt density = new BindableInt(25) { MinValue = 1, MaxValue = 100 };
        private readonly BindableInt gapSnaps = new BindableInt(1) { MinValue = 0, MaxValue = 128 };
        private static readonly ConditionalWeakTable<EditorBeatmap, LockedRange> lockedRanges = new ConditionalWeakTable<EditorBeatmap, LockedRange>();
        private sealed class LockedRange
        {
            public readonly BindableDouble Start = new BindableDouble(0) { MinValue = 0, MaxValue = 86400000 };
            public readonly BindableDouble End = new BindableDouble(4000) { MinValue = 0, MaxValue = 86400000 };
            public readonly BindableInt Gap = new BindableInt(1) { MinValue = 0, MaxValue = 128 };
        }
        private EditorToolStatus sourceSummary = null!;
        private readonly BindableDouble figureScale = new BindableDouble(0.55) { MinValue = 0.1, MaxValue = 2, Precision = 0.05 };
        private readonly BindableDouble offsetX = new BindableDouble(0) { MinValue = -192, MaxValue = 192, Precision = 4 };
        private readonly BindableDouble offsetY = new BindableDouble(0) { MinValue = -144, MaxValue = 144, Precision = 4 };

        private readonly List<Slider> previewSliders = new List<Slider>();
        private EditorToolStatus statusText = null!;
        private RoundedButton generateButton = null!;
        private DancePoseSequence? sequence;
        private double sequenceStartTime;

        public DancePoseSliderPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            var content = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(6),
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = "Slider Toolbox 3",
                        Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                        RelativeSizeAxes = Axes.X
                    },
                    new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 12))
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Text = "Import a pose sequence from the external recogniser. Each sampled pose becomes one native linear slider."
                    },
                    new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 34,
                        Text = "Drop pose JSON here / Browse…",
                        Action = chooseFile
                    },
                    filePicker = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 0,
                        Masking = true,
                        Child = new OsuFileSelector(null, new[] { ".json" })
                        {
                            RelativeSizeAxes = Axes.Both,
                            CurrentFile = { BindTarget = poseFile }
                        }
                    },
                    sourceSummary = new EditorToolStatus { Text = "Source JSON: no frames loaded" },
                    createTwoColumnRow(
                        new FormSliderBar<double>
                        {
                            Width = 0.49f,
                            Caption = "Timeline start (ms)",
                            Current = rangeStart,
                            KeyboardStep = 1,
                            TabbableContentContainer = this
                        },
                        new FormSliderBar<double>
                        {
                            Width = 0.49f,
                            Caption = "Timeline end (ms)",
                            Current = rangeEnd,
                            KeyboardStep = 1,
                            TabbableContentContainer = this
                        }),
                    createTwoColumnRow(
                        new RoundedButton { Width = 0.49f, RelativeSizeAxes = Axes.X, Height = 30, Text = "Lock start at cursor", Action = () => rangeStart.Value = beatSnapProvider.SnapTime(editorClock.CurrentTime) },
                        new RoundedButton { Width = 0.49f, RelativeSizeAxes = Axes.X, Height = 30, Text = "Lock end at cursor", Action = () => rangeEnd.Value = beatSnapProvider.SnapTime(editorClock.CurrentTime) }),
                    new FormSliderBar<int> { Caption = "Sample density (% of source frames)", Current = density, KeyboardStep = 1, TabbableContentContainer = this },
                    new FormSliderBar<int> { Caption = "Gap between sliders (snaps)", Current = gapSnaps, KeyboardStep = 1, TabbableContentContainer = this },
                    new FormEnumDropdown<DancePosePlacementMode>
                    {
                        Caption = "Positioning",
                        HintText = "Preserve the detected movement across the frame, or stabilise the body centre while keeping limb motion.",
                        Current = placementMode
                    },
                    createTwoColumnRow(
                        new FormSliderBar<double>
                        {
                            Width = 0.49f,
                            Caption = "Figure scale",
                            Current = figureScale,
                            KeyboardStep = 0.05f,
                            LabelFormat = value => $"{value:0.00}x",
                            TooltipFormat = value => $"{value:0.00}x",
                            TabbableContentContainer = this
                        },
                        new FormSliderBar<double>
                        {
                            Width = 0.49f,
                            Caption = "Offset X",
                            Current = offsetX,
                            KeyboardStep = 4,
                            LabelFormat = value => $"{value:0} px",
                            TooltipFormat = value => $"{value:0} px",
                            TabbableContentContainer = this
                        }),
                    new FormSliderBar<double>
                    {
                        Caption = "Offset Y",
                        Current = offsetY,
                        KeyboardStep = 4,
                        LabelFormat = value => $"{value:0} px",
                        TooltipFormat = value => $"{value:0} px",
                        TabbableContentContainer = this
                    },
                    statusText = new EditorToolStatus { Text = "Choose a pose sequence to preview the generated slider frames." },
                    new EditorToolActionRow(generateButton = new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 34,
                        Text = "Generate sliders",
                        Enabled = { Value = false },
                        Action = commit
                    }, Hide)
                }
            };

            Child = new OsuScrollContainer
            {
                Width = 420,
                Height = 460,
                Child = content
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sequenceStartTime = beatSnapProvider.SnapTime(editorClock.CurrentTime);
            LockedRange locked = lockedRanges.GetValue(editorBeatmap, _ => new LockedRange());
            rangeStart.BindTo(locked.Start);
            rangeEnd.BindTo(locked.End);
            gapSnaps.BindTo(locked.Gap);
            game.RegisterImportHandler(this);
            rangeStart.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));
            rangeEnd.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));
            density.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));
            gapSnaps.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));

            poseFile.BindValueChanged(_ => Scheduler.AddOnce(loadSequenceAndPreview));
            placementMode.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));
            figureScale.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));
            offsetX.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));
            offsetY.BindValueChanged(_ => Scheduler.AddOnce(applyPreview));
        }

        private static FillFlowContainer createTwoColumnRow(Drawable first, Drawable second) => new EditorToolControlRow
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Full,
            Spacing = new Vector2(6, 0),
            Children = new[] { first, second }
        };

        private void loadSequenceAndPreview()
        {
            if (PreviewClosed)
                return;

            filePicker.Height = 0;
            sequence = null;

            if (poseFile.Value == null)
            {
                applyPreview();
                return;
            }

            try
            {
                using (FileStream stream = poseFile.Value.OpenRead())
                    sequence = DancePoseSequence.Read(stream);
                sourceSummary.Text = $"{Path.GetFileName(poseFile.Value.Name)} | {sequence.Frames.Count} source frames";

                statusText.Text = $"Loaded {sequence.Frames.Count} recognition frames from {Path.GetFileName(poseFile.Value.Name)}.";
            }
            catch (Exception e)
            {
                statusText.Text = $"Could not read pose sequence: {e.Message}";
            }

            applyPreview();
        }

        private void chooseFile()
        {
            filePicker.Height = filePicker.Height == 0 ? 260 : 0;
        }

        private void applyPreview()
        {
            if (PreviewClosed)
                return;

            if (sequence == null)
            {
                removePreview();
                generateButton.Enabled.Value = false;
                if (poseFile.Value == null)
                    statusText.Text = "Choose a pose sequence to preview the generated slider frames.";
                return;
            }

            try
            {
                double start = beatSnapProvider.SnapTime(rangeStart.Value);
                double end = beatSnapProvider.SnapTime(rangeEnd.Value);
                if (end <= start)
                    throw new ArgumentException("Timeline end must be after timeline start.");
                int requested = Math.Min(512, Math.Max(1, (int)Math.Round(sequence.Frames.Count * density.Value / 100.0)));
                var boundaries = Enumerable.Range(0, requested + 1)
                    .Select(i => i == requested ? end : beatSnapProvider.SnapTime(start + (end - start) * i / requested))
                    .Where(time => time >= start && time <= end).Distinct().OrderBy(time => time).ToArray();
                DancePoseGenerationResult result = DancePoseSliderGenerator.Generate(
                    sequence,
                    sequenceStartTime,
                    1,
                    1,
                    editorBeatmap.Difficulty.SliderMultiplier,
                    time => editorBeatmap.ControlPointInfo.TimingPointAt(time).BeatLength,
                    placementMode.Value,
                    figureScale.Value,
                    offsetX.Value,
                    offsetY.Value,
                    boundaries,
                    time => gapSnaps.Value * editorBeatmap.GetBeatLengthAtTime(time));

                // Opening the popover is not an edit. Begin an editor transaction only once we
                // are about to insert the first live preview; this also avoids the initial hidden
                // PopOut emitted by VisibilityContainer during async loading from ending a change
                // that has not actually started yet.
                if (changeHandler == null)
                    throw new InvalidOperationException("The editor is not ready to preview generated sliders yet.");

                if (!BeginPreview())
                    return;

                TernaryState previousNewComboState = composer.SelectionNewComboState!.Value;
                if (result.Sliders.Count > 0)
                    result.Sliders[0].NewCombo = previousNewComboState == TernaryState.True;
                ReplacePreviewObjects(previewSliders, result.Sliders);
                composer.SelectionNewComboState!.Value = previousNewComboState;

                statusText.Text = $"{sequence.Frames.Count} source → {result.Sliders.Count} sliders | {result.Duration:0} ms | SV {result.MinimumVelocity:0.##}x–{result.MaximumVelocity:0.##}x";
                generateButton.Enabled.Value = changeHandler != null;
            }
            catch (Exception e)
            {
                removePreview();
                statusText.Text = e.Message;
                generateButton.Enabled.Value = false;
            }
        }

        private void removePreview()
        {
            if (previewSliders.Count == 0)
                return;

            ReplacePreviewObjects(previewSliders, Array.Empty<Slider>());
        }

        private void commit()
        {
            applyPreview();

            if (!generateButton.Enabled.Value || changeHandler == null)
                return;

            TernaryState previousNewComboState = composer.SelectionNewComboState!.Value;
            editorBeatmap.SelectedHitObjects.Clear();
            editorBeatmap.SelectedHitObjects.AddRange(previewSliders);
            composer.SelectionNewComboState!.Value = previousNewComboState;

            CommitPreview();
        }

        protected override void PopOut()
        {
            base.PopOut();
            if (PreviewClosed)
                game?.UnregisterImportHandler(this);
        }

        protected override void RestorePreview() => removePreview();

        public IEnumerable<string> HandledExtensions => new[] { ".json" };
        public Task Import(params string[] paths)
        {
            if (paths.Length > 0)
                Schedule(() =>
                {
                    if (!PreviewClosed && State.Value == Visibility.Visible)
                        poseFile.Value = new FileInfo(paths[0]);
                });
            return Task.CompletedTask;
        }
        public Task Import(ImportTask[] tasks, ImportParameters parameters) => throw new NotSupportedException();
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && game != null)
                game.UnregisterImportHandler(this);
            base.Dispose(isDisposing);
        }
    }
}
