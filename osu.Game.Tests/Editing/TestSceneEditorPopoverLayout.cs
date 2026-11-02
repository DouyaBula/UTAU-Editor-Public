// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Curves;
using osu.Game.Rulesets.Osu.Edit.PatternGallery;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Editing
{
    [HeadlessTest]
    public partial class TestSceneEditorPopoverLayout : EditorClockTestScene
    {
        private PopoverContainer host = null!;
        private EditorToolButton opener = null!;
        private EditorPopover popover = null!;
        private EditorBeatmap beatmap = null!;
        private BeatmapEditorChangeHandler changeHandler = null!;

        protected override bool UseFreshStoragePerRun => true;
        protected override bool ScrollUsingMouseWheel => false;

        private void createPanel(Func<EditorPopover> create, float width = 600, float height = 360)
        {
            beatmap = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } });
            changeHandler = new BeatmapEditorChangeHandler(beatmap);
            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(EditorBeatmap), beatmap),
                    (typeof(IBeatSnapProvider), beatmap),
                    (typeof(IEditorChangeHandler), changeHandler),
                },
                Child = host = new PopoverContainer
                {
                    Size = new Vector2(width, height),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = opener = new EditorToolButton("Open", () => new Box(), () => popover = create())
                    {
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(150, 32),
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                    },
                },
            };
        }

        [TestCase("hitsound")]
        [TestCase("difficulty")]
        [TestCase("curves")]
        [TestCase("gallery")]
        [TestCase("detail")]
        [TestCase("offset")]
        [TestCase("zoom")]
        [TestCase("approach")]
        public void TestActualPanelsFitSmallEditor(string tool)
        {
            AddStep("create panel host", () => createPanel(() => tool switch
            {
                "hitsound" => new HitsoundToolsPopover(),
                "difficulty" => new DifficultyVisualizationPopover(new OsuHitObjectComposer(new OsuRuleset())),
                "curves" => new CurveToolsPopover(),
                "gallery" => new PatternGalleryPopover(),
                "detail" => new SliderDetailPopover(),
                "zoom" => new CanvasZoomPopover(),
                "approach" => new ApproachRateTogglePopover(),
                _ => new GlobalOffsetPopover(),
            }));
            AddStep("open panel", () => opener.TriggerClick());
            AddUntilStep("panel loaded", () => popover?.IsLoaded == true);
            AddUntilStep("panel fits host", fitsHost);
            AddStep("resize host", () => host.Size = new Vector2(400, 260));
            AddUntilStep("panel refits host", fitsHost);
            AddWaitStep("allow autosize and scrollbar padding to settle", 5);
            AddStep("scroll to bottom", () => popover.ChildrenOfType<OsuScrollContainer>().First().ScrollToEnd(false));
            AddWaitStep("settle scroll", 5);
            AddAssert("scroll reached bottom", () =>
            {
                var scroll = popover.ChildrenOfType<OsuScrollContainer>().First();
                Assert.That(scroll.Target, Is.EqualTo(scroll.ScrollableExtent).Within(1),
                    $"content={scroll.AvailableContent}, viewport={scroll.DisplayableContent}, child={scroll.Child.Size}, position={scroll.Child.Position}, count={popover.ChildrenOfType<OsuScrollContainer>().Count()}");
                return true;
            });
            AddAssert("still visible", () => popover.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestWidePanelBottomButtonRemainsClickable()
        {
            bool clicked = false;
            RoundedButton bottom = null!;
            AddStep("create wide long tool", () => createPanel(() => new EditorPopover(false)
            {
                Child = new FillFlowContainer
                {
                    Width = 980,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.X, Height = 900 },
                        bottom = new RoundedButton { RelativeSizeAxes = Axes.X, Height = 32, Text = "Apply", Action = () => clicked = true },
                    },
                },
            }));
            AddStep("open panel", () => opener.TriggerClick());
            AddUntilStep("panel fits host", fitsHost);
            AddStep("scroll to bottom", () => popover.ChildrenOfType<OsuScrollContainer>().Single().ScrollToEnd(false));
            AddUntilStep("bottom in view", () => popover.ChildrenOfType<OsuScrollContainer>().Single().IsScrolledToEnd());
            AddStep("click bottom button", () =>
            {
                InputManager.MoveMouseTo(bottom);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("button action ran", () => clicked);
            AddAssert("panel not dismissed", () => popover.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestFilePickerDoesNotDismissParent()
        {
            FormFileSelector selector = null!;
            var current = new Bindable<FileInfo?>();
            AddStep("create file tool", () => createPanel(() => new EditorPopover(false)
            {
                Child = new FillFlowContainer
                {
                    Width = 430,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        selector = new FormFileSelector(".mid") { Current = current, AllowClear = true, Caption = "MIDI" },
                    },
                },
            }));
            AddStep("open panel", () => opener.TriggerClick());
            AddUntilStep("panel fits host", fitsHost);
            AddAssert("initial null binding did not dismiss panel", () => popover.State.Value == Visibility.Visible);
            AddStep("open nested picker", () => selector.ShowPopover());
            AddUntilStep("picker loaded", () => popover.ChildrenOfType<FormFileSelector.FileChooserPopover>().Any(p => p.IsLoaded));
            AddAssert("tool is still current", () => host.CurrentPopover, () => Is.SameAs(popover));
            AddStep("select file", () => current.Value = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "test.mid")));
            AddAssert("parent remains open", () => popover.State.Value == Visibility.Visible);
            AddAssert("child picker closed", () => selector.FindClosestParent<PopoverContainer>()!.CurrentTarget, () => Is.Null);
            AddStep("clear file without picker", () => current.Value = null);
            AddAssert("parent still open", () => popover.State.Value == Visibility.Visible);
        }

        private bool fitsHost()
        {
            if (popover?.IsLoaded != true || popover.Width < 10 || popover.Height < 10)
                return false;

            var viewport = popover.ChildrenOfType<PopoverContainer>().First();
            var bounds = host.ToLocalSpace(viewport.ScreenSpaceDrawQuad).AABBFloat;
            return bounds.Left >= -0.1f && bounds.Top >= -0.1f && bounds.Right <= host.DrawWidth + 0.1f && bounds.Bottom <= host.DrawHeight + 0.1f;
        }

        [Test]
        public void TestSplitAndParameterRowsReflowOnNarrowScreens()
        {
            EditorToolSplitPane split = null!;
            EditorToolControlRow row = null!;
            AddStep("create responsive tool", () => createPanel(() => new EditorPopover(false)
            {
                Child = new FillFlowContainer
                {
                    Width = 900,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        split = new EditorToolSplitPane(200, 0.4f, new Box(), new Box()),
                        row = new EditorToolControlRow { Children = Enumerable.Range(0, 5).Select(_ => new Box { Height = 40 }).ToArray() },
                    },
                },
            }, 1000, 600));
            AddStep("open", () => opener.TriggerClick());
            AddUntilStep("wide layout", () => split?.IsLoaded == true && split.Height == 200);
            AddStep("shrink", () => host.Size = new Vector2(400, 300));
            AddUntilStep("stacked panes", () => split.Height == 408);
            AddUntilStep("controls wrapped with readable widths", () => row.Children.All(c => c.DrawWidth >= 150) && row.DrawHeight > 40);
            AddUntilStep("still in host", fitsHost);
        }

        [Test]
        public void TestPreviewFooterAlwaysVisibleAndCancelInvalidatesQueuedWork()
        {
            PreviewTestPanel preview = null!;
            string before = string.Empty;
            AddStep("create preview panel", () =>
            {
                createPanel(() => preview = new PreviewTestPanel(), 420, 260);
                beatmap.Breaks.Add(new ManualBreakPeriod(2000, 4000));
                before = changeHandler.CurrentStateHash;
            });
            AddStep("open preview", () => opener.TriggerClick());
            AddUntilStep("fits", fitsHost);
            AddAssert("preview inserted", () => beatmap.HitObjects.Count == 1);
            AddAssert("footer is not inside scroll", () => preview.Apply.FindClosestParent<OsuScrollContainer>() == null);
            AddStep("queue write then cancel", () =>
            {
                preview.QueuePreview();
                var cancel = preview.ChildrenOfType<RoundedButton>().Single(b => b.Text.ToString() == "Cancel");
                InputManager.MoveMouseTo(cancel);
                InputManager.Click(MouseButton.Left);
            });
            AddWaitStep("allow queued callback", 5);
            AddAssert("no delayed insertion", () => beatmap.HitObjects.Count == 0);
            AddAssert("transaction closed", () => !changeHandler.TransactionActive);
            AddAssert("break and map state restored", () => changeHandler.CurrentStateHash == before);
        }

        [Test]
        public void TestLongStatusWrapsInsteadOfClipping()
        {
            EditorToolStatus status = null!;
            AddStep("create status panel", () => createPanel(() => new EditorPopover(false)
            {
                Child = new FillFlowContainer
                {
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Child = status = new EditorToolStatus
                    {
                        Text = string.Join(" ", Enumerable.Repeat("The generated slider needs a lower velocity. Increase duration or reduce its size.", 6)),
                    },
                },
            }, 400, 260));
            AddStep("open", () => opener.TriggerClick());
            AddUntilStep("wrapped into multiple lines", () => status?.IsLoaded == true && status.DrawHeight > 60);
            AddUntilStep("fits host", fitsHost);
            AddAssert("no fixed status height", () => status.AutoSizeAxes == Axes.Y);
        }

        [Test]
        public void TestHitsoundTasksRemainSelectableAfterResize()
        {
            AddStep("create hitsound tool", () => createPanel(() => new HitsoundToolsPopover(), 430, 300));
            AddStep("open", () => opener.TriggerClick());
            AddUntilStep("loaded", fitsHost);
            AddStep("switch workflow", () => popover.ChildrenOfType<EditorToolPages>().First().Current.Value = "Import / merge");
            AddUntilStep("fits import page", fitsHost);
            AddStep("switch input", () => popover.ChildrenOfType<EditorToolPages>().Last().Current.Value = "MIDI notes");
            AddUntilStep("fits MIDI page", fitsHost);
            AddAssert("selector visible", () => popover.ChildrenOfType<FormFileSelector>().Single().IsPresent);
        }

        private partial class PreviewTestPanel : EditorPreviewPopover
        {
            [osu.Framework.Allocation.Resolved]
            private EditorBeatmap beatmap { get; set; } = null!;

            private readonly HitCircle circle = new HitCircle { StartTime = 3000 };
            public RoundedButton Apply { get; }

            public PreviewTestPanel()
                : base(false)
            {
                Child = new FillFlowContainer
                {
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.X, Height = 900 },
                        new EditorToolActionRow(Apply = new RoundedButton { Action = CommitPreview }, Hide),
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                apply();
            }

            public void QueuePreview() => Scheduler.AddOnce(apply);

            private void apply()
            {
                if (!BeginPreview()) return;
                beatmap.Remove(circle);
                beatmap.Add(circle);
                beatmap.Breaks.Clear();
            }

            protected override void RestorePreview() => beatmap.Remove(circle);
        }
    }
}
