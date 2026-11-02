// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Lines;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A visualisation of the lines between <see cref="PathControlPointPiece{T}"/>s.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="OsuHitObject"/> which this <see cref="PathControlPointConnection{T}"/> visualises.</typeparam>
    public partial class PathControlPointConnection<T> : SmoothPath where T : OsuHitObject, IHasPath
    {
        private readonly T hitObject;
        private readonly bool useRoughSampling;

        private IBindable<Vector2> hitObjectPosition;
        private IBindable<int> pathVersion;
        private IBindable<int> stackHeight;
        private readonly Bindable<string> controlLineColour = new Bindable<string>();
        private readonly Bindable<float> controlLineThickness = new BindableFloat(1);
        private Bindable<float> roughControlLineMaxPoints = null!;

        public PathControlPointConnection(T hitObject, bool useRoughSampling = false)
        {
            this.hitObject = hitObject;
            this.useRoughSampling = useRoughSampling;
            PathRadius = 1;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.EditorSliderControlLineColour, controlLineColour);
            config.BindWith(OsuSetting.EditorSliderControlLineThickness, controlLineThickness);
            roughControlLineMaxPoints = config.GetBindable<float>(OsuSetting.EditorSliderRoughControlLineMaxPoints);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            hitObjectPosition = hitObject.PositionBindable.GetBoundCopy();
            hitObjectPosition.BindValueChanged(_ => Scheduler.AddOnce(updateConnectingPath));

            pathVersion = hitObject.Path.Version.GetBoundCopy();
            pathVersion.BindValueChanged(_ => Scheduler.AddOnce(updateConnectingPath));

            stackHeight = hitObject.StackHeightBindable.GetBoundCopy();
            stackHeight.BindValueChanged(_ => updateConnectingPath());

            controlLineColour.BindValueChanged(_ => updateLineStyle(), true);
            controlLineThickness.BindValueChanged(_ => updateLineStyle(), true);
            roughControlLineMaxPoints.BindValueChanged(_ => Scheduler.AddOnce(updateConnectingPath));

            updateConnectingPath();
        }

        private void updateLineStyle()
        {
            // Original:
            // PathRadius = 1;
            PathRadius = controlLineThickness.Value;
            Colour = EditorSelectionSettings.ParseColour(controlLineColour.Value, Color4Extensions.FromHex("ffcc22"));
        }

        /// <summary>
        /// Updates the path connecting this control point to the next one.
        /// </summary>
        private void updateConnectingPath()
        {
            Position = hitObject.StackedPosition;

            ClearVertices();

            if (useRoughSampling)
                addSampledVertices();
            else
            {
                foreach (var controlPoint in hitObject.Path.ControlPoints)
                    AddVertex(controlPoint.Position);
            }

            OriginPosition = PositionInBoundingBox(Vector2.Zero);
        }

        private void addSampledVertices()
        {
            var controlPoints = hitObject.Path.ControlPoints;

            if (controlPoints.Count == 0)
                return;

            int maxPoints = Math.Max(2, (int)Math.Round(roughControlLineMaxPoints.Value));

            if (controlPoints.Count <= maxPoints)
            {
                foreach (var controlPoint in controlPoints)
                    AddVertex(controlPoint.Position);

                return;
            }

            int previousIndex = -1;

            for (int i = 0; i < maxPoints; i++)
            {
                int index = (int)Math.Round(i * (controlPoints.Count - 1) / (double)(maxPoints - 1));

                if (index == previousIndex)
                    continue;

                previousIndex = index;
                AddVertex(controlPoints[index].Position);
            }
        }
    }
}
