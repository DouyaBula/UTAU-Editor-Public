// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO.Serialization;

namespace osu.Game.Rulesets.Osu.Edit.PatternGallery
{
    /// <summary>
    /// Persists gallery entries in app storage rather than in a beatmap. This makes the gallery
    /// available from every beatmap while keeping .osu files completely untouched.
    /// </summary>
    public sealed class PatternGalleryStore
    {
        private const string storage_directory = "pattern-gallery";
        private const string storage_file = "gallery.json";
        private const string export_file = "gallery-export.json";

        private readonly Storage storage;

        public PatternGalleryData Data { get; private set; }
        public string? LoadError { get; private set; }

        public PatternGalleryStore(Storage rootStorage)
        {
            storage = rootStorage.GetStorageForDirectory(storage_directory);
            Data = load();
        }

        public void Add(PatternGalleryEntry entry)
        {
            if (!isUsable(entry))
                throw new ArgumentException("The pattern is empty or invalid.", nameof(entry));
            var patterns = Data.Patterns.Where(p => p.Id != entry.Id).ToList();
            patterns.Insert(0, entry);
            savePatterns(patterns);
        }

        public void Remove(PatternGalleryEntry entry)
        {
            savePatterns(Data.Patterns.Where(p => p.Id != entry.Id).ToList());
        }

        public void RecordUse(PatternGalleryEntry entry)
        {
            int count = entry.UseCount;
            DateTime lastUsed = entry.LastUsedUtc;
            entry.UseCount++;
            entry.LastUsedUtc = DateTime.UtcNow;
            try
            {
                savePatterns(Data.Patterns.ToList());
            }
            catch
            {
                entry.UseCount = count;
                entry.LastUsedUtc = lastUsed;
                throw;
            }
        }

        private void savePatterns(List<PatternGalleryEntry> patterns)
        {
            ensureWritable();
            var candidate = new PatternGalleryData { Patterns = patterns };
            write(storage_file, candidate);
            Data = candidate;
        }

        private void ensureWritable()
        {
            if (LoadError != null)
                throw new InvalidOperationException(LoadError);
        }

        private void write(string filename, PatternGalleryData data)
        {
            string json = data.Serialize();
            storage.GetFullPath(".", true);
            using var stream = storage.CreateFileSafely(filename);
            using var writer = new StreamWriter(stream);
            writer.Write(json);
        }

        /// <summary>
        /// Writes a portable copy of the gallery to the editor storage directory and returns its
        /// absolute path. Keeping this separate from the live gallery file means an export can be
        /// copied to another installation without replacing the user's existing collection.
        /// </summary>
        public string ExportToFile()
        {
            ensureWritable();
            write(export_file, Data);
            return storage.GetFullPath(export_file);
        }

        /// <summary>
        /// Imports a gallery JSON file and merges it by pattern id. Existing entries with the
        /// same id are refreshed, while new entries are appended. Invalid entries are ignored so
        /// one stale record cannot make the whole gallery unusable.
        /// </summary>
        public int ImportFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An import file path is required.", nameof(path));

            ensureWritable();
            var imported = File.ReadAllText(path).Deserialize<PatternGalleryData>() ?? throw new InvalidDataException("The gallery JSON is empty.");
            var validEntries = normalise(imported).Patterns.Where(isUsable).ToList();
            int changed = 0;
            var patterns = Data.Patterns.ToList();
            var indices = patterns.Select((entry, index) => (entry.Id, index))
                              .GroupBy(item => item.Id).ToDictionary(group => group.Key, group => group.First().index);

            foreach (var entry in validEntries)
            {
                if (indices.TryGetValue(entry.Id, out int existingIndex))
                    patterns[existingIndex] = entry;
                else
                {
                    indices.Add(entry.Id, patterns.Count);
                    patterns.Add(entry);
                }

                changed++;
            }

            if (changed > 0)
                savePatterns(patterns);

            return changed;
        }

        private PatternGalleryData load()
        {
            try
            {
                if (!storage.Exists(storage_file))
                    return new PatternGalleryData();

                using var stream = storage.GetStream(storage_file, FileAccess.Read, FileMode.Open);
                using var reader = new StreamReader(stream);
                return normalise(reader.ReadToEnd().Deserialize<PatternGalleryData>() ?? throw new InvalidDataException("The gallery JSON is empty."));
            }
            catch (Exception e)
            {
                LoadError = $"Gallery could not be loaded. The original file has been left untouched; saving is disabled. Restore a backup of {storage.GetFullPath(storage_file)} and reopen this panel. {e.Message}";
                Logger.Error(e, LoadError);
                return new PatternGalleryData();
            }
        }

        private static bool isUsable(PatternGalleryEntry? entry)
            => entry != null
               && entry.Id != Guid.Empty
               && !string.IsNullOrWhiteSpace(entry.Name)
               && !string.IsNullOrWhiteSpace(entry.SerializedObjects)
               && entry.ObjectCount > 0;

        private static PatternGalleryData normalise(PatternGalleryData data)
        {
            if (data.FormatVersion > 2)
                throw new InvalidDataException($"Gallery format {data.FormatVersion} is newer than this editor supports.");

            data.Patterns ??= new System.Collections.Generic.List<PatternGalleryEntry>();

            foreach (var pattern in data.Patterns.Where(pattern => pattern != null))
            {
                pattern.RelativeStartBeats ??= new System.Collections.Generic.List<double>();
                pattern.DurationBeats ??= new System.Collections.Generic.List<double>();
            }

            // Ignore malformed entries instead of making the whole gallery unusable. Older
            // versions can still be opened as long as their typed clipboard payload exists.
            data.Patterns = data.Patterns.Where(isUsable).GroupBy(p => p.Id).Select(g => g.Last()).ToList();
            data.FormatVersion = Math.Max(data.FormatVersion, 1);
            return data;
        }
    }
}
