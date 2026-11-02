// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.PatternGallery
{
    /// <summary>A compact, non-interactive thumbnail of the objects contained in a pattern.</summary>
    public partial class PatternGalleryPreview : CompositeDrawable
    {
        private const int preview_padding = 5;
        private const int max_preview_vertices = 96;
        private const int max_visible_markers = 48;
        private const int max_visible_slider_paths = 28;

        // Keep thumbnails in the same coordinate ratio as the editor playfield. Without this
        // cap a small pattern would be zoomed until hit circles became much larger than they are
        // in the actual editor, making dense patterns unreadable.
        private static readonly Vector2 editor_playfield_size = OsuPlayfield.BASE_SIZE;

        private readonly Container content = new Container
        {
            RelativeSizeAxes = Axes.Both,
        };

        public PatternGalleryPreview()
        {
            Size = new Vector2(78, 58);
            InternalChild = content;
        }

        public void SetObjects(IReadOnlyList<HitObject> objects)
        {
            content.Clear();

            float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
            void include(Vector2 point)
            {
                minX = MathF.Min(minX, point.X);
                minY = MathF.Min(minY, point.Y);
                maxX = MathF.Max(maxX, point.X);
                maxY = MathF.Max(maxY, point.Y);
            }
            foreach (var hitObject in objects)
            {
                if (hitObject is not OsuHitObject osuHitObject)
                    continue;

                if (hitObject is Slider slider && slider.Path.CalculatedPath.Count > 0)
                {
                    foreach (var point in slider.Path.CalculatedPath)
                        include(osuHitObject.Position + point);
                }
                else
                    include(osuHitObject.Position);
            }

            if (float.IsPositiveInfinity(minX))
                return;

            float width = MathF.Max(1, maxX - minX);
            float height = MathF.Max(1, maxY - minY);
            float availableWidth = MathF.Max(1, Width - preview_padding * 2);
            float availableHeight = MathF.Max(1, Height - preview_padding * 2);
            float fitScale = MathF.Min(availableWidth / width, availableHeight / height);
            float editorScale = MathF.Min(availableWidth / editor_playfield_size.X, availableHeight / editor_playfield_size.Y);
            float scale = MathF.Min(fitScale, editorScale);
            float xOffset = (availableWidth - width * scale) / 2;
            float yOffset = (availableHeight - height * scale) / 2;
            var origin = new Vector2(minX, minY);
            int objectCount = objects.Count(h => h is OsuHitObject);
            float densityScale = calculateDensityScale(objectCount);
            int markerStride = Math.Max(1, (objectCount + max_visible_markers - 1) / max_visible_markers);
            int sliderCount = objects.Count(h => h is Slider);
            int sliderStride = Math.Max(1, (sliderCount + max_visible_slider_paths - 1) / max_visible_slider_paths);
            int markerIndex = 0;
            int sliderIndex = 0;

            foreach (var hitObject in objects)
            {
                if (hitObject is not OsuHitObject osuHitObject)
                    continue;

                if (hitObject is Slider slider && slider.Path.CalculatedPath.Count > 1)
                {
                    bool showPath = sliderIndex % sliderStride == 0
                                    || sliderIndex == sliderCount - 1;
                    if (showPath)
                        addSliderPreview(slider, osuHitObject.Position, origin, scale, xOffset, yOffset, densityScale);
                    sliderIndex++;
                }
                else if (markerIndex % markerStride == 0 || markerIndex == objectCount - 1)
                {
                    addHitMarker(toPreviewPosition(osuHitObject.Position, origin, scale, xOffset, yOffset),
                        markerDiameter(osuHitObject, scale, densityScale), new Color4(0.2f, 0.9f, 1, MathF.Max(0.55f, densityScale)));
                }

                markerIndex++;
            }
        }

        private static float calculateDensityScale(int objectCount)
        {
            if (objectCount <= 12)
                return 1;

            // Marker size follows the square root of the amount of content. This keeps sparse
            // patterns readable while preventing a dense pattern from turning into one solid blob.
            return Math.Clamp(MathF.Sqrt(12f / objectCount), 0.38f, 1);
        }

        private void addSliderPreview(Slider slider, Vector2 position, Vector2 origin, float scale, float xOffset, float yOffset, float densityScale)
        {
            float objectScale = MathF.Max(0.01f, slider.Scale);
            float bodyRadius = MathF.Max(0.5f, OsuHitObject.OBJECT_RADIUS * objectScale * scale * densityScale);
            var outer = new SmoothPath
            {
                PathRadius = bodyRadius + MathF.Max(0.35f, scale * 2),
                Colour = new Color4(0.02f, 0.08f, 0.11f, 0.55f + densityScale * 0.35f)
            };
            var inner = new SmoothPath
            {
                PathRadius = bodyRadius,
                Colour = new Color4(0.05f, 0.8f, 0.95f, 0.42f + densityScale * 0.4f)
            };

            {
                var calculatedPath = slider.Path.CalculatedPath;
                int step = Math.Max(1, (int)Math.Ceiling((calculatedPath.Count - 1d) / (max_preview_vertices - 1)));
                for (int i = 0; i < calculatedPath.Count; i += step)
                {
                    var previewPoint = toPreviewPosition(position + calculatedPath[i], origin, scale, xOffset, yOffset);
                    outer.AddVertex(previewPoint);
                    inner.AddVertex(previewPoint);
                }

                // Always include the exact tail, even when the path was downsampled.
                var tailPoint = toPreviewPosition(position + slider.Path.PositionAt(1), origin, scale, xOffset, yOffset);
                outer.AddVertex(tailPoint);
                inner.AddVertex(tailPoint);
                outer.OriginPosition = outer.PositionInBoundingBox(Vector2.Zero);
                inner.OriginPosition = inner.PositionInBoundingBox(Vector2.Zero);
                content.Add(outer);
                content.Add(inner);
            }

            var tail = toPreviewPosition(position + slider.Path.PositionAt(1), origin, scale, xOffset, yOffset);
            float diameter = markerDiameter(slider, scale, densityScale);
            addHitMarker(toPreviewPosition(position, origin, scale, xOffset, yOffset), diameter, Colour4.Cyan);
            addHitMarker(tail, diameter, new Color4(0.2f, 0.95f, 1, MathF.Max(0.55f, densityScale)));

            // Repeat points are shown as small markers, which makes long multi-span sliders
            // recognisable without attempting to render full gameplay arrows in a tiny card.
            // Repeats alternate between the same two endpoints. Draw each at most once.
            for (int repeat = 1; repeat <= Math.Min(2, slider.RepeatCount); repeat++)
            {
                Vector2 repeatPosition = position + slider.Path.PositionAt(repeat % 2 == 0 ? 0 : 1);
                addHitMarker(toPreviewPosition(repeatPosition, origin, scale, xOffset, yOffset), MathF.Max(0.5f, bodyRadius * 1.05f), new Color4(1, 1, 1, MathF.Max(0.5f, densityScale)));
            }
        }

        private static float markerDiameter(OsuHitObject hitObject, float scale, float densityScale)
            => MathF.Max(0.5f, OsuHitObject.OBJECT_DIMENSIONS.X * MathF.Max(0.01f, hitObject.Scale) * scale * densityScale);

        private static Vector2 toPreviewPosition(Vector2 position, Vector2 origin, float scale, float xOffset, float yOffset)
            => (position - origin) * scale + new Vector2(preview_padding + xOffset, preview_padding + yOffset);

        private void addHitMarker(Vector2 position, float diameter, Color4 colour)
        {
            content.Add(new Container
            {
                Size = new Vector2(diameter),
                Position = position,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(0.01f, 0.04f, 0.06f, 0.92f)
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(MathF.Min(0.5f, diameter * 0.08f)),
                        Child = new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colour
                        }
                    }
                }
            });
        }
    }
}
