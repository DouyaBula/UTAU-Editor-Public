// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Edit.Curves;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>, IHasPopover, IKeyBindingHandler<GlobalAction>
    {
        protected new DrawableSlider DrawableObject => (DrawableSlider)base.DrawableObject;

        protected SliderBodyPiece BodyPiece { get; private set; } = null!;
        protected SliderCircleOverlay HeadOverlay { get; private set; } = null!;
        protected SliderCircleOverlay TailOverlay { get; private set; } = null!;

        protected PathControlPointVisualiser<Slider>? ControlPointVisualiser { get; private set; }
        private PathControlPointConnection<Slider>? roughControlPointConnection;
        private AnchorPathOverlay? anchorOverlay;
        private readonly Bindable<bool> anchorMode = new BindableBool();
        private readonly Bindable<bool> keepDuration = new BindableBool();

        [Resolved]
        private IDistanceSnapProvider? distanceSnapProvider { get; set; }

        [Resolved]
        private IPlacementHandler? placementHandler { get; set; }

        [Resolved]
        private EditorBeatmap? editorBeatmap { get; set; }

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved]
        private BindableBeatDivisor? beatDivisor { get; set; }

        [Resolved]
        private EditorClock? editorClock { get; set; }

        private PathControlPoint? placementControlPoint;

        public override Quad SelectionQuad
        {
            get
            {
                var result = BodyPiece.ScreenSpaceDrawQuad.AABBFloat;

                // Keep head/tail overlay bounds so selection handles and outlines are included in selection visuals.
                result = RectangleF.Union(result, HeadOverlay.VisibleQuad);
                result = RectangleF.Union(result, TailOverlay.VisibleQuad);

                if (ControlPointVisualiser != null)
                {
                    foreach (var piece in ControlPointVisualiser.Pieces)
                        result = RectangleF.Union(result, piece.ScreenSpaceDrawQuad.AABBFloat);
                }

                return result;
            }
        }

        private readonly BindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();
        private readonly BindableList<HitObject> selectedObjects = new BindableList<HitObject>();
        private readonly Bindable<bool> showHitMarkers = new Bindable<bool>();
        private readonly Bindable<bool> detailedControlPointMode = new BindableBool();
        private readonly Bindable<float> detailedControlPointLimit = new BindableFloat();
        private readonly Bindable<bool> controlPointSnapTargets = new BindableBool(true);
        private readonly Bindable<float> tangentGlitchDistance = new BindableFloat(12);
        private float? preConvertToStreamDrawableAlpha;
        private float? preConvertToStreamBlueprintAlpha;
        private Func<Popover>? createCurrentPopover;

        // Cached slider path which ignored the expected distance value.
        private readonly Cached<SliderPath> fullPathCache = new Cached<SliderPath>();

        private Vector2 lastRightClickPosition;

        public SliderSelectionBlueprint(Slider slider)
            : base(slider)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                classicSymmetryPreview = new ClassicSliderSymmetryPreview(),
                BodyPiece = new SliderBodyPiece(),
                HeadOverlay = CreateCircleOverlay(HitObject, SliderPosition.Start),
                TailOverlay = CreateCircleOverlay(HitObject, SliderPosition.End),
            };

            // tail will always have a non-null end drag marker.
            Debug.Assert(TailOverlay.EndDragMarker != null);

            TailOverlay.EndDragMarker.StartDrag += startAdjustingLength;
            TailOverlay.EndDragMarker.Drag += adjustLength;
            TailOverlay.EndDragMarker.EndDrag += endAdjustLength;

            config.BindWith(OsuSetting.EditorShowHitMarkers, showHitMarkers);
            config.BindWith(OsuSetting.EditorAnchorMode, anchorMode);
            config.BindWith(OsuSetting.EditorAnchorKeepDuration, keepDuration);
            loadClassicSymmetry(config);
            config.BindWith(OsuSetting.EditorSliderDetailedControlPointMode, detailedControlPointMode);
            config.BindWith(OsuSetting.EditorSliderDetailedControlPointLimit, detailedControlPointLimit);
            config.BindWith(OsuSetting.EditorSliderControlPointSnapTargets, controlPointSnapTargets);
            config.BindWith(OsuSetting.EditorSliderTangentGlitchDistance, tangentGlitchDistance);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(HitObject.Path.ControlPoints);
            controlPoints.CollectionChanged += (_, _) =>
            {
                fullPathCache.Invalidate();
                updateVisualDefinition();
            };

            pathVersion.BindTo(HitObject.Path.Version);
            pathVersion.BindValueChanged(_ => editorBeatmap?.Update(HitObject));

            BodyPiece.UpdateFrom(HitObject);

            if (editorBeatmap != null)
                selectedObjects.BindTo(editorBeatmap.SelectedHitObjects);
            selectedObjects.BindCollectionChanged((_, _) => updateVisualDefinition(), true);
            showHitMarkers.BindValueChanged(_ =>
            {
                if (!showHitMarkers.Value)
                    DrawableObject.RestoreHitAnimations();
            });
            detailedControlPointMode.BindValueChanged(_ => updateVisualDefinition());
            detailedControlPointLimit.BindValueChanged(_ => updateVisualDefinition());
            anchorMode.BindValueChanged(_ =>
            {
                updateVisualDefinition();
                updateClassicSymmetryPreview();
            });
        }

        public override bool HandleQuickDeletion()
        {
            if (anchorOverlay != null)
                return anchorOverlay.DeleteQuickTarget() || anchorOverlay.DeleteSelected();
            var hoveredControlPoint = ControlPointVisualiser?.Pieces.FirstOrDefault(p => p.IsHovered);

            if (hoveredControlPoint == null)
                return false;

            if (hoveredControlPoint.IsSelected.Value)
                ControlPointVisualiser?.DeleteSelected();
            else
                ControlPointVisualiser?.Delete([hoveredControlPoint.ControlPoint]);

            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (IsSelected)
                BodyPiece.UpdateFrom(HitObject);

            updateClassicSymmetryPreview();

            if (showHitMarkers.Value)
                DrawableObject.SuppressHitAnimations();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateVisualDefinition();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateVisualDefinition();
            base.OnHoverLost(e);
        }

        protected override void OnSelected()
        {
            updateVisualDefinition();
            base.OnSelected();
        }

        protected override void OnDeselected()
        {
            base.OnDeselected();

            if (placementControlPoint != null)
                endControlPointPlacement();

            updateVisualDefinition();
            BodyPiece.RecyclePath();
        }

        private void updateVisualDefinition()
        {
            if (anchorMode.Value && AnchorPath.TryRead(HitObject.Path, out _) && IsSelected && selectedObjects.Count < 2)
            {
                ControlPointVisualiser?.Expire();
                ControlPointVisualiser = null;
                roughControlPointConnection?.Expire();
                roughControlPointConnection = null;
                if (anchorOverlay == null)
                    AddInternal(anchorOverlay = new AnchorPathOverlay(HitObject));
                return;
            }
            anchorOverlay?.FinishInteraction();
            anchorOverlay?.Expire();
            anchorOverlay = null;
            // To reduce overhead of drawing these blueprints, only add extra detail when only this slider is selected.
            if (IsSelected && selectedObjects.Count < 2)
            {
                if (shouldUseDetailedControlPointVisualiser())
                {
                    roughControlPointConnection?.Expire();
                    roughControlPointConnection = null;

                    if (ControlPointVisualiser == null)
                    {
                        AddInternal(ControlPointVisualiser = new PathControlPointVisualiser<Slider>(HitObject, true)
                        {
                            RemoveControlPointsRequested = removeControlPoints,
                            SplitControlPointsRequested = splitControlPoints
                        });
                    }
                }
                else
                {
                    ControlPointVisualiser?.Expire();
                    ControlPointVisualiser = null;

                    if (roughControlPointConnection == null)
                        AddInternal(roughControlPointConnection = new PathControlPointConnection<Slider>(HitObject, true));
                }
            }
            else
            {
                ControlPointVisualiser?.Expire();
                ControlPointVisualiser = null;
                roughControlPointConnection?.Expire();
                roughControlPointConnection = null;
            }
        }

        private bool shouldUseDetailedControlPointVisualiser()
            => HitObject.Path.ControlPoints.Count <= (int)Math.Round(detailedControlPointLimit.Value)
               || detailedControlPointMode.Value;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    lastRightClickPosition = e.MouseDownPosition;
                    return false; // Allow right click to be handled by context menu

                case MouseButton.Left:

                    // If there's more than two objects selected, ctrl+click should deselect
                    if ((!anchorMode.Value || !AnchorPath.TryRead(HitObject.Path, out _)) && e.ControlPressed && IsSelected && selectedObjects.Count < 2)
                    {
                        changeHandler?.BeginChange();
                        placementControlPoint = addControlPoint(e.MousePosition);
                        if (placementControlPoint != null)
                            ControlPointVisualiser?.SetSelectionTo(placementControlPoint);
                        else
                            changeHandler?.EndChange();
                        return true; // Stop input from being handled and modifying the selection
                    }

                    break;
            }

            return false;
        }

        #region Length Adjustment (independent of path nodes)

        private Vector2 lengthAdjustMouseOffset;
        private double oldDuration;
        private double oldVelocityMultiplier;
        private double desiredDistance;
        private bool isAdjustingLength;
        private bool adjustVelocityMomentary;

        private void startAdjustingLength(DragStartEvent e)
        {
            isAdjustingLength = true;
            adjustVelocityMomentary = e.ShiftPressed;
            lengthAdjustMouseOffset = ToLocalSpace(e.ScreenSpaceMouseDownPosition) - HitObject.Position - HitObject.Path.PositionAt(1);
            oldDuration = HitObject.Path.Distance / HitObject.SliderVelocityMultiplier;
            oldVelocityMultiplier = HitObject.SliderVelocityMultiplier;
            changeHandler?.BeginChange();
        }

        private void endAdjustLength()
        {
            trimExcessControlPoints(HitObject.Path);
            changeHandler?.EndChange();
            isAdjustingLength = false;
        }

        private void adjustLength(MouseEvent e) => adjustLength(findClosestPathDistance(e), e.ShiftPressed);

        private void adjustLength(double proposedDistance, bool adjustVelocity)
        {
            desiredDistance = proposedDistance;
            double proposedVelocity = oldVelocityMultiplier;

            if (adjustVelocity)
            {
                proposedVelocity = proposedDistance / oldDuration;
                proposedDistance = Math.Clamp(proposedDistance, 0.1 * oldDuration, 10 * oldDuration);
            }
            else
            {
                double minDistance = distanceSnapProvider?.GetBeatSnapDistance() * oldVelocityMultiplier ?? 1;
                // do not allow the slider to extend beyond the path's calculated distance.
                // this can happen in two specific circumstances:
                // - floating point issues (`minDistance` is just ever so slightly larger than the calculated distance)
                // - the slider was placed with a higher beat snap active than the current one,
                //   therefore snapping it to the current beat snap distance would mean extrapolating it beyond its actual shape as defined by its control points
                minDistance = Math.Min(minDistance, HitObject.Path.CalculatedDistance);

                // Add a small amount to the proposed distance to make it easier to snap to the full length of the slider.
                proposedDistance = distanceSnapProvider?.FindSnappedDistance((float)proposedDistance + 1, HitObject.StartTime, HitObject) ?? proposedDistance;
                proposedDistance = Math.Clamp(proposedDistance, minDistance, HitObject.Path.CalculatedDistance);
            }

            if (Precision.AlmostEquals(proposedDistance, HitObject.Path.Distance) && Precision.AlmostEquals(proposedVelocity, HitObject.SliderVelocityMultiplier))
                return;

            HitObject.SliderVelocityMultiplier = proposedVelocity;
            HitObject.Path.ExpectedDistance.Value = proposedDistance;
            editorBeatmap?.Update(HitObject);
        }

        /// <summary>
        /// Trims control points from the end of the slider path which are not required to reach the expected end of the slider.
        /// </summary>
        /// <param name="sliderPath">The slider path to trim control points of.</param>
        private void trimExcessControlPoints(SliderPath sliderPath)
        {
            if (!sliderPath.ExpectedDistance.Value.HasValue)
                return;

            double[] segmentEnds = sliderPath.GetSegmentEnds().ToArray();
            int segmentIndex = 0;

            for (int i = 1; i < sliderPath.ControlPoints.Count - 1; i++)
            {
                if (!sliderPath.ControlPoints[i].Type.HasValue) continue;

                if (Precision.AlmostBigger(segmentEnds[segmentIndex], 1, 1E-3))
                {
                    sliderPath.ControlPoints.RemoveRange(i + 1, sliderPath.ControlPoints.Count - i - 1);
                    sliderPath.ControlPoints[^1].Type = null;
                    break;
                }

                segmentIndex++;
            }
        }

        /// <summary>
        /// Finds the expected distance value for which the slider end is closest to the mouse position.
        /// </summary>
        private double findClosestPathDistance(MouseEvent e)
        {
            const double step1 = 10;
            const double step2 = 0.1;
            const double longer_distance_bias = 0.01;

            var desiredPosition = ToLocalSpace(e.ScreenSpaceMousePosition) - HitObject.Position - lengthAdjustMouseOffset;

            if (!fullPathCache.IsValid)
                fullPathCache.Value = new SliderPath(HitObject.Path.ControlPoints.ToArray());

            // Do a linear search to find the closest point on the path to the mouse position.
            double bestValue = 0;
            double minDistance = double.MaxValue;

            for (double d = 0; d <= fullPathCache.Value.CalculatedDistance; d += step1)
            {
                double t = d / fullPathCache.Value.CalculatedDistance;
                double dist = Vector2.Distance(fullPathCache.Value.PositionAt(t), desiredPosition) - d * longer_distance_bias;

                if (dist >= minDistance) continue;

                minDistance = dist;
                bestValue = d;
            }

            // Do another linear search to fine-tune the result.
            double maxValue = Math.Min(bestValue + step1, fullPathCache.Value.CalculatedDistance);

            for (double d = bestValue - step1; d <= maxValue; d += step2)
            {
                double t = d / fullPathCache.Value.CalculatedDistance;
                double dist = Vector2.Distance(fullPathCache.Value.PositionAt(t), desiredPosition) - d * longer_distance_bias;

                if (dist >= minDistance) continue;

                minDistance = dist;
                bestValue = d;
            }

            return bestValue;
        }

        #endregion

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (placementControlPoint == null)
                return base.OnDragStart(e);

            ControlPointVisualiser?.DragStarted(placementControlPoint);
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            if (placementControlPoint != null)
                ControlPointVisualiser?.DragInProgress(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (placementControlPoint != null)
                endControlPointPlacement();
        }

        private void endControlPointPlacement()
        {
            if (IsDragged)
                ControlPointVisualiser?.DragEnded();

            placementControlPoint = null;
            changeHandler?.EndChange();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && IsSelected && e.AltPressed && !e.ControlPressed && !e.ShiftPressed && !e.SuperPressed && e.Key == Key.P)
            {
                anchorMode.Value = !anchorMode.Value;
                return true;
            }

            // The editor maps C to playback and Alt+arrows to timeline/other global actions.
            // A hovered selected slider is an active editing target, so let its local editor
            // own these keystrokes. Alt itself is intentionally left to the composer: it
            // momentarily enables distance snap while the whole slider is being moved.
            if (!e.Repeat && IsSelected && IsHovered)
            {
                if (!e.ControlPressed && !e.ShiftPressed && !e.AltPressed && !e.SuperPressed && e.Key == Key.C)
                    return true;

                if (e.AltPressed && !e.ControlPressed && e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
                    return true;
            }

            if (!IsSelected)
                return false;

            if (e.Key == Key.F && e.ControlPressed && e.ShiftPressed)
            {
                showConvertToStreamPopover();
                return true;
            }

            if (isAdjustingLength && e.ShiftPressed != adjustVelocityMomentary)
            {
                adjustVelocityMomentary = e.ShiftPressed;
                adjustLength(desiredDistance, adjustVelocityMomentary);
                return true;
            }

            return false;
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (!IsSelected || !isAdjustingLength || e.ShiftPressed == adjustVelocityMomentary) return;

            adjustVelocityMomentary = e.ShiftPressed;
            adjustLength(desiredDistance, adjustVelocityMomentary);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (!IsSelected || e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.EditorSliderInsertAndSplitAtCurrentTime:
                    splitCurrentTimeBySampling();
                    return true;

                case GlobalAction.EditorSliderAddTangentGlitchAtCurrentTime:
                    addCurrentTimeTangentGlitch();
                    return true;

                case GlobalAction.EditorSeekToPreviousBookmark:
                case GlobalAction.EditorSeekToNextBookmark:
                case GlobalAction.IncreaseVolume:
                case GlobalAction.DecreaseVolume:
                case GlobalAction.PreviousVolumeMeter:
                case GlobalAction.NextVolumeMeter:
                    // Keep Alt+arrows from seeking the timeline while a selected slider is
                    // under the pointer. The composer still receives the raw Alt key event and
                    // therefore keeps its momentary distance-snap behaviour.
                    return IsHovered;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private PathControlPoint? addControlPoint(Vector2 position)
        {
            position -= HitObject.Position;

            var pathControlPoint = insertControlPointAtPathPosition(position, null);

            return pathControlPoint;
        }

        private PathControlPoint? insertControlPointAtPathPosition(Vector2 position, PathType? pathType)
        {
            int insertionIndex = findControlPointInsertionIndex(position);

            PathControlPoint[] originalPoints = keepDuration.Value ? HitObject.Path.ControlPoints.ToArray() : Array.Empty<PathControlPoint>();
            Vector2[] originalPositions = originalPoints.Select(point => point.Position).ToArray();
            PathType?[] originalTypes = originalPoints.Select(point => point.Type).ToArray();
            double? originalExpectedDistance = HitObject.Path.ExpectedDistance.Value;
            double originalDistance = HitObject.Path.Distance;
            double originalVelocity = HitObject.SliderVelocityMultiplier;

            var pathControlPoint = new PathControlPoint
            {
                Position = position,
                Type = pathType
            };

            // Move the control points from the insertion index onwards to make room for the insertion
            controlPoints.Insert(insertionIndex, pathControlPoint);

            ControlPointVisualiser?.EnsureValidPathTypes();

            if (keepDuration.Value)
            {
                if (!SliderDurationPreserver.TryApply(HitObject, originalDistance, originalVelocity, editorBeatmap, out _)
                    || !HitObject.Path.HasValidLengthForPlacement)
                {
                    restoreInsertedControlPoint(originalPoints, originalPositions, originalTypes, originalExpectedDistance, originalVelocity);
                    return null;
                }
            }
            else
                HitObject.SnapTo(distanceSnapProvider);

            return pathControlPoint;
        }

        private void restoreInsertedControlPoint(PathControlPoint[] originalPoints, Vector2[] originalPositions, PathType?[] originalTypes,
                                                 double? originalExpectedDistance, double originalVelocity)
        {
            for (int i = 0; i < originalPoints.Length; i++)
            {
                originalPoints[i].Position = originalPositions[i];
                originalPoints[i].Type = originalTypes[i];
            }

            HitObject.Path.ControlPoints.ReplaceRange(0, HitObject.Path.ControlPoints.Count, originalPoints);
            HitObject.Path.ExpectedDistance.Value = originalExpectedDistance;
            HitObject.SliderVelocityMultiplier = originalVelocity;
            if (editorBeatmap != null)
                HitObject.UpdateVelocity(editorBeatmap.ControlPointInfo, editorBeatmap.Difficulty);
        }

        private bool tryGetCurrentSliderPathProgress(out double pathProgress)
            => tryGetCurrentSliderProgress(out _, out pathProgress);

        private bool tryGetCurrentSliderProgress(out double linearProgress, out double pathProgress)
        {
            linearProgress = 0;
            pathProgress = 0;

            if (editorClock == null || HitObject.Duration <= 0)
                return false;

            linearProgress = (editorClock.CurrentTime - HitObject.StartTime) / HitObject.Duration;

            if (linearProgress < 0 || linearProgress > 1)
                return false;

            pathProgress = pathProgressAtLinearProgress(linearProgress);

            return pathProgress > 0.001 && pathProgress < 0.999;
        }

        private void insertCurrentTimeControlPoint(PathType pathType)
        {
            if (!tryGetCurrentSliderPathProgress(out double pathProgress))
                return;

            changeHandler?.BeginChange();

            insertControlPointAtPathPosition(HitObject.Path.PositionAt(pathProgress), pathType);

            changeHandler?.EndChange();
        }

        private void splitCurrentTimeBySampling()
        {
            if (editorBeatmap == null || !tryGetCurrentSliderProgress(out double linearProgress, out _))
                return;

            double originalDistance = HitObject.Path.Distance;

            if (originalDistance <= Precision.DOUBLE_EPSILON)
                return;

            var firstPositions = sampleSliderTimelineSegment(0, linearProgress, originalDistance);
            var secondPositions = sampleSliderTimelineSegment(linearProgress, 1, originalDistance);

            if (firstPositions.Count < 2 || secondPositions.Count < 2)
                return;

            double splitTime = HitObject.StartTime + HitObject.Duration * linearProgress;

            Slider firstSlider = createSampledSplitSlider(firstPositions, HitObject.StartTime, HitObject.NewCombo, linearProgress, originalDistance);
            Slider secondSlider = createSampledSplitSlider(secondPositions, splitTime, false, 1 - linearProgress, originalDistance);

            changeHandler?.BeginChange();

            editorBeatmap.SelectedHitObjects.Clear();
            editorBeatmap.Remove(HitObject);
            editorBeatmap.Add(firstSlider);
            editorBeatmap.Add(secondSlider);
            editorBeatmap.SelectedHitObjects.AddRange(new HitObject[] { firstSlider, secondSlider });

            changeHandler?.EndChange();
        }

        private Slider createSampledSplitSlider(IReadOnlyList<Vector2> absolutePositions, double startTime, bool newCombo, double durationFraction, double originalDistance)
        {
            Vector2 origin = absolutePositions[0];
            var path = new SliderPath(absolutePositions.Select((position, index) => new PathControlPoint(position - origin, index == 0 ? PathType.LINEAR : null)).ToArray());
            double pathDistance = path.CalculatedDistance;
            path.ExpectedDistance.Value = pathDistance;

            double targetDistanceAtOriginalVelocity = originalDistance * HitObject.SpanCount() * durationFraction;
            double velocityMultiplier = targetDistanceAtOriginalVelocity > Precision.DOUBLE_EPSILON
                ? HitObject.SliderVelocityMultiplier * pathDistance / targetDistanceAtOriginalVelocity
                : HitObject.SliderVelocityMultiplier;

            return new Slider
            {
                StartTime = startTime,
                Position = HitObject.Position + origin,
                NewCombo = newCombo,
                SliderVelocityMultiplier = Math.Clamp(velocityMultiplier, HitObject.SliderVelocityMultiplierBindable.MinValue, HitObject.SliderVelocityMultiplierBindable.MaxValue),
                Samples = cloneSamples(HitObject.Samples),
                RepeatCount = 0,
                TickDistanceMultiplier = HitObject.TickDistanceMultiplier,
                GenerateTicks = HitObject.GenerateTicks,
                ClassicSliderBehaviour = HitObject.ClassicSliderBehaviour,
                NodeSamples = cloneEndpointNodeSamples(),
                Path = path
            };
        }

        private List<Vector2> sampleSliderTimelineSegment(double startLinearProgress, double endLinearProgress, double originalDistance)
        {
            double travelledDistance = Math.Abs(endLinearProgress - startLinearProgress) * HitObject.SpanCount() * originalDistance;
            int sampleCount = Math.Clamp((int)Math.Ceiling(travelledDistance / 2), 2, 12000);
            var positions = new List<Vector2>(sampleCount);

            for (int i = 0; i < sampleCount; i++)
            {
                double linearProgress = sampleCount == 1 ? startLinearProgress : Interpolation.ValueAt(i, startLinearProgress, endLinearProgress, 0, sampleCount - 1);
                Vector2 position = HitObject.Path.PositionAt(pathProgressAtLinearProgress(linearProgress));

                if (positions.Count == 0 || (positions[^1] - position).LengthSquared > 0.0001f)
                    positions.Add(position);
            }

            return positions;
        }

        private double pathProgressAtLinearProgress(double linearProgress)
        {
            double positionWithRepeats = Math.Clamp(linearProgress, 0, 1) * HitObject.SpanCount();
            double pathProgress = positionWithRepeats - Math.Floor(positionWithRepeats);

            if (positionWithRepeats % 2 >= 1)
                pathProgress = 1 - pathProgress;

            return pathProgress;
        }

        private IList<HitSampleInfo> cloneSamples(IEnumerable<HitSampleInfo> samples) => samples.Select(sample => sample.With()).ToList();

        private IList<IList<HitSampleInfo>> cloneEndpointNodeSamples()
        {
            IList<HitSampleInfo> headSamples = HitObject.NodeSamples.Count > 0 ? HitObject.NodeSamples[0] : HitObject.Samples;
            IList<HitSampleInfo> tailSamples = HitObject.NodeSamples.Count > 1 ? HitObject.NodeSamples[^1] : HitObject.Samples;

            return new List<IList<HitSampleInfo>>
            {
                cloneSamples(headSamples),
                cloneSamples(tailSamples)
            };
        }

        private void addCurrentTimeTangentGlitch()
        {
            if (!tryGetCurrentSliderPathProgress(out double pathProgress))
                return;

            double originalDistance = HitObject.Path.Distance;

            if (originalDistance <= Precision.DOUBLE_EPSILON)
                return;

            PathControlPoint[] inserted = createTangentGlitchControlPoints(pathProgress, originalDistance, out int insertionIndex, out int replacedControlPointCount);

            if (inserted.Length == 0)
                return;

            double originalVelocityMultiplier = HitObject.SliderVelocityMultiplier;

            changeHandler?.BeginChange();

            for (int i = 0; i < replacedControlPointCount; i++)
                controlPoints.RemoveAt(insertionIndex);

            for (int i = 0; i < inserted.Length; i++)
                controlPoints.Insert(insertionIndex + i, inserted[i]);

            ControlPointVisualiser?.EnsureValidPathTypes();

            double newDistance = HitObject.Path.CalculatedDistance;
            HitObject.Path.ExpectedDistance.Value = newDistance;
            HitObject.SliderVelocityMultiplier = Math.Clamp(
                originalVelocityMultiplier * newDistance / originalDistance,
                HitObject.SliderVelocityMultiplierBindable.MinValue,
                HitObject.SliderVelocityMultiplierBindable.MaxValue);
            editorBeatmap?.Update(HitObject);

            changeHandler?.EndChange();
        }

        private PathControlPoint[] createTangentGlitchControlPoints(double pathProgress, double originalDistance, out int insertionIndex, out int replacedControlPointCount)
        {
            insertionIndex = 0;
            replacedControlPointCount = 0;

            if (controlPoints.Count < 4)
                return Array.Empty<PathControlPoint>();

            double foldDistance = Math.Clamp(tangentGlitchDistance.Value, 1, 96);
            double progressRadius = Math.Clamp(foldDistance / originalDistance, 0.001, 0.15);
            double startProgress = Math.Max(0.0001, pathProgress - progressRadius);
            double endProgress = Math.Min(0.9999, pathProgress + progressRadius);

            if (endProgress - startProgress <= Precision.DOUBLE_EPSILON)
                return Array.Empty<PathControlPoint>();

            double segmentDistance = (endProgress - startProgress) * originalDistance;
            int sampleCount = Math.Clamp((int)Math.Ceiling(segmentDistance / 3), 4, 48);
            var localSegment = samplePathSegment(startProgress, endProgress, sampleCount);

            if (localSegment.Count < 2)
                return Array.Empty<PathControlPoint>();

            insertionIndex = findDenseControlPointIndexAtPathDistance(startProgress * originalDistance);
            int endIndex = findDenseControlPointIndexAtPathDistance(endProgress * originalDistance);

            insertionIndex = Math.Clamp(insertionIndex, 1, controlPoints.Count - 2);
            endIndex = Math.Clamp(endIndex, insertionIndex, controlPoints.Count - 2);
            replacedControlPointCount = endIndex - insertionIndex + 1;

            var foldedPositions = new List<Vector2>(localSegment.Count * 3);
            appendDistinct(foldedPositions, localSegment);
            appendDistinct(foldedPositions, localSegment.AsEnumerable().Reverse().Skip(1));
            appendDistinct(foldedPositions, localSegment.Skip(1));

            return foldedPositions
                   .Select(position => new PathControlPoint
                   {
                       Position = position,
                       Type = PathType.LINEAR
                   })
                   .ToArray();
        }

        private List<Vector2> samplePathSegment(double startProgress, double endProgress, int sampleCount)
        {
            var result = new List<Vector2>(sampleCount);

            for (int i = 0; i < sampleCount; i++)
            {
                double progress = sampleCount == 1 ? startProgress : Interpolation.ValueAt(i, startProgress, endProgress, 0, sampleCount - 1);
                result.Add(HitObject.Path.PositionAt(progress));
            }

            return result;
        }

        private int findDenseControlPointIndexAtPathDistance(double targetDistance)
        {
            double distance = 0;

            for (int i = 1; i < controlPoints.Count; i++)
            {
                distance += (controlPoints[i].Position - controlPoints[i - 1].Position).Length;

                if (distance >= targetDistance)
                    return i;
            }

            return controlPoints.Count - 1;
        }

        private static void appendDistinct(List<Vector2> target, IEnumerable<Vector2> source)
        {
            foreach (Vector2 position in source)
            {
                if (target.Count == 0 || (target[^1] - position).LengthSquared > 0.0001f)
                    target.Add(position);
            }
        }

        private int findControlPointInsertionIndex(Vector2 position)
        {
            int insertionIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                float dist = new Line(controlPoints[i].Position, controlPoints[i + 1].Position).DistanceToPoint(position);

                if (dist < minDistance)
                {
                    insertionIndex = i + 1;
                    minDistance = dist;
                }
            }

            return insertionIndex;
        }

        private void removeControlPoints(List<PathControlPoint> toRemove)
        {
            // Ensure that there are any points to be deleted
            if (toRemove.Count == 0)
                return;

            PathControlPoint[] originalPoints = HitObject.Path.ControlPoints.ToArray();
            Vector2[] originalPositions = originalPoints.Select(point => point.Position).ToArray();
            PathType?[] originalTypes = originalPoints.Select(point => point.Type).ToArray();
            double originalDistance = HitObject.Path.Distance;
            double originalVelocity = HitObject.SliderVelocityMultiplier;
            double? originalExpectedDistance = HitObject.Path.ExpectedDistance.Value;
            Vector2 originalPosition = HitObject.Position;

            foreach (var c in toRemove)
            {
                // The first control point in the slider must have a type, so take it from the previous "first" one
                // Todo: Should be handled within SliderPath itself
                if (c == controlPoints[0] && controlPoints.Count > 1 && controlPoints[1].Type == null)
                    controlPoints[1].Type = controlPoints[0].Type;

                controlPoints.Remove(c);
            }

            ControlPointVisualiser?.EnsureValidPathTypes();

            // If there are 0 or 1 remaining control points, or the slider has an invalid length, it is in a degenerate form and should be deleted
            if (controlPoints.Count <= 1)
            {
                placementHandler?.Delete(HitObject);
                return;
            }

            // The path will have a non-zero offset if the head is removed, but sliders don't support this behaviour since the head is positioned at the slider's position
            // So the slider needs to be offset by this amount instead, and all control points offset backwards such that the path is re-positioned at (0, 0)
            Vector2 first = controlPoints[0].Position;
            foreach (var c in controlPoints)
                c.Position -= first;
            HitObject.Position += first;

            if (keepDuration.Value)
            {
                if (!SliderDurationPreserver.TryApply(HitObject, originalDistance, originalVelocity, editorBeatmap, out _)
                    || !HitObject.Path.HasValidLengthForPlacement)
                {
                    restoreRemovedControlPoints(originalPoints, originalPositions, originalTypes, originalPosition, originalExpectedDistance, originalVelocity);
                }
            }
            else
            {
                // Snap the slider to the current beat divisor before checking length validity.
                HitObject.SnapTo(distanceSnapProvider);

                if (!HitObject.Path.HasValidLengthForPlacement)
                    placementHandler?.Delete(HitObject);
            }
        }

        private void restoreRemovedControlPoints(PathControlPoint[] originalPoints, Vector2[] originalPositions, PathType?[] originalTypes,
                                                 Vector2 originalPosition, double? originalExpectedDistance, double originalVelocity)
        {
            for (int i = 0; i < originalPoints.Length; i++)
            {
                originalPoints[i].Position = originalPositions[i];
                originalPoints[i].Type = originalTypes[i];
            }

            HitObject.Path.ControlPoints.ReplaceRange(0, HitObject.Path.ControlPoints.Count, originalPoints);
            HitObject.Position = originalPosition;
            HitObject.Path.ExpectedDistance.Value = originalExpectedDistance;
            HitObject.SliderVelocityMultiplier = originalVelocity;
            if (editorBeatmap != null)
                HitObject.UpdateVelocity(editorBeatmap.ControlPointInfo, editorBeatmap.Difficulty);
        }

        private void splitControlPoints(List<PathControlPoint> controlPointsToSplitAt)
        {
            if (editorBeatmap == null)
                return;

            // Arbitrary gap in milliseconds to put between split slider pieces
            const double split_gap = 100;

            // Ensure that there are any points to be split
            if (controlPointsToSplitAt.Count == 0)
                return;

            editorBeatmap.SelectedHitObjects.Clear();

            foreach (var splitPoint in controlPointsToSplitAt)
            {
                if (splitPoint == controlPoints[0] || splitPoint == controlPoints[^1] || splitPoint.Type == null)
                    continue;

                // Split off the section of slider before this control point so the remaining control points to split are in the latter part of the slider.
                int index = controlPoints.IndexOf(splitPoint);

                if (index <= 0)
                    continue;

                // Extract the split portion and remove from the original slider.
                var splitControlPoints = controlPoints.Take(index + 1).ToList();
                controlPoints.RemoveRange(0, index);

                var newSlider = new Slider
                {
                    StartTime = HitObject.StartTime,
                    Position = HitObject.Position + splitControlPoints[0].Position,
                    NewCombo = HitObject.NewCombo,
                    SliderVelocityMultiplier = HitObject.SliderVelocityMultiplier,
                    Samples = HitObject.Samples.Select(s => s.With()).ToList(),
                    RepeatCount = HitObject.RepeatCount,
                    NodeSamples = HitObject.NodeSamples.Select(n => (IList<HitSampleInfo>)n.Select(s => s.With()).ToList()).ToList(),
                    Path = new SliderPath(splitControlPoints.Select(o => new PathControlPoint(o.Position - splitControlPoints[0].Position, o == splitControlPoints[^1] ? null : o.Type)).ToArray())
                };

                // Increase the start time of the slider before adding the new slider so the new slider is immediately inserted at the correct index and internal state remains valid.
                HitObject.StartTime += split_gap;

                editorBeatmap.Add(newSlider);

                HitObject.NewCombo = false;
                HitObject.Path.ExpectedDistance.Value -= newSlider.Path.CalculatedDistance;
                HitObject.StartTime += newSlider.SpanDuration;

                // In case the remainder of the slider has no length left over, give it length anyways so we don't get a 0 length slider.
                if (HitObject.Path.ExpectedDistance.Value <= Precision.DOUBLE_EPSILON)
                {
                    HitObject.Path.ExpectedDistance.Value = null;
                }
            }

            // Once all required pieces have been split off, the original slider has the final split.
            // As a final step, we must reset its control points to have an origin of (0,0).
            Vector2 first = controlPoints[0].Position;
            foreach (var c in controlPoints)
                c.Position -= first;
            HitObject.Position += first;
        }

        private void showConvertToStreamPopover()
        {
            createCurrentPopover = () => new ConvertToStreamPopover(HitObject, this);
            this.ShowPopover();
        }

        private void showSliderVelocityPopover()
        {
            createCurrentPopover = () => new SliderVelocityPopover(new[] { HitObject });
            this.ShowPopover();
        }

        private MenuItem[] createCurrentTimeControlPointItems() => new[]
        {
            new OsuMenuItem("Linear", MenuItemType.Standard, () => insertCurrentTimeControlPoint(PathType.LINEAR)),
            new OsuMenuItem("Bezier", MenuItemType.Standard, () => insertCurrentTimeControlPoint(PathType.BEZIER)),
            new OsuMenuItem("Perfect curve", MenuItemType.Standard, () => insertCurrentTimeControlPoint(PathType.PERFECT_CURVE)),
            new OsuMenuItem("B-spline", MenuItemType.Standard, () => insertCurrentTimeControlPoint(PathType.BSpline(4))),
            new OsuMenuItem("Catmull", MenuItemType.Standard, () => insertCurrentTimeControlPoint(PathType.CATMULL))
        };

        internal void SetConvertToStreamPreviewHidden(bool hidden)
        {
            if (hidden)
            {
                if (preConvertToStreamDrawableAlpha != null || preConvertToStreamBlueprintAlpha != null)
                    return;

                preConvertToStreamDrawableAlpha = DrawableObject.Alpha;
                preConvertToStreamBlueprintAlpha = Alpha;

                DrawableObject.Alpha = 0;
                Alpha = 0;
            }
            else
            {
                if (preConvertToStreamDrawableAlpha != null)
                    DrawableObject.Alpha = preConvertToStreamDrawableAlpha.Value;

                if (preConvertToStreamBlueprintAlpha != null)
                    Alpha = preConvertToStreamBlueprintAlpha.Value;

                preConvertToStreamDrawableAlpha = null;
                preConvertToStreamBlueprintAlpha = null;
            }
        }

        public Popover GetPopover() => createCurrentPopover?.Invoke() ?? new ConvertToStreamPopover(HitObject, this);

        public override MenuItem[] ContextMenuItems
        {
            get
            {
                var items = new List<MenuItem>();

                if ((!anchorMode.Value || !AnchorPath.TryRead(HitObject.Path, out _)) && shouldUseDetailedControlPointVisualiser())
                {
                    items.Add(new OsuMenuItem("Add control point", MenuItemType.Standard, () =>
                    {
                        changeHandler?.BeginChange();
                        addControlPoint(lastRightClickPosition);
                        changeHandler?.EndChange();
                    })
                    {
                        Hotkey = new Hotkey(new KeyCombination(InputKey.Control, InputKey.MouseLeft))
                    });
                }

                items.Add(new OsuMenuItem("Convert to stream...", MenuItemType.Standard, showConvertToStreamPopover)
                {
                    Hotkey = new Hotkey(new KeyCombination(InputKey.Control, InputKey.Shift, InputKey.F))
                });
                items.Add(new OsuMenuItem("Slider velocity...", MenuItemType.Standard, showSliderVelocityPopover));
                items.Add(new OsuMenuItem("Insert control point at current time")
                {
                    Items = createCurrentTimeControlPointItems()
                });
                items.Add(new OsuMenuItem("Insert and split at current time", MenuItemType.Standard, splitCurrentTimeBySampling)
                {
                    Hotkey = new Hotkey(GlobalAction.EditorSliderInsertAndSplitAtCurrentTime)
                });
                items.Add(new OsuMenuItem("Add tangent glitch at current time", MenuItemType.Standard, addCurrentTimeTangentGlitch)
                {
                    Hotkey = new Hotkey(GlobalAction.EditorSliderAddTangentGlitchAtCurrentTime)
                });

                return items.ToArray();
            }
        }

        // Always refer to the drawable object's slider body so subsequent movement deltas are calculated with updated positions.
        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.SliderBody?.ToScreenSpace(DrawableObject.SliderBody.PathOffset)
                                                             ?? BodyPiece.ToScreenSpace(BodyPiece.PathStartLocation);

        public Vector2 ScreenSpaceTailSnapPoint =>
            DrawableObject.SliderBody?.ToScreenSpace(DrawableObject.SliderBody.PathEndOffset) ?? BodyPiece.ToScreenSpace(BodyPiece.PathEndLocation);

        public Vector2[] ScreenSpaceNonControlPointSnapPoints => new[]
        {
            ScreenSpaceSelectionPoint,
            ScreenSpaceTailSnapPoint
        };

        public Vector2[] ScreenSpaceControlPointSnapPoints => getScreenSpaceControlPointNodes().ToArray();

        protected override Vector2[] ScreenSpaceAdditionalNodes => controlPointSnapTargets.Value
            ? getScreenSpaceControlPointNodes().Prepend(ScreenSpaceTailSnapPoint).ToArray()
            : new[] { ScreenSpaceTailSnapPoint };

        private IEnumerable<Vector2> getScreenSpaceControlPointNodes()
        {
            // Returns the positions of control points that produce visible kinks on the slider's path
            // This excludes inherited control points from Bezier, B-Spline, Perfect, and Catmull curves
            if (DrawableObject.SliderBody == null)
                yield break;

            PathType? currentPathType = null;

            // Skip the last control point because its always either not on the slider path or exactly on the slider end
            for (int i = 0; i < DrawableObject.HitObject.Path.ControlPoints.Count - 1; i++)
            {
                var controlPoint = DrawableObject.HitObject.Path.ControlPoints[i];

                if (controlPoint.Type is not null)
                    currentPathType = controlPoint.Type;

                // Skip the first control point because it is already covered by the slider head
                if (i == 0)
                    continue;

                if (controlPoint.Type is null && currentPathType != PathType.LINEAR)
                    continue;

                var screenSpacePosition = DrawableObject.SliderBody.ToScreenSpace(DrawableObject.SliderBody.PathOffset + controlPoint.Position);
                yield return screenSpacePosition;
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            if (BodyPiece.ReceivePositionalInputAt(screenSpacePos) && (IsSelected || DrawableObject.Body.Alpha > 0 || DrawableObject.HeadCircle.Alpha > 0))
                return true;

            if (ControlPointVisualiser == null)
                return false;

            foreach (var p in ControlPointVisualiser.Pieces)
            {
                if (p.ReceivePositionalInputAt(screenSpacePos))
                    return true;
            }

            return false;
        }

        protected virtual SliderCircleOverlay CreateCircleOverlay(Slider slider, SliderPosition position) => new SliderCircleOverlay(slider, position);
    }
}
