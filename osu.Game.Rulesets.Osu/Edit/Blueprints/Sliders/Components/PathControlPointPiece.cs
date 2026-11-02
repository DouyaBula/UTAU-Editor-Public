// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A visualisation of a single <see cref="PathControlPoint"/> in an osu hit object with a path.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="OsuHitObject"/> which this <see cref="PathControlPointPiece{T}"/> visualises.</typeparam>
    public partial class PathControlPointPiece<T> : BlueprintPiece<T>, IHasTooltip
        where T : OsuHitObject, IHasPath
    {
        public Action<PathControlPointPiece<T>, MouseButtonEvent> RequestSelection;

        public Action<PathControlPoint> DragStarted;
        public Action<DragEvent> DragInProgress;
        public Action DragEnded;

        public readonly BindableBool IsSelected = new BindableBool();
        public readonly PathControlPoint ControlPoint;

        private readonly T hitObject;
        private readonly FastCircle inputCircle;
        private readonly FastCircle circle;
        private readonly Drawable markerRing;

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly Bindable<string> controlPointColour = new Bindable<string>();
        private readonly Bindable<string> controlPointInheritedColour = new Bindable<string>();
        private readonly Bindable<string> controlPointCatmullColour = new Bindable<string>();
        private readonly Bindable<string> controlPointBezierColour = new Bindable<string>();
        private readonly Bindable<string> controlPointLinearColour = new Bindable<string>();
        private readonly Bindable<string> controlPointPerfectCurveColour = new Bindable<string>();
        private readonly Bindable<string> controlPointBSplineDegree1Colour = new Bindable<string>();
        private readonly Bindable<string> controlPointBSplineDegree2Colour = new Bindable<string>();
        private readonly Bindable<string> controlPointBSplineDegree3Colour = new Bindable<string>();
        private readonly Bindable<string> controlPointBSplineDegree4Colour = new Bindable<string>();
        private readonly Bindable<float> controlPointSize = new BindableFloat(20);
        private readonly Bindable<float> controlPointHitAreaSize = new BindableFloat(28);

        private IBindable<Vector2> hitObjectPosition;
        private IBindable<float> hitObjectScale;
        private IBindable<int> stackHeight;

        public PathControlPointPiece(T hitObject, PathControlPoint controlPoint)
        {
            this.hitObject = hitObject;
            ControlPoint = controlPoint;

            controlPoint.Changed += updateMarkerDisplay;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                inputCircle = new FastCircle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Size = new Vector2(28),
                    AlwaysPresent = true,
                },
                circle = new FastCircle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20),
                },
                markerRing = new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(28),
                    Alpha = 0,
                    InnerRadius = 0.1f,
                    Progress = 1
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.EditorSliderControlPointColour, controlPointColour);
            config.BindWith(OsuSetting.EditorSliderControlPointInheritedColour, controlPointInheritedColour);
            config.BindWith(OsuSetting.EditorSliderControlPointCatmullColour, controlPointCatmullColour);
            config.BindWith(OsuSetting.EditorSliderControlPointBezierColour, controlPointBezierColour);
            config.BindWith(OsuSetting.EditorSliderControlPointLinearColour, controlPointLinearColour);
            config.BindWith(OsuSetting.EditorSliderControlPointPerfectCurveColour, controlPointPerfectCurveColour);
            config.BindWith(OsuSetting.EditorSliderControlPointBSplineDegree1Colour, controlPointBSplineDegree1Colour);
            config.BindWith(OsuSetting.EditorSliderControlPointBSplineDegree2Colour, controlPointBSplineDegree2Colour);
            config.BindWith(OsuSetting.EditorSliderControlPointBSplineDegree3Colour, controlPointBSplineDegree3Colour);
            config.BindWith(OsuSetting.EditorSliderControlPointBSplineDegree4Colour, controlPointBSplineDegree4Colour);
            config.BindWith(OsuSetting.EditorSliderControlPointSize, controlPointSize);
            config.BindWith(OsuSetting.EditorSliderControlPointHitAreaSize, controlPointHitAreaSize);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            hitObjectPosition = hitObject.PositionBindable.GetBoundCopy();
            hitObjectPosition.BindValueChanged(_ => updateMarkerDisplay());

            hitObjectScale = hitObject.ScaleBindable.GetBoundCopy();
            hitObjectScale.BindValueChanged(_ => updateMarkerDisplay());

            stackHeight = hitObject.StackHeightBindable.GetBoundCopy();
            stackHeight.BindValueChanged(_ => updateMarkerDisplay());

            IsSelected.BindValueChanged(_ => updateMarkerDisplay());
            controlPointColour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointInheritedColour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointCatmullColour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointBezierColour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointLinearColour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointPerfectCurveColour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointBSplineDegree1Colour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointBSplineDegree2Colour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointBSplineDegree3Colour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointBSplineDegree4Colour.BindValueChanged(_ => updateMarkerDisplay());
            controlPointSize.BindValueChanged(_ => updateMarkerDisplay());
            controlPointHitAreaSize.BindValueChanged(_ => updateMarkerDisplay());

            updateMarkerDisplay();
        }

        // The connecting path is excluded from positional input
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            // Original:
            // return circle.ReceivePositionalInputAt(screenSpacePos);
            return inputCircle.ReceivePositionalInputAt(screenSpacePos);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateMarkerDisplay();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateMarkerDisplay();
        }

        // Used to pair up mouse down/drag events with their corresponding mouse up events,
        // to avoid deselecting the piece by accident when the mouse up corresponding to the mouse down/drag fires.
        private bool keepSelection;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (RequestSelection == null)
                return false;

            switch (e.Button)
            {
                case MouseButton.Left:
                    // if control is pressed, do not do anything as the user may be adding to current selection
                    // or dragging all currently selected control points.
                    // if it isn't and the user's intent is to deselect, deselection will happen on mouse up.
                    if (e.ControlPressed && IsSelected.Value)
                        return true;

                    RequestSelection.Invoke(this, e);
                    keepSelection = true;

                    return true;

                case MouseButton.Right:
                    if (!IsSelected.Value)
                        RequestSelection.Invoke(this, e);

                    keepSelection = true;
                    return false; // Allow context menu to show
            }

            return false;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            // ctrl+click deselects this piece, but only if this event
            // wasn't immediately preceded by a matching mouse down or drag.
            if (IsSelected.Value && e.ControlPressed && !keepSelection)
                IsSelected.Value = false;

            keepSelection = false;
        }

        protected override bool OnClick(ClickEvent e) => RequestSelection != null;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (RequestSelection == null)
                return false;

            if (e.Button == MouseButton.Left)
            {
                DragStarted?.Invoke(ControlPoint);
                keepSelection = true;
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e) => DragInProgress?.Invoke(e);

        protected override void OnDragEnd(DragEndEvent e) => DragEnded?.Invoke();

        /// <summary>
        /// Updates the state of the circular control point marker.
        /// </summary>
        private void updateMarkerDisplay()
        {
            Position = hitObject.StackedPosition + ControlPoint.Position;

            circle.Size = new Vector2(controlPointSize.Value);
            inputCircle.Size = new Vector2(Math.Max(controlPointSize.Value, controlPointHitAreaSize.Value));
            markerRing.Size = new Vector2(controlPointSize.Value + 8);
            markerRing.Alpha = IsSelected.Value ? 1 : 0;

            // Original:
            // Color4 colour = getColourFromNodeType();
            Color4 colour = getConfiguredColourFromNodeType();

            if (IsHovered || IsSelected.Value)
                colour = colour.Lighten(1);

            Colour = colour;
            Scale = new Vector2(hitObject.Scale);
        }

        private Color4 getConfiguredColourFromNodeType()
        {
            if (ControlPoint.Type is not PathType pathType)
            {
                // Original:
                // return EditorSelectionSettings.ParseColour(controlPointColour.Value, colours.Yellow);
                return EditorSelectionSettings.ParseColour(controlPointInheritedColour.Value, colours.Yellow);
            }

            switch (pathType.Type)
            {
                case SplineType.Catmull:
                    return EditorSelectionSettings.ParseColour(controlPointCatmullColour.Value, colours.SeaFoam);

                case SplineType.BSpline:
                    if (!pathType.Degree.HasValue)
                        return EditorSelectionSettings.ParseColour(controlPointBezierColour.Value, colours.PinkLighter);

                    if (pathType.Degree.Value <= 1)
                        return EditorSelectionSettings.ParseColour(controlPointBSplineDegree1Colour.Value, colours.PinkDark);

                    if (pathType.Degree.Value == 2)
                        return EditorSelectionSettings.ParseColour(controlPointBSplineDegree2Colour.Value, colours.Pink);

                    if (pathType.Degree.Value == 3)
                        return EditorSelectionSettings.ParseColour(controlPointBSplineDegree3Colour.Value, colours.PinkLight);

                    return EditorSelectionSettings.ParseColour(controlPointBSplineDegree4Colour.Value, colours.PinkLight);

                case SplineType.PerfectCurve:
                    return EditorSelectionSettings.ParseColour(controlPointPerfectCurveColour.Value, colours.PurpleDark);

                case SplineType.Linear:
                default:
                    return EditorSelectionSettings.ParseColour(controlPointLinearColour.Value, colours.Red);
            }
        }

        private Color4 getColourFromNodeType()
        {
            if (ControlPoint.Type is not PathType pathType)
                return colours.Yellow;

            switch (pathType.Type)
            {
                case SplineType.Catmull:
                    return colours.SeaFoam;

                case SplineType.BSpline:
                    if (!pathType.Degree.HasValue)
                        return colours.PinkLighter;

                    int idx = Math.Clamp(pathType.Degree.Value, 0, 3);
                    return new[] { colours.PinkDarker, colours.PinkDark, colours.Pink, colours.PinkLight }[idx];

                case SplineType.PerfectCurve:
                    return colours.PurpleDark;

                default:
                    return colours.Red;
            }
        }

        public LocalisableString TooltipText => ControlPoint.Type?.Description ?? string.Empty;
    }
}
