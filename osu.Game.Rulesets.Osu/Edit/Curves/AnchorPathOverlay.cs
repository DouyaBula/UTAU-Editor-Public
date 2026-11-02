// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Curves
{
    /// <summary>Direct on-playfield node editing. All edits are native slider edits in one undo transaction per gesture.</summary>
    public partial class AnchorPathOverlay : CompositeDrawable, IHasContextMenu, IHasTooltip, IKeyBindingHandler<PlatformAction>, IKeyBindingHandler<GlobalAction>
    {
        private readonly Slider slider;
        private readonly Container visuals = new Container { RelativeSizeAxes = Axes.Both };
        private readonly Container editVisuals = new Container { RelativeSizeAxes = Axes.Both };
        private readonly Container handleVisuals = new Container { RelativeSizeAxes = Axes.Both };
        private readonly Container anchorVisuals = new Container { RelativeSizeAxes = Axes.Both };
        private readonly SmoothPath curveVisual = new SmoothPath { PathRadius = 0.6f, Colour = Colour4.Cyan };
        private readonly Box symmetryAxisVisual = new Box { Colour = Colour4.Yellow, Alpha = 0.0f };
        private readonly List<(Box line, Circle tip)> handlePairs = new List<(Box line, Circle tip)>();
        private readonly List<Box> anchorNodes = new List<Box>();
        private readonly HashSet<int> selected = new HashSet<int>();
        private AnchorPath? model;
        private AnchorPath? dragStart;
        private int version = -1;
        private int dragAnchor = -1;
        private int dragHandle;
        private int quickDeleteAnchor = -1;
        private int quickDeleteHandle;
        private int dragSegment = -1;
        private float dragT;
        private Vector2 downPosition;
        private Vector2 initialStackedPosition;
        private Vector2 initialPosition;
        private bool dragging;
        private bool dirty = true;
        private CurveMirror liveMirror;
        private float mirrorCentre;
        private Dictionary<int, int> mirrorPartners = new Dictionary<int, int>();
        private AnchorPath? pendingDrag;
        private string message = string.Empty;
        private Bindable<bool> anchorMode = null!;
        private Bindable<bool> preserveDuration = null!;
        private Bindable<float> cornerAmount = null!;
        private AnchorPath? hitTestModel;
        private Vector2 hitTestOrigin;
        private Vector2 hitTestXAxis;
        private Vector2 hitTestYAxis;
        private readonly List<Vector2> hitTestSamples = new List<Vector2>();

        [Resolved] private EditorBeatmap beatmap { get; set; } = null!;
        [Resolved] private IEditorChangeHandler changes { get; set; } = null!;
        [Resolved] private OsuHitObjectComposer composer { get; set; } = null!;

        public AnchorPathOverlay(Slider slider)
        {
            this.slider = slider;
            // Initialise synchronously. The overlay is swapped in from
            // SliderSelectionBlueprint and can receive a pointer event before the
            // next frame's Update() has had a chance to parse the native path. If
            // model stayed null for that first event, the parent selection blueprint
            // could handle the click and insert classic control points instead.
            model = AnchorEditSession.TryRead(slider, out var initial) ? initial : null;
            version = slider.Path.Version.Value;
            RelativeSizeAxes = Axes.Both;
            // Above head/tail overlays so an anchor is grabbed before the whole object.
            Depth = -10;
            InternalChild = visuals;
            visuals.Add(curveVisual);
            visuals.Add(symmetryAxisVisual);
            visuals.Add(editVisuals);
            editVisuals.Add(handleVisuals);
            editVisuals.Add(anchorVisuals);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            anchorMode = config.GetBindable<bool>(OsuSetting.EditorAnchorMode);
            preserveDuration = config.GetBindable<bool>(OsuSetting.EditorAnchorKeepDuration);
            cornerAmount = config.GetBindable<float>(OsuSetting.EditorAnchorCornerAmount);
        }

        protected override void Update()
        {
            base.Update();
            visuals.Position = slider.StackedPosition;

            // Pointer events can arrive several times per frame. Coalesce geometry writes so
            // native path invalidation and distance calculation happen at most once per frame.
            if (pendingDrag != null && dragging)
            {
                var pending = pendingDrag;
                pendingDrag = null;
                apply(pending);
            }

            if (version != slider.Path.Version.Value)
            {
                version = slider.Path.Version.Value;
                model = AnchorEditSession.TryRead(slider, out var read) ? read : null;
                selected.RemoveWhere(i => model == null || i >= model.Anchors.Count);
                dirty = true;
            }
            if (dirty) redraw();
        }

        public LocalisableString TooltipText => message.Length > 0 ? message : model == null
            ? "Anchor mode edits pure Bézier sliders only. This slider remains in classic control-point mode."
            : "Drag anchors or curve. Ctrl-click: multi-select; Shift-drag: constrain; Alt-drag: independent handle; Alt while moving the whole slider: distance snap. Double-click curve: insert anchor. Right-click: node tools.";

        private Vector2 local(Vector2 screen) => ToLocalSpace(screen) - slider.StackedPosition;

        private (int index, int handle) pick(Vector2 screen)
        {
            if (model == null) return (-1, 0);
            // Handle hit radii use screen pixels, remaining usable at any canvas zoom.
            for (int i = model.Anchors.Count - 1; i >= 0; i--)
            {
                var a = model.Anchors[i];
                for (int part = 0; part < 3; part++)
                {
                    int h = part == 1 ? -1 : part == 2 ? 1 : 0;
                    var offset = h == -1 ? a.Incoming : h == 1 ? a.Outgoing : Vector2.Zero;
                    if (h != 0 && (offset.LengthSquared < 0.001f || !selected.Contains(i))) continue;
                    if (Vector2.DistanceSquared(screen, ToScreenSpace(slider.StackedPosition + a.Position + offset)) <= 81)
                        return (i, h);
                }
            }
            return (-1, 0);
        }

        private (int segment, float t, float distance) nearest(Vector2 screen)
        {
            int best = -1;
            float bestT = 0, distance = 64;
            if (model == null) return (best, bestT, distance);
            updateHitTestSamples();
            for (int s = 0; s < model.SegmentCount; s++)
            {
                Vector2 prev = hitTestSamples[s * 33];
                for (int j = 1; j <= 32; j++)
                {
                    Vector2 next = hitTestSamples[s * 33 + j];
                    var d = next - prev;
                    float u = d.LengthSquared < 0.0001f ? 0 : Math.Clamp(Vector2.Dot(screen - prev, d) / d.LengthSquared, 0, 1);
                    float ds = Vector2.DistanceSquared(screen, prev + u * d);
                    if (ds < distance) { distance = ds; best = s; bestT = (j - 1 + u) / 32; }
                    prev = next;
                }
            }
            return (best, bestT, distance);
        }

        private void updateHitTestSamples()
        {
            Vector2 origin = ToScreenSpace(slider.StackedPosition);
            Vector2 xAxis = ToScreenSpace(slider.StackedPosition + Vector2.UnitX) - origin;
            Vector2 yAxis = ToScreenSpace(slider.StackedPosition + Vector2.UnitY) - origin;
            if (ReferenceEquals(hitTestModel, model) && origin == hitTestOrigin && xAxis == hitTestXAxis && yAxis == hitTestYAxis)
                return;

            hitTestModel = model;
            hitTestOrigin = origin;
            hitTestXAxis = xAxis;
            hitTestYAxis = yAxis;
            hitTestSamples.Clear();
            for (int segment = 0; segment < model!.SegmentCount; segment++)
            {
                for (int step = 0; step <= 32; step++)
                {
                    Vector2 point = model.Evaluate(segment, step / 32f);
                    hitTestSamples.Add(origin + point.X * xAxis + point.Y * yAxis);
                }
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            => model != null && (pick(screenSpacePos).index >= 0 || nearest(screenSpacePos).segment >= 0);

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (model == null || (e.Button != MouseButton.Left && e.Button != MouseButton.Right)) return false;
            var hit = pick(e.ScreenSpaceMousePosition);
            quickDeleteAnchor = -1;
            quickDeleteHandle = 0;
            if (hit.index >= 0)
            {
                if (e.ControlPressed && e.Button == MouseButton.Left && hit.handle == 0)
                {
                    if (!selected.Add(hit.index)) selected.Remove(hit.index);
                }
                else if (!selected.Contains(hit.index))
                {
                    selected.Clear(); selected.Add(hit.index);
                }
                dragAnchor = hit.index; dragHandle = hit.handle; dragSegment = -1;
                if (e.AltPressed && hit.handle == 0) dragHandle = 1;
                if (e.Button == MouseButton.Right && e.ShiftPressed && hit.handle != 0)
                {
                    quickDeleteAnchor = hit.index;
                    quickDeleteHandle = hit.handle;
                    // Consume the gesture here. The outer selection handler's Shift+right-click
                    // path is node-oriented and may otherwise delete the slider/anchor before it
                    // can see that the pointer was on a direction handle.
                    DeleteQuickTarget();
                    return true;
                }
            }
            else
            {
                var near = nearest(e.ScreenSpaceMousePosition);
                dragSegment = near.segment; dragT = near.t; dragAnchor = -1;
                if (e.ControlPressed && e.Button == MouseButton.Left && dragSegment >= 0)
                {
                    insert(dragSegment, dragT);
                    return true;
                }
            }
            dirty = true;
            downPosition = local(e.ScreenSpaceMousePosition);
            return e.Button == MouseButton.Left;
        }

        protected override bool OnClick(ClickEvent e)
            // osu!framework only sends DoubleClickEvent to drawables that accepted the first click.
            => model != null && e.Button == MouseButton.Left;

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (model == null) return false;
            // Two quick Ctrl-clicks are a valid multi-selection gesture. Do not let
            // the framework's double-click synthesis turn that gesture into a node
            // type toggle or curve insertion as a side effect.
            if (e.ControlPressed) return true;
            var hit = pick(e.ScreenSpaceMousePosition);
            if (hit.index >= 0)
                edit(p => p.SetKind(hit.index, p.Anchors[hit.index].Kind == AnchorKind.Corner ? AnchorKind.AutoSmooth : AnchorKind.Corner));
            else
            {
                var near = nearest(e.ScreenSpaceMousePosition);
                if (near.segment < 0) return false;
                insert(near.segment, near.t);
            }
            return true;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button != MouseButton.Left || model == null || (dragAnchor < 0 && dragSegment < 0)) return false;
            dragStart = model.Clone();
            initialStackedPosition = slider.StackedPosition;
            initialPosition = slider.Position;
            mirrorPartners = findMirrorPartners(dragStart);
            AnchorEditSession.Remember(slider, model);
            changes.BeginChange();
            dragging = true;
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            if (!dragging || dragStart == null) return;
            var candidate = dragStart.Clone();
            var mouse = ToLocalSpace(e.ScreenSpaceMousePosition) - initialStackedPosition;
            if (dragAnchor >= 0)
            {
                if (dragHandle == 0)
                {
                    var snap = composer.TrySnapToPositionGrid(e.ScreenSpaceMousePosition);
                    if (snap != null) mouse = ToLocalSpace(snap.ScreenSpacePosition) - initialStackedPosition;
                    var delta = mouse - downPosition;
                    if (e.ShiftPressed) delta = Math.Abs(delta.X) > Math.Abs(delta.Y) ? new Vector2(delta.X, 0) : new Vector2(0, delta.Y);
                    foreach (int i in selected) candidate.Anchors[i].Position += delta;
                    candidate.RecalculateAutoSmooth();
                }
                else
                {
                    var offset = mouse - candidate.Anchors[dragAnchor].Position;
                    if (e.ShiftPressed) offset = ConstrainAngle(offset);
                    candidate.MoveHandle(dragAnchor, dragHandle == 1, offset, e.AltPressed);
                }
                foreach (int i in selected)
                {
                    if (!mirrorPartners.TryGetValue(i, out int partner) || selected.Contains(partner)) continue;
                    var a = candidate.Anchors[i]; var b = candidate.Anchors[partner];
                    b.Position = AnchorPath.Reflect(a.Position, liveMirror, mirrorCentre - axisPosition(initialPosition));
                    // Partner handles are reversed because reflected closed contours run in opposite directions.
                    b.Incoming = AnchorPath.Reflect(a.Outgoing, liveMirror, 0);
                    b.Outgoing = AnchorPath.Reflect(a.Incoming, liveMirror, 0);
                    b.Kind = a.Kind;
                }
            }
            else
            {
                // Least-norm handle displacement that moves B(t) by the mouse delta, keeping both anchors fixed.
                float t = Math.Clamp(dragT, 0.05f, 0.95f), u = 1 - t;
                float w1 = 3 * u * u * t, w2 = 3 * u * t * t, denominator = w1 * w1 + w2 * w2;
                var delta = mouse - downPosition;
                var a = candidate.Anchors[dragSegment]; var b = candidate.Anchors[(dragSegment + 1) % candidate.Anchors.Count];
                a.Outgoing += delta * w1 / denominator; b.Incoming += delta * w2 / denominator;
                a.Kind = b.Kind = AnchorKind.Corner;
            }
            // The baseline coordinates must remain fixed even when dragging the slider head.
            foreach (var a in candidate.Anchors) a.Position += initialPosition - slider.Position;
            pendingDrag = candidate;
            dirty = true;
        }

        public static Vector2 ConstrainAngle(Vector2 vector)
        {
            float angle = MathF.Round(MathF.Atan2(vector.Y, vector.X) / (MathF.PI / 12)) * (MathF.PI / 12);
            return vector.Length * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        protected override void OnDragEnd(DragEndEvent e) => finishDrag();
        public void FinishInteraction() => finishDrag();

        private void finishDrag()
        {
            if (!dragging) return;
            bool committedPending = false;
            if (pendingDrag != null)
            {
                var pending = pendingDrag;
                pendingDrag = null;
                // The final pointer sample is the one that should create the undoable state.
                committedPending = apply(pending, true);
            }
            if (!committedPending && model != null)
                apply(model, true);
            dragging = false; dragStart = null;
            changes.EndChange();
        }

        private bool apply(AnchorPath candidate, bool rebuildDefaults = false)
        {
            if (!AnchorEditSession.TryApply(slider, candidate, beatmap, preserveDuration.Value, out message,
                                             rebuildDefaults, rebuildDefaults, rebuildDefaults)) return false;
            // TryApply normalises the first anchor into Slider.Position. Keep the same
            // normalised model locally instead of reparsing the native path on every pointer
            // event (the latter was a major source of pen-mode stalls).
            Vector2 origin = candidate.Anchors[0].Position;
            foreach (var anchor in candidate.Anchors)
                anchor.Position -= origin;
            model = candidate;
            version = slider.Path.Version.Value;
            dirty = true;
            return true;
        }

        private void edit(Action<AnchorPath> action)
        {
            if (model == null || dragging) return;
            var candidate = model.Clone();
            action(candidate);
            AnchorEditSession.Remember(slider, model);
            changes.BeginChange();
            try { apply(candidate, true); }
            finally { changes.EndChange(); }
            selected.RemoveWhere(i => model == null || i >= model.Anchors.Count);
            dirty = true;
        }

        private void insert(int segment, float t)
        {
            edit(p =>
            {
                int i = p.Split(segment, t);
                if (i >= 0) { selected.Clear(); selected.Add(i); }
            });
        }

        public bool DeleteSelected()
        {
            if (model == null || selected.Count == 0) return false;
            if (model.Anchors.Count - selected.Count < (model.Closed ? 3 : 2))
            {
                message = "Keep at least two anchors (three for a closed path). Deselect nodes to delete the whole slider.";
                return true;
            }
            edit(p =>
            {
                foreach (int i in selected.OrderDescending()) p.Anchors.RemoveAt(i);
                p.RecalculateAutoSmooth();
            });
            selected.Clear();
            return true;
        }

        /// <summary>
        /// Handles Shift+right-click quick deletion when the pointer is over a direction handle.
        /// A handle is geometry, not a node: remove only the selected side and make the node a
        /// corner so a smoothing rule cannot immediately recreate the deleted handle.
        /// </summary>
        public bool DeleteQuickTarget()
        {
            int anchor = quickDeleteAnchor;
            int handle = quickDeleteHandle;
            quickDeleteAnchor = -1;
            quickDeleteHandle = 0;

            if (model == null || anchor < 0 || handle == 0 || anchor >= model.Anchors.Count)
                return false;

            edit(p =>
            {
                var a = p.Anchors[anchor];
                if (handle < 0)
                    a.Incoming = Vector2.Zero;
                else
                    a.Outgoing = Vector2.Zero;
                a.Kind = AnchorKind.Corner;
            });
            selected.Clear();
            selected.Add(anchor);
            return true;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // A selected anchor remains the active editing target even if the pointer
            // briefly leaves the overlay. This keeps C/Delete/Alt-arrow ownership
            // stable while still allowing an unselected slider to use global editor
            // shortcuts normally.
            if ((!IsHovered && selected.Count == 0) || model == null || dragging) return false;

            if (!e.Repeat && !e.ControlPressed && !e.SuperPressed)
            {
                if (e.AltPressed)
                {
                    switch (e.Key)
                    {
                        case Key.P:
                            anchorMode.Value = !anchorMode.Value;
                            return true;

                        case Key.X:
                            if (selected.Count > 0) edit(p => arrangeSelected(p, true, e.ShiftPressed));
                            return true;

                        case Key.Y:
                            if (selected.Count > 0) edit(p => arrangeSelected(p, false, e.ShiftPressed));
                            return true;

                        case Key.H:
                            if (selected.Count > 0) edit(p => p.Mirror(CurveMirror.Horizontal, centre(p, CurveMirror.Horizontal)));
                            return true;

                        case Key.V:
                            if (selected.Count > 0) edit(p => p.Mirror(CurveMirror.Vertical, centre(p, CurveMirror.Vertical)));
                            return true;

                        case Key.M:
                            setLiveMirror(CurveMirror.Horizontal);
                            return true;

                        case Key.N:
                            setLiveMirror(CurveMirror.Vertical);
                            return true;

                        case Key.Number0:
                            liveMirror = CurveMirror.None;
                            message = string.Empty;
                            dirty = true;
                            return true;
                    }
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.C:
                            if (selected.Count > 0)
                                edit(p => foreachSelected(p, i => p.SetKind(i, e.ShiftPressed ? AnchorKind.Corner : p.Anchors[i].Kind == AnchorKind.Corner ? AnchorKind.Smooth : AnchorKind.Corner)));
                            return true;

                        case Key.S:
                            if (selected.Count > 0)
                                edit(p => foreachSelected(p, i => p.SetKind(i, e.ShiftPressed ? AnchorKind.Smooth : AnchorKind.Symmetric)));
                            return true;

                        case Key.A:
                            if (selected.Count > 0) edit(p => foreachSelected(p, i => p.SetKind(i, AnchorKind.AutoSmooth)));
                            return true;

                        case Key.R:
                            if (selected.Count > 0) round(false);
                            return true;

                        case Key.B:
                            if (selected.Count > 0) round(true);
                            return true;

                        case Key.H:
                            if (selected.Count > 0) edit(p => foreachSelected(p, i =>
                            {
                                p.Anchors[i].Incoming = p.Anchors[i].Outgoing = Vector2.Zero;
                                p.Anchors[i].Kind = AnchorKind.Corner;
                            }));
                            return true;

                        case Key.L:
                            if (selected.Count > 1) edit(straightenSelected);
                            return true;

                        case Key.I:
                            if (selected.Count > 0) edit(insertAfterSelected);
                            return true;

                        case Key.O:
                            if (selected.Count > 0) edit(p => { if (p.Anchors.Count >= 3) p.Closed = !p.Closed; });
                            return true;

                        case Key.V:
                            if (selected.Count > 0) edit(p => p.Reverse());
                            return true;

                        case Key.Delete:
                            DeleteSelected();
                            return true;
                    }
                }
            }

            if (e.Key == Key.Escape) { selected.Clear(); dirty = true; return true; }
            // Keep global select-all and normal arrow navigation untouched. Alt+arrows are local node nudges.
            if (selected.Count == 0 || !e.AltPressed || e.ControlPressed) return false;
            Vector2 delta = e.Key switch
            {
                Key.Left => new Vector2(-1, 0),
                Key.Right => new Vector2(1, 0),
                Key.Up => new Vector2(0, -1),
                Key.Down => new Vector2(0, 1),
                _ => Vector2.Zero
            };
            if (delta == Vector2.Zero) return false;
            edit(p => { foreach (int i in selected) p.Anchors[i].Position += delta * (e.ShiftPressed ? 10 : 1); p.RecalculateAutoSmooth(); });
            return true;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat || e.Action != PlatformAction.Delete || model == null)
                return false;

            // Prevent the outer selection handler from deleting the whole slider while the
            // anchor editor is active. With no selected node this is intentionally a no-op.
            DeleteSelected();
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (model == null || (!IsHovered && selected.Count == 0))
                return false;

            // The global action container handles Alt+arrow before the raw KeyDown event can
            // reach this drawable. Reproduce the local anchor nudge here and consume the
            // action so bookmark/audio navigation does not also run.
            if (!e.CurrentState.Keyboard.AltPressed || e.CurrentState.Keyboard.ControlPressed)
                return false;

            Vector2 delta = e.Action switch
            {
                GlobalAction.EditorSeekToPreviousBookmark or GlobalAction.PreviousVolumeMeter => new Vector2(-1, 0),
                GlobalAction.EditorSeekToNextBookmark or GlobalAction.NextVolumeMeter => new Vector2(1, 0),
                GlobalAction.IncreaseVolume => new Vector2(0, -1),
                GlobalAction.DecreaseVolume => new Vector2(0, 1),
                _ => Vector2.Zero
            };

            if (delta == Vector2.Zero)
                return false;

            edit(p =>
            {
                foreach (int index in selected)
                    if (index >= 0 && index < p.Anchors.Count)
                        p.Anchors[index].Position += delta * (e.CurrentState.Keyboard.ShiftPressed ? 10 : 1);
                p.RecalculateAutoSmooth();
            });
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void foreachSelected(AnchorPath path, Action<int> action)
        {
            foreach (int index in selected.ToArray())
                if (index >= 0 && index < path.Anchors.Count)
                    action(index);
            path.RecalculateAutoSmooth();
        }

        private void straightenSelected(AnchorPath path)
        {
            for (int i = 0; i < path.SegmentCount; i++)
            {
                int next = (i + 1) % path.Anchors.Count;
                if (!selected.Contains(i) || !selected.Contains(next)) continue;
                path.Anchors[i].Outgoing = path.Anchors[next].Incoming = Vector2.Zero;
                path.Anchors[i].Kind = path.Anchors[next].Kind = AnchorKind.Corner;
            }
        }

        private void insertAfterSelected(AnchorPath path)
        {
            foreach (int index in selected.OrderDescending())
                if (index < path.SegmentCount)
                    path.Split(index, 0.5f);
            selected.Clear();
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (model == null) return Array.Empty<MenuItem>();
                var items = new List<MenuItem>
                {
                    new OsuMenuItem("Node type")
                    {
                        Items = new MenuItem[]
                        {
                            shortcut("Toggle corner / smooth", InputKey.C, () => edit(p => foreachSelected(p, i => p.SetKind(i, p.Anchors[i].Kind == AnchorKind.Corner ? AnchorKind.Smooth : AnchorKind.Corner)))),
                            shortcut("Corner", new[] { InputKey.Shift, InputKey.C }, () => edit(p => foreachSelected(p, i => p.SetKind(i, AnchorKind.Corner)))),
                            shortcut("Smooth", new[] { InputKey.Shift, InputKey.S }, () => edit(p => foreachSelected(p, i => p.SetKind(i, AnchorKind.Smooth)))),
                            shortcut("Symmetric", InputKey.S, () => edit(p => foreachSelected(p, i => p.SetKind(i, AnchorKind.Symmetric)))),
                            shortcut("Auto smooth", InputKey.A, () => edit(p => foreachSelected(p, i => p.SetKind(i, AnchorKind.AutoSmooth))))
                        }
                    },
                    new OsuMenuItem("Handles")
                    {
                        Items = new MenuItem[]
                        {
                            shortcut("Retract handles", InputKey.H, () => edit(p => foreachSelected(p, i =>
                            {
                                p.Anchors[i].Incoming = p.Anchors[i].Outgoing = Vector2.Zero;
                                p.Anchors[i].Kind = AnchorKind.Corner;
                            }))),
                            shortcut("Straighten selected segments", InputKey.L, () => edit(straightenSelected))
                        }
                    },
                    new OsuMenuItem("Geometry")
                    {
                        Items = new MenuItem[]
                        {
                            shortcut("Round corners", InputKey.R, () => round(false)),
                            shortcut("Bevel corners", InputKey.B, () => round(true)),
                            shortcut("Insert midpoint", InputKey.I, () => edit(insertAfterSelected)),
                            new OsuMenuItem("Delete anchors", MenuItemType.Destructive, () => DeleteSelected()) { Hotkey = new Hotkey(PlatformAction.Delete) }
                        }
                    },
                    new OsuMenuItem("Arrange")
                    {
                        Items = new MenuItem[]
                        {
                            shortcut("Align X", new[] { InputKey.Alt, InputKey.X }, () => arrange(true, false)),
                            shortcut("Align Y", new[] { InputKey.Alt, InputKey.Y }, () => arrange(false, false)),
                            shortcut("Distribute X", new[] { InputKey.Alt, InputKey.Shift, InputKey.X }, () => arrange(true, true)),
                            shortcut("Distribute Y", new[] { InputKey.Alt, InputKey.Shift, InputKey.Y }, () => arrange(false, true))
                        }
                    },
                    shortcut(model.Closed ? "Open path" : "Close path", InputKey.O,
                        () => edit(p => { if (p.Anchors.Count >= 3) p.Closed = !p.Closed; })),
                    shortcut("Reverse path", InputKey.V, () => { edit(p => p.Reverse()); selected.Clear(); })
                };
                foreach (var axis in new[] { CurveMirror.Horizontal, CurveMirror.Vertical })
                {
                    InputKey flipKey = axis == CurveMirror.Horizontal ? InputKey.H : InputKey.V;
                    InputKey mirrorKey = axis == CurveMirror.Horizontal ? InputKey.M : InputKey.N;
                    items.Add(shortcut($"Flip {axis}", new[] { InputKey.Alt, flipKey }, () => edit(p => p.Mirror(axis, centre(p, axis)))));
                    items.Add(shortcut($"Live symmetry: {axis}", new[] { InputKey.Alt, mirrorKey }, () =>
                    {
                        setLiveMirror(axis);
                    }));
                }
                items.Add(shortcut("Live symmetry: off", new[] { InputKey.Alt, InputKey.Number0 }, () => { liveMirror = CurveMirror.None; dirty = true; message = string.Empty; }));
                return items.ToArray();
            }
        }

        private static OsuMenuItem shortcut(string text, InputKey? key, Action action)
        {
            return key.HasValue
                ? new OsuMenuItem(text, MenuItemType.Standard, action) { Hotkey = new Hotkey(new KeyCombination(key.Value)) }
                : new OsuMenuItem(text, MenuItemType.Standard, action);
        }

        private static OsuMenuItem shortcut(string text, InputKey[] keys, Action action)
            => new OsuMenuItem(text, MenuItemType.Standard, action) { Hotkey = new Hotkey(new KeyCombination(keys)) };

        private void round(bool bevel) => edit(p =>
        {
            foreach (int i in selected.OrderDescending()) p.RoundCorner(i, cornerAmount.Value, bevel);
            selected.Clear();
        });

        private void arrange(bool x, bool distribute)
        {
            if (selected.Count < 2) return;
            edit(p =>
            {
                arrangeSelected(p, x, distribute);
            });
        }

        private void arrangeSelected(AnchorPath path, bool x, bool distribute)
        {
            if (selected.Count < 2) return;
            var indices = selected.OrderBy(i => x ? path.Anchors[i].Position.X : path.Anchors[i].Position.Y).ToArray();
            var values = indices.Select(i => x ? path.Anchors[i].Position.X : path.Anchors[i].Position.Y).ToArray();
            for (int j = 0; j < indices.Length; j++)
            {
                float value = distribute ? values[0] + (values[^1] - values[0]) * j / (indices.Length - 1) : values.Average();
                var a = path.Anchors[indices[j]];
                a.Position = x ? new Vector2(value, a.Position.Y) : new Vector2(a.Position.X, value);
            }
            path.RecalculateAutoSmooth();
        }

        private void setLiveMirror(CurveMirror axis)
        {
            if (model == null || selected.Count == 0) return;
            liveMirror = axis;
            mirrorCentre = centre(model, axis) + axisPosition(slider.Position);
            message = $"Live symmetry {axis}: select one side, then drag a selected anchor. The nearest unpaired anchor across the yellow axis follows.";
            dirty = true;
        }

        private static float centre(AnchorPath p, CurveMirror axis)
        {
            var values = p.Anchors.Select(a => axis == CurveMirror.Horizontal ? a.Position.X : a.Position.Y);
            return (values.Min() + values.Max()) / 2;
        }

        private Dictionary<int, int> findMirrorPartners(AnchorPath p)
        {
            var result = new Dictionary<int, int>();
            if (liveMirror == CurveMirror.None) return result;
            float localCentre = mirrorCentre - axisPosition(slider.Position);
            for (int i = 0; i < p.Anchors.Count; i++)
            {
                if (result.ContainsKey(i)) continue;
                float sourceSide = axisPosition(p.Anchors[i].Position) - localCentre;
                // A node on the axis is already symmetric and must not consume a
                // separate partner. Non-axis nodes may only pair with the opposite
                // side; without this guard a sparse contour could mirror an unrelated
                // node on the same side simply because it was nearest in Euclidean
                // distance.
                if (Math.Abs(sourceSide) < 0.001f) continue;
                var target = AnchorPath.Reflect(p.Anchors[i].Position, liveMirror, mirrorCentre - axisPosition(slider.Position));
                // An anchor already on the symmetry axis is its own mirror and should not
                // consume a different anchor as a partner.
                if (Vector2.DistanceSquared(target, p.Anchors[i].Position) < 1)
                    continue;
                int partner = -1; float distance = float.MaxValue;
                for (int j = 0; j < p.Anchors.Count; j++)
                {
                    if (j == i) continue;
                    if (result.ContainsKey(j)) continue;
                    float partnerSide = axisPosition(p.Anchors[j].Position) - localCentre;
                    if (sourceSide * partnerSide >= 0) continue;
                    float d = Vector2.DistanceSquared(target, p.Anchors[j].Position);
                    if (d < distance) { distance = d; partner = j; }
                }
                if (partner >= 0) { result[i] = partner; result[partner] = i; }
            }
            return result;
        }

        private float axisPosition(Vector2 position) => liveMirror == CurveMirror.Horizontal ? position.X : position.Y;

        private void redraw()
        {
            dirty = false;
            curveVisual.ClearVertices();
            symmetryAxisVisual.Alpha = 0;
            foreach (var pair in handlePairs)
                pair.line.Alpha = pair.tip.Alpha = 0;
            foreach (var node in anchorNodes)
                node.Alpha = 0;
            if (model == null) return;
            // Thin centreline makes the editable curve legible even with a transparent slider skin.
            for (int i = 0; i < model.SegmentCount; i++)
                for (int j = 0; j < 24; j++) curveVisual.AddVertex(model.Evaluate(i, j / 24f));
            if (model.SegmentCount > 0) curveVisual.AddVertex(model.Evaluate(model.SegmentCount - 1, 1));
            curveVisual.OriginPosition = curveVisual.PositionInBoundingBox(Vector2.Zero);
            if (liveMirror != CurveMirror.None)
            {
                float axis = mirrorCentre - axisPosition(slider.Position);
                var min = new Vector2(model.Anchors.Min(a => a.Position.X), model.Anchors.Min(a => a.Position.Y)) - new Vector2(20);
                var max = new Vector2(model.Anchors.Max(a => a.Position.X), model.Anchors.Max(a => a.Position.Y)) + new Vector2(20);
                symmetryAxisVisual.Position = liveMirror == CurveMirror.Horizontal ? new Vector2(axis, min.Y) : new Vector2(min.X, axis);
                symmetryAxisVisual.Size = liveMirror == CurveMirror.Horizontal ? new Vector2(1, max.Y - min.Y) : new Vector2(max.X - min.X, 1);
                symmetryAxisVisual.Alpha = 0.4f;
            }
            int handleIndex = 0;
            int nodeIndex = 0;
            for (int i = 0; i < model.Anchors.Count; i++)
            {
                var a = model.Anchors[i];
                if (selected.Contains(i))
                {
                    for (int side = 0; side < 2; side++)
                    {
                        var offset = side == 0 ? a.Incoming : a.Outgoing;
                        if (offset.LengthSquared < 0.001f) continue;
                        var pair = getHandlePair(handleIndex++);
                        pair.line.Position = a.Position;
                        pair.line.Size = new Vector2(offset.Length, 1);
                        pair.line.Rotation = MathF.Atan2(offset.Y, offset.X) * 180 / MathF.PI;
                        pair.line.Alpha = 1;
                        pair.tip.Position = a.Position + offset;
                        pair.tip.Alpha = 1;
                    }
                }
                var node = getAnchorNode(nodeIndex++);
                node.Position = a.Position;
                node.Size = new Vector2(selected.Contains(i) ? 9 : 6);
                node.Rotation = a.Kind == AnchorKind.Corner ? 45 : 0;
                node.Colour = selected.Contains(i) ? Colour4.White : Colour4.Cyan;
                node.Alpha = 1;
            }
        }

        private (Box line, Circle tip) getHandlePair(int index)
        {
            while (handlePairs.Count <= index)
            {
                var line = new Box { Colour = Colour4.Yellow, Alpha = 0 };
                var tip = new Circle { Origin = Anchor.Centre, Size = new Vector2(6), Colour = Colour4.Yellow, Alpha = 0 };
                handleVisuals.Add(line);
                handleVisuals.Add(tip);
                handlePairs.Add((line, tip));
            }

            return handlePairs[index];
        }

        private Box getAnchorNode(int index)
        {
            while (anchorNodes.Count <= index)
            {
                var node = new Box { Origin = Anchor.Centre, Alpha = 0 };
                anchorVisuals.Add(node);
                anchorNodes.Add(node);
            }

            return anchorNodes[index];
        }

        protected override void Dispose(bool isDisposing)
        {
            finishDrag();
            base.Dispose(isDisposing);
        }
    }
}
