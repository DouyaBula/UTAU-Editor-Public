// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Edit.DancePose;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public class DancePoseSliderGeneratorTest
    {
        [Test]
        public void ImportReadsVersionedCanonicalSequence()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""
                {
                  "schema":"osu-pose-sequence",
                  "version":1,
                  "skeleton":"osu.simple-body.v1",
                  "coordinateSystem":"normalized-top-left",
                  "source":{"width":512,"height":384,"frameRate":30,"durationMs":1000},
                  "frames":[
                    {"timestampMs":0,"joints":{}},
                    {"timestampMs":1000,"joints":{}}
                  ]
                }
                """));

            DancePoseSequence sequence = DancePoseSequence.Read(stream);

            Assert.That(sequence.Frames, Has.Count.EqualTo(2));
            Assert.That(sequence.Source.Width, Is.EqualTo(512));
        }

        [Test]
        public void RejectsUnsupportedSchemaAndNonMonotonicTimestamps()
        {
            var badSchema = makeSequence();
            badSchema.Schema = "some-other-format";
            Assert.Throws<InvalidDataException>(() => badSchema.Validate());

            var badTime = makeSequence();
            badTime.Frames[1].TimestampMs = badTime.Frames[0].TimestampMs;
            Assert.Throws<InvalidDataException>(() => badTime.Validate());
        }

        [Test]
        public void GeneratesOneBeatAlignedNativeSliderPerSample()
        {
            DancePoseGenerationResult result = DancePoseSliderGenerator.Generate(
                makeSequence(),
                startTime: 2000,
                targetBeats: 4,
                samplesPerBeat: 2,
                sliderMultiplier: 1,
                beatLengthAtTime: _ => 500,
                scale: 0.2);

            Assert.That(result.Sliders, Has.Count.EqualTo(8));
            Assert.That(result.Sliders[0].StartTime, Is.EqualTo(2000));
            Assert.That(result.Sliders[1].StartTime, Is.EqualTo(2250));
            Assert.That(result.Sliders[^1].StartTime, Is.EqualTo(3750));
            Assert.That(result.Duration, Is.EqualTo(2000).Within(0.001));
            Assert.That(result.Sliders.All(s => s.Path.CalculatedDistance > 0), Is.True);
            Assert.That(result.Sliders.All(s => s.SliderVelocityMultiplier is >= 0.1 and <= 10), Is.True);
        }

        [Test]
        public void FollowsTimingPointBeatLengthChangesDuringGeneration()
        {
            DancePoseGenerationResult result = DancePoseSliderGenerator.Generate(
                makeSequence(),
                startTime: 2000,
                targetBeats: 2,
                samplesPerBeat: 2,
                sliderMultiplier: 1,
                beatLengthAtTime: time => time < 2500 ? 500 : 1000,
                scale: 0.2);

            Assert.That(result.Sliders.Select(slider => slider.StartTime), Is.EqualTo(new[] { 2000, 2250, 2500, 3000 }));
            Assert.That(result.Duration, Is.EqualTo(1500).Within(0.001));
        }

        [Test]
        public void RejectsGenerationBeyondSafeSliderCountBeforeMultiplying()
        {
            ArgumentException? error = Assert.Throws<ArgumentException>(() => DancePoseSliderGenerator.Generate(
                makeSequence(), 0, int.MaxValue, 16, 1, _ => 500));

            Assert.That(error!.Message, Does.Contain("512 sliders"));
        }

        [Test]
        public void ReducesBranchRetracingInsteadOfDuplicatingEveryBone()
        {
            DancePoseGenerationResult result = DancePoseSliderGenerator.Generate(
                makeSequence(), 0, 1, 1, 1, _ => 500, scale: 0.5);

            var slider = result.Sliders[0];
            double sourceEdgeLength = slider.Path.CalculatedDistance;
            Assert.That(sourceEdgeLength, Is.LessThan(1000));
        }

        [Test]
        public void ReportsWhenSampleRateRequiresUnsupportedVelocity()
        {
            ArgumentException? error = Assert.Throws<ArgumentException>(() => DancePoseSliderGenerator.Generate(
                makeSequence(), 0, 4, 16, 1, _ => 500));

            Assert.That(error!.Message, Does.Contain("supported range"));
        }

        private static DancePoseSequence makeSequence()
        {
            return new DancePoseSequence
            {
                Schema = DancePoseSequence.SCHEMA,
                Version = DancePoseSequence.CURRENT_VERSION,
                Skeleton = DancePoseSequence.SKELETON,
                CoordinateSystem = DancePoseSequence.COORDINATE_SYSTEM,
                Source = new DancePoseSource
                {
                    Width = 512,
                    Height = 384,
                    FrameRate = 30,
                    DurationMs = 1000
                },
                Frames = new List<DancePoseFrame>
                {
                    new DancePoseFrame { TimestampMs = 0, Joints = createJoints(0.0) },
                    new DancePoseFrame { TimestampMs = 1000, Joints = createJoints(0.08) }
                }
            };
        }

        private static Dictionary<string, DancePoseJoint> createJoints(double armLift)
        {
            return new Dictionary<string, DancePoseJoint>(StringComparer.OrdinalIgnoreCase)
            {
                ["head"] = joint(0.5, 0.16),
                ["neck"] = joint(0.5, 0.29),
                ["shoulderLeft"] = joint(0.42, 0.30),
                ["shoulderRight"] = joint(0.58, 0.30),
                ["elbowLeft"] = joint(0.35, 0.39 - armLift),
                ["elbowRight"] = joint(0.65, 0.39 - armLift),
                ["wristLeft"] = joint(0.30, 0.49 - armLift),
                ["wristRight"] = joint(0.70, 0.49 - armLift),
                ["hipLeft"] = joint(0.46, 0.53),
                ["hipRight"] = joint(0.54, 0.53),
                ["kneeLeft"] = joint(0.44, 0.70),
                ["kneeRight"] = joint(0.56, 0.70),
                ["ankleLeft"] = joint(0.42, 0.90),
                ["ankleRight"] = joint(0.58, 0.90)
            };
        }

        private static DancePoseJoint joint(double x, double y) => new DancePoseJoint { X = x, Y = y, Confidence = 1 };
    }
}
