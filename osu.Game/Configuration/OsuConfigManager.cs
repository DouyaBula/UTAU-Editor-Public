// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Overlays.Mods.Input;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Configuration
{
    public class OsuConfigManager : IniConfigManager<OsuSetting>, IGameplaySettings
    {
        public OsuConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            // UI/selection defaults
            SetDefault(OsuSetting.Ruleset, string.Empty);
            SetDefault(OsuSetting.Skin, SkinInfo.ARGON_SKIN.ToString());

            SetDefault(OsuSetting.BeatmapDetailTab, BeatmapDetailTab.Local);
            SetDefault(OsuSetting.BeatmapLeaderboardSortMode, LeaderboardSortMode.Score);
            SetDefault(OsuSetting.BeatmapDetailModsFilter, false);

            SetDefault(OsuSetting.ShowConvertedBeatmaps, true);
            SetDefault(OsuSetting.DisplayStarsMinimum, 0.0, 0, 10, 0.1);
            SetDefault(OsuSetting.DisplayStarsMaximum, 10.1, 0, 10.1, 0.1);

            SetDefault(OsuSetting.SongSelectGroupMode, GroupMode.None);
            SetDefault(OsuSetting.SongSelectSortingMode, SortMode.Title);
            SetDefault(OsuSetting.SongSelectCollectionFilter, string.Empty);

            SetDefault(OsuSetting.RandomSelectAlgorithm, RandomSelectAlgorithm.RandomPermutation);
            SetDefault(OsuSetting.ModSelectHotkeyStyle, ModSelectHotkeyStyle.Sequential);
            SetDefault(OsuSetting.ModSelectTextSearchStartsActive, true);

            SetDefault(OsuSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2f, 1f, 0.01f);

            SetDefault(OsuSetting.BeatmapListingCardSize, BeatmapCardSize.Normal);
            SetDefault(OsuSetting.BeatmapListingFeaturedArtistFilter, true);

            SetDefault(OsuSetting.ProfileCoverExpanded, true);

            SetDefault(OsuSetting.ToolbarClockDisplayMode, ToolbarClockDisplayMode.Full);

            SetDefault(OsuSetting.SongSelectBackgroundBlur, false);

            // Online settings
            SetDefault(OsuSetting.Username, string.Empty);
            SetDefault(OsuSetting.Token, string.Empty);

            SetDefault(OsuSetting.AutomaticallyDownloadMissingBeatmaps, true);

            SetDefault(OsuSetting.SavePassword, true).ValueChanged += enabled =>
            {
                if (enabled.NewValue)
                    SetValue(OsuSetting.SaveUsername, true);
                else
                    GetBindable<string>(OsuSetting.Token).SetDefault();
            };

            SetDefault(OsuSetting.SaveUsername, true).ValueChanged += enabled =>
            {
                if (!enabled.NewValue)
                {
                    GetBindable<string>(OsuSetting.Username).SetDefault();
                    SetValue(OsuSetting.SavePassword, false);
                }
            };

            SetDefault(OsuSetting.ExternalLinkWarning, true);
            SetDefault(OsuSetting.PreferNoVideo, false);

            SetDefault(OsuSetting.ShowOnlineExplicitContent, false);

            SetDefault(OsuSetting.NotifyOnUsernameMentioned, true);
            SetDefault(OsuSetting.NotifyOnPrivateMessage, true);
            SetDefault(OsuSetting.NotifyOnFriendPresenceChange, true);

            // Audio
            SetDefault(OsuSetting.VolumeInactive, 0.25, 0, 1, 0.01);

            SetDefault(OsuSetting.HitsoundVolume, 0.6, 0, 1, 0.01);

            SetDefault(OsuSetting.MenuVoice, true);
            SetDefault(OsuSetting.MenuMusic, true);
            SetDefault(OsuSetting.MenuTips, true);

            SetDefault(OsuSetting.AudioOffset, 0, -500.0, 500.0, 1);

            SetDefault(OsuSetting.AutomaticallyAdjustBeatmapOffset, false);

            // Input
            SetDefault(OsuSetting.MenuCursorSize, 1.0f, 0.5f, 2f, 0.01f);
            SetDefault(OsuSetting.GameplayCursorSize, 1.0f, 0.1f, 2f, 0.01f);
            SetDefault(OsuSetting.GameplayCursorDuringTouch, false);
            SetDefault(OsuSetting.AutoCursorSize, false);

            SetDefault(OsuSetting.MouseDisableButtons, false);
            SetDefault(OsuSetting.MouseDisableWheel, false);
            SetDefault(OsuSetting.ConfineMouseMode, OsuConfineMouseMode.DuringGameplay);

            SetDefault(OsuSetting.TouchDisableGameplayTaps, false);

            // Graphics
            SetDefault(OsuSetting.ShowFpsDisplay, false);

            SetDefault(OsuSetting.ShowStoryboard, true);
            SetDefault(OsuSetting.BeatmapSkins, true);
            SetDefault(OsuSetting.BeatmapColours, true);
            SetDefault(OsuSetting.BeatmapHitsounds, true);

            SetDefault(OsuSetting.CursorRotation, true);

#pragma warning disable CS0612 // Type or member is obsolete (setting default value to avoid risk of any future crashes)
            SetDefault(OsuSetting.MenuParallax, true);
#pragma warning restore CS0612 // Type or member is obsolete
            SetDefault(OsuSetting.MenuParallaxScale, 1.0f, 0.0f, 2.0f, 0.1f);

            // See https://stackoverflow.com/a/63307411 for default sourcing.
            SetDefault(OsuSetting.Prefer24HourTime, !CultureInfoHelper.SystemCulture.DateTimeFormat.ShortTimePattern.Contains(@"tt"));

            // Gameplay
            SetDefault(OsuSetting.PositionalHitsoundsLevel, 0.2f, 0, 1, 0.01f);
            SetDefault(OsuSetting.DimLevel, 0.7, 0, 1, 0.01);
            SetDefault(OsuSetting.BlurLevel, 0, 0, 1, 0.01);
            SetDefault(OsuSetting.LightenDuringBreaks, true);

            SetDefault(OsuSetting.HitLighting, true);
            SetDefault(OsuSetting.StarFountains, true);

            SetDefault(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Always);
            SetDefault(OsuSetting.ShowHealthDisplayWhenCantFail, true);
            SetDefault(OsuSetting.FadePlayfieldWhenHealthLow, true);
            SetDefault(OsuSetting.KeyOverlay, false);
            SetDefault(OsuSetting.ReplaySettingsOverlay, true);
            SetDefault(OsuSetting.ReplayPlaybackControlsExpanded, true);
            SetDefault(OsuSetting.GameplayLeaderboard, true);
            SetDefault(OsuSetting.AlwaysPlayFirstComboBreak, true);

            SetDefault(OsuSetting.FloatingComments, false);

            SetDefault(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised);

            SetDefault(OsuSetting.IncreaseFirstObjectVisibility, true);
            SetDefault(OsuSetting.GameplayDisableWinKey, true);

            // Update
            SetDefault(OsuSetting.ReleaseStream, ReleaseStream.Lazer);

            SetDefault(OsuSetting.Version, string.Empty);

            SetDefault(OsuSetting.ShowFirstRunSetup, true);
            SetDefault(OsuSetting.ShowMobileDisclaimer, RuntimeInfo.IsMobile);

            SetDefault(OsuSetting.ScreenshotFormat, ScreenshotFormat.Jpg);
            SetDefault(OsuSetting.ScreenshotCaptureMenuCursor, false);

            SetDefault(OsuSetting.Scaling, ScalingMode.Off);
            SetDefault(OsuSetting.SafeAreaConsiderations, true);
            SetDefault(OsuSetting.ScalingBackgroundDim, 0.9f, 0.5f, 1f, 0.01f);

            SetDefault(OsuSetting.ScalingSizeX, 0.8f, 0.2f, 1f, 0.01f);
            SetDefault(OsuSetting.ScalingSizeY, 0.8f, 0.2f, 1f, 0.01f);

            SetDefault(OsuSetting.ScalingPositionX, 0.5f, 0f, 1f, 0.01f);
            SetDefault(OsuSetting.ScalingPositionY, 0.5f, 0f, 1f, 0.01f);

            if (RuntimeInfo.IsMobile)
                SetDefault(OsuSetting.UIScale, 1f, 0.8f, 1.1f, 0.01f);
            else
                SetDefault(OsuSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f);

            SetDefault(OsuSetting.UIHoldActivationDelay, 200.0, 0.0, 500.0, 50.0);

            SetDefault(OsuSetting.IntroSequence, IntroSequence.Triangles);

            SetDefault(OsuSetting.MenuBackgroundSource, BackgroundSource.Skin);
            SetDefault(OsuSetting.SeasonalBackgroundMode, SeasonalBackgroundMode.Sometimes);

            SetDefault(OsuSetting.DiscordRichPresence, DiscordRichPresenceMode.Full);

            // Original:
            // SetDefault(OsuSetting.EditorDim, 0.25f, 0f, 1f, 0.25f);
            SetDefault(OsuSetting.EditorDim, 0.25f, 0f, 1f, 0.01f);
            SetDefault(OsuSetting.EditorWaveformOpacity, 0.25f, 0f, 1f, 0.25f);
            SetDefault(OsuSetting.EditorGridOpacity, 10f, 0f, 100f, 1f);
            SetDefault(OsuSetting.EditorSliderBodyFill, true);
            SetDefault(OsuSetting.EditorDifficultyRatingVisible, true);
            SetDefault(OsuSetting.EditorDifficultyTimelineVisible, true);
            SetDefault(OsuSetting.EditorDifficultyTimelineOpacity, 0.55f, 0.1f, 1f, 0.05f);
            SetDefault(OsuSetting.EditorDifficultyTimelineHeight, 0.55f, 0.2f, 0.9f, 0.05f);
            SetDefault(OsuSetting.EditorDifficultyTimelineInterval, 400f, 100f, 1000f, 50f);
            SetDefault(OsuSetting.EditorDifficultyTimelineOnTop, true);
            SetDefault(OsuSetting.EditorDifficultyUpdateOnSave, true);
            SetDefault(OsuSetting.EditorDifficultyRatingOnLeft, false);
            SetDefault(OsuSetting.EditorShowHitMarkers, true);
            SetDefault(OsuSetting.EditorSelectionOutlineThickness, 9f, 0f, 24f, 0.5f);
            SetDefault(OsuSetting.EditorSelectionBoxOutlineThickness, 3f, 0f, 24f, 0.5f);
            SetDefault(OsuSetting.EditorSelectionOutlineColour, "ffcc22");
            SetDefault(OsuSetting.EditorSelectionHitCircleOutlineColour, "ffcc22");
            SetDefault(OsuSetting.EditorSelectionSliderBodyOutlineColour, "ffcc22");
            SetDefault(OsuSetting.EditorSelectionBoxColour, "eeaa00");
            SetDefault(OsuSetting.EditorSelectionControlColour, "eeaa00");
            SetDefault(OsuSetting.EditorSelectionControlButtonSize, 20f, 8f, 64f, 1f);
            SetDefault(OsuSetting.EditorSelectionScaleHandleSize, 10f, 4f, 48f, 1f);
            SetDefault(OsuSetting.EditorSelectionRotationHandleSize, 15f, 4f, 64f, 1f);
            SetDefault(OsuSetting.EditorSliderControlPointColour, "ffcc22");
            SetDefault(OsuSetting.EditorSliderControlPointInheritedColour, "ffcc22");
            SetDefault(OsuSetting.EditorSliderControlPointCatmullColour, "05ffa2");
            SetDefault(OsuSetting.EditorSliderControlPointBezierColour, "ffddee");
            SetDefault(OsuSetting.EditorSliderControlPointLinearColour, "ed1121");
            SetDefault(OsuSetting.EditorSliderControlPointPerfectCurveColour, "6644cc");
            SetDefault(OsuSetting.EditorSliderControlPointBSplineDegree1Colour, "cc5288");
            SetDefault(OsuSetting.EditorSliderControlPointBSplineDegree2Colour, "ff66aa");
            SetDefault(OsuSetting.EditorSliderControlPointBSplineDegree3Colour, "ff99cc");
            SetDefault(OsuSetting.EditorSliderControlPointBSplineDegree4Colour, "ff99cc");
            SetDefault(OsuSetting.EditorSliderControlLineColour, "ffcc22");
            SetDefault(OsuSetting.EditorNearbyObjectSnapTargets, true);
            SetDefault(OsuSetting.EditorSliderControlPointSnapTargets, true);
            SetDefault(OsuSetting.EditorSliderControlPointSnapTargetThreshold, 6f, 0f, 64f, 1f);
            SetDefault(OsuSetting.EditorSliderControlPointSize, 20f, 4f, 48f, 1f);
            SetDefault(OsuSetting.EditorSliderControlPointHitAreaSize, 28f, 4f, 96f, 1f);
            SetDefault(OsuSetting.EditorSliderControlLineThickness, 1f, 0f, 8f, 0.5f);
            SetDefault(OsuSetting.EditorSliderDetailedControlPointMode, false);
            SetDefault(OsuSetting.EditorAnchorMode, false);
            SetDefault(OsuSetting.EditorAnchorKeepDuration, true);
            SetDefault(OsuSetting.EditorAnchorCornerAmount, 0.2f, 0.01f, 0.49f, 0.01f);
            SetDefault(OsuSetting.EditorAnchorCurvaturePen, false);
            SetDefault(OsuSetting.EditorClassicSymmetry, false);
            SetDefault(OsuSetting.EditorClassicSymmetryAxis, EditorClassicSymmetryAxis.Vertical);
            SetDefault(OsuSetting.EditorSliderDetailedControlPointLimit, 1024f, 0f, 10000f, 1f);
            SetDefault(OsuSetting.EditorSliderRoughControlLineMaxPoints, 256f, 2f, 2048f, 1f);
            SetDefault(OsuSetting.EditorSliderTailArcColour, "ffcc22");
            SetDefault(OsuSetting.EditorSliderTailArcThickness, 5f, 0f, 24f, 0.5f);
            SetDefault(OsuSetting.EditorFixedDurationSliderVelocityDelta, 0.1f, 0.01f, 5f, 0.01f);
            SetDefault(OsuSetting.EditorNudgeDistance, 5f, 1f, 100f, 1f);
            SetDefault(OsuSetting.EditorConvertToStreamEasing, "None");
            SetDefault(OsuSetting.EditorConvertToStreamAcceleration, 0.5d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorConvertToStreamMirror, false);
            SetDefault(OsuSetting.EditorRandomPerturbMode, "Circle");
            SetDefault(OsuSetting.EditorRandomPerturbStrength, 10d, 0d, 200d, 0.5d);
            SetDefault(OsuSetting.EditorRandomPerturbSeed, 1f, 0f, 9999f, 1f);
            SetDefault(OsuSetting.EditorApproachRateToggleValueA, 9.5f, 0f, 10f, 0.1f);
            SetDefault(OsuSetting.EditorApproachRateToggleValueB, 6.5f, 0f, 10f, 0.1f);
            SetDefault(OsuSetting.EditorCanvasZoom, 1f, 0.1f, 2f, 0.01f);
            SetDefault(OsuSetting.EditorCanvasZoomStep, 0.1f, 0.01f, 0.5f, 0.01f);
            SetDefault(OsuSetting.EditorCanvasZoomResetValue, 1f, 0.1f, 2f, 0.01f);
            SetDefault(OsuSetting.EditorSliderVelocityCurve, "AccelerateThenDecelerate");
            SetDefault(OsuSetting.EditorSliderVelocityIntensity, 1d, 0d, 1d, 0.05d);
            SetDefault(OsuSetting.EditorSliderVelocityMultiplier, 2d, 1d, 10d, 0.1d);
            SetDefault(OsuSetting.EditorSliderVelocityPower, 3d, 1d, 8d, 0.25d);
            SetDefault(OsuSetting.EditorSliderVelocityFrequency, 3f, 1f, 8f, 1f);
            SetDefault(OsuSetting.EditorSliderVelocityOvershoot, 0.35d, 0d, 1d, 0.05d);
            SetDefault(OsuSetting.EditorSliderVelocitySampleCount, 96f, 24f, 256f, 8f);
            SetDefault(OsuSetting.EditorSliderVelocityMaxDetourLength, 6d, 1d, 12d, 0.5d);
            SetDefault(OsuSetting.EditorSliderTangentGlitchDistance, 12f, 1f, 96f, 0.5f);
            SetDefault(OsuSetting.EditorHitsoundCopyTemporalLeniency, 5f, 0f, 50f, 0.5f);
            SetDefault(OsuSetting.EditorHitsoundTargetVolume, 100f, 5f, 100f, 1f);
            SetDefault(OsuSetting.EditorHitsoundPreserveMutedVolume, true);
            SetDefault(OsuSetting.EditorHitsoundMidiExportSliderEnds, false);
            SetDefault(OsuSetting.EditorHitsoundMidiImportClearExisting, true);
            SetDefault(OsuSetting.EditorHitsoundMidiImportTemporalLeniency, 5f, 0f, 50f, 0.5f);
            SetDefault(OsuSetting.EditorHitsoundManiaReferenceColumns, 4, 1, 10);
            SetDefault(OsuSetting.EditorHitsoundManiaColumnSampleMapping, "1=normal-hitnormal;2=normal-hitwhistle;3=normal-hitfinish;4=normal-hitclap;5=soft-hitwhistle;6=soft-hitfinish;7=soft-hitclap;8=drum-hitwhistle;9=drum-hitfinish;10=drum-hitclap");
            SetDefault(OsuSetting.EditorHitsoundManiaDefaultNormalSampleBank, "normal");
            SetDefault(OsuSetting.EditorParametricSliderShape, "Polygon");
            SetDefault(OsuSetting.EditorParametricSliderVertices, 5f, 3f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderIterations, 4f, 1f, 10f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderResolution, 80f, 8f, 180f, 4f);
            SetDefault(OsuSetting.EditorParametricSliderJuliaReal, -0.8d, -2d, 2d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderJuliaImaginary, 0.156d, -2d, 2d, 0.001d);
            SetDefault(OsuSetting.EditorParametricSliderSymbolText, "A");
            SetDefault(OsuSetting.EditorParametricSliderSymbolSequenceText, "");
            SetDefault(OsuSetting.EditorParametricSliderSymbolSequenceColumnSpacing, 96d, 0d, 512d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderSymbolSequenceRowSpacing, 120d, 0d, 512d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderSymbolSequenceColumns, 8f, 1f, 64f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderPrimaryParameter, 3d, 1d, 32d, 0.1d);
            SetDefault(OsuSetting.EditorParametricSliderSecondaryParameter, 2d, 1d, 32d, 0.1d);
            SetDefault(OsuSetting.EditorParametricSliderHeadPosition, 0d, 0d, 256d, 0.001d);
            SetDefault(OsuSetting.EditorParametricSliderHeadControlPointType, "Linear");
            SetDefault(OsuSetting.EditorParametricSliderOffsetX, 0d, -512d, 512d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderOffsetY, 0d, -384d, 384d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderRotationTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderRotation, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderPanelOpacity, 1d, 0.15d, 1d, 0.05d);
            SetDefault(OsuSetting.EditorParametricSliderPolyhedronFaces, 6f, 4f, 20f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderQuaternionX, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderQuaternionY, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderQuaternionZ, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderQuaternionW, 1d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderQuaternionTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderQuaternionDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderYawTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderYawDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderPitchTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderPitchDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderRollTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderRollDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderPerspective, 0.35d, 0d, 1d, 0.05d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationFrames, 8f, 1f, 64f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationFrameDurationSnaps, 1f, 1f, 64f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationFrameGapSnaps, 1d, 0d, 64d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEasing, "EaseInOut");
            SetDefault(OsuSetting.EditorParametricSliderAnimationStartScale, 1d, 0.1d, 4d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndScale, 1d, 0.1d, 4d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndVertices, 8f, 3f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndIterations, 6f, 1f, 10f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndResolution, 80f, 8f, 180f, 4f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndJuliaReal, -0.4d, -2d, 2d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndJuliaImaginary, 0.6d, -2d, 2d, 0.001d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndPrimaryParameter, 6d, 1d, 32d, 0.1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndSecondaryParameter, 4d, 1d, 32d, 0.1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndHeadPosition, 1d, 0d, 256d, 0.001d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndOffsetX, 0d, -512d, 512d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndOffsetY, 0d, -384d, 384d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndRotationTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndRotation, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndPolyhedronFaces, 20f, 4f, 20f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndQuaternionX, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndQuaternionY, 1d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndQuaternionZ, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndQuaternionW, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndQuaternionTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndQuaternionDegrees, 180d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndYawTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndYawDegrees, 180d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndPitchTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndPitchDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndRollTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndRollDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorParametricSliderAnimationEndPerspective, 0.7d, 0d, 1d, 0.05d);
            SetDefault(OsuSetting.EditorNoiseSliderCategory, "VectorFlow");
            SetDefault(OsuSetting.EditorNoiseSliderType, "NoiseSampledPath");
            SetDefault(OsuSetting.EditorNoiseSliderHeadControlPointType, "Linear");
            SetDefault(OsuSetting.EditorNoiseSliderSeedPattern, "Centre");
            SetDefault(OsuSetting.EditorNoiseSliderNoiseType, "FractalValue");
            SetDefault(OsuSetting.EditorNoiseSliderSeedPosition, 0.5d, 0d, 1d, 0.001d);
            SetDefault(OsuSetting.EditorNoiseSliderSeedAngle, 0d, -180d, 180d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderFieldBias, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorNoiseSliderStreamlineLength, 220d, 16d, 1024d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderSamples, 96f, 8f, 1024f, 4f);
            SetDefault(OsuSetting.EditorNoiseSliderWidth, 360d, 16d, 512d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderHeight, 160d, 16d, 512d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAmplitude, 36d, 0d, 256d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderFrequency, 3d, 0.1d, 16d, 0.1d);
            SetDefault(OsuSetting.EditorNoiseSliderOctaves, 4f, 1f, 8f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderPersistence, 0.5d, 0.1d, 0.95d, 0.01d);
            SetDefault(OsuSetting.EditorNoiseSliderSmoothing, 2f, 0f, 16f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderSeed, 1f, -999999f, 999999f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderScale, 1d, 0.1d, 4d, 0.01d);
            SetDefault(OsuSetting.EditorNoiseSliderRotationTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderRotationDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderOffsetX, 0d, -512d, 512d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderOffsetY, 0d, -384d, 384d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEasing, "EaseInOut");
            SetDefault(OsuSetting.EditorNoiseSliderAnimationFrames, 1f, 1f, 64f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndSeedPosition, 0.5d, 0d, 1d, 0.001d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndSeedAngle, 0d, -180d, 180d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndFieldBias, 0d, -1d, 1d, 0.01d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndStreamlineLength, 220d, 16d, 1024d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndSamples, 96f, 8f, 1024f, 4f);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndWidth, 360d, 16d, 512d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndHeight, 160d, 16d, 512d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndAmplitude, 48d, 0d, 256d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndFrequency, 5d, 0.1d, 16d, 0.1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndOctaves, 4f, 1f, 8f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndPersistence, 0.5d, 0.1d, 0.95d, 0.01d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndSmoothing, 2f, 0f, 16f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndSeed, 2f, -999999f, 999999f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndScale, 1d, 0.1d, 4d, 0.01d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndRotationTurns, 0f, -32f, 32f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndRotationDegrees, 0d, -359d, 359d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndOffsetX, 0d, -512d, 512d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderAnimationEndOffsetY, 0d, -384d, 384d, 1d);
            SetDefault(OsuSetting.EditorNoiseSliderFrameDurationSnaps, 1f, 1f, 64f, 1f);
            SetDefault(OsuSetting.EditorNoiseSliderFrameGapSnaps, 1d, 0d, 64d, 1d);
            SetDefault(OsuSetting.EditorAllowOutOfBoundsPlacement, false);
            SetDefault(OsuSetting.EditorAutoSeekOnPlacement, true);
            SetDefault(OsuSetting.EditorLimitedDistanceSnap, false);
            SetDefault(OsuSetting.EditorShowSpeedChanges, false);
            SetDefault(OsuSetting.EditorScaleOrigin, EditorOrigin.GridCentre);
            SetDefault(OsuSetting.EditorRotationOrigin, EditorOrigin.GridCentre);
            SetDefault(OsuSetting.EditorAdjustExistingObjectsOnTimingChanges, true);

            SetDefault(OsuSetting.HideCountryFlags, false);

            SetDefault(OsuSetting.MultiplayerRoomFilter, RoomPermissionsFilter.All);
            SetDefault(OsuSetting.MultiplayerShowInProgressFilter, true);
            SetDefault(OsuSetting.MultiplayerShowFullFilter, false);

            SetDefault(OsuSetting.LastProcessedMetadataId, -1);

            SetDefault(OsuSetting.ComboColourNormalisationAmount, 0.2f, 0f, 1f, 0.01f);
            SetDefault(OsuSetting.UserOnlineStatus, UserStatus.Online);

            SetDefault(OsuSetting.EditorTimelineShowTimingChanges, true);
            SetDefault(OsuSetting.EditorTimelineShowBreaks, true);
            SetDefault(OsuSetting.EditorTimelineShowTicks, true);

            SetDefault(OsuSetting.EditorContractSidebars, false);
            SetDefault(OsuSetting.EditorLeftToolboxOrder, string.Empty);
            SetDefault(OsuSetting.EditorRightToolboxOrder, string.Empty);

            SetDefault(OsuSetting.AlwaysShowHoldForMenuButton, false);
            SetDefault(OsuSetting.AlwaysRequireHoldingForPause, false);
            SetDefault(OsuSetting.EditorShowStoryboard, true);

            SetDefault(OsuSetting.EditorSubmissionNotifyOnDiscussionReplies, true);
            SetDefault(OsuSetting.EditorSubmissionLoadInBrowserAfterSubmission, true);

            SetDefault(OsuSetting.WasSupporter, false);

            // intentionally uses `DateTime?` and not `DateTimeOffset?` because the latter fails due to `DateTimeOffset` not implementing `IConvertible`
            SetDefault(OsuSetting.LastOnlineTagsPopulation, (DateTime?)null);

            SetDefault(OsuSetting.DashboardSortMode, UserSortCriteria.LastVisit);
            SetDefault(OsuSetting.DashboardDisplayStyle, OverlayPanelDisplayStyle.Card);

            SetDefault(OsuSetting.PMFriendsOnly, false);
        }

        protected override bool CheckLookupContainsPrivateInformation(OsuSetting lookup)
        {
            switch (lookup)
            {
                case OsuSetting.Token:
                    return true;
            }

            return false;
        }

        public override TrackedSettings CreateTrackedSettings()
        {
            return new TrackedSettings
            {
                new TrackedSetting<bool>(OsuSetting.ShowFpsDisplay, state => new SettingDescription(
                    rawValue: state,
                    name: GlobalActionKeyBindingStrings.ToggleFPSCounter,
                    value: state ? CommonStrings.Enabled.ToLower() : CommonStrings.Disabled.ToLower(),
                    shortcut: LookupKeyBindings(GlobalAction.ToggleFPSDisplay))
                ),
                new TrackedSetting<bool>(OsuSetting.MouseDisableButtons, disabledState => new SettingDescription(
                    rawValue: !disabledState,
                    name: GlobalActionKeyBindingStrings.ToggleGameplayMouseButtons,
                    value: disabledState ? CommonStrings.Disabled.ToLower() : CommonStrings.Enabled.ToLower(),
                    shortcut: LookupKeyBindings(GlobalAction.ToggleGameplayMouseButtons))
                ),
                new TrackedSetting<bool>(OsuSetting.GameplayLeaderboard, state => new SettingDescription(
                    rawValue: state,
                    name: GlobalActionKeyBindingStrings.ToggleInGameLeaderboard,
                    value: state ? CommonStrings.Enabled.ToLower() : CommonStrings.Disabled.ToLower(),
                    shortcut: LookupKeyBindings(GlobalAction.ToggleInGameLeaderboard))
                ),
                new TrackedSetting<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode, visibilityMode => new SettingDescription(
                    rawValue: visibilityMode,
                    name: GameplaySettingsStrings.HUDVisibilityMode,
                    value: visibilityMode.GetLocalisableDescription(),
                    shortcut: new TranslatableString(@"_", @"{0}: {1} {2}: {3}",
                        GlobalActionKeyBindingStrings.ToggleInGameInterface,
                        LookupKeyBindings(GlobalAction.ToggleInGameInterface),
                        GlobalActionKeyBindingStrings.HoldForHUD,
                        LookupKeyBindings(GlobalAction.HoldForHUD)))
                ),
                new TrackedSetting<ScalingMode>(OsuSetting.Scaling, scalingMode => new SettingDescription(
                        rawValue: scalingMode,
                        name: GraphicsSettingsStrings.ScreenScaling,
                        value: scalingMode.GetLocalisableDescription()
                    )
                ),
                new TrackedSetting<string>(OsuSetting.Skin, skin =>
                {
                    string skinName = string.Empty;

                    if (Guid.TryParse(skin, out var id))
                        skinName = LookupSkinName(id);

                    return new SettingDescription(
                        rawValue: skinName,
                        name: SkinSettingsStrings.SkinSectionHeader,
                        value: skinName,
                        shortcut: new TranslatableString(@"_", @"{0}: {1}",
                            GlobalActionKeyBindingStrings.RandomSkin,
                            LookupKeyBindings(GlobalAction.RandomSkin))
                    );
                }),
                new TrackedSetting<float>(OsuSetting.UIScale, scale => new SettingDescription(
                        rawValue: scale,
                        name: GraphicsSettingsStrings.UIScaling,
                        value: $"{scale:N2}x"
                        // TODO: implement lookup for framework platform key bindings
                    )
                ),
            };
        }

        public Func<Guid, string> LookupSkinName { private get; set; } = _ => @"unknown";
        public Func<GlobalAction, LocalisableString> LookupKeyBindings { private get; set; } = _ => @"unknown";

        IBindable<float> IGameplaySettings.ComboColourNormalisationAmount => GetOriginalBindable<float>(OsuSetting.ComboColourNormalisationAmount);
        IBindable<float> IGameplaySettings.PositionalHitsoundsLevel => GetOriginalBindable<float>(OsuSetting.PositionalHitsoundsLevel);
    }

    // IMPORTANT: These are used in user configuration files.
    // The naming of these keys should not be changed once they are deployed in a release, unless migration logic is also added.
    public enum OsuSetting
    {
        Ruleset,
        Token,
        MenuCursorSize,
        GameplayCursorSize,
        AutoCursorSize,
        GameplayCursorDuringTouch,
        DimLevel,
        BlurLevel,
        EditorDim,
        LightenDuringBreaks,
        ShowStoryboard,
        KeyOverlay,
        GameplayLeaderboard,
        PositionalHitsoundsLevel,
        AlwaysPlayFirstComboBreak,
        FloatingComments,
        HUDVisibilityMode,

        ShowHealthDisplayWhenCantFail,
        FadePlayfieldWhenHealthLow,

        /// <summary>
        /// Disables mouse buttons clicks during gameplay.
        /// </summary>
        MouseDisableButtons,
        MouseDisableWheel,
        ConfineMouseMode,

        /// <summary>
        /// Globally applied audio offset.
        /// This is added to the audio track's current time. Higher values will cause gameplay to occur earlier, relative to the audio track.
        /// </summary>
        AudioOffset,

        VolumeInactive,
        MenuMusic,
        MenuVoice,
        MenuTips,
        CursorRotation,

        [Obsolete]
        MenuParallax, // todo: can be removed 20270101

        MenuParallaxScale,
        Prefer24HourTime,
        BeatmapDetailTab,
        BeatmapLeaderboardSortMode,
        BeatmapDetailModsFilter,
        Username,
        ReleaseStream,
        SavePassword,
        SaveUsername,
        DisplayStarsMinimum,
        DisplayStarsMaximum,
        SongSelectGroupMode,
        SongSelectSortingMode,
        SongSelectCollectionFilter,
        RandomSelectAlgorithm,
        ModSelectHotkeyStyle,
        ShowFpsDisplay,
        ChatDisplayHeight,
        BeatmapListingCardSize,
        ToolbarClockDisplayMode,
        SongSelectBackgroundBlur,
        Version,
        ShowFirstRunSetup,
        ShowConvertedBeatmaps,
        Skin,
        ScreenshotFormat,
        ScreenshotCaptureMenuCursor,
        BeatmapSkins,
        BeatmapColours,
        BeatmapHitsounds,
        IncreaseFirstObjectVisibility,
        ScoreDisplayMode,
        ExternalLinkWarning,
        PreferNoVideo,
        Scaling,
        ScalingPositionX,
        ScalingPositionY,
        ScalingSizeX,
        ScalingSizeY,
        ScalingBackgroundDim,
        UIScale,
        IntroSequence,
        NotifyOnUsernameMentioned,
        NotifyOnPrivateMessage,
        NotifyOnFriendPresenceChange,
        UIHoldActivationDelay,
        HitLighting,
        StarFountains,
        MenuBackgroundSource,
        GameplayDisableWinKey,
        SeasonalBackgroundMode,
        EditorWaveformOpacity,
        EditorGridOpacity,
        EditorSliderBodyFill,
        EditorDifficultyRatingVisible,
        EditorDifficultyTimelineVisible,
        EditorDifficultyTimelineOpacity,
        EditorDifficultyTimelineHeight,
        EditorDifficultyTimelineInterval,
        EditorDifficultyTimelineOnTop,
        EditorDifficultyUpdateOnSave,
        EditorDifficultyRatingOnLeft,
        EditorShowHitMarkers,
        EditorSelectionOutlineThickness,
        EditorSelectionBoxOutlineThickness,
        EditorSelectionOutlineColour,
        EditorSelectionHitCircleOutlineColour,
        EditorSelectionSliderBodyOutlineColour,
        EditorSelectionBoxColour,
        EditorSelectionControlColour,
        EditorSelectionControlButtonSize,
        EditorSelectionScaleHandleSize,
        EditorSelectionRotationHandleSize,
        EditorSliderControlPointColour,
        EditorSliderControlPointInheritedColour,
        EditorSliderControlPointCatmullColour,
        EditorSliderControlPointBezierColour,
        EditorSliderControlPointLinearColour,
        EditorSliderControlPointPerfectCurveColour,
        EditorSliderControlPointBSplineDegree1Colour,
        EditorSliderControlPointBSplineDegree2Colour,
        EditorSliderControlPointBSplineDegree3Colour,
        EditorSliderControlPointBSplineDegree4Colour,
        EditorSliderControlLineColour,
        EditorNearbyObjectSnapTargets,
        EditorSliderControlPointSnapTargets,
        EditorSliderControlPointSnapTargetThreshold,
        EditorSliderControlPointSize,
        EditorSliderControlPointHitAreaSize,
        EditorSliderControlLineThickness,
        EditorSliderDetailedControlPointMode,
        EditorSliderDetailedControlPointLimit,
        EditorSliderRoughControlLineMaxPoints,
        EditorSliderTailArcColour,
        EditorSliderTailArcThickness,
        EditorAutoSeekOnPlacement,
        DiscordRichPresence,

        ShowOnlineExplicitContent,
        LastProcessedMetadataId,
        SafeAreaConsiderations,
        ComboColourNormalisationAmount,
        ProfileCoverExpanded,
        EditorLimitedDistanceSnap,
        ReplaySettingsOverlay,
        ReplayPlaybackControlsExpanded,
        AutomaticallyDownloadMissingBeatmaps,
        EditorShowSpeedChanges,
        TouchDisableGameplayTaps,
        ModSelectTextSearchStartsActive,

        /// <summary>
        /// The status for the current user to broadcast to other players.
        /// </summary>
        UserOnlineStatus,

        MultiplayerRoomFilter,
        HideCountryFlags,
        EditorTimelineShowTimingChanges,
        EditorTimelineShowTicks,
        AlwaysShowHoldForMenuButton,
        EditorContractSidebars,
        EditorScaleOrigin,
        EditorRotationOrigin,
        EditorTimelineShowBreaks,
        EditorAdjustExistingObjectsOnTimingChanges,
        AlwaysRequireHoldingForPause,
        MultiplayerShowInProgressFilter,
        MultiplayerShowFullFilter,
        BeatmapListingFeaturedArtistFilter,
        ShowMobileDisclaimer,
        EditorShowStoryboard,
        EditorSubmissionNotifyOnDiscussionReplies,
        EditorSubmissionLoadInBrowserAfterSubmission,

        /// <summary>
        /// Cached state of whether local user is a supporter.
        /// Used to allow early checks (ie for startup samples) to be in the correct state, even if the API authentication process has not completed.
        /// </summary>
        WasSupporter,

        LastOnlineTagsPopulation,

        AutomaticallyAdjustBeatmapOffset,

        DashboardSortMode,
        DashboardDisplayStyle,
        PMFriendsOnly,
        HitsoundVolume,
        EditorFixedDurationSliderVelocityDelta,
        EditorNudgeDistance,
        EditorConvertToStreamEasing,
        EditorConvertToStreamAcceleration,
        EditorLeftToolboxOrder,
        EditorRightToolboxOrder,
        EditorConvertToStreamMirror,
        EditorRandomPerturbMode,
        EditorRandomPerturbStrength,
        EditorRandomPerturbSeed,
        EditorApproachRateToggleValueA,
        EditorApproachRateToggleValueB,
        EditorCanvasZoom,
        EditorCanvasZoomStep,
        EditorCanvasZoomResetValue,
        EditorSliderVelocityCurve,
        EditorSliderVelocityIntensity,
        EditorSliderVelocityMultiplier,
        EditorSliderVelocityPower,
        EditorSliderVelocityFrequency,
        EditorSliderVelocityOvershoot,
        EditorSliderVelocitySampleCount,
        EditorSliderVelocityMaxDetourLength,
        EditorSliderTangentGlitchDistance,
        EditorHitsoundCopyTemporalLeniency,
        EditorHitsoundTargetVolume,
        EditorHitsoundPreserveMutedVolume,
        EditorHitsoundMidiExportSliderEnds,
        EditorHitsoundMidiImportClearExisting,
        EditorHitsoundMidiImportTemporalLeniency,
        EditorHitsoundManiaReferenceColumns,
        EditorHitsoundManiaColumnSampleMapping,
        EditorHitsoundManiaDefaultNormalSampleBank,
        EditorParametricSliderShape,
        EditorParametricSliderVertices,
        EditorParametricSliderIterations,
        EditorParametricSliderResolution,
        EditorParametricSliderJuliaReal,
        EditorParametricSliderJuliaImaginary,
        EditorParametricSliderSymbolText,
        EditorParametricSliderSymbolSequenceText,
        EditorParametricSliderSymbolSequenceColumnSpacing,
        EditorParametricSliderSymbolSequenceRowSpacing,
        EditorParametricSliderSymbolSequenceColumns,
        EditorParametricSliderPrimaryParameter,
        EditorParametricSliderSecondaryParameter,
        EditorParametricSliderHeadPosition,
        EditorParametricSliderHeadControlPointType,
        EditorParametricSliderOffsetX,
        EditorParametricSliderOffsetY,
        EditorParametricSliderRotationTurns,
        EditorParametricSliderRotation,
        EditorParametricSliderPanelOpacity,
        EditorParametricSliderPolyhedronFaces,
        EditorParametricSliderQuaternionX,
        EditorParametricSliderQuaternionY,
        EditorParametricSliderQuaternionZ,
        EditorParametricSliderQuaternionW,
        EditorParametricSliderQuaternionTurns,
        EditorParametricSliderQuaternionDegrees,
        EditorParametricSliderYawTurns,
        EditorParametricSliderYawDegrees,
        EditorParametricSliderPitchTurns,
        EditorParametricSliderPitchDegrees,
        EditorParametricSliderRollTurns,
        EditorParametricSliderRollDegrees,
        EditorParametricSliderPerspective,
        EditorParametricSliderAnimationFrames,
        EditorParametricSliderAnimationFrameDurationSnaps,
        EditorParametricSliderAnimationFrameGapSnaps,
        EditorParametricSliderAnimationEasing,
        EditorParametricSliderAnimationStartScale,
        EditorParametricSliderAnimationEndScale,
        EditorParametricSliderAnimationEndVertices,
        EditorParametricSliderAnimationEndIterations,
        EditorParametricSliderAnimationEndResolution,
        EditorParametricSliderAnimationEndJuliaReal,
        EditorParametricSliderAnimationEndJuliaImaginary,
        EditorParametricSliderAnimationEndPrimaryParameter,
        EditorParametricSliderAnimationEndSecondaryParameter,
        EditorParametricSliderAnimationEndHeadPosition,
        EditorParametricSliderAnimationEndOffsetX,
        EditorParametricSliderAnimationEndOffsetY,
        EditorParametricSliderAnimationEndRotationTurns,
        EditorParametricSliderAnimationEndRotation,
        EditorParametricSliderAnimationEndPolyhedronFaces,
        EditorParametricSliderAnimationEndQuaternionX,
        EditorParametricSliderAnimationEndQuaternionY,
        EditorParametricSliderAnimationEndQuaternionZ,
        EditorParametricSliderAnimationEndQuaternionW,
        EditorParametricSliderAnimationEndQuaternionTurns,
        EditorParametricSliderAnimationEndQuaternionDegrees,
        EditorParametricSliderAnimationEndYawTurns,
        EditorParametricSliderAnimationEndYawDegrees,
        EditorParametricSliderAnimationEndPitchTurns,
        EditorParametricSliderAnimationEndPitchDegrees,
        EditorParametricSliderAnimationEndRollTurns,
        EditorParametricSliderAnimationEndRollDegrees,
        EditorParametricSliderAnimationEndPerspective,
        EditorNoiseSliderCategory,
        EditorNoiseSliderType,
        EditorNoiseSliderHeadControlPointType,
        EditorNoiseSliderSeedPattern,
        EditorNoiseSliderNoiseType,
        EditorNoiseSliderSeedPosition,
        EditorNoiseSliderSeedAngle,
        EditorNoiseSliderFieldBias,
        EditorNoiseSliderStreamlineLength,
        EditorNoiseSliderSamples,
        EditorNoiseSliderWidth,
        EditorNoiseSliderHeight,
        EditorNoiseSliderAmplitude,
        EditorNoiseSliderFrequency,
        EditorNoiseSliderOctaves,
        EditorNoiseSliderPersistence,
        EditorNoiseSliderSmoothing,
        EditorNoiseSliderSeed,
        EditorNoiseSliderScale,
        EditorNoiseSliderRotationTurns,
        EditorNoiseSliderRotationDegrees,
        EditorNoiseSliderOffsetX,
        EditorNoiseSliderOffsetY,
        EditorNoiseSliderAnimationEasing,
        EditorNoiseSliderAnimationFrames,
        EditorNoiseSliderAnimationEndSeedPosition,
        EditorNoiseSliderAnimationEndSeedAngle,
        EditorNoiseSliderAnimationEndFieldBias,
        EditorNoiseSliderAnimationEndStreamlineLength,
        EditorNoiseSliderAnimationEndSamples,
        EditorNoiseSliderAnimationEndWidth,
        EditorNoiseSliderAnimationEndHeight,
        EditorNoiseSliderAnimationEndAmplitude,
        EditorNoiseSliderAnimationEndFrequency,
        EditorNoiseSliderAnimationEndOctaves,
        EditorNoiseSliderAnimationEndPersistence,
        EditorNoiseSliderAnimationEndSmoothing,
        EditorNoiseSliderAnimationEndSeed,
        EditorNoiseSliderAnimationEndScale,
        EditorNoiseSliderAnimationEndRotationTurns,
        EditorNoiseSliderAnimationEndRotationDegrees,
        EditorNoiseSliderAnimationEndOffsetX,
        EditorNoiseSliderAnimationEndOffsetY,
        EditorNoiseSliderFrameDurationSnaps,
        EditorNoiseSliderFrameGapSnaps,
        EditorAllowOutOfBoundsPlacement,
        UtauEditorFrameworkDefaultsApplied,
        EditorAnchorMode,
        EditorAnchorKeepDuration,
        EditorAnchorCornerAmount,
        EditorAnchorCurvaturePen,
        EditorClassicSymmetry,
        EditorClassicSymmetryAxis,
    }

    public enum EditorClassicSymmetryAxis
    {
        Vertical,
        Horizontal
    }
}
