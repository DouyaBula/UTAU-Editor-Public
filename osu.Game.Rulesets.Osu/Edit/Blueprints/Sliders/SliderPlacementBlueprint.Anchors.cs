// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Curves;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderPlacementBlueprint
    {
        private readonly Bindable<bool> anchorPlacementMode = new BindableBool();
        private readonly Bindable<bool> curvaturePenEnabled = new BindableBool();
        private bool anchorPlacementEnabled => anchorPlacementMode.Value;
        private readonly AnchorPath penPath = new AnchorPath();
        private Container penMarkers = null!;
        private Vector2 penCursor;
        private bool penDragging;
        private bool penMovingAnchor;
        private Vector2 penDragStartAnchor;
        private Vector2 penDragStartLocal;
        private Box? penCursorMarker;
        private int renderedMarkerAnchorCount = -1;
        private bool penPreviewInitialised;

        private void loadAnchorPlacement(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.EditorAnchorMode, anchorPlacementMode);
            config.BindWith(OsuSetting.EditorAnchorCurvaturePen, curvaturePenEnabled);
            AddInternal(penMarkers = new Container { RelativeSizeAxes = Axes.Both });
            controlPointVisualiser.Alpha = anchorPlacementEnabled ? 0 : 1;
            controlPointVisualiser.InputEnabled = !anchorPlacementEnabled;
            anchorPlacementMode.BindValueChanged(_ =>
            {
                // An unfinished construction has no committed geometry. Switching abandons that preview only.
                if (PlacementActive == PlacementState.Active)
                    EndPlacement(false);
                clearPenState();
                controlPointVisualiser.Alpha = anchorPlacementEnabled ? 0 : 1;
                controlPointVisualiser.InputEnabled = !anchorPlacementEnabled;
            });
        }

        private void clearPenState()
        {
            penPath.Anchors.Clear();
            penPath.Closed = false;
            penDragging = false;
            penMovingAnchor = false;
            penMarkers.Clear();
            penCursorMarker = null;
            renderedMarkerAnchorCount = -1;
            penPreviewInitialised = false;
        }

        private void updateAnchorPlacement(SnapResult snap)
        {
            if (PlacementActive == PlacementState.Waiting && penPath.Anchors.Count > 0)
            {
                // Global Back cancels through PlacementBlueprint. Clear our editor-only state before the next hover.
                clearPenState();
            }

            // Capture the previous head before the waiting-state hover moves it. This
            // comparison drives the initial-point preview; capturing it afterwards
            // makes the comparison tautologically false and leaves the first point
            // visually stuck at the position of the first frame.
            var previousCursor = penCursor;
            var previousHeadPosition = HitObject.Position;

            if (penPath.Anchors.Count == 0 && PlacementActive == PlacementState.Waiting)
            {
                BeginPlacement();
                HitObject.Position = ToLocalSpace(snap.ScreenSpacePosition);
                HitObject.SliderVelocityMultiplier = sliderVelocityToolbox?.GetPlacementVelocity(HitObject.StartTime) ?? 1;
                ApplyDefaultsToHitObject();
            }
            penCursor = ToLocalSpace(snap.ScreenSpacePosition) - HitObject.Position;
            // Before the first click the head itself follows the snapped cursor, so the local
            // pen cursor remains zero. Rebuild when the snapped head actually changes in that
            // state; comparing only the relative cursor would leave the initial point visually
            // stuck at its first frame.
            if (!penDragging && ((penPath.Anchors.Count == 0 && HitObject.Position != previousHeadPosition)
                                 || !penPreviewInitialised || penCursor != previousCursor))
                rebuildPenPreview(true);
        }

        private SnapResult snapAnchor(Vector2 screenPosition)
        {
            var result = composer?.TrySnapToNearbyObjects(screenPosition, HitObject.StartTime);
            result ??= composer?.TrySnapToDistanceGrid(screenPosition, limitedDistanceSnap.Value && editorClock != null ? editorClock.CurrentTime : null);
            result ??= composer?.TrySnapToPositionGrid(screenPosition, HitObject.StartTime);
            return result ?? new SnapResult(screenPosition, HitObject.StartTime);
        }

        private bool anchorMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left) return false;
            var snapped = snapAnchor(e.ScreenSpaceMousePosition);
            if (penPath.Anchors.Count == 0)
            {
                // Re-snap at click time. A frame of stale hover state must never move the slider head off-grid.
                HitObject.Position = ToLocalSpace(snapped.ScreenSpacePosition);
                if (snapped.Time.HasValue)
                    HitObject.StartTime = snapped.Time.Value;
                penCursor = Vector2.Zero;
                if (PlacementActive == PlacementState.Waiting)
                    BeginPlacement(commitStart: true);
            }
            else
                penCursor = ToLocalSpace(snapped.ScreenSpacePosition) - HitObject.Position;
            if (penPath.Anchors.Count >= AnchorPath.MaxAnchors) return true;
            if (penPath.Anchors.Count == 0)
            {
                penPath.Anchors.Add(new CurveAnchor(Vector2.Zero));
            }
            else
            {
                if (penPath.Anchors.Count >= 3 && Vector2.DistanceSquared(penCursor, penPath.Anchors[0].Position) < 16)
                {
                    penPath.Closed = true;
                    finishAnchorPlacement();
                    return true;
                }
                var position = penCursor;
                if (e.ShiftPressed)
                {
                    var last = penPath.Anchors[^1].Position;
                    position = last + AnchorPathOverlay.ConstrainAngle(position - last);
                }
                if (Vector2.DistanceSquared(position, penPath.Anchors[^1].Position) < 0.01f) return true;
                penPath.Anchors.Add(new CurveAnchor(position));
            }
            // With the curvature pen disabled, a click creates a true corner.
            // Explicitly enabling it retains the opt-in auto-smoothing workflow.
            if (curvaturePenEnabled.Value)
            {
                penPath.Anchors[^1].Kind = AnchorKind.AutoSmooth;
                penPath.RecalculateAutoSmooth();
            }
            else
                penPath.Anchors[^1].Kind = AnchorKind.Corner;
            rebuildPenPreview(false);
            return true;
        }

        private bool anchorDragStart(DragStartEvent e)
        {
            penDragging = e.Button == MouseButton.Left && penPath.Anchors.Count > 0;
            penMovingAnchor = penDragging && e.ControlPressed;
            if (penMovingAnchor)
            {
                penDragStartAnchor = penPath.Anchors[^1].Position;
                penDragStartLocal = ToLocalSpace(e.ScreenSpaceMouseDownPosition) - HitObject.Position;
            }
            return penDragging;
        }

        private void anchorDrag(DragEvent e)
        {
            if (!penDragging) return;
            var a = penPath.Anchors[^1];
            if (penMovingAnchor)
            {
                var delta = ToLocalSpace(e.ScreenSpaceMousePosition) - HitObject.Position - penDragStartLocal;
                if (e.ShiftPressed) delta = AnchorPathOverlay.ConstrainAngle(delta);

                if (penPath.Anchors.Count == 1)
                {
                    // Keep the native slider head at the first anchor. This makes the
                    // initial point movable without leaving a non-zero path origin behind.
                    HitObject.Position += delta;
                    a.Position = Vector2.Zero;
                }
                else
                    a.Position = penDragStartAnchor + delta;

                penCursor = ToLocalSpace(e.ScreenSpaceMousePosition) - HitObject.Position;
                rebuildPenPreview(false);
                return;
            }
            var offset = ToLocalSpace(e.ScreenSpaceMousePosition) - HitObject.Position - a.Position;
            if (e.ShiftPressed) offset = AnchorPathOverlay.ConstrainAngle(offset);
            a.Kind = e.AltPressed ? AnchorKind.Corner : AnchorKind.Symmetric;
            a.Outgoing = offset;
            if (!e.AltPressed) a.Incoming = -offset;
            rebuildPenPreview(false);
        }

        private void anchorDragEnd()
        {
            penDragging = false;
            penMovingAnchor = false;
        }

        private bool anchorKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            if (e.AltPressed && !e.ControlPressed && !e.ShiftPressed && e.Key == Key.P)
            {
                anchorPlacementMode.Value = false;
                return true;
            }

            // Alt+arrows are mapped to timeline/bookmark actions globally. While a
            // pen construction is active they must not move the editor clock; Alt
            // remains available to the mouse gesture for independent handles.
            if (PlacementActive == PlacementState.Active && e.AltPressed && !e.ControlPressed
                && e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
                return true;

            if (e.ControlPressed || e.AltPressed) return false;
            switch (e.Key)
            {
                case Key.Escape:
                    if (PlacementActive != PlacementState.Active && penPath.Anchors.Count == 0)
                        return false;
                    EndPlacement(false);
                    clearPenState();
                    return true;
                case Key.Enter: finishAnchorPlacement(); return true;
                case Key.BackSpace:
                    if (penPath.Anchors.Count <= 1) { EndPlacement(false); clearPenState(); }
                    else { penPath.Anchors.RemoveAt(penPath.Anchors.Count - 1); penPath.RecalculateAutoSmooth(); rebuildPenPreview(true); }
                    return true;
                case Key.C:
                    // Slider construction owns C while the pen is active; otherwise the editor
                    // interprets it as play/pause. L remains available to the surrounding editor.
                    curvaturePenEnabled.Value = !curvaturePenEnabled.Value;
                    renderedMarkerAnchorCount = -1;
                    rebuildPenPreview(true);
                    return true;
            }
            return false;
        }

        private void finishAnchorPlacement()
        {
            if (penPath.Anchors.Count < 2)
            {
                EndPlacement(false);
                clearPenState();
                return;
            }
            rebuildPenPreview(false);
            // Apply the same final distance snap as the official control-point placement path.
            // Keeping this at commit time also covers a final click that arrived between hover
            // frames and therefore never passed through updateAnchorPlacement().
            HitObject.Path.ExpectedDistance.Value = distanceSnapProvider?.FindSnappedDistance(
                (float)HitObject.Path.CalculatedDistance, HitObject.StartTime, HitObject)
                ?? HitObject.Path.CalculatedDistance;
            ApplyDefaultsToHitObject();
            AnchorEditSession.Remember(HitObject, penPath);
            EndPlacement(true);
        }

        private void rebuildPenPreview(bool rubberBand)
        {
            var preview = penPath.Clone();
            if (rubberBand && preview.Anchors.Count > 0 && !preview.Closed)
            {
                preview.Anchors.Add(new CurveAnchor(penCursor) { Kind = curvaturePenEnabled.Value ? AnchorKind.AutoSmooth : AnchorKind.Corner });
                if (curvaturePenEnabled.Value) preview.RecalculateAutoSmooth();
            }
            if (preview.Anchors.Count > 0)
            {
                preview.WriteTo(HitObject.Path);
                // Match classic placement: the visible slider length follows the distance snap
                // while the path is being constructed, rather than only snapping after the fact.
                HitObject.Path.ExpectedDistance.Value = distanceSnapProvider?.FindSnappedDistance(
                    (float)HitObject.Path.CalculatedDistance, HitObject.StartTime, HitObject)
                    ?? HitObject.Path.CalculatedDistance;
            }
            bodyPiece.UpdateFrom(HitObject);
            headCirclePiece.UpdateFrom(HitObject.HeadCircle);
            tailCirclePiece.UpdateFrom(HitObject.TailCircle);
            penMarkers.Position = HitObject.Position;
            if (!rubberBand || renderedMarkerAnchorCount != penPath.Anchors.Count)
            {
                penMarkers.Clear();
                foreach (var a in penPath.Anchors)
                {
                    penMarkers.Add(new Box { Position = a.Position, Origin = Anchor.Centre, Size = new Vector2(6), Colour = Colour4.Cyan });
                    foreach (var offset in new[] { a.Incoming, a.Outgoing }.Where(v => v.LengthSquared > 0.001f))
                    {
                        penMarkers.Add(new Box { Position = a.Position, Size = new Vector2(offset.Length, 1), Rotation = System.MathF.Atan2(offset.Y, offset.X) * 180 / System.MathF.PI, Colour = Colour4.Yellow });
                        penMarkers.Add(new Circle { Position = a.Position + offset, Origin = Anchor.Centre, Size = new Vector2(5), Colour = Colour4.Yellow });
                    }
                }
                penCursorMarker = new Box { Origin = Anchor.Centre, Size = new Vector2(6), Colour = Colour4.Cyan };
                penMarkers.Add(penCursorMarker);
                renderedMarkerAnchorCount = penPath.Anchors.Count;
            }
            if (penCursorMarker != null)
            {
                penCursorMarker.Position = penCursor;
                penCursorMarker.Alpha = rubberBand && !preview.Closed ? 1 : 0;
            }

            penPreviewInitialised = true;
        }
    }
}
