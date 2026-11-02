// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class CanvasZoomPopover : EditorPopover
    {
        private Bindable<float> canvasZoom = null!;
        private Bindable<float> zoomStep = null!;
        private Bindable<float> resetZoom = null!;

        [Resolved(CanBeNull = true)]
        private Editor? editor { get; set; }

        public CanvasZoomPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            canvasZoom = config.GetBindable<float>(OsuSetting.EditorCanvasZoom);
            zoomStep = config.GetBindable<float>(OsuSetting.EditorCanvasZoomStep);
            resetZoom = config.GetBindable<float>(OsuSetting.EditorCanvasZoomResetValue);

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            Child = new FillFlowContainer
            {
                Width = 300,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(20),
                Children = new Drawable[]
                {
                    new FormSliderBar<float>
                    {
                        Caption = EditorStrings.CanvasZoom,
                        Current = canvasZoom,
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true,
                        TabbableContentContainer = this,
                    },
                    new FormSliderBar<float>
                    {
                        Caption = EditorStrings.CanvasZoomStep,
                        Current = zoomStep,
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true,
                        TabbableContentContainer = this,
                    },
                    new FormSliderBar<float>
                    {
                        Caption = EditorStrings.CanvasZoomResetValue,
                        Current = resetZoom,
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true,
                        TabbableContentContainer = this,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(8),
                        Children = new Drawable[]
                        {
                            new RoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.31f,
                                Text = EditorStrings.CanvasZoomOut,
                                Action = () => canvasZoom.Value -= zoomStep.Value,
                            },
                            new RoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.31f,
                                Text = EditorStrings.Reset,
                                Action = () =>
                                {
                                    canvasZoom.Value = resetZoom.Value;
                                    if (editor != null)
                                        editor.CanvasPanOffset.Value = Vector2.Zero;
                                },
                            },
                            new RoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.31f,
                                Text = EditorStrings.CanvasZoomIn,
                                Action = () => canvasZoom.Value += zoomStep.Value,
                            },
                        }
                    }
                }
            };
        }
    }
}
