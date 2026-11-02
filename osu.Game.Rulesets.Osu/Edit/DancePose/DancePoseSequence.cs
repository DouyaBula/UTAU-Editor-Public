// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace osu.Game.Rulesets.Osu.Edit.DancePose
{
    /// <summary>
    /// Versioned interchange format shared by the external pose extractor and Slider Toolbox 3.
    /// Coordinates are normalised to the source image, with the origin at the top-left.
    /// </summary>
    public sealed class DancePoseSequence
    {
        public const string SCHEMA = "osu-pose-sequence";
        public const string SKELETON = "osu.simple-body.v1";
        public const string COORDINATE_SYSTEM = "normalized-top-left";
        public const int CURRENT_VERSION = 1;
        public const int MAX_FILE_SIZE_BYTES = 64 * 1024 * 1024;
        public const int MAX_FRAME_COUNT = 20_000;

        private static readonly string[] required_joints =
        {
            "head", "neck", "shoulderLeft", "shoulderRight", "elbowLeft", "elbowRight",
            "wristLeft", "wristRight", "hipLeft", "hipRight", "kneeLeft", "kneeRight",
            "ankleLeft", "ankleRight"
        };

        private static readonly JsonSerializerOptions json_options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            MaxDepth = 32
        };

        [JsonPropertyName("schema")]
        public string Schema { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("skeleton")]
        public string Skeleton { get; set; } = string.Empty;

        [JsonPropertyName("coordinateSystem")]
        public string CoordinateSystem { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public DancePoseSource Source { get; set; } = new DancePoseSource();

        [JsonPropertyName("frames")]
        public List<DancePoseFrame> Frames { get; set; } = new List<DancePoseFrame>();

        public static IReadOnlyList<string> RequiredJoints => required_joints;

        public static DancePoseSequence Read(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (stream.CanSeek && stream.Length > MAX_FILE_SIZE_BYTES)
                throw new InvalidDataException("Pose sequence file is larger than 64 MB.");

            DancePoseSequence sequence;

            try
            {
                sequence = JsonSerializer.Deserialize<DancePoseSequence>(stream, json_options)
                           ?? throw new InvalidDataException("The pose sequence file is empty.");
            }
            catch (JsonException e)
            {
                throw new InvalidDataException($"Invalid pose sequence JSON: {e.Message}", e);
            }

            sequence.normaliseJointNames();
            sequence.Validate();
            return sequence;
        }

        public void Validate()
        {
            if (!string.Equals(Schema, SCHEMA, StringComparison.Ordinal))
                throw new InvalidDataException($"Unsupported pose sequence schema '{Schema}'.");

            if (Version != CURRENT_VERSION)
                throw new InvalidDataException($"Pose sequence version {Version} is not supported (expected {CURRENT_VERSION}).");

            if (!string.Equals(Skeleton, SKELETON, StringComparison.Ordinal))
                throw new InvalidDataException($"Unsupported skeleton '{Skeleton}'. Expected '{SKELETON}'.");

            if (!string.Equals(CoordinateSystem, COORDINATE_SYSTEM, StringComparison.Ordinal))
                throw new InvalidDataException($"Unsupported coordinate system '{CoordinateSystem}'.");

            if (Source == null || Source.Width <= 0 || Source.Height <= 0 || !double.IsFinite(Source.FrameRate) || Source.FrameRate <= 0
                || !double.IsFinite(Source.DurationMs) || Source.DurationMs <= 0)
                throw new InvalidDataException("Pose sequence source metadata must include positive image dimensions, frame rate, and duration.");

            if (Frames == null || Frames.Count < 2 || Frames.Count > MAX_FRAME_COUNT)
                throw new InvalidDataException($"Pose sequence must contain between 2 and {MAX_FRAME_COUNT} frames.");

            double previousTimestamp = double.NegativeInfinity;

            foreach (DancePoseFrame? frame in Frames)
            {
                if (frame == null || !double.IsFinite(frame.TimestampMs) || frame.TimestampMs < 0 || frame.TimestampMs <= previousTimestamp)
                    throw new InvalidDataException("Frame timestamps must be finite, non-negative, and strictly increasing.");

                previousTimestamp = frame.TimestampMs;

                if (frame.Joints == null)
                    continue;

                foreach (KeyValuePair<string, DancePoseJoint> entry in frame.Joints)
                {
                    if (entry.Value == null)
                        continue;

                    if (!double.IsFinite(entry.Value.X) || !double.IsFinite(entry.Value.Y)
                        || !double.IsFinite(entry.Value.Confidence) || entry.Value.Confidence < 0 || entry.Value.Confidence > 1)
                        throw new InvalidDataException($"Joint '{entry.Key}' in frame at {frame.TimestampMs:0.##} ms has invalid coordinates or confidence.");
                }
            }

            if (Frames[^1].TimestampMs <= Frames[0].TimestampMs)
                throw new InvalidDataException("Pose sequence must span a positive duration.");
        }

        private void normaliseJointNames()
        {
            foreach (DancePoseFrame frame in Frames ?? new List<DancePoseFrame>())
            {
                if (frame?.Joints == null || frame.Joints.Comparer == StringComparer.OrdinalIgnoreCase)
                    continue;

                var normalised = new Dictionary<string, DancePoseJoint>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, DancePoseJoint> joint in frame.Joints)
                {
                    if (!normalised.TryAdd(joint.Key, joint.Value))
                        throw new InvalidDataException($"Frame at {frame.TimestampMs:0.##} ms contains duplicate joint names with different casing.");
                }

                frame.Joints = normalised;
            }
        }
    }

    public sealed class DancePoseSource
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("frameRate")]
        public double FrameRate { get; set; }

        [JsonPropertyName("durationMs")]
        public double DurationMs { get; set; }
    }

    public sealed class DancePoseFrame
    {
        [JsonPropertyName("timestampMs")]
        public double TimestampMs { get; set; }

        /// <summary>
        /// Missing or low-confidence joints are omitted by the extractor and interpolated by the importer.
        /// Empty dictionaries represent frames where the detector found no pose.
        /// </summary>
        [JsonPropertyName("joints")]
        public Dictionary<string, DancePoseJoint>? Joints { get; set; }
    }

    public sealed class DancePoseJoint
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; } = 1;
    }
}
