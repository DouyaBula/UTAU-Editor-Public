// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal enum SliderVelocityCurve
    {
        Linear,

        [Description("Accelerate")]
        Accelerate,

        [Description("Decelerate")]
        Decelerate,

        [Description("Accelerate then decelerate")]
        AccelerateThenDecelerate,

        [Description("Decelerate then accelerate")]
        DecelerateThenAccelerate,

        [Description("Speed wave")]
        Wave,

        Bounce,
        Elastic,
        Back
    }

    internal static class SliderVelocityTransformer
    {
        private const int max_control_points = 12000;

        public static SliderVelocityTransformation? Create(
            SliderPath sourcePath,
            double sourceDistance,
            double requestedVelocityMultiplier,
            SliderVelocityCurve curve,
            double intensity,
            double power,
            int frequency,
            double overshoot,
            int sampleCount,
            double maxDetourLength)
        {
            if (!Precision.DefinitelyBigger(sourceDistance, 0))
                return null;

            requestedVelocityMultiplier = Math.Max(1, requestedVelocityMultiplier);
            intensity = Math.Clamp(intensity, 0, 1);
            power = Math.Max(1, power);
            frequency = Math.Max(1, frequency);
            sampleCount = Math.Max(2, sampleCount);
            maxDetourLength = Math.Max(0.25, maxDetourLength);

            double intervalLength = sourceDistance * requestedVelocityMultiplier / sampleCount;
            double appliedIntensity = findFittingIntensity(sourcePath, curve, intensity, power, frequency, overshoot, sampleCount, intervalLength);
            Sample[] samples = createSamples(sourcePath, curve, appliedIntensity, power, frequency, overshoot, sampleCount);

            var controlPoints = new List<PathControlPoint>
            {
                new PathControlPoint(samples[0].Position, PathType.LINEAR)
            };

            for (int i = 0; i < sampleCount; i++)
            {
                Vector2 start = samples[i].Position;
                Vector2 end = samples[i + 1].Position;
                double directDistance = Vector2.Distance(start, end);
                double extraDistance = Math.Max(0, intervalLength - directDistance);
                Vector2 direction = findDetourDirection(sourcePath, samples[i].PathProgress, start, end);

                // Short backtracking branches consume time while remaining hidden inside the slider body.
                while (Precision.DefinitelyBigger(extraDistance, 0.001))
                {
                    double branchLength = Math.Min(maxDetourLength, extraDistance / 2);

                    if (branchLength < 0.0005)
                        break;

                    controlPoints.Add(new PathControlPoint(start + direction * (float)branchLength));
                    controlPoints.Add(new PathControlPoint(start));
                    extraDistance -= branchLength * 2;

                    if (controlPoints.Count > max_control_points)
                        return null;
                }

                controlPoints.Add(new PathControlPoint(end));
            }

            var transformedPath = new SliderPath(controlPoints.ToArray());
            double actualDistance = transformedPath.CalculatedDistance;

            if (!Precision.DefinitelyBigger(actualDistance, 0))
                return null;

            return new SliderVelocityTransformation(
                transformedPath,
                actualDistance / sourceDistance,
                appliedIntensity,
                controlPoints.Count);
        }

        private static double findFittingIntensity(
            SliderPath sourcePath,
            SliderVelocityCurve curve,
            double requestedIntensity,
            double power,
            int frequency,
            double overshoot,
            int sampleCount,
            double intervalLength)
        {
            if (intervalsFit(createSamples(sourcePath, curve, requestedIntensity, power, frequency, overshoot, sampleCount), intervalLength))
                return requestedIntensity;

            double low = 0;
            double high = requestedIntensity;

            for (int i = 0; i < 20; i++)
            {
                double middle = (low + high) / 2;

                if (intervalsFit(createSamples(sourcePath, curve, middle, power, frequency, overshoot, sampleCount), intervalLength))
                    low = middle;
                else
                    high = middle;
            }

            return low;
        }

        private static bool intervalsFit(IReadOnlyList<Sample> samples, double intervalLength)
        {
            for (int i = 0; i < samples.Count - 1; i++)
            {
                if (Precision.DefinitelyBigger(Vector2.Distance(samples[i].Position, samples[i + 1].Position), intervalLength, 0.001))
                    return false;
            }

            return true;
        }

        private static Sample[] createSamples(
            SliderPath sourcePath,
            SliderVelocityCurve curve,
            double intensity,
            double power,
            int frequency,
            double overshoot,
            int sampleCount)
        {
            var samples = new Sample[sampleCount + 1];
            Vector2 origin = sourcePath.PositionAt(0);

            for (int i = 0; i <= sampleCount; i++)
            {
                double timeProgress = i / (double)sampleCount;
                double pathProgress = applyCurve(curve, timeProgress, intensity, power, frequency, overshoot);
                samples[i] = new Sample(sourcePath.PositionAt(pathProgress) - origin, pathProgress);
            }

            return samples;
        }

        private static double applyCurve(SliderVelocityCurve curve, double progress, double intensity, double power, int frequency, double overshoot)
        {
            double curved = curve switch
            {
                SliderVelocityCurve.Accelerate => Math.Pow(progress, power),
                SliderVelocityCurve.Decelerate => 1 - Math.Pow(1 - progress, power),
                SliderVelocityCurve.AccelerateThenDecelerate => powerInOut(progress, power),
                SliderVelocityCurve.DecelerateThenAccelerate => powerOutIn(progress, power),
                SliderVelocityCurve.Wave => wave(progress, frequency, overshoot),
                SliderVelocityCurve.Bounce => bounce(progress, frequency, power),
                SliderVelocityCurve.Elastic => elastic(progress, frequency, overshoot),
                SliderVelocityCurve.Back => backInOut(progress, overshoot),
                _ => progress
            };

            return Math.Clamp(progress + (curved - progress) * intensity, 0, 1);
        }

        private static double powerInOut(double progress, double power)
        {
            if (progress < 0.5)
                return Math.Pow(progress * 2, power) / 2;

            return 1 - Math.Pow((1 - progress) * 2, power) / 2;
        }

        private static double powerOutIn(double progress, double power)
        {
            if (progress < 0.5)
                return (1 - Math.Pow(1 - progress * 2, power)) / 2;

            return (1 + Math.Pow(progress * 2 - 1, power)) / 2;
        }

        private static double wave(double progress, int frequency, double amplitude)
            => progress + Math.Clamp(amplitude, 0, 1) * Math.Sin(progress * Math.PI * 2 * frequency) / (Math.PI * 2 * frequency);

        private static double bounce(double progress, int frequency, double power)
        {
            double decay = Math.Pow(1 - progress, Math.Max(1, power / 2));
            return 1 - Math.Abs(Math.Cos(progress * Math.PI * (frequency + 0.5))) * decay;
        }

        private static double elastic(double progress, int frequency, double overshoot)
        {
            if (progress <= 0 || progress >= 1)
                return progress;

            return progress + Math.Sin(progress * Math.PI * 2 * frequency)
                   * Math.Sin(progress * Math.PI)
                   * Math.Clamp(overshoot, 0, 1);
        }

        private static double backInOut(double progress, double overshoot)
        {
            double strength = 1.70158 * (1 + Math.Clamp(overshoot, 0, 1) * 2);
            double doubled = progress * 2;

            if (doubled < 1)
                return doubled * doubled * ((strength + 1) * doubled - strength) / 2;

            doubled -= 2;
            return (doubled * doubled * ((strength + 1) * doubled + strength) + 2) / 2;
        }

        private static Vector2 findDetourDirection(SliderPath sourcePath, double progress, Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;

            if (direction.LengthSquared < 0.0001f)
            {
                const double epsilon = 0.001;
                direction = sourcePath.PositionAt(Math.Min(1, progress + epsilon))
                            - sourcePath.PositionAt(Math.Max(0, progress - epsilon));
            }

            if (direction.LengthSquared < 0.0001f)
                return Vector2.UnitX;

            return direction.Normalized();
        }

        private readonly struct Sample
        {
            public readonly Vector2 Position;
            public readonly double PathProgress;

            public Sample(Vector2 position, double pathProgress)
            {
                Position = position;
                PathProgress = pathProgress;
            }
        }
    }

    internal sealed class SliderVelocityTransformation
    {
        public SliderPath Path { get; }
        public double VelocityMultiplier { get; }
        public double AppliedIntensity { get; }
        public int ControlPointCount { get; }

        public SliderVelocityTransformation(SliderPath path, double velocityMultiplier, double appliedIntensity, int controlPointCount)
        {
            Path = path;
            VelocityMultiplier = velocityMultiplier;
            AppliedIntensity = appliedIntensity;
            ControlPointCount = controlPointCount;
        }
    }
}
