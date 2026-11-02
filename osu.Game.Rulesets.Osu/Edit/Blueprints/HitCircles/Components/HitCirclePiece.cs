// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components
{
    public partial class HitCirclePiece : BlueprintPiece<HitCircle>
    {
        private readonly bool useSelectionOutlineSettings;
        private readonly RingPiece ring;
        private readonly Bindable<float> outlineThickness = new BindableFloat();
        private readonly Bindable<string> outlineColour = new Bindable<string>();

        public HitCirclePiece(bool useSelectionOutlineSettings = false)
        {
            this.useSelectionOutlineSettings = useSelectionOutlineSettings;

            Origin = Anchor.Centre;
            Size = OsuHitObject.OBJECT_DIMENSIONS;
            CornerRadius = Size.X / 2;
            CornerExponent = 2;

            // Original:
            // InternalChild = new RingPiece();

            InternalChild = ring = new RingPiece(innerOnly: useSelectionOutlineSettings);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            Colour = colours.Yellow;

            if (useSelectionOutlineSettings)
            {
                config.BindWith(OsuSetting.EditorSelectionOutlineThickness, outlineThickness);
                // Original:
                // config.BindWith(OsuSetting.EditorSelectionOutlineColour, outlineColour);
                config.BindWith(OsuSetting.EditorSelectionHitCircleOutlineColour, outlineColour);

                outlineThickness.BindValueChanged(v => ring.Thickness = v.NewValue, true);
                outlineColour.BindValueChanged(v =>
                {
                    // Original:
                    // Colour = colours.Yellow;
                    Colour = EditorSelectionSettings.ParseColour(v.NewValue, colours.Yellow);
                }, true);
            }
        }

        public override void UpdateFrom(HitCircle hitObject)
        {
            base.UpdateFrom(hitObject);

            Scale = new Vector2(hitObject.Scale);
        }
    }
}
