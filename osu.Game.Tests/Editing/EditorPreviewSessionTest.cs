// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class EditorPreviewSessionTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void CloseBalancesOnceAndRejectsQueuedWrites(bool commit)
        {
            using var beatmap = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } });
            using var handler = new BeatmapEditorChangeHandler(beatmap);
            var preview = new EditorPreviewSession();
            var circle = new HitCircle { StartTime = 1000 };
            Assert.That(preview.TryBegin(handler), Is.True);
            Assert.That(preview.TryBegin(handler), Is.True);
            beatmap.Add(circle);
            if (commit)
                preview.Commit();
            else
                preview.Cancel(() =>
                {
                    beatmap.Remove(circle);
                    preview.Cancel(() => throw new Exception("Must not re-enter cancellation"));
                });

            preview.Commit();
            preview.Cancel(() => throw new Exception("Already closed"));
            Assert.That(handler.TransactionActive, Is.False);
            Assert.That(preview.TryBegin(handler), Is.False);
            Assert.That(beatmap.HitObjects.Count, Is.EqualTo(commit ? 1 : 0));
            if (commit)
            {
                handler.RestoreState(-1);
                Assert.That(beatmap.HitObjects, Is.Empty);
            }
        }

        [Test]
        public void CancellationFailureStillClosesTransaction()
        {
            using var beatmap = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } });
            using var handler = new BeatmapEditorChangeHandler(beatmap);
            var preview = new EditorPreviewSession();
            preview.TryBegin(handler);
            Assert.Throws<InvalidOperationException>(() => preview.Cancel(() => throw new InvalidOperationException()));
            Assert.That(handler.TransactionActive, Is.False);
            Assert.That(preview.TryBegin(handler), Is.False);
        }

        [Test]
        public void ClosingWithoutPreviewDoesNotRestoreOrStartTransaction()
        {
            var preview = new EditorPreviewSession();
            preview.Cancel(() => throw new Exception("Nothing to restore"));
            Assert.That(preview.TryBegin(null), Is.False);
        }
    }
}
