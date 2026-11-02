// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Edit.PatternGallery;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public class PatternGalleryTest
    {
        [Test]
        public void DensePreviewHasBoundedDrawableCount()
        {
            var sliders = Enumerable.Range(0, 200).Select(i => (HitObject)new Slider
            {
                Position = new Vector2(i % 20 * 20, i / 20 * 20),
                RepeatCount = 100,
                Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(10, 10) })
            }).ToArray();
            using var preview = new PatternGalleryPreview();
            preview.SetObjects(sliders);
            Assert.That(preview.ChildrenOfType<osu.Framework.Graphics.Shapes.Circle>().Count(), Is.LessThanOrEqualTo(29 * 8));
            Assert.That(preview.ChildrenOfType<osu.Framework.Graphics.Lines.SmoothPath>().Count(), Is.InRange(1, 58));
        }

        [Test]
        public void CaptureRoundTripsNativeObjects()
        {
            var circle = new HitCircle
            {
                Position = new Vector2(100, 120),
                StartTime = 1000
            };
            var slider = new Slider
            {
                Position = new Vector2(240, 160),
                StartTime = 1250,
                Path = new SliderPath(PathType.BEZIER, new[]
                {
                    Vector2.Zero,
                    new Vector2(80, 50),
                    new Vector2(160, 0)
                })
            };
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 4 });

            var entry = PatternGalleryOperations.Capture(new HitObject[] { slider, circle }, "Tech ending", "tech, opener");
            var restored = PatternGalleryOperations.Deserialize(entry);

            Assert.That(entry.Name, Is.EqualTo("Tech ending"));
            Assert.That(entry.Tags, Is.EqualTo("tech, opener"));
            Assert.That(entry.ObjectCount, Is.EqualTo(2));
            Assert.That(restored, Has.Count.EqualTo(2));
            Assert.That(restored[0], Is.TypeOf<HitCircle>());
            Assert.That(restored[1], Is.TypeOf<Slider>());
            Assert.That(((Slider)restored[1]).Path.ControlPoints, Has.Count.EqualTo(3));
        }

        [Test]
        public void PrepareForInsertionShiftsOnlyRelativeTime()
        {
            var first = new HitCircle { Position = new Vector2(20, 30), StartTime = 1000 };
            var second = new HitCircle { Position = new Vector2(80, 90), StartTime = 1500 };
            var entry = PatternGalleryOperations.Capture(new HitObject[] { first, second }, "Two circles");

            var prepared = PatternGalleryOperations.PrepareForInsertion(entry, 5000);

            Assert.That(prepared[0].StartTime, Is.EqualTo(5000));
            Assert.That(prepared[1].StartTime, Is.EqualTo(5500));
            Assert.That(((OsuHitObject)prepared[0]).Position, Is.EqualTo(first.Position));
            Assert.That(first.StartTime, Is.EqualTo(1000));
            Assert.That(second.StartTime, Is.EqualTo(1500));
        }

        [Test]
        public void PrepareForInsertionPreservesBeatRelativePatternAcrossTimings()
        {
            var sourceBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo },
                Difficulty = new BeatmapDifficulty { SliderMultiplier = 1 }
            });
            sourceBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 600 });

            var firstSlider = new Slider
            {
                StartTime = 0,
                Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(300, 0) }),
                SliderVelocityMultiplier = 4
            };
            firstSlider.ApplyDefaults(sourceBeatmap.ControlPointInfo, sourceBeatmap.Difficulty);

            var circleA = new HitCircle { StartTime = 600 };
            var circleB = new HitCircle { StartTime = 750 };
            var lastSlider = new Slider
            {
                StartTime = 900,
                Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(300, 0) }),
                SliderVelocityMultiplier = 6
            };
            lastSlider.ApplyDefaults(sourceBeatmap.ControlPointInfo, sourceBeatmap.Difficulty);

            var entry = PatternGalleryOperations.Capture(
                new HitObject[] { firstSlider, circleA, circleB, lastSlider },
                "timed pattern",
                sourceBeatmap: sourceBeatmap);

            var destinationBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo },
                Difficulty = new BeatmapDifficulty { SliderMultiplier = 1 }
            });
            destinationBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 400 });

            var prepared = PatternGalleryOperations.PrepareForInsertion(entry, 2000, destinationBeatmap);
            var preparedSlider = (Slider)prepared[0];

            Assert.That(prepared.Select(h => h.StartTime), Is.EqualTo(new[] { 2000d, 2400d, 2500d, 2600d }).Within(0.001));
            Assert.That(preparedSlider.GetEndTime() - preparedSlider.StartTime, Is.EqualTo(300).Within(1));
            Assert.That(((Slider)prepared[3]).GetEndTime() - prepared[3].StartTime, Is.EqualTo(200).Within(1));
        }

        [Test]
        public void MalformedEntryDoesNotCrashGalleryOperations()
        {
            var entry = new PatternGalleryEntry
            {
                Name = "stale pattern",
                ObjectCount = 1,
                SerializedObjects = "not valid json"
            };

            Assert.That(PatternGalleryOperations.Deserialize(entry), Is.Empty);
            Assert.That(PatternGalleryOperations.PrepareForInsertion(entry, 4000), Is.Empty);
        }

        [Test]
        public void StoreCanExportAndMergeGalleryData()
        {
            using var sourceStorage = new TemporaryNativeStorage("pattern-gallery-export-source");
            var source = new PatternGalleryStore(sourceStorage);
            source.Add(PatternGalleryOperations.Capture(new HitObject[]
            {
                new HitCircle { StartTime = 1000, Position = new Vector2(100, 100) }
            }, "portable pattern"));

            string exportPath = source.ExportToFile();
            Assert.That(File.Exists(exportPath), Is.True);

            using var destinationStorage = new TemporaryNativeStorage("pattern-gallery-export-destination");
            var destination = new PatternGalleryStore(destinationStorage);
            Assert.That(destination.ImportFromFile(exportPath), Is.EqualTo(1));
            Assert.That(destination.Data.Patterns.Single().Name, Is.EqualTo("portable pattern"));
        }

        [TestCase("not json")]
        [TestCase("null")]
        [TestCase("{\"format_version\":999,\"patterns\":[]}")]
        public void UnreadableLibraryCannotBeOverwritten(string contents)
        {
            using var storage = new TemporaryNativeStorage("pattern-gallery-unreadable");
            var library = storage.GetStorageForDirectory("pattern-gallery");
            library.GetFullPath(".", true);
            using (var stream = library.GetStream("gallery.json", FileAccess.Write, FileMode.Create))
            using (var writer = new StreamWriter(stream))
                writer.Write(contents);

            var store = new PatternGalleryStore(storage);
            Assert.That(store.LoadError, Is.Not.Null);
            Assert.Throws<InvalidOperationException>(() => store.Add(PatternGalleryOperations.Capture(new[] { new HitCircle() }, "new")));
            Assert.That(store.Data.Patterns, Is.Empty);
            Assert.That(File.ReadAllText(library.GetFullPath("gallery.json")), Is.EqualTo(contents));
        }

        [Test]
        public void FailedWriteDoesNotChangeInMemoryLibrary()
        {
            using var storage = new TemporaryNativeStorage("pattern-gallery-failed-write");
            var store = new PatternGalleryStore(storage);
            var entry = PatternGalleryOperations.Capture(new[] { new HitCircle() }, "kept");
            store.Add(entry);
            // A directory at the destination makes atomic file replacement fail deterministically.
            string path = storage.GetFullPath("pattern-gallery/gallery.json");
            File.Move(path, path + ".backup");
            Directory.CreateDirectory(path);
            Assert.Catch(() => store.Remove(entry));
            Assert.That(store.Data.Patterns.Single(), Is.SameAs(entry));
            Assert.Catch(() => store.RecordUse(entry));
            Assert.That(entry.UseCount, Is.Zero);
        }

        [Test]
        public void ImpossibleBeatTimingIsRejectedWithoutChangingSourceOrDestination()
        {
            using var source = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }, Difficulty = new BeatmapDifficulty { SliderMultiplier = 1 } });
            source.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            var slider = new Slider { Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(300, 0) }), SliderVelocityMultiplier = 10 };
            slider.ApplyDefaults(source.ControlPointInfo, source.Difficulty);
            var entry = PatternGalleryOperations.Capture(new[] { slider }, "fast", sourceBeatmap: source);
            using var destination = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }, Difficulty = new BeatmapDifficulty { SliderMultiplier = 0.5f } });
            string original = entry.SerializedObjects;
            Assert.Throws<InvalidOperationException>(() => PatternGalleryOperations.PrepareForInsertion(entry, 1000, destination));
            Assert.That(destination.HitObjects, Is.Empty);
            Assert.That(entry.SerializedObjects, Is.EqualTo(original));
        }

        [Test]
        public void CaptureAndInsertionIntegrateAcrossMultipleTimingSections()
        {
            using var source = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }, Difficulty = new BeatmapDifficulty { SliderMultiplier = 1 } });
            source.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            source.ControlPointInfo.Add(1000, new TimingControlPoint { BeatLength = 250 });
            var slider = new Slider
            {
                StartTime = 750,
                RepeatCount = 1,
                Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(50, 0) }),
            };
            slider.ApplyDefaults(source.ControlPointInfo, source.Difficulty);
            var entry = PatternGalleryOperations.Capture(new HitObject[] { slider, new HitCircle { StartTime = 1375 } }, "BPM change", sourceBeatmap: source);
            Assert.That(entry.DurationBeats[0], Is.EqualTo(1.5).Within(1e-5));
            Assert.That(entry.RelativeStartBeats[1], Is.EqualTo(2).Within(1e-5));
            using var destination = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }, Difficulty = new BeatmapDifficulty { SliderMultiplier = 1 } });
            destination.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 400 });
            destination.ControlPointInfo.Add(2200, new TimingControlPoint { BeatLength = 800 });
            var objects = PatternGalleryOperations.PrepareForInsertion(entry, 2000, destination);
            Assert.That(objects[0].GetEndTime(), Is.EqualTo(3000).Within(0.01));
            Assert.That(objects[1].StartTime, Is.EqualTo(3400).Within(0.01));
            Assert.That(((Slider)objects[0]).RepeatCount, Is.EqualTo(1));
            Assert.That(((Slider)objects[0]).Path.Distance, Is.EqualTo(50));
        }

        [Test]
        public void PopoverBuildsWithFreshStorage()
        {
            using var storage = new TemporaryNativeStorage("pattern-gallery-popover-test");
            var slider = new Slider
            {
                Position = new Vector2(100, 100),
                StartTime = 1000,
                Path = new SliderPath(PathType.BEZIER, new[]
                {
                    Vector2.Zero,
                    new Vector2(80, 40),
                    new Vector2(160, 0)
                })
            };
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 4 });
            new PatternGalleryStore(storage).Add(PatternGalleryOperations.Capture(new HitObject[] { slider }, "preview"));

            var popover = new PatternGalleryPopover();
            var loader = typeof(PatternGalleryPopover).GetMethod("load", BindingFlags.Instance | BindingFlags.NonPublic);
            var filterField = typeof(PatternGalleryPopover).GetField("filter", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(loader, Is.Not.Null);
            Assert.That(filterField, Is.Not.Null);
            ((Bindable<string>)filterField!.GetValue(popover)!).Value = null!;
            Assert.DoesNotThrow(() => loader!.Invoke(popover, new object[] { storage }));
        }

        [Test]
        public void SnapToDestinationTimingResnapsObjectsAndSliderTail()
        {
            var editorBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            });
            editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });

            var circle = new HitCircle { StartTime = 1240 };
            var slider = new Slider
            {
                StartTime = 1260,
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(300, 0)
                })
            };
            var sourceTiming = new ControlPointInfo();
            sourceTiming.Add(0, new TimingControlPoint { BeatLength = 600 });
            slider.ApplyDefaults(sourceTiming, editorBeatmap.Difficulty);
            var objects = new List<HitObject> { circle, slider };

            PatternGalleryOperations.SnapToDestinationTiming(objects, editorBeatmap);

            Assert.That(circle.StartTime, Is.EqualTo(1000));
            Assert.That(slider.StartTime, Is.EqualTo(1500));

            // AddRange() applies destination defaults in production; emulate that here so the
            // assertion checks the actual resulting tail time rather than stale source velocity.
            slider.ApplyDefaults(editorBeatmap.ControlPointInfo, editorBeatmap.Difficulty);
            Assert.That(slider.SliderVelocityMultiplier, Is.Not.EqualTo(1));
            Assert.That(slider.EndTime, Is.EqualTo(editorBeatmap.SnapTime(slider.EndTime, slider.StartTime)).Within(1));
        }
    }
}
