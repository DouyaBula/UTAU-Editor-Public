// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class GlobalOffsetPopover : EditorPopover
    {
        private readonly Bindable<string> offsetText = new Bindable<string>("0");
        private OsuTextFlowContainer summary = null!;
        private RoundedButton applyButton = null!;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private Bindable<ControlPointGroup>? selectedGroup { get; set; }

        [Resolved(canBeNull: true)]
        private EditorClock? editorClock { get; set; }

        public GlobalOffsetPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Width = 370,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(12),
                Spacing = new Vector2(8),
                Children = new Drawable[]
                {
                    new OsuSpriteText { Text = "Global offset", Font = OsuFont.Default.With(size: 20, weight: FontWeight.Bold) },
                    new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 14))
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Text = "Move ALL timing, kiai, objects, breaks, bookmarks and the preview point in this difficulty together. BPM, slider shapes and durations stay unchanged. Other difficulties, audio, video and storyboard are not modified.",
                    },
                    new FormTextBox
                    {
                        Caption = "Shift by (ms)",
                        HintText = "Positive = later; negative = earlier. For example, +250 for audio with 250 ms more silence at the start. Whole milliseconds; no beat snapping.",
                        Current = offsetText,
                        SelectAllOnFocus = true,
                        TabbableContentContainer = this,
                    },
                    summary = new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 14))
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                    applyButton = new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 32,
                        Text = "Apply to entire difficulty",
                        Action = apply,
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            offsetText.BindValueChanged(_ => updateSummary(), true);
        }

        private bool tryGetOffset(out int offset) => int.TryParse(offsetText.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out offset);

        private void updateSummary()
        {
            if (!tryGetOffset(out int offset))
            {
                applyButton.Enabled.Value = false;
                summary.Text = "Enter a signed whole number of milliseconds, for example +250 or -120.";
                return;
            }

            string? error = GlobalOffsetAdjustment.Validate(beatmap, offset);
            applyButton.Enabled.Value = offset != 0 && error == null;
            if (error != null)
            {
                summary.Text = error;
                return;
            }

            var first = beatmap.ControlPointInfo.TimingPoints.FirstOrDefault();
            string firstTiming = first == null ? "No timing points." : $"First timing: {first.Time:0.###} -> {first.Time + offset:0.###} ms.";
            summary.Text = $"{beatmap.ControlPointInfo.TimingPoints.Count} timing points, {beatmap.HitObjects.Count} objects. {firstTiming}\nApply once, then use Ctrl+Z to undo the whole shift.";
        }

        private void apply()
        {
            if (!tryGetOffset(out int offset))
                return;

            double? selectedTime = selectedGroup?.Value?.Time;
            if (!GlobalOffsetAdjustment.Apply(beatmap, offset, out string? error))
            {
                updateSummary();
                if (error != null)
                    summary.Text = error;
                return;
            }

            if (selectedGroup != null && selectedTime.HasValue)
                selectedGroup.Value = beatmap.ControlPointInfo.GroupAt(selectedTime.Value + offset);

            editorClock?.Seek(editorClock.CurrentTime + offset);
            offsetText.Value = "0";
            summary.Text = $"Shifted the entire difficulty by {offset:+0;-0;0} ms. Ctrl+Z undoes this operation.";
        }
    }
}
