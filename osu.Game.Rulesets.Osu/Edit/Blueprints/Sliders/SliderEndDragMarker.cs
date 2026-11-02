// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderEndDragMarker : SmoothPath
    {
        public Action<DragStartEvent>? StartDrag { get; set; }
        public Action<DragEvent>? Drag { get; set; }
        public Action? EndDrag { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private readonly Bindable<string> tailArcColour = new Bindable<string>();
        private readonly Bindable<float> tailArcThickness = new BindableFloat(5);

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            var path = PathApproximator.CircularArcToPiecewiseLinear([
                new Vector2(0, OsuHitObject.OBJECT_RADIUS),
                new Vector2(OsuHitObject.OBJECT_RADIUS, 0),
                new Vector2(0, -OsuHitObject.OBJECT_RADIUS)
            ]);

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            // Original:
            // PathRadius = 5;
            PathRadius = tailArcThickness.Value;
            Vertices = path;

            config.BindWith(OsuSetting.EditorSliderTailArcColour, tailArcColour);
            config.BindWith(OsuSetting.EditorSliderTailArcThickness, tailArcThickness);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            tailArcColour.BindValueChanged(_ => updateState());
            tailArcThickness.BindValueChanged(thickness =>
            {
                // Original:
                // PathRadius = 5;
                PathRadius = thickness.NewValue;
            }, true);

            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            updateState();
            StartDrag?.Invoke(e);
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            updateState();
            base.OnDrag(e);
            Drag?.Invoke(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            updateState();
            EndDrag?.Invoke();
            base.OnDragEnd(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e) => e.Button == MouseButton.Left;

        protected override bool OnClick(ClickEvent e) => e.Button == MouseButton.Left;

        private void updateState()
        {
            // Original:
            // Colour = IsHovered || IsDragged ? colours.Red : colours.Yellow;
            var colour = EditorSelectionSettings.ParseColour(tailArcColour.Value, colours.Yellow);
            Colour = IsHovered || IsDragged ? colour.Lighten(1) : colour;
        }
    }
}
