// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
    public partial class NoiseSliderToolboxPopover : EditorPreviewPopover
    {
        private const int preview_max_samples = 80;
        private readonly Bindable<SliderToolbox2Category> category = new Bindable<SliderToolbox2Category>(SliderToolbox2Category.VectorFlow);
        private readonly Bindable<SliderToolbox2Type> toolboxType = new Bindable<SliderToolbox2Type>(SliderToolbox2Type.NoiseSampledPath);
        private readonly Bindable<ParametricSliderHeadPathType> headControlPointType = new Bindable<ParametricSliderHeadPathType>(ParametricSliderHeadPathType.Linear);
        private readonly Bindable<NoiseSliderSeedPattern> seedPattern = new Bindable<NoiseSliderSeedPattern>(NoiseSliderSeedPattern.Centre);
        private readonly Bindable<NoiseSliderNoiseType> noiseType = new Bindable<NoiseSliderNoiseType>(NoiseSliderNoiseType.FractalValue);
        private readonly Bindable<ParametricSliderAnimationEasing> easing = new Bindable<ParametricSliderAnimationEasing>();
        private readonly List<SingleFrameInactiveContainer> singleFrameInactiveControls = new List<SingleFrameInactiveContainer>();
        private readonly List<Slider> previewSliders = new List<Slider>();

        private FormSliderBar<double> startSeedPosition = null!;
        private FormSliderBar<double> endSeedPosition = null!;
        private FormSliderBar<double> startSeedAngle = null!;
        private FormSliderBar<double> endSeedAngle = null!;
        private FormSliderBar<double> startFieldBias = null!;
        private FormSliderBar<double> endFieldBias = null!;
        private FormSliderBar<double> startStreamlineLength = null!;
        private FormSliderBar<double> endStreamlineLength = null!;
        private FormSliderBar<int> startSamples = null!;
        private FormSliderBar<int> endSamples = null!;
        private FormSliderBar<double> startWidth = null!;
        private FormSliderBar<double> endWidth = null!;
        private FormSliderBar<double> startHeight = null!;
        private FormSliderBar<double> endHeight = null!;
        private FormSliderBar<double> startAmplitude = null!;
        private FormSliderBar<double> endAmplitude = null!;
        private FormSliderBar<double> startFrequency = null!;
        private FormSliderBar<double> endFrequency = null!;
        private FormSliderBar<int> startOctaves = null!;
        private FormSliderBar<int> endOctaves = null!;
        private FormSliderBar<double> startPersistence = null!;
        private FormSliderBar<double> endPersistence = null!;
        private FormSliderBar<int> startSmoothing = null!;
        private FormSliderBar<int> endSmoothing = null!;
        private FormSliderBar<int> startSeed = null!;
        private FormSliderBar<int> endSeed = null!;
        private FormSliderBar<double> startScale = null!;
        private FormSliderBar<double> endScale = null!;
        private FormSliderBar<int> startRotationTurns = null!;
        private FormSliderBar<int> endRotationTurns = null!;
        private FormSliderBar<double> startRotationDegrees = null!;
        private FormSliderBar<double> endRotationDegrees = null!;
        private FormSliderBar<double> startOffsetX = null!;
        private FormSliderBar<double> endOffsetX = null!;
        private FormSliderBar<double> startOffsetY = null!;
        private FormSliderBar<double> endOffsetY = null!;
        private FormSliderBar<int> framesInput = null!;
        private FormSliderBar<int> frameDurationInput = null!;
        private FormSliderBar<double> frameGapInput = null!;
        private FormSliderBar<double> panelOpacityInput = null!;
        private FormEnumDropdown<SliderToolbox2Type> typeDropdown = null!;
        private Drawable noiseOptionsRow = null!;
        private Container noisePreviewDrawables = null!;
        private OsuSpriteText previewText = null!;
        private EditorToolStatus resultText = null!;
        private RoundedButton createButton = null!;
        private EditorToolTimeAnchor insertionTime = null!;

        private Bindable<string> categoryConfig = null!;
        private Bindable<string> typeConfig = null!;
        private Bindable<string> headControlPointTypeConfig = null!;
        private Bindable<string> seedPatternConfig = null!;
        private Bindable<string> noiseTypeConfig = null!;
        private Bindable<string> easingConfig = null!;
        private Bindable<double> startSeedPositionConfig = null!;
        private Bindable<double> endSeedPositionConfig = null!;
        private Bindable<double> startSeedAngleConfig = null!;
        private Bindable<double> endSeedAngleConfig = null!;
        private Bindable<double> startFieldBiasConfig = null!;
        private Bindable<double> endFieldBiasConfig = null!;
        private Bindable<double> startStreamlineLengthConfig = null!;
        private Bindable<double> endStreamlineLengthConfig = null!;
        private Bindable<float> startSamplesConfig = null!;
        private Bindable<float> endSamplesConfig = null!;
        private Bindable<double> startWidthConfig = null!;
        private Bindable<double> endWidthConfig = null!;
        private Bindable<double> startHeightConfig = null!;
        private Bindable<double> endHeightConfig = null!;
        private Bindable<double> startAmplitudeConfig = null!;
        private Bindable<double> endAmplitudeConfig = null!;
        private Bindable<double> startFrequencyConfig = null!;
        private Bindable<double> endFrequencyConfig = null!;
        private Bindable<float> startOctavesConfig = null!;
        private Bindable<float> endOctavesConfig = null!;
        private Bindable<double> startPersistenceConfig = null!;
        private Bindable<double> endPersistenceConfig = null!;
        private Bindable<float> startSmoothingConfig = null!;
        private Bindable<float> endSmoothingConfig = null!;
        private Bindable<float> startSeedConfig = null!;
        private Bindable<float> endSeedConfig = null!;
        private Bindable<double> startScaleConfig = null!;
        private Bindable<double> endScaleConfig = null!;
        private Bindable<float> startRotationTurnsConfig = null!;
        private Bindable<float> endRotationTurnsConfig = null!;
        private Bindable<double> startRotationDegreesConfig = null!;
        private Bindable<double> endRotationDegreesConfig = null!;
        private Bindable<double> startOffsetXConfig = null!;
        private Bindable<double> endOffsetXConfig = null!;
        private Bindable<double> startOffsetYConfig = null!;
        private Bindable<double> endOffsetYConfig = null!;
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

        public NoiseSliderToolboxPopover()
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

            var startParameterColumn = createFrameParameterColumn(
                "Start frame",
                startSeedPosition,
                startSeedAngle,
                startFieldBias,
                startStreamlineLength,
                startSamples,
                startWidth,
                startHeight,
                startAmplitude,
                startFrequency,
                startOctaves,
                startPersistence,
                startSmoothing,
                startSeed,
                startScale,
                startOffsetX,
                startOffsetY,
                startRotationTurns,
                startRotationDegrees);
            startParameterColumn.Width = 0.47f;

            var endParameterColumn = SliderToolboxUiUtils.CreateSingleFrameInactiveControl(createFrameParameterColumn(
                "End frame",
                endSeedPosition,
                endSeedAngle,
                endFieldBias,
                endStreamlineLength,
                endSamples,
                endWidth,
                endHeight,
                endAmplitude,
                endFrequency,
                endOctaves,
                endPersistence,
                endSmoothing,
                endSeed,
                endScale,
                endOffsetX,
                endOffsetY,
                endRotationTurns,
                endRotationDegrees), 0.47f);
            singleFrameInactiveControls.Add(endParameterColumn);

            var parameterRows = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Full,
                Spacing = new Vector2(6, 0),
                Children = new Drawable[]
                {
                    startParameterColumn,
                    endParameterColumn
                }
            };
            var motionCurveControl = SliderToolboxUiUtils.CreateSingleFrameInactiveControl(new FormEnumDropdown<ParametricSliderAnimationEasing>
            {
                Width = 1,
                Caption = "Curve",
                Current = easing
            }, 0.2f);
            singleFrameInactiveControls.Add(motionCurveControl);
            var frameGapControl = SliderToolboxUiUtils.CreateSingleFrameInactiveControl(frameGapInput = new FormSliderBar<double>
            {
                Width = 1,
                Caption = "Gap",
                Current = new BindableDouble(frameGapConfig.Value)
                {
                    MinValue = 0,
                    MaxValue = 64,
                    Precision = 1
                },
                KeyboardStep = 1,
                LabelFormat = value => SliderToolboxUiUtils.FormatSnapCount(value),
                TooltipFormat = value => SliderToolboxUiUtils.FormatSnapCount(value),
                TabbableContentContainer = this
            }, 0.17f);
            singleFrameInactiveControls.Add(frameGapControl);

            Child = new FillFlowContainer
            {
                Width = 800,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(3),
                Padding = new MarginPadding(5),
                Children = new Drawable[]
                {
                    insertionTime = new EditorToolTimeAnchor(),
                    new EditorToolControlRow
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            new FormEnumDropdown<SliderToolbox2Category>
                            {
                                Width = 0.18f,
                                Caption = "Category",
                                Current = category
                            },
                            typeDropdown = new FormEnumDropdown<SliderToolbox2Type>
                            {
                                Width = 0.25f,
                                Caption = "Type",
                                Current = toolboxType
                            },
                            new FormEnumDropdown<ParametricSliderHeadPathType>
                            {
                                Width = 0.22f,
                                Caption = "Head",
                                Current = headControlPointType
                            },
                            motionCurveControl,
                            panelOpacityInput = new FormSliderBar<double>
                            {
                                Width = 0.13f,
                                Caption = "Opacity",
                                Current = config.GetBindable<double>(OsuSetting.EditorParametricSliderPanelOpacity),
                                KeyboardStep = 0.05f,
                                LabelFormat = value => $"{value:P0}",
                                TooltipFormat = value => $"{value:P0}",
                                TabbableContentContainer = this
                            }
                        }
                    },
                    noiseOptionsRow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Spacing = new Vector2(6),
                        Children = new Drawable[]
                        {
                            new FormEnumDropdown<NoiseSliderSeedPattern>
                            {
                                Width = 0.49f,
                                Caption = "Seed pattern",
                                Current = seedPattern
                            },
                            new FormEnumDropdown<NoiseSliderNoiseType>
                            {
                                Width = 0.49f,
                                Caption = "Field",
                                Current = noiseType
                            }
                        }
                    },
                    new EditorToolSplitPane(230, 0.35f,
                            createNoisePreviewPane(colours),
                            new Container
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.64f,
                                Child = new OsuScrollContainer(Direction.Vertical)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = parameterRows
                                }
                            }),
                    new EditorToolControlRow
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            framesInput = new FormSliderBar<int>
                            {
                                Width = 0.17f,
                                Caption = "Frames",
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
                                Width = 0.17f,
                                Caption = "Duration",
                                Current = new BindableInt((int)Math.Round(frameDurationConfig.Value))
                                {
                                    MinValue = 1,
                                    MaxValue = 64
                                },
                                KeyboardStep = 1,
                                LabelFormat = value => SliderToolboxUiUtils.FormatSnapCount(value),
                                TooltipFormat = value => SliderToolboxUiUtils.FormatSnapCount(value),
                                TabbableContentContainer = this
                            },
                            frameGapControl,
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.34f,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                                Spacing = new Vector2(6),
                                Children = new Drawable[]
                                {
                                    new RoundedButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.46f,
                                        Height = 30,
                                        Text = "Swap",
                                        TooltipText = "Swap all start and end parameters.",
                                        Action = swapStartEndParameters
                                    },
                                    new RoundedButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.46f,
                                        Height = 30,
                                        Text = "Copy > End",
                                        TooltipText = "Copy all start parameters to end parameters.",
                                        Action = copyStartParametersToEnd
                                    }
                                }
                            }
                        }
                    },
                    resultText = new EditorToolStatus { Text = "Preparing sliders..." },
                    new EditorToolActionRow(createButton = new RoundedButton { Action = commit }, Hide)
                }
            };

        }

        private Drawable createNoisePreviewPane(OsuColour colours) => new Container
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            RelativeSizeAxes = Axes.Both,
            Width = 0.34f,
            Masking = true,
            CornerRadius = 6,
            BorderThickness = 2,
            BorderColour = colours.GreySeaFoam,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.35f
                },
                noisePreviewDrawables = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                previewText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -8,
                    Font = OsuFont.Default.With(size: 12),
                    Colour = colours.GrayF,
                    Text = "field / source preview"
                }
            }
        };

        private FillFlowContainer createFrameParameterColumn(string title, params Drawable[] controls)
        {
            var children = new List<Drawable>
            {
                createSectionHeader(title),
                createSectionHeader("Source / field")
            };

            for (int i = 0; i < controls.Length; i++)
            {
                if (i == 13)
                    children.Add(createSectionHeader("Placement"));

                controls[i].RelativeSizeAxes = Axes.X;
                controls[i].Width = 1;
                children.Add(controls[i]);
            }

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 3),
                Children = children.ToArray()
            };
        }

        private Drawable createSectionHeader(string text) => new OsuSpriteText
        {
            RelativeSizeAxes = Axes.X,
            Font = OsuFont.Default.With(size: 12),
            Text = text,
            Alpha = 0.72f
        };

        private void loadConfig(OsuConfigManager config)
        {
            categoryConfig = config.GetBindable<string>(OsuSetting.EditorNoiseSliderCategory);
            typeConfig = config.GetBindable<string>(OsuSetting.EditorNoiseSliderType);
            headControlPointTypeConfig = config.GetBindable<string>(OsuSetting.EditorNoiseSliderHeadControlPointType);
            seedPatternConfig = config.GetBindable<string>(OsuSetting.EditorNoiseSliderSeedPattern);
            noiseTypeConfig = config.GetBindable<string>(OsuSetting.EditorNoiseSliderNoiseType);
            easingConfig = config.GetBindable<string>(OsuSetting.EditorNoiseSliderAnimationEasing);
            startSeedPositionConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderSeedPosition);
            endSeedPositionConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndSeedPosition);
            startSeedAngleConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderSeedAngle);
            endSeedAngleConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndSeedAngle);
            startFieldBiasConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderFieldBias);
            endFieldBiasConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndFieldBias);
            startStreamlineLengthConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderStreamlineLength);
            endStreamlineLengthConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndStreamlineLength);
            startSamplesConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderSamples);
            endSamplesConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderAnimationEndSamples);
            startWidthConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderWidth);
            endWidthConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndWidth);
            startHeightConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderHeight);
            endHeightConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndHeight);
            startAmplitudeConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAmplitude);
            endAmplitudeConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndAmplitude);
            startFrequencyConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderFrequency);
            endFrequencyConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndFrequency);
            startOctavesConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderOctaves);
            endOctavesConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderAnimationEndOctaves);
            startPersistenceConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderPersistence);
            endPersistenceConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndPersistence);
            startSmoothingConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderSmoothing);
            endSmoothingConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderAnimationEndSmoothing);
            startSeedConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderSeed);
            endSeedConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderAnimationEndSeed);
            startScaleConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderScale);
            endScaleConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndScale);
            startRotationTurnsConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderRotationTurns);
            endRotationTurnsConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderAnimationEndRotationTurns);
            startRotationDegreesConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderRotationDegrees);
            endRotationDegreesConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndRotationDegrees);
            startOffsetXConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderOffsetX);
            endOffsetXConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndOffsetX);
            startOffsetYConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderOffsetY);
            endOffsetYConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderAnimationEndOffsetY);
            framesConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderAnimationFrames);
            frameDurationConfig = config.GetBindable<float>(OsuSetting.EditorNoiseSliderFrameDurationSnaps);
            frameGapConfig = config.GetBindable<double>(OsuSetting.EditorNoiseSliderFrameGapSnaps);

            toolboxType.Value = parseEnum(typeConfig.Value, SliderToolbox2Type.NoiseSampledPath);
            category.Value = parseEnum(categoryConfig.Value, NoiseSliderGenerator.GetCategory(toolboxType.Value));
            headControlPointType.Value = parseEnum(headControlPointTypeConfig.Value, ParametricSliderHeadPathType.Linear);
            seedPattern.Value = parseEnum(seedPatternConfig.Value, NoiseSliderSeedPattern.Centre);
            noiseType.Value = parseEnum(noiseTypeConfig.Value, NoiseSliderNoiseType.FractalValue);
            easing.Value = parseEnum(easingConfig.Value, ParametricSliderAnimationEasing.EaseInOut);
        }

        private void initialiseParameterControls()
        {
            startSeedPosition = createDoubleInput("Seed position", startSeedPositionConfig.Value, 0, 1, 0.001, 0.02f, "0.000");
            endSeedPosition = createDoubleInput("Seed position", endSeedPositionConfig.Value, 0, 1, 0.001, 0.02f, "0.000");
            startSeedAngle = createDoubleInput("Seed angle", startSeedAngleConfig.Value, -180, 180, 1, 5, "0");
            endSeedAngle = createDoubleInput("Seed angle", endSeedAngleConfig.Value, -180, 180, 1, 5, "0");
            startFieldBias = createDoubleInput("Field bias", startFieldBiasConfig.Value, -1, 1, 0.01, 0.05f, "0.00");
            endFieldBias = createDoubleInput("Field bias", endFieldBiasConfig.Value, -1, 1, 0.01, 0.05f, "0.00");
            startStreamlineLength = createDoubleInput("Source length", startStreamlineLengthConfig.Value, 16, 1024, 1, 8, "0");
            endStreamlineLength = createDoubleInput("Source length", endStreamlineLengthConfig.Value, 16, 1024, 1, 8, "0");
            startSamples = createIntInput("Samples", startSamplesConfig.Value, 8, 1024, 4);
            endSamples = createIntInput("Samples", endSamplesConfig.Value, 8, 1024, 4);
            startWidth = createDoubleInput("Field width", startWidthConfig.Value, 16, 512, 1, 8, "0");
            endWidth = createDoubleInput("Field width", endWidthConfig.Value, 16, 512, 1, 8, "0");
            startHeight = createDoubleInput("Field height", startHeightConfig.Value, 16, 512, 1, 8, "0");
            endHeight = createDoubleInput("Field height", endHeightConfig.Value, 16, 512, 1, 8, "0");
            startAmplitude = createDoubleInput("Amplitude", startAmplitudeConfig.Value, 0, 256, 1, 4, "0");
            endAmplitude = createDoubleInput("Amplitude", endAmplitudeConfig.Value, 0, 256, 1, 4, "0");
            startFrequency = createDoubleInput("Frequency", startFrequencyConfig.Value, 0.1, 16, 0.1, 0.25f, "0.0");
            endFrequency = createDoubleInput("Frequency", endFrequencyConfig.Value, 0.1, 16, 0.1, 0.25f, "0.0");
            startOctaves = createIntInput("Octaves", startOctavesConfig.Value, 1, 8);
            endOctaves = createIntInput("Octaves", endOctavesConfig.Value, 1, 8);
            startPersistence = createDoubleInput("Persistence", startPersistenceConfig.Value, 0.1, 0.95, 0.01, 0.05f, "0.00");
            endPersistence = createDoubleInput("Persistence", endPersistenceConfig.Value, 0.1, 0.95, 0.01, 0.05f, "0.00");
            startSmoothing = createIntInput("Smooth", startSmoothingConfig.Value, 0, 16);
            endSmoothing = createIntInput("Smooth", endSmoothingConfig.Value, 0, 16);
            startSeed = createIntInput("Seed", startSeedConfig.Value, -999999, 999999);
            endSeed = createIntInput("Seed", endSeedConfig.Value, -999999, 999999);
            startScale = createDoubleInput("Scale", startScaleConfig.Value, 0.1, 4, 0.01, 0.05f, "0.00");
            endScale = createDoubleInput("Scale", endScaleConfig.Value, 0.1, 4, 0.01, 0.05f, "0.00");
            startOffsetX = createDoubleInput("Offset X", startOffsetXConfig.Value, -512, 512, 1, 8, "0");
            endOffsetX = createDoubleInput("Offset X", endOffsetXConfig.Value, -512, 512, 1, 8, "0");
            startOffsetY = createDoubleInput("Offset Y", startOffsetYConfig.Value, -384, 384, 1, 8, "0");
            endOffsetY = createDoubleInput("Offset Y", endOffsetYConfig.Value, -384, 384, 1, 8, "0");
            startRotationTurns = createIntInput("Rotation turns", startRotationTurnsConfig.Value, -32, 32);
            endRotationTurns = createIntInput("Rotation turns", endRotationTurnsConfig.Value, -32, 32);
            startRotationDegrees = createDoubleInput("Rotation degrees", startRotationDegreesConfig.Value, -359, 359, 1, 5, "0");
            endRotationDegrees = createDoubleInput("Rotation degrees", endRotationDegreesConfig.Value, -359, 359, 1, 5, "0");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            insertionTime.Current.BindValueChanged(_ => schedulePreview());
            category.BindValueChanged(value =>
            {
                updateTypeDropdownItems();
                updateParameterAvailability();
                schedulePreview();
            });
            toolboxType.BindValueChanged(value =>
            {
                SliderToolbox2Category typeCategory = NoiseSliderGenerator.GetCategory(value.NewValue);

                if (category.Value != typeCategory)
                    category.Value = typeCategory;

                updateParameterAvailability();
                schedulePreview();
            });
            seedPattern.BindValueChanged(_ => schedulePreview());
            noiseType.BindValueChanged(_ => schedulePreview());
            headControlPointType.BindValueChanged(_ => schedulePreview());
            easing.BindValueChanged(_ => schedulePreview());
            framesInput.Current.BindValueChanged(_ =>
            {
                updateFrameCountMode();
                schedulePreview();
            });

            bindPreview(startSamples, endSamples, startOctaves, endOctaves, startSmoothing, endSmoothing,
                startSeed, endSeed, startRotationTurns, endRotationTurns, frameDurationInput);
            bindPreview(startSeedPosition, endSeedPosition, startSeedAngle, endSeedAngle, startFieldBias, endFieldBias, startStreamlineLength, endStreamlineLength,
                startWidth, endWidth, startHeight, endHeight, startAmplitude, endAmplitude,
                startFrequency, endFrequency, startPersistence, endPersistence, startScale, endScale,
                startOffsetX, endOffsetX, startOffsetY, endOffsetY, startRotationDegrees, endRotationDegrees, frameGapInput);
            beatDivisor = (BindableBeatDivisor)beatDivisor.GetBoundCopy();
            beatDivisor.BindValueChanged(_ => schedulePreview());
            panelOpacityInput.Current.BindValueChanged(value => Body.FadeTo((float)value.NewValue, 80, Easing.OutQuint), true);
            newComboState.BindValueChanged(_ => schedulePreview());

            ScheduleAfterChildren(() =>
            {
                updateTypeDropdownItems();
                updateFrameCountMode();
                updateParameterAvailability();
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

        private void updateParameterAvailability()
        {
            SliderToolbox2Category activeCategory = NoiseSliderGenerator.GetCategory(toolboxType.Value);
            bool fieldFamily = activeCategory == SliderToolbox2Category.VectorFlow;
            setVisible(noiseOptionsRow, fieldFamily);

            startSeedPosition.Caption = endSeedPosition.Caption = "Seed position";
            startSeedAngle.Caption = endSeedAngle.Caption = "Seed angle";
            startFieldBias.Caption = endFieldBias.Caption = "Field bias";
            startStreamlineLength.Caption = endStreamlineLength.Caption = "Source length";
            startWidth.Caption = endWidth.Caption = "Source width";
            startHeight.Caption = endHeight.Caption = "Source height";
            startAmplitude.Caption = endAmplitude.Caption = "Amplitude";
            startFrequency.Caption = endFrequency.Caption = "Frequency";
            startOctaves.Caption = endOctaves.Caption = "Octaves";
            startPersistence.Caption = endPersistence.Caption = "Persistence";
            startSmoothing.Caption = endSmoothing.Caption = "Smooth";

            switch (activeCategory)
            {
                case SliderToolbox2Category.VectorFlow:
                    startSeedPosition.Caption = endSeedPosition.Caption = "Seed position";
                    startSeedAngle.Caption = endSeedAngle.Caption = "Bias angle";
                    startFieldBias.Caption = endFieldBias.Caption = "Direction bias";
                    startStreamlineLength.Caption = endStreamlineLength.Caption = "Flow length";
                    startAmplitude.Caption = endAmplitude.Caption = "Field pull";
                    startFrequency.Caption = endFrequency.Caption = "Field scale";
                    startOctaves.Caption = endOctaves.Caption = "Field detail";
                    startPersistence.Caption = endPersistence.Caption = "Drift";
                    break;

                case SliderToolbox2Category.ImplicitContours:
                    startSeedPosition.Caption = endSeedPosition.Caption = "Phase";
                    startSeedAngle.Caption = endSeedAngle.Caption = "Rotation";
                    startFieldBias.Caption = endFieldBias.Caption = "Contour skew";
                    startStreamlineLength.Caption = endStreamlineLength.Caption = "Path length";
                    startWidth.Caption = endWidth.Caption = "Shape width";
                    startHeight.Caption = endHeight.Caption = "Shape height";
                    startAmplitude.Caption = endAmplitude.Caption = "Contour force";
                    startFrequency.Caption = endFrequency.Caption = "Lobes";
                    startOctaves.Caption = endOctaves.Caption = "Layers";
                    break;

                case SliderToolbox2Category.GraphPaths:
                    startSeedPosition.Caption = endSeedPosition.Caption = "Route offset";
                    startSeedAngle.Caption = endSeedAngle.Caption = "Path rotation";
                    startFieldBias.Caption = endFieldBias.Caption = "Route bend";
                    startStreamlineLength.Caption = endStreamlineLength.Caption = "Path length";
                    startWidth.Caption = endWidth.Caption = "Grid width";
                    startHeight.Caption = endHeight.Caption = "Grid height";
                    startFrequency.Caption = endFrequency.Caption = "Count / density";
                    startOctaves.Caption = endOctaves.Caption = "Order";
                    startPersistence.Caption = endPersistence.Caption = "Branching";
                    break;

                case SliderToolbox2Category.Attractors:
                    startSeedPosition.Caption = endSeedPosition.Caption = "Warm offset";
                    startSeedAngle.Caption = endSeedAngle.Caption = "Rotation";
                    startFieldBias.Caption = endFieldBias.Caption = "Projection";
                    startStreamlineLength.Caption = endStreamlineLength.Caption = "Path length";
                    startWidth.Caption = endWidth.Caption = "Output width";
                    startHeight.Caption = endHeight.Caption = "Output height";
                    startAmplitude.Caption = endAmplitude.Caption = "Time step";
                    startFrequency.Caption = endFrequency.Caption = "Parameter";
                    startPersistence.Caption = endPersistence.Caption = "Parameter B";
                    break;

                case SliderToolbox2Category.StrokeGrammar:
                    startSeedPosition.Caption = endSeedPosition.Caption = "Phase";
                    startSeedAngle.Caption = endSeedAngle.Caption = "Rotation";
                    startFieldBias.Caption = endFieldBias.Caption = "Stroke slant";
                    startStreamlineLength.Caption = endStreamlineLength.Caption = "Path length";
                    startWidth.Caption = endWidth.Caption = "Stroke width";
                    startHeight.Caption = endHeight.Caption = "Stroke height";
                    startAmplitude.Caption = endAmplitude.Caption = "Height";
                    startFrequency.Caption = endFrequency.Caption = "Repeats";
                    startPersistence.Caption = endPersistence.Caption = "Roundness";
                    break;

                case SliderToolbox2Category.MaskTracing:
                    startSeedPosition.Caption = endSeedPosition.Caption = "Contour phase";
                    startSeedAngle.Caption = endSeedAngle.Caption = "Rotation";
                    startFieldBias.Caption = endFieldBias.Caption = "Mask skew";
                    startStreamlineLength.Caption = endStreamlineLength.Caption = "Path length";
                    startWidth.Caption = endWidth.Caption = "Mask width";
                    startHeight.Caption = endHeight.Caption = "Mask height";
                    startAmplitude.Caption = endAmplitude.Caption = "Mask force";
                    startFrequency.Caption = endFrequency.Caption = "Mask scale";
                    startOctaves.Caption = endOctaves.Caption = "Mask detail";
                    startPersistence.Caption = endPersistence.Caption = "Threshold";
                    break;

                case SliderToolbox2Category.BeatDriven:
                    startSeedPosition.Caption = endSeedPosition.Caption = "Phase";
                    startSeedAngle.Caption = endSeedAngle.Caption = "Rotation";
                    startFieldBias.Caption = endFieldBias.Caption = "Wave tilt";
                    startStreamlineLength.Caption = endStreamlineLength.Caption = "Path length";
                    startWidth.Caption = endWidth.Caption = "Wave width";
                    startHeight.Caption = endHeight.Caption = "Wave height";
                    startAmplitude.Caption = endAmplitude.Caption = "Pulse height";
                    startFrequency.Caption = endFrequency.Caption = "Pulses";
                    startPersistence.Caption = endPersistence.Caption = "Decay";
                    break;
            }
        }

        private void updateTypeDropdownItems()
        {
            SliderToolbox2Type[] categoryTypes = Enum.GetValues<SliderToolbox2Type>()
                                                     .Where(type => NoiseSliderGenerator.GetCategory(type) == category.Value)
                                                     .ToArray();

            if (categoryTypes.Length == 0)
                categoryTypes = new[] { NoiseSliderGenerator.GetDefaultType(category.Value) };

            typeDropdown.Items = categoryTypes;

            if (!categoryTypes.Contains(toolboxType.Value))
                toolboxType.Value = categoryTypes[0];
        }

        private static void setVisible(Drawable drawable, bool visible)
        {
            if (visible)
                drawable.Show();
            else
                drawable.Hide();
        }

        private void updateFrameCountMode()
        {
            bool singleFrame = framesInput.Current.Value == 1;

            foreach (SingleFrameInactiveContainer control in singleFrameInactiveControls)
                control.Inactive = singleFrame;
        }

        private void swapStartEndParameters()
        {
            SliderToolboxUiUtils.Swap(startSeedPosition, endSeedPosition);
            SliderToolboxUiUtils.Swap(startSeedAngle, endSeedAngle);
            SliderToolboxUiUtils.Swap(startFieldBias, endFieldBias);
            SliderToolboxUiUtils.Swap(startStreamlineLength, endStreamlineLength);
            SliderToolboxUiUtils.Swap(startSamples, endSamples);
            SliderToolboxUiUtils.Swap(startWidth, endWidth);
            SliderToolboxUiUtils.Swap(startHeight, endHeight);
            SliderToolboxUiUtils.Swap(startAmplitude, endAmplitude);
            SliderToolboxUiUtils.Swap(startFrequency, endFrequency);
            SliderToolboxUiUtils.Swap(startOctaves, endOctaves);
            SliderToolboxUiUtils.Swap(startPersistence, endPersistence);
            SliderToolboxUiUtils.Swap(startSmoothing, endSmoothing);
            SliderToolboxUiUtils.Swap(startSeed, endSeed);
            SliderToolboxUiUtils.Swap(startScale, endScale);
            SliderToolboxUiUtils.Swap(startOffsetX, endOffsetX);
            SliderToolboxUiUtils.Swap(startOffsetY, endOffsetY);
            SliderToolboxUiUtils.Swap(startRotationTurns, endRotationTurns);
            SliderToolboxUiUtils.Swap(startRotationDegrees, endRotationDegrees);
            schedulePreview();
        }

        private void copyStartParametersToEnd()
        {
            SliderToolboxUiUtils.Copy(startSeedPosition, endSeedPosition);
            SliderToolboxUiUtils.Copy(startSeedAngle, endSeedAngle);
            SliderToolboxUiUtils.Copy(startFieldBias, endFieldBias);
            SliderToolboxUiUtils.Copy(startStreamlineLength, endStreamlineLength);
            SliderToolboxUiUtils.Copy(startSamples, endSamples);
            SliderToolboxUiUtils.Copy(startWidth, endWidth);
            SliderToolboxUiUtils.Copy(startHeight, endHeight);
            SliderToolboxUiUtils.Copy(startAmplitude, endAmplitude);
            SliderToolboxUiUtils.Copy(startFrequency, endFrequency);
            SliderToolboxUiUtils.Copy(startOctaves, endOctaves);
            SliderToolboxUiUtils.Copy(startPersistence, endPersistence);
            SliderToolboxUiUtils.Copy(startSmoothing, endSmoothing);
            SliderToolboxUiUtils.Copy(startSeed, endSeed);
            SliderToolboxUiUtils.Copy(startScale, endScale);
            SliderToolboxUiUtils.Copy(startOffsetX, endOffsetX);
            SliderToolboxUiUtils.Copy(startOffsetY, endOffsetY);
            SliderToolboxUiUtils.Copy(startRotationTurns, endRotationTurns);
            SliderToolboxUiUtils.Copy(startRotationDegrees, endRotationDegrees);
            schedulePreview();
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

            int frameCount = framesInput.Current.Value;
            double sequenceStartTime = beatSnapProvider.SnapTime(insertionTime.Current.Value);
            double durationBeats = frameDurationInput.Current.Value / (double)beatSnapProvider.BeatDivisor;
            double gapBeats = frameGapInput.Current.Value / beatSnapProvider.BeatDivisor;
            double frameTime = sequenceStartTime;
            double sequenceEndTime = sequenceStartTime;

            if (!double.IsFinite(durationBeats) || durationBeats <= 0 || !double.IsFinite(gapBeats) || gapBeats < 0)
            {
                showError("Frame duration must be positive and frame gap cannot be negative.");
                return;
            }

            updateNoisePreview(frameCount);

            var generatedSliders = new List<Slider>();

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                double rawProgress = frameCount == 1 ? 0 : (double)frameIndex / (frameCount - 1);
                double progress = SliderToolboxUiUtils.ApplyEasing(rawProgress, easing.Value);
                double startTime = frameTime;
                sequenceEndTime = BeatTimeline.TimeAfterBeats(editorBeatmap.ControlPointInfo, startTime, durationBeats);
                double targetDuration = sequenceEndTime - startTime;
                frameTime = BeatTimeline.TimeAfterBeats(editorBeatmap.ControlPointInfo, sequenceEndTime, gapBeats);
                NoiseSliderFrame frame = buildFrame(progress);
                IReadOnlyList<Vector2> generatedPath = NoiseSliderGenerator.GeneratePath(frame.Parameters);
                Slider? slider = createSlider(generatedPath, frame, startTime, targetDuration, frameIndex == 0);

                if (slider == null)
                    return;

                generatedSliders.Add(slider);
            }

            TernaryState previousNewComboState = newComboState.Value;
            ReplacePreviewObjects(previewSliders, generatedSliders);
            newComboState.Value = previousNewComboState;

            double totalDuration = sequenceEndTime - sequenceStartTime;
            resultText.Text = frameCount == 1
                ? $"{generatedSliders.Count} sliders | 1 frame | {SliderToolboxUiUtils.FormatSnapCount(frameDurationInput.Current.Value)} ({totalDuration:0.##} ms)"
                : $"{generatedSliders.Count} sliders | {frameCount} frames | {SliderToolboxUiUtils.FormatSnapCount(frameDurationInput.Current.Value)} each | {SliderToolboxUiUtils.FormatSnapCount(frameGapInput.Current.Value)} gap | {totalDuration:0.##} ms total";
            createButton.Enabled.Value = true;
        }

        private NoiseSliderFrame buildFrame(double progress)
        {
            var parameters = new NoiseSliderParameters(
                seedPattern.Value,
                noiseType.Value,
                toolboxType.Value,
                SliderToolboxUiUtils.Interpolate(startSeedPosition, endSeedPosition, progress),
                SliderToolboxUiUtils.Interpolate(startSeedAngle, endSeedAngle, progress),
                SliderToolboxUiUtils.Interpolate(startFieldBias, endFieldBias, progress),
                SliderToolboxUiUtils.Interpolate(startStreamlineLength, endStreamlineLength, progress),
                SliderToolboxUiUtils.InterpolateInt(startSamples, endSamples, progress),
                SliderToolboxUiUtils.Interpolate(startWidth, endWidth, progress),
                SliderToolboxUiUtils.Interpolate(startHeight, endHeight, progress),
                SliderToolboxUiUtils.Interpolate(startAmplitude, endAmplitude, progress),
                SliderToolboxUiUtils.Interpolate(startFrequency, endFrequency, progress),
                SliderToolboxUiUtils.InterpolateInt(startOctaves, endOctaves, progress),
                SliderToolboxUiUtils.Interpolate(startPersistence, endPersistence, progress),
                SliderToolboxUiUtils.InterpolateInt(startSmoothing, endSmoothing, progress),
                SliderToolboxUiUtils.InterpolateInt(startSeed, endSeed, progress));

            return new NoiseSliderFrame(
                parameters,
                SliderToolboxUiUtils.Interpolate(startScale, endScale, progress),
                SliderToolboxUiUtils.InterpolateAngle(startRotationTurns, startRotationDegrees, endRotationTurns, endRotationDegrees, progress),
                SliderToolboxUiUtils.Interpolate(startOffsetX, endOffsetX, progress),
                SliderToolboxUiUtils.Interpolate(startOffsetY, endOffsetY, progress));
        }

        private Slider? createSlider(IReadOnlyList<Vector2> sourcePoints, NoiseSliderFrame frame, double startTime, double targetDuration, bool firstSlider)
        {
            IReadOnlyList<Vector2> points = ParametricSliderGenerator.TransformObject(
                sourcePoints,
                frame.Scale,
                frame.RotationDegrees,
                frame.OffsetX,
                frame.OffsetY);

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
                showError($"Equal duration requires {sliderVelocity:0.###}x SV. Adjust frame duration or scale.");
                return null;
            }

            var slider = new Slider
            {
                Position = headPosition,
                StartTime = startTime,
                NewCombo = firstSlider && newComboState.Value == TernaryState.True,
                SliderVelocityMultiplier = sliderVelocity,
                Path = path
            };
            slider.Samples.Add(slider.CreateHitSampleInfo());
            return slider;
        }

        private void updateNoisePreview(int frameCount)
        {
            noisePreviewDrawables.Clear();

            NoiseSliderFrame startFrame = buildFrame(0);
            drawGeneratorGuides(startFrame.Parameters, new Color4(1, 1, 1, 1));

            if (NoiseSliderGenerator.GetCategory(toolboxType.Value) == SliderToolbox2Category.VectorFlow)
                drawField(startFrame.Parameters, Color4.White);

            drawSamplePaths(startFrame, new Color4(70 / 255f, 180 / 255f, 1, 1));

            if (frameCount > 1)
                drawSamplePaths(buildFrame(1), new Color4(1, 172 / 255f, 82 / 255f, 1));

            previewText.Text = NoiseSliderGenerator.GetCategory(toolboxType.Value) == SliderToolbox2Category.VectorFlow
                ? frameCount == 1 ? "field + streamline" : "field + start/end"
                : frameCount == 1 ? "guides + source" : "guides + start/end";
        }

        private void drawGeneratorGuides(NoiseSliderParameters parameters, Color4 colour)
        {
            float drawWidth = Math.Max(1, noisePreviewDrawables.DrawWidth);
            float drawHeight = Math.Max(1, noisePreviewDrawables.DrawHeight);
            SliderToolbox2Category activeCategory = NoiseSliderGenerator.GetCategory(parameters.Type);

            switch (activeCategory)
            {
                case SliderToolbox2Category.VectorFlow:
                    drawPreviewBox(parameters, drawWidth, drawHeight, colour, 0.18f);
                    break;

                case SliderToolbox2Category.GraphPaths:
                    drawPreviewGrid(parameters, drawWidth, drawHeight, colour, 0.16f);
                    break;

                case SliderToolbox2Category.Attractors:
                    drawPreviewAxes(parameters, drawWidth, drawHeight, colour, 0.18f);
                    break;

                case SliderToolbox2Category.StrokeGrammar:
                case SliderToolbox2Category.BeatDriven:
                    drawPreviewBaseline(parameters, drawWidth, drawHeight, colour, 0.18f);
                    break;

                default:
                    drawPreviewBox(parameters, drawWidth, drawHeight, colour, 0.16f);
                    drawPreviewAxes(parameters, drawWidth, drawHeight, colour, 0.1f);
                    break;
            }
        }

        private void drawField(NoiseSliderParameters parameters, Color4 colour)
        {
            float drawWidth = Math.Max(1, noisePreviewDrawables.DrawWidth);
            float drawHeight = Math.Max(1, noisePreviewDrawables.DrawHeight);
            int detail = Math.Clamp(parameters.Octaves, 1, 8);
            int fieldColumns = 7 + detail * 2;
            int fieldRows = 4 + detail;

            for (int y = 0; y < fieldRows; y++)
            {
                double yProgress = fieldRows == 1 ? 0.5 : y / (double)(fieldRows - 1);

                for (int x = 0; x < fieldColumns; x++)
                {
                    double xProgress = fieldColumns == 1 ? 0.5 : x / (double)(fieldColumns - 1);
                    var point = new Vector2(
                        (float)(256 - parameters.Width / 2 + parameters.Width * xProgress),
                        (float)(192 - parameters.Height / 2 + parameters.Height * yProgress));
                    Vector2 fieldVector = NoiseSliderGenerator.PreviewFieldVector(parameters, point);

                    Vector2 start = mapToPreview(point, parameters, drawWidth, drawHeight);
                    Vector2 end = mapToPreview(point + fieldVector * 0.35f, parameters, drawWidth, drawHeight);
                    drawLine(start, end, 1.5f, colour, 0.38f);
                }
            }
        }

        private void drawSamplePaths(NoiseSliderFrame frame, Color4 colour)
        {
            NoiseSliderParameters parameters = frame.Parameters with
            {
                Samples = Math.Min(frame.Parameters.Samples, preview_max_samples)
            };
            IReadOnlyList<Vector2> path = NoiseSliderGenerator.GeneratePath(parameters);
            float drawWidth = Math.Max(1, noisePreviewDrawables.DrawWidth);
            float drawHeight = Math.Max(1, noisePreviewDrawables.DrawHeight);
            int stride = Math.Max(1, path.Count / preview_max_samples);
            Vector2? previous = null;

            for (int i = 0; i < path.Count; i += stride)
            {
                Vector2 current = mapToPreview(path[i], parameters, drawWidth, drawHeight);

                if (previous != null)
                    drawLine(previous.Value, current, 2.5f, colour, 0.85f);

                noisePreviewDrawables.Add(new Circle
                {
                    Origin = Anchor.Centre,
                    Position = current,
                    Size = new Vector2(i == 0 || i + stride >= path.Count ? 6 : 3),
                    Colour = colour,
                    Alpha = i == 0 || i + stride >= path.Count ? 0.95f : 0.65f
                });

                previous = current;
            }
        }

        private void drawPreviewBox(NoiseSliderParameters parameters, float drawWidth, float drawHeight, Color4 colour, float alpha)
        {
            var topLeft = mapToPreview(new Vector2((float)(256 - parameters.Width / 2), (float)(192 - parameters.Height / 2)), parameters, drawWidth, drawHeight);
            var topRight = mapToPreview(new Vector2((float)(256 + parameters.Width / 2), (float)(192 - parameters.Height / 2)), parameters, drawWidth, drawHeight);
            var bottomRight = mapToPreview(new Vector2((float)(256 + parameters.Width / 2), (float)(192 + parameters.Height / 2)), parameters, drawWidth, drawHeight);
            var bottomLeft = mapToPreview(new Vector2((float)(256 - parameters.Width / 2), (float)(192 + parameters.Height / 2)), parameters, drawWidth, drawHeight);

            drawLine(topLeft, topRight, 1, colour, alpha);
            drawLine(topRight, bottomRight, 1, colour, alpha);
            drawLine(bottomRight, bottomLeft, 1, colour, alpha);
            drawLine(bottomLeft, topLeft, 1, colour, alpha);
        }

        private void drawPreviewGrid(NoiseSliderParameters parameters, float drawWidth, float drawHeight, Color4 colour, float alpha)
        {
            drawPreviewBox(parameters, drawWidth, drawHeight, colour, alpha);

            for (int i = 1; i < 4; i++)
            {
                float x = (float)(256 - parameters.Width / 2 + parameters.Width * i / 4);
                drawLine(
                    mapToPreview(new Vector2(x, (float)(192 - parameters.Height / 2)), parameters, drawWidth, drawHeight),
                    mapToPreview(new Vector2(x, (float)(192 + parameters.Height / 2)), parameters, drawWidth, drawHeight),
                    1,
                    colour,
                    alpha * 0.75f);
            }

            for (int i = 1; i < 3; i++)
            {
                float y = (float)(192 - parameters.Height / 2 + parameters.Height * i / 3);
                drawLine(
                    mapToPreview(new Vector2((float)(256 - parameters.Width / 2), y), parameters, drawWidth, drawHeight),
                    mapToPreview(new Vector2((float)(256 + parameters.Width / 2), y), parameters, drawWidth, drawHeight),
                    1,
                    colour,
                    alpha * 0.75f);
            }
        }

        private void drawPreviewAxes(NoiseSliderParameters parameters, float drawWidth, float drawHeight, Color4 colour, float alpha)
        {
            drawLine(
                mapToPreview(new Vector2((float)(256 - parameters.Width / 2), 192), parameters, drawWidth, drawHeight),
                mapToPreview(new Vector2((float)(256 + parameters.Width / 2), 192), parameters, drawWidth, drawHeight),
                1,
                colour,
                alpha);
            drawLine(
                mapToPreview(new Vector2(256, (float)(192 - parameters.Height / 2)), parameters, drawWidth, drawHeight),
                mapToPreview(new Vector2(256, (float)(192 + parameters.Height / 2)), parameters, drawWidth, drawHeight),
                1,
                colour,
                alpha);
        }

        private void drawPreviewBaseline(NoiseSliderParameters parameters, float drawWidth, float drawHeight, Color4 colour, float alpha)
        {
            drawLine(
                mapToPreview(new Vector2((float)(256 - parameters.StreamlineLength / 2), 192), parameters, drawWidth, drawHeight),
                mapToPreview(new Vector2((float)(256 + parameters.StreamlineLength / 2), 192), parameters, drawWidth, drawHeight),
                1,
                colour,
                alpha);

            int marks = Math.Clamp((int)Math.Round(parameters.Frequency), 2, 16);

            for (int i = 0; i <= marks; i++)
            {
                float x = (float)(256 - parameters.StreamlineLength / 2 + parameters.StreamlineLength * i / marks);
                drawLine(
                    mapToPreview(new Vector2(x, 184), parameters, drawWidth, drawHeight),
                    mapToPreview(new Vector2(x, 200), parameters, drawWidth, drawHeight),
                    1,
                    colour,
                    alpha * 0.65f);
            }
        }

        private Vector2 mapToPreview(Vector2 point, NoiseSliderParameters parameters, float drawWidth, float drawHeight)
        {
            float margin = 32;
            bool vectorFieldType = NoiseSliderGenerator.GetCategory(parameters.Type) == SliderToolbox2Category.VectorFlow;
            float halfWidth = vectorFieldType
                ? (float)Math.Max(parameters.Width / 2, parameters.Amplitude * 0.75)
                : (float)Math.Max(Math.Max(parameters.Width, parameters.StreamlineLength) / 2, parameters.Amplitude * 0.75);
            float halfHeight = vectorFieldType
                ? (float)Math.Max(parameters.Height / 2, parameters.Amplitude * 0.75)
                : (float)Math.Max(Math.Max(parameters.Height, parameters.StreamlineLength) / 2, parameters.Amplitude * 0.75);
            float left = 256 - halfWidth - margin;
            float right = 256 + halfWidth + margin;
            float top = 192 - halfHeight - margin;
            float bottom = 192 + halfHeight + margin;

            float x = (point.X - left) / Math.Max(1, right - left) * drawWidth;
            float y = (point.Y - top) / Math.Max(1, bottom - top) * drawHeight;
            return new Vector2(Math.Clamp(x, 0, drawWidth), Math.Clamp(y, 0, drawHeight));
        }

        private void drawLine(Vector2 start, Vector2 end, float thickness, Color4 colour, float alpha)
        {
            Vector2 delta = end - start;
            float length = delta.Length;

            if (length <= 0.1f)
                return;

            noisePreviewDrawables.Add(new Box
            {
                Origin = Anchor.CentreLeft,
                Position = start,
                Width = length,
                Height = thickness,
                Rotation = (float)(Math.Atan2(delta.Y, delta.X) * 180 / Math.PI),
                Colour = colour,
                Alpha = alpha
            });
        }

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
            categoryConfig.Value = category.Value.ToString();
            typeConfig.Value = toolboxType.Value.ToString();
            headControlPointTypeConfig.Value = headControlPointType.Value.ToString();
            seedPatternConfig.Value = seedPattern.Value.ToString();
            noiseTypeConfig.Value = noiseType.Value.ToString();
            easingConfig.Value = easing.Value.ToString();
            startSeedPositionConfig.Value = startSeedPosition.Current.Value;
            endSeedPositionConfig.Value = endSeedPosition.Current.Value;
            startSeedAngleConfig.Value = startSeedAngle.Current.Value;
            endSeedAngleConfig.Value = endSeedAngle.Current.Value;
            startFieldBiasConfig.Value = startFieldBias.Current.Value;
            endFieldBiasConfig.Value = endFieldBias.Current.Value;
            startStreamlineLengthConfig.Value = startStreamlineLength.Current.Value;
            endStreamlineLengthConfig.Value = endStreamlineLength.Current.Value;
            startSamplesConfig.Value = startSamples.Current.Value;
            endSamplesConfig.Value = endSamples.Current.Value;
            startWidthConfig.Value = startWidth.Current.Value;
            endWidthConfig.Value = endWidth.Current.Value;
            startHeightConfig.Value = startHeight.Current.Value;
            endHeightConfig.Value = endHeight.Current.Value;
            startAmplitudeConfig.Value = startAmplitude.Current.Value;
            endAmplitudeConfig.Value = endAmplitude.Current.Value;
            startFrequencyConfig.Value = startFrequency.Current.Value;
            endFrequencyConfig.Value = endFrequency.Current.Value;
            startOctavesConfig.Value = startOctaves.Current.Value;
            endOctavesConfig.Value = endOctaves.Current.Value;
            startPersistenceConfig.Value = startPersistence.Current.Value;
            endPersistenceConfig.Value = endPersistence.Current.Value;
            startSmoothingConfig.Value = startSmoothing.Current.Value;
            endSmoothingConfig.Value = endSmoothing.Current.Value;
            startSeedConfig.Value = startSeed.Current.Value;
            endSeedConfig.Value = endSeed.Current.Value;
            startScaleConfig.Value = startScale.Current.Value;
            endScaleConfig.Value = endScale.Current.Value;
            startRotationTurnsConfig.Value = startRotationTurns.Current.Value;
            endRotationTurnsConfig.Value = endRotationTurns.Current.Value;
            startRotationDegreesConfig.Value = startRotationDegrees.Current.Value;
            endRotationDegreesConfig.Value = endRotationDegrees.Current.Value;
            startOffsetXConfig.Value = startOffsetX.Current.Value;
            endOffsetXConfig.Value = endOffsetX.Current.Value;
            startOffsetYConfig.Value = startOffsetY.Current.Value;
            endOffsetYConfig.Value = endOffsetY.Current.Value;
            framesConfig.Value = framesInput.Current.Value;
            frameDurationConfig.Value = frameDurationInput.Current.Value;
            frameGapConfig.Value = frameGapInput.Current.Value;
        }

        protected override void RestorePreview() => removePreview();

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

        private static T parseEnum<T>(string? value, T fallback)
            where T : struct, Enum =>
            Enum.TryParse(value, out T result) ? result : fallback;

        private readonly record struct NoiseSliderFrame(
            NoiseSliderParameters Parameters,
            double Scale,
            double RotationDegrees,
            double OffsetX,
            double OffsetY);
    }
}
