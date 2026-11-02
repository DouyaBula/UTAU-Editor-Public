// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal static class SliderToolboxUiUtils
    {
        public static string FormatSnapCount(double value) => value == 1 ? "1 snap" : $"{value:0} snaps";

        public static double Interpolate(FormSliderBar<double> start, FormSliderBar<double> end, double progress) =>
            lerp(start.Current.Value, end.Current.Value, progress);

        public static int InterpolateInt(FormSliderBar<int> start, FormSliderBar<int> end, double progress) =>
            (int)Math.Round(lerp(start.Current.Value, end.Current.Value, progress));

        public static double InterpolateAngle(FormSliderBar<int> startTurns, FormSliderBar<double> startDegrees, FormSliderBar<int> endTurns, FormSliderBar<double> endDegrees, double progress) =>
            lerp(startTurns.Current.Value * 360 + startDegrees.Current.Value, endTurns.Current.Value * 360 + endDegrees.Current.Value, progress);

        public static double ApplyEasing(double progress, ParametricSliderAnimationEasing animationEasing) => animationEasing switch
        {
            ParametricSliderAnimationEasing.EaseIn => progress * progress,
            ParametricSliderAnimationEasing.EaseOut => 1 - (1 - progress) * (1 - progress),
            ParametricSliderAnimationEasing.EaseInOut => progress < 0.5
                ? 2 * progress * progress
                : 1 - Math.Pow(-2 * progress + 2, 2) / 2,
            ParametricSliderAnimationEasing.SmoothStep => progress * progress * (3 - 2 * progress),
            _ => progress
        };

        public static void Swap(FormSliderBar<int> first, FormSliderBar<int> second)
        {
            int value = first.Current.Value;
            first.Current.Value = second.Current.Value;
            second.Current.Value = value;
        }

        public static void Swap(FormSliderBar<double> first, FormSliderBar<double> second)
        {
            double value = first.Current.Value;
            first.Current.Value = second.Current.Value;
            second.Current.Value = value;
        }

        public static void Copy(FormSliderBar<int> source, FormSliderBar<int> target) =>
            target.Current.Value = source.Current.Value;

        public static void Copy(FormSliderBar<double> source, FormSliderBar<double> target) =>
            target.Current.Value = source.Current.Value;

        public static SingleFrameInactiveContainer CreateSingleFrameInactiveControl(Drawable content, float width)
        {
            content.Width = 1;
            return new SingleFrameInactiveContainer(content) { Width = width };
        }

        private static double lerp(double start, double end, double progress) => start + (end - start) * progress;
    }

    internal partial class SingleFrameInactiveContainer : CompositeDrawable
    {
        private readonly Drawable content;
        private readonly InputBlockingContainer inputBlocker;
        private bool inactive;

        public bool Inactive
        {
            get => inactive;
            set
            {
                inactive = value;
                content.Alpha = value ? 0.35f : 1;
                inputBlocker.AlwaysPresent = value;
                inputBlocker.Alpha = value ? 1 : 0;
            }
        }

        public SingleFrameInactiveContainer(Drawable content)
        {
            this.content = content;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            content.RelativeSizeAxes = Axes.X;

            InternalChildren = new[]
            {
                content,
                inputBlocker = new InputBlockingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                }
            };
        }
    }
}
