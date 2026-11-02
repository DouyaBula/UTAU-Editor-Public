// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit
{
    public readonly struct EditorHitsoundMidiExportResult
    {
        public readonly int ObjectNotes;
        public readonly int SliderEndNotes;

        public EditorHitsoundMidiExportResult(int objectNotes, int sliderEndNotes)
        {
            ObjectNotes = objectNotes;
            SliderEndNotes = sliderEndNotes;
        }
    }

    public readonly struct EditorHitsoundMidiImportResult
    {
        public readonly int ImportedNotes;
        public readonly int MatchedNotes;
        public readonly int UpdatedObjects;

        public EditorHitsoundMidiImportResult(int importedNotes, int matchedNotes, int updatedObjects)
        {
            ImportedNotes = importedNotes;
            MatchedNotes = matchedNotes;
            UpdatedObjects = updatedObjects;
        }
    }

    public static class EditorHitsoundMidiTools
    {
        public const int TICKS_PER_QUARTER = 480;

        private const int object_note = 60;
        private const int slider_end_note = 61;
        private const int exported_note_velocity = 100;
        private const int max_exported_note_length_ticks = TICKS_PER_QUARTER / 16;

        public static EditorHitsoundMidiExportResult Export(EditorBeatmap editorBeatmap, Stream output, bool includeSliderEnds)
        {
            var tempoMap = TempoMap.FromBeatmap(editorBeatmap);
            var objectTicks = editorBeatmap.HitObjects
                                           .OrderBy(h => h.StartTime)
                                           .Select(h => tempoMap.TimeToTick(h.StartTime))
                                           .ToArray();
            var sliderEndTicks = includeSliderEnds
                ? editorBeatmap.HitObjects
                               .OrderBy(h => h.StartTime)
                               .Where(h => h is IHasRepeats)
                               .Select(h => tempoMap.TimeToTick(h.StartTime + ((IHasRepeats)h).Duration))
                               .ToArray()
                : Array.Empty<long>();
            long noteLength = calculateExportedNoteLength(objectTicks.Concat(sliderEndTicks));
            var tracks = new List<MidiTrack>
            {
                createTempoTrack(tempoMap),
                createObjectTrack(objectTicks, noteLength),
            };

            int sliderEndNotes = 0;

            if (includeSliderEnds)
            {
                var sliderEndTrack = createSliderEndTrack(sliderEndTicks, noteLength);
                sliderEndNotes = sliderEndTrack.NoteCount;
                tracks.Add(sliderEndTrack);
            }

            writeMidi(output, tracks);

            return new EditorHitsoundMidiExportResult(editorBeatmap.HitObjects.Count, sliderEndNotes);
        }

        public static EditorHitsoundMidiImportResult Import(EditorBeatmap editorBeatmap, Stream input, double temporalLeniency, bool clearExistingHitsounds)
        {
            HitsoundTimeMatcher.ValidateTolerance(temporalLeniency);
            var imported = readMidi(input);
            var notes = imported.Notes.OrderBy(n => n.Time).ToArray();
            var updatedObjects = new HashSet<HitObject>();
            int matched = 0;

            if (clearExistingHitsounds)
            {
                foreach (var hitObject in editorBeatmap.HitObjects)
                {
                    resetSamples(hitObject);
                    updatedObjects.Add(hitObject);
                }
            }

            var targets = createTargets(editorBeatmap).OrderBy(t => t.Time).ToArray();

            foreach (var note in notes)
            {
                var target = findNearestTarget(note, targets, temporalLeniency);

                if (target == null)
                    continue;

                target.AddSample(createSample(note));
                updatedObjects.Add(target.HitObject);
                matched++;
            }

            foreach (var hitObject in updatedObjects)
                editorBeatmap.Update(hitObject);

            return new EditorHitsoundMidiImportResult(notes.Length, matched, updatedObjects.Count);
        }

        public static string CreateExportFilename(EditorBeatmap editorBeatmap)
        {
            string artist = sanitiseFilenamePart(editorBeatmap.Metadata.Artist);
            string title = sanitiseFilenamePart(editorBeatmap.Metadata.Title);
            string difficulty = sanitiseFilenamePart(editorBeatmap.BeatmapInfo.DifficultyName);

            string prefix = string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(title)
                ? "beatmap"
                : $"{artist} - {title}".Trim(' ', '-');

            return $"{prefix} [{difficulty}] hitsound-midi.mid";
        }

        private static MidiTrack createTempoTrack(TempoMap tempoMap)
        {
            var track = new MidiTrack("osu timing");

            foreach (var tempo in tempoMap.Tempos)
            {
                track.Events.Add(new MidiEvent(tempo.Tick, [0xff, 0x51, 0x03, (byte)(tempo.MicrosecondsPerQuarter >> 16), (byte)(tempo.MicrosecondsPerQuarter >> 8), (byte)tempo.MicrosecondsPerQuarter]));
            }

            return track;
        }

        private static MidiTrack createObjectTrack(IEnumerable<long> objectTicks, long noteLength)
        {
            var track = new MidiTrack("osu hit objects");

            foreach (long tick in objectTicks)
                addNote(track, tick, noteLength, 0, object_note, exported_note_velocity);

            return track;
        }

        private static MidiTrack createSliderEndTrack(IEnumerable<long> sliderEndTicks, long noteLength)
        {
            var track = new MidiTrack("osu slider ends");

            foreach (long tick in sliderEndTicks)
                addNote(track, tick, noteLength, 0, slider_end_note, exported_note_velocity);

            return track;
        }

        private static long calculateExportedNoteLength(IEnumerable<long> ticks)
        {
            var orderedTicks = ticks.Distinct().Order().ToArray();

            if (orderedTicks.Length < 2)
                return max_exported_note_length_ticks;

            long minGap = long.MaxValue;

            for (int i = 1; i < orderedTicks.Length; i++)
                minGap = Math.Min(minGap, orderedTicks[i] - orderedTicks[i - 1]);

            return Math.Clamp(minGap / 2, 1, max_exported_note_length_ticks);
        }

        private static void addNote(MidiTrack track, long tick, long length, int channel, int note, int velocity)
        {
            track.Events.Add(new MidiEvent(tick, [(byte)(0x90 | channel), (byte)note, (byte)velocity]));
            track.Events.Add(new MidiEvent(tick + length, [(byte)(0x80 | channel), (byte)note, 0]));
            track.NoteCount++;
        }

        private static IEnumerable<HitTarget> createTargets(EditorBeatmap editorBeatmap)
        {
            foreach (var hitObject in editorBeatmap.HitObjects)
            {
                yield return new HitTarget(hitObject, hitObject.StartTime, HitTargetType.HitObject);

                if (hitObject is not IHasRepeats repeats)
                    continue;

                populateNodeSamples(hitObject, repeats);
                yield return new HitTarget(hitObject, hitObject.StartTime + repeats.Duration, HitTargetType.SliderEnd, repeats.RepeatCount + 1);
            }
        }

        private static HitTarget? findNearestTarget(ImportedNote note, IReadOnlyList<HitTarget> targets, double temporalLeniency)
        {
            var preferredType = getPreferredTargetType(note);
            int index = HitsoundTimeMatcher.FindNearest(targets, note.Time, temporalLeniency, target => target.Time,
                preferred: target => preferredType.HasValue && target.Type == preferredType.Value);
            return index < 0 ? null : targets[index];
        }

        private static HitTargetType? getPreferredTargetType(ImportedNote note)
        {
            if (note.Note == slider_end_note || note.TrackName.Contains("slider end", StringComparison.OrdinalIgnoreCase))
                return HitTargetType.SliderEnd;

            if (note.Note == object_note || note.TrackName.Contains("hit object", StringComparison.OrdinalIgnoreCase))
                return HitTargetType.HitObject;

            return null;
        }

        private static HitSampleInfo createSample(ImportedNote note)
        {
            int volume = Math.Clamp((int)Math.Round(note.Velocity / 127d * 100), EditorHitsoundTools.MUTED_VOLUME, 100);
            return new ConvertHitObjectParser.FileHitSampleInfo(createSampleFilename(note), volume);
        }

        private static string createSampleFilename(ImportedNote note)
        {
            string trackName = sanitiseFilenamePart(note.TrackName);

            if (string.IsNullOrEmpty(trackName))
                trackName = $"track{note.TrackIndex + 1:00}";

            string patch = note.Patch >= 0 ? $"-p{note.Patch:000}" : string.Empty;
            string bank = note.Bank >= 0 ? $"-b{note.Bank:00000}" : string.Empty;

            return $"midi-{trackName}-c{note.Channel + 1:00}{bank}{patch}-n{note.Note:000}.wav";
        }

        private static string sanitiseFilenamePart(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var builder = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (Path.GetInvalidFileNameChars().Contains(c) || Path.GetInvalidPathChars().Contains(c))
                    continue;

                builder.Append(char.IsWhiteSpace(c) ? '-' : c);
            }

            return builder.ToString().Trim('-', '.', ' ');
        }

        private static void resetSamples(HitObject hitObject)
        {
            hitObject.Samples = new List<HitSampleInfo>();

            if (hitObject is not IHasRepeats repeats)
                return;

            populateNodeSamples(hitObject, repeats);

            for (int i = 0; i < repeats.NodeSamples.Count; i++)
                repeats.NodeSamples[i] = new List<HitSampleInfo>();
        }

        private static void populateNodeSamples(HitObject hitObject, IHasRepeats repeats)
        {
            while (repeats.NodeSamples.Count < repeats.RepeatCount + 2)
                repeats.NodeSamples.Add(hitObject.Samples.Select(sample => sample.With()).ToList());
        }

        private static void writeMidi(Stream output, IReadOnlyList<MidiTrack> tracks)
        {
            using var writer = new BinaryWriter(output, Encoding.ASCII, true);

            writer.Write(Encoding.ASCII.GetBytes("MThd"));
            writeInt32BigEndian(writer, 6);
            writeInt16BigEndian(writer, 1);
            writeInt16BigEndian(writer, (short)tracks.Count);
            writeInt16BigEndian(writer, TICKS_PER_QUARTER);

            foreach (var track in tracks)
                writeTrack(writer, track);
        }

        private static void writeTrack(BinaryWriter writer, MidiTrack track)
        {
            using var stream = new MemoryStream();
            using (var trackWriter = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                long previousTick = 0;

                if (!string.IsNullOrEmpty(track.Name))
                    writeEvent(trackWriter, 0, createTrackNameEvent(track.Name));

                foreach (var midiEvent in track.Events.OrderBy(e => e.Tick).ThenBy(getEventSortPriority))
                {
                    writeEvent(trackWriter, midiEvent.Tick - previousTick, midiEvent.Data);
                    previousTick = midiEvent.Tick;
                }

                writeEvent(trackWriter, 0, [0xff, 0x2f, 0x00]);
            }

            writer.Write(Encoding.ASCII.GetBytes("MTrk"));
            writeInt32BigEndian(writer, (int)stream.Length);
            writer.Write(stream.ToArray());
        }

        private static int getEventSortPriority(MidiEvent midiEvent)
        {
            byte status = midiEvent.Data[0];

            if (status == 0xff)
                return 0;

            int command = status & 0xf0;

            if (command == 0x80 || (command == 0x90 && midiEvent.Data.Length > 2 && midiEvent.Data[2] == 0))
                return 1;

            return 2;
        }

        private static byte[] createTrackNameEvent(string name)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            using var stream = new MemoryStream();
            stream.WriteByte(0xff);
            stream.WriteByte(0x03);
            writeVariableLength(stream, nameBytes.Length);
            stream.Write(nameBytes);
            return stream.ToArray();
        }

        private static void writeEvent(BinaryWriter writer, long delta, byte[] data)
        {
            writeVariableLength(writer.BaseStream, delta);
            writer.Write(data);
        }

        private static ImportedMidi readMidi(Stream input)
        {
            using var reader = new BinaryReader(input, Encoding.ASCII, true);

            if (readAscii(reader, 4) != "MThd")
                throw new InvalidDataException("Not a standard MIDI file.");

            int headerLength = readInt32BigEndian(reader);
            int format = readInt16BigEndian(reader);
            int trackCount = readInt16BigEndian(reader);
            int division = readInt16BigEndian(reader);

            if (format is < 0 or > 1)
                throw new InvalidDataException("Only MIDI format 0 and 1 files are supported.");

            if ((division & 0x8000) != 0)
                throw new InvalidDataException("SMPTE MIDI timing is not supported.");

            if (headerLength > 6)
                reader.BaseStream.Seek(headerLength - 6, SeekOrigin.Current);

            var rawNotes = new List<RawNote>();
            var tempoEvents = new List<RawTempoEvent> { new RawTempoEvent(0, 500000) };

            for (int trackIndex = 0; trackIndex < trackCount; trackIndex++)
            {
                if (readAscii(reader, 4) != "MTrk")
                    throw new InvalidDataException("Invalid MIDI track chunk.");

                int length = readInt32BigEndian(reader);
                long trackEnd = reader.BaseStream.Position + length;
                parseTrack(reader, trackEnd, trackIndex, rawNotes, tempoEvents);
                reader.BaseStream.Position = trackEnd;
            }

            var tempoMap = TempoMap.FromMidiTempos(division, tempoEvents);
            var notes = rawNotes.Select(n => new ImportedNote(
                tempoMap.TickToTime(n.Tick),
                n.TrackIndex,
                n.TrackName,
                n.Channel,
                n.Note,
                n.Velocity,
                n.Bank,
                n.Patch)).ToArray();

            return new ImportedMidi(notes);
        }

        private static void parseTrack(BinaryReader reader, long trackEnd, int trackIndex, List<RawNote> notes, List<RawTempoEvent> tempoEvents)
        {
            long absoluteTick = 0;
            int? runningStatus = null;
            string trackName = string.Empty;
            var activeNotes = new Dictionary<(int Channel, int Note), Queue<RawNoteStart>>();
            var channelBanks = new Dictionary<int, int>();
            var channelPatches = new Dictionary<int, int>();
            channelBanks[9] = 128;

            while (reader.BaseStream.Position < trackEnd)
            {
                absoluteTick += readVariableLength(reader.BaseStream);
                int status = reader.ReadByte();

                if (status < 0x80)
                {
                    if (runningStatus == null)
                        throw new InvalidDataException("MIDI running status used before a status byte.");

                    reader.BaseStream.Position--;
                    status = runningStatus.Value;
                }
                else if (status < 0xf0)
                    runningStatus = status;

                if (status == 0xff)
                {
                    int type = reader.ReadByte();
                    int length = (int)readVariableLength(reader.BaseStream);
                    byte[] data = reader.ReadBytes(length);

                    switch (type)
                    {
                        case 0x03:
                            trackName = Encoding.UTF8.GetString(data);
                            break;

                        case 0x51 when data.Length == 3:
                            tempoEvents.Add(new RawTempoEvent(absoluteTick, (data[0] << 16) | (data[1] << 8) | data[2]));
                            break;
                    }

                    if (type == 0x2f)
                        break;

                    continue;
                }

                if (status == 0xf0 || status == 0xf7)
                {
                    int length = (int)readVariableLength(reader.BaseStream);
                    reader.BaseStream.Seek(length, SeekOrigin.Current);
                    runningStatus = null;
                    continue;
                }

                int command = status & 0xf0;
                int channel = status & 0x0f;
                int data1 = reader.ReadByte();
                int data2 = command is 0xc0 or 0xd0 ? -1 : reader.ReadByte();

                switch (command)
                {
                    case 0x80:
                        finishNote(channel, data1);
                        break;

                    case 0x90:
                        if (data2 == 0)
                            finishNote(channel, data1);
                        else
                            startNote(channel, data1, data2);
                        break;

                    case 0xb0:
                        updateBank(channel, data1, data2);
                        break;

                    case 0xc0:
                        channelPatches[channel] = data1;
                        break;
                }
            }

            void startNote(int channel, int note, int velocity)
            {
                var key = (channel, note);

                if (!activeNotes.TryGetValue(key, out var queue))
                    activeNotes[key] = queue = new Queue<RawNoteStart>();

                queue.Enqueue(new RawNoteStart(absoluteTick, velocity, getBank(channel), getPatch(channel)));
            }

            void finishNote(int channel, int note)
            {
                var key = (channel, note);

                if (!activeNotes.TryGetValue(key, out var queue) || queue.Count == 0)
                    return;

                var started = queue.Dequeue();
                notes.Add(new RawNote(started.Tick, trackIndex, trackName, channel, note, started.Velocity, started.Bank, started.Patch));
            }

            void updateBank(int channel, int controller, int value)
            {
                int existing = getBank(channel);

                switch (controller)
                {
                    case 0:
                        channelBanks[channel] = (value << 7) | (existing & 0x7f);
                        break;

                    case 32:
                        channelBanks[channel] = (existing & ~0x7f) | (value & 0x7f);
                        break;
                }
            }

            int getBank(int channel) => channelBanks.GetValueOrDefault(channel, -1);

            int getPatch(int channel) => channelPatches.GetValueOrDefault(channel, -1);
        }

        private static string readAscii(BinaryReader reader, int length) => Encoding.ASCII.GetString(reader.ReadBytes(length));

        private static int readInt16BigEndian(BinaryReader reader)
        {
            int b1 = reader.ReadByte();
            int b2 = reader.ReadByte();
            return (b1 << 8) | b2;
        }

        private static int readInt32BigEndian(BinaryReader reader)
        {
            int b1 = reader.ReadByte();
            int b2 = reader.ReadByte();
            int b3 = reader.ReadByte();
            int b4 = reader.ReadByte();
            return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
        }

        private static void writeInt16BigEndian(BinaryWriter writer, int value)
        {
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
        }

        private static void writeInt32BigEndian(BinaryWriter writer, int value)
        {
            writer.Write((byte)(value >> 24));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)value);
        }

        private static long readVariableLength(Stream stream)
        {
            long value = 0;

            while (true)
            {
                int b = stream.ReadByte();

                if (b < 0)
                    throw new EndOfStreamException();

                byte data = (byte)b;
                value = value * 128 + (data & 0x7f);

                if (data < 0x80)
                    return value;
            }
        }

        private static void writeVariableLength(Stream stream, long value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            Span<byte> buffer = stackalloc byte[10];
            int index = buffer.Length - 1;
            buffer[index] = (byte)(value & 0x7f);

            while ((value >>= 7) > 0)
                buffer[--index] = (byte)((byte)(value & 0x7f) | 0x80);

            stream.Write(buffer[index..]);
        }

        private enum HitTargetType
        {
            HitObject,
            SliderEnd,
        }

        private sealed class HitTarget
        {
            public readonly HitObject HitObject;
            public readonly double Time;
            public readonly HitTargetType Type;

            private readonly int nodeIndex;

            public HitTarget(HitObject hitObject, double time, HitTargetType type, int nodeIndex = -1)
            {
                HitObject = hitObject;
                Time = time;
                Type = type;
                this.nodeIndex = nodeIndex;
            }

            public void AddSample(HitSampleInfo sample) => getMutableSamples().Add(sample);

            private IList<HitSampleInfo> getMutableSamples()
            {
                switch (Type)
                {
                    case HitTargetType.HitObject:
                        if (HitObject.Samples.IsReadOnly)
                            HitObject.Samples = HitObject.Samples.ToList();

                        return HitObject.Samples;

                    case HitTargetType.SliderEnd:
                        var repeats = (IHasRepeats)HitObject;
                        var samples = repeats.NodeSamples[nodeIndex];

                        if (samples.IsReadOnly)
                            repeats.NodeSamples[nodeIndex] = samples = samples.ToList();

                        return samples;

                    default:
                        throw new InvalidOperationException($"Unsupported MIDI hitsound target type {Type}.");
                }
            }
        }

        private readonly record struct MidiEvent(long Tick, byte[] Data);

        private sealed class MidiTrack
        {
            public readonly string Name;
            public readonly List<MidiEvent> Events = new List<MidiEvent>();
            public int NoteCount;

            public MidiTrack(string name)
            {
                Name = name;
            }
        }

        private readonly record struct ImportedMidi(IReadOnlyList<ImportedNote> Notes);

        private readonly record struct ImportedNote(double Time, int TrackIndex, string TrackName, int Channel, int Note, int Velocity, int Bank, int Patch);

        private readonly record struct RawNote(long Tick, int TrackIndex, string TrackName, int Channel, int Note, int Velocity, int Bank, int Patch);

        private readonly record struct RawNoteStart(long Tick, int Velocity, int Bank, int Patch);

        private readonly record struct RawTempoEvent(long Tick, int MicrosecondsPerQuarter);

        private readonly record struct TempoEvent(long Tick, double Time, int MicrosecondsPerQuarter);

        private sealed class TempoMap
        {
            public readonly int TicksPerQuarter;
            public readonly IReadOnlyList<TempoEvent> Tempos;

            private TempoMap(int ticksPerQuarter, IReadOnlyList<TempoEvent> tempos)
            {
                TicksPerQuarter = ticksPerQuarter;
                Tempos = tempos;
            }

            public static TempoMap FromBeatmap(EditorBeatmap editorBeatmap)
            {
                var timingPoints = editorBeatmap.ControlPointInfo.TimingPoints.OrderBy(t => t.Time).ToArray();
                var activeAtZero = editorBeatmap.ControlPointInfo.TimingPointAt(0);
                var source = new List<BeatmapTempoPoint>
                {
                    new BeatmapTempoPoint(0, activeAtZero.BeatLength),
                };

                source.AddRange(timingPoints.Where(t => t.Time > 0).Select(t => new BeatmapTempoPoint(t.Time, t.BeatLength)));

                var tempos = new List<TempoEvent>(source.Count);
                long tick = 0;
                double time = source[0].Time;
                double beatLength = source[0].BeatLength;

                foreach (var point in source)
                {
                    if (point.Time > time)
                        tick += (long)Math.Round((point.Time - time) / beatLength * TICKS_PER_QUARTER);

                    time = point.Time;
                    beatLength = point.BeatLength;
                    tempos.Add(new TempoEvent(tick, point.Time, (int)Math.Round(point.BeatLength * 1000)));
                }

                return new TempoMap(TICKS_PER_QUARTER, tempos);
            }

            public static TempoMap FromMidiTempos(int ticksPerQuarter, IEnumerable<RawTempoEvent> rawTempos)
            {
                var raw = rawTempos.OrderBy(t => t.Tick).ToArray();
                var tempos = new List<TempoEvent>();
                long lastTick = 0;
                double lastTime = 0;
                int lastMicrosecondsPerQuarter = 500000;

                foreach (var tempo in raw)
                {
                    if (tempo.Tick < lastTick)
                        continue;

                    double time = lastTime + (tempo.Tick - lastTick) * lastMicrosecondsPerQuarter / 1000d / ticksPerQuarter;
                    tempos.Add(new TempoEvent(tempo.Tick, time, tempo.MicrosecondsPerQuarter));

                    lastTick = tempo.Tick;
                    lastTime = time;
                    lastMicrosecondsPerQuarter = tempo.MicrosecondsPerQuarter;
                }

                if (tempos.Count == 0 || tempos[0].Tick != 0)
                    tempos.Insert(0, new TempoEvent(0, 0, 500000));

                return new TempoMap(ticksPerQuarter, tempos);
            }

            public long TimeToTick(double time)
            {
                var tempo = Tempos[0];

                for (int i = 1; i < Tempos.Count && Tempos[i].Time <= time; i++)
                    tempo = Tempos[i];

                return tempo.Tick + (long)Math.Round((time - tempo.Time) * 1000 / tempo.MicrosecondsPerQuarter * TicksPerQuarter);
            }

            public double TickToTime(long tick)
            {
                var tempo = Tempos[0];

                for (int i = 1; i < Tempos.Count && Tempos[i].Tick <= tick; i++)
                    tempo = Tempos[i];

                return tempo.Time + (tick - tempo.Tick) * tempo.MicrosecondsPerQuarter / 1000d / TicksPerQuarter;
            }
        }

        private readonly record struct BeatmapTempoPoint(double Time, double BeatLength);
    }
}
