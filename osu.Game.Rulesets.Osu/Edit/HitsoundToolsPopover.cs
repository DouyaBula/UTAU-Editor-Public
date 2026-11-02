// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;

using IOFileInfo = System.IO.FileInfo;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class HitsoundToolsPopover : EditorPopover
    {
        private Bindable<float> targetVolume = null!;
        private Bindable<bool> preserveMuted = null!;
        private Bindable<bool> midiExportSliderEnds = null!;
        private Bindable<bool> midiImportClearExisting = null!;
        private Bindable<float> midiImportTemporalLeniency = null!;
        private Bindable<int> maniaReferenceColumns = null!;
        private Bindable<string> maniaColumnSampleMapping = null!;
        private Bindable<string> maniaDefaultNormalSampleBank = null!;
        private readonly Bindable<ManiaDifficultyChoice?> selectedManiaDifficulty = new Bindable<ManiaDifficultyChoice?>();
        private readonly Bindable<IOFileInfo?> midiImportFile = new Bindable<IOFileInfo?>();

        private Storage exportStorage = null!;
        private FormDropdown<ManiaDifficultyChoice?> maniaDifficultyDropdown = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved(canBeNull: true)]
        private INotificationOverlay? notifications { get; set; }

        public HitsoundToolsPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, Storage storage)
        {
            targetVolume = config.GetBindable<float>(OsuSetting.EditorHitsoundTargetVolume);
            preserveMuted = config.GetBindable<bool>(OsuSetting.EditorHitsoundPreserveMutedVolume);
            midiExportSliderEnds = config.GetBindable<bool>(OsuSetting.EditorHitsoundMidiExportSliderEnds);
            midiImportClearExisting = config.GetBindable<bool>(OsuSetting.EditorHitsoundMidiImportClearExisting);
            midiImportTemporalLeniency = config.GetBindable<float>(OsuSetting.EditorHitsoundMidiImportTemporalLeniency);
            maniaReferenceColumns = config.GetBindable<int>(OsuSetting.EditorHitsoundManiaReferenceColumns);
            maniaColumnSampleMapping = config.GetBindable<string>(OsuSetting.EditorHitsoundManiaColumnSampleMapping);
            maniaDefaultNormalSampleBank = config.GetBindable<string>(OsuSetting.EditorHitsoundManiaDefaultNormalSampleBank);
            exportStorage = (storage as OsuStorage)?.GetExportStorage() ?? storage.GetStorageForDirectory(@"exports");

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            Child = new OsuScrollContainer
            {
                Width = 430,
                Height = 520,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(6),
                    Padding = new MarginPadding(12),
                    Children = new Drawable[]
                    {
                        createHeader("Hitsound tools"),
                        new EditorToolPages(
                            ("Quick edits", new Drawable[]
                            {
                                new RoundedButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = EditorStrings.MuteAllSliderEnds,
                                    Action = () => perform("Muted slider ends", () => EditorHitsoundTools.MuteAllSliderEnds(editorBeatmap))
                                },
                                new RoundedButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = EditorStrings.MuteSelectedSliderEnds,
                                    Action = () => perform("Muted selected slider ends", () => EditorHitsoundTools.MuteSelectedSliderEnds(editorBeatmap))
                                },
                                new FormSliderBar<float>
                                {
                                    Caption = EditorStrings.SelectedObjectVolume,
                                    HintText = "Target volume for selected hit objects and slider node samples. Click the value and type a number for manual input.",
                                    Current = targetVolume,
                                    KeyboardStep = 1f,
                                    LabelFormat = value => $"{value:0}%",
                                    TooltipFormat = value => $"{value:0}%",
                                    TabbableContentContainer = this,
                                },
                                new FormCheckBox
                                {
                                    Caption = EditorStrings.PreserveMutedHitsounds,
                                    HintText = "When enabled, samples already at 5% volume are left unchanged.",
                                    Current = preserveMuted,
                                },
                                new RoundedButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = EditorStrings.ApplyVolumeToSelectedObjects,
                                    Action = () => perform("Updated selected object volume", () => EditorHitsoundTools.SetSelectedObjectVolume(editorBeatmap, clampVolume(targetVolume.Value), preserveMuted.Value))
                                },
                            }),
                            ("Export reference", new Drawable[]
                            {
                                new FormCheckBox
                                {
                                    Caption = EditorStrings.MidiExportSliderEnds,
                                    HintText = "When enabled, final slider tail nodes are exported as a separate MIDI note track.",
                                    Current = midiExportSliderEnds,
                                },
                                new RoundedButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = EditorStrings.ExportBeatmapMidi,
                                    Action = exportMidi
                                },
                                new RoundedButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = EditorStrings.ExportHitsoundNotesDifficulty,
                                    Action = exportNotesDifficulty
                                },
                                new FormSliderBar<int>
                                {
                                    Caption = EditorStrings.HitsoundManiaReferenceColumns,
                                    HintText = "Number of columns in the generated mania reference difficulty. Notes are initially placed in column 1.",
                                    Current = maniaReferenceColumns,
                                    KeyboardStep = 1,
                                    LabelFormat = value => $"{value:0}K",
                                    TooltipFormat = value => $"{value:0}K",
                                    TabbableContentContainer = this,
                                },
                                new RoundedButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = EditorStrings.ExportHitsoundManiaReferenceDifficulty,
                                    Action = exportManiaReferenceDifficulty
                                },
                            }),
                            ("Import / merge", new Drawable[]
                            {
                                new FormCheckBox
                                {
                                    Caption = EditorStrings.MidiImportClearExisting,
                                    HintText = "Applies to both import methods below. Clears samples on ALL objects, including unmatched objects. Undo restores them.",
                                    Current = midiImportClearExisting,
                                },
                                new FormSliderBar<float>
                                {
                                    Caption = "Matching tolerance (MIDI & mania)",
                                    HintText = "Maximum time distance for matching a note to an object or slider tail.",
                                    Current = midiImportTemporalLeniency,
                                    KeyboardStep = 0.5f,
                                    LabelFormat = value => $"{value:0.#} ms",
                                    TooltipFormat = value => $"{value:0.#} ms",
                                    TabbableContentContainer = this,
                                },
                                new EditorToolPages(
                                    ("Mania columns", new Drawable[]
                                    {
                                        maniaDifficultyDropdown = new FormDropdown<ManiaDifficultyChoice?>
                                        {
                                            Caption = EditorStrings.HitsoundManiaSourceDifficulty,
                                            HintText = "The mania difficulty whose columns will be merged into hitsounds on the current osu difficulty.",
                                            Current = selectedManiaDifficulty,
                                        },
                                        new FormTextBox
                                        {
                                            Caption = EditorStrings.HitsoundManiaColumnSampleMapping,
                                            HintText = "Semicolon-separated mappings, for example: 1=normal-hitnormal;2=soft-hitwhistle;3=drum-hitfinish.",
                                            Current = maniaColumnSampleMapping,
                                            TabbableContentContainer = this,
                                        },
                                        new FormTextBox
                                        {
                                            Caption = EditorStrings.HitsoundManiaDefaultNormalSampleBank,
                                            HintText = "Used when a mapped column does not include hitnormal. Valid values: normal, soft, drum.",
                                            Current = maniaDefaultNormalSampleBank,
                                            TabbableContentContainer = this,
                                        },
                                        new RoundedButton
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Text = EditorStrings.MergeManiaHitsoundsIntoCurrentDifficulty,
                                            Action = mergeManiaHitsounds
                                        },
                                    }),
                                    ("MIDI notes", new Drawable[]
                                    {
                                        new FormFileSelector(".mid", ".midi")
                                        {
                                            Caption = EditorStrings.ImportMidiFile,
                                            PlaceholderText = "Choose a .mid or .midi file",
                                            HintText = "MIDI notes are matched to the nearest hit object start or slider tail within the import leniency.",
                                            Current = midiImportFile,
                                        },
                                        new RoundedButton
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Text = EditorStrings.ImportMidiHitsounds,
                                            Action = importMidi
                                        },
                                    }))
                            }))
                    },
                }
            };

            refreshManiaDifficultyChoices();
        }

        private void perform(string message, Func<int> operation)
        {
            changeHandler?.BeginChange();

            int changed;

            try
            {
                changed = operation();
            }
            finally
            {
                changeHandler?.EndChange();
            }

            notifications?.Post(new SimpleNotification
            {
                Text = $"{message}: {changed} object{(changed == 1 ? string.Empty : "s")}."
            });
        }

        private void exportMidi()
        {
            string filename = getAvailableExportFilename(EditorHitsoundMidiTools.CreateExportFilename(editorBeatmap));

            try
            {
                using (var stream = exportStorage.GetStream(filename, FileAccess.Write, FileMode.Create))
                {
                    var result = EditorHitsoundMidiTools.Export(editorBeatmap, stream, midiExportSliderEnds.Value);

                    notifications?.Post(new SimpleNotification
                    {
                        Text = $"Exported MIDI: {result.ObjectNotes} object note{(result.ObjectNotes == 1 ? string.Empty : "s")}, {result.SliderEndNotes} slider end note{(result.SliderEndNotes == 1 ? string.Empty : "s")}.",
                        Activated = () =>
                        {
                            exportStorage.PresentFileExternally(filename);
                            return true;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = $"MIDI export failed: {ex.Message}",
                });
            }
        }

        private void importMidi()
        {
            if (midiImportFile.Value == null)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "Choose a MIDI file before importing.",
                });

                return;
            }

            changeHandler?.BeginChange();

            try
            {
                EditorHitsoundMidiImportResult result;

                using (var stream = midiImportFile.Value.OpenRead())
                    result = EditorHitsoundMidiTools.Import(editorBeatmap, stream, midiImportTemporalLeniency.Value, midiImportClearExisting.Value);

                notifications?.Post(new SimpleNotification
                {
                    Text = $"Imported MIDI hitsounds: {result.MatchedNotes}/{result.ImportedNotes} note{(result.ImportedNotes == 1 ? string.Empty : "s")} matched, {result.UpdatedObjects} object{(result.UpdatedObjects == 1 ? string.Empty : "s")} updated.",
                });
            }
            catch (Exception ex)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = $"MIDI import failed: {ex.Message}",
                });
            }
            finally
            {
                changeHandler?.EndChange();
            }
        }

        private void exportNotesDifficulty()
        {
            try
            {
                var result = EditorHitsoundDifficultyTools.ExportStandardNotesDifficulty(editorBeatmap, beatmapManager, midiExportSliderEnds.Value);

                notifications?.Post(new SimpleNotification
                {
                    Text = $"Created hitsound notes difficulty \"{result.DifficultyName}\" with {result.NoteCount} circle{(result.NoteCount == 1 ? string.Empty : "s")}.",
                });
            }
            catch (Exception ex)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = $"Notes difficulty export failed: {ex.Message}",
                });
            }
        }

        private void exportManiaReferenceDifficulty()
        {
            var maniaRuleset = rulesets.GetRuleset("mania");

            if (maniaRuleset == null)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "Mania ruleset is not available.",
                });

                return;
            }

            try
            {
                var result = EditorHitsoundDifficultyTools.ExportManiaReferenceDifficulty(editorBeatmap, beatmapManager, maniaRuleset, midiExportSliderEnds.Value, maniaReferenceColumns.Value);

                refreshManiaDifficultyChoices();

                notifications?.Post(new SimpleNotification
                {
                    Text = $"Created mania reference difficulty \"{result.DifficultyName}\" with {result.NoteCount} note{(result.NoteCount == 1 ? string.Empty : "s")}.",
                });
            }
            catch (Exception ex)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = $"Mania reference export failed: {ex.Message}",
                });
            }
        }

        private void mergeManiaHitsounds()
        {
            if (selectedManiaDifficulty.Value == null)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "Choose a mania source difficulty before merging.",
                });

                return;
            }

            changeHandler?.BeginChange();

            try
            {
                var result = EditorHitsoundDifficultyTools.MergeManiaHitsoundsInto(editorBeatmap, beatmapManager, selectedManiaDifficulty.Value.BeatmapInfo, maniaColumnSampleMapping.Value,
                    maniaDefaultNormalSampleBank.Value, midiImportTemporalLeniency.Value, midiImportClearExisting.Value);

                notifications?.Post(new SimpleNotification
                {
                    Text = $"Merged mania hitsounds: {result.MatchedNotes}/{result.SourceNotes} note{(result.SourceNotes == 1 ? string.Empty : "s")} matched, {result.UpdatedObjects} object{(result.UpdatedObjects == 1 ? string.Empty : "s")} updated.",
                });
            }
            catch (Exception ex)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = $"Mania hitsound merge failed: {ex.Message}",
                });
            }
            finally
            {
                changeHandler?.EndChange();
            }
        }

        private void refreshManiaDifficultyChoices()
        {
            if (maniaDifficultyDropdown == null)
                return;

            var current = beatmapManager.QueryBeatmap(b => b.ID == editorBeatmap.BeatmapInfo.ID);
            var choices = current?.BeatmapSet?.Beatmaps
                                 .Where(b => b.Ruleset.ShortName == "mania")
                                 .OrderBy(b => b.DifficultyName)
                                 .Select(b => new ManiaDifficultyChoice(b))
                                 .ToArray() ?? Array.Empty<ManiaDifficultyChoice>();

            // Re-enable before assigning Value. If the previous refresh had no mania difficulties,
            // this bindable is disabled and setting Value first will throw.
            selectedManiaDifficulty.Disabled = false;
            maniaDifficultyDropdown.Items = choices;
            selectedManiaDifficulty.Value = choices.FirstOrDefault();
            selectedManiaDifficulty.Disabled = choices.Length == 0;
        }

        private string getAvailableExportFilename(string filename)
        {
            string extension = Path.GetExtension(filename);
            string withoutExtension = Path.GetFileNameWithoutExtension(filename);

            if (!exportStorage.Exists(filename))
                return filename;

            for (int i = 1; i < int.MaxValue; i++)
            {
                string indexed = $"{withoutExtension}-{i}{extension}";

                if (!exportStorage.Exists(indexed))
                    return indexed;
            }

            throw new InvalidOperationException("Could not reserve an export filename.");
        }

        private static OsuSpriteText createHeader(LocalisableString text) => new OsuSpriteText
        {
            Text = text,
            Font = OsuFont.GetFont(size: 16, weight: FontWeight.SemiBold),
            Margin = new MarginPadding { Top = 6 },
        };

        private static int clampVolume(float volume) => Math.Clamp((int)Math.Round(volume), EditorHitsoundTools.MUTED_VOLUME, 100);

        private sealed class ManiaDifficultyChoice
        {
            public readonly BeatmapInfo BeatmapInfo;

            public ManiaDifficultyChoice(BeatmapInfo beatmapInfo)
            {
                BeatmapInfo = beatmapInfo;
            }

            public override string ToString() => BeatmapInfo.DifficultyName;
        }
    }
}
