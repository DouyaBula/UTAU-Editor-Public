// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Edit.Curves;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public partial class PathControlPointVisualiser<T> : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
        where T : OsuHitObject, IHasPath, IHasSliderVelocity
    {
        /// <summary>
        /// Whether this visualiser (including its control-point pieces) is allowed to receive pointer input.
        ///
        /// The placement blueprint keeps the classic visualiser alive so that switching modes does not
        /// recreate the drawable tree.  Merely fading it out is not enough: <see cref="PathControlPointPiece{T}"/>
        /// can still win hit-testing and steal a pen click.  Keep input gating here so the whole subtree is
        /// disabled atomically while anchor placement is active.
        /// </summary>
        public bool InputEnabled { get; set; } = true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InputEnabled; // allow context menu to appear outside the playfield.

        public override bool PropagatePositionalInputSubTree => InputEnabled && base.PropagatePositionalInputSubTree;

        internal readonly Container<PathControlPointPiece<T>> Pieces;

        private readonly IBindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly T hitObject;
        private readonly bool allowSelection;

        private InputManager inputManager;

        public Action<List<PathControlPoint>> RemoveControlPointsRequested;
        public Action<List<PathControlPoint>> SplitControlPointsRequested;

        [Resolved(CanBeNull = true)]
        [CanBeNull]
        private OsuHitObjectComposer positionSnapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider distanceSnapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private EditorBeatmap editorBeatmap { get; set; }

        private Bindable<bool> limitedDistanceSnap { get; set; } = null!;
        private Bindable<bool> keepDuration { get; set; } = null!;
        private Bindable<float> cornerAmount { get; set; } = null!;

        public PathControlPointVisualiser(T hitObject, bool allowSelection)
        {
            this.hitObject = hitObject;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new PathControlPointConnection<T>(hitObject),
                Pieces = new Container<PathControlPointPiece<T>> { RelativeSizeAxes = Axes.Both }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            limitedDistanceSnap = config.GetBindable<bool>(OsuSetting.EditorLimitedDistanceSnap);
            keepDuration = config.GetBindable<bool>(OsuSetting.EditorAnchorKeepDuration);
            cornerAmount = config.GetBindable<float>(OsuSetting.EditorAnchorCornerAmount);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            controlPoints.CollectionChanged += onControlPointsChanged;
            controlPoints.BindTo(hitObject.Path.ControlPoints);
        }

        // Generally all the control points are within the visible area all the time.
        public override bool UpdateSubTreeMasking() => true;

        /// <summary>
        /// Handles correction of invalid path types.
        /// </summary>
        public void EnsureValidPathTypes()
        {
            List<PathControlPoint> pointsInCurrentSegment = new List<PathControlPoint>();

            foreach (var controlPoint in controlPoints)
            {
                // A typed point is both the end of the previous segment and the start of the next
                // one. Include it while validating the previous segment, but never validate a
                // one-point list containing only the first anchor (that used to turn every valid
                // perfect-curve start into Bézier during Keep duration edits).
                if (controlPoint.Type != null && pointsInCurrentSegment.Count > 0)
                {
                    pointsInCurrentSegment.Add(controlPoint);
                    ensureValidPathType(pointsInCurrentSegment);
                    pointsInCurrentSegment.Clear();
                }

                pointsInCurrentSegment.Add(controlPoint);
            }

            ensureValidPathType(pointsInCurrentSegment);
        }

        private void ensureValidPathType(IReadOnlyList<PathControlPoint> segment)
        {
            if (segment.Count == 0)
                return;

            PathControlPoint first = segment[0];

            if (first.Type != PathType.PERFECT_CURVE)
                return;

            if (segment.Count != 3)
                first.Type = PathType.BEZIER;

            if (segment.Count != 3)
                return;

            ReadOnlySpan<Vector2> points = segment.Select(p => p.Position).ToArray();
            RectangleF boundingBox = PathApproximator.CircularArcBoundingBox(points);
            if (boundingBox.Width >= 640 || boundingBox.Height >= 480)
                first.Type = PathType.BEZIER;
        }

        /// <summary>
        /// Selects the <see cref="PathControlPointPiece{T}"/> corresponding to the given <paramref name="pathControlPoint"/>,
        /// and deselects all other <see cref="PathControlPointPiece{T}"/>s.
        /// </summary>
        public void SetSelectionTo(PathControlPoint pathControlPoint)
        {
            foreach (var p in Pieces)
                p.IsSelected.Value = p.ControlPoint == pathControlPoint;
        }

        /// <summary>
        /// Delete all visually selected <see cref="PathControlPoint"/>s.
        /// </summary>
        /// <returns>Whether any change actually took place.</returns>
        public bool DeleteSelected()
        {
            List<PathControlPoint> toRemove = Pieces.Where(p => p.IsSelected.Value).Select(p => p.ControlPoint).ToList();

            if (!Delete(toRemove))
                return false;

            // Since pieces are re-used, they will not point to the deleted control points while remaining selected
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;

            return true;
        }

        /// <summary>
        /// Delete the specified <see cref="PathControlPoint"/>s.
        /// </summary>
        /// <returns>Whether any change actually took place.</returns>
        public bool Delete(List<PathControlPoint> toRemove)
        {
            // Ensure that there are any points to be deleted
            if (toRemove.Count == 0)
                return false;

            changeHandler?.BeginChange();
            RemoveControlPointsRequested?.Invoke(toRemove);
            changeHandler?.EndChange();
            return true;
        }

        private bool splitSelected()
        {
            List<PathControlPoint> controlPointsToSplitAt = Pieces.Where(p => p.IsSelected.Value && isSplittable(p)).Select(p => p.ControlPoint).ToList();

            // Ensure that there are any points to be split
            if (controlPointsToSplitAt.Count == 0)
                return false;

            changeHandler?.BeginChange();
            SplitControlPointsRequested?.Invoke(controlPointsToSplitAt);
            changeHandler?.EndChange();

            // Since pieces are re-used, they will not point to the deleted control points while remaining selected
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;

            return true;
        }

        private bool isSplittable(PathControlPointPiece<T> p) =>
            // A hit object can only be split on control points which connect two different path segments.
            p.ControlPoint.Type.HasValue && p.ControlPoint != controlPoints.FirstOrDefault() && p.ControlPoint != controlPoints.LastOrDefault();

        private void onControlPointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var point = (PathControlPoint)e.NewItems[i];
                        Pieces.Add(createPiece(point));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var point in e.OldItems.Cast<PathControlPoint>())
                        removePiece(point);

                    break;

                case NotifyCollectionChangedAction.Replace:
                    // ReplaceRange() is used by topology-changing classic tools. Rebuild the
                    // corresponding drawable pieces; otherwise the UI would retain references to
                    // removed points and every subsequent tool would appear to do nothing.
                    if (e.OldItems != null)
                    {
                        foreach (var point in e.OldItems.Cast<PathControlPoint>())
                            removePiece(point);
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var point in e.NewItems.Cast<PathControlPoint>())
                            Pieces.Add(createPiece(point));
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var piece in Pieces.ToArray())
                    {
                        piece.ControlPoint.Changed -= controlPointChanged;
                        piece.RemoveAndDisposeImmediately();
                    }

                    foreach (var point in controlPoints)
                        Pieces.Add(createPiece(point));

                    break;
            }
        }

        private PathControlPointPiece<T> createPiece(PathControlPoint point)
            => new PathControlPointPiece<T>(hitObject, point).With(d =>
            {
                if (allowSelection)
                    d.RequestSelection = selectionRequested;

                d.ControlPoint.Changed += controlPointChanged;
                d.DragStarted = DragStarted;
                d.DragInProgress = DragInProgress;
                d.DragEnded = DragEnded;
            });

        private void removePiece(PathControlPoint point)
        {
            point.Changed -= controlPointChanged;

            foreach (var piece in Pieces.Where(p => ReferenceEquals(p.ControlPoint, point)).ToArray())
                piece.RemoveAndDisposeImmediately();
        }

        private void controlPointChanged() => updateCurveMenuItems();

        protected override bool OnClick(ClickEvent e)
        {
            if (!InputEnabled)
                return false;

            if (Pieces.Any(piece => piece.IsHovered))
                return false;

            foreach (var piece in Pieces)
            {
                piece.IsSelected.Value = false;
            }

            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (!InputEnabled)
                return false;

            switch (e.Action)
            {
                case PlatformAction.Delete:
                    return DeleteSelected();
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly PathType?[] path_types =
        [
            PathType.LINEAR,
            PathType.BEZIER,
            PathType.PERFECT_CURVE,
            PathType.BSpline(4),
            PathType.CATMULL,
            null,
        ];

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!InputEnabled || e.Repeat)
                return false;

            if (e.AltPressed && !e.ControlPressed && !e.ShiftPressed)
            {
                switch (e.Key)
                {
                    case Key.R: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryRoundOrBevel(slider, points, cornerAmount.Value, false, false, out _));
                    case Key.B: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryRoundOrBevel(slider, points, cornerAmount.Value, true, false, out _));
                    case Key.L: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryStraighten(slider, points));
                    case Key.H: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryRetractHandles(slider, points));
                    case Key.S: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TrySmooth(slider, points, false));
                    case Key.Y: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TrySmooth(slider, points, true));
                    case Key.X: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryAlign(slider, points, true));
                    case Key.V: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryAlign(slider, points, false));
                    case Key.Q: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TrySimplifyLinear(slider, points));
                }
            }

            if (e.AltPressed && e.ShiftPressed && !e.ControlPressed)
            {
                switch (e.Key)
                {
                    case Key.X: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryDistribute(slider, points, true));
                    case Key.V: return applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryDistribute(slider, points, false));
                }
            }

            // C is a play/pause shortcut in the editor. Once a control point is the active
            // editing target, consume it here so the slider editor keeps keyboard priority.
            if (e.Key == Key.C && Pieces.Any(p => p.IsHovered || p.IsSelected.Value))
                return true;

            switch (e.Key)
            {
                case Key.Tab:
                {
                    var selectedPieces = Pieces.Where(p => p.IsSelected.Value).ToArray();
                    if (selectedPieces.Length != 1)
                        return false;

                    PathControlPointPiece<T> selectedPiece = selectedPieces.Single();
                    PathControlPoint selectedPoint = selectedPiece.ControlPoint;

                    PathType?[] validTypes = path_types;

                    if (selectedPoint == controlPoints[0])
                        validTypes = validTypes.Where(t => t != null).ToArray();

                    // Catmull is a structural knot-based path type and should only be cycled to
                    // when the selected point is already part of a Catmull segment. Keeping it out
                    // of the ordinary type cycle avoids an accidental Linear -> Catmull jump while
                    // still allowing existing Catmull paths to be edited and preserved.
                    if (selectedPoint.Type?.Type != SplineType.Catmull)
                        validTypes = validTypes.Where(t => t?.Type != SplineType.Catmull).ToArray();

                    int currentTypeIndex = Array.IndexOf(validTypes, selectedPoint.Type);

                    if (currentTypeIndex < 0 && e.ShiftPressed)
                        currentTypeIndex = 0;

                    changeHandler?.BeginChange();

                    for (int attempt = 0; attempt < validTypes.Length; attempt++)
                    {
                        currentTypeIndex = (validTypes.Length + currentTypeIndex + (e.ShiftPressed ? -1 : 1)) % validTypes.Length;

                        bool applied = updatePathTypeOfSelectedPieces(validTypes[currentTypeIndex]);
                        if (!applied || selectedPoint.Type == validTypes[currentTypeIndex])
                            break;
                    }

                    changeHandler?.EndChange();

                    return true;
                }

                case Key.Number1:
                case Key.Number2:
                case Key.Number3:
                case Key.Number4:
                case Key.Number5:
                case Key.Number6:
                {
                    if (!e.AltPressed)
                        return false;

                    // If no pieces are selected, we can't change the path type.
                    if (Pieces.All(p => !p.IsSelected.Value))
                        return false;

                    PathType? type = path_types[e.Key - Key.Number1];

                    // The first control point can never be inherit type
                    if (Pieces[0].IsSelected.Value && type == null)
                        return false;

                    updatePathTypeOfSelectedPieces(type);
                    return true;
                }

                default:
                    return false;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            foreach (var p in Pieces)
                p.ControlPoint.Changed -= controlPointChanged;

            if (draggedControlPointIndex >= 0)
                DragEnded();
        }

        private void selectionRequested(PathControlPointPiece<T> piece, MouseButtonEvent e)
        {
            if (e.Button == MouseButton.Left && inputManager.CurrentState.Keyboard.ControlPressed)
                piece.IsSelected.Toggle();
            else
                SetSelectionTo(piece.ControlPoint);
        }

        /// <summary>
        /// Attempts to set all selected control point pieces to the given path type.
        /// If that fails, try to change the path such that it instead succeeds
        /// in a UX-friendly way.
        /// </summary>
        /// <param name="type">The path type we want to assign to the given control point piece.</param>
        private bool updatePathTypeOfSelectedPieces(PathType? type)
        {
            PathControlPoint[] selectedPoints = Pieces.Where(p => p.IsSelected.Value).Select(p => p.ControlPoint).ToArray();
            if (selectedPoints.Length == 0)
                return false;

            PathControlPoint[] originalPoints = hitObject.Path.ControlPoints.ToArray();
            Vector2[] originalPositions = originalPoints.Select(p => p.Position).ToArray();
            PathType?[] originalTypes = originalPoints.Select(p => p.Type).ToArray();
            double? originalExpectedDistance = hitObject.Path.ExpectedDistance.Value;
            double originalDistance = hitObject.Path.Distance;
            double originalVelocity = hitObject.SliderVelocityMultiplier;

            changeHandler?.BeginChange();
            try
            {
                hitObject.Path.ExpectedDistance.Value = null;

                foreach (PathControlPoint selectedPoint in selectedPoints)
                {
                    List<PathControlPoint> pointsInSegment = hitObject.Path.PointsInSegment(selectedPoint);
                    int indexInSegment = pointsInSegment.IndexOf(selectedPoint);

                    if (indexInSegment < 0)
                        continue;

                    if (type?.Type == SplineType.PerfectCurve)
                    {
                        int pointsFromSelectedPoint = pointsInSegment.Count - indexInSegment;

                        if (pointsFromSelectedPoint == 2)
                        {
                            // A perfect curve needs three points. When switching a two-point
                            // segment to Perfect Curve, create a shallow inherited midpoint so
                            // the type is immediately usable without requiring a third manual click.
                            PathControlPoint midpoint = createPerfectCurveMidpoint(selectedPoint, pointsInSegment[^1]);
                            if (midpoint == null)
                                continue;

                            int globalIndex = hitObject.Path.ControlPoints.IndexOf(selectedPoint);
                            hitObject.Path.ControlPoints.Insert(globalIndex + 1, midpoint);
                        }
                        else
                        {
                            // Can't always create a circular arc out of 4 or more points,
                            // so split the segment after the first three points and continue
                            // the old path type from there.
                            int thirdPointIndex = indexInSegment + 2;

                            if (pointsInSegment.Count > thirdPointIndex + 1)
                                pointsInSegment[thirdPointIndex].Type = pointsInSegment[0].Type;
                        }
                    }

                    selectedPoint.Type = type;
                }

                EnsureValidPathTypes();

                if (keepDuration.Value)
                {
                    if (hitObject is not Slider slider || !SliderDurationPreserver.TryApply(slider, originalDistance, originalVelocity, editorBeatmap, out _))
                    {
                        restorePathTypeEdit(originalPoints, originalPositions, originalTypes, originalExpectedDistance, originalVelocity);
                        return false;
                    }

                    if (!hitObject.Path.HasValidLengthForPlacement)
                    {
                        restorePathTypeEdit(originalPoints, originalPositions, originalTypes, originalExpectedDistance, originalVelocity);
                        return false;
                    }
                }
                else
                {
                    if (hitObject.Path.Distance < originalDistance)
                        hitObject.SnapTo(distanceSnapProvider);
                    else
                        hitObject.Path.ExpectedDistance.Value = originalDistance;
                }

                return true;
            }
            finally
            {
                changeHandler?.EndChange();
            }
        }

        private static PathControlPoint createPerfectCurveMidpoint(PathControlPoint start, PathControlPoint end)
        {
            Vector2 chord = end.Position - start.Position;
            float chordLength = chord.Length;

            if (!float.IsFinite(chordLength) || chordLength < 0.01f)
                return null;

            Vector2 normal = new Vector2(-chord.Y, chord.X) / chordLength;
            float offset = Math.Min(chordLength * 0.2f, 80);
            return new PathControlPoint((start.Position + end.Position) * 0.5f + normal * offset);
        }

        private void restorePathTypeEdit(PathControlPoint[] originalPoints, Vector2[] originalPositions, PathType?[] originalTypes, double? originalExpectedDistance, double originalVelocity)
        {
            for (int i = 0; i < originalPoints.Length; i++)
            {
                originalPoints[i].Position = originalPositions[i];
                originalPoints[i].Type = originalTypes[i];
            }

            hitObject.Path.ControlPoints.ReplaceRange(0, hitObject.Path.ControlPoints.Count, originalPoints);
            hitObject.Path.ExpectedDistance.Value = originalExpectedDistance;
            hitObject.SliderVelocityMultiplier = originalVelocity;

            if (hitObject is Slider slider && editorBeatmap != null)
                slider.UpdateVelocity(editorBeatmap.ControlPointInfo, editorBeatmap.Difficulty);
        }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        #region Drag handling

        private Vector2[] dragStartPositions;
        private PathType?[] dragPathTypes;
        private double dragStartDistance;
        private double dragStartVelocity;
        private int draggedControlPointIndex = -1;
        private HashSet<PathControlPoint> selectedControlPoints;

        private List<MenuItem> curveTypeItems;

        public void DragStarted(PathControlPoint controlPoint)
        {
            if (!InputEnabled)
                return;

            dragStartPositions = hitObject.Path.ControlPoints.Select(point => point.Position).ToArray();
            dragPathTypes = hitObject.Path.ControlPoints.Select(point => point.Type).ToArray();
            dragStartDistance = hitObject.Path.Distance;
            dragStartVelocity = hitObject.SliderVelocityMultiplier;
            draggedControlPointIndex = hitObject.Path.ControlPoints.IndexOf(controlPoint);
            selectedControlPoints = new HashSet<PathControlPoint>(Pieces.Where(piece => piece.IsSelected.Value).Select(piece => piece.ControlPoint));

            Debug.Assert(draggedControlPointIndex >= 0);

            changeHandler?.BeginChange();
        }

        public void DragInProgress(DragEvent e)
        {
            if (!InputEnabled)
                return;

            Vector2[] oldControlPoints = hitObject.Path.ControlPoints.Select(cp => cp.Position).ToArray();
            Vector2 oldPosition = hitObject.Position;
            double oldStartTime = hitObject.StartTime;
            double oldVelocity = hitObject.SliderVelocityMultiplier;
            double? oldExpectedDistance = hitObject.Path.ExpectedDistance.Value;

            if (selectedControlPoints.Contains(hitObject.Path.ControlPoints[0]))
            {
                // Special handling for selections containing head control point - the position of the hit object changes which means the snapped position and time have to be taken into account
                Vector2 newHeadPosition = Parent!.ToScreenSpace(e.MousePosition + (dragStartPositions[0] - dragStartPositions[draggedControlPointIndex]));

                var result = positionSnapProvider?.TrySnapToNearbyObjects(newHeadPosition, oldStartTime);
                result ??= positionSnapProvider?.TrySnapToDistanceGrid(newHeadPosition, limitedDistanceSnap.Value ? oldStartTime : null);
                if (positionSnapProvider?.TrySnapToPositionGrid(result?.ScreenSpacePosition ?? newHeadPosition, result?.Time ?? oldStartTime) is SnapResult gridSnapResult)
                    result = gridSnapResult;
                result ??= new SnapResult(newHeadPosition, oldStartTime);

                Vector2 movementDelta = Parent!.ToLocalSpace(result.ScreenSpacePosition) - hitObject.Position;

                hitObject.Position += movementDelta;
                hitObject.StartTime = result.Time ?? hitObject.StartTime;

                for (int i = 1; i < hitObject.Path.ControlPoints.Count; i++)
                {
                    PathControlPoint controlPoint = hitObject.Path.ControlPoints[i];
                    // Since control points are relative to the position of the hit object, all points that are _not_ selected
                    // need to be offset _back_ by the delta corresponding to the movement of the head point.
                    // All other selected control points (if any) will move together with the head point
                    // (and so they will not move at all, relative to each other).
                    if (!selectedControlPoints.Contains(controlPoint))
                        controlPoint.Position -= movementDelta;
                }
            }
            else
            {
                Vector2 newControlPointPosition = Parent!.ToScreenSpace(e.MousePosition);

                // Snapping inherited B-spline control points to nearby objects would be unintuitive, because snapping them does not equate to snapping the interpolated slider path.
                bool shouldSnapToNearbyObjects = dragPathTypes[draggedControlPointIndex] is not null ||
                                                 dragPathTypes[..draggedControlPointIndex].LastOrDefault(t => t is not null)?.Type != SplineType.BSpline;

                SnapResult result = null;
                if (shouldSnapToNearbyObjects)
                    result = positionSnapProvider?.TrySnapToNearbyObjects(newControlPointPosition, oldStartTime);
                if (positionSnapProvider?.TrySnapToPositionGrid(result?.ScreenSpacePosition ?? newControlPointPosition, result?.Time ?? oldStartTime) is SnapResult gridSnapResult)
                    result = gridSnapResult;
                result ??= new SnapResult(newControlPointPosition, oldStartTime);

                Vector2 movementDelta = Parent!.ToLocalSpace(result.ScreenSpacePosition) - dragStartPositions[draggedControlPointIndex] - hitObject.Position;

                for (int i = 0; i < controlPoints.Count; ++i)
                {
                    PathControlPoint controlPoint = controlPoints[i];
                    if (selectedControlPoints.Contains(controlPoint))
                        controlPoint.Position = dragStartPositions[i] + movementDelta;
                }
            }

            // Restore the drag-start types before measuring geometry. Perfect-curve validation
            // can change the path, so measuring first would leave the compensated SV stale.
            for (int i = 0; i < hitObject.Path.ControlPoints.Count; i++)
                hitObject.Path.ControlPoints[i].Type = dragPathTypes[i];
            EnsureValidPathTypes();

            if (keepDuration.Value && hitObject is Slider slider)
            {
                if (!SliderDurationPreserver.TryApply(slider, dragStartDistance, dragStartVelocity, editorBeatmap, out _))
                {
                    restoreDragState(oldControlPoints, oldPosition, oldStartTime, oldVelocity, oldExpectedDistance);
                    return;
                }
            }
            else
            {
                // Keep the established classic-editor behaviour unchanged when duration
                // preservation is disabled.
                hitObject.SnapTo(distanceSnapProvider);
            }

            if (!hitObject.Path.HasValidLengthForPlacement)
            {
                restoreDragState(oldControlPoints, oldPosition, oldStartTime, oldVelocity, oldExpectedDistance);
                return;
            }

        }

        private void restoreDragState(Vector2[] oldControlPoints, Vector2 oldPosition, double oldStartTime, double oldVelocity, double? oldExpectedDistance)
        {
            for (int i = 0; i < hitObject.Path.ControlPoints.Count && i < oldControlPoints.Length; i++)
                hitObject.Path.ControlPoints[i].Position = oldControlPoints[i];

            hitObject.Position = oldPosition;
            hitObject.StartTime = oldStartTime;
            if (keepDuration.Value)
            {
                hitObject.Path.ExpectedDistance.Value = oldExpectedDistance;
                hitObject.SliderVelocityMultiplier = oldVelocity;
                if (hitObject is Slider slider && editorBeatmap != null)
                    slider.UpdateVelocity(editorBeatmap.ControlPointInfo, editorBeatmap.Difficulty);
            }
            else
            {
                // Keep the official control-point editor's fallback algorithm unchanged when
                // duration preservation is disabled.
                hitObject.SnapTo(distanceSnapProvider);
            }
        }

        public void DragEnded()
        {
            changeHandler?.EndChange();
            draggedControlPointIndex = -1;
        }

        #endregion

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!InputEnabled || !Pieces.Any(p => p.IsHovered))
                    return null;

                var selectedPieces = Pieces.Where(p => p.IsSelected.Value).ToList();
                int count = selectedPieces.Count;

                if (count == 0)
                    return null;

                var splittablePieces = selectedPieces.Where(isSplittable).ToList();
                int splittableCount = splittablePieces.Count;

                curveTypeItems = new List<MenuItem>();

                for (int i = 0; i < path_types.Length; ++i)
                {
                    PathType? type = path_types[i];

                    // special inherit case
                    if (type == null)
                    {
                        if (selectedPieces.Contains(Pieces[0]))
                            continue;

                        curveTypeItems.Add(new OsuMenuItemSpacer());
                    }

                    curveTypeItems.Add(createMenuItemForPathType(type, InputKey.Number1 + i));
                }

                // Original: Catmull was only shown when an already-Catmull point was selected.
                // if (selectedPieces.Any(piece => piece.ControlPoint.Type?.Type == SplineType.Catmull))
                // {
                //     curveTypeItems.Add(new OsuMenuItemSpacer());
                //     curveTypeItems.Add(createMenuItemForPathType(PathType.CATMULL));
                // }

                var menuItems = new List<MenuItem>
                {
                    new OsuMenuItem("Curve type")
                    {
                        Items = curveTypeItems
                    }
                };

                // Classic curve tools deliberately target uninherited anchors only. Do not present
                // a menu full of no-op actions when the user selected an inherited handle; Delete
                // and curve-type actions remain available below.
                if (selectedPieces.Any(piece => piece.ControlPoint.Type.HasValue))
                {
                    menuItems.Add(new OsuMenuItem("Classic curve tools")
                    {
                        Items = new MenuItem[]
                        {
                            classicShortcut("Round corners", InputKey.R, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryRoundOrBevel(slider, points, cornerAmount.Value, false, false, out _))),
                            classicShortcut("Bevel corners", InputKey.B, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryRoundOrBevel(slider, points, cornerAmount.Value, true, false, out _))),
                            classicShortcut("Straighten segments", InputKey.L, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryStraighten(slider, points))),
                            classicShortcut("Smooth handles", InputKey.S, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TrySmooth(slider, points, false))),
                            classicShortcut("Symmetric handles", InputKey.Y, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TrySmooth(slider, points, true))),
                            classicShortcut("Retract handles", InputKey.H, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryRetractHandles(slider, points))),
                            classicShortcut("Align X", InputKey.X, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryAlign(slider, points, true))),
                            classicShortcut("Align Y", InputKey.V, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryAlign(slider, points, false))),
                            classicShortcut("Simplify linear segment", InputKey.Q, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TrySimplifyLinear(slider, points))),
                            classicShortcutCombo("Distribute X", new[] { InputKey.Shift, InputKey.X }, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryDistribute(slider, points, true))),
                            classicShortcutCombo("Distribute Y", new[] { InputKey.Shift, InputKey.V }, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryDistribute(slider, points, false))),
                            classicShortcut("Mirror X", InputKey.M, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryMirror(slider, points, true))),
                            classicShortcut("Mirror Y", InputKey.N, () => applyClassicOperation((slider, points) => ClassicSliderCurveOperations.TryMirror(slider, points, false)))
                        }
                    });
                }

                if (splittableCount > 0)
                {
                    menuItems.Add(new OsuMenuItem($"Split {"control point".ToQuantity(splittableCount, splittableCount > 1 ? ShowQuantityAs.Numeric : ShowQuantityAs.None)}",
                        MenuItemType.Destructive,
                        () => splitSelected()));
                }

                menuItems.Add(
                    new OsuMenuItem($"Delete {"control point".ToQuantity(count, count > 1 ? ShowQuantityAs.Numeric : ShowQuantityAs.None)}",
                        MenuItemType.Destructive,
                        () => DeleteSelected())
                );

                updateCurveMenuItems();

                return menuItems.ToArray();

                CurveTypeMenuItem createMenuItemForPathType(PathType? type, InputKey? key = null)
                {
                    Hotkey hotkey = default;

                    if (key != null)
                        hotkey = new Hotkey(new KeyCombination(InputKey.Alt, key.Value));

                    return new CurveTypeMenuItem(type, _ => updatePathTypeOfSelectedPieces(type)) { Hotkey = hotkey };
                }

                OsuMenuItem classicShortcut(string text, InputKey key, Action action)
                    => new OsuMenuItem(text, MenuItemType.Standard, action) { Hotkey = new Hotkey(new KeyCombination(InputKey.Alt, key)) };

                OsuMenuItem classicShortcutCombo(string text, InputKey[] keys, Action action)
                    => new OsuMenuItem(text, MenuItemType.Standard, action) { Hotkey = new Hotkey(new KeyCombination(new[] { InputKey.Alt }.Concat(keys).ToArray())) };
            }
        }

        private bool applyClassicOperation(Func<Slider, IReadOnlyCollection<PathControlPoint>, bool> operation)
        {
            if (hitObject is not Slider slider)
                return false;

            var selected = Pieces.Where(p => p.IsSelected.Value).Select(p => p.ControlPoint).ToArray();
            if (selected.Length == 0)
                return false;

            double originalDistance = hitObject.Path.Distance;
            double originalVelocity = hitObject.SliderVelocityMultiplier;
            double? expectedDistance = hitObject.Path.ExpectedDistance.Value;
            var originalPoints = hitObject.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            changeHandler?.BeginChange();
            try
            {
                if (!operation(slider, selected))
                    return false;

                // Topology tools can split a perfect-curve segment at an arbitrary inherited
                // point. Normalise invalid perfect-curve arities immediately so the path remains
                // valid before duration/snap calculations and before new pieces are displayed.
                EnsureValidPathTypes();

                if (keepDuration.Value)
                {
                    if (!SliderDurationPreserver.TryApply(slider, originalDistance, originalVelocity, editorBeatmap, out _)
                        || !hitObject.Path.HasValidLengthForPlacement)
                    {
                        hitObject.Path.ExpectedDistance.Value = expectedDistance;
                        hitObject.SliderVelocityMultiplier = originalVelocity;
                        hitObject.Path.ControlPoints.ReplaceRange(0, hitObject.Path.ControlPoints.Count, originalPoints);
                        if (editorBeatmap != null)
                            slider.UpdateVelocity(editorBeatmap.ControlPointInfo, editorBeatmap.Difficulty);
                        return false;
                    }
                }
                else
                    hitObject.SnapTo(distanceSnapProvider);

                return true;
            }
            finally
            {
                changeHandler?.EndChange();
            }
        }

        private void updateCurveMenuItems()
        {
            if (curveTypeItems == null)
                return;

            foreach (var item in curveTypeItems.OfType<CurveTypeMenuItem>())
            {
                int totalCount = Pieces.Count(p => p.IsSelected.Value);
                int countOfState = Pieces.Where(p => p.IsSelected.Value).Count(p => p.ControlPoint.Type == item.PathType);

                if (countOfState == totalCount)
                    item.State.Value = TernaryState.True;
                else if (countOfState > 0)
                    item.State.Value = TernaryState.Indeterminate;
                else
                    item.State.Value = TernaryState.False;
            }
        }

        private class CurveTypeMenuItem : TernaryStateRadioMenuItem
        {
            public readonly PathType? PathType;

            public CurveTypeMenuItem(PathType? pathType, Action<TernaryState> action)
                : base(pathType?.Description ?? "Inherit", MenuItemType.Standard, action)
            {
                PathType = pathType;
            }
        }
    }
}
