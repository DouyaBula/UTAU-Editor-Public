// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Editor";

        /// <summary>
        /// "Beatmap editor"
        /// </summary>
        public static LocalisableString BeatmapEditor => new TranslatableString(getKey(@"beatmap_editor"), @"Beatmap editor");

        /// <summary>
        /// "Waveform opacity"
        /// </summary>
        public static LocalisableString WaveformOpacity => new TranslatableString(getKey(@"waveform_opacity"), @"Waveform opacity");

        /// <summary>
        /// "Show storyboard"
        /// </summary>
        public static LocalisableString ShowStoryboard => new TranslatableString(getKey(@"show_storyboard"), @"Show storyboard");

        /// <summary>
        /// "Show hit markers"
        /// </summary>
        public static LocalisableString ShowHitMarkers => new TranslatableString(getKey(@"show_hit_markers"), @"Show hit markers");

        /// <summary>
        /// "Selection outline thickness"
        /// </summary>
        public static LocalisableString SelectionOutlineThickness => new TranslatableString(getKey(@"selection_outline_thickness"), @"Selection outline thickness");

        /// <summary>
        /// "UTAU_Editor"
        /// </summary>
        public static LocalisableString UtauEditor => new TranslatableString(getKey(@"utau_editor"), @"UTAU_Editor");

        /// <summary>
        /// "Workflow"
        /// </summary>
        public static LocalisableString Workflow => new TranslatableString(getKey(@"workflow"), @"Workflow");

        /// <summary>
        /// "Slider tools"
        /// </summary>
        public static LocalisableString SliderTools => new TranslatableString(getKey(@"slider_tools"), @"Slider tools");

        /// <summary>
        /// "Hitsound copier"
        /// </summary>
        public static LocalisableString HitsoundCopier => new TranslatableString(getKey(@"hitsound_copier"), @"Hitsound copier");

        /// <summary>
        /// "Hitsound tools"
        /// </summary>
        public static LocalisableString HitsoundTools => new TranslatableString(getKey(@"hitsound_tools"), @"Hitsound tools");

        /// <summary>
        /// "Temporal leniency"
        /// </summary>
        public static LocalisableString HitsoundCopyTemporalLeniency => new TranslatableString(getKey(@"hitsound_copy_temporal_leniency"), @"Temporal leniency");

        /// <summary>
        /// "Overwrite everything"
        /// </summary>
        public static LocalisableString HitsoundCopyOverwriteEverything => new TranslatableString(getKey(@"hitsound_copy_overwrite_everything"), @"Overwrite everything");

        /// <summary>
        /// "Overwrite only defined"
        /// </summary>
        public static LocalisableString HitsoundCopyOverwriteOnlyDefined => new TranslatableString(getKey(@"hitsound_copy_overwrite_only_defined"), @"Overwrite only defined");

        /// <summary>
        /// "Mute all slider ends"
        /// </summary>
        public static LocalisableString MuteAllSliderEnds => new TranslatableString(getKey(@"mute_all_slider_ends"), @"Mute all slider ends");

        /// <summary>
        /// "Mute selected slider ends"
        /// </summary>
        public static LocalisableString MuteSelectedSliderEnds => new TranslatableString(getKey(@"mute_selected_slider_ends"), @"Mute selected slider ends");

        /// <summary>
        /// "Selected object volume"
        /// </summary>
        public static LocalisableString SelectedObjectVolume => new TranslatableString(getKey(@"selected_object_volume"), @"Selected object volume");

        /// <summary>
        /// "Preserve muted 5% samples"
        /// </summary>
        public static LocalisableString PreserveMutedHitsounds => new TranslatableString(getKey(@"preserve_muted_hitsounds"), @"Preserve muted 5% samples");

        /// <summary>
        /// "Apply volume to selected objects"
        /// </summary>
        public static LocalisableString ApplyVolumeToSelectedObjects => new TranslatableString(getKey(@"apply_volume_to_selected_objects"), @"Apply volume to selected objects");

        /// <summary>
        /// "DAW-style hitsound workflow"
        /// </summary>
        public static LocalisableString MidiHitsoundWorkflow => new TranslatableString(getKey(@"midi_hitsound_workflow"), @"DAW-style hitsound workflow");

        /// <summary>
        /// "Export slider ends"
        /// </summary>
        public static LocalisableString MidiExportSliderEnds => new TranslatableString(getKey(@"midi_export_slider_ends"), @"Export slider ends");

        /// <summary>
        /// "Export beatmap MIDI"
        /// </summary>
        public static LocalisableString ExportBeatmapMidi => new TranslatableString(getKey(@"export_beatmap_midi"), @"Export beatmap MIDI");

        /// <summary>
        /// "Export notes difficulty"
        /// </summary>
        public static LocalisableString ExportHitsoundNotesDifficulty => new TranslatableString(getKey(@"export_hitsound_notes_difficulty"), @"Export notes difficulty");

        /// <summary>
        /// "Mania reference columns"
        /// </summary>
        public static LocalisableString HitsoundManiaReferenceColumns => new TranslatableString(getKey(@"hitsound_mania_reference_columns"), @"Mania reference columns");

        /// <summary>
        /// "Export mania reference difficulty"
        /// </summary>
        public static LocalisableString ExportHitsoundManiaReferenceDifficulty => new TranslatableString(getKey(@"export_hitsound_mania_reference_difficulty"), @"Export mania reference difficulty");

        /// <summary>
        /// "Mania source difficulty"
        /// </summary>
        public static LocalisableString HitsoundManiaSourceDifficulty => new TranslatableString(getKey(@"hitsound_mania_source_difficulty"), @"Mania source difficulty");

        /// <summary>
        /// "Column hitsound mapping"
        /// </summary>
        public static LocalisableString HitsoundManiaColumnSampleMapping => new TranslatableString(getKey(@"hitsound_mania_column_sample_mapping"), @"Column hitsound mapping");

        /// <summary>
        /// "Default normal sample bank"
        /// </summary>
        public static LocalisableString HitsoundManiaDefaultNormalSampleBank => new TranslatableString(getKey(@"hitsound_mania_default_normal_sample_bank"), @"Default normal sample bank");

        /// <summary>
        /// "Merge mania hitsounds into current difficulty"
        /// </summary>
        public static LocalisableString MergeManiaHitsoundsIntoCurrentDifficulty => new TranslatableString(getKey(@"merge_mania_hitsounds_into_current_difficulty"), @"Merge mania hitsounds into current difficulty");

        /// <summary>
        /// "Import MIDI file"
        /// </summary>
        public static LocalisableString ImportMidiFile => new TranslatableString(getKey(@"import_midi_file"), @"Import MIDI file");

        /// <summary>
        /// "Clear existing hitsounds before MIDI import"
        /// </summary>
        public static LocalisableString MidiImportClearExisting => new TranslatableString(getKey(@"midi_import_clear_existing"), @"Clear existing hitsounds before MIDI import");

        /// <summary>
        /// "MIDI import temporal leniency"
        /// </summary>
        public static LocalisableString MidiImportTemporalLeniency => new TranslatableString(getKey(@"midi_import_temporal_leniency"), @"MIDI import temporal leniency");

        /// <summary>
        /// "Import MIDI hitsounds"
        /// </summary>
        public static LocalisableString ImportMidiHitsounds => new TranslatableString(getKey(@"import_midi_hitsounds"), @"Import MIDI hitsounds");

        /// <summary>
        /// "Tangent glitch distance"
        /// </summary>
        public static LocalisableString TangentGlitchDistance => new TranslatableString(getKey(@"tangent_glitch_distance"), @"Tangent glitch distance");

        /// <summary>
        /// "Selection visuals"
        /// </summary>
        public static LocalisableString SelectionVisuals => new TranslatableString(getKey(@"selection_visuals"), @"Selection visuals");

        /// <summary>
        /// "Slider path visuals"
        /// </summary>
        public static LocalisableString SliderPathVisuals => new TranslatableString(getKey(@"slider_path_visuals"), @"Slider path visuals");

        /// <summary>
        /// "Nudge distance"
        /// </summary>
        public static LocalisableString NudgeDistance => new TranslatableString(getKey(@"nudge_distance"), @"Nudge distance");

        /// <summary>
        /// "Selection box outline thickness"
        /// </summary>
        public static LocalisableString SelectionBoxOutlineThickness => new TranslatableString(getKey(@"selection_box_outline_thickness"), @"Selection box outline thickness");

        /// <summary>
        /// "Selection colours"
        /// </summary>
        public static LocalisableString SelectionColours => new TranslatableString(getKey(@"selection_colours"), @"Selection colours");

        /// <summary>
        /// "Selection outline colour"
        /// </summary>
        public static LocalisableString SelectionOutlineColour => new TranslatableString(getKey(@"selection_outline_colour"), @"Selection outline colour");

        /// <summary>
        /// "Circle outline colour"
        /// </summary>
        public static LocalisableString SelectionHitCircleOutlineColour => new TranslatableString(getKey(@"selection_hit_circle_outline_colour"), @"Circle outline colour");

        /// <summary>
        /// "Slider body outline colour"
        /// </summary>
        public static LocalisableString SelectionSliderBodyOutlineColour => new TranslatableString(getKey(@"selection_slider_body_outline_colour"), @"Slider body outline colour");

        /// <summary>
        /// "Selection box colour"
        /// </summary>
        public static LocalisableString SelectionBoxColour => new TranslatableString(getKey(@"selection_box_colour"), @"Selection box colour");

        /// <summary>
        /// "Selection control colour"
        /// </summary>
        public static LocalisableString SelectionControlColour => new TranslatableString(getKey(@"selection_control_colour"), @"Selection control colour");

        /// <summary>
        /// "Selection button size"
        /// </summary>
        public static LocalisableString SelectionControlButtonSize => new TranslatableString(getKey(@"selection_control_button_size"), @"Selection button size");

        /// <summary>
        /// "Selection scale handle size"
        /// </summary>
        public static LocalisableString SelectionScaleHandleSize => new TranslatableString(getKey(@"selection_scale_handle_size"), @"Selection scale handle size");

        /// <summary>
        /// "Selection rotation handle size"
        /// </summary>
        public static LocalisableString SelectionRotationHandleSize => new TranslatableString(getKey(@"selection_rotation_handle_size"), @"Selection rotation handle size");

        /// <summary>
        /// "Slider control points"
        /// </summary>
        public static LocalisableString SliderControlPoints => new TranslatableString(getKey(@"slider_control_points"), @"Slider control points");

        /// <summary>
        /// "Snapping"
        /// </summary>
        public static LocalisableString Snapping => new TranslatableString(getKey(@"snapping"), @"Snapping");

        /// <summary>
        /// "Control point colour"
        /// </summary>
        public static LocalisableString SliderControlPointColour => new TranslatableString(getKey(@"slider_control_point_colour"), @"Control point colour");

        /// <summary>
        /// "Inherited point colour"
        /// </summary>
        public static LocalisableString SliderControlPointInheritedColour => new TranslatableString(getKey(@"slider_control_point_inherited_colour"), @"Inherited point colour");

        /// <summary>
        /// "Catmull point colour"
        /// </summary>
        public static LocalisableString SliderControlPointCatmullColour => new TranslatableString(getKey(@"slider_control_point_catmull_colour"), @"Catmull point colour");

        /// <summary>
        /// "Bezier point colour"
        /// </summary>
        public static LocalisableString SliderControlPointBezierColour => new TranslatableString(getKey(@"slider_control_point_bezier_colour"), @"Bezier point colour");

        /// <summary>
        /// "Linear point colour"
        /// </summary>
        public static LocalisableString SliderControlPointLinearColour => new TranslatableString(getKey(@"slider_control_point_linear_colour"), @"Linear point colour");

        /// <summary>
        /// "Perfect curve point colour"
        /// </summary>
        public static LocalisableString SliderControlPointPerfectCurveColour => new TranslatableString(getKey(@"slider_control_point_perfect_curve_colour"), @"Perfect curve point colour");

        /// <summary>
        /// "B-Spline degree 1 point colour"
        /// </summary>
        public static LocalisableString SliderControlPointBSplineDegree1Colour => new TranslatableString(getKey(@"slider_control_point_b_spline_degree_1_colour"), @"B-Spline degree 1 point colour");

        /// <summary>
        /// "B-Spline degree 2 point colour"
        /// </summary>
        public static LocalisableString SliderControlPointBSplineDegree2Colour => new TranslatableString(getKey(@"slider_control_point_b_spline_degree_2_colour"), @"B-Spline degree 2 point colour");

        /// <summary>
        /// "B-Spline degree 3 point colour"
        /// </summary>
        public static LocalisableString SliderControlPointBSplineDegree3Colour => new TranslatableString(getKey(@"slider_control_point_b_spline_degree_3_colour"), @"B-Spline degree 3 point colour");

        /// <summary>
        /// "B-Spline degree 4+ point colour"
        /// </summary>
        public static LocalisableString SliderControlPointBSplineDegree4Colour => new TranslatableString(getKey(@"slider_control_point_b_spline_degree_4_colour"), @"B-Spline degree 4+ point colour");

        /// <summary>
        /// "Control line colour"
        /// </summary>
        public static LocalisableString SliderControlLineColour => new TranslatableString(getKey(@"slider_control_line_colour"), @"Control line colour");

        /// <summary>
        /// "Slider control point snap targets"
        /// </summary>
        public static LocalisableString SliderControlPointSnapTargets => new TranslatableString(getKey(@"slider_control_point_snap_targets"), @"Slider control point snap targets");

        /// <summary>
        /// "Nearby object snap targets"
        /// </summary>
        public static LocalisableString NearbyObjectSnapTargets => new TranslatableString(getKey(@"nearby_object_snap_targets"), @"Nearby object snap targets");

        /// <summary>
        /// "Snap target threshold"
        /// </summary>
        public static LocalisableString SliderControlPointSnapTargetThreshold => new TranslatableString(getKey(@"slider_control_point_snap_target_threshold"), @"Snap target threshold");

        /// <summary>
        /// "Control point size"
        /// </summary>
        public static LocalisableString SliderControlPointSize => new TranslatableString(getKey(@"slider_control_point_size"), @"Control point size");

        /// <summary>
        /// "Control point click size"
        /// </summary>
        public static LocalisableString SliderControlPointHitAreaSize => new TranslatableString(getKey(@"slider_control_point_hit_area_size"), @"Control point click size");

        /// <summary>
        /// "Control line thickness"
        /// </summary>
        public static LocalisableString SliderControlLineThickness => new TranslatableString(getKey(@"slider_control_line_thickness"), @"Control line thickness");

        /// <summary>
        /// "Slider tail arc colour"
        /// </summary>
        public static LocalisableString SliderTailArcColour => new TranslatableString(getKey(@"slider_tail_arc_colour"), @"Slider tail arc colour");

        /// <summary>
        /// "Slider tail arc thickness"
        /// </summary>
        public static LocalisableString SliderTailArcThickness => new TranslatableString(getKey(@"slider_tail_arc_thickness"), @"Slider tail arc thickness");

        /// <summary>
        /// "Fixed-duration slider SV step"
        /// </summary>
        public static LocalisableString FixedDurationSliderVelocityDelta => new TranslatableString(getKey(@"fixed_duration_slider_velocity_delta"), @"Fixed-duration slider SV step");

        /// <summary>
        /// "Out-of-bounds placement"
        /// </summary>
        public static LocalisableString OutOfBoundsPlacement => new TranslatableString(getKey(@"out_of_bounds_placement"), @"Out-of-bounds placement");

        /// <summary>
        /// "Convert to stream curve"
        /// </summary>
        public static LocalisableString ConvertToStreamCurve => new TranslatableString(getKey(@"convert_to_stream_curve"), @"Convert to stream curve");

        /// <summary>
        /// "Toggle approach rate"
        /// </summary>
        public static LocalisableString ToggleApproachRate => new TranslatableString(getKey(@"toggle_approach_rate"), @"Toggle approach rate");

        /// <summary>
        /// "Approach rate toggle value A"
        /// </summary>
        public static LocalisableString ApproachRateToggleValueA => new TranslatableString(getKey(@"approach_rate_toggle_value_a"), @"Approach rate toggle value A");

        /// <summary>
        /// "Approach rate toggle value B"
        /// </summary>
        public static LocalisableString ApproachRateToggleValueB => new TranslatableString(getKey(@"approach_rate_toggle_value_b"), @"Approach rate toggle value B");

        /// <summary>
        /// "Canvas zoom"
        /// </summary>
        public static LocalisableString CanvasZoom => new TranslatableString(getKey(@"canvas_zoom"), @"Canvas zoom");

        /// <summary>
        /// "Zoom step"
        /// </summary>
        public static LocalisableString CanvasZoomStep => new TranslatableString(getKey(@"canvas_zoom_step"), @"Zoom step");

        /// <summary>
        /// "Reset zoom"
        /// </summary>
        public static LocalisableString CanvasZoomResetValue => new TranslatableString(getKey(@"canvas_zoom_reset_value"), @"Reset zoom");

        /// <summary>
        /// "Zoom in"
        /// </summary>
        public static LocalisableString CanvasZoomIn => new TranslatableString(getKey(@"canvas_zoom_in"), @"Zoom in");

        /// <summary>
        /// "Zoom out"
        /// </summary>
        public static LocalisableString CanvasZoomOut => new TranslatableString(getKey(@"canvas_zoom_out"), @"Zoom out");

        /// <summary>
        /// "Reset"
        /// </summary>
        public static LocalisableString Reset => new TranslatableString(getKey(@"reset"), @"Reset");

        /// <summary>
        /// "Automatically seek after placing objects"
        /// </summary>
        public static LocalisableString AutoSeekOnPlacement => new TranslatableString(getKey(@"auto_seek_on_placement"), @"Automatically seek after placing objects");

        /// <summary>
        /// "Timing"
        /// </summary>
        public static LocalisableString Timing => new TranslatableString(getKey(@"timing"), @"Timing");

        /// <summary>
        /// "Set preview point to current time"
        /// </summary>
        public static LocalisableString SetPreviewPointToCurrent => new TranslatableString(getKey(@"set_preview_point_to_current"), @"Set preview point to current time");

        /// <summary>
        /// "Snap all notes to current snap divisor..."
        /// </summary>
        public static LocalisableString SnapAllNotesToCurrentSnapDivisor => new TranslatableString(getKey(@"snap_all_notes_to_current_snap_divisor"), @"Snap all notes to current snap divisor...");

        /// <summary>
        /// "Move already placed objects when changing timing"
        /// </summary>
        public static LocalisableString AdjustExistingObjectsOnTimingChanges =>
            new TranslatableString(getKey(@"adjust_existing_objects_on_timing_changes"), @"Move already placed objects when changing timing");

        /// <summary>
        /// "For editing (.olz)"
        /// </summary>
        public static LocalisableString ExportForEditing => new TranslatableString(getKey(@"export_for_editing"), @"For editing (.olz)");

        /// <summary>
        /// "For compatibility (.osz)"
        /// </summary>
        public static LocalisableString ExportForCompatibility => new TranslatableString(getKey(@"export_for_compatibility"), @"For compatibility (.osz)");

        /// <summary>
        /// "Guest difficulty (.osu)"
        /// </summary>
        public static LocalisableString ExportGuestDifficulty => new TranslatableString(getKey(@"export_guest_difficulty"), @"Guest difficulty (.osu)");

        /// <summary>
        /// "Create new difficulty"
        /// </summary>
        public static LocalisableString CreateNewDifficulty => new TranslatableString(getKey(@"create_new_difficulty"), @"Create new difficulty");

        /// <summary>
        /// "Change difficulty"
        /// </summary>
        public static LocalisableString ChangeDifficulty => new TranslatableString(getKey(@"change_difficulty"), @"Change difficulty");

        /// <summary>
        /// "Delete difficulty..."
        /// </summary>
        public static LocalisableString DeleteDifficulty => new TranslatableString(getKey(@"delete_difficulty"), @"Delete difficulty...");

        /// <summary>
        /// "Edit externally..."
        /// </summary>
        public static LocalisableString EditExternally => new TranslatableString(getKey(@"edit_externally"), @"Edit externally...");

        /// <summary>
        /// "Submit beatmap..."
        /// </summary>
        public static LocalisableString SubmitBeatmap => new TranslatableString(getKey(@"submit_beatmap"), @"Submit beatmap...");

        /// <summary>
        /// "setup"
        /// </summary>
        public static LocalisableString SetupScreen => new TranslatableString(getKey(@"setup_screen"), @"setup");

        /// <summary>
        /// "compose"
        /// </summary>
        public static LocalisableString ComposeScreen => new TranslatableString(getKey(@"compose_screen"), @"compose");

        /// <summary>
        /// "design"
        /// </summary>
        public static LocalisableString DesignScreen => new TranslatableString(getKey(@"design_screen"), @"design");

        /// <summary>
        /// "timing"
        /// </summary>
        public static LocalisableString TimingScreen => new TranslatableString(getKey(@"timing_screen"), @"timing");

        /// <summary>
        /// "verify"
        /// </summary>
        public static LocalisableString VerifyScreen => new TranslatableString(getKey(@"verify_screen"), @"verify");

        /// <summary>
        /// "Playback speed"
        /// </summary>
        public static LocalisableString PlaybackSpeed => new TranslatableString(getKey(@"playback_speed"), @"Playback speed");

        /// <summary>
        /// "Test!"
        /// </summary>
        public static LocalisableString TestBeatmap => new TranslatableString(getKey(@"test_beatmap"), @"Test!");

        /// <summary>
        /// "{0:0}&#176;"
        /// </summary>
        public static LocalisableString RotationUnsnapped(float newRotation) => new TranslatableString(getKey(@"rotation_unsnapped"), @"{0:0}°", newRotation);

        /// <summary>
        /// "{0:0}&#176; (snapped)"
        /// </summary>
        public static LocalisableString RotationSnapped(float newRotation) => new TranslatableString(getKey(@"rotation_snapped"), @"{0:0}° (snapped)", newRotation);

        /// <summary>
        /// "Limit placement to current time"
        /// </summary>
        public static LocalisableString LimitedDistanceSnap => new TranslatableString(getKey(@"limit_placement_to_current_time"), @"Limit placement to current time");

        /// <summary>
        /// "Contract sidebars when not hovered"
        /// </summary>
        public static LocalisableString ContractSidebars => new TranslatableString(getKey(@"contract_sidebars"), @"Contract sidebars when not hovered");

        /// <summary>
        /// "Must be in edit mode to handle editor links"
        /// </summary>
        public static LocalisableString MustBeInEditorToHandleLinks => new TranslatableString(getKey(@"must_be_in_editor_to_handle_links"), @"Must be in edit mode to handle editor links");

        /// <summary>
        /// "Failed to parse editor link"
        /// </summary>
        public static LocalisableString FailedToParseEditorLink => new TranslatableString(getKey(@"failed_to_parse_edtior_link"), @"Failed to parse editor link");

        /// <summary>
        /// "Timeline"
        /// </summary>
        public static LocalisableString Timeline => new TranslatableString(getKey(@"timeline"), @"Timeline");

        /// <summary>
        /// "Show timing changes"
        /// </summary>
        public static LocalisableString TimelineShowTimingChanges => new TranslatableString(getKey(@"timeline_show_timing_changes"), @"Show timing changes");

        /// <summary>
        /// "Finish editing and import changes"
        /// </summary>
        public static LocalisableString FinishEditingExternally => new TranslatableString(getKey(@"Finish editing and import changes"), @"Finish editing and import changes");

        /// <summary>
        /// "Show breaks"
        /// </summary>
        public static LocalisableString TimelineShowBreaks => new TranslatableString(getKey(@"timeline_show_breaks"), @"Show breaks");

        /// <summary>
        /// "Show ticks"
        /// </summary>
        public static LocalisableString TimelineShowTicks => new TranslatableString(getKey(@"timeline_show_ticks"), @"Show ticks");

        /// <summary>
        /// "Bookmarks"
        /// </summary>
        public static LocalisableString Bookmarks => new TranslatableString(getKey(@"bookmarks"), @"Bookmarks");

        /// <summary>
        /// "Add bookmark"
        /// </summary>
        public static LocalisableString AddBookmark => new TranslatableString(getKey(@"add_bookmark"), @"Add bookmark");

        /// <summary>
        /// "Remove closest bookmark"
        /// </summary>
        public static LocalisableString RemoveClosestBookmark => new TranslatableString(getKey(@"remove_closest_bookmark"), @"Remove closest bookmark");

        /// <summary>
        /// "Seek to previous bookmark"
        /// </summary>
        public static LocalisableString SeekToPreviousBookmark => new TranslatableString(getKey(@"seek_to_previous_bookmark"), @"Seek to previous bookmark");

        /// <summary>
        /// "Seek to next bookmark"
        /// </summary>
        public static LocalisableString SeekToNextBookmark => new TranslatableString(getKey(@"seek_to_next_bookmark"), @"Seek to next bookmark");

        /// <summary>
        /// "Reset bookmarks..."
        /// </summary>
        public static LocalisableString ResetBookmarks => new TranslatableString(getKey(@"reset_bookmarks..."), @"Reset bookmarks...");

        /// <summary>
        /// "Open beatmap info page"
        /// </summary>
        public static LocalisableString OpenInfoPage => new TranslatableString(getKey(@"open_info_page"), @"Open beatmap info page");

        /// <summary>
        /// "Open beatmap discussion page"
        /// </summary>
        public static LocalisableString OpenDiscussionPage => new TranslatableString(getKey(@"open_discussion_page"), @"Open beatmap discussion page");

        /// <summary>
        /// "Current difficulty"
        /// </summary>
        public static LocalisableString CheckCurrentDifficulty => new TranslatableString(getKey(@"check_current_difficulty"), @"Current difficulty");

        /// <summary>
        /// "Entire beatmap set"
        /// </summary>
        public static LocalisableString CheckEntireBeatmapSet => new TranslatableString(getKey(@"check_entire_beatmap_set"), @"Entire beatmap set");

        /// <summary>
        /// "Saving is not supported for this ruleset yet, sorry!"
        /// </summary>
        public static LocalisableString RulesetNotSupportSaving => new TranslatableString(getKey(@"ruleset_not_support_saving"), @"Saving is not supported for this ruleset yet, sorry!");

        /// <summary>
        /// "Export failed!"
        /// </summary>
        public static LocalisableString ExportFailed => new TranslatableString(getKey(@"export_failed"), @"Export failed!");

        /// <summary>
        /// "Import failed!"
        /// </summary>
        public static LocalisableString ImportFailed => new TranslatableString(getKey(@"import_failed"), @"Import failed!");

        /// <summary>
        /// "Open folder"
        /// </summary>
        public static LocalisableString OpenFolder => new TranslatableString(getKey(@"open_folder"), @"Open folder");

        /// <summary>
        /// "Cleaning up..."
        /// </summary>
        public static LocalisableString ExternalEditCleaningUp => new TranslatableString(getKey(@"external_edit_cleaning_up"), @"Cleaning up...");

        /// <summary>
        /// "Exporting for edit..."
        /// </summary>
        public static LocalisableString ExternalEditExporting => new TranslatableString(getKey(@"external_edit_exporting"), @"Exporting for edit...");

        /// <summary>
        /// "Beatmap is mounted externally"
        /// </summary>
        public static LocalisableString BeatmapMountedExternally => new TranslatableString(getKey(@"beatmap_mounted_externally"), @"Beatmap is mounted externally");

        /// <summary>
        /// "Any changes made to the exported folder will be imported to the game, including file additions, modifications and deletions."
        /// </summary>
        public static LocalisableString ExternalEditMountedExplanation => new TranslatableString(getKey(@"external_edit_mounted_explanation"),
            @"Any changes made to the exported folder will be imported to the game, including file additions, modifications and deletions.");

        /// <summary>
        /// "New combo"
        /// </summary>
        public static LocalisableString NewCombo => new TranslatableString(getKey(@"new_combo"), @"New combo");

        /// <summary>
        /// "Bank"
        /// </summary>
        public static LocalisableString Bank => new TranslatableString(getKey(@"bank"), @"Bank");

        /// <summary>
        /// "Normal bank"
        /// </summary>
        public static LocalisableString NormalBank => new TranslatableString(getKey(@"normal_bank"), @"Normal bank");

        /// <summary>
        /// "Addition bank"
        /// </summary>
        public static LocalisableString AdditionBank => new TranslatableString(getKey(@"addition_bank"), @"Addition bank");

        /// <summary>
        /// "Volume"
        /// </summary>
        public static LocalisableString SampleVolume => new TranslatableString(getKey(@"sample_volume"), @"Volume");

        /// <summary>
        /// "Sample"
        /// </summary>
        public static LocalisableString Sample => new TranslatableString(getKey(@"sample"), @"Sample");

        /// <summary>
        /// "Sample set"
        /// </summary>
        public static LocalisableString SampleSet => new TranslatableString(getKey(@"sample_set"), @"Sample set");

        /// <summary>
        /// "Select"
        /// </summary>
        public static LocalisableString SelectTool => new TranslatableString(getKey(@"select_tool"), @"Select");

        /// <summary>
        /// "Synchronise..."
        /// </summary>
        public static LocalisableString Synchronise => new TranslatableString(getKey(@"synchronise"), @"Synchronise...");

        /// <summary>
        /// "Seek backwards"
        /// </summary>
        public static LocalisableString SeekBackwards => new TranslatableString(getKey(@"seek_backwards"), @"Seek backwards");

        /// <summary>
        /// "Seek forwards"
        /// </summary>
        public static LocalisableString SeekForwards => new TranslatableString(getKey(@"seek_forwards"), @"Seek forwards");

        /// <summary>
        /// "Seek to previous timing point"
        /// </summary>
        public static LocalisableString SeekToPreviousTimingPoint => new TranslatableString(getKey(@"seek_to_previous_timing_point"), @"Seek to previous timing point");

        /// <summary>
        /// "Seek to next timing point"
        /// </summary>
        public static LocalisableString SeekToNextTimingPoint => new TranslatableString(getKey(@"seek_to_next_timing_point"), @"Seek to next timing point");

        /// <summary>
        /// "Seek to first object / track start"
        /// </summary>
        public static LocalisableString SeekToStart => new TranslatableString(getKey(@"seek_to_start"), @"Seek to first object / track start");

        /// <summary>
        /// "Play from track start"
        /// </summary>
        public static LocalisableString PlayFromStart => new TranslatableString(getKey(@"play_from_start"), @"Play from track start");

        /// <summary>
        /// "Toggle pause"
        /// </summary>
        public static LocalisableString TogglePause => new TranslatableString(getKey(@"toggle_pause"), @"Toggle pause");

        /// <summary>
        /// "Seek to last object / track end"
        /// </summary>
        public static LocalisableString SeekToEnd => new TranslatableString(getKey(@"seek_to_end"), @"Seek to last object / track end");

        /// <summary>
        /// "Toggle new combo"
        /// </summary>
        public static LocalisableString ToggleNewCombo => new TranslatableString(getKey(@"toggle_new_combo"), @"Toggle new combo");

        /// <summary>
        /// "Toggle whistle sound"
        /// </summary>
        public static LocalisableString ToggleWhistleSound => new TranslatableString(getKey(@"toggle_whistle_sound"), @"Toggle whistle sound");

        /// <summary>
        /// "Toggle finish sound"
        /// </summary>
        public static LocalisableString ToggleFinishSound => new TranslatableString(getKey(@"toggle_finish_sound"), @"Toggle finish sound");

        /// <summary>
        /// "Toggle clap sound"
        /// </summary>
        public static LocalisableString ToggleClapSound => new TranslatableString(getKey(@"toggle_clap_sound"), @"Toggle clap sound");

        /// <summary>
        /// "Set normal sample bank to auto"
        /// </summary>
        public static LocalisableString ToggleNormalAutoBank => new TranslatableString(getKey(@"toggle_normal_auto_bank"), @"Set normal sample bank to auto");

        /// <summary>
        /// "Set normal sample bank to normal"
        /// </summary>
        public static LocalisableString ToggleNormalNormalBank => new TranslatableString(getKey(@"toggle_normal_normal_bank"), @"Set normal sample bank to normal");

        /// <summary>
        /// "Set normal sample bank to soft"
        /// </summary>
        public static LocalisableString ToggleNormalSoftBank => new TranslatableString(getKey(@"toggle_normal_soft_bank"), @"Set normal sample bank to soft");

        /// <summary>
        /// "Set normal sample bank to drum"
        /// </summary>
        public static LocalisableString ToggleNormalDrumBank => new TranslatableString(getKey(@"toggle_normal_drum_bank"), @"Set normal sample bank to drum");

        /// <summary>
        /// "Set addition sample bank to auto"
        /// </summary>
        public static LocalisableString ToggleAdditionAutoBank => new TranslatableString(getKey(@"toggle_addition_auto_bank"), @"Set addition sample bank to auto");

        /// <summary>
        /// "Set addition sample bank to normal"
        /// </summary>
        public static LocalisableString ToggleAdditionNormalBank => new TranslatableString(getKey(@"toggle_addition_normal_bank"), @"Set addition sample bank to normal");

        /// <summary>
        /// "Set addition sample bank to soft"
        /// </summary>
        public static LocalisableString ToggleAdditionSoftBank => new TranslatableString(getKey(@"toggle_addition_soft_bank"), @"Set addition sample bank to soft");

        /// <summary>
        /// "Set addition sample bank to drum"
        /// </summary>
        public static LocalisableString ToggleAdditionDrumBank => new TranslatableString(getKey(@"toggle_addition_drum_bank"), @"Set addition sample bank to drum");

        /// <summary>
        /// "Toggle distance snap grid"
        /// </summary>
        public static LocalisableString ToggleDistanceSnap => new TranslatableString(getKey(@"toggle_distance_snap"), @"Toggle distance snap grid");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
