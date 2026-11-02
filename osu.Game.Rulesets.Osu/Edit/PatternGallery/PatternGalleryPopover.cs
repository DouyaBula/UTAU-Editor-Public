// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Graphics.UserInterface.PageSelector;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.Osu.Edit.PatternGallery
{
    /// <summary>
    /// A persistent, editor-wide collection of reusable hit-object patterns. Patterns are stored
    /// as typed clipboard payloads and are always inserted as fresh native hit objects.
    /// </summary>
    public partial class PatternGalleryPopover : EditorPopover
    {
        [Resolved] private EditorBeatmap editorBeatmap { get; set; } = null!;
        [Resolved] private EditorClock editorClock { get; set; } = null!;
        [Resolved] private IBeatSnapProvider beatSnapProvider { get; set; } = null!;
        [Resolved(canBeNull: true)] private IEditorChangeHandler? changeHandler { get; set; }

        private readonly Bindable<string> filter = new Bindable<string>(string.Empty);
        private readonly Bindable<string> patternName = new Bindable<string>(string.Empty);
        private readonly Bindable<string> patternTags = new Bindable<string>(string.Empty);
        private readonly BindableBool snapStart = new BindableBool(true);
        private readonly Bindable<PatternGallerySort> sort = new Bindable<PatternGallerySort>(PatternGallerySort.Recent);
        private readonly Bindable<FileInfo?> importFile = new Bindable<FileInfo?>();
        private const int gallery_page_size = 6;
        private bool rebuildingEntries;

        private PatternGalleryStore? store;
        // These controls are created before the background loader runs. This is intentional:
        // bindables and child drawables can raise callbacks while the popover is being attached,
        // so rebuildEntries() must never observe a partially assigned UI tree.
        private readonly FillFlowContainer entries = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Full,
            Spacing = new Vector2(6)
        };

        private readonly PageSelector galleryPages = new PageSelector();

        private readonly OsuSpriteText entrySummary = new OsuSpriteText
        {
            RelativeSizeAxes = Axes.X,
            Font = OsuFont.Default.With(size: 12),
            Colour = Colour4.LightGray
        };

        private readonly OsuTextFlowContainer status = new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 12))
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Text = string.Empty
        };

        public PatternGalleryPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            store = new PatternGalleryStore(storage);
            patternName.Value = $"Pattern {store.Data.Patterns.Count + 1}";

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            var content = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(4),
                Padding = new MarginPadding(8)
            };

            Child = new OsuScrollContainer
            {
                Width = 480,
                Height = 480,
                Child = content
            };

            content.Add(new OsuSpriteText
            {
                Text = "Pattern gallery",
                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                RelativeSizeAxes = Axes.X
            });
            content.Add(new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 12))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Text = "Save selected objects and reuse them across beatmaps."
            });

            // Keep the two high-frequency actions together in a compact upper row. The gallery
            // itself occupies the full-width lower row so browsing does not compete with capture
            // and insertion controls.
            content.Add(new EditorToolControlRow(200)
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
                Spacing = new Vector2(8, 0),
                Children = new Drawable[]
                {
                    captureGroup(),
                    searchGroup()
                }
            });
            content.Add(dataExchangeGroup());
            content.Add(new EditorToolboxGroup("Saved patterns")
            {
                Spacing = new Vector2(0, 6),
                Children = new Drawable[]
                {
                    entrySummary,
                    entries,
                    galleryPages
                }
            });

            content.Add(status);

            filter.BindValueChanged(_ => resetSearch());
            sort.BindValueChanged(_ => resetSearch());
            galleryPages.CurrentPage.BindValueChanged(_ => rebuildEntries());
            rebuildEntries();
            if (store.LoadError != null)
                setStatus(store.LoadError, true);
            importFile.BindValueChanged(value =>
            {
                if (value.NewValue != null)
                    Schedule(() => importFromFile(value.NewValue));
            });
        }

        private EditorToolboxGroup dataExchangeGroup()
        {
            return new EditorToolboxGroup("Gallery data")
            {
                Spacing = new Vector2(0, 6),
                Children = new Drawable[]
                {
                    new FormFileSelector(".json")
                    {
                        Caption = "Gallery JSON to import",
                        PlaceholderText = "Browse gallery JSON…",
                        Current = importFile,
                        AllowClear = true,
                        HintText = "Choosing a file merges its patterns into this library. Matching IDs are replaced; other patterns are kept.",
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(8, 0),
                        Children = new Drawable[]
                        {
                            new RoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 32,
                                Text = "Export gallery JSON",
                                Action = exportGallery
                            }
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            editorBeatmap.SelectedHitObjects.CollectionChanged += selectionChanged;
            updateSelectionSummary();
        }

        private void selectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => updateSelectionSummary();

        private void resetSearch()
        {
            galleryPages.CurrentPage.Value = 0;
            Scheduler.AddOnce(rebuildEntries);
        }

        private EditorToolboxGroup captureGroup()
            => new EditorToolboxGroup("Capture selection")
            {
                Width = 0.49f,
                Spacing = new Vector2(0, 6),
                Children = new Drawable[]
                {
                    new FormTextBox
                    {
                        Caption = "Pattern name",
                        PlaceholderText = "For example: tech jump ending",
                        Current = patternName,
                        LengthLimit = 80,
                        TabbableContentContainer = this
                    },
                    new FormTextBox
                    {
                        Caption = "Tags (optional)",
                        PlaceholderText = "stream, tech, opener",
                        Current = patternTags,
                        LengthLimit = 160,
                        TabbableContentContainer = this
                    },
                    new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 32,
                        Text = "Save selected objects",
                        Action = saveSelection
                    },
                }
            };

        private EditorToolboxGroup searchGroup()
            => new EditorToolboxGroup("Find & insert")
            {
                Width = 0.49f,
                Spacing = new Vector2(0, 6),
                Children = new Drawable[]
                {
                    new FormTextBox
                    {
                        Caption = "Search",
                        PlaceholderText = "Name or tag",
                        Current = filter,
                        LengthLimit = 100,
                        TabbableContentContainer = this
                    },
                    new FormCheckBox
                    {
                        Caption = "Snap objects and slider tails",
                        Current = snapStart,
                        HintText = "When enabled, every object start and duration-bearing tail is snapped to the destination beatmap timing. Slider velocity is adjusted to keep the tail on the snapped beat."
                    },
                    new FormEnumDropdown<PatternGallerySort>
                    {
                        Caption = "Sort patterns",
                        Current = sort
                    },
                }
            };

        private void saveSelection()
        {
            if (store == null)
            {
                setStatus("The pattern gallery is still loading; try again in a moment.", true);
                return;
            }

            var selected = editorBeatmap.SelectedHitObjects.ToArray();
            if (selected.Length == 0)
            {
                setStatus("Select one or more hit objects first.", true);
                return;
            }

            try
            {
                var entry = PatternGalleryOperations.Capture(selected, patternName.Value, patternTags.Value, editorBeatmap);
                store.Add(entry);
                patternName.Value = $"Pattern {store.Data.Patterns.Count + 1}";
                patternTags.Value = string.Empty;
                rebuildEntries();
                setStatus($"Saved {entry.ObjectCount} object(s) as \"{entry.Name}\".", false);
            }
            catch (Exception e)
            {
                setStatus($"Could not save selection: {e.Message}", true);
            }
        }

        private void insert(PatternGalleryEntry entry)
        {
            if (store == null)
            {
                setStatus("The pattern gallery is still loading; try again in a moment.", true);
                return;
            }

            try
            {
                double targetTime = snapStart.Value
                    ? beatSnapProvider.SnapTime(editorClock.CurrentTime)
                    : editorClock.CurrentTime;
                bool preserveBeatTiming = snapStart.Value && entry.HasTimingMetadata;
                var objects = PatternGalleryOperations.PrepareForInsertion(
                    entry,
                    targetTime,
                    preserveBeatTiming ? editorBeatmap : null);
                if (objects.Count == 0)
                {
                    setStatus($"\"{entry.Name}\" contains no readable objects.", true);
                    return;
                }

                if (snapStart.Value && !preserveBeatTiming)
                    PatternGalleryOperations.SnapToDestinationTiming(objects, editorBeatmap);

                if (changeHandler == null)
                {
                    setStatus("The editor change handler is unavailable; insertion was cancelled.", true);
                    return;
                }

                changeHandler.BeginChange();
                try
                {
                    editorBeatmap.SelectedHitObjects.Clear();
                    editorBeatmap.AddRange(objects);
                    editorBeatmap.SelectedHitObjects.AddRange(objects);
                }
                finally
                {
                    changeHandler.EndChange();
                }

                setStatus($"Inserted {objects.Count} object(s) from \"{entry.Name}\" at {targetTime:0} ms.", false);
                try
                {
                    store.RecordUse(entry);
                }
                catch (Exception e)
                {
                    setStatus($"Inserted {objects.Count} object(s). Could not save usage statistics: {e.Message}", true);
                }
                rebuildEntries();
            }
            catch (Exception e)
            {
                setStatus($"Could not insert \"{entry.Name}\": {e.Message}", true);
            }
        }

        private void remove(PatternGalleryEntry entry)
        {
            if (store == null)
                return;

            try
            {
                store.Remove(entry);
                rebuildEntries();
                setStatus($"Removed \"{entry.Name}\" from the gallery.", false);
            }
            catch (Exception e)
            {
                setStatus($"Could not remove \"{entry.Name}\": {e.Message}", true);
            }
        }

        private void exportGallery()
        {
            if (store == null)
            {
                setStatus("The pattern gallery is still loading; try again in a moment.", true);
                return;
            }

            try
            {
                string path = store.ExportToFile();
                setStatus($"Exported gallery JSON to {Path.GetFileName(path)} ({path}).", false);
            }
            catch (Exception e)
            {
                setStatus($"Could not export the gallery: {e.Message}", true);
            }
        }

        private void importFromFile(FileInfo file)
        {
            if (store == null)
            {
                setStatus("The pattern gallery is still loading; try again in a moment.", true);
                return;
            }

            try
            {
                int imported = store.ImportFromFile(file.FullName);
                patternName.Value = $"Pattern {store.Data.Patterns.Count + 1}";
                rebuildEntries();
                setStatus(imported == 0
                    ? "No new valid patterns were found in that file."
                    : $"Imported {imported} pattern(s) from {file.Name}.", imported == 0);
            }
            catch (Exception e)
            {
                setStatus($"Could not import {file.Name}: {e.Message}", true);
            }
        }

        private void rebuildEntries()
        {
            if (rebuildingEntries)
                return;

            var currentStore = store;
            if (currentStore == null)
                return;

            rebuildingEntries = true;

            try
            {
                entries.Clear();
                string query = filter.Value?.Trim() ?? string.Empty;
                IEnumerable<PatternGalleryEntry> visible = currentStore.Data?.Patterns ?? new List<PatternGalleryEntry>();
                if (query.Length > 0)
                {
                    visible = visible.Where(p => (p.Name ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase)
                                                 || (p.Tags ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase));
                }

                visible = sort.Value switch
                {
                    PatternGallerySort.Name => visible.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase),
                    PatternGallerySort.MostUsed => visible.OrderByDescending(p => p.UseCount).ThenByDescending(p => p.LastUsedUtc),
                    _ => visible.OrderByDescending(p => p.LastUsedUtc)
                };

                var visibleEntries = visible.ToList();
                var allPatterns = currentStore.Data?.Patterns ?? new List<PatternGalleryEntry>();
                entrySummary.Text = visibleEntries.Count == allPatterns.Count
                    ? $"{allPatterns.Count} pattern(s)"
                    : $"{visibleEntries.Count} of {allPatterns.Count} pattern(s)";

                int pageCount = Math.Max(1, (visibleEntries.Count + gallery_page_size - 1) / gallery_page_size);
                galleryPages.AvailablePages.Value = pageCount;
                galleryPages.CurrentPage.Value = Math.Clamp(galleryPages.CurrentPage.Value, 0, pageCount - 1);

                foreach (var entry in visibleEntries.Skip(galleryPages.CurrentPage.Value * gallery_page_size).Take(gallery_page_size))
                    entries.Add(createEntryCard(entry));
            }
            finally
            {
                rebuildingEntries = false;
            }
        }

        private Drawable createEntryCard(PatternGalleryEntry entry)
        {
            var objects = PatternGalleryOperations.Deserialize(entry);
            var patternPreview = new PatternGalleryPreview
            {
                Size = new Vector2(64, 48),
                Position = new Vector2(4, 6)
            };
            var card = new Container
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.49f,
                Height = 100,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(1, 1, 1, 0.06f)
                    },
                    patternPreview,
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = 74, Right = 6, Vertical = 6 },
                        Children = new Drawable[]
                        {
                            new TruncatingSpriteText
                            {
                                Text = entry.Name,
                                Font = OsuFont.Default.With(size: 14, weight: FontWeight.Bold),
                                RelativeSizeAxes = Axes.X
                            },
                            new TruncatingSpriteText
                            {
                                Text = metadata(entry),
                                Y = 23,
                                Font = OsuFont.Default.With(size: 11),
                                Colour = Colour4.LightGray,
                                RelativeSizeAxes = Axes.X
                            }
                        }
                    },
                    new RoundedButton
                    {
                        Text = "Insert",
                        Size = new Vector2(52, 26),
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        X = -36,
                        Y = -6,
                        Action = () => insert(entry)
                    },
                    new RoundedButton
                    {
                        Text = "×",
                        Size = new Vector2(24, 26),
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        X = -6,
                        Y = -6,
                        Action = () => remove(entry)
                    }
                }
            };

            patternPreview.SetObjects(objects);
            return card;
        }

        private static string metadata(PatternGalleryEntry entry)
        {
            string tags = string.IsNullOrWhiteSpace(entry.Tags) ? string.Empty : $" · {entry.Tags}";
            return $"{entry.ObjectCount} object(s) · {entry.Duration:0} ms · used {entry.UseCount}×{tags}";
        }

        private void updateSelectionSummary()
        {
            if (status == null)
                return;

            if (store?.LoadError != null)
            {
                setStatus(store.LoadError, true);
                return;
            }

            int count = editorBeatmap.SelectedHitObjects.Count;
            if (count > 0)
                setStatus($"{count} object(s) selected and ready to save.", false);
        }

        private void setStatus(string text, bool error)
        {
            status.Text = text;
            status.Colour = error ? Colour4.OrangeRed : Colour4.LightGray;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (editorBeatmap != null)
                    editorBeatmap.SelectedHitObjects.CollectionChanged -= selectionChanged;
            }

            base.Dispose(isDisposing);
        }
    }
}
