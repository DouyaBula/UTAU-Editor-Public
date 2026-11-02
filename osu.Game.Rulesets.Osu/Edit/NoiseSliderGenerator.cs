// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal enum SliderToolbox2Category
    {
        [Description("Vector fields")]
        VectorFlow,

        [Description("Implicit contours")]
        ImplicitContours,

        [Description("Graph paths")]
        GraphPaths,

        Attractors,

        [Description("Stroke grammar")]
        StrokeGrammar,

        [Description("Mask tracing")]
        MaskTracing,

        [Description("Beat driven")]
        BeatDriven
    }

    internal enum SliderToolbox2Type
    {
        [Description("Noise sampled path")]
        NoiseSampledPath,

        [Description("Curl flow line")]
        CurlFlowLine,

        [Description("Spiral flow line")]
        SpiralFlowLine,

        [Description("Orbital flow line")]
        OrbitalFlowLine,

        [Description("Superellipse contour")]
        SuperellipseContour,

        [Description("Blobby contour")]
        BlobbyContour,

        [Description("Rounded polygon")]
        RoundedPolygonContour,

        [Description("Metaball contour")]
        MetaballContour,

        [Description("Boolean field contour")]
        BooleanFieldContour,

        [Description("Hilbert path")]
        HilbertPath,

        [Description("Peano path")]
        PeanoPath,

        [Description("Maze walk")]
        MazeWalk,

        [Description("Maze solution")]
        MazeSolutionPath,

        [Description("Voronoi ridge walk")]
        VoronoiRidgeWalk,

        [Description("Point network tour")]
        PointNetworkTour,

        [Description("Lorenz attractor")]
        LorenzAttractor,

        [Description("Rossler attractor")]
        RosslerAttractor,

        [Description("Halvorsen attractor")]
        HalvorsenAttractor,

        [Description("Thomas attractor")]
        ThomasAttractor,

        [Description("Clifford map")]
        CliffordMap,

        [Description("De Jong map")]
        DeJongMap,

        [Description("Ikeda map")]
        IkedaMap,

        [Description("Gumowski-Mira map")]
        GumowskiMiraMap,

        [Description("Spring pendulum")]
        SpringPendulumTrace,

        [Description("Loop chain")]
        LoopChain,

        [Description("Zigzag ribbon")]
        ZigzagRibbon,

        [Description("Arc chain")]
        ArcChain,

        [Description("Hook chain")]
        HookChain,

        [Description("Calligraphy stroke")]
        CalligraphyStroke,

        [Description("Radial rosette")]
        RadialRosetteStroke,

        [Description("L-system stroke")]
        LSystemStroke,

        [Description("Noise mask trace")]
        NoiseMaskTrace,

        [Description("Layered mask contour")]
        LayeredMaskContour,

        [Description("Beat pulse wave")]
        BeatPulseWave,

        [Description("Envelope wave")]
        EnvelopeWave,

        [Description("Radial pulse")]
        RadialPulse
    }

    internal enum NoiseSliderSeedPattern
    {
        Centre,

        [Description("Left edge")]
        LeftEdge,

        [Description("Right edge")]
        RightEdge,

        [Description("Top edge")]
        TopEdge,

        [Description("Bottom edge")]
        BottomEdge,

        Ring,

        Spiral,

        Random
    }

    internal enum NoiseSliderNoiseType
    {
        Gaussian,

        [Description("Value noise")]
        Value,

        [Description("Fractal value")]
        FractalValue,

        [Description("Curl field")]
        Curl,

        [Description("Sine warp")]
        SineWarp
    }

    internal readonly record struct NoiseSliderParameters(
        NoiseSliderSeedPattern SeedPattern,
        NoiseSliderNoiseType NoiseType,
        SliderToolbox2Type Type,
        double SeedPosition,
        double SeedAngle,
        double FieldBias,
        double StreamlineLength,
        int Samples,
        double Width,
        double Height,
        double Amplitude,
        double Frequency,
        int Octaves,
        double Persistence,
        int SmoothingPasses,
        int Seed);

    internal static class NoiseSliderGenerator
    {
        public static IReadOnlyList<Vector2> GeneratePath(NoiseSliderParameters parameters)
        {
            int samples = Math.Clamp(parameters.Samples, 2, 1024);
            const int source_samples = 1024;
            double width = Math.Clamp(parameters.Width, 16, 512);
            double height = Math.Clamp(parameters.Height, 16, 512);
            double amplitude = Math.Clamp(parameters.Amplitude, 0, 256);
            double streamlineLength = Math.Clamp(parameters.StreamlineLength, 16, 1024);
            double frequency = Math.Clamp(parameters.Frequency, 0.1, 16);
            int octaves = Math.Clamp(parameters.Octaves, 1, 8);
            double persistence = Math.Clamp(parameters.Persistence, 0.1, 0.95);
            int smoothingPasses = Math.Clamp(parameters.SmoothingPasses, 0, 16);
            double seedPosition = Math.Clamp(parameters.SeedPosition, 0, 1);
            SliderToolbox2Category activeCategory = GetCategory(parameters.Type);
            bool vectorFieldType = activeCategory == SliderToolbox2Category.VectorFlow;
            List<Vector2> points = parameters.Type switch
            {
                SliderToolbox2Type.CurlFlowLine or SliderToolbox2Type.SpiralFlowLine or SliderToolbox2Type.OrbitalFlowLine =>
                    createStreamline(parameters.Type, parameters.SeedPattern, parameters.NoiseType, seedPosition, source_samples, (float)width, (float)height, (float)streamlineLength, parameters.SeedAngle, parameters.FieldBias, amplitude, frequency, octaves, persistence, parameters.Seed),
                SliderToolbox2Type.SuperellipseContour => createSuperellipse(source_samples, (float)width, (float)height, amplitude, frequency, parameters.SeedAngle),
                SliderToolbox2Type.BlobbyContour => createBlobbyContour(source_samples, (float)width, (float)height, amplitude, frequency, octaves, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.RoundedPolygonContour => createRoundedPolygonContour(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                SliderToolbox2Type.MetaballContour => createMetaballContour(source_samples, (float)width, (float)height, amplitude, frequency, octaves, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.BooleanFieldContour => createBooleanFieldContour(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.HilbertPath => createHilbertPath(source_samples, (float)width, (float)height, octaves, parameters.SeedAngle),
                SliderToolbox2Type.PeanoPath => createPeanoPath(source_samples, (float)width, (float)height, octaves, parameters.SeedAngle),
                SliderToolbox2Type.MazeWalk => createMazeWalk(source_samples, (float)width, (float)height, octaves, persistence, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.MazeSolutionPath => createMazeSolutionPath(source_samples, (float)width, (float)height, octaves, persistence, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.VoronoiRidgeWalk => createVoronoiRidgeWalk(source_samples, (float)width, (float)height, amplitude, frequency, octaves, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.PointNetworkTour => createPointNetworkTour(source_samples, (float)width, (float)height, amplitude, frequency, octaves, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.LorenzAttractor => createLorenzAttractor(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.SeedAngle),
                SliderToolbox2Type.RosslerAttractor => createRosslerAttractor(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.SeedAngle),
                SliderToolbox2Type.HalvorsenAttractor => createHalvorsenAttractor(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.SeedAngle),
                SliderToolbox2Type.ThomasAttractor => createThomasAttractor(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.SeedAngle),
                SliderToolbox2Type.CliffordMap => createCliffordMap(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.DeJongMap => createDeJongMap(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.IkedaMap => createIkedaMap(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.GumowskiMiraMap => createGumowskiMiraMap(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.SpringPendulumTrace => createSpringPendulumTrace(source_samples, (float)width, (float)height, amplitude, frequency, persistence, seedPosition, parameters.FieldBias, parameters.SeedAngle),
                SliderToolbox2Type.LoopChain => createLoopChain(source_samples, (float)width, (float)height, amplitude, frequency, parameters.SeedAngle),
                SliderToolbox2Type.ZigzagRibbon => createZigzagRibbon(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                SliderToolbox2Type.ArcChain => createArcChain(source_samples, (float)width, (float)height, amplitude, frequency, parameters.SeedAngle),
                SliderToolbox2Type.HookChain => createHookChain(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                SliderToolbox2Type.CalligraphyStroke => createCalligraphyStroke(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                SliderToolbox2Type.RadialRosetteStroke => createRadialRosetteStroke(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                SliderToolbox2Type.LSystemStroke => createLSystemStroke(source_samples, (float)width, (float)height, amplitude, frequency, octaves, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.NoiseMaskTrace => createNoiseMaskTrace(source_samples, (float)width, (float)height, amplitude, frequency, octaves, persistence, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.LayeredMaskContour => createLayeredMaskContour(source_samples, (float)width, (float)height, amplitude, frequency, octaves, persistence, parameters.Seed, parameters.SeedAngle),
                SliderToolbox2Type.BeatPulseWave => createBeatPulseWave(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                SliderToolbox2Type.EnvelopeWave => createEnvelopeWave(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                SliderToolbox2Type.RadialPulse => createRadialPulse(source_samples, (float)width, (float)height, amplitude, frequency, persistence, parameters.SeedAngle),
                _ => createStreamline(parameters.Type, parameters.SeedPattern, parameters.NoiseType, seedPosition, source_samples, (float)width, (float)height, (float)streamlineLength, parameters.SeedAngle, parameters.FieldBias, amplitude, frequency, octaves, persistence, parameters.Seed)
            };

            if (!vectorFieldType)
            {
                if (activeCategory != SliderToolbox2Category.Attractors)
                {
                    points = applyPathOffset(points, seedPosition, isCyclicType(parameters.Type));
                    points = applyNonFieldBias(points, parameters.FieldBias, activeCategory);
                }

                points = normalisePlayfieldPathLength(points, (float)streamlineLength);
            }

            return clean(resample(smooth(points, smoothingPasses), samples));
        }

        public static SliderToolbox2Category GetCategory(SliderToolbox2Type type) => type switch
        {
            SliderToolbox2Type.CurlFlowLine or SliderToolbox2Type.SpiralFlowLine or SliderToolbox2Type.OrbitalFlowLine => SliderToolbox2Category.VectorFlow,
            SliderToolbox2Type.SuperellipseContour or SliderToolbox2Type.BlobbyContour or SliderToolbox2Type.RoundedPolygonContour or SliderToolbox2Type.MetaballContour or SliderToolbox2Type.BooleanFieldContour => SliderToolbox2Category.ImplicitContours,
            SliderToolbox2Type.HilbertPath or SliderToolbox2Type.PeanoPath or SliderToolbox2Type.MazeWalk or SliderToolbox2Type.MazeSolutionPath or SliderToolbox2Type.VoronoiRidgeWalk or SliderToolbox2Type.PointNetworkTour => SliderToolbox2Category.GraphPaths,
            SliderToolbox2Type.LorenzAttractor or SliderToolbox2Type.RosslerAttractor or SliderToolbox2Type.HalvorsenAttractor or SliderToolbox2Type.ThomasAttractor or SliderToolbox2Type.CliffordMap or SliderToolbox2Type.DeJongMap or SliderToolbox2Type.IkedaMap or SliderToolbox2Type.GumowskiMiraMap or SliderToolbox2Type.SpringPendulumTrace => SliderToolbox2Category.Attractors,
            SliderToolbox2Type.LoopChain or SliderToolbox2Type.ZigzagRibbon or SliderToolbox2Type.ArcChain or SliderToolbox2Type.HookChain or SliderToolbox2Type.CalligraphyStroke or SliderToolbox2Type.RadialRosetteStroke or SliderToolbox2Type.LSystemStroke => SliderToolbox2Category.StrokeGrammar,
            SliderToolbox2Type.NoiseMaskTrace or SliderToolbox2Type.LayeredMaskContour => SliderToolbox2Category.MaskTracing,
            SliderToolbox2Type.BeatPulseWave or SliderToolbox2Type.EnvelopeWave or SliderToolbox2Type.RadialPulse => SliderToolbox2Category.BeatDriven,
            _ => SliderToolbox2Category.VectorFlow
        };

        public static SliderToolbox2Type GetDefaultType(SliderToolbox2Category category) => category switch
        {
            SliderToolbox2Category.VectorFlow => SliderToolbox2Type.NoiseSampledPath,
            SliderToolbox2Category.ImplicitContours => SliderToolbox2Type.SuperellipseContour,
            SliderToolbox2Category.GraphPaths => SliderToolbox2Type.HilbertPath,
            SliderToolbox2Category.Attractors => SliderToolbox2Type.LorenzAttractor,
            SliderToolbox2Category.StrokeGrammar => SliderToolbox2Type.LoopChain,
            SliderToolbox2Category.MaskTracing => SliderToolbox2Type.NoiseMaskTrace,
            SliderToolbox2Category.BeatDriven => SliderToolbox2Type.BeatPulseWave,
            _ => SliderToolbox2Type.NoiseSampledPath
        };

        private static bool isVectorFieldType(SliderToolbox2Type type) => GetCategory(type) == SliderToolbox2Category.VectorFlow;

        public static Vector2 PreviewFieldVector(NoiseSliderParameters parameters, Vector2 playfieldPoint)
        {
            var local = new Vector2(playfieldPoint.X - 256, playfieldPoint.Y - 192);
            Vector2 vector = vectorField(parameters.Type, parameters.NoiseType, local, (float)parameters.Width, (float)parameters.Height,
                parameters.Frequency, parameters.Octaves, parameters.Persistence, parameters.Seed);
            vector = applyFieldBias(vector, parameters.SeedAngle, parameters.FieldBias);

            if (vector.LengthSquared <= 0.000001f)
                return Vector2.Zero;

            vector.Normalize();
            return vector * (float)Math.Clamp(parameters.Amplitude, 8, 256);
        }

        private static List<Vector2> createStreamline(
            SliderToolbox2Type type,
            NoiseSliderSeedPattern seedPattern,
            NoiseSliderNoiseType noiseType,
            double seedPosition,
            int samples,
            float width,
            float height,
            float streamlineLength,
            double seedAngle,
            double fieldBias,
            double amplitude,
            double frequency,
            int octaves,
            double persistence,
            int seed)
        {
            Vector2 point = seedPoint(seedPattern, seedPosition, width, height, seed);
            float targetLength = Math.Max(1, streamlineLength);
            float detailMultiplier = 1 + Math.Clamp(octaves, 1, 8) * 0.75f;
            float stepLength = Math.Clamp(targetLength / Math.Max(samples * detailMultiplier, 32f), 0.5f, 10f);
            int maxIterations = Math.Max(2, (int)Math.Ceiling(targetLength / stepLength) + 2);
            double travelled = 0;
            float fieldPull = (float)Math.Clamp(amplitude / 128, 0.05, 1);
            Vector2? previousDirection = null;
            var localPoints = new List<Vector2>(Math.Max(samples, 2)) { point };

            for (int i = 0; i < maxIterations && travelled < targetLength; i++)
            {
                Vector2 direction = streamlineDirection(type, noiseType, point, width, height, seedAngle, fieldBias, frequency, octaves, persistence, seed);
                Vector2 midpoint = point + direction * (stepLength * 0.5f);
                Vector2 midpointDirection = streamlineDirection(type, noiseType, midpoint, width, height, seedAngle, fieldBias, frequency, octaves, persistence, seed);
                Vector2 stepDirection = midpointDirection + boundarySteer(point, width, height) * 0.45f;

                if (stepDirection.LengthSquared <= 0.000001f)
                    stepDirection = direction;

                stepDirection.Normalize();

                if (previousDirection != null)
                {
                    stepDirection = previousDirection.Value * (1 - fieldPull) + stepDirection * fieldPull;

                    if (stepDirection.LengthSquared <= 0.000001f)
                        stepDirection = previousDirection.Value;
                    else
                        stepDirection.Normalize();
                }

                float currentStep = (float)Math.Min(stepLength, targetLength - travelled);
                point += stepDirection * currentStep;
                travelled += currentStep;
                previousDirection = stepDirection;
                localPoints.Add(point);
            }

            if (localPoints.Count == 1)
                localPoints.Add(point + Vector2.UnitX * targetLength);

            List<Vector2> sampled = resample(localPoints, samples);
            var result = new List<Vector2>(sampled.Count);

            foreach (Vector2 localPoint in sampled)
                result.Add(new Vector2(256, 192) + localPoint);

            return result;
        }

        private static Vector2 streamlineDirection(
            SliderToolbox2Type type,
            NoiseSliderNoiseType noiseType,
            Vector2 point,
            float width,
            float height,
            double seedAngle,
            double fieldBias,
            double frequency,
            int octaves,
            double persistence,
            int seed)
        {
            Vector2 direction = applyFieldBias(vectorField(type, noiseType, point, width, height, frequency, octaves, persistence, seed), seedAngle, fieldBias);

            if (direction.LengthSquared <= 0.000001f)
                return new Vector2(1, 0);

            direction.Normalize();
            return direction;
        }

        private static Vector2 vectorField(
            SliderToolbox2Type type,
            NoiseSliderNoiseType noiseType,
            Vector2 point,
            float width,
            float height,
            double frequency,
            int octaves,
            double persistence,
            int seed)
        {
            Vector2 relative = point - new Vector2(
                (float)(Math.Sin(seed * 0.013) * width * 0.12),
                (float)(Math.Cos(seed * 0.017) * height * 0.12));

            if (relative.LengthSquared <= 0.0001f)
                relative = Vector2.UnitX;

            Vector2 radial = relative;
            radial.Normalize();
            var tangent = new Vector2(-radial.Y, radial.X);
            double scalar = fractalNoise(point.X + 256, point.Y + 192, Math.Clamp(frequency, 0.1, 16), Math.Clamp(octaves, 1, 8), persistence, seed);
            var noiseVector = new Vector2((float)Math.Cos(scalar * Math.Tau), (float)Math.Sin(scalar * Math.Tau));

            Vector2 result = type switch
            {
                SliderToolbox2Type.SpiralFlowLine => tangent * 0.75f - radial * 0.42f + noiseVector * 0.2f,
                SliderToolbox2Type.OrbitalFlowLine => tangent + noiseVector * 0.16f,
                SliderToolbox2Type.CurlFlowLine => curl(point + new Vector2(256, 192), frequency, octaves, persistence, seed) + noiseVector * 0.18f,
                _ => noiseType switch
                {
                    NoiseSliderNoiseType.Curl => curl(point + new Vector2(256, 192), frequency, octaves, persistence, seed),
                    NoiseSliderNoiseType.SineWarp => sineVector(point, frequency, seed),
                    NoiseSliderNoiseType.Gaussian => hashVector(point, seed),
                    NoiseSliderNoiseType.Value => new Vector2((float)Math.Cos(valueNoise(point.X + 256, point.Y + 192, frequency, seed) * Math.Tau), (float)Math.Sin(valueNoise(point.X - 37, point.Y + 91, frequency, seed + 53) * Math.Tau)),
                    _ => noiseVector
                }
            };

            if (result.LengthSquared <= 0.000001f)
                return Vector2.UnitX;

            return result;
        }

        private static Vector2 applyFieldBias(Vector2 vector, double seedAngle, double fieldBias)
        {
            fieldBias = Math.Clamp(fieldBias, -1, 1);

            if (Math.Abs(fieldBias) <= 0.0001)
                return vector;

            double radians = seedAngle * Math.PI / 180;
            var biasDirection = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            return vector + biasDirection * (float)fieldBias;
        }

        private static Vector2 seedPoint(NoiseSliderSeedPattern pattern, double position, float width, float height, int seed)
        {
            position = Math.Clamp(position, 0, 1);
            double t = Math.Tau * position;

            return pattern switch
            {
                NoiseSliderSeedPattern.LeftEdge => new Vector2(-width / 2, -height / 2 + height * (float)position),
                NoiseSliderSeedPattern.RightEdge => new Vector2(width / 2, -height / 2 + height * (float)position),
                NoiseSliderSeedPattern.TopEdge => new Vector2(-width / 2 + width * (float)position, -height / 2),
                NoiseSliderSeedPattern.BottomEdge => new Vector2(-width / 2 + width * (float)position, height / 2),
                NoiseSliderSeedPattern.Ring => new Vector2((float)Math.Cos(t) * width * 0.35f, (float)Math.Sin(t) * height * 0.35f),
                NoiseSliderSeedPattern.Spiral => new Vector2((float)Math.Cos(t * 2.5) * width * (float)(0.06 + position * 0.34), (float)Math.Sin(t * 2.5) * height * (float)(0.06 + position * 0.34)),
                NoiseSliderSeedPattern.Random => randomSeedPoint(width, height, seed, position),
                _ => Vector2.Zero
            };
        }

        private static Vector2 randomSeedPoint(float width, float height, int seed, double position)
        {
            int index = (int)Math.Round(position * 997);
            return new Vector2(
                (float)(hashNoise(index, 17, seed) * width * 0.45),
                (float)(hashNoise(index, 31, seed + 7919) * height * 0.45));
        }

        private static Vector2 boundarySteer(Vector2 point, float width, float height)
        {
            float xLimit = Math.Max(1, width * 0.55f);
            float yLimit = Math.Max(1, height * 0.55f);
            float xOverflow = Math.Max(0, Math.Abs(point.X) - xLimit) / xLimit;
            float yOverflow = Math.Max(0, Math.Abs(point.Y) - yLimit) / yLimit;

            if (xOverflow <= 0 && yOverflow <= 0)
                return Vector2.Zero;

            var steer = new Vector2(-Math.Sign(point.X) * xOverflow, -Math.Sign(point.Y) * yOverflow);

            if (steer.LengthSquared <= 0.000001f)
                return Vector2.Zero;

            steer.Normalize();
            return steer;
        }

        private static void normaliseLengthToPlayfield(IReadOnlyList<Vector2> source, List<Vector2> result, float targetLength)
        {
            if (source.Count == 0)
                return;

            Vector2 centre = Vector2.Zero;

            foreach (Vector2 point in source)
                centre += point;

            centre /= source.Count;

            double length = 0;

            for (int i = 1; i < source.Count; i++)
                length += Vector2.Distance(source[i - 1], source[i]);

            float scale = length <= 0.0001 ? 1 : targetLength / (float)length;

            foreach (Vector2 point in source)
                result.Add(new Vector2(256, 192) + (point - centre) * scale);
        }

        private static List<Vector2> normalisePlayfieldPathLength(IReadOnlyList<Vector2> source, float targetLength)
        {
            var local = new List<Vector2>(source.Count);

            foreach (Vector2 point in source)
                local.Add(point - new Vector2(256, 192));

            var result = new List<Vector2>(source.Count);
            normaliseLengthToPlayfield(local, result, targetLength);
            return result;
        }

        private static Vector2 sineVector(Vector2 point, double frequency, int seed)
        {
            double scale = frequency / 96;
            return new Vector2(
                (float)Math.Cos(point.Y * scale + seed * 0.013),
                (float)Math.Sin(point.X * scale + seed * 0.017));
        }

        private static Vector2 hashVector(Vector2 point, int seed)
        {
            double x = hashNoise((int)Math.Round(point.X * 2), (int)Math.Round(point.Y * 2), seed);
            double y = hashNoise((int)Math.Round(point.X * 2), (int)Math.Round(point.Y * 2), seed + 1299721);
            return new Vector2((float)x, (float)y);
        }

        private static List<Vector2> createSuperellipse(int samples, float width, float height, double amplitude, double frequency, double pathAngle)
        {
            double exponent = Math.Clamp(0.45 + amplitude / 48, 0.45, 6);
            double wobble = Math.Clamp(frequency / 40, 0, 0.35);
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = Math.Tau * i / Math.Max(1, samples - 1);
                double cos = Math.Cos(t);
                double sin = Math.Sin(t);
                double radius = 1 + wobble * Math.Sin(4 * t);
                double x = Math.Sign(cos) * Math.Pow(Math.Abs(cos), 2 / exponent) * width / 2 * radius;
                double y = Math.Sign(sin) * Math.Pow(Math.Abs(sin), 2 / exponent) * height / 2 * radius;
                result.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return result;
        }

        private static List<Vector2> createBlobbyContour(int samples, float width, float height, double amplitude, double frequency, int octaves, int seed, double pathAngle)
        {
            int lobes = Math.Clamp((int)Math.Round(frequency), 2, 24);
            double strength = Math.Clamp(amplitude / 160, 0, 0.65);
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = Math.Tau * i / Math.Max(1, samples - 1);
                double radius = 1;

                for (int octave = 0; octave < Math.Clamp(octaves, 1, 8); octave++)
                    radius += strength / (octave + 1) * Math.Sin((lobes + octave * 2) * t + seed * 0.017 + octave);

                double x = Math.Cos(t) * width / 2 * radius;
                double y = Math.Sin(t) * height / 2 * radius;
                result.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return result;
        }

        private static List<Vector2> createRoundedPolygonContour(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            int sides = Math.Clamp((int)Math.Round(frequency), 3, 24);
            double roundness = Math.Clamp(persistence, 0.1, 0.95);
            double wobble = Math.Clamp(amplitude / 360, 0, 0.5);
            var vertices = new Vector2[sides];

            for (int i = 0; i < sides; i++)
            {
                double t = Math.Tau * i / sides - Math.PI / 2;
                double radius = 1 + wobble * Math.Sin((i + 1) * 1.618);
                vertices[i] = new Vector2((float)(Math.Cos(t) * width * 0.5 * radius), (float)(Math.Sin(t) * height * 0.5 * radius));
            }

            var source = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double p = i / (double)Math.Max(1, samples - 1) * sides;
                int index = (int)Math.Floor(p) % sides;
                double local = p - Math.Floor(p);
                Vector2 previous = vertices[(index - 1 + sides) % sides];
                Vector2 current = vertices[index];
                Vector2 next = vertices[(index + 1) % sides];
                Vector2 edgePoint = Vector2.Lerp(current, next, (float)local);
                Vector2 smoothPoint = quadraticBezier(Vector2.Lerp(previous, current, 0.5f), current, Vector2.Lerp(current, next, 0.5f), (float)local);
                source.Add(Vector2.Lerp(edgePoint, smoothPoint, (float)roundness));
            }

            return source.Select(p => rotateToPlayfield(p, pathAngle)).ToList();
        }

        private static List<Vector2> createMetaballContour(int samples, float width, float height, double amplitude, double frequency, int octaves, int seed, double pathAngle)
        {
            int ballCount = Math.Clamp(octaves + 2, 3, 10);
            double influence = Math.Clamp(amplitude / 180, 0.2, 1.3);
            double radius = 0.62 + influence * 0.18;
            var centres = new Vector2[ballCount];

            for (int i = 0; i < ballCount; i++)
            {
                double t = Math.Tau * i / ballCount + seed * 0.017;
                double r = 0.15 + 0.32 * Math.Abs(Math.Sin(seed * 0.031 + i));
                centres[i] = new Vector2((float)(Math.Cos(t) * width * r), (float)(Math.Sin(t * 1.13) * height * r));
            }

            return sampleRadialContour(samples, width, height, pathAngle, t =>
            {
                var direction = new Vector2((float)Math.Cos(t), (float)Math.Sin(t));
                double field = 0;

                foreach (Vector2 centre in centres)
                {
                    Vector2 unitCentre = new Vector2(centre.X / Math.Max(1, width), centre.Y / Math.Max(1, height));
                    double projection = Vector2.Dot(direction, unitCentre);
                    field += Math.Max(0, projection) * 0.35 + 0.08 / Math.Max(0.08, Math.Abs(projection) + 0.16);
                }

                return radius + Math.Sin(t * Math.Clamp(frequency, 1, 16) + seed * 0.01) * influence * 0.06 + field * influence * 0.08;
            });
        }

        private static List<Vector2> createBooleanFieldContour(int samples, float width, float height, double amplitude, double frequency, double persistence, int seed, double pathAngle)
        {
            int lobes = Math.Clamp((int)Math.Round(frequency), 2, 16);
            double unionWeight = Math.Clamp(persistence, 0.1, 0.95);
            double cornering = Math.Clamp(amplitude / 180, 0, 1);

            return sampleRadialContour(samples, width, height, pathAngle, t =>
            {
                double capsule = 0.72 + 0.18 * Math.Abs(Math.Cos(t * 2));
                double circles = 0.72 + 0.14 * Math.Max(Math.Cos(t * lobes + seed * 0.011), Math.Cos((lobes - 1) * t - seed * 0.019));
                double box = Math.Min(1.05, 0.58 / Math.Max(0.36, Math.Max(Math.Abs(Math.Cos(t)), Math.Abs(Math.Sin(t)))));
                double union = Math.Max(capsule, circles);
                double intersection = Math.Min(union, box);
                return lerp(intersection, union, unionWeight) * (1 + cornering * 0.08 * Math.Sin(t * 4));
            });
        }

        private static List<Vector2> createHilbertPath(int samples, float width, float height, int octaves, double pathAngle)
        {
            int order = Math.Clamp(octaves, 1, 6);
            int side = 1 << order;
            int count = side * side;
            var source = new List<Vector2>(count);

            for (int d = 0; d < count; d++)
            {
                (int x, int y) = hilbertD2xy(side, d);
                source.Add(new Vector2(
                    -width / 2 + width * x / Math.Max(1, side - 1),
                    -height / 2 + height * y / Math.Max(1, side - 1)));
            }

            return resample(source.Select(p => rotateToPlayfield(p, pathAngle)).ToList(), samples);
        }

        private static List<Vector2> createPeanoPath(int samples, float width, float height, int octaves, double pathAngle)
        {
            int order = Math.Clamp(octaves, 1, 4);
            int side = 1;

            for (int i = 0; i < order; i++)
                side *= 3;

            var source = new List<Vector2>(side * side);

            for (int row = 0; row < side; row++)
            {
                bool reverse = row % 2 == 1;

                for (int step = 0; step < side; step++)
                {
                    int column = reverse ? side - 1 - step : step;
                    source.Add(new Vector2(
                        -width / 2 + width * column / Math.Max(1, side - 1),
                        -height / 2 + height * row / Math.Max(1, side - 1)));
                }
            }

            return resample(source.Select(p => rotateToPlayfield(p, pathAngle)).ToList(), samples);
        }

        private static List<Vector2> createMazeWalk(int samples, float width, float height, int octaves, double persistence, int seed, double pathAngle)
        {
            int rows = Math.Clamp(octaves + 3, 4, 14);
            int columns = Math.Clamp((int)Math.Round(rows * (1.2 + persistence)), 4, 20);
            var random = new Random(seed);
            var source = new List<Vector2>(rows * columns);

            for (int row = 0; row < rows; row++)
            {
                bool reverse = row % 2 == 1;

                for (int step = 0; step < columns; step++)
                {
                    int column = reverse ? columns - 1 - step : step;
                    float jitter = (float)(random.NextDouble() - 0.5) * height / rows * 0.35f;
                    source.Add(new Vector2(
                        -width / 2 + width * column / Math.Max(1, columns - 1),
                        -height / 2 + height * row / Math.Max(1, rows - 1) + jitter));
                }
            }

            return resample(source.Select(p => rotateToPlayfield(p, pathAngle)).ToList(), samples);
        }

        private static List<Vector2> createMazeSolutionPath(int samples, float width, float height, int octaves, double persistence, int seed, double pathAngle)
        {
            int rows = Math.Clamp(octaves + 4, 5, 15);
            int columns = Math.Clamp((int)Math.Round(rows * (1.1 + persistence)), 5, 22);
            var random = new Random(seed);
            var source = new List<Vector2>();
            int row = 0;
            int column = 0;
            source.Add(gridPoint(row, column, rows, columns, width, height));

            while (row < rows - 1 || column < columns - 1)
            {
                bool canRight = column < columns - 1;
                bool canDown = row < rows - 1;
                bool moveRight = canRight && (!canDown || random.NextDouble() < 0.52 + (persistence - 0.5) * 0.35);

                if (moveRight)
                    column++;
                else if (canDown)
                    row++;

                source.Add(gridPoint(row, column, rows, columns, width, height));

                if (random.NextDouble() < 0.22)
                {
                    int spurColumn = Math.Clamp(column + (random.Next(2) == 0 ? -1 : 1), 0, columns - 1);
                    source.Add(gridPoint(row, spurColumn, rows, columns, width, height));
                    source.Add(gridPoint(row, column, rows, columns, width, height));
                }
            }

            return resample(source.Select(p => rotateToPlayfield(p, pathAngle)).ToList(), samples);
        }

        private static List<Vector2> createVoronoiRidgeWalk(int samples, float width, float height, double amplitude, double frequency, int octaves, int seed, double pathAngle)
        {
            int siteCount = Math.Clamp((int)Math.Round(8 + frequency * 2 + octaves), 8, 48);
            List<Vector2> sites = deterministicPoints(siteCount, width, height, seed);
            var edges = new List<(int A, int B, double Distance)>();

            for (int i = 0; i < sites.Count; i++)
            {
                for (int j = i + 1; j < sites.Count; j++)
                {
                    double distance = Vector2.DistanceSquared(sites[i], sites[j]);

                    if (isNearNeighbourEdge(sites, i, j, distance, 4))
                        edges.Add((i, j, distance));
                }
            }

            List<int> route = nearestEdgeRoute(sites, edges, seed);
            var source = new List<Vector2>(route.Count);

            foreach (int index in route)
            {
                Vector2 site = sites[index];
                double jitter = Math.Clamp(amplitude / 256, 0, 1) * 0.12;
                var offset = new Vector2((float)(hashNoise(index, seed, seed + 17) * width * jitter), (float)(hashNoise(index, seed, seed + 31) * height * jitter));
                source.Add(site + offset);
            }

            return resample(source.Select(p => rotateToPlayfield(p, pathAngle)).ToList(), samples);
        }

        private static List<Vector2> createPointNetworkTour(int samples, float width, float height, double amplitude, double frequency, int octaves, int seed, double pathAngle)
        {
            int pointCount = Math.Clamp((int)Math.Round(10 + frequency * 3 + octaves * 2), 8, 80);
            List<Vector2> points = deterministicPoints(pointCount, width, height, seed);
            bool[] used = new bool[points.Count];
            var source = new List<Vector2>(points.Count);
            int current = Math.Abs(seed) % points.Count;

            for (int step = 0; step < points.Count; step++)
            {
                used[current] = true;
                source.Add(points[current]);
                int next = -1;
                double best = double.MaxValue;

                for (int i = 0; i < points.Count; i++)
                {
                    if (used[i])
                        continue;

                    double distance = Vector2.DistanceSquared(points[current], points[i]);
                    distance *= 1 + Math.Clamp(amplitude / 256, 0, 1) * Math.Abs(hashNoise(current, i, seed));

                    if (distance < best)
                    {
                        best = distance;
                        next = i;
                    }
                }

                if (next < 0)
                    break;

                current = next;
            }

            return resample(source.Select(p => rotateToPlayfield(p, pathAngle)).ToList(), samples);
        }

        private static List<Vector2> createLorenzAttractor(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, double pathAngle)
        {
            double sigma = 10;
            double rho = Math.Clamp(20 + frequency, 5, 48);
            double beta = Math.Clamp(2.2 + persistence, 1, 5);
            double dt = Math.Clamp(0.004 + amplitude / 24000, 0.002, 0.018);
            double x = 0.1;
            double y = 0;
            double z = 0;
            int warmup = 120 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double dx = sigma * (y - x);
                double dy = x * (rho - z) - y;
                double dz = x * y - beta * z;
                x += dx * dt;
                y += dy * dt;
                z += dz * dt;

                if (i >= warmup)
                    raw.Add(projectAttractor(x, y, z, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createRosslerAttractor(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, double pathAngle)
        {
            double a = 0.15 + persistence * 0.12;
            double b = 0.2;
            double c = Math.Clamp(4 + frequency * 0.45, 4, 12);
            double dt = Math.Clamp(0.018 + amplitude / 18000, 0.01, 0.045);
            double x = 0.1;
            double y = 0;
            double z = 0;
            int warmup = 180 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double dx = -y - z;
                double dy = x + a * y;
                double dz = b + z * (x - c);
                x += dx * dt;
                y += dy * dt;
                z += dz * dt;

                if (i >= warmup)
                    raw.Add(projectAttractor(x, y, z, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createHalvorsenAttractor(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, double pathAngle)
        {
            double a = Math.Clamp(1.25 + frequency / 32 + persistence * 0.2, 1.1, 1.8);
            double dt = Math.Clamp(0.006 + amplitude / 32000, 0.004, 0.018);
            double x = 0.1;
            double y = 0;
            double z = 0;
            int warmup = 200 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double dx = -a * x - 4 * y - 4 * z - y * y;
                double dy = -a * y - 4 * z - 4 * x - z * z;
                double dz = -a * z - 4 * x - 4 * y - x * x;
                x += dx * dt;
                y += dy * dt;
                z += dz * dt;

                if (i >= warmup)
                    raw.Add(projectAttractor(x, y, z, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createThomasAttractor(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, double pathAngle)
        {
            double b = Math.Clamp(0.12 + persistence * 0.12 + frequency / 240, 0.1, 0.35);
            double dt = Math.Clamp(0.025 + amplitude / 14000, 0.015, 0.06);
            double x = 0.1;
            double y = 0;
            double z = 0;
            int warmup = 160 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double dx = Math.Sin(y) - b * x;
                double dy = Math.Sin(z) - b * y;
                double dz = Math.Sin(x) - b * z;
                x += dx * dt;
                y += dy * dt;
                z += dz * dt;

                if (i >= warmup)
                    raw.Add(projectAttractor(x, y, z, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createCliffordMap(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, int seed, double pathAngle)
        {
            double a = -1.4 + Math.Sin(seed * 0.01) * 0.35 + amplitude / 420;
            double b = 1.6 + Math.Cos(seed * 0.013) * 0.35;
            double c = 1.0 + frequency / 18;
            double d = 0.7 + persistence;
            double x = 0.1;
            double y = 0.1;
            int warmup = 80 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double nextX = Math.Sin(a * y) + c * Math.Cos(a * x);
                double nextY = Math.Sin(b * x) + d * Math.Cos(b * y);
                x = nextX;
                y = nextY;

                if (i >= warmup)
                    raw.Add(projectMap(x, y, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createDeJongMap(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, int seed, double pathAngle)
        {
            double a = -2.0 + Math.Sin(seed * 0.011) * 0.55 + frequency / 40;
            double b = -2.3 + Math.Cos(seed * 0.017) * 0.55 + persistence;
            double c = 1.9 + Math.Sin(seed * 0.023) * 0.45 + amplitude / 420;
            double d = -0.7 + Math.Cos(seed * 0.029) * 0.45;
            double x = 0.1;
            double y = 0.1;
            int warmup = 100 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double nextX = Math.Sin(a * y) - Math.Cos(b * x);
                double nextY = Math.Sin(c * x) - Math.Cos(d * y);
                x = nextX;
                y = nextY;

                if (i >= warmup)
                    raw.Add(projectMap(x, y, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createIkedaMap(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, int seed, double pathAngle)
        {
            double u = Math.Clamp(0.76 + persistence * 0.18, 0.75, 0.94);
            double phase = seed * 0.001 + frequency * 0.05;
            double x = 0.1;
            double y = 0.1;
            int warmup = 120 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double t = 0.4 - 6 / (1 + x * x + y * y) + phase;
                double nextX = 1 + u * (x * Math.Cos(t) - y * Math.Sin(t));
                double nextY = u * (x * Math.Sin(t) + y * Math.Cos(t)) + amplitude / 1000;
                x = nextX;
                y = nextY;

                if (i >= warmup)
                    raw.Add(projectMap(x, y, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createGumowskiMiraMap(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, int seed, double pathAngle)
        {
            double a = Math.Clamp(0.008 + frequency / 2000, 0.001, 0.03);
            double b = Math.Clamp(0.75 + persistence * 0.25 + amplitude / 1800, 0.7, 1.1);
            double x = 0.1 + Math.Sin(seed * 0.01) * 0.05;
            double y = 0;
            int warmup = 120 + (int)Math.Round(Math.Clamp(warmOffset, 0, 1) * 900);
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples + warmup; i++)
            {
                double fx = a * x + 2 * (1 - a) * x * x / (1 + x * x);
                double nextX = b * y + fx;
                double nextY = -x + fx;
                x = nextX;
                y = nextY;

                if (i >= warmup)
                    raw.Add(projectMap(x, y, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createSpringPendulumTrace(int samples, float width, float height, double amplitude, double frequency, double persistence, double warmOffset, double projectionBias, double pathAngle)
        {
            double damping = Math.Clamp(0.006 + (1 - persistence) * 0.02, 0.004, 0.03);
            double angularFrequency = Math.Clamp(frequency, 0.5, 16) * 0.12;
            double radialFrequency = angularFrequency * (1.35 + persistence);
            double radialAmplitude = Math.Clamp(amplitude, 8, 256) / 256;
            var raw = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = (warmOffset + i / (double)Math.Max(1, samples - 1)) * Math.Tau * (1 + frequency * 0.55);
                double envelope = Math.Exp(-damping * i);
                double radius = (0.45 + radialAmplitude * 0.35 * Math.Sin(radialFrequency * t)) * envelope;
                double angle = angularFrequency * t + radialAmplitude * Math.Sin(t * 0.7);
                raw.Add(projectMap(Math.Cos(angle) * radius, Math.Sin(angle) * radius, projectionBias));
            }

            return normaliseAndRotate(raw, width, height, pathAngle);
        }

        private static List<Vector2> createLoopChain(int samples, float width, float height, double amplitude, double frequency, double pathAngle)
        {
            double loops = Math.Clamp(frequency, 1, 24);
            double loopRadius = Math.Clamp(amplitude, 8, 160);
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)Math.Max(1, samples - 1);
                double x = -width / 2 + width * t;
                double y = Math.Sin(Math.Tau * loops * t) * Math.Min(height / 2, loopRadius);
                x += Math.Cos(Math.Tau * loops * t) * loopRadius * 0.25;
                result.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return result;
        }

        private static List<Vector2> createZigzagRibbon(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            double zigs = Math.Clamp(frequency, 1, 64);
            double roundness = Math.Clamp(persistence, 0.1, 0.95);
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)Math.Max(1, samples - 1);
                double phase = t * zigs;
                double triangle = 2 * Math.Abs(phase - Math.Floor(phase + 0.5));
                triangle = triangle * 2 - 1;
                double y = lerp(triangle, Math.Sin(Math.Tau * phase), roundness) * Math.Min(height / 2, amplitude);
                double x = -width / 2 + width * t;
                result.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return result;
        }

        private static List<Vector2> createArcChain(int samples, float width, float height, double amplitude, double frequency, double pathAngle)
        {
            double arcs = Math.Clamp(frequency, 1, 32);
            double arcHeight = Math.Min(height / 2, Math.Max(8, amplitude));
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)Math.Max(1, samples - 1);
                double phase = t * arcs;
                double local = phase - Math.Floor(phase);
                double sign = Math.Floor(phase) % 2 == 0 ? 1 : -1;
                double x = -width / 2 + width * t;
                double y = sign * Math.Sin(Math.PI * local) * arcHeight;
                result.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return result;
        }

        private static List<Vector2> createHookChain(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            int hooks = Math.Clamp((int)Math.Round(frequency), 1, 24);
            double hookRadius = Math.Min(height / 2, Math.Max(8, amplitude));
            double curl = Math.Clamp(persistence, 0.1, 0.95);
            var source = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)Math.Max(1, samples - 1);
                double phase = t * hooks;
                double local = phase - Math.Floor(phase);
                double hookAngle = Math.PI * (0.25 + local * (1.25 + curl));
                double x = -width / 2 + width * t + Math.Cos(hookAngle) * hookRadius * 0.22;
                double y = Math.Sin(hookAngle) * hookRadius * (Math.Floor(phase) % 2 == 0 ? 1 : -1);
                source.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return source;
        }

        private static List<Vector2> createCalligraphyStroke(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            double strokes = Math.Clamp(frequency, 1, 20);
            double pressure = Math.Clamp(amplitude / 160, 0.05, 1.4);
            double slant = (persistence - 0.5) * 0.75;
            var source = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)Math.Max(1, samples - 1);
                double envelope = Math.Sin(Math.PI * t);
                double wave = Math.Sin(Math.Tau * strokes * t);
                double fine = Math.Sin(Math.Tau * (strokes * 2.7) * t + 0.8) * 0.28;
                double x = -width / 2 + width * t + slant * height * envelope * 0.35;
                double y = (wave + fine) * Math.Min(height / 2, amplitude) * pressure * (0.35 + envelope * 0.65);
                source.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return source;
        }

        private static List<Vector2> createRadialRosetteStroke(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            double petals = Math.Clamp(frequency, 2, 32);
            double inner = Math.Clamp(persistence, 0.1, 0.95);
            double modulation = Math.Clamp(amplitude / 200, 0.05, 1.2);
            var source = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = Math.Tau * i / Math.Max(1, samples - 1);
                double radius = 0.35 + modulation * 0.22 * Math.Cos(petals * t);
                radius = Math.Max(0.08, lerp(Math.Abs(radius), radius, inner));
                double x = Math.Cos(t) * width * radius;
                double y = Math.Sin(t) * height * radius;
                source.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return source;
        }

        private static List<Vector2> createLSystemStroke(int samples, float width, float height, double amplitude, double frequency, int octaves, int seed, double pathAngle)
        {
            int iterations = Math.Clamp(octaves, 1, 6);
            double turnAngle = (25 + Math.Clamp(frequency, 1, 16) * 4) * Math.PI / 180;
            string sequence = "F";

            for (int i = 0; i < iterations; i++)
            {
                var next = new System.Text.StringBuilder(sequence.Length * 3);

                foreach (char token in sequence)
                    next.Append(token == 'F' ? "F+F-F-F+F" : token);

                sequence = next.ToString();
            }

            double angle = -Math.PI / 2 + seed * 0.001;
            var position = Vector2.Zero;
            var source = new List<Vector2> { position };
            double step = 1;
            int consumed = 0;
            int maxTokens = Math.Min(sequence.Length, 12000);

            for (int i = 0; i < maxTokens; i++)
            {
                char token = sequence[i];

                if (token == 'F')
                {
                    position += new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (float)step;
                    source.Add(position);
                    consumed++;
                }
                else if (token == '+')
                    angle += turnAngle;
                else if (token == '-')
                    angle -= turnAngle * (0.85 + Math.Clamp(amplitude / 256, 0, 1) * 0.3);

                if (consumed > samples * 8)
                    break;
            }

            return resample(normaliseAndRotate(source, width, height, pathAngle), samples);
        }

        private static List<Vector2> createNoiseMaskTrace(int samples, float width, float height, double amplitude, double frequency, int octaves, double persistence, int seed, double pathAngle)
        {
            double threshold = Math.Clamp(persistence, 0.1, 0.95);
            double radiusStrength = Math.Clamp(amplitude / 180, 0.05, 0.9);
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = Math.Tau * i / Math.Max(1, samples - 1);
                double x = Math.Cos(t);
                double y = Math.Sin(t);
                double mask = fractalNoise(256 + x * width * 0.35, 192 + y * height * 0.35, frequency, octaves, persistence, seed);
                double contour = 1 + radiusStrength * (mask - (threshold - 0.5));
                double localX = x * width / 2 * contour;
                double localY = y * height / 2 * contour;
                result.Add(rotateToPlayfield(new Vector2((float)localX, (float)localY), pathAngle));
            }

            return result;
        }

        private static List<Vector2> createLayeredMaskContour(int samples, float width, float height, double amplitude, double frequency, int octaves, double persistence, int seed, double pathAngle)
        {
            int layers = Math.Clamp(octaves, 1, 8);
            double threshold = Math.Clamp(persistence, 0.1, 0.95);
            double force = Math.Clamp(amplitude / 160, 0.05, 1.2);

            return sampleRadialContour(samples, width, height, pathAngle, t =>
            {
                double radius = 0.42;

                for (int layer = 0; layer < layers; layer++)
                {
                    double phase = t + layer * Math.Tau / layers;
                    double sampleX = 256 + Math.Cos(phase) * width * (0.18 + layer * 0.035);
                    double sampleY = 192 + Math.Sin(phase) * height * (0.18 + layer * 0.035);
                    double mask = fractalNoise(sampleX, sampleY, frequency + layer * 0.7, octaves, persistence, seed + layer * 193);
                    radius += (mask - (threshold - 0.5)) * force * 0.08 / (layer + 1);
                }

                radius += Math.Sin(t * layers * 2 + seed * 0.013) * force * 0.025;
                return Math.Clamp(radius, 0.16, 0.95);
            });
        }

        private static List<Vector2> createBeatPulseWave(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            double pulses = Math.Clamp(frequency, 1, 64);
            double decay = Math.Clamp(1 - persistence, 0.05, 0.95);
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)Math.Max(1, samples - 1);
                double phase = t * pulses;
                double local = phase - Math.Floor(phase);
                double pulse = Math.Exp(-local / decay) * Math.Sin(Math.PI * local);
                double y = pulse * Math.Min(height / 2, amplitude);
                double x = -width / 2 + width * t;
                result.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return result;
        }

        private static List<Vector2> createEnvelopeWave(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            double cycles = Math.Clamp(frequency, 1, 64);
            double attack = Math.Clamp(persistence, 0.1, 0.95);
            var source = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = i / (double)Math.Max(1, samples - 1);
                double beat = t * cycles;
                double local = beat - Math.Floor(beat);
                double envelope = local < attack
                    ? smoothStep(local / attack)
                    : 1 - smoothStep((local - attack) / Math.Max(0.001, 1 - attack));
                double carrier = Math.Sin(Math.Tau * beat) * 0.65 + Math.Sin(Math.Tau * beat * 2.01) * 0.25;
                double x = -width / 2 + width * t;
                double y = carrier * envelope * Math.Min(height / 2, amplitude);
                source.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return source;
        }

        private static List<Vector2> createRadialPulse(int samples, float width, float height, double amplitude, double frequency, double persistence, double pathAngle)
        {
            double pulses = Math.Clamp(frequency, 2, 48);
            double decay = Math.Clamp(1 - persistence, 0.05, 0.95);
            double force = Math.Clamp(amplitude / 160, 0.05, 1.4);
            var source = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = Math.Tau * i / Math.Max(1, samples - 1);
                double phase = pulses * t / Math.Tau;
                double local = phase - Math.Floor(phase);
                double pulse = Math.Exp(-local / decay) * Math.Sin(Math.PI * local);
                double radius = 0.36 + pulse * force * 0.22 + Math.Sin(t * 3) * 0.035;
                double x = Math.Cos(t) * width * radius;
                double y = Math.Sin(t) * height * radius;
                source.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return source;
        }

        private static bool isCyclicType(SliderToolbox2Type type) => type is
            SliderToolbox2Type.SuperellipseContour
            or SliderToolbox2Type.BlobbyContour
            or SliderToolbox2Type.RoundedPolygonContour
            or SliderToolbox2Type.MetaballContour
            or SliderToolbox2Type.BooleanFieldContour
            or SliderToolbox2Type.RadialRosetteStroke
            or SliderToolbox2Type.NoiseMaskTrace
            or SliderToolbox2Type.LayeredMaskContour
            or SliderToolbox2Type.RadialPulse;

        private static List<Vector2> applyPathOffset(IReadOnlyList<Vector2> points, double offset, bool cyclic)
        {
            if (points.Count < 2)
                return points.ToList();

            offset = offset - Math.Floor(offset);

            if (offset <= 0.0001)
                return points.ToList();

            int shift = Math.Clamp((int)Math.Round(offset * (points.Count - 1)), 0, points.Count - 1);

            if (shift == 0)
                return points.ToList();

            var result = new List<Vector2>(points.Count);

            if (!cyclic)
            {
                for (int i = shift; i < points.Count; i++)
                    result.Add(points[i]);

                for (int i = shift - 1; i >= 0; i--)
                    result.Add(points[i]);

                return result;
            }

            for (int i = 0; i < points.Count; i++)
            {
                int index = (i + shift) % points.Count;
                result.Add(points[index]);
            }

            return result;
        }

        private static List<Vector2> applyNonFieldBias(IReadOnlyList<Vector2> points, double bias, SliderToolbox2Category category)
        {
            bias = Math.Clamp(bias, -1, 1);

            if (points.Count == 0 || Math.Abs(bias) <= 0.0001)
                return points.ToList();

            var result = new List<Vector2>(points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                double progress = i / (double)Math.Max(1, points.Count - 1);
                Vector2 local = points[i] - new Vector2(256, 192);

                switch (category)
                {
                    case SliderToolbox2Category.ImplicitContours:
                    case SliderToolbox2Category.MaskTracing:
                        local.X += local.Y * (float)(bias * 0.35);
                        break;

                    case SliderToolbox2Category.GraphPaths:
                        local.Y += (float)(Math.Sin(progress * Math.PI) * bias * 64);
                        break;

                    case SliderToolbox2Category.StrokeGrammar:
                    case SliderToolbox2Category.BeatDriven:
                        local.Y += (float)((progress - 0.5) * bias * 128);
                        break;
                }

                result.Add(new Vector2(256, 192) + local);
            }

            return result;
        }

        private static Vector2 projectAttractor(double x, double y, double z, double projectionBias)
        {
            projectionBias = Math.Clamp(projectionBias, -1, 1);
            double zWeight = Math.Abs(projectionBias);
            double projectedX = projectionBias >= 0 ? lerp(x, z, zWeight) : x;
            double projectedY = projectionBias < 0 ? lerp(y, z, zWeight) : y;
            return new Vector2((float)projectedX, (float)projectedY);
        }

        private static Vector2 projectMap(double x, double y, double projectionBias)
        {
            projectionBias = Math.Clamp(projectionBias, -1, 1);
            double projectedX = x + y * projectionBias * 0.35;
            double projectedY = y - x * projectionBias * 0.35;
            return new Vector2((float)projectedX, (float)projectedY);
        }

        private static List<Vector2> sampleRadialContour(int samples, float width, float height, double pathAngle, Func<double, double> radiusAt)
        {
            var result = new List<Vector2>(samples);

            for (int i = 0; i < samples; i++)
            {
                double t = Math.Tau * i / Math.Max(1, samples - 1);
                double radius = Math.Clamp(radiusAt(t), 0.02, 1.8);
                double x = Math.Cos(t) * width * 0.5 * radius;
                double y = Math.Sin(t) * height * 0.5 * radius;
                result.Add(rotateToPlayfield(new Vector2((float)x, (float)y), pathAngle));
            }

            return result;
        }

        private static Vector2 quadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
        {
            float inverse = 1 - t;
            return start * inverse * inverse + control * 2 * inverse * t + end * t * t;
        }

        private static Vector2 gridPoint(int row, int column, int rows, int columns, float width, float height) => new Vector2(
            -width / 2 + width * column / Math.Max(1, columns - 1),
            -height / 2 + height * row / Math.Max(1, rows - 1));

        private static List<Vector2> deterministicPoints(int count, float width, float height, int seed)
        {
            var result = new List<Vector2>(count);

            for (int i = 0; i < count; i++)
            {
                double radius = Math.Sqrt((hashNoise(i, seed, seed + 11) + 1) * 0.5) * 0.48;
                double angle = Math.Tau * ((hashNoise(i, seed, seed + 23) + 1) * 0.5);
                double jitterX = hashNoise(i, 97, seed) * width * 0.04;
                double jitterY = hashNoise(i, 193, seed) * height * 0.04;
                result.Add(new Vector2(
                    (float)(Math.Cos(angle) * width * radius + jitterX),
                    (float)(Math.Sin(angle) * height * radius + jitterY)));
            }

            return result;
        }

        private static bool isNearNeighbourEdge(IReadOnlyList<Vector2> points, int first, int second, double distance, int neighbours)
        {
            int closerToFirst = 0;
            int closerToSecond = 0;

            for (int i = 0; i < points.Count; i++)
            {
                if (i == first || i == second)
                    continue;

                if (Vector2.DistanceSquared(points[first], points[i]) < distance)
                    closerToFirst++;

                if (Vector2.DistanceSquared(points[second], points[i]) < distance)
                    closerToSecond++;
            }

            return closerToFirst < neighbours || closerToSecond < neighbours;
        }

        private static List<int> nearestEdgeRoute(IReadOnlyList<Vector2> points, IReadOnlyList<(int A, int B, double Distance)> edges, int seed)
        {
            if (points.Count == 0)
                return new List<int>();

            if (edges.Count == 0)
                return Enumerable.Range(0, points.Count).ToList();

            var adjacency = new Dictionary<int, List<int>>();

            foreach (var (a, b, _) in edges)
            {
                if (!adjacency.TryGetValue(a, out List<int>? aEdges))
                    adjacency[a] = aEdges = new List<int>();

                if (!adjacency.TryGetValue(b, out List<int>? bEdges))
                    adjacency[b] = bEdges = new List<int>();

                aEdges.Add(b);
                bEdges.Add(a);
            }

            var used = new bool[points.Count];
            var route = new List<int>();
            int current = Math.Abs(seed) % points.Count;

            for (int step = 0; step < points.Count; step++)
            {
                used[current] = true;
                route.Add(current);
                int next = -1;
                double best = double.MaxValue;

                IEnumerable<int> candidates = adjacency.TryGetValue(current, out List<int>? neighbours)
                    ? neighbours.Where(index => !used[index])
                    : Enumerable.Empty<int>();

                foreach (int candidate in candidates)
                {
                    double distance = Vector2.DistanceSquared(points[current], points[candidate]);

                    if (distance < best)
                    {
                        best = distance;
                        next = candidate;
                    }
                }

                if (next < 0)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (used[i])
                            continue;

                        double distance = Vector2.DistanceSquared(points[current], points[i]);

                        if (distance < best)
                        {
                            best = distance;
                            next = i;
                        }
                    }
                }

                if (next < 0)
                    break;

                current = next;
            }

            return route;
        }

        private static (int X, int Y) hilbertD2xy(int side, int distance)
        {
            int x = 0;
            int y = 0;
            int t = distance;

            for (int s = 1; s < side; s *= 2)
            {
                int rx = 1 & (t / 2);
                int ry = 1 & (t ^ rx);
                rotateHilbertQuadrant(s, ref x, ref y, rx, ry);
                x += s * rx;
                y += s * ry;
                t /= 4;
            }

            return (x, y);
        }

        private static void rotateHilbertQuadrant(int side, ref int x, ref int y, int rx, int ry)
        {
            if (ry != 0)
                return;

            if (rx == 1)
            {
                x = side - 1 - x;
                y = side - 1 - y;
            }

            (x, y) = (y, x);
        }

        private static List<Vector2> resample(IReadOnlyList<Vector2> source, int samples)
        {
            if (source.Count <= 1)
                return source.ToList();

            var distances = new double[source.Count];
            double total = 0;

            for (int i = 1; i < source.Count; i++)
            {
                total += Vector2.Distance(source[i - 1], source[i]);
                distances[i] = total;
            }

            if (total <= 0.0001)
                return source.ToList();

            var result = new List<Vector2>(samples);
            int segment = 1;

            for (int i = 0; i < samples; i++)
            {
                double target = total * i / Math.Max(1, samples - 1);

                while (segment < distances.Length - 1 && distances[segment] < target)
                    segment++;

                double previousDistance = distances[segment - 1];
                double segmentLength = Math.Max(0.0001, distances[segment] - previousDistance);
                float amount = (float)((target - previousDistance) / segmentLength);
                result.Add(Vector2.Lerp(source[segment - 1], source[segment], amount));
            }

            return result;
        }

        private static List<Vector2> normaliseAndRotate(IReadOnlyList<Vector2> source, float width, float height, double pathAngle)
        {
            var finite = source.Where(point => float.IsFinite(point.X) && float.IsFinite(point.Y)).ToList();

            if (finite.Count < 2)
                return new List<Vector2>
                {
                    rotateToPlayfield(new Vector2(-width / 2, 0), pathAngle),
                    rotateToPlayfield(new Vector2(width / 2, 0), pathAngle)
                };

            float minX = finite.Min(p => p.X);
            float maxX = finite.Max(p => p.X);
            float minY = finite.Min(p => p.Y);
            float maxY = finite.Max(p => p.Y);
            float rangeX = Math.Max(0.0001f, maxX - minX);
            float rangeY = Math.Max(0.0001f, maxY - minY);
            var result = new List<Vector2>(finite.Count);

            foreach (Vector2 point in finite)
            {
                var local = new Vector2(
                    ((point.X - minX) / rangeX - 0.5f) * width,
                    ((point.Y - minY) / rangeY - 0.5f) * height);
                result.Add(rotateToPlayfield(local, pathAngle));
            }

            return result;
        }

        private static Vector2 rotateToPlayfield(Vector2 local, double pathAngle)
        {
            double radians = pathAngle * Math.PI / 180;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            return new Vector2(
                256 + (float)(local.X * cos - local.Y * sin),
                192 + (float)(local.X * sin + local.Y * cos));
        }

        private static List<Vector2> smooth(List<Vector2> points, int passes)
        {
            if (points.Count < 3 || passes <= 0)
                return points;

            var current = points;

            for (int pass = 0; pass < passes; pass++)
            {
                var next = new List<Vector2>(current.Count) { current[0] };

                for (int i = 1; i < current.Count - 1; i++)
                    next.Add(current[i - 1] * 0.25f + current[i] * 0.5f + current[i + 1] * 0.25f);

                next.Add(current[^1]);
                current = next;
            }

            return current;
        }

        private static List<Vector2> clean(IReadOnlyList<Vector2> source)
        {
            var result = new List<Vector2>(source.Count);

            foreach (Vector2 point in source)
            {
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                    continue;

                if (result.Count == 0 || Vector2.DistanceSquared(result[^1], point) > 0.0001f)
                    result.Add(point);
            }

            return result;
        }

        private static Vector2 curl(Vector2 point, double frequency, int octaves, double persistence, int seed)
        {
            const double epsilon = 0.75;
            double left = fractalNoise(point.X - epsilon, point.Y, frequency, octaves, persistence, seed);
            double right = fractalNoise(point.X + epsilon, point.Y, frequency, octaves, persistence, seed);
            double down = fractalNoise(point.X, point.Y - epsilon, frequency, octaves, persistence, seed);
            double up = fractalNoise(point.X, point.Y + epsilon, frequency, octaves, persistence, seed);
            var vector = new Vector2((float)(up - down), (float)(-(right - left)));

            if (vector.LengthSquared <= 0.000001f)
                return Vector2.Zero;

            vector.Normalize();
            return vector;
        }

        private static double fractalNoise(double x, double y, double frequency, int octaves, double persistence, int seed)
        {
            double value = 0;
            double amplitude = 1;
            double normaliser = 0;

            for (int octave = 0; octave < octaves; octave++)
            {
                value += valueNoise(x, y, frequency, seed + octave * 1013) * amplitude;
                normaliser += amplitude;
                frequency *= 2;
                amplitude *= persistence;
            }

            return normaliser <= 0 ? 0 : value / normaliser;
        }

        private static double valueNoise(double x, double y, double frequency, int seed)
        {
            double scale = frequency / 128;
            double sx = x * scale;
            double sy = y * scale;
            int x0 = (int)Math.Floor(sx);
            int y0 = (int)Math.Floor(sy);
            double tx = smoothStep(sx - x0);
            double ty = smoothStep(sy - y0);
            double a = hashNoise(x0, y0, seed);
            double b = hashNoise(x0 + 1, y0, seed);
            double c = hashNoise(x0, y0 + 1, seed);
            double d = hashNoise(x0 + 1, y0 + 1, seed);
            return lerp(lerp(a, b, tx), lerp(c, d, tx), ty);
        }

        private static double hashNoise(int x, int y, int seed)
        {
            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ x;
                hash = hash * 397 ^ y;
                hash ^= hash << 13;
                hash ^= hash >> 17;
                hash ^= hash << 5;
                return (hash & 0x7fffffff) / (double)0x3fffffff - 1;
            }
        }

        private static double smoothStep(double value) => value * value * (3 - 2 * value);

        private static double lerp(double start, double end, double amount) => start + (end - start) * amount;
    }
}
