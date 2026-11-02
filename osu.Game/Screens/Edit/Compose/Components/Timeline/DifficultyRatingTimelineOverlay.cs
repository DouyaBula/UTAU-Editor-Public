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
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class DifficultyRatingTimelineOverlay : osu.Game.Screens.Edit.Components.Timelines.Summary.Parts.TimelinePart<Drawable>
    {
        private readonly IBindable<IReadOnlyList<DifficultyRatingSample>> samples;
        private readonly SmoothPath curve = new SmoothPath { PathRadius = 1.5f };
        private Bindable<bool> visible = null!;
        private Bindable<float> opacity = null!;
        private Bindable<float> height = null!;
        private OsuColour colours = null!;
        private bool initialized;
        private float lastCurveWidth = -1;
        private float lastCurveHeight = -1;
        private float lastTimelineDuration = -1;

        internal int RenderedSampleCount { get; private set; }

        public DifficultyRatingTimelineOverlay(IBindable<IReadOnlyList<DifficultyRatingSample>> samples)
        {
            this.samples = samples;
            RelativeSizeAxes = Axes.Both;
            curve.AutoSizeAxes = Axes.None;
            curve.RelativeSizeAxes = Axes.Both;
            curve.Anchor = Anchor.TopLeft;
            curve.Origin = Anchor.TopLeft;
            // Draw in viewport coordinates instead of inside Content's virtual millisecond
            // coordinate space. Otherwise PathRadius is also scaled by the full track length
            // and the rendered line becomes effectively sub-pixel thin.
            AddInternal(curve);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuColour colours)
        {
            this.colours = colours;
            curve.Colour = colours.BlueLight;
            visible = config.GetBindable<bool>(OsuSetting.EditorDifficultyTimelineVisible);
            opacity = config.GetBindable<float>(OsuSetting.EditorDifficultyTimelineOpacity);
            height = config.GetBindable<float>(OsuSetting.EditorDifficultyTimelineHeight);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            samples.ValueChanged += onSamplesChanged;
            visible.BindValueChanged(_ => rebuild(), true);
            opacity.BindValueChanged(_ => rebuild());
            height.BindValueChanged(_ => rebuild());
            initialized = true;
            rebuild();
        }

        private void onSamplesChanged(ValueChangedEvent<IReadOnlyList<DifficultyRatingSample>> _) => rebuild();

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
                samples.ValueChanged -= onSamplesChanged;
            base.Dispose(isDisposing);
        }

        protected override void Update()
        {
            base.Update();

            float curveWidth = curve.DrawWidth;
            float curveHeight = curve.DrawHeight;
            float timelineDuration = Content.RelativeChildSize.X;

            // This path is a child of the virtual timeline content, whose size can change
            // independently of this overlay (for example when the track length becomes known).
            // Watching the overlay's own DrawSize misses that case and can leave the path empty.
            if (curveWidth != lastCurveWidth || curveHeight != lastCurveHeight || timelineDuration != lastTimelineDuration)
            {
                lastCurveWidth = curveWidth;
                lastCurveHeight = curveHeight;
                lastTimelineDuration = timelineDuration;
                rebuild();
            }
        }

        private void rebuild()
        {
            curve.ClearVertices();
            RenderedSampleCount = 0;
            curve.Alpha = opacity?.Value ?? 0;

            if (!initialized || !visible.Value || samples.Value.Count < 2 || curve.DrawWidth <= 0 || curve.DrawHeight <= 0)
                return;

            double timelineDuration = Math.Max(Content.RelativeChildSize.X, samples.Value[^1].Time);
            if (timelineDuration <= 0)
                return;

            double minimum = samples.Value.Min(sample => sample.RelativeDifficulty);
            double maximum = samples.Value.Max(sample => sample.RelativeDifficulty);
            double valueRange = maximum - minimum;
            double graphHeight = Math.Clamp(height.Value, 0, 1);
            float pathRadius = curve.PathRadius;
            float usableWidth = Math.Max(0, curve.DrawWidth - 2 * pathRadius);
            float usableHeight = Math.Max(0, curve.DrawHeight - 2 * pathRadius);

            foreach (var sample in samples.Value)
            {
                double normalizedDifficulty = valueRange > 1e-8
                    ? Math.Clamp((sample.RelativeDifficulty - minimum) / valueRange, 0, 1)
                    : 0.5;

                float x = pathRadius + (float)(Math.Clamp(sample.Time / timelineDuration, 0, 1) * usableWidth);
                float y = pathRadius + (float)((1 - normalizedDifficulty * graphHeight) * usableHeight);
                curve.AddVertex(new Vector2(x, y));
                RenderedSampleCount++;
            }
        }
    }
}
