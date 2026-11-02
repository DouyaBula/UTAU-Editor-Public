// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public sealed partial class SelectionBoxButton : SelectionBoxControl, IHasTooltip
    {
        private SpriteIcon icon = null!;

        private readonly IconUsage iconUsage;

        public Action? Action;

        public event Action? Clicked;

        public event Action? HoverLost;

        private readonly Bindable<float> controlButtonSize = new BindableFloat(20);

        public SelectionBoxButton(IconUsage iconUsage, string tooltip)
        {
            this.iconUsage = iconUsage;

            TooltipText = tooltip;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            // Original:
            // Size = new Vector2(20);
            AddInternal(icon = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Icon = iconUsage,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            config.BindWith(OsuSetting.EditorSelectionControlButtonSize, controlButtonSize);
            controlButtonSize.BindValueChanged(size => Size = new Vector2(size.NewValue), true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Clicked?.Invoke();

            TriggerAction();

            return true;
        }

        protected override void UpdateHoverState()
        {
            base.UpdateHoverState();
            // Original:
            // icon.FadeColour(!IsHeld && IsHovered ? Color4.White : Color4.Black, TRANSFORM_DURATION, Easing.OutQuint);
            icon.FadeColour(!IsHeld && IsHovered ? Color4.White : CurrentControlForegroundColour, TRANSFORM_DURATION, Easing.OutQuint);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            HoverLost?.Invoke();
        }

        public LocalisableString TooltipText { get; }

        public void TriggerAction()
        {
            Circle.FlashColour(Colours.GrayF, 300);

            TriggerOperationStarted();
            Action?.Invoke();
            TriggerOperationEnded();
        }
    }
}
