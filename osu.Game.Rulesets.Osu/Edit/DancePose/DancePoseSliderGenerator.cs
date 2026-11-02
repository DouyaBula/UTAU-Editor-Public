// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.DancePose
{
    public enum DancePosePlacementMode
    {
        [System.ComponentModel.Description("Preserve detected movement")]
        PreserveMotion,

        [System.ComponentModel.Description("Stabilise body centre")]
        StabilizeCentre
    }

    public sealed class DancePoseGenerationResult
    {
        public IReadOnlyList<Slider> Sliders { get; }
        public double Duration { get; }
        public double MinimumVelocity { get; }
        public double MaximumVelocity { get; }
        public int InterpolatedJointSamples { get; }

        internal DancePoseGenerationResult(IReadOnlyList<Slider> sliders, double duration, double minimumVelocity, double maximumVelocity, int interpolatedJointSamples)
        {
            Sliders = sliders;
            Duration = duration;
            MinimumVelocity = minimumVelocity;
            MaximumVelocity = maximumVelocity;
            InterpolatedJointSamples = interpolatedJointSamples;
        }
    }

    /// <summary>
    /// Converts sampled poses into one native linear slider per pose. The bone graph is covered by
    /// a minimum-length open route (an undirected Chinese-postman/T-join problem), so branches are
    /// retraced only when required and are never connected by off-skeleton shortcuts.
    /// </summary>
    public static class DancePoseSliderGenerator
    {
        public const int MAX_SLIDERS = 512;
        public const double MIN_SLIDER_VELOCITY = 0.1;
        public const double MAX_SLIDER_VELOCITY = 10;

        private const int head_ring_segments = 8;
        private const double confidence_threshold = 0.15;
        private const float playfield_margin = 36;

        /// Uses the active beat length at each frame, allowing the sequence to follow timing changes in the beatmap.
        public static DancePoseGenerationResult Generate(
            DancePoseSequence sequence,
            double startTime,
            int targetBeats,
            int samplesPerBeat,
            double sliderMultiplier,
            Func<double, double> beatLengthAtTime,
            DancePosePlacementMode placementMode = DancePosePlacementMode.PreserveMotion,
            double scale = 1,
            double offsetX = 0,
            double offsetY = 0,
            IReadOnlyList<double>? sampleBoundaries = null,
            Func<double, double>? gapDurationAtTime = null)
        {
            ArgumentNullException.ThrowIfNull(sequence);

            sequence.Validate();

            ArgumentNullException.ThrowIfNull(beatLengthAtTime);

            if (!double.IsFinite(startTime) || targetBeats < 1 || samplesPerBeat < 1 || samplesPerBeat > 16)
                throw new ArgumentException("Start time, target beat count, or samples-per-beat setting is invalid.");

            if (targetBeats > MAX_SLIDERS / samplesPerBeat)
                throw new ArgumentException($"This sequence would create more than {MAX_SLIDERS} sliders. Reduce the duration or samples per beat.");

            int sampleCount = targetBeats * samplesPerBeat;
            if (sampleBoundaries != null)
            {
                sampleCount = sampleBoundaries.Count - 1;
                if (sampleCount < 1 || sampleCount > MAX_SLIDERS)
                    throw new ArgumentException("The selected range must produce between 1 and 512 slider frames.");
                for (int i = 0; i < sampleBoundaries.Count; i++)
                    if (!double.IsFinite(sampleBoundaries[i]) || (i > 0 && sampleBoundaries[i] <= sampleBoundaries[i - 1]))
                        throw new ArgumentException("Sample boundaries must be finite and strictly increasing.");
                startTime = sampleBoundaries[0];
            }

            if (!double.IsFinite(sliderMultiplier) || sliderMultiplier <= 0)
                throw new ArgumentException("Beatmap slider multiplier must be positive.");

            if (!double.IsFinite(scale) || scale <= 0 || !double.IsFinite(offsetX) || !double.IsFinite(offsetY))
                throw new ArgumentException("Scale and position offsets must be finite, and scale must be positive.");

            Dictionary<string, (double Timestamp, DancePoseJoint Joint)[]> jointTracks = buildJointTracks(sequence);
            var sampledPoses = new List<Dictionary<string, Vector2>>(sampleCount);
            int interpolatedJointSamples = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                double progress = sampleCount == 1 ? 0 : (double)i / (sampleCount - 1);
                double timestamp = sequence.Frames[0].TimestampMs + progress * (sequence.Frames[^1].TimestampMs - sequence.Frames[0].TimestampMs);
                Dictionary<string, Vector2> pose = samplePose(jointTracks, sequence.Source.Width, sequence.Source.Height, timestamp, ref interpolatedJointSamples);
                sampledPoses.Add(pose);
            }

            var positionedPoses = fitPoses(sampledPoses, placementMode, scale, offsetX, offsetY);
            var sliders = new List<Slider>(sampleCount);
            double currentTime = startTime;
            double minimumVelocity = double.PositiveInfinity;
            double maximumVelocity = double.NegativeInfinity;

            for (int i = 0; i < positionedPoses.Count; i++)
            {
                if (sampleBoundaries != null)
                    currentTime = sampleBoundaries[i];

                double beatLength = beatLengthAtTime(currentTime);
                double frameDuration = beatLength / samplesPerBeat;
                if (sampleBoundaries != null)
                {
                    currentTime = sampleBoundaries[i];
                    frameDuration = sampleBoundaries[i + 1] - currentTime;
                }
                double gapDuration = i < positionedPoses.Count - 1 ? gapDurationAtTime?.Invoke(currentTime) ?? 0 : 0;
                if (!double.IsFinite(gapDuration) || gapDuration < 0)
                    throw new ArgumentException("Slider gap must be finite and non-negative.");
                frameDuration -= gapDuration;

                if (!double.IsFinite(beatLength) || beatLength <= 0 || !double.IsFinite(frameDuration) || frameDuration <= 0)
                    throw new ArgumentException("The selected density leaves no duration after the slider gap. Reduce sample density or the gap.");

                Vector2[] route = createShortestSkeletonWalk(positionedPoses[i]);
                if (route.Length < 2)
                    throw new ArgumentException($"Pose {i + 1} produced an unusable slider path.");

                Vector2 headPosition = route[0];
                Vector2[] relativePoints = route.Select(p => p - headPosition).ToArray();
                var path = new SliderPath(PathType.LINEAR, relativePoints);
                path.ExpectedDistance.Value = path.CalculatedDistance;

                double velocity = path.CalculatedDistance * beatLength
                                  / (OsuHitObject.BASE_SCORING_DISTANCE * sliderMultiplier * frameDuration);

                minimumVelocity = Math.Min(minimumVelocity, velocity);
                maximumVelocity = Math.Max(maximumVelocity, velocity);

                if (!double.IsFinite(velocity) || velocity < MIN_SLIDER_VELOCITY || velocity > MAX_SLIDER_VELOCITY)
                    throw new ArgumentException($"Pose {i + 1} requires {velocity:0.###}x SV to fit one beat subdivision. Current supported range is {MIN_SLIDER_VELOCITY:0.0}x-{MAX_SLIDER_VELOCITY:0.0}x; reduce samples per beat or scale the figure.");

                var slider = new Slider
                {
                    Position = headPosition,
                    StartTime = currentTime,
                    SliderVelocityMultiplier = velocity,
                    Path = path
                };
                slider.Samples.Add(slider.CreateHitSampleInfo());
                sliders.Add(slider);

                currentTime += frameDuration + gapDuration;
            }

            return new DancePoseGenerationResult(sliders, currentTime - startTime, minimumVelocity, maximumVelocity, interpolatedJointSamples);
        }

        private static Dictionary<string, (double Timestamp, DancePoseJoint Joint)[]> buildJointTracks(DancePoseSequence sequence)
        {
            var tracks = new Dictionary<string, (double Timestamp, DancePoseJoint Joint)[]>(StringComparer.OrdinalIgnoreCase);

            foreach (string jointName in DancePoseSequence.RequiredJoints)
            {
                var track = new List<(double Timestamp, DancePoseJoint Joint)>();

                foreach (DancePoseFrame frame in sequence.Frames)
                {
                    DancePoseJoint? joint = getUsableJoint(frame, jointName);
                    if (joint != null)
                        track.Add((frame.TimestampMs, joint));
                }

                tracks[jointName] = track.ToArray();

                if (tracks[jointName].Length == 0)
                    throw new ArgumentException($"The pose data does not contain a usable '{jointName}' joint.");
            }

            return tracks;
        }

        private static Dictionary<string, Vector2> samplePose(
            IReadOnlyDictionary<string, (double Timestamp, DancePoseJoint Joint)[]> tracks,
            int imageWidth,
            int imageHeight,
            double timestamp,
            ref int interpolatedJointSamples)
        {
            var result = new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase);

            foreach (string jointName in DancePoseSequence.RequiredJoints)
            {
                (double BeforeTime, DancePoseJoint? Before, double AfterTime, DancePoseJoint? After) = findJointSamples(tracks[jointName], timestamp);
                DancePoseJoint? beforeJoint = Before;
                DancePoseJoint? afterJoint = After;

                if (beforeJoint == null && afterJoint == null)
                    throw new ArgumentException($"The pose data does not contain a usable '{jointName}' joint.");

                double x;
                double y;

                if (beforeJoint == null)
                {
                    x = afterJoint!.X;
                    y = afterJoint.Y;
                    interpolatedJointSamples++;
                }
                else if (afterJoint == null || beforeJoint == afterJoint)
                {
                    x = beforeJoint.X;
                    y = beforeJoint.Y;
                    if (afterJoint == null)
                        interpolatedJointSamples++;
                }
                else
                {
                    double amount = Math.Clamp((timestamp - BeforeTime) / (AfterTime - BeforeTime), 0, 1);
                    x = beforeJoint.X + (afterJoint.X - beforeJoint.X) * amount;
                    y = beforeJoint.Y + (afterJoint.Y - beforeJoint.Y) * amount;
                }

                result[jointName] = new Vector2((float)(x * imageWidth), (float)(y * imageHeight));
            }

            return result;
        }

        private static (double BeforeTime, DancePoseJoint? Before, double AfterTime, DancePoseJoint? After) findJointSamples(
            IReadOnlyList<(double Timestamp, DancePoseJoint Joint)> track,
            double timestamp)
        {
            int low = 0;
            int high = track.Count;

            while (low < high)
            {
                int middle = low + (high - low) / 2;
                if (track[middle].Timestamp < timestamp)
                    low = middle + 1;
                else
                    high = middle;
            }

            int afterIndex = low < track.Count ? low : -1;
            int beforeIndex = low < track.Count && track[low].Timestamp == timestamp ? low : low - 1;
            if (beforeIndex >= track.Count)
                beforeIndex = track.Count - 1;

            return (
                beforeIndex >= 0 ? track[beforeIndex].Timestamp : double.NaN,
                beforeIndex >= 0 ? track[beforeIndex].Joint : null,
                afterIndex >= 0 ? track[afterIndex].Timestamp : double.NaN,
                afterIndex >= 0 ? track[afterIndex].Joint : null);
        }

        private static DancePoseJoint? getUsableJoint(DancePoseFrame frame, string jointName)
        {
            if (frame.Joints == null || !frame.Joints.TryGetValue(jointName, out DancePoseJoint? joint) || joint.Confidence < confidence_threshold)
                return null;

            return joint;
        }

        private static List<Dictionary<string, Vector2>> fitPoses(
            IReadOnlyList<Dictionary<string, Vector2>> poses,
            DancePosePlacementMode placementMode,
            double scale,
            double offsetX,
            double offsetY)
        {
            var transformed = new List<Dictionary<string, Vector2>>(poses.Count);
            var unaligned = new List<Dictionary<string, Vector2>>(poses.Count);
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            foreach (Dictionary<string, Vector2> pose in poses)
            {
                Vector2 root = (pose["hipLeft"] + pose["hipRight"]) / 2;
                var points = new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase);

                foreach (KeyValuePair<string, Vector2> joint in pose)
                {
                    Vector2 point = placementMode == DancePosePlacementMode.StabilizeCentre ? joint.Value - root : joint.Value;
                    points[joint.Key] = point;
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);
                }

                unaligned.Add(points);
            }

            float boundsWidth = Math.Max(1, maxX - minX);
            float boundsHeight = Math.Max(1, maxY - minY);
            float availableWidth = OsuPlayfield.BASE_SIZE.X - playfield_margin * 2;
            float availableHeight = OsuPlayfield.BASE_SIZE.Y - playfield_margin * 2;
            float fitScale = (float)(Math.Min(availableWidth / boundsWidth, availableHeight / boundsHeight) * scale);
            var boundsCentre = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
            var playfieldCentre = OsuPlayfield.BASE_SIZE / 2 + new Vector2((float)offsetX, (float)offsetY);

            for (int i = 0; i < unaligned.Count; i++)
            {
                var positioned = new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase);

                foreach (KeyValuePair<string, Vector2> joint in unaligned[i])
                {
                    Vector2 point = placementMode == DancePosePlacementMode.StabilizeCentre
                        ? joint.Value * fitScale + playfieldCentre
                        : (joint.Value - boundsCentre) * fitScale + playfieldCentre;
                    positioned[joint.Key] = point;
                }

                transformed.Add(positioned);
            }

            return transformed;
        }

        private static Vector2[] createShortestSkeletonWalk(IReadOnlyDictionary<string, Vector2> pose)
        {
            var nodes = new List<Vector2>(head_ring_segments + 14);
            Vector2 head = pose["head"];
            Vector2 neck = pose["neck"];
            Vector2 shoulderCentre = (pose["shoulderLeft"] + pose["shoulderRight"]) / 2;
            float headRadius = Math.Max(Vector2.Distance(head, neck) * 0.62f, Vector2.Distance(pose["shoulderLeft"], pose["shoulderRight"]) * 0.1f);

            for (int i = 0; i < head_ring_segments; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI * 2 / head_ring_segments;
                nodes.Add(head + new Vector2((float)(Math.Cos(angle) * headRadius), (float)(Math.Sin(angle) * headRadius)));
            }

            int neckNode = nodes.Count;
            nodes.Add(neck);
            int shoulderLeft = nodes.Count;
            nodes.Add(pose["shoulderLeft"]);
            int elbowLeft = nodes.Count;
            nodes.Add(pose["elbowLeft"]);
            int wristLeft = nodes.Count;
            nodes.Add(pose["wristLeft"]);
            int shoulderRight = nodes.Count;
            nodes.Add(pose["shoulderRight"]);
            int elbowRight = nodes.Count;
            nodes.Add(pose["elbowRight"]);
            int wristRight = nodes.Count;
            nodes.Add(pose["wristRight"]);
            int torso = nodes.Count;
            nodes.Add(shoulderCentre + ((pose["hipLeft"] + pose["hipRight"]) / 2 - shoulderCentre) * 0.65f);
            int hipLeft = nodes.Count;
            nodes.Add(pose["hipLeft"]);
            int kneeLeft = nodes.Count;
            nodes.Add(pose["kneeLeft"]);
            int ankleLeft = nodes.Count;
            nodes.Add(pose["ankleLeft"]);
            int hipRight = nodes.Count;
            nodes.Add(pose["hipRight"]);
            int kneeRight = nodes.Count;
            nodes.Add(pose["kneeRight"]);
            int ankleRight = nodes.Count;
            nodes.Add(pose["ankleRight"]);

            var edges = new List<(int A, int B)>();

            for (int i = 0; i < head_ring_segments; i++)
                edges.Add((i, (i + 1) % head_ring_segments));

            // Ring index 4 is the bottom vertex (the ring starts at the top).
            edges.Add((4, neckNode));
            edges.Add((neckNode, shoulderLeft));
            edges.Add((shoulderLeft, elbowLeft));
            edges.Add((elbowLeft, wristLeft));
            edges.Add((neckNode, shoulderRight));
            edges.Add((shoulderRight, elbowRight));
            edges.Add((elbowRight, wristRight));
            edges.Add((neckNode, torso));
            edges.Add((torso, hipLeft));
            edges.Add((hipLeft, kneeLeft));
            edges.Add((kneeLeft, ankleLeft));
            edges.Add((torso, hipRight));
            edges.Add((hipRight, kneeRight));
            edges.Add((kneeRight, ankleRight));

            return findMinimumOpenPostmanRoute(nodes, edges).Select(node => nodes[node]).ToArray();
        }

        private static int[] findMinimumOpenPostmanRoute(IReadOnlyList<Vector2> nodes, IReadOnlyList<(int A, int B)> edges)
        {
            int count = nodes.Count;
            var distance = new double[count, count];
            var next = new int[count, count];
            var edgeIndexByPair = new Dictionary<(int, int), int>();

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    distance[i, j] = i == j ? 0 : double.PositiveInfinity;
                    next[i, j] = -1;
                }
            }

            for (int i = 0; i < edges.Count; i++)
            {
                (int a, int b) = edges[i];
                double length = Vector2.Distance(nodes[a], nodes[b]);
                distance[a, b] = distance[b, a] = length;
                next[a, b] = b;
                next[b, a] = a;
                edgeIndexByPair[orderedPair(a, b)] = i;
            }

            for (int k = 0; k < count; k++)
            {
                for (int i = 0; i < count; i++)
                {
                    if (double.IsPositiveInfinity(distance[i, k]))
                        continue;

                    for (int j = 0; j < count; j++)
                    {
                        double candidate = distance[i, k] + distance[k, j];
                        if (candidate < distance[i, j])
                        {
                            distance[i, j] = candidate;
                            next[i, j] = next[i, k];
                        }
                    }
                }
            }

            int[] degree = new int[count];
            foreach ((int a, int b) in edges)
            {
                degree[a]++;
                degree[b]++;
            }

            int[] oddNodes = Enumerable.Range(0, count).Where(n => (degree[n] & 1) != 0).ToArray();
            var duplicatedEdges = new List<(int A, int B)>();
            int routeStart = 0;

            if (oddNodes.Length > 0)
            {
                if (oddNodes.Length > 16)
                    throw new InvalidOperationException("The pose skeleton has too many branch endpoints to optimise safely.");

                double bestCost = double.PositiveInfinity;
                List<(int A, int B)> bestPairs = new List<(int A, int B)>();

                for (int firstEndpoint = 0; firstEndpoint < oddNodes.Length; firstEndpoint++)
                {
                    for (int secondEndpoint = firstEndpoint + 1; secondEndpoint < oddNodes.Length; secondEndpoint++)
                    {
                        int mask = (1 << oddNodes.Length) - 1;
                        mask &= ~(1 << firstEndpoint);
                        mask &= ~(1 << secondEndpoint);
                        var memo = new Dictionary<int, (double Cost, List<(int A, int B)> Pairs)>();
                        var matching = findMinimumPairing(mask, oddNodes, distance, memo);

                        if (matching.Cost < bestCost)
                        {
                            bestCost = matching.Cost;
                            bestPairs = matching.Pairs;
                            routeStart = oddNodes[firstEndpoint];
                        }
                    }
                }

                foreach ((int oddA, int oddB) in bestPairs)
                {
                    int current = oddA;
                    while (current != oddB)
                    {
                        int following = next[current, oddB];
                        if (following < 0 || !edgeIndexByPair.ContainsKey(orderedPair(current, following)))
                            throw new InvalidOperationException("Could not reconstruct a shortest branch-retrace route.");

                        duplicatedEdges.Add((current, following));
                        current = following;
                    }
                }
            }

            var augmentedEdges = edges.Concat(duplicatedEdges).ToArray();
            var adjacency = new List<(int Edge, int Other)>[count];
            for (int i = 0; i < count; i++)
                adjacency[i] = new List<(int Edge, int Other)>();

            for (int i = 0; i < augmentedEdges.Length; i++)
            {
                (int a, int b) = augmentedEdges[i];
                adjacency[a].Add((i, b));
                adjacency[b].Add((i, a));
            }

            bool[] used = new bool[augmentedEdges.Length];
            int[] offsets = new int[count];
            var stack = new Stack<int>();
            var reversedRoute = new List<int>(augmentedEdges.Length + 1);
            stack.Push(routeStart);

            while (stack.Count > 0)
            {
                int node = stack.Peek();
                List<(int Edge, int Other)> neighbours = adjacency[node];

                while (offsets[node] < neighbours.Count && used[neighbours[offsets[node]].Edge])
                    offsets[node]++;

                if (offsets[node] < neighbours.Count)
                {
                    (int edge, int other) = neighbours[offsets[node]++];
                    if (!used[edge])
                    {
                        used[edge] = true;
                        stack.Push(other);
                    }
                }
                else
                    reversedRoute.Add(stack.Pop());
            }

            reversedRoute.Reverse();

            if (used.Any(value => !value))
                throw new InvalidOperationException("Could not visit every skeleton bone in the generated slider route.");

            return reversedRoute.ToArray();
        }

        private static (double Cost, List<(int A, int B)> Pairs) findMinimumPairing(
            int mask,
            IReadOnlyList<int> oddNodes,
            double[,] distances,
            IDictionary<int, (double Cost, List<(int A, int B)> Pairs)> memo)
        {
            if (mask == 0)
                return (0, new List<(int A, int B)>());

            if (memo.TryGetValue(mask, out var cached))
                return cached;

            int firstIndex = 0;
            while ((mask & (1 << firstIndex)) == 0)
                firstIndex++;

            int remaining = mask & ~(1 << firstIndex);
            double bestCost = double.PositiveInfinity;
            List<(int A, int B)> bestPairs = new List<(int A, int B)>();

            for (int secondIndex = firstIndex + 1; secondIndex < oddNodes.Count; secondIndex++)
            {
                if ((remaining & (1 << secondIndex)) == 0)
                    continue;

                int nextMask = remaining & ~(1 << secondIndex);
                var tail = findMinimumPairing(nextMask, oddNodes, distances, memo);
                double cost = distances[oddNodes[firstIndex], oddNodes[secondIndex]] + tail.Cost;

                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestPairs = new List<(int A, int B)>(tail.Pairs) { (oddNodes[firstIndex], oddNodes[secondIndex]) };
                }
            }

            var result = (bestCost, bestPairs);
            memo[mask] = result;
            return result;
        }

        private static (int, int) orderedPair(int a, int b) => a < b ? (a, b) : (b, a);
    }
}
