// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public partial class SliderBodyPiece : BlueprintPiece<Slider>
    {
        private readonly ManualSliderBody body;

        /// <summary>
        /// Offset in absolute (local) coordinates from the start of the curve.
        /// </summary>
        public Vector2 PathStartLocation => body.PathOffset;

        /// <summary>
        /// Offset in absolute (local) coordinates from the end of the curve.
        /// </summary>
        public Vector2 PathEndLocation => body.PathEndOffset;

        public SliderBodyPiece()
        {
            AutoSizeAxes = Axes.Both;

            // SliderSelectionBlueprint relies on calling ReceivePositionalInputAt on this drawable to determine whether selection should occur.
            // Without AlwaysPresent, a movement in a parent container (ie. the editor composer area resizing) could cause incorrect input handling.
            AlwaysPresent = true;

            InternalChild = body = new ManualSliderBody
            {
                AccentColour = Color4.Transparent
            };
        }

        private readonly Bindable<float> outlineThickness = new BindableFloat(9);
        private readonly Bindable<string> outlineColour = new Bindable<string>();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            body.BorderColour = colours.Yellow;

            config.BindWith(OsuSetting.EditorSelectionOutlineThickness, outlineThickness);
            // Original:
            // config.BindWith(OsuSetting.EditorSelectionOutlineColour, outlineColour);
            config.BindWith(OsuSetting.EditorSelectionSliderBodyOutlineColour, outlineColour);

            outlineThickness.BindValueChanged(_ => updateBorderSize(), true);
            outlineColour.BindValueChanged(v =>
            {
                // Original:
                // body.BorderColour = colours.Yellow;
                body.BorderColour = EditorSelectionSettings.ParseColour(v.NewValue, colours.Yellow);
            }, true);
        }

        private void updateBorderSize()
        {
            if (body.PathRadius <= 0)
                return;

            // Original:
            // body.BorderSize = 1;

            body.BorderSize = Math.Clamp(
                outlineThickness.Value / (body.PathRadius * DrawableSliderPath.BORDER_PORTION),
                0,
                8);
        }

        private int? lastVersion;

        public override void UpdateFrom(Slider hitObject)
        {
            base.UpdateFrom(hitObject);

            body.PathRadius = hitObject.Scale * OsuHitObject.OBJECT_RADIUS;
            updateBorderSize();

            if (lastVersion != hitObject.Path.Version.Value)
            {
                lastVersion = hitObject.Path.Version.Value;
                body.SetVertices(hitObject.Path.CalculatedPath);
            }

            OriginPosition = body.PathOffset;
        }

        public void RecyclePath() => body.RecyclePath();

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => body.ReceivePositionalInputAt(screenSpacePos);
    }
}
