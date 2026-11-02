// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Configuration
{
    public class UtauEditorOsuConfigManager : OsuConfigManager
    {
        protected override string Filename => base.Filename.Replace(".ini", ".utau-editor.ini");

        public UtauEditorOsuConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            setDefault(OsuSetting.ShowConvertedBeatmaps, false);
            setDefault(OsuSetting.SongSelectSortingMode, SortMode.DateAdded);
            setDefault(OsuSetting.CursorRotation, false);
            setDefault(OsuSetting.Prefer24HourTime, false);
            setDefault(OsuSetting.ShowFirstRunSetup, false);
            setDefault(OsuSetting.UIScale, 1.1f);

            setDefault(OsuSetting.EditorDim, 0f);
            setDefault(OsuSetting.EditorWaveformOpacity, 0.75f);
            setDefault(OsuSetting.EditorSelectionOutlineThickness, 2f);
            setDefault(OsuSetting.EditorSelectionBoxOutlineThickness, 2f);
            setDefault(OsuSetting.EditorSelectionHitCircleOutlineColour, "05f4fd");
            setDefault(OsuSetting.EditorSelectionSliderBodyOutlineColour, "05f4fd");
            setDefault(OsuSetting.EditorSelectionBoxColour, "05f4fd");
            setDefault(OsuSetting.EditorSelectionControlColour, "05f4fd");
            setDefault(OsuSetting.EditorSelectionControlButtonSize, 15f);
            setDefault(OsuSetting.EditorSelectionScaleHandleSize, 6f);
            setDefault(OsuSetting.EditorSelectionRotationHandleSize, 12f);
            setDefault(OsuSetting.EditorSliderControlLineColour, "ff5a5a");
            setDefault(OsuSetting.EditorSliderControlPointSize, 8f);
            setDefault(OsuSetting.EditorSliderControlPointHitAreaSize, 20f);
            setDefault(OsuSetting.EditorSliderTailArcColour, "05f4fd");
            setDefault(OsuSetting.EditorSliderTailArcThickness, 2f);
            setDefault(OsuSetting.EditorConvertToStreamAcceleration, -0.1d);
            setDefault(OsuSetting.EditorRandomPerturbMode, "Square");
            setDefault(OsuSetting.EditorRandomPerturbStrength, 8.5d);
            setDefault(OsuSetting.EditorRandomPerturbSeed, 4713f);
            setDefault(OsuSetting.EditorApproachRateToggleValueA, 6f);
            setDefault(OsuSetting.EditorApproachRateToggleValueB, 9.2f);
            setDefault(OsuSetting.EditorSliderVelocityCurve, "AccelerateThenDecelerate");
            setDefault(OsuSetting.EditorSliderVelocityIntensity, 1d);
            setDefault(OsuSetting.EditorSliderVelocityMultiplier, 2d);
            setDefault(OsuSetting.EditorSliderVelocityPower, 3d);
            setDefault(OsuSetting.EditorSliderVelocityFrequency, 3f);
            setDefault(OsuSetting.EditorSliderVelocityOvershoot, 0.35d);
            setDefault(OsuSetting.EditorSliderVelocitySampleCount, 96f);
            setDefault(OsuSetting.EditorSliderVelocityMaxDetourLength, 6d);
            setDefault(OsuSetting.EditorAllowOutOfBoundsPlacement, false);
            setDefault(OsuSetting.EditorScaleOrigin, EditorOrigin.SelectionCentre);
            setDefault(OsuSetting.EditorRotationOrigin, EditorOrigin.SelectionCentre);
            setDefault(OsuSetting.EditorTimelineShowBreaks, false);
            setDefault(OsuSetting.EditorContractSidebars, true);

            SetDefault(OsuSetting.UtauEditorFrameworkDefaultsApplied, false);
        }

        private void setDefault<T>(OsuSetting setting, T value)
        {
            var bindable = GetBindable<T>(setting);
            bindable.Default = value;
            bindable.SetDefault();
        }
    }
}
