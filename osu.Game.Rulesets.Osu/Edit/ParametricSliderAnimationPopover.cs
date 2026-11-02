// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal enum ParametricSliderAnimationEasing
    {
        Linear,

        [System.ComponentModel.Description("Ease in")]
        EaseIn,

        [System.ComponentModel.Description("Ease out")]
        EaseOut,

        [System.ComponentModel.Description("Ease in / out")]
        EaseInOut,

        [System.ComponentModel.Description("Smooth step")]
        SmoothStep
    }

    public partial class ParametricSliderAnimationPopover : EditorPreviewPopover
    {
        private readonly Bindable<ParametricSliderShape> shape = new Bindable<ParametricSliderShape>();
        private readonly Bindable<ParametricSliderAnimationEasing> easing = new Bindable<ParametricSliderAnimationEasing>();
        private readonly Bindable<ParametricSliderHeadPathType> headControlPointType = new Bindable<ParametricSliderHeadPathType>();
        private readonly Dictionary<ParametricSliderShape, RoundedButton> shapeButtons = new Dictionary<ParametricSliderShape, RoundedButton>();
        private readonly Dictionary<ParametricSliderShapeCategory, RoundedButton> categoryButtons = new Dictionary<ParametricSliderShapeCategory, RoundedButton>();
        private readonly Dictionary<ParametricSliderShapeCategory, Drawable> categoryContents = new Dictionary<ParametricSliderShapeCategory, Drawable>();
        private readonly List<SingleFrameInactiveContainer> singleFrameInactiveControls = new List<SingleFrameInactiveContainer>();
        private ParametricSliderShapeCategory? expandedCategory;

        private FormSliderBar<int> startVertices = null!;
        private FormSliderBar<int> endVertices = null!;
        private FormSliderBar<int> startIterations = null!;
        private FormSliderBar<int> endIterations = null!;
        private FormSliderBar<int> startResolution = null!;
        private FormSliderBar<int> endResolution = null!;
        private FormSliderBar<double> startPrimary = null!;
        private FormSliderBar<double> endPrimary = null!;
        private FormSliderBar<double> startSecondary = null!;
        private FormSliderBar<double> endSecondary = null!;
        private FormSliderBar<double> startJuliaReal = null!;
        private FormSliderBar<double> endJuliaReal = null!;
        private FormSliderBar<double> startJuliaImaginary = null!;
        private FormSliderBar<double> endJuliaImaginary = null!;
        private FormSliderBar<double> startHeadPosition = null!;
        private FormSliderBar<double> endHeadPosition = null!;
        private FormSliderBar<int> startPolyhedronFaces = null!;
        private FormSliderBar<int> endPolyhedronFaces = null!;
        private FormSliderBar<int> startYawTurns = null!;
        private FormSliderBar<int> endYawTurns = null!;
        private FormSliderBar<double> startYawDegrees = null!;
        private FormSliderBar<double> endYawDegrees = null!;
        private FormSliderBar<int> startPitchTurns = null!;
        private FormSliderBar<int> endPitchTurns = null!;
        private FormSliderBar<double> startPitchDegrees = null!;
        private FormSliderBar<double> endPitchDegrees = null!;
        private FormSliderBar<int> startRollTurns = null!;
        private FormSliderBar<int> endRollTurns = null!;
        private FormSliderBar<double> startRollDegrees = null!;
        private FormSliderBar<double> endRollDegrees = null!;
        private FormSliderBar<double> startPerspective = null!;
        private FormSliderBar<double> endPerspective = null!;
        private FormSliderBar<double> startScale = null!;
        private FormSliderBar<double> endScale = null!;
        private FormSliderBar<double> startOffsetX = null!;
        private FormSliderBar<double> endOffsetX = null!;
        private FormSliderBar<double> startOffsetY = null!;
        private FormSliderBar<double> endOffsetY = null!;
        private FormSliderBar<int> startRotationTurns = null!;
        private FormSliderBar<int> endRotationTurns = null!;
        private FormSliderBar<double> startRotation = null!;
        private FormSliderBar<double> endRotation = null!;
        private FormSliderBar<int> framesInput = null!;
        private FormSliderBar<int> frameDurationInput = null!;
        private FormSliderBar<double> frameGapInput = null!;
        private FormSliderBar<double> panelOpacityInput = null!;
        private OsuTextBox symbolInput = null!;
        private OsuTextBox symbolSequenceInput = null!;
        private FormSliderBar<double> symbolColumnSpacingInput = null!;
        private FormSliderBar<double> symbolRowSpacingInput = null!;
        private FormSliderBar<int> symbolColumnsInput = null!;
        private BindableDouble startPrimaryBindable = null!;
        private BindableDouble endPrimaryBindable = null!;
        private BindableDouble startSecondaryBindable = null!;
        private BindableDouble endSecondaryBindable = null!;
        private EditorToolStatus resultText = null!;
        private RoundedButton createButton = null!;
        private EditorToolTimeAnchor insertionTime = null!;
        private Drawable verticesRow = null!;
        private Drawable iterationsRow = null!;
        private Drawable resolutionRow = null!;
        private Drawable symbolRow = null!;
        private Drawable symbolSequenceRow = null!;
        private Drawable symbolLayoutRow = null!;
        private Drawable primaryRow = null!;
        private Drawable secondaryRow = null!;
        private Drawable juliaRealRow = null!;
        private Drawable juliaImaginaryRow = null!;
        private Drawable headPositionRow = null!;
        private Drawable polyhedronFacesRow = null!;
        private Drawable yawTurnsRow = null!;
        private Drawable yawDegreesRow = null!;
        private Drawable pitchTurnsRow = null!;
        private Drawable pitchDegreesRow = null!;
        private Drawable rollTurnsRow = null!;
        private Drawable rollDegreesRow = null!;
        private Drawable perspectiveRow = null!;

        private readonly List<Slider> previewSliders = new List<Slider>();
        private Color4 selectedShapeColour;
        private Color4 unselectedShapeColour;

        private Bindable<string> shapeConfig = null!;
        private Bindable<string> easingConfig = null!;
        private Bindable<float> startVerticesConfig = null!;
        private Bindable<float> endVerticesConfig = null!;
        private Bindable<float> startIterationsConfig = null!;
        private Bindable<float> endIterationsConfig = null!;
        private Bindable<float> startResolutionConfig = null!;
        private Bindable<float> endResolutionConfig = null!;
        private Bindable<string> symbolTextConfig = null!;
        private Bindable<string> symbolSequenceConfig = null!;
        private Bindable<double> symbolColumnSpacingConfig = null!;
        private Bindable<double> symbolRowSpacingConfig = null!;
        private Bindable<float> symbolColumnsConfig = null!;
        private Bindable<double> startPrimaryConfig = null!;
        private Bindable<double> endPrimaryConfig = null!;
        private Bindable<double> startSecondaryConfig = null!;
        private Bindable<double> endSecondaryConfig = null!;
        private Bindable<double> startJuliaRealConfig = null!;
        private Bindable<double> endJuliaRealConfig = null!;
        private Bindable<double> startJuliaImaginaryConfig = null!;
        private Bindable<double> endJuliaImaginaryConfig = null!;
        private Bindable<double> startHeadPositionConfig = null!;
        private Bindable<double> endHeadPositionConfig = null!;
        private Bindable<string> headControlPointTypeConfig = null!;
        private Bindable<float> startPolyhedronFacesConfig = null!;
        private Bindable<float> endPolyhedronFacesConfig = null!;
        private Bindable<float> startYawTurnsConfig = null!;
        private Bindable<float> endYawTurnsConfig = null!;
        private Bindable<double> startYawDegreesConfig = null!;
        private Bindable<double> endYawDegreesConfig = null!;
        private Bindable<float> startPitchTurnsConfig = null!;
        private Bindable<float> endPitchTurnsConfig = null!;
        private Bindable<double> startPitchDegreesConfig = null!;
        private Bindable<double> endPitchDegreesConfig = null!;
        private Bindable<float> startRollTurnsConfig = null!;
        private Bindable<float> endRollTurnsConfig = null!;
        private Bindable<double> startRollDegreesConfig = null!;
        private Bindable<double> endRollDegreesConfig = null!;
        private Bindable<double> startPerspectiveConfig = null!;
        private Bindable<double> endPerspectiveConfig = null!;
        private Bindable<double> startScaleConfig = null!;
        private Bindable<double> endScaleConfig = null!;
        private Bindable<double> startOffsetXConfig = null!;
        private Bindable<double> endOffsetXConfig = null!;
        private Bindable<double> startOffsetYConfig = null!;
        private Bindable<double> endOffsetYConfig = null!;
        private Bindable<float> startRotationTurnsConfig = null!;
        private Bindable<float> endRotationTurnsConfig = null!;
        private Bindable<double> startRotationConfig = null!;
        private Bindable<double> endRotationConfig = null!;
        private Bindable<float> framesConfig = null!;
        private Bindable<float> frameDurationConfig = null!;
        private Bindable<double> frameGapConfig = null!;

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private HitObjectComposer composer { get; set; } = null!;

        private Bindable<TernaryState> newComboState = null!;

        public ParametricSliderAnimationPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            newComboState = composer.SelectionNewComboState!.GetBoundCopy();
            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            loadConfig(config);
            initialiseParameterControls();

            selectedShapeColour = colours.BlueDark;
            unselectedShapeColour = colours.GreySkyDark;
            expandedCategory = ParametricSliderGenerator.GetCategory(shape.Value);

            var categoryButtonFlow = new EditorToolControlRow(92)
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
                Spacing = new Vector2(4)
            };

            foreach (ParametricSliderShapeCategory category in Enum.GetValues<ParametricSliderShapeCategory>())
            {
                var categoryButton = new RoundedButton
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 0.112f,
                    Height = 28,
                    BackgroundColour = unselectedShapeColour,
                    Action = () => toggleCategory(category)
                };

                categoryButtons.Add(category, categoryButton);
                categoryButtonFlow.Add(categoryButton);
                categoryContents.Add(category, createShapeGrid(category));
            }

            var categoryContentHost = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = categoryContents.Values.ToArray()
            };

            Child = new FillFlowContainer
            {
                Width = 980,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(6),
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    insertionTime = new EditorToolTimeAnchor(),
                    categoryButtonFlow,
                    new EditorToolSplitPane(300, 0.45f,
                            new Container
                            {
                                Width = 0.45f,
                                RelativeSizeAxes = Axes.Both,
                                Child = categoryContentHost
                            },
                            new OsuScrollContainer(Direction.Vertical)
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Width = 0.54f,
                                RelativeSizeAxes = Axes.Both,
                                Child = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 3),
                                    Padding = new MarginPadding { Right = 5 },
                                    Children = new Drawable[]
                                    {
                                        panelOpacityInput = new FormSliderBar<double>
                                        {
                                            Caption = "Panel opacity",
                                            Current = config.GetBindable<double>(OsuSetting.EditorParametricSliderPanelOpacity),
                                            KeyboardStep = 0.05f,
                                            LabelFormat = value => $"{value:P0}",
                                            TooltipFormat = value => $"{value:P0}",
                                            TabbableContentContainer = this
                                        },
                                        verticesRow = createPairRow(startVertices, endVertices),
                                        iterationsRow = createPairRow(startIterations, endIterations),
                                        resolutionRow = createPairRow(startResolution, endResolution),
                                        symbolRow = createSymbolRow(),
                                        symbolSequenceRow = createSymbolSequenceRow(),
                                        symbolLayoutRow = createSymbolLayoutRow(),
                                        primaryRow = createPairRow(startPrimary, endPrimary),
                                        secondaryRow = createPairRow(startSecondary, endSecondary),
                                        juliaRealRow = createPairRow(startJuliaReal, endJuliaReal),
                                        juliaImaginaryRow = createPairRow(startJuliaImaginary, endJuliaImaginary),
                                        createPairRow(startScale, endScale),
                                        createPairRow(startOffsetX, endOffsetX),
                                        createPairRow(startOffsetY, endOffsetY),
                                        createPairRow(startRotationTurns, endRotationTurns),
                                        createPairRow(startRotation, endRotation),
                                        headPositionRow = createPairRow(startHeadPosition, endHeadPosition),
                                        new FormEnumDropdown<ParametricSliderHeadPathType>
                                        {
                                            Caption = "Head control point",
                                            HintText = "Path type assigned to the first generated control point.",
                                            Current = headControlPointType
                                        },
                                        polyhedronFacesRow = createPairRow(startPolyhedronFaces, endPolyhedronFaces),
                                        yawTurnsRow = createPairRow(startYawTurns, endYawTurns),
                                        yawDegreesRow = createPairRow(startYawDegrees, endYawDegrees),
                                        pitchTurnsRow = createPairRow(startPitchTurns, endPitchTurns),
                                        pitchDegreesRow = createPairRow(startPitchDegrees, endPitchDegrees),
                                        rollTurnsRow = createPairRow(startRollTurns, endRollTurns),
                                        rollDegreesRow = createPairRow(startRollDegrees, endRollDegrees),
                                        perspectiveRow = createPairRow(startPerspective, endPerspective)
                                    }
                                }
                            }),
                    new EditorToolControlRow
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            createSingleFrameInactiveControl(new FormEnumDropdown<ParametricSliderAnimationEasing>
                            {
                                Width = 0.19f,
                                Caption = "Motion curve",
                                Current = easing
                            }, 0.19f),
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.19f,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                                Spacing = new Vector2(4, 0),
                                Children = new Drawable[]
                                {
                                    new RoundedButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.46f,
                                        Height = 48,
                                        Text = "Swap",
                                        TooltipText = "Swap all start and end parameters.",
                                        Action = swapStartEndParameters
                                    },
                                    new RoundedButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.46f,
                                        Height = 48,
                                        Text = "Copy > End",
                                        TooltipText = "Copy all start parameters to end parameters.",
                                        Action = copyStartParametersToEnd
                                    }
                                }
                            },
                            framesInput = new FormSliderBar<int>
                            {
                                Width = 0.19f,
                                Caption = "Frames",
                                HintText = "Number of sliders to generate.",
                                Current = new BindableInt((int)Math.Round(framesConfig.Value))
                                {
                                    MinValue = 1,
                                    MaxValue = 64
                                },
                                KeyboardStep = 1,
                                TabbableContentContainer = this
                            },
                            frameDurationInput = new FormSliderBar<int>
                            {
                                Width = 0.19f,
                                Caption = "Frame duration",
                                HintText = "Duration of every generated slider in units of the current snap divisor.",
                                Current = new BindableInt((int)Math.Round(frameDurationConfig.Value))
                                {
                                    MinValue = 1,
                                    MaxValue = 64
                                },
                                KeyboardStep = 1,
                                LabelFormat = value => value == 1 ? "1 snap" : $"{value} snaps",
                                TooltipFormat = value => value == 1 ? "1 snap" : $"{value} snaps",
                                TabbableContentContainer = this
                            },
                            createSingleFrameInactiveControl(frameGapInput = new FormSliderBar<double>
                            {
                                Width = 0.19f,
                                Caption = "Frame gap",
                                HintText = "Empty time from the previous slider tail to the next slider head in units of the current snap divisor.",
                                Current = new BindableDouble(frameGapConfig.Value)
                                {
                                    MinValue = 0,
                                    MaxValue = 64,
                                    Precision = 1
                                },
                                KeyboardStep = 1,
                                LabelFormat = value => formatSnapCount(value),
                                TooltipFormat = value => formatSnapCount(value),
                                TabbableContentContainer = this
                            }, 0.19f)
                        }
                    },
                    resultText = new EditorToolStatus { Text = "Preparing sliders..." },
                    new EditorToolActionRow(createButton = new RoundedButton { Action = commit }, Hide)
                }
            };
        }

        private void loadConfig(OsuConfigManager config)
        {
            shapeConfig = config.GetBindable<string>(OsuSetting.EditorParametricSliderShape);
            easingConfig = config.GetBindable<string>(OsuSetting.EditorParametricSliderAnimationEasing);
            startVerticesConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderVertices);
            endVerticesConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndVertices);
            startIterationsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderIterations);
            endIterationsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndIterations);
            startResolutionConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderResolution);
            endResolutionConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndResolution);
            symbolTextConfig = config.GetBindable<string>(OsuSetting.EditorParametricSliderSymbolText);
            symbolSequenceConfig = config.GetBindable<string>(OsuSetting.EditorParametricSliderSymbolSequenceText);
            symbolColumnSpacingConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderSymbolSequenceColumnSpacing);
            symbolRowSpacingConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderSymbolSequenceRowSpacing);
            symbolColumnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderSymbolSequenceColumns);
            startPrimaryConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderPrimaryParameter);
            endPrimaryConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndPrimaryParameter);
            startSecondaryConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderSecondaryParameter);
            endSecondaryConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndSecondaryParameter);
            startJuliaRealConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderJuliaReal);
            endJuliaRealConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndJuliaReal);
            startJuliaImaginaryConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderJuliaImaginary);
            endJuliaImaginaryConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndJuliaImaginary);
            startHeadPositionConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderHeadPosition);
            endHeadPositionConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndHeadPosition);
            headControlPointTypeConfig = config.GetBindable<string>(OsuSetting.EditorParametricSliderHeadControlPointType);
            startPolyhedronFacesConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderPolyhedronFaces);
            endPolyhedronFacesConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndPolyhedronFaces);
            startYawTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderYawTurns);
            endYawTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndYawTurns);
            startYawDegreesConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderYawDegrees);
            endYawDegreesConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndYawDegrees);
            startPitchTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderPitchTurns);
            endPitchTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndPitchTurns);
            startPitchDegreesConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderPitchDegrees);
            endPitchDegreesConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndPitchDegrees);
            startRollTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderRollTurns);
            endRollTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndRollTurns);
            startRollDegreesConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderRollDegrees);
            endRollDegreesConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndRollDegrees);
            startPerspectiveConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderPerspective);
            endPerspectiveConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndPerspective);
            startScaleConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationStartScale);
            endScaleConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndScale);
            startOffsetXConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderOffsetX);
            endOffsetXConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndOffsetX);
            startOffsetYConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderOffsetY);
            endOffsetYConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndOffsetY);
            startRotationTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderRotationTurns);
            endRotationTurnsConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationEndRotationTurns);
            startRotationConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderRotation);
            endRotationConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationEndRotation);
            framesConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationFrames);
            frameDurationConfig = config.GetBindable<float>(OsuSetting.EditorParametricSliderAnimationFrameDurationSnaps);
            frameGapConfig = config.GetBindable<double>(OsuSetting.EditorParametricSliderAnimationFrameGapSnaps);

            shape.Value = parseEnum(shapeConfig.Value, ParametricSliderShape.Polygon);
            easing.Value = parseEnum(easingConfig.Value, ParametricSliderAnimationEasing.EaseInOut);
            headControlPointType.Value = parseEnum(headControlPointTypeConfig.Value, ParametricSliderHeadPathType.Linear);
        }

        private void initialiseParameterControls()
        {
            startPrimaryBindable = new BindableDouble(startPrimaryConfig.Value) { MinValue = 1, MaxValue = 32, Precision = 0.1 };
            endPrimaryBindable = new BindableDouble(endPrimaryConfig.Value) { MinValue = 1, MaxValue = 32, Precision = 0.1 };
            startSecondaryBindable = new BindableDouble(startSecondaryConfig.Value) { MinValue = 1, MaxValue = 32, Precision = 0.1 };
            endSecondaryBindable = new BindableDouble(endSecondaryConfig.Value) { MinValue = 1, MaxValue = 32, Precision = 0.1 };

            startVertices = createIntInput("Start vertices", startVerticesConfig.Value, 3, 32);
            endVertices = createIntInput("End vertices", endVerticesConfig.Value, 3, 32);
            startIterations = createIntInput("Start depth", startIterationsConfig.Value, 1, 10);
            endIterations = createIntInput("End depth", endIterationsConfig.Value, 1, 10);
            startResolution = createIntInput("Start resolution", startResolutionConfig.Value, 8, 180, 4);
            endResolution = createIntInput("End resolution", endResolutionConfig.Value, 8, 180, 4);
            startPrimary = createDoubleInput("Start parameter", startPrimaryBindable, 0.1f);
            endPrimary = createDoubleInput("End parameter", endPrimaryBindable, 0.1f);
            startSecondary = createDoubleInput("Start secondary", startSecondaryBindable, 0.1f);
            endSecondary = createDoubleInput("End secondary", endSecondaryBindable, 0.1f);
            startJuliaReal = createDoubleInput("Start real", startJuliaRealConfig.Value, -2, 2, 0.01, 0.05f, "0.00");
            endJuliaReal = createDoubleInput("End real", endJuliaRealConfig.Value, -2, 2, 0.01, 0.05f, "0.00");
            startJuliaImaginary = createDoubleInput("Start imaginary", startJuliaImaginaryConfig.Value, -2, 2, 0.001, 0.01f, "0.000");
            endJuliaImaginary = createDoubleInput("End imaginary", endJuliaImaginaryConfig.Value, -2, 2, 0.001, 0.01f, "0.000");
            startScale = createScaleInput("Start scale", startScaleConfig.Value);
            endScale = createScaleInput("End scale", endScaleConfig.Value);
            startOffsetX = createOffsetInput("Start offset X", startOffsetXConfig.Value, 512);
            endOffsetX = createOffsetInput("End offset X", endOffsetXConfig.Value, 512);
            startOffsetY = createOffsetInput("Start offset Y", startOffsetYConfig.Value, 384);
            endOffsetY = createOffsetInput("End offset Y", endOffsetYConfig.Value, 384);
            (int startRotationTurnsValue, double startRotationDegreesValue) = normaliseAngleParts(startRotationTurnsConfig.Value, startRotationConfig.Value);
            (int endRotationTurnsValue, double endRotationDegreesValue) = normaliseAngleParts(endRotationTurnsConfig.Value, endRotationConfig.Value);
            startRotationTurns = createTurnsInput("Start rotation turns", startRotationTurnsValue);
            endRotationTurns = createTurnsInput("End rotation turns", endRotationTurnsValue);
            startRotation = createRotationDegreesInput("Start rotation degrees", startRotationDegreesValue);
            endRotation = createRotationDegreesInput("End rotation degrees", endRotationDegreesValue);
            startHeadPosition = createHeadPositionInput("Start head", startHeadPositionConfig.Value);
            endHeadPosition = createHeadPositionInput("End head", endHeadPositionConfig.Value);
            startPolyhedronFaces = createIntInput("Start faces", startPolyhedronFacesConfig.Value, 4, 20);
            endPolyhedronFaces = createIntInput("End faces", endPolyhedronFacesConfig.Value, 4, 20);
            startYawTurns = createTurnsInput("Start yaw turns", startYawTurnsConfig.Value);
            endYawTurns = createTurnsInput("End yaw turns", endYawTurnsConfig.Value);
            startYawDegrees = createRotationDegreesInput("Start yaw degrees", startYawDegreesConfig.Value);
            endYawDegrees = createRotationDegreesInput("End yaw degrees", endYawDegreesConfig.Value);
            startPitchTurns = createTurnsInput("Start pitch turns", startPitchTurnsConfig.Value);
            endPitchTurns = createTurnsInput("End pitch turns", endPitchTurnsConfig.Value);
            startPitchDegrees = createRotationDegreesInput("Start pitch degrees", startPitchDegreesConfig.Value);
            endPitchDegrees = createRotationDegreesInput("End pitch degrees", endPitchDegreesConfig.Value);
            startRollTurns = createTurnsInput("Start roll turns", startRollTurnsConfig.Value);
            endRollTurns = createTurnsInput("End roll turns", endRollTurnsConfig.Value);
            startRollDegrees = createRotationDegreesInput("Start roll degrees", startRollDegreesConfig.Value);
            endRollDegrees = createRotationDegreesInput("End roll degrees", endRollDegreesConfig.Value);
            startPerspective = createPercentInput("Start perspective", startPerspectiveConfig.Value);
            endPerspective = createPercentInput("End perspective", endPerspectiveConfig.Value);
            symbolColumnSpacingInput = createDoubleInput("Spacing X", symbolColumnSpacingConfig.Value, 0, 512, 1, 8, "0");
            symbolRowSpacingInput = createDoubleInput("Spacing Y", symbolRowSpacingConfig.Value, 0, 512, 1, 8, "0");
            symbolColumnsInput = createIntInput("Columns", symbolColumnsConfig.Value, 1, 64);
        }

        private FillFlowContainer createPairRow(Drawable start, Drawable end)
        {
            start.Width = 0.47f;

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
                Spacing = new Vector2(6, 0),
                Children = new[] { start, createSingleFrameInactiveControl(end, 0.47f) }
            };
        }

        private Drawable createSymbolRow()
        {
            symbolInput = new OsuTextBox
            {
                RelativeSizeAxes = Axes.X,
                RelativePositionAxes = Axes.X,
                Width = 0.7f,
                Height = 34,
                X = 0.3f,
                PlaceholderText = "A, R, alpha, int, subset, to...",
                SelectAllOnFocus = true,
            };
            symbolInput.Current.Value = symbolTextConfig.Value ?? string.Empty;

            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 38,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.28f,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.Default.With(size: 14),
                        Text = "Symbol"
                    },
                    symbolInput
                }
            };
        }

        private Drawable createSymbolSequenceRow()
        {
            symbolSequenceInput = new OsuTextBox
            {
                RelativeSizeAxes = Axes.X,
                RelativePositionAxes = Axes.X,
                Width = 0.7f,
                Height = 34,
                X = 0.3f,
                PlaceholderText = "A B C alpha beta gamma",
                SelectAllOnFocus = true,
            };
            symbolSequenceInput.Current.Value = symbolSequenceConfig.Value ?? string.Empty;

            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 38,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.28f,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.Default.With(size: 14),
                        Text = "Sequence"
                    },
                    symbolSequenceInput
                }
            };
        }

        private FillFlowContainer createSymbolLayoutRow()
        {
            symbolColumnsInput.Width = 0.32f;
            symbolColumnSpacingInput.Width = 0.32f;
            symbolRowSpacingInput.Width = 0.32f;

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[] { symbolColumnsInput, symbolColumnSpacingInput, symbolRowSpacingInput }
            };
        }

        private SingleFrameInactiveContainer createSingleFrameInactiveControl(Drawable content, float width)
        {
            var container = SliderToolboxUiUtils.CreateSingleFrameInactiveControl(content, width);

            singleFrameInactiveControls.Add(container);
            return container;
        }

        private FormSliderBar<int> createIntInput(string caption, double value, int min, int max, int step = 1) => new FormSliderBar<int>
        {
            Caption = caption,
            Current = new BindableInt((int)Math.Round(value))
            {
                MinValue = min,
                MaxValue = max
            },
            KeyboardStep = step,
            TabbableContentContainer = this
        };

        private FormSliderBar<double> createDoubleInput(string caption, BindableDouble current, float step) => new FormSliderBar<double>
        {
            Caption = caption,
            Current = current,
            KeyboardStep = step,
            LabelFormat = value => $"{value:0.#}",
            TooltipFormat = value => $"{value:0.#}",
            TabbableContentContainer = this
        };

        private FormSliderBar<double> createDoubleInput(string caption, double value, double min, double max, double precision, float step, string format) =>
            new FormSliderBar<double>
            {
                Caption = caption,
                Current = new BindableDouble(value)
                {
                    MinValue = min,
                    MaxValue = max,
                    Precision = precision
                },
                KeyboardStep = step,
                LabelFormat = component => component.ToString(format),
                TooltipFormat = component => component.ToString(format),
                TabbableContentContainer = this
            };

        private FormSliderBar<double> createPercentInput(string caption, double value) => new FormSliderBar<double>
        {
            Caption = caption,
            Current = new BindableDouble(value)
            {
                MinValue = 0,
                MaxValue = 1,
                Precision = 0.001
            },
            KeyboardStep = 0.01f,
            LabelFormat = component => $"{component:P0}",
            TooltipFormat = component => $"{component:P1}",
            TabbableContentContainer = this
        };

        private FormSliderBar<double> createHeadPositionInput(string caption, double value) => new FormSliderBar<double>
        {
            Caption = caption,
            HintText = "Moves the slider head around the closed path. 1 turn is one full loop.",
            Current = new BindableDouble(value)
            {
                MinValue = 0,
                MaxValue = 256,
                Precision = 0.001
            },
            KeyboardStep = 0.01f,
            LabelFormat = value => formatHeadPosition(value),
            TooltipFormat = value => formatHeadPosition(value),
            TabbableContentContainer = this
        };

        private static string formatHeadPosition(double value) => $"{value:0.###} turns";

        private static string formatSnapCount(double value) => SliderToolboxUiUtils.FormatSnapCount(value);

        private FormSliderBar<double> createOffsetInput(string caption, double value, double maximum) => new FormSliderBar<double>
        {
            Caption = caption,
            HintText = "Moves the generated slider object after shape generation.",
            Current = new BindableDouble(value)
            {
                MinValue = -maximum,
                MaxValue = maximum,
                Precision = 1
            },
            KeyboardStep = 5,
            LabelFormat = component => $"{component:0}px",
            TooltipFormat = component => $"{component:0}px",
            TabbableContentContainer = this
        };

        private FormSliderBar<int> createTurnsInput(string caption, double value) => new FormSliderBar<int>
        {
            Caption = caption,
            HintText = "Full 360 degree turns.",
            Current = new BindableInt((int)Math.Round(value))
            {
                MinValue = -32,
                MaxValue = 32
            },
            KeyboardStep = 1,
            LabelFormat = value => value == 1 || value == -1 ? $"{value} turn" : $"{value} turns",
            TooltipFormat = value => value == 1 || value == -1 ? $"{value} turn" : $"{value} turns",
            TabbableContentContainer = this
        };

        private FormSliderBar<double> createRotationDegreesInput(string caption, double value) => new FormSliderBar<double>
        {
            Caption = caption,
            HintText = "Residual degrees added after full turns.",
            Current = new BindableDouble(value)
            {
                MinValue = -359,
                MaxValue = 359,
                Precision = 1
            },
            KeyboardStep = 5,
            LabelFormat = component => $"{component:0} deg",
            TooltipFormat = component => $"{component:0} deg",
            TabbableContentContainer = this
        };

        private FormSliderBar<double> createScaleInput(string caption, double value) => new FormSliderBar<double>
        {
            Caption = caption,
            Current = new BindableDouble(value)
            {
                MinValue = 0.1,
                MaxValue = 4,
                Precision = 0.01
            },
            KeyboardStep = 0.05f,
            LabelFormat = scale => $"{scale:P0}",
            TooltipFormat = scale => $"{scale:P0}",
            TabbableContentContainer = this
        };

        private Drawable createShapeGrid(ParametricSliderShapeCategory category)
        {
            ParametricSliderShape[] shapes = Enum.GetValues<ParametricSliderShape>()
                                                 .Where(candidate => ParametricSliderGenerator.GetCategory(candidate) == category)
                                                 .Where(ParametricSliderGenerator.IsVisibleInToolbox)
                                                 .ToArray();
            var rows = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 3),
                Padding = new MarginPadding { Right = 5 }
            };

            for (int i = 0; i < shapes.Length; i += 3)
            {
                var row = new EditorToolControlRow(120)
                {
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(4, 0)
                };

                for (int column = 0; column < 3 && i + column < shapes.Length; column++)
                {
                    ParametricSliderShape availableShape = shapes[i + column];
                    var button = new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.325f,
                        Height = 28,
                        Text = getShapeName(availableShape),
                        TooltipText = getShapeName(availableShape),
                        BackgroundColour = unselectedShapeColour,
                        Action = () => shape.Value = availableShape
                    };

                    shapeButtons.Add(availableShape, button);
                    row.Add(button);
                }

                rows.Add(row);
            }

            return new OsuScrollContainer(Direction.Vertical)
            {
                RelativeSizeAxes = Axes.Both,
                Child = rows
            };
        }

        private void toggleCategory(ParametricSliderShapeCategory category)
        {
            expandedCategory = expandedCategory == category ? null : category;
            updateCategoryState();
        }

        private void updateCategoryState()
        {
            foreach ((ParametricSliderShapeCategory category, Drawable content) in categoryContents)
            {
                bool expanded = expandedCategory == category;
                setVisible(content, expanded);

                RoundedButton button = categoryButtons[category];
                button.Text = $"{(expanded ? "-" : "+")} {getCategoryName(category)}";
                button.BackgroundColour = expanded ? selectedShapeColour : unselectedShapeColour;
            }
        }

        private void updateShapeButtonSelection()
        {
            foreach ((ParametricSliderShape buttonShape, RoundedButton button) in shapeButtons)
                button.BackgroundColour = buttonShape == shape.Value ? selectedShapeColour : unselectedShapeColour;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            insertionTime.Current.BindValueChanged(_ => schedulePreview());
            shape.BindValueChanged(_ =>
            {
                updateShapeButtonSelection();
                updateParameterAvailability();
                updateFrameCountMode();
                schedulePreview();
            });
            easing.BindValueChanged(_ => schedulePreview());
            headControlPointType.BindValueChanged(_ => schedulePreview());
            bindPreview(startVertices, endVertices, startIterations, endIterations, startResolution, endResolution,
                frameDurationInput);
            framesInput.Current.BindValueChanged(_ =>
            {
                updateFrameCountMode();
                schedulePreview();
            });
            symbolInput.Current.BindValueChanged(_ => schedulePreview());
            symbolSequenceInput.Current.BindValueChanged(_ =>
            {
                updateFrameCountMode();
                schedulePreview();
            });
            bindPreview(startPrimary, endPrimary, startSecondary, endSecondary, startJuliaReal, endJuliaReal,
                startJuliaImaginary, endJuliaImaginary, startHeadPosition, endHeadPosition,
                startYawDegrees, endYawDegrees, startPitchDegrees, endPitchDegrees, startRollDegrees, endRollDegrees,
                startPerspective, endPerspective, startScale, endScale,
                startOffsetX, endOffsetX, startOffsetY, endOffsetY, startRotation, endRotation,
                frameGapInput, symbolColumnSpacingInput, symbolRowSpacingInput);
            bindPreview(symbolColumnsInput, startRotationTurns, endRotationTurns, startYawTurns, endYawTurns, startPitchTurns, endPitchTurns, startRollTurns, endRollTurns);
            bindPolyhedronFaces(startPolyhedronFaces);
            bindPolyhedronFaces(endPolyhedronFaces);
            beatDivisor = (BindableBeatDivisor)beatDivisor.GetBoundCopy();
            beatDivisor.BindValueChanged(_ => schedulePreview());
            panelOpacityInput.Current.BindValueChanged(value => Body.FadeTo((float)value.NewValue, 80, Easing.OutQuint), true);
            newComboState.BindValueChanged(_ => schedulePreview());

            ScheduleAfterChildren(() =>
            {
                updateCategoryState();
                updateShapeButtonSelection();
                updateParameterAvailability();
                updateFrameCountMode();
                applyPreview();
            });
        }

        private void bindPreview(params FormSliderBar<int>[] controls)
        {
            foreach (FormSliderBar<int> control in controls)
                control.Current.BindValueChanged(_ => schedulePreview());
        }

        private void bindPreview(params FormSliderBar<double>[] controls)
        {
            foreach (FormSliderBar<double> control in controls)
                control.Current.BindValueChanged(_ => schedulePreview());
        }

        private void bindPolyhedronFaces(FormSliderBar<int> control)
        {
            control.Current.BindValueChanged(value =>
            {
                int normalised = ParametricSliderGenerator.NormalisePolyhedronFaceCount(value.NewValue);

                if (normalised != value.NewValue)
                    control.Current.Value = normalised;
                else
                    schedulePreview();
            });
        }

        private void updateParameterAvailability()
        {
            bool usesVertices = ParametricSliderGenerator.UsesVertices(shape.Value);
            string verticesName = ParametricSliderGenerator.GetVerticesParameterName(shape.Value);
            startVertices.Caption = $"Start {verticesName}";
            endVertices.Caption = $"End {verticesName}";
            setVisible(verticesRow, usesVertices);
            setVisible(iterationsRow, ParametricSliderGenerator.UsesIterations(shape.Value));
            setVisible(resolutionRow, ParametricSliderGenerator.UsesResolution(shape.Value));
            bool usesSymbolText = ParametricSliderGenerator.UsesSymbolText(shape.Value);
            setVisible(symbolRow, usesSymbolText);
            setVisible(symbolSequenceRow, usesSymbolText);
            setVisible(symbolLayoutRow, usesSymbolText);

            bool usesPrimary = ParametricSliderGenerator.UsesCurveParameter(shape.Value);
            string primaryName = ParametricSliderGenerator.GetPrimaryParameterName(shape.Value);
            startPrimary.Caption = $"Start {primaryName}";
            endPrimary.Caption = $"End {primaryName}";
            bool discretePrimary = ParametricSliderGenerator.PrimaryParameterIsDiscrete(shape.Value);
            startPrimaryBindable.Precision = endPrimaryBindable.Precision = discretePrimary ? 1 : 0.1;
            startPrimary.KeyboardStep = endPrimary.KeyboardStep = discretePrimary ? 1 : 0.1f;
            setVisible(primaryRow, usesPrimary);

            bool usesSecondary = ParametricSliderGenerator.UsesSecondaryCurveParameter(shape.Value);
            string secondaryName = ParametricSliderGenerator.GetSecondaryParameterName(shape.Value);
            startSecondary.Caption = $"Start {secondaryName}";
            endSecondary.Caption = $"End {secondaryName}";
            bool discreteSecondary = ParametricSliderGenerator.SecondaryParameterIsDiscrete(shape.Value);
            startSecondaryBindable.Precision = endSecondaryBindable.Precision = discreteSecondary ? 1 : 0.1;
            startSecondary.KeyboardStep = endSecondary.KeyboardStep = discreteSecondary ? 1 : 0.1f;
            setVisible(secondaryRow, usesSecondary);

            bool usesJulia = shape.Value == ParametricSliderShape.JuliaSet;
            setVisible(juliaRealRow, usesJulia);
            setVisible(juliaImaginaryRow, usesJulia);
            setVisible(headPositionRow, ParametricSliderGenerator.SupportsCyclicHeadPosition(shape.Value));

            bool usesProjection = ParametricSliderGenerator.GetCategory(shape.Value) == ParametricSliderShapeCategory.Projections3D;
            setVisible(polyhedronFacesRow, shape.Value == ParametricSliderShape.RegularPolyhedronProjection);
            setVisible(yawTurnsRow, usesProjection);
            setVisible(yawDegreesRow, usesProjection);
            setVisible(pitchTurnsRow, usesProjection);
            setVisible(pitchDegreesRow, usesProjection);
            setVisible(rollTurnsRow, usesProjection);
            setVisible(rollDegreesRow, usesProjection);
            setVisible(perspectiveRow, usesProjection);
        }

        private void updateFrameCountMode()
        {
            bool singleFrame = framesInput.Current.Value == 1;

            foreach (SingleFrameInactiveContainer control in singleFrameInactiveControls)
                control.Inactive = singleFrame;
        }

        private string[] getSymbolSequenceTokens() =>
            (symbolSequenceInput.Current.Value ?? string.Empty)
                               .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(token => token.Trim())
                               .Where(token => token.Length > 0)
                               .ToArray();

        private Vector2 getSymbolSequenceOffset(int index)
        {
            int columns = Math.Max(1, symbolColumnsInput.Current.Value);
            int column = index % columns;
            int row = index / columns;

            return new Vector2(
                column * (float)symbolColumnSpacingInput.Current.Value,
                row * (float)symbolRowSpacingInput.Current.Value);
        }

        private void swapStartEndParameters()
        {
            swap(startVertices, endVertices);
            swap(startIterations, endIterations);
            swap(startResolution, endResolution);
            swap(startPrimary, endPrimary);
            swap(startSecondary, endSecondary);
            swap(startJuliaReal, endJuliaReal);
            swap(startJuliaImaginary, endJuliaImaginary);
            swap(startScale, endScale);
            swap(startOffsetX, endOffsetX);
            swap(startOffsetY, endOffsetY);
            swap(startRotationTurns, endRotationTurns);
            swap(startRotation, endRotation);
            swap(startHeadPosition, endHeadPosition);
            swap(startPolyhedronFaces, endPolyhedronFaces);
            swap(startYawTurns, endYawTurns);
            swap(startYawDegrees, endYawDegrees);
            swap(startPitchTurns, endPitchTurns);
            swap(startPitchDegrees, endPitchDegrees);
            swap(startRollTurns, endRollTurns);
            swap(startRollDegrees, endRollDegrees);
            swap(startPerspective, endPerspective);

            schedulePreview();
        }

        private void copyStartParametersToEnd()
        {
            copy(startVertices, endVertices);
            copy(startIterations, endIterations);
            copy(startResolution, endResolution);
            copy(startPrimary, endPrimary);
            copy(startSecondary, endSecondary);
            copy(startJuliaReal, endJuliaReal);
            copy(startJuliaImaginary, endJuliaImaginary);
            copy(startScale, endScale);
            copy(startOffsetX, endOffsetX);
            copy(startOffsetY, endOffsetY);
            copy(startRotationTurns, endRotationTurns);
            copy(startRotation, endRotation);
            copy(startHeadPosition, endHeadPosition);
            copy(startPolyhedronFaces, endPolyhedronFaces);
            copy(startYawTurns, endYawTurns);
            copy(startYawDegrees, endYawDegrees);
            copy(startPitchTurns, endPitchTurns);
            copy(startPitchDegrees, endPitchDegrees);
            copy(startRollTurns, endRollTurns);
            copy(startRollDegrees, endRollDegrees);
            copy(startPerspective, endPerspective);

            schedulePreview();
        }

        private static void swap(FormSliderBar<int> first, FormSliderBar<int> second)
            => SliderToolboxUiUtils.Swap(first, second);

        private static void swap(FormSliderBar<double> first, FormSliderBar<double> second)
            => SliderToolboxUiUtils.Swap(first, second);

        private static void copy(FormSliderBar<int> source, FormSliderBar<int> target) =>
            SliderToolboxUiUtils.Copy(source, target);

        private static void copy(FormSliderBar<double> source, FormSliderBar<double> target) =>
            SliderToolboxUiUtils.Copy(source, target);

        private static void setVisible(Drawable drawable, bool visible)
        {
            if (visible)
                drawable.Show();
            else
                drawable.Hide();
        }

        private void schedulePreview()
        {
            if (!PreviewClosed)
                Scheduler.AddOnce(applyPreview);
        }

        private void applyPreview()
        {
            if (!BeginPreview())
                return;

            string[] symbolSequenceTokens = getSymbolSequenceTokens();
            bool symbolSequenceMode = ParametricSliderGenerator.UsesSymbolText(shape.Value) && symbolSequenceTokens.Length > 0;
            int frameCount = symbolSequenceMode ? symbolSequenceTokens.Length : framesInput.Current.Value;
            double sequenceStartTime = beatSnapProvider.SnapTime(insertionTime.Current.Value);
            double durationBeats = frameDurationInput.Current.Value / (double)beatSnapProvider.BeatDivisor;
            double gapBeats = frameGapInput.Current.Value / beatSnapProvider.BeatDivisor;
            double frameTime = sequenceStartTime;
            double sequenceEndTime = sequenceStartTime;
            var generatedSliders = new List<Slider>(frameCount);

            if (frameCount > 512)
            {
                showError("Symbol sequence is limited to 512 sliders.");
                return;
            }

            if (!double.IsFinite(durationBeats) || durationBeats <= 0 || !double.IsFinite(gapBeats) || gapBeats < 0)
            {
                showError("Frame duration must be positive and frame gap cannot be negative.");
                return;
            }

            for (int index = 0; index < frameCount; index++)
            {
                double rawProgress = symbolSequenceMode || frameCount == 1 ? 0 : (double)index / (frameCount - 1);
                double progress = applyEasing(rawProgress, easing.Value);
                double startTime = frameTime;
                sequenceEndTime = BeatTimeline.TimeAfterBeats(editorBeatmap.ControlPointInfo, startTime, durationBeats);
                double targetDuration = sequenceEndTime - startTime;
                frameTime = BeatTimeline.TimeAfterBeats(editorBeatmap.ControlPointInfo, sequenceEndTime, gapBeats);
                string symbolText = symbolSequenceMode ? symbolSequenceTokens[index] : symbolInput.Current.Value;
                Vector2 layoutOffset = symbolSequenceMode ? getSymbolSequenceOffset(index) : Vector2.Zero;

                Slider? slider = createFrame(progress, startTime, targetDuration, index == 0, symbolText, layoutOffset);

                if (slider == null)
                    return;

                generatedSliders.Add(slider);
            }

            TernaryState previousNewComboState = newComboState.Value;
            ReplacePreviewObjects(previewSliders, generatedSliders);
            newComboState.Value = previousNewComboState;

            double totalDuration = sequenceEndTime - sequenceStartTime;
            string durationUnit = frameDurationInput.Current.Value == 1 ? "1 snap" : $"{frameDurationInput.Current.Value} snaps";
            resultText.Text = symbolSequenceMode
                ? $"{frameCount} symbols | {symbolColumnsInput.Current.Value} columns | {durationUnit} each | {formatSnapCount(frameGapInput.Current.Value)} gap"
                : frameCount == 1
                ? $"1 frame | {durationUnit} ({totalDuration:0.##} ms)"
                : $"{frameCount} frames | {durationUnit} each | {formatSnapCount(frameGapInput.Current.Value)} gap | {totalDuration:0.##} ms total";
            createButton.Enabled.Value = true;
        }

        private Slider? createFrame(double progress, double startTime, double targetDuration, bool firstFrame, string symbolText, Vector2 layoutOffset)
        {
            int polyhedronFaces = ParametricSliderGenerator.NormalisePolyhedronFaceCount(interpolateInt(startPolyhedronFaces, endPolyhedronFaces, progress));
            (double quaternionX, double quaternionY, double quaternionZ, double quaternionW) = interpolateQuaternion(progress);

            IReadOnlyList<Vector2> points = ParametricSliderGenerator.Generate(
                shape.Value,
                interpolateInt(startVertices, endVertices, progress),
                interpolateInt(startIterations, endIterations, progress),
                interpolateInt(startResolution, endResolution, progress),
                interpolate(startJuliaReal, endJuliaReal, progress),
                interpolate(startJuliaImaginary, endJuliaImaginary, progress),
                interpolate(startPrimary, endPrimary, progress),
                interpolate(startSecondary, endSecondary, progress),
                polyhedronFaces,
                quaternionX,
                quaternionY,
                quaternionZ,
                quaternionW,
                interpolate(startPerspective, endPerspective, progress),
                symbolText);

            if (ParametricSliderGenerator.SupportsCyclicHeadPosition(shape.Value))
                points = ParametricSliderGenerator.MoveHeadAlongClosedPath(points, interpolate(startHeadPosition, endHeadPosition, progress));

            points = ParametricSliderGenerator.TransformObject(
                points,
                interpolate(startScale, endScale, progress),
                interpolateAngle(startRotationTurns, startRotation, endRotationTurns, endRotation, progress),
                interpolate(startOffsetX, endOffsetX, progress) + layoutOffset.X,
                interpolate(startOffsetY, endOffsetY, progress) + layoutOffset.Y);

            if (points.Count < 2)
            {
                showError("A frame produced an unusable path.");
                return null;
            }

            Vector2 headPosition = points[0];
            var controlPoints = points.Select((point, index) =>
                new PathControlPoint(point - headPosition, index == 0 ? ParametricSliderGenerator.GetHeadPathType(headControlPointType.Value) : null)).ToArray();
            var path = new SliderPath(controlPoints);
            path.ExpectedDistance.Value = path.CalculatedDistance;

            TimingControlPoint timingPoint = editorBeatmap.ControlPointInfo.TimingPointAt(startTime);
            double sliderVelocity = path.CalculatedDistance * timingPoint.BeatLength
                                    / (OsuHitObject.BASE_SCORING_DISTANCE * editorBeatmap.Difficulty.SliderMultiplier * targetDuration);

            if (!double.IsFinite(sliderVelocity) || sliderVelocity < 0.1 || sliderVelocity > 10)
            {
                showError($"Equal duration requires {sliderVelocity:0.###}x SV. Adjust frame duration to keep every frame within 0.1x-10x.");
                return null;
            }

            var slider = new Slider
            {
                Position = headPosition,
                StartTime = startTime,
                NewCombo = firstFrame && newComboState.Value == TernaryState.True,
                SliderVelocityMultiplier = sliderVelocity,
                Path = path
            };
            slider.Samples.Add(slider.CreateHitSampleInfo());
            return slider;
        }

        private (double X, double Y, double Z, double W) interpolateQuaternion(double progress)
        {
            double yaw = interpolateAngle(startYawTurns, startYawDegrees, endYawTurns, endYawDegrees, progress);
            double pitch = interpolateAngle(startPitchTurns, startPitchDegrees, endPitchTurns, endPitchDegrees, progress);
            double roll = interpolateAngle(startRollTurns, startRollDegrees, endRollTurns, endRollDegrees, progress);

            return multiplyQuaternions(
                multiplyQuaternions(axisAngleQuaternion(0, 1, 0, yaw), axisAngleQuaternion(1, 0, 0, pitch)),
                axisAngleQuaternion(0, 0, 1, roll));
        }

        private static (double X, double Y, double Z, double W) axisAngleQuaternion(double axisX, double axisY, double axisZ, double degrees)
        {
            double halfRadians = Math.PI / 360 * degrees;
            double sine = Math.Sin(halfRadians);
            return (axisX * sine, axisY * sine, axisZ * sine, Math.Cos(halfRadians));
        }

        private static (double X, double Y, double Z, double W) multiplyQuaternions(
            (double X, double Y, double Z, double W) first,
            (double X, double Y, double Z, double W) second)
        {
            double x = first.W * second.X + first.X * second.W + first.Y * second.Z - first.Z * second.Y;
            double y = first.W * second.Y - first.X * second.Z + first.Y * second.W + first.Z * second.X;
            double z = first.W * second.Z + first.X * second.Y - first.Y * second.X + first.Z * second.W;
            double w = first.W * second.W - first.X * second.X - first.Y * second.Y - first.Z * second.Z;
            double length = Math.Sqrt(x * x + y * y + z * z + w * w);

            return length <= 0.000001 ? (0, 0, 0, 1) : (x / length, y / length, z / length, w / length);
        }

        private static double interpolate(FormSliderBar<double> start, FormSliderBar<double> end, double progress) =>
            SliderToolboxUiUtils.Interpolate(start, end, progress);

        private static double interpolateAngle(FormSliderBar<int> startTurns, FormSliderBar<double> startDegrees, FormSliderBar<int> endTurns, FormSliderBar<double> endDegrees, double progress) =>
            SliderToolboxUiUtils.InterpolateAngle(startTurns, startDegrees, endTurns, endDegrees, progress);

        private static (int Turns, double Degrees) normaliseAngleParts(double turns, double degrees)
        {
            if (degrees is >= -359 and <= 359)
                return ((int)Math.Round(turns), degrees);

            double totalDegrees = turns * 360 + degrees;
            int wholeTurns = (int)Math.Truncate(totalDegrees / 360);
            double residualDegrees = totalDegrees - wholeTurns * 360;

            return (wholeTurns, residualDegrees);
        }

        private static int interpolateInt(FormSliderBar<int> start, FormSliderBar<int> end, double progress) =>
            SliderToolboxUiUtils.InterpolateInt(start, end, progress);

        private static double applyEasing(double progress, ParametricSliderAnimationEasing animationEasing)
            => SliderToolboxUiUtils.ApplyEasing(progress, animationEasing);

        private void showError(string message)
        {
            removePreview();
            resultText.Text = message;
            createButton.Enabled.Value = false;
        }

        private void removePreview()
        {
            if (previewSliders.Count == 0)
                return;

            ReplacePreviewObjects(previewSliders, Array.Empty<Slider>());
        }

        private void commit()
        {
            applyPreview();

            if (!createButton.Enabled.Value)
                return;

            saveConfig();
            CommitPreview();
        }

        private void saveConfig()
        {
            shapeConfig.Value = shape.Value.ToString();
            easingConfig.Value = easing.Value.ToString();
            headControlPointTypeConfig.Value = headControlPointType.Value.ToString();
            framesConfig.Value = framesInput.Current.Value;
            frameDurationConfig.Value = frameDurationInput.Current.Value;
            frameGapConfig.Value = frameGapInput.Current.Value;
            symbolTextConfig.Value = symbolInput.Current.Value ?? string.Empty;
            symbolSequenceConfig.Value = symbolSequenceInput.Current.Value ?? string.Empty;
            symbolColumnSpacingConfig.Value = symbolColumnSpacingInput.Current.Value;
            symbolRowSpacingConfig.Value = symbolRowSpacingInput.Current.Value;
            symbolColumnsConfig.Value = symbolColumnsInput.Current.Value;

            startVerticesConfig.Value = startVertices.Current.Value;
            endVerticesConfig.Value = endVertices.Current.Value;
            startIterationsConfig.Value = startIterations.Current.Value;
            endIterationsConfig.Value = endIterations.Current.Value;
            startResolutionConfig.Value = startResolution.Current.Value;
            endResolutionConfig.Value = endResolution.Current.Value;
            startPrimaryConfig.Value = startPrimary.Current.Value;
            endPrimaryConfig.Value = endPrimary.Current.Value;
            startSecondaryConfig.Value = startSecondary.Current.Value;
            endSecondaryConfig.Value = endSecondary.Current.Value;
            startJuliaRealConfig.Value = startJuliaReal.Current.Value;
            endJuliaRealConfig.Value = endJuliaReal.Current.Value;
            startJuliaImaginaryConfig.Value = startJuliaImaginary.Current.Value;
            endJuliaImaginaryConfig.Value = endJuliaImaginary.Current.Value;
            startHeadPositionConfig.Value = startHeadPosition.Current.Value;
            endHeadPositionConfig.Value = endHeadPosition.Current.Value;
            startPolyhedronFacesConfig.Value = startPolyhedronFaces.Current.Value;
            endPolyhedronFacesConfig.Value = endPolyhedronFaces.Current.Value;
            startYawTurnsConfig.Value = startYawTurns.Current.Value;
            endYawTurnsConfig.Value = endYawTurns.Current.Value;
            startYawDegreesConfig.Value = startYawDegrees.Current.Value;
            endYawDegreesConfig.Value = endYawDegrees.Current.Value;
            startPitchTurnsConfig.Value = startPitchTurns.Current.Value;
            endPitchTurnsConfig.Value = endPitchTurns.Current.Value;
            startPitchDegreesConfig.Value = startPitchDegrees.Current.Value;
            endPitchDegreesConfig.Value = endPitchDegrees.Current.Value;
            startRollTurnsConfig.Value = startRollTurns.Current.Value;
            endRollTurnsConfig.Value = endRollTurns.Current.Value;
            startRollDegreesConfig.Value = startRollDegrees.Current.Value;
            endRollDegreesConfig.Value = endRollDegrees.Current.Value;
            startPerspectiveConfig.Value = startPerspective.Current.Value;
            endPerspectiveConfig.Value = endPerspective.Current.Value;
            startScaleConfig.Value = startScale.Current.Value;
            endScaleConfig.Value = endScale.Current.Value;
            startOffsetXConfig.Value = startOffsetX.Current.Value;
            endOffsetXConfig.Value = endOffsetX.Current.Value;
            startOffsetYConfig.Value = startOffsetY.Current.Value;
            endOffsetYConfig.Value = endOffsetY.Current.Value;
            startRotationTurnsConfig.Value = startRotationTurns.Current.Value;
            endRotationTurnsConfig.Value = endRotationTurns.Current.Value;
            startRotationConfig.Value = startRotation.Current.Value;
            endRotationConfig.Value = endRotation.Current.Value;
        }

        protected override void RestorePreview() => removePreview();

        private static string getShapeName(ParametricSliderShape sliderShape) => sliderShape switch
        {
            ParametricSliderShape.StarPolygon => "Star polygon",
            ParametricSliderShape.ArrowHead => "Arrowhead",
            ParametricSliderShape.SymbolLatinUppercase => "Latin uppercase",
            ParametricSliderShape.SymbolLatinLowercase => "Latin lowercase",
            ParametricSliderShape.SymbolItalicUppercase => "Italic uppercase",
            ParametricSliderShape.SymbolItalicLowercase => "Italic lowercase",
            ParametricSliderShape.SymbolBoldUppercase => "Bold uppercase",
            ParametricSliderShape.SymbolBoldLowercase => "Bold lowercase",
            ParametricSliderShape.SymbolScriptUppercase => "Script uppercase",
            ParametricSliderShape.SymbolScriptLowercase => "Script lowercase",
            ParametricSliderShape.SymbolFrakturUppercase => "Fraktur uppercase",
            ParametricSliderShape.SymbolFrakturLowercase => "Fraktur lowercase",
            ParametricSliderShape.SymbolBoldItalicUppercase => "Bold italic uppercase",
            ParametricSliderShape.SymbolBoldItalicLowercase => "Bold italic lowercase",
            ParametricSliderShape.SymbolSansSerifUppercase => "Sans-serif uppercase",
            ParametricSliderShape.SymbolSansSerifLowercase => "Sans-serif lowercase",
            ParametricSliderShape.SymbolSansSerifBoldUppercase => "Sans-serif bold uppercase",
            ParametricSliderShape.SymbolSansSerifBoldLowercase => "Sans-serif bold lowercase",
            ParametricSliderShape.SymbolMonospaceUppercase => "Monospace uppercase",
            ParametricSliderShape.SymbolMonospaceLowercase => "Monospace lowercase",
            ParametricSliderShape.SymbolBlackboardLetters => "Blackboard letters",
            ParametricSliderShape.SymbolGreekUppercase => "Greek uppercase",
            ParametricSliderShape.SymbolGreekLowercase => "Greek lowercase",
            ParametricSliderShape.SymbolHebrewLetters => "Hebrew letters",
            ParametricSliderShape.SymbolNamedMathLetters => "Named math letters",
            ParametricSliderShape.SymbolCalculus => "Calculus",
            ParametricSliderShape.SymbolOperators => "Operators",
            ParametricSliderShape.SymbolRelations => "Relations",
            ParametricSliderShape.SymbolSetLogic => "Set / logic",
            ParametricSliderShape.SymbolArrows => "Arrows",
            ParametricSliderShape.SymbolDelimiters => "Delimiters",
            ParametricSliderShape.BlackboardN => "Blackboard N",
            ParametricSliderShape.BlackboardZ => "Blackboard Z",
            ParametricSliderShape.BlackboardQ => "Blackboard Q",
            ParametricSliderShape.BlackboardR => "Blackboard R",
            ParametricSliderShape.BlackboardC => "Blackboard C",
            ParametricSliderShape.InfinitySymbol => "Infinity",
            ParametricSliderShape.IntegralSymbol => "Integral",
            ParametricSliderShape.ContourIntegral => "Contour integral",
            ParametricSliderShape.SummationSigma => "Summation",
            ParametricSliderShape.ProductPi => "Product",
            ParametricSliderShape.PartialDerivative => "Partial derivative",
            ParametricSliderShape.ForAll => "For all",
            ParametricSliderShape.EmptySet => "Empty set",
            ParametricSliderShape.ElementOf => "Element of",
            ParametricSliderShape.LogicalAnd => "Logical and",
            ParametricSliderShape.LogicalOr => "Logical or",
            ParametricSliderShape.ApproximatelyEqual => "Approximately equal",
            ParametricSliderShape.ProportionalTo => "Proportional to",
            ParametricSliderShape.SineWave => "Sine wave",
            ParametricSliderShape.CosineWave => "Cosine wave",
            ParametricSliderShape.DampedSineWave => "Damped sine wave",
            ParametricSliderShape.SincCurve => "Sinc curve",
            ParametricSliderShape.GaussianFunction => "Gaussian function",
            ParametricSliderShape.GaussianMixture => "Gaussian mixture",
            ParametricSliderShape.LaplaceDistribution => "Laplace distribution",
            ParametricSliderShape.CauchyDistribution => "Cauchy distribution",
            ParametricSliderShape.RickerWavelet => "Ricker wavelet",
            ParametricSliderShape.BetaDistribution => "Beta distribution",
            ParametricSliderShape.GammaDistribution => "Gamma distribution",
            ParametricSliderShape.LogNormalDistribution => "Log-normal distribution",
            ParametricSliderShape.WeibullDistribution => "Weibull distribution",
            ParametricSliderShape.ExponentialDecay => "Exponential decay",
            ParametricSliderShape.LogisticSigmoid => "Logistic sigmoid",
            ParametricSliderShape.TanhCurve => "Tanh curve",
            ParametricSliderShape.LeakyReLU => "Leaky ReLU",
            ParametricSliderShape.CubicCurve => "Cubic curve",
            ParametricSliderShape.SemicubicalParabola => "Semicubical parabola",
            ParametricSliderShape.HeartCurve => "Heart curve",
            ParametricSliderShape.LissajousCurve => "Lissajous curve",
            ParametricSliderShape.RoseCurve => "Rose curve",
            ParametricSliderShape.Lemniscate => "Bernoulli lemniscate",
            ParametricSliderShape.GearCurve => "Gear curve",
            ParametricSliderShape.MaurerRose => "Maurer rose",
            ParametricSliderShape.ChebyshevCurve => "Chebyshev curve",
            ParametricSliderShape.ArchimedeanSpiral => "Archimedean spiral",
            ParametricSliderShape.LogarithmicSpiral => "Logarithmic spiral",
            ParametricSliderShape.HyperbolicSpiral => "Hyperbolic spiral",
            ParametricSliderShape.FermatSpiral => "Fermat spiral",
            ParametricSliderShape.EulerSpiral => "Euler spiral",
            ParametricSliderShape.ButterflyCurve => "Butterfly curve",
            ParametricSliderShape.CircleInvolute => "Circle involute",
            ParametricSliderShape.WitchOfAgnesi => "Witch of Agnesi",
            ParametricSliderShape.GoldenSpiral => "Golden spiral",
            ParametricSliderShape.LituusSpiral => "Lituus spiral",
            ParametricSliderShape.SpiralWave => "Spiral wave",
            ParametricSliderShape.KochCurve => "Koch curve",
            ParametricSliderShape.KochSnowflake => "Koch snowflake",
            ParametricSliderShape.DragonCurve => "Dragon curve",
            ParametricSliderShape.SierpinskiArrowhead => "Sierpinski arrowhead",
            ParametricSliderShape.HilbertCurve => "Hilbert curve",
            ParametricSliderShape.GosperCurve => "Gosper curve",
            ParametricSliderShape.LevyCurve => "Levy C curve",
            ParametricSliderShape.MengerCarpet => "Menger carpet",
            ParametricSliderShape.BarnsleyFern => "Barnsley fern",
            ParametricSliderShape.MandelbrotSet => "Mandelbrot set",
            ParametricSliderShape.JuliaSet => "Julia set",
            ParametricSliderShape.BurningShip => "Burning Ship",
            ParametricSliderShape.NewtonFractal => "Newton fractal",
            ParametricSliderShape.PeanoCurve => "Peano curve",
            ParametricSliderShape.MooreCurve => "Moore curve",
            ParametricSliderShape.MinkowskiSausage => "Minkowski sausage",
            ParametricSliderShape.MinkowskiIsland => "Minkowski island",
            ParametricSliderShape.TerdragonCurve => "Terdragon curve",
            ParametricSliderShape.CesaroCurve => "Cesaro curve",
            ParametricSliderShape.FractalTree => "Fractal tree",
            ParametricSliderShape.BinaryTree => "Binary tree",
            ParametricSliderShape.RegularPolyhedronProjection => "Regular polyhedron",
            ParametricSliderShape.TorusKnotProjection => "Torus knot",
            ParametricSliderShape.SphericalHelixProjection => "Spherical helix",
            _ => sliderShape.ToString()
        };

        private static string getCategoryName(ParametricSliderShapeCategory category) => category switch
        {
            ParametricSliderShapeCategory.Basic => "BASIC",
            ParametricSliderShapeCategory.Symbols => "SYMBOLS",
            ParametricSliderShapeCategory.Waves => "WAVES",
            ParametricSliderShapeCategory.Functions => "FUNCTIONS",
            ParametricSliderShapeCategory.Parametric => "PARAMETRIC",
            ParametricSliderShapeCategory.Spirals => "SPIRALS",
            ParametricSliderShapeCategory.Fractals => "FRACTALS",
            ParametricSliderShapeCategory.Projections3D => "3D",
            _ => category.ToString().ToUpperInvariant()
        };

        private static T parseEnum<T>(string value, T fallback)
            where T : struct, Enum =>
            Enum.TryParse(value, out T parsed) ? parsed : fallback;

    }
}
