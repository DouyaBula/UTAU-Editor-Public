// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A non-interactive, session-only mirror of a selected slider. It deliberately remains a
    /// drawable preview rather than a second beatmap object: enabling the option must not add an
    /// undo entry or silently change the exported .osu file.
    /// </summary>
    public partial class ClassicSliderSymmetryPreview : CompositeDrawable
    {
        private readonly SmoothPath path = new SmoothPath
        {
            Colour = Colour4.Orange,
            Alpha = 0.55f
        };

        private readonly Circle head = new Circle
        {
            Colour = Colour4.Orange,
            Alpha = 0.7f,
            Origin = Anchor.Centre
        };

        private readonly Circle tail = new Circle
        {
            Colour = Colour4.Orange,
            Alpha = 0.7f,
            Origin = Anchor.Centre
        };

        private int lastVersion = -1;
        private EditorClassicSymmetryAxis lastAxis;
        private Vector2 lastPosition;
        private int lastStackHeight;
        private float lastScale;

        /// <summary>Mirrored start point in screen-local coordinates, exposed for visual tests.</summary>
        public Vector2 MirroredStart { get; private set; }

        /// <summary>Mirrored end point in screen-local coordinates, exposed for visual tests.</summary>
        public Vector2 MirroredEnd { get; private set; }

        public ClassicSliderSymmetryPreview()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;

            // Keep the preview behind the real slider and make it impossible for the preview to
            // steal selection or control-point input.
            InternalChildren = new Drawable[] { path, head, tail };
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => false;

        public void UpdateFrom(Slider source, EditorClassicSymmetryAxis axis, bool visible)
        {
            if (!visible || source.Path.CalculatedPath.Count < 2)
            {
                Alpha = 0;
                lastVersion = -1;
                return;
            }

            if (lastVersion == source.Path.Version.Value
                && lastAxis == axis
                && lastPosition == source.StackedPosition
                && lastStackHeight == source.StackHeight
                && MathF.Abs(lastScale - source.Scale) < 0.0001f
                && Alpha > 0)
                return;

            path.ClearVertices();
            path.Position = source.StackedPosition;
            path.PathRadius = source.Scale * OsuHitObject.OBJECT_RADIUS * 0.85f;

            foreach (Vector2 point in source.Path.CalculatedPath)
                path.AddVertex(reflect(point, axis));

            // SmoothPath uses its geometry bounds as its origin in the editor. The circles are
            // positioned in the parent coordinate space so they remain aligned with the path.
            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
            MirroredStart = source.StackedPosition + reflect(source.Path.PositionAt(0), axis);
            MirroredEnd = source.StackedPosition + reflect(source.Path.PositionAt(1), axis);

            head.Position = MirroredStart;
            tail.Position = MirroredEnd;
            head.Size = tail.Size = OsuHitObject.OBJECT_DIMENSIONS * source.Scale * 0.72f;

            lastVersion = source.Path.Version.Value;
            lastAxis = axis;
            lastPosition = source.StackedPosition;
            lastStackHeight = source.StackHeight;
            lastScale = source.Scale;
            Alpha = 1;
        }

        private static Vector2 reflect(Vector2 point, EditorClassicSymmetryAxis axis)
            => axis == EditorClassicSymmetryAxis.Vertical
                ? new Vector2(-point.X, point.Y)
                : new Vector2(point.X, -point.Y);
    }
}
