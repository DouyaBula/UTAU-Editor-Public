// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// Represents the base appearance for UI controls of the <see cref="SelectionBox"/>,
    /// such as scale handles, rotation handles, buttons, etc...
    /// </summary>
    public abstract partial class SelectionBoxControl : CompositeDrawable
    {
        public const double TRANSFORM_DURATION = 100;

        public event Action OperationStarted;
        public event Action OperationEnded;

        protected Circle Circle { get; private set; }

        /// <summary>
        /// Whether the user is currently holding the control with mouse.
        /// </summary>
        public bool IsHeld { get; private set; }

        [Resolved]
        protected OsuColour Colours { get; private set; }

        private readonly Bindable<string> controlColour = new Bindable<string>();

        protected Color4 CurrentControlColour => EditorSelectionSettings.ParseColour(controlColour.Value, Colours.YellowDark);

        protected Color4 CurrentControlForegroundColour => EditorSelectionSettings.ForegroundFor(CurrentControlColour);

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                Circle = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };

            config.BindWith(OsuSetting.EditorSelectionControlColour, controlColour);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlColour.BindValueChanged(_ => UpdateHoverState(), true);
            UpdateHoverState();
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateHoverState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateHoverState();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            IsHeld = true;
            UpdateHoverState();
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            IsHeld = false;
            UpdateHoverState();
        }

        protected virtual void UpdateHoverState()
        {
            if (IsHeld)
                Circle.FadeColour(Colours.GrayF, TRANSFORM_DURATION, Easing.OutQuint);
            else
            {
                // Original:
                // Circle.FadeColour(IsHovered ? Colours.Red : Colours.YellowDark, TRANSFORM_DURATION, Easing.OutQuint);
                Circle.FadeColour(IsHovered ? CurrentControlColour.Lighten(0.4f) : CurrentControlColour, TRANSFORM_DURATION, Easing.OutQuint);
            }

            this.ScaleTo(IsHeld || IsHovered ? 1.5f : 1, TRANSFORM_DURATION, Easing.OutQuint);
        }

        protected void TriggerOperationStarted() => OperationStarted?.Invoke();

        protected void TriggerOperationEnded() => OperationEnded?.Invoke();
    }
}
