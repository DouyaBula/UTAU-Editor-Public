// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal static class MathSymbolFontPath
    {
        private static readonly Lazy<TrueTypeFont> font = new Lazy<TrueTypeFont>(loadFont);

        public static List<Vector2>? Create(int codepoint, int samples)
            => font.Value.TryCreateGlyphPath(codepoint, samples);

        private static TrueTypeFont loadFont()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string? resourceName = assembly.GetManifestResourceNames()
                                           .FirstOrDefault(name => name.EndsWith("NotoSansMath-Regular.ttf", StringComparison.Ordinal));

            if (resourceName == null)
                throw new InvalidOperationException("NotoSansMath-Regular.ttf embedded resource was not found.");

            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                                  ?? throw new InvalidOperationException("NotoSansMath-Regular.ttf embedded resource could not be opened.");
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return new TrueTypeFont(memory.ToArray());
        }

        private sealed class TrueTypeFont
        {
            private readonly byte[] data;
            private readonly Dictionary<string, TableRecord> tables = new Dictionary<string, TableRecord>();
            private readonly int unitsPerEm;
            private readonly short indexToLocFormat;
            private readonly int numGlyphs;
            private readonly int cmapOffset;

            public TrueTypeFont(byte[] data)
            {
                this.data = data;

                int tableCount = readUInt16(4);

                for (int i = 0; i < tableCount; i++)
                {
                    int offset = 12 + i * 16;
                    string tag = System.Text.Encoding.ASCII.GetString(data, offset, 4);
                    tables[tag] = new TableRecord(readUInt32(offset + 8), readUInt32(offset + 12));
                }

                unitsPerEm = readUInt16(table("head").Offset + 18);
                indexToLocFormat = readInt16(table("head").Offset + 50);
                numGlyphs = readUInt16(table("maxp").Offset + 4);
                cmapOffset = table("cmap").Offset;
            }

            public List<Vector2>? TryCreateGlyphPath(int codepoint, int samples)
            {
                int glyphIndex = glyphIndexForCodepoint(codepoint);

                if (glyphIndex <= 0 || glyphIndex >= numGlyphs)
                    return null;

                List<List<GlyphPoint>> contours = readGlyph(glyphIndex, 0);

                if (contours.Count == 0)
                    return null;

                List<List<Vector2>> sampledContours = contours.Select(contour => sampleContour(contour, samples))
                                                               .Where(contour => contour.Count > 1)
                                                               .ToList();

                if (sampledContours.Count == 0)
                    return null;

                return normalise(connectContours(sampledContours));
            }

            private List<List<GlyphPoint>> readGlyph(int glyphIndex, int depth)
            {
                if (depth > 8)
                    return new List<List<GlyphPoint>>();

                TableRecord glyf = table("glyf");
                int glyphStart = glyf.Offset + glyphOffset(glyphIndex);
                int glyphEnd = glyf.Offset + glyphOffset(glyphIndex + 1);

                if (glyphStart >= glyphEnd)
                    return new List<List<GlyphPoint>>();

                short contourCount = readInt16(glyphStart);

                if (contourCount >= 0)
                    return readSimpleGlyph(glyphStart, contourCount);

                return readCompositeGlyph(glyphStart, depth);
            }

            private List<List<GlyphPoint>> readSimpleGlyph(int glyphStart, int contourCount)
            {
                var endPoints = new int[contourCount];

                for (int i = 0; i < contourCount; i++)
                    endPoints[i] = readUInt16(glyphStart + 10 + i * 2);

                int pointCount = contourCount == 0 ? 0 : endPoints[^1] + 1;

                if (pointCount <= 0)
                    return new List<List<GlyphPoint>>();

                int instructionLengthOffset = glyphStart + 10 + contourCount * 2;
                int instructionLength = readUInt16(instructionLengthOffset);
                int offset = instructionLengthOffset + 2 + instructionLength;
                var flags = new byte[pointCount];

                for (int i = 0; i < pointCount; i++)
                {
                    byte flag = data[offset++];
                    flags[i] = flag;

                    if ((flag & 0x08) == 0)
                        continue;

                    int repeat = data[offset++];

                    for (int r = 0; r < repeat; r++)
                        flags[++i] = flag;
                }

                var xCoordinates = new short[pointCount];
                short x = 0;

                for (int i = 0; i < pointCount; i++)
                {
                    byte flag = flags[i];

                    if ((flag & 0x02) != 0)
                    {
                        short delta = data[offset++];
                        x += (short)(((flag & 0x10) != 0) ? delta : -delta);
                    }
                    else if ((flag & 0x10) == 0)
                    {
                        x += readInt16(offset);
                        offset += 2;
                    }

                    xCoordinates[i] = x;
                }

                var yCoordinates = new short[pointCount];
                short y = 0;

                for (int i = 0; i < pointCount; i++)
                {
                    byte flag = flags[i];

                    if ((flag & 0x04) != 0)
                    {
                        short delta = data[offset++];
                        y += (short)(((flag & 0x20) != 0) ? delta : -delta);
                    }
                    else if ((flag & 0x20) == 0)
                    {
                        y += readInt16(offset);
                        offset += 2;
                    }

                    yCoordinates[i] = y;
                }

                var contours = new List<List<GlyphPoint>>(contourCount);
                int start = 0;

                foreach (int end in endPoints)
                {
                    var contour = new List<GlyphPoint>(end - start + 1);

                    for (int i = start; i <= end; i++)
                        contour.Add(new GlyphPoint(new Vector2(xCoordinates[i], -yCoordinates[i]), (flags[i] & 0x01) != 0));

                    contours.Add(contour);
                    start = end + 1;
                }

                return contours;
            }

            private List<List<GlyphPoint>> readCompositeGlyph(int glyphStart, int depth)
            {
                var contours = new List<List<GlyphPoint>>();
                int offset = glyphStart + 10;
                ushort flags;

                do
                {
                    flags = readUInt16(offset);
                    int componentGlyphIndex = readUInt16(offset + 2);
                    offset += 4;

                    int arg1;
                    int arg2;

                    if ((flags & 0x0001) != 0)
                    {
                        arg1 = readInt16(offset);
                        arg2 = readInt16(offset + 2);
                        offset += 4;
                    }
                    else
                    {
                        arg1 = (sbyte)data[offset];
                        arg2 = (sbyte)data[offset + 1];
                        offset += 2;
                    }

                    float a = 1;
                    float b = 0;
                    float c = 0;
                    float d = 1;

                    if ((flags & 0x0008) != 0)
                    {
                        a = d = fixed2Dot14(offset);
                        offset += 2;
                    }
                    else if ((flags & 0x0040) != 0)
                    {
                        a = fixed2Dot14(offset);
                        d = fixed2Dot14(offset + 2);
                        offset += 4;
                    }
                    else if ((flags & 0x0080) != 0)
                    {
                        a = fixed2Dot14(offset);
                        b = fixed2Dot14(offset + 2);
                        c = fixed2Dot14(offset + 4);
                        d = fixed2Dot14(offset + 6);
                        offset += 8;
                    }

                    Vector2 translation = (flags & 0x0002) != 0 ? new Vector2(arg1, -arg2) : Vector2.Zero;

                    foreach (List<GlyphPoint> contour in readGlyph(componentGlyphIndex, depth + 1))
                    {
                        contours.Add(contour.Select(point =>
                        {
                            Vector2 position = point.Position;
                            return new GlyphPoint(new Vector2(
                                position.X * a + position.Y * b + translation.X,
                                position.X * c + position.Y * d + translation.Y), point.OnCurve);
                        }).ToList());
                    }
                } while ((flags & 0x0020) != 0);

                return contours;
            }

            private static List<Vector2> sampleContour(List<GlyphPoint> contour, int samples)
            {
                var expanded = new List<GlyphPoint>(contour.Count * 2);

                for (int i = 0; i < contour.Count; i++)
                {
                    GlyphPoint current = contour[i];
                    GlyphPoint next = contour[(i + 1) % contour.Count];
                    expanded.Add(current);

                    if (!current.OnCurve && !next.OnCurve)
                        expanded.Add(new GlyphPoint((current.Position + next.Position) / 2, true));
                }

                int startIndex = expanded.FindIndex(point => point.OnCurve);

                if (startIndex < 0)
                    return new List<Vector2>();

                var ordered = new List<GlyphPoint>(expanded.Count + 1);

                for (int i = 0; i < expanded.Count; i++)
                    ordered.Add(expanded[(startIndex + i) % expanded.Count]);

                ordered.Add(ordered[0]);

                var result = new List<Vector2>();
                Vector2 currentPosition = ordered[0].Position;
                result.Add(currentPosition);
                int sampleSteps = Math.Clamp(samples / 24, 3, 18);

                for (int i = 1; i < ordered.Count; i++)
                {
                    GlyphPoint point = ordered[i];

                    if (point.OnCurve)
                    {
                        addLine(result, point.Position, sampleSteps);
                        currentPosition = point.Position;
                    }
                    else if (i + 1 < ordered.Count)
                    {
                        GlyphPoint end = ordered[++i];
                        addQuadratic(result, currentPosition, point.Position, end.Position, sampleSteps);
                        currentPosition = end.Position;
                    }
                }

                if (result.Count > 0)
                    result[^1] = result[0];

                return result;
            }

            private static void addLine(List<Vector2> target, Vector2 end, int steps)
            {
                Vector2 start = target[^1];

                for (int i = 1; i <= steps; i++)
                    addDistinct(target, Vector2.Lerp(start, end, i / (float)steps));
            }

            private static void addQuadratic(List<Vector2> target, Vector2 start, Vector2 control, Vector2 end, int steps)
            {
                for (int i = 1; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    float inverse = 1 - t;
                    addDistinct(target, start * (inverse * inverse) + control * (2 * inverse * t) + end * (t * t));
                }
            }

            private static void addDistinct(List<Vector2> target, Vector2 point)
            {
                if (target.Count == 0 || Vector2.DistanceSquared(target[^1], point) > 0.000001f)
                    target.Add(point);
            }

            private static List<Vector2> connectContours(List<List<Vector2>> contours)
            {
                contours = contours.OrderByDescending(contour => Math.Abs(signedArea(contour))).ToList();
                var result = new List<Vector2>(contours.Sum(contour => contour.Count));

                while (contours.Count > 0)
                {
                    int contourIndex = 0;
                    int startIndex = 0;

                    if (result.Count > 0)
                    {
                        float bestDistance = float.MaxValue;

                        for (int i = 0; i < contours.Count; i++)
                        {
                            for (int j = 0; j < contours[i].Count - 1; j++)
                            {
                                float distance = Vector2.DistanceSquared(result[^1], contours[i][j]);

                                if (distance >= bestDistance)
                                    continue;

                                bestDistance = distance;
                                contourIndex = i;
                                startIndex = j;
                            }
                        }
                    }

                    List<Vector2> contour = rotateClosedContour(contours[contourIndex], startIndex);

                    int firstContourPoint = 0;

                    if (result.Count > 0 && Vector2.DistanceSquared(result[^1], contour[0]) > 0.000001f)
                    {
                        Vector2 current = result[^1];
                        addDistinct(result, contour[0]);
                        addDistinct(result, current);
                        addDistinct(result, contour[0]);
                        firstContourPoint = 1;
                    }

                    for (int i = firstContourPoint; i < contour.Count; i++)
                        addDistinct(result, contour[i]);

                    contours.RemoveAt(contourIndex);
                }

                return result;
            }

            private static List<Vector2> rotateClosedContour(List<Vector2> contour, int start)
            {
                int count = contour.Count - 1;
                var result = new List<Vector2>(contour.Count);

                for (int i = 0; i < count; i++)
                    result.Add(contour[(start + i) % count]);

                result.Add(result[0]);
                return result;
            }

            private static float signedArea(IReadOnlyList<Vector2> contour)
            {
                float area = 0;

                for (int i = 0; i < contour.Count - 1; i++)
                    area += contour[i].X * contour[i + 1].Y - contour[i + 1].X * contour[i].Y;

                return area / 2;
            }

            private List<Vector2> normalise(List<Vector2> points)
            {
                float minX = points.Min(point => point.X);
                float maxX = points.Max(point => point.X);
                float minY = points.Min(point => point.Y);
                float maxY = points.Max(point => point.Y);
                float largest = Math.Max(maxX - minX, maxY - minY);

                if (largest <= 0.0001f)
                    return new List<Vector2>();

                Vector2 centre = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
                float scale = unitsPerEm / largest * 2;
                return points.Select(point => (point - centre) * scale / unitsPerEm).ToList();
            }

            private int glyphIndexForCodepoint(int codepoint)
            {
                int subtableCount = readUInt16(cmapOffset + 2);
                int format4Offset = -1;

                for (int i = 0; i < subtableCount; i++)
                {
                    int record = cmapOffset + 4 + i * 8;
                    int subtableOffset = cmapOffset + readUInt32(record + 4);
                    int format = readUInt16(subtableOffset);

                    if (format == 12)
                    {
                        int glyphIndex = glyphIndexFromFormat12(subtableOffset, codepoint);

                        if (glyphIndex > 0)
                            return glyphIndex;
                    }
                    else if (format == 4)
                        format4Offset = subtableOffset;
                }

                return codepoint <= 0xffff && format4Offset >= 0 ? glyphIndexFromFormat4(format4Offset, codepoint) : 0;
            }

            private int glyphIndexFromFormat12(int offset, int codepoint)
            {
                int groups = readUInt32(offset + 12);

                for (int i = 0; i < groups; i++)
                {
                    int group = offset + 16 + i * 12;
                    int start = readUInt32(group);
                    int end = readUInt32(group + 4);

                    if (codepoint >= start && codepoint <= end)
                        return readUInt32(group + 8) + codepoint - start;
                }

                return 0;
            }

            private int glyphIndexFromFormat4(int offset, int codepoint)
            {
                int segmentCount = readUInt16(offset + 6) / 2;
                int endCodeOffset = offset + 14;
                int startCodeOffset = endCodeOffset + segmentCount * 2 + 2;
                int idDeltaOffset = startCodeOffset + segmentCount * 2;
                int idRangeOffsetOffset = idDeltaOffset + segmentCount * 2;

                for (int i = 0; i < segmentCount; i++)
                {
                    int end = readUInt16(endCodeOffset + i * 2);
                    int start = readUInt16(startCodeOffset + i * 2);

                    if (codepoint < start || codepoint > end)
                        continue;

                    int delta = readInt16(idDeltaOffset + i * 2);
                    int rangeOffset = readUInt16(idRangeOffsetOffset + i * 2);

                    if (rangeOffset == 0)
                        return (codepoint + delta) & 0xffff;

                    int glyphOffset = idRangeOffsetOffset + i * 2 + rangeOffset + (codepoint - start) * 2;
                    int glyphIndex = readUInt16(glyphOffset);
                    return glyphIndex == 0 ? 0 : (glyphIndex + delta) & 0xffff;
                }

                return 0;
            }

            private int glyphOffset(int glyphIndex)
            {
                TableRecord loca = table("loca");

                if (indexToLocFormat == 0)
                    return readUInt16(loca.Offset + glyphIndex * 2) * 2;

                return readUInt32(loca.Offset + glyphIndex * 4);
            }

            private TableRecord table(string tag) => tables.TryGetValue(tag, out TableRecord record)
                ? record
                : throw new InvalidOperationException($"TrueType table '{tag}' was not found.");

            private ushort readUInt16(int offset) => (ushort)((data[offset] << 8) | data[offset + 1]);

            private short readInt16(int offset) => unchecked((short)readUInt16(offset));

            private int readUInt32(int offset) => (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];

            private float fixed2Dot14(int offset) => readInt16(offset) / 16384f;
        }

        private readonly struct TableRecord
        {
            public readonly int Offset;
            public readonly int Length;

            public TableRecord(int offset, int length)
            {
                Offset = offset;
                Length = length;
            }
        }

        private readonly struct GlyphPoint
        {
            public readonly Vector2 Position;
            public readonly bool OnCurve;

            public GlyphPoint(Vector2 position, bool onCurve)
            {
                Position = position;
                OnCurve = onCurve;
            }
        }
    }
}
