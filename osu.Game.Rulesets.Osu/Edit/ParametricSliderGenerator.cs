// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal enum ParametricSliderShape
    {
        Polygon,
        Rectangle,
        Diamond,
        Parallelogram,
        Rhombus,
        Trapezoid,
        Kite,

        [Description("Star polygon")]
        StarPolygon,

        Starburst,
        Cross,
        Chevron,

        [Description("Arrowhead")]
        ArrowHead,

        [Description("Latin uppercase")]
        SymbolLatinUppercase,

        [Description("Latin lowercase")]
        SymbolLatinLowercase,

        [Description("Italic uppercase")]
        SymbolItalicUppercase,

        [Description("Italic lowercase")]
        SymbolItalicLowercase,

        [Description("Bold uppercase")]
        SymbolBoldUppercase,

        [Description("Bold lowercase")]
        SymbolBoldLowercase,

        [Description("Script uppercase")]
        SymbolScriptUppercase,

        [Description("Script lowercase")]
        SymbolScriptLowercase,

        [Description("Fraktur uppercase")]
        SymbolFrakturUppercase,

        [Description("Fraktur lowercase")]
        SymbolFrakturLowercase,

        [Description("Bold italic uppercase")]
        SymbolBoldItalicUppercase,

        [Description("Bold italic lowercase")]
        SymbolBoldItalicLowercase,

        [Description("Sans-serif uppercase")]
        SymbolSansSerifUppercase,

        [Description("Sans-serif lowercase")]
        SymbolSansSerifLowercase,

        [Description("Sans-serif bold uppercase")]
        SymbolSansSerifBoldUppercase,

        [Description("Sans-serif bold lowercase")]
        SymbolSansSerifBoldLowercase,

        [Description("Monospace uppercase")]
        SymbolMonospaceUppercase,

        [Description("Monospace lowercase")]
        SymbolMonospaceLowercase,

        [Description("Blackboard letters")]
        SymbolBlackboardLetters,

        [Description("Greek uppercase")]
        SymbolGreekUppercase,

        [Description("Greek lowercase")]
        SymbolGreekLowercase,

        [Description("Hebrew letters")]
        SymbolHebrewLetters,

        [Description("Named math letters")]
        SymbolNamedMathLetters,

        [Description("Calculus symbols")]
        SymbolCalculus,

        [Description("Operators")]
        SymbolOperators,

        [Description("Relations")]
        SymbolRelations,

        [Description("Set / logic")]
        SymbolSetLogic,

        [Description("Arrows")]
        SymbolArrows,

        [Description("Delimiters")]
        SymbolDelimiters,

        [Description("Blackboard N")]
        BlackboardN,

        [Description("Blackboard Z")]
        BlackboardZ,

        [Description("Blackboard Q")]
        BlackboardQ,

        [Description("Blackboard R")]
        BlackboardR,

        [Description("Blackboard C")]
        BlackboardC,

        Aleph,
        Beth,

        [Description("Infinity")]
        InfinitySymbol,

        [Description("Integral")]
        IntegralSymbol,

        [Description("Contour integral")]
        ContourIntegral,

        [Description("Summation")]
        SummationSigma,

        [Description("Product")]
        ProductPi,

        [Description("Partial derivative")]
        PartialDerivative,

        Nabla,

        [Description("For all")]
        ForAll,

        Exists,

        [Description("Empty set")]
        EmptySet,

        [Description("Element of")]
        ElementOf,

        [Description("Subset")]
        Subset,

        [Description("Superset")]
        Superset,

        Union,
        Intersection,

        [Description("Logical and")]
        LogicalAnd,

        [Description("Logical or")]
        LogicalOr,

        [Description("Approximately equal")]
        ApproximatelyEqual,

        [Description("Proportional to")]
        ProportionalTo,

        Therefore,
        Because,

        Circle,
        Ellipse,

        [Description("Sine wave")]
        SineWave,

        [Description("Cosine wave")]
        CosineWave,

        [Description("Damped sine wave")]
        DampedSineWave,

        [Description("Sinc curve")]
        SincCurve,

        Parabola,

        [Description("Cubic curve")]
        CubicCurve,

        [Description("Semicubical parabola")]
        SemicubicalParabola,

        Cardioid,

        [Description("Heart curve")]
        HeartCurve,

        [Description("Lissajous curve")]
        LissajousCurve,

        [Description("Rose curve")]
        RoseCurve,

        [Description("Bernoulli lemniscate")]
        Lemniscate,

        Astroid,

        Superellipse,
        Limacon,
        Nephroid,
        Deltoid,
        Epispiral,

        [Description("Gear curve")]
        GearCurve,

        [Description("Maurer rose")]
        MaurerRose,

        [Description("Chebyshev curve")]
        ChebyshevCurve,

        Harmonograph,
        Cycloid,
        Epicycloid,
        Hypocycloid,
        Epitrochoid,
        Hypotrochoid,

        [Description("Archimedean spiral")]
        ArchimedeanSpiral,

        [Description("Logarithmic spiral")]
        LogarithmicSpiral,

        [Description("Hyperbolic spiral")]
        HyperbolicSpiral,

        [Description("Fermat spiral")]
        FermatSpiral,

        [Description("Euler spiral")]
        EulerSpiral,

        [Description("Butterfly curve")]
        ButterflyCurve,

        Cochleoid,

        [Description("Circle involute")]
        CircleInvolute,

        Catenary,

        [Description("Witch of Agnesi")]
        WitchOfAgnesi,

        Superformula,

        [Description("Golden spiral")]
        GoldenSpiral,

        [Description("Lituus spiral")]
        LituusSpiral,

        [Description("Spiral wave")]
        SpiralWave,

        [Description("Gaussian function")]
        GaussianFunction,

        [Description("Gaussian mixture")]
        GaussianMixture,

        [Description("Laplace distribution")]
        LaplaceDistribution,

        [Description("Cauchy distribution")]
        CauchyDistribution,

        [Description("Ricker wavelet")]
        RickerWavelet,

        [Description("Beta distribution")]
        BetaDistribution,

        [Description("Gamma distribution")]
        GammaDistribution,

        [Description("Log-normal distribution")]
        LogNormalDistribution,

        [Description("Weibull distribution")]
        WeibullDistribution,

        [Description("Exponential decay")]
        ExponentialDecay,

        [Description("Logistic sigmoid")]
        LogisticSigmoid,

        [Description("Tanh curve")]
        TanhCurve,

        Softplus,
        Swish,
        GELU,
        ReLU,

        [Description("Leaky ReLU")]
        LeakyReLU,
        ELU,

        [Description("Koch curve")]
        KochCurve,

        [Description("Koch snowflake")]
        KochSnowflake,

        [Description("Dragon curve")]
        DragonCurve,

        [Description("Sierpinski arrowhead")]
        SierpinskiArrowhead,

        [Description("Hilbert curve")]
        HilbertCurve,

        [Description("Gosper curve")]
        GosperCurve,

        [Description("Levy C curve")]
        LevyCurve,

        [Description("Menger carpet")]
        MengerCarpet,

        [Description("Barnsley fern")]
        BarnsleyFern,

        [Description("Mandelbrot set")]
        MandelbrotSet,

        [Description("Julia set")]
        JuliaSet,

        [Description("Burning Ship")]
        BurningShip,

        [Description("Newton fractal")]
        NewtonFractal,

        [Description("Peano curve")]
        PeanoCurve,

        [Description("Moore curve")]
        MooreCurve,

        [Description("Minkowski sausage")]
        MinkowskiSausage,

        [Description("Minkowski island")]
        MinkowskiIsland,

        [Description("Terdragon curve")]
        TerdragonCurve,

        [Description("Cesaro curve")]
        CesaroCurve,

        [Description("Fractal tree")]
        FractalTree,

        [Description("Binary tree")]
        BinaryTree,

        [Description("Regular polyhedron")]
        RegularPolyhedronProjection,

        [Description("Torus knot")]
        TorusKnotProjection,

        [Description("Spherical helix")]
        SphericalHelixProjection
    }

    internal enum ParametricSliderShapeCategory
    {
        Basic,
        Symbols,
        Waves,
        Functions,
        Parametric,
        Spirals,
        Fractals,
        Projections3D
    }

    internal enum ParametricSliderHeadPathType
    {
        Linear,
        Bezier,

        [Description("Perfect curve")]
        PerfectCurve,

        [Description("B-spline")]
        BSpline,
        Catmull
    }

    internal static class ParametricSliderGenerator
    {
        private const int max_control_points = 8000;
        private static readonly int[] valid_polyhedron_face_counts = { 4, 6, 8, 12, 20 };

        public static ParametricSliderShapeCategory GetCategory(ParametricSliderShape shape)
        {
            if (shape <= ParametricSliderShape.ArrowHead)
                return ParametricSliderShapeCategory.Basic;

            if (shape <= ParametricSliderShape.Because)
                return ParametricSliderShapeCategory.Symbols;

            if (shape is ParametricSliderShape.SineWave
                or ParametricSliderShape.CosineWave
                or ParametricSliderShape.DampedSineWave
                or ParametricSliderShape.SincCurve)
                return ParametricSliderShapeCategory.Waves;

            if (shape >= ParametricSliderShape.GaussianFunction && shape <= ParametricSliderShape.ELU)
                return ParametricSliderShapeCategory.Functions;

            if (shape is ParametricSliderShape.RegularPolyhedronProjection
                or ParametricSliderShape.TorusKnotProjection
                or ParametricSliderShape.SphericalHelixProjection)
                return ParametricSliderShapeCategory.Projections3D;

            if (shape is ParametricSliderShape.ArchimedeanSpiral
                or ParametricSliderShape.LogarithmicSpiral
                or ParametricSliderShape.HyperbolicSpiral
                or ParametricSliderShape.FermatSpiral
                or ParametricSliderShape.EulerSpiral
                or ParametricSliderShape.Cochleoid
                or ParametricSliderShape.CircleInvolute
                or ParametricSliderShape.GoldenSpiral
                or ParametricSliderShape.LituusSpiral
                or ParametricSliderShape.SpiralWave)
                return ParametricSliderShapeCategory.Spirals;

            return shape < ParametricSliderShape.KochCurve
                ? ParametricSliderShapeCategory.Parametric
                : ParametricSliderShapeCategory.Fractals;
        }

        public static int NormalisePolyhedronFaceCount(int faceCount)
        {
            int nearest = valid_polyhedron_face_counts[0];

            foreach (int validFaceCount in valid_polyhedron_face_counts)
            {
                if (Math.Abs(validFaceCount - faceCount) < Math.Abs(nearest - faceCount))
                    nearest = validFaceCount;
            }

            return nearest;
        }

        public static PathType GetHeadPathType(ParametricSliderHeadPathType pathType) => pathType switch
        {
            ParametricSliderHeadPathType.Bezier => PathType.BEZIER,
            ParametricSliderHeadPathType.PerfectCurve => PathType.PERFECT_CURVE,
            ParametricSliderHeadPathType.BSpline => PathType.BSpline(4),
            ParametricSliderHeadPathType.Catmull => PathType.CATMULL,
            _ => PathType.LINEAR
        };

        public static bool IsVisibleInToolbox(ParametricSliderShape shape) =>
            shape < ParametricSliderShape.BlackboardN || shape > ParametricSliderShape.Because;

        public static bool UsesSymbolText(ParametricSliderShape shape) => shape is
            ParametricSliderShape.SymbolLatinUppercase
            or ParametricSliderShape.SymbolLatinLowercase
            or ParametricSliderShape.SymbolItalicUppercase
            or ParametricSliderShape.SymbolItalicLowercase
            or ParametricSliderShape.SymbolBoldUppercase
            or ParametricSliderShape.SymbolBoldLowercase
            or ParametricSliderShape.SymbolScriptUppercase
            or ParametricSliderShape.SymbolScriptLowercase
            or ParametricSliderShape.SymbolFrakturUppercase
            or ParametricSliderShape.SymbolFrakturLowercase
            or ParametricSliderShape.SymbolBoldItalicUppercase
            or ParametricSliderShape.SymbolBoldItalicLowercase
            or ParametricSliderShape.SymbolSansSerifUppercase
            or ParametricSliderShape.SymbolSansSerifLowercase
            or ParametricSliderShape.SymbolSansSerifBoldUppercase
            or ParametricSliderShape.SymbolSansSerifBoldLowercase
            or ParametricSliderShape.SymbolMonospaceUppercase
            or ParametricSliderShape.SymbolMonospaceLowercase
            or ParametricSliderShape.SymbolBlackboardLetters
            or ParametricSliderShape.SymbolGreekUppercase
            or ParametricSliderShape.SymbolGreekLowercase
            or ParametricSliderShape.SymbolHebrewLetters
            or ParametricSliderShape.SymbolNamedMathLetters
            or ParametricSliderShape.SymbolCalculus
            or ParametricSliderShape.SymbolOperators
            or ParametricSliderShape.SymbolRelations
            or ParametricSliderShape.SymbolSetLogic
            or ParametricSliderShape.SymbolArrows
            or ParametricSliderShape.SymbolDelimiters;

        public static bool UsesCurveParameter(ParametricSliderShape shape) => shape is
            ParametricSliderShape.Rectangle
            or ParametricSliderShape.Parallelogram
            or ParametricSliderShape.Rhombus
            or ParametricSliderShape.Trapezoid
            or ParametricSliderShape.Kite
            or ParametricSliderShape.StarPolygon
            or ParametricSliderShape.Starburst
            or ParametricSliderShape.Cross
            or ParametricSliderShape.Chevron
            or ParametricSliderShape.ArrowHead
            or ParametricSliderShape.Ellipse
            or ParametricSliderShape.SineWave
            or ParametricSliderShape.CosineWave
            or ParametricSliderShape.DampedSineWave
            or ParametricSliderShape.SincCurve
            or ParametricSliderShape.LissajousCurve
            or ParametricSliderShape.RoseCurve
            or ParametricSliderShape.Superellipse
            or ParametricSliderShape.Limacon
            or ParametricSliderShape.Epispiral
            or ParametricSliderShape.GearCurve
            or ParametricSliderShape.MaurerRose
            or ParametricSliderShape.ChebyshevCurve
            or ParametricSliderShape.Harmonograph
            or ParametricSliderShape.Cycloid
            or ParametricSliderShape.Epicycloid
            or ParametricSliderShape.Hypocycloid
            or ParametricSliderShape.Epitrochoid
            or ParametricSliderShape.Hypotrochoid
            or ParametricSliderShape.ArchimedeanSpiral
            or ParametricSliderShape.LogarithmicSpiral
            or ParametricSliderShape.HyperbolicSpiral
            or ParametricSliderShape.FermatSpiral
            or ParametricSliderShape.EulerSpiral
            or ParametricSliderShape.Cochleoid
            or ParametricSliderShape.CircleInvolute
            or ParametricSliderShape.Superformula
            or ParametricSliderShape.GoldenSpiral
            or ParametricSliderShape.LituusSpiral
            or ParametricSliderShape.SpiralWave
            or ParametricSliderShape.GaussianFunction
            or ParametricSliderShape.GaussianMixture
            or ParametricSliderShape.LaplaceDistribution
            or ParametricSliderShape.CauchyDistribution
            or ParametricSliderShape.RickerWavelet
            or ParametricSliderShape.BetaDistribution
            or ParametricSliderShape.GammaDistribution
            or ParametricSliderShape.LogNormalDistribution
            or ParametricSliderShape.WeibullDistribution
            or ParametricSliderShape.ExponentialDecay
            or ParametricSliderShape.LogisticSigmoid
            or ParametricSliderShape.TanhCurve
            or ParametricSliderShape.Softplus
            or ParametricSliderShape.Swish
            or ParametricSliderShape.GELU
            or ParametricSliderShape.ReLU
            or ParametricSliderShape.LeakyReLU
            or ParametricSliderShape.ELU
            or ParametricSliderShape.TorusKnotProjection
            or ParametricSliderShape.SphericalHelixProjection;

        public static bool UsesSecondaryCurveParameter(ParametricSliderShape shape) => shape is
            ParametricSliderShape.Parallelogram
            or ParametricSliderShape.Trapezoid
            or ParametricSliderShape.Kite
            or ParametricSliderShape.Cross
            or ParametricSliderShape.Chevron
            or ParametricSliderShape.ArrowHead
            or ParametricSliderShape.GaussianMixture
            or ParametricSliderShape.BetaDistribution
            or ParametricSliderShape.GammaDistribution
            or ParametricSliderShape.LogNormalDistribution
            or ParametricSliderShape.WeibullDistribution
            or ParametricSliderShape.LissajousCurve
            or ParametricSliderShape.GearCurve
            or ParametricSliderShape.MaurerRose
            or ParametricSliderShape.ChebyshevCurve
            or ParametricSliderShape.Harmonograph
            or ParametricSliderShape.Epitrochoid
            or ParametricSliderShape.Hypotrochoid
            or ParametricSliderShape.Superformula
            or ParametricSliderShape.SpiralWave
            or ParametricSliderShape.TorusKnotProjection
            or ParametricSliderShape.SphericalHelixProjection;

        public static bool UsesVertices(ParametricSliderShape shape) => shape is
            ParametricSliderShape.Polygon
            or ParametricSliderShape.StarPolygon
            or ParametricSliderShape.Starburst;

        public static bool PrimaryParameterIsDiscrete(ParametricSliderShape shape) => shape is
            ParametricSliderShape.LissajousCurve
            or ParametricSliderShape.RoseCurve
            or ParametricSliderShape.Epispiral
            or ParametricSliderShape.GearCurve
            or ParametricSliderShape.MaurerRose
            or ParametricSliderShape.ChebyshevCurve
            or ParametricSliderShape.Harmonograph
            or ParametricSliderShape.Cycloid
            or ParametricSliderShape.Epicycloid
            or ParametricSliderShape.Hypocycloid
            or ParametricSliderShape.Epitrochoid
            or ParametricSliderShape.Hypotrochoid
            or ParametricSliderShape.Superformula
            or ParametricSliderShape.TorusKnotProjection;

        public static bool SecondaryParameterIsDiscrete(ParametricSliderShape shape) => shape is
            ParametricSliderShape.LissajousCurve
            or ParametricSliderShape.MaurerRose
            or ParametricSliderShape.ChebyshevCurve
            or ParametricSliderShape.Harmonograph
            or ParametricSliderShape.TorusKnotProjection;

        public static string GetVerticesParameterName(ParametricSliderShape shape) => shape switch
        {
            ParametricSliderShape.StarPolygon or ParametricSliderShape.Starburst => "points",
            _ => "vertices"
        };

        public static string GetPrimaryParameterName(ParametricSliderShape shape) => shape switch
        {
            ParametricSliderShape.Rectangle or ParametricSliderShape.Parallelogram => "Aspect",
            ParametricSliderShape.Rhombus => "Angle",
            ParametricSliderShape.Trapezoid => "Top width",
            ParametricSliderShape.Kite => "Upper length",
            ParametricSliderShape.StarPolygon or ParametricSliderShape.Starburst => "Inner radius",
            ParametricSliderShape.Cross => "Arm width",
            ParametricSliderShape.Chevron => "Notch",
            ParametricSliderShape.ArrowHead => "Head width",
            ParametricSliderShape.Ellipse => "Aspect",
            ParametricSliderShape.SineWave or ParametricSliderShape.CosineWave or ParametricSliderShape.DampedSineWave => "Cycles",
            ParametricSliderShape.SincCurve => "Frequency",
            ParametricSliderShape.GaussianFunction
                or ParametricSliderShape.LaplaceDistribution
                or ParametricSliderShape.CauchyDistribution
                or ParametricSliderShape.RickerWavelet => "Width",
            ParametricSliderShape.GaussianMixture => "Separation",
            ParametricSliderShape.BetaDistribution => "Alpha",
            ParametricSliderShape.GammaDistribution => "Shape",
            ParametricSliderShape.LogNormalDistribution => "Location",
            ParametricSliderShape.WeibullDistribution => "Shape",
            ParametricSliderShape.ExponentialDecay => "Rate",
            ParametricSliderShape.LogisticSigmoid
                or ParametricSliderShape.TanhCurve
                or ParametricSliderShape.Softplus
                or ParametricSliderShape.Swish
                or ParametricSliderShape.GELU
                or ParametricSliderShape.ReLU
                or ParametricSliderShape.LeakyReLU
                or ParametricSliderShape.ELU => "Steepness",
            ParametricSliderShape.LissajousCurve => "X frequency",
            ParametricSliderShape.RoseCurve => "Petals",
            ParametricSliderShape.Superellipse => "Exponent",
            ParametricSliderShape.Limacon => "Loop size",
            ParametricSliderShape.Epispiral => "Branches",
            ParametricSliderShape.GearCurve => "Teeth",
            ParametricSliderShape.MaurerRose => "Petals",
            ParametricSliderShape.ChebyshevCurve or ParametricSliderShape.Harmonograph => "X frequency",
            ParametricSliderShape.Cycloid => "Cusps",
            ParametricSliderShape.Epicycloid or ParametricSliderShape.Hypocycloid => "Cusps",
            ParametricSliderShape.Epitrochoid or ParametricSliderShape.Hypotrochoid => "Lobes",
            ParametricSliderShape.Superformula => "Symmetry",
            ParametricSliderShape.TorusKnotProjection => "Major winding",
            ParametricSliderShape.SphericalHelixProjection => "Turns",
            _ => "Turns"
        };

        public static string GetSecondaryParameterName(ParametricSliderShape shape) => shape switch
        {
            ParametricSliderShape.Parallelogram => "Skew",
            ParametricSliderShape.Trapezoid => "Height",
            ParametricSliderShape.Kite => "Waist",
            ParametricSliderShape.Cross => "Stem width",
            ParametricSliderShape.Chevron => "Depth",
            ParametricSliderShape.ArrowHead => "Tail width",
            ParametricSliderShape.GaussianMixture => "Width",
            ParametricSliderShape.BetaDistribution => "Beta",
            ParametricSliderShape.GammaDistribution => "Scale",
            ParametricSliderShape.LogNormalDistribution => "Spread",
            ParametricSliderShape.WeibullDistribution => "Scale",
            ParametricSliderShape.LissajousCurve => "Y frequency",
            ParametricSliderShape.GearCurve => "Tooth depth",
            ParametricSliderShape.MaurerRose => "Step",
            ParametricSliderShape.ChebyshevCurve or ParametricSliderShape.Harmonograph => "Y frequency",
            ParametricSliderShape.Epitrochoid or ParametricSliderShape.Hypotrochoid => "Pen offset",
            ParametricSliderShape.Superformula => "Roundness",
            ParametricSliderShape.SpiralWave => "Wave depth",
            ParametricSliderShape.TorusKnotProjection => "Minor winding",
            ParametricSliderShape.SphericalHelixProjection => "Latitude waves",
            _ => "Secondary parameter"
        };

        public static bool UsesResolution(ParametricSliderShape shape) =>
            GetCategory(shape) is ParametricSliderShapeCategory.Waves
                or ParametricSliderShapeCategory.Symbols
                or ParametricSliderShapeCategory.Functions
                or ParametricSliderShapeCategory.Parametric
                or ParametricSliderShapeCategory.Spirals
            || shape is ParametricSliderShape.TorusKnotProjection
                or ParametricSliderShape.SphericalHelixProjection
            || shape is ParametricSliderShape.BarnsleyFern
                or ParametricSliderShape.MandelbrotSet
                or ParametricSliderShape.JuliaSet
                or ParametricSliderShape.BurningShip
                or ParametricSliderShape.NewtonFractal;

        public static bool UsesIterations(ParametricSliderShape shape) =>
            GetCategory(shape) == ParametricSliderShapeCategory.Fractals;

        public static bool SupportsCyclicHeadPosition(ParametricSliderShape shape) =>
            GetCategory(shape) == ParametricSliderShapeCategory.Symbols
            || shape is
            ParametricSliderShape.Polygon
            or ParametricSliderShape.Rectangle
            or ParametricSliderShape.Diamond
            or ParametricSliderShape.Parallelogram
            or ParametricSliderShape.Rhombus
            or ParametricSliderShape.Trapezoid
            or ParametricSliderShape.Kite
            or ParametricSliderShape.StarPolygon
            or ParametricSliderShape.Starburst
            or ParametricSliderShape.Cross
            or ParametricSliderShape.Chevron
            or ParametricSliderShape.ArrowHead
            or ParametricSliderShape.Circle
            or ParametricSliderShape.Ellipse
            or ParametricSliderShape.Cardioid
            or ParametricSliderShape.HeartCurve
            or ParametricSliderShape.LissajousCurve
            or ParametricSliderShape.RoseCurve
            or ParametricSliderShape.Lemniscate
            or ParametricSliderShape.Astroid
            or ParametricSliderShape.Superellipse
            or ParametricSliderShape.Limacon
            or ParametricSliderShape.Nephroid
            or ParametricSliderShape.Deltoid
            or ParametricSliderShape.GearCurve
            or ParametricSliderShape.MaurerRose
            or ParametricSliderShape.ChebyshevCurve
            or ParametricSliderShape.Epicycloid
            or ParametricSliderShape.Hypocycloid
            or ParametricSliderShape.Epitrochoid
            or ParametricSliderShape.Hypotrochoid
            or ParametricSliderShape.ButterflyCurve
            or ParametricSliderShape.Superformula
            or ParametricSliderShape.KochSnowflake
            or ParametricSliderShape.MinkowskiIsland
            or ParametricSliderShape.MooreCurve
            or ParametricSliderShape.MandelbrotSet
            or ParametricSliderShape.JuliaSet
            or ParametricSliderShape.BurningShip
            or ParametricSliderShape.RegularPolyhedronProjection
            or ParametricSliderShape.TorusKnotProjection;

        public static IReadOnlyList<Vector2> Generate(
            ParametricSliderShape shape,
            int vertices,
            int iterations,
            int resolution,
            double parameterReal,
            double parameterImaginary,
            double primaryParameter,
            double secondaryParameter,
            int polyhedronFaces,
            double quaternionX,
            double quaternionY,
            double quaternionZ,
            double quaternionW,
            double perspective,
            string symbolText = "")
        {
            iterations = Math.Clamp(iterations, 1, 10);
            resolution = Math.Clamp(resolution, 8, 180);
            primaryParameter = Math.Clamp(primaryParameter, 1, 32);
            secondaryParameter = Math.Clamp(secondaryParameter, 1, 32);
            int samples = Math.Clamp(resolution * 6, 48, 1440);

            List<Vector2> points = shape switch
            {
                ParametricSliderShape.Polygon => polygon(Math.Clamp(vertices, 3, 64)),
                ParametricSliderShape.Rectangle => rectangle(primaryParameter),
                ParametricSliderShape.Diamond => polygonFromPoints(
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(-1, 0)),
                ParametricSliderShape.Parallelogram => parallelogram(primaryParameter, secondaryParameter),
                ParametricSliderShape.Rhombus => rhombus(primaryParameter),
                ParametricSliderShape.Trapezoid => trapezoid(primaryParameter, secondaryParameter),
                ParametricSliderShape.Kite => kite(primaryParameter, secondaryParameter),
                ParametricSliderShape.StarPolygon => star(Math.Clamp(vertices, 3, 32), primaryParameter, false),
                ParametricSliderShape.Starburst => star(Math.Clamp(vertices, 3, 32), primaryParameter, true),
                ParametricSliderShape.Cross => cross(primaryParameter, secondaryParameter),
                ParametricSliderShape.Chevron => chevron(primaryParameter, secondaryParameter),
                ParametricSliderShape.ArrowHead => arrowHead(primaryParameter, secondaryParameter),
                ParametricSliderShape.SymbolLatinUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolLatinLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolItalicUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolItalicLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolBoldUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolBoldLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolScriptUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolScriptLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolFrakturUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolFrakturLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolBoldItalicUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolBoldItalicLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolSansSerifUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolSansSerifLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolSansSerifBoldUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolSansSerifBoldLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolMonospaceUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolMonospaceLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolBlackboardLetters => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolGreekUppercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolGreekLowercase => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolHebrewLetters => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolNamedMathLetters => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolCalculus => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolOperators => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolRelations => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolSetLogic => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolArrows => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.SymbolDelimiters => mathGlyph(symbolCodepoint(shape, symbolText), samples),
                ParametricSliderShape.BlackboardN => mathGlyph(0x2115, samples),
                ParametricSliderShape.BlackboardZ => mathGlyph(0x2124, samples),
                ParametricSliderShape.BlackboardQ => mathGlyph(0x211A, samples),
                ParametricSliderShape.BlackboardR => mathGlyph(0x211D, samples),
                ParametricSliderShape.BlackboardC => mathGlyph(0x2102, samples),
                ParametricSliderShape.Aleph => mathGlyph(0x2135, samples),
                ParametricSliderShape.Beth => mathGlyph(0x2136, samples),
                ParametricSliderShape.InfinitySymbol => mathGlyph(0x221E, samples),
                ParametricSliderShape.IntegralSymbol => mathGlyph(0x222B, samples),
                ParametricSliderShape.ContourIntegral => mathGlyph(0x222E, samples),
                ParametricSliderShape.SummationSigma => mathGlyph(0x2211, samples),
                ParametricSliderShape.ProductPi => mathGlyph(0x220F, samples),
                ParametricSliderShape.PartialDerivative => mathGlyph(0x2202, samples),
                ParametricSliderShape.Nabla => mathGlyph(0x2207, samples),
                ParametricSliderShape.ForAll => mathGlyph(0x2200, samples),
                ParametricSliderShape.Exists => mathGlyph(0x2203, samples),
                ParametricSliderShape.EmptySet => mathGlyph(0x2205, samples),
                ParametricSliderShape.ElementOf => mathGlyph(0x2208, samples),
                ParametricSliderShape.Subset => mathGlyph(0x2282, samples),
                ParametricSliderShape.Superset => mathGlyph(0x2283, samples),
                ParametricSliderShape.Union => mathGlyph(0x222A, samples),
                ParametricSliderShape.Intersection => mathGlyph(0x2229, samples),
                ParametricSliderShape.LogicalAnd => mathGlyph(0x2227, samples),
                ParametricSliderShape.LogicalOr => mathGlyph(0x2228, samples),
                ParametricSliderShape.ApproximatelyEqual => mathGlyph(0x2248, samples),
                ParametricSliderShape.ProportionalTo => mathGlyph(0x221D, samples),
                ParametricSliderShape.Therefore => mathGlyph(0x2234, samples),
                ParametricSliderShape.Because => mathGlyph(0x2235, samples),
                ParametricSliderShape.Circle => parametric(0, Math.Tau, samples, t => polar(1, t), true),
                ParametricSliderShape.Ellipse => parametric(0, Math.Tau, samples, t => ellipse(t, primaryParameter), true),
                ParametricSliderShape.SineWave => functionCurve(samples, x => Math.Sin(Math.Tau * primaryParameter * x)),
                ParametricSliderShape.CosineWave => functionCurve(samples, x => Math.Cos(Math.Tau * primaryParameter * x)),
                ParametricSliderShape.DampedSineWave => functionCurve(samples, x => Math.Exp(-1.8 * (x + 1)) * Math.Sin(Math.Tau * primaryParameter * (x + 1))),
                ParametricSliderShape.SincCurve => functionCurve(samples, x => sinc(Math.PI * primaryParameter * x)),
                ParametricSliderShape.Parabola => functionCurve(samples, x => 2 * x * x - 1),
                ParametricSliderShape.CubicCurve => functionCurve(samples, x => x * x * x),
                ParametricSliderShape.SemicubicalParabola => parametric(-1.3, 1.3, samples, t => new Vector2((float)(t * t), (float)(t * t * t))),
                ParametricSliderShape.Cardioid => parametric(0, Math.Tau, samples, t => polar(1 - Math.Cos(t), t), true),
                ParametricSliderShape.HeartCurve => parametric(0, Math.Tau, samples, heart, true),
                ParametricSliderShape.LissajousCurve => parametric(0, Math.Tau, samples, t => new Vector2(
                    (float)Math.Sin(Math.Round(primaryParameter) * t + Math.PI / 2),
                    (float)Math.Sin(Math.Round(secondaryParameter) * t)), true),
                ParametricSliderShape.RoseCurve => parametric(0, Math.Tau, samples, t => polar(Math.Cos(Math.Round(primaryParameter) * t), t), true),
                ParametricSliderShape.Lemniscate => parametric(0, Math.Tau, samples, lemniscate, true),
                ParametricSliderShape.Astroid => parametric(0, Math.Tau, samples, t => new Vector2(cube(Math.Cos(t)), cube(Math.Sin(t))), true),
                ParametricSliderShape.Superellipse => parametric(0, Math.Tau, samples, t => superellipse(t, primaryParameter), true),
                ParametricSliderShape.Limacon => parametric(0, Math.Tau, samples, t => polar(1 + primaryParameter / 8 * Math.Cos(t), t), true),
                ParametricSliderShape.Nephroid => parametric(0, Math.Tau, samples, t => epiHypocycloid(t, 2, false), true),
                ParametricSliderShape.Deltoid => parametric(0, Math.Tau, samples, t => epiHypocycloid(t, 3, true), true),
                ParametricSliderShape.Epispiral => parametric(0, Math.Tau, samples, t =>
                    polar(Math.Clamp(1 / Math.Max(0.18, Math.Abs(Math.Cos(Math.Round(primaryParameter) * t))), 0, 4), t), true),
                ParametricSliderShape.GearCurve => parametric(0, Math.Tau, samples, t =>
                    polar(1 + 0.45 * Math.Clamp(secondaryParameter / 32, 0.03, 1) * Math.Cos(Math.Round(primaryParameter) * t), t), true),
                ParametricSliderShape.MaurerRose => maurerRose(Math.Round(primaryParameter), Math.Round(secondaryParameter)),
                ParametricSliderShape.ChebyshevCurve => parametric(0, Math.Tau, samples, t => new Vector2(
                    (float)Math.Cos(Math.Round(primaryParameter) * t),
                    (float)Math.Cos(Math.Round(secondaryParameter) * t)), true),
                ParametricSliderShape.Harmonograph => harmonograph(samples, Math.Round(primaryParameter), Math.Round(secondaryParameter)),
                ParametricSliderShape.Cycloid => parametric(0, Math.Tau * Math.Round(primaryParameter), samples, t =>
                    new Vector2((float)(t - Math.Sin(t)), (float)(1 - Math.Cos(t)))),
                ParametricSliderShape.Epicycloid => parametric(0, Math.Tau, samples, t => epiHypocycloid(t, Math.Max(2, Math.Round(primaryParameter)), false), true),
                ParametricSliderShape.Hypocycloid => parametric(0, Math.Tau, samples, t => epiHypocycloid(t, Math.Max(3, Math.Round(primaryParameter)), true), true),
                ParametricSliderShape.Epitrochoid => parametric(0, Math.Tau, samples, t =>
                    trochoid(t, Math.Max(2, Math.Round(primaryParameter)), secondaryParameter / 4, false), true),
                ParametricSliderShape.Hypotrochoid => parametric(0, Math.Tau, samples, t =>
                    trochoid(t, Math.Max(3, Math.Round(primaryParameter)), secondaryParameter / 4, true), true),
                ParametricSliderShape.ArchimedeanSpiral => parametric(0, Math.Tau * primaryParameter, samples, t => polar(0.08 + t, t)),
                ParametricSliderShape.LogarithmicSpiral => parametric(0, Math.Tau * primaryParameter, samples, t => polar(Math.Exp(0.12 * t), t)),
                ParametricSliderShape.HyperbolicSpiral => parametric(0.35, Math.Tau * primaryParameter + 0.35, samples, t => polar(1 / t, t)),
                ParametricSliderShape.FermatSpiral => parametric(-Math.Tau * primaryParameter, Math.Tau * primaryParameter, samples, t =>
                    polar(Math.Sign(t) * Math.Sqrt(Math.Abs(t)), t)),
                ParametricSliderShape.EulerSpiral => eulerSpiral(samples, primaryParameter),
                ParametricSliderShape.ButterflyCurve => parametric(0, Math.Tau * 12, samples, butterfly, true),
                ParametricSliderShape.Cochleoid => parametric(-Math.PI * primaryParameter, Math.PI * primaryParameter, samples, t =>
                    polar(Math.Abs(t) < 0.000001 ? 1 : Math.Sin(t) / t, t)),
                ParametricSliderShape.CircleInvolute => parametric(0, Math.Tau * primaryParameter, samples, t => new Vector2(
                    (float)(Math.Cos(t) + t * Math.Sin(t)),
                    (float)(Math.Sin(t) - t * Math.Cos(t)))),
                ParametricSliderShape.Catenary => functionCurve(samples, x => (Math.Cosh(2 * x) - 1) / 3),
                ParametricSliderShape.WitchOfAgnesi => functionCurve(samples, x => 1 / (1 + 8 * x * x)),
                ParametricSliderShape.Superformula => parametric(0, Math.Tau, samples, t =>
                    superformula(t, Math.Round(primaryParameter), secondaryParameter), true),
                ParametricSliderShape.GoldenSpiral => parametric(0, Math.Tau * primaryParameter, samples, t => polar(Math.Exp(0.18 * t), t)),
                ParametricSliderShape.LituusSpiral => parametric(0.35, Math.Tau * primaryParameter + 0.35, samples, t => polar(1 / Math.Sqrt(t), t)),
                ParametricSliderShape.SpiralWave => parametric(0, Math.Tau * primaryParameter, samples, t =>
                    polar(t * (1 + 0.35 * Math.Clamp(secondaryParameter / 32, 0.03, 1) * Math.Sin(6 * t)), t)),
                ParametricSliderShape.GaussianFunction => functionCurve(samples, x => gaussian(x, width(primaryParameter))),
                ParametricSliderShape.GaussianMixture => functionCurve(samples, x =>
                {
                    double separation = Math.Clamp(primaryParameter / 16, 0.08, 1.6);
                    double sigma = width(secondaryParameter);
                    return 0.72 * gaussian(x - separation, sigma) + 0.72 * gaussian(x + separation, sigma * 0.72);
                }),
                ParametricSliderShape.LaplaceDistribution => functionCurve(samples, x => Math.Exp(-Math.Abs(x) / width(primaryParameter))),
                ParametricSliderShape.CauchyDistribution => functionCurve(samples, x =>
                {
                    double gamma = width(primaryParameter);
                    return 1 / (1 + x * x / (gamma * gamma));
                }),
                ParametricSliderShape.RickerWavelet => functionCurve(samples, x =>
                {
                    double sigma = width(primaryParameter);
                    double squared = x * x / (sigma * sigma);
                    return (1 - squared) * Math.Exp(-squared / 2);
                }),
                ParametricSliderShape.BetaDistribution => functionCurve(samples, x => betaDensity((x + 1) / 2, primaryParameter, secondaryParameter)),
                ParametricSliderShape.GammaDistribution => functionCurve(samples, x => gammaDensity((x + 1) * 3.5 + 0.001, primaryParameter, secondaryParameter)),
                ParametricSliderShape.LogNormalDistribution => functionCurve(samples, x => logNormalDensity((x + 1) * 2.2 + 0.001, primaryParameter, secondaryParameter)),
                ParametricSliderShape.WeibullDistribution => functionCurve(samples, x => weibullDensity((x + 1) * 2.5 + 0.001, primaryParameter, secondaryParameter)),
                ParametricSliderShape.ExponentialDecay => functionCurve(samples, x => Math.Exp(-Math.Clamp(primaryParameter / 4, 0.25, 8) * (x + 1))),
                ParametricSliderShape.LogisticSigmoid => functionCurve(samples, x => logistic(Math.Clamp(primaryParameter, 1, 32) * x)),
                ParametricSliderShape.TanhCurve => functionCurve(samples, x => Math.Tanh(Math.Clamp(primaryParameter, 1, 32) * x / 4)),
                ParametricSliderShape.Softplus => functionCurve(samples, x =>
                {
                    double steepness = Math.Clamp(primaryParameter, 1, 32);
                    return Math.Log(1 + Math.Exp(steepness * x)) / steepness;
                }),
                ParametricSliderShape.Swish => functionCurve(samples, x => x * logistic(Math.Clamp(primaryParameter, 1, 32) * x / 4)),
                ParametricSliderShape.GELU => functionCurve(samples, x => 0.5 * x * (1 + Math.Tanh(Math.Sqrt(2 / Math.PI) * (x + 0.044715 * x * x * x) * Math.Clamp(primaryParameter / 4, 0.25, 8)))),
                ParametricSliderShape.ReLU => functionCurve(samples, x => Math.Max(0, Math.Clamp(primaryParameter / 4, 0.25, 8) * x)),
                ParametricSliderShape.LeakyReLU => functionCurve(samples, x => x >= 0 ? x : x * Math.Clamp(primaryParameter / 64, 0.02, 0.5)),
                ParametricSliderShape.ELU => functionCurve(samples, x => x >= 0 ? x : Math.Clamp(primaryParameter / 4, 0.25, 8) * (Math.Exp(x) - 1)),
                ParametricSliderShape.KochCurve => lSystem("F", new Dictionary<char, string> { ['F'] = "F+F--F+F" }, Math.Min(iterations, 6), 60, "F"),
                ParametricSliderShape.KochSnowflake => lSystem("F--F--F", new Dictionary<char, string> { ['F'] = "F+F--F+F" }, Math.Min(iterations, 5), 60, "F"),
                ParametricSliderShape.DragonCurve => lSystem("FX", new Dictionary<char, string> { ['X'] = "X+YF+", ['Y'] = "-FX-Y" }, Math.Min(iterations, 14), 90, "F"),
                ParametricSliderShape.SierpinskiArrowhead => lSystem(iterations % 2 == 0 ? "A" : "B", new Dictionary<char, string> { ['A'] = "B-A-B", ['B'] = "A+B+A" }, Math.Min(iterations, 9), 60, "AB"),
                ParametricSliderShape.HilbertCurve => lSystem("A", new Dictionary<char, string> { ['A'] = "+BF-AFA-FB+", ['B'] = "-AF+BFB+FA-" }, Math.Min(iterations, 7), 90, "F"),
                ParametricSliderShape.GosperCurve => lSystem("A", new Dictionary<char, string> { ['A'] = "A-B--B+A++AA+B-", ['B'] = "+A-BB--B-A++A+B" }, Math.Min(iterations, 5), 60, "AB"),
                ParametricSliderShape.LevyCurve => levyCurve(Math.Min(iterations, 14)),
                ParametricSliderShape.MengerCarpet => mengerCarpet(Math.Min(iterations, 4)),
                ParametricSliderShape.BarnsleyFern => barnsleyFern(Math.Min(max_control_points, 800 + resolution * Math.Min(iterations, 8))),
                ParametricSliderShape.MandelbrotSet => escapeTimeContour(resolution, Math.Max(24, iterations * 12), EscapeFractal.Mandelbrot, parameterReal, parameterImaginary),
                ParametricSliderShape.JuliaSet => escapeTimeContour(resolution, Math.Max(24, iterations * 12), EscapeFractal.Julia, parameterReal, parameterImaginary),
                ParametricSliderShape.BurningShip => escapeTimeContour(resolution, Math.Max(24, iterations * 12), EscapeFractal.BurningShip, parameterReal, parameterImaginary),
                ParametricSliderShape.NewtonFractal => newtonBoundary(resolution, Math.Max(8, iterations * 4)),
                ParametricSliderShape.PeanoCurve => lSystem("X", new Dictionary<char, string>
                {
                    ['X'] = "XFYFX+F+YFXFY-F-XFYFX",
                    ['Y'] = "YFXFY-F-XFYFX+F+YFXFY"
                }, Math.Min(iterations, 4), 90, "F"),
                ParametricSliderShape.MooreCurve => lSystem("LFL+F+LFL", new Dictionary<char, string>
                {
                    ['L'] = "-RF+LFL+FR-",
                    ['R'] = "+LF-RFR-FL+"
                }, Math.Min(iterations, 6), 90, "F"),
                ParametricSliderShape.MinkowskiSausage => lSystem("F", new Dictionary<char, string>
                {
                    ['F'] = "F+F-F-FF+F+F-F"
                }, Math.Min(iterations, 4), 90, "F"),
                ParametricSliderShape.MinkowskiIsland => lSystem("F+F+F+F", new Dictionary<char, string>
                {
                    ['F'] = "F+F-F-FF+F+F-F"
                }, Math.Min(iterations, 3), 90, "F"),
                ParametricSliderShape.TerdragonCurve => lSystem("F", new Dictionary<char, string>
                {
                    ['F'] = "F+F-F"
                }, Math.Min(iterations, 8), 120, "F"),
                ParametricSliderShape.CesaroCurve => lSystem("F", new Dictionary<char, string>
                {
                    ['F'] = "F+F--F+F"
                }, Math.Min(iterations, 6), 85, "F"),
                ParametricSliderShape.FractalTree => lSystem("X", new Dictionary<char, string>
                {
                    ['X'] = "F+[[X]-X]-F[-FX]+X",
                    ['F'] = "FF"
                }, Math.Min(iterations, 6), 25, "F", true),
                ParametricSliderShape.BinaryTree => lSystem("F", new Dictionary<char, string>
                {
                    ['F'] = "F[+F][-F]"
                }, Math.Min(iterations, 7), 35, "F", true),
                ParametricSliderShape.RegularPolyhedronProjection => regularPolyhedronProjection(
                    polyhedronFaces,
                    quaternionX,
                    quaternionY,
                    quaternionZ,
                    quaternionW,
                    perspective),
                ParametricSliderShape.TorusKnotProjection => parametric3D(0, Math.Tau, samples, t =>
                    torusKnot(t, Math.Round(primaryParameter), Math.Round(secondaryParameter)), quaternionX, quaternionY, quaternionZ, quaternionW, perspective, true),
                ParametricSliderShape.SphericalHelixProjection => parametric3D(0, Math.Tau, samples, t =>
                    sphericalHelix(t, primaryParameter, secondaryParameter), quaternionX, quaternionY, quaternionZ, quaternionW, perspective),
                _ => polygon(vertices)
            };

            return normaliseAndLimit(points, 260);
        }

        private static List<Vector2> regularPolyhedronProjection(
            int faceCount,
            double quaternionX,
            double quaternionY,
            double quaternionZ,
            double quaternionW,
            double perspective)
        {
            Vector3[] vertices = createRegularPolyhedronVertices(NormalisePolyhedronFaceCount(faceCount));
            List<(int A, int B)> edges = deriveEdges(vertices);
            List<int> walk = createClosedEdgeCoveringWalk(vertices.Length, edges);

            normaliseQuaternion(ref quaternionX, ref quaternionY, ref quaternionZ, ref quaternionW);
            perspective = Math.Clamp(perspective, 0, 1);

            var projectedVertices = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 rotated = rotate(vertices[i], quaternionX, quaternionY, quaternionZ, quaternionW);
                double depthScale = 1 / (1 - perspective * rotated.Z * 0.45);
                projectedVertices[i] = new Vector2((float)(rotated.X * depthScale), (float)(-rotated.Y * depthScale));
            }

            return walk.Select(index => projectedVertices[index]).ToList();
        }

        private static Vector3[] createRegularPolyhedronVertices(int faceCount)
        {
            const float inverse_sqrt_3 = 0.5773502692f;

            switch (faceCount)
            {
                case 4:
                    return new[]
                    {
                        new Vector3(inverse_sqrt_3, inverse_sqrt_3, inverse_sqrt_3),
                        new Vector3(-inverse_sqrt_3, -inverse_sqrt_3, inverse_sqrt_3),
                        new Vector3(-inverse_sqrt_3, inverse_sqrt_3, -inverse_sqrt_3),
                        new Vector3(inverse_sqrt_3, -inverse_sqrt_3, -inverse_sqrt_3)
                    };

                case 6:
                    return signedCubeVertices();

                case 8:
                    return new[]
                    {
                        new Vector3(1, 0, 0),
                        new Vector3(-1, 0, 0),
                        new Vector3(0, 1, 0),
                        new Vector3(0, -1, 0),
                        new Vector3(0, 0, 1),
                        new Vector3(0, 0, -1)
                    };

                case 12:
                    return dodecahedronVertices();

                default:
                    return icosahedronVertices();
            }
        }

        private static Vector3[] signedCubeVertices()
        {
            const float scale = 0.5773502692f;
            var vertices = new List<Vector3>(8);

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                        vertices.Add(new Vector3(x * scale, y * scale, z * scale));
                }
            }

            return vertices.ToArray();
        }

        private static Vector3[] icosahedronVertices()
        {
            float phi = (1 + MathF.Sqrt(5)) / 2;
            var vertices = new List<Vector3>(12);

            addSignedCoordinatePairs(vertices, 0, 1, phi);
            addSignedCoordinatePairs(vertices, 1, phi, 0);
            addSignedCoordinatePairs(vertices, phi, 0, 1);
            normaliseVertices(vertices);
            return vertices.ToArray();
        }

        private static Vector3[] dodecahedronVertices()
        {
            float phi = (1 + MathF.Sqrt(5)) / 2;
            float inversePhi = 1 / phi;
            var vertices = new List<Vector3>(20);

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                        vertices.Add(new Vector3(x, y, z));
                }
            }

            addSignedCoordinatePairs(vertices, 0, inversePhi, phi);
            addSignedCoordinatePairs(vertices, inversePhi, phi, 0);
            addSignedCoordinatePairs(vertices, phi, 0, inversePhi);
            normaliseVertices(vertices);
            return vertices.ToArray();
        }

        private static void addSignedCoordinatePairs(List<Vector3> vertices, float x, float y, float z)
        {
            for (int firstSign = -1; firstSign <= 1; firstSign += 2)
            {
                for (int secondSign = -1; secondSign <= 1; secondSign += 2)
                {
                    vertices.Add(new Vector3(
                        x == 0 ? 0 : x * firstSign,
                        y == 0 ? 0 : y * (x == 0 ? firstSign : secondSign),
                        z == 0 ? 0 : z * secondSign));
                }
            }
        }

        private static void normaliseVertices(List<Vector3> vertices)
        {
            float maximumLength = vertices.Max(vertex => MathF.Sqrt(vertex.X * vertex.X + vertex.Y * vertex.Y + vertex.Z * vertex.Z));

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];
                vertices[i] = new Vector3(vertex.X / maximumLength, vertex.Y / maximumLength, vertex.Z / maximumLength);
            }
        }

        private static List<(int A, int B)> deriveEdges(IReadOnlyList<Vector3> vertices)
        {
            float edgeLengthSquared = float.MaxValue;

            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                    edgeLengthSquared = Math.Min(edgeLengthSquared, distanceSquared(vertices[i], vertices[j]));
            }

            float tolerance = edgeLengthSquared * 0.02f;
            var edges = new List<(int A, int B)>();

            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    if (Math.Abs(distanceSquared(vertices[i], vertices[j]) - edgeLengthSquared) <= tolerance)
                        edges.Add((i, j));
                }
            }

            return edges;
        }

        private static float distanceSquared(Vector3 first, Vector3 second)
        {
            float x = first.X - second.X;
            float y = first.Y - second.Y;
            float z = first.Z - second.Z;
            return x * x + y * y + z * z;
        }

        private static List<int> createClosedEdgeCoveringWalk(int vertexCount, IReadOnlyList<(int A, int B)> edges)
        {
            var adjacency = Enumerable.Range(0, vertexCount).Select(_ => new List<(int Edge, int Vertex)>()).ToArray();

            for (int i = 0; i < edges.Count; i++)
            {
                adjacency[edges[i].A].Add((i, edges[i].B));
                adjacency[edges[i].B].Add((i, edges[i].A));
            }

            var visited = new bool[edges.Count];
            var walk = new List<int> { 0 };
            visit(0);
            return walk;

            void visit(int vertex)
            {
                foreach ((int edge, int neighbour) in adjacency[vertex])
                {
                    if (visited[edge])
                        continue;

                    visited[edge] = true;
                    walk.Add(neighbour);
                    visit(neighbour);
                    walk.Add(vertex);
                }
            }
        }

        private static void normaliseQuaternion(ref double x, ref double y, ref double z, ref double w)
        {
            double length = Math.Sqrt(x * x + y * y + z * z + w * w);

            if (length < 0.000001)
            {
                x = y = z = 0;
                w = 1;
                return;
            }

            x /= length;
            y /= length;
            z /= length;
            w /= length;
        }

        private static Vector3 rotate(Vector3 point, double x, double y, double z, double w)
        {
            double dot = x * point.X + y * point.Y + z * point.Z;
            double crossX = y * point.Z - z * point.Y;
            double crossY = z * point.X - x * point.Z;
            double crossZ = x * point.Y - y * point.X;
            double vectorScale = w * w - x * x - y * y - z * z;

            return new Vector3(
                (float)(2 * dot * x + vectorScale * point.X + 2 * w * crossX),
                (float)(2 * dot * y + vectorScale * point.Y + 2 * w * crossY),
                (float)(2 * dot * z + vectorScale * point.Z + 2 * w * crossZ));
        }

        private static List<Vector2> parametric3D(
            double start,
            double end,
            int samples,
            Func<double, Vector3> evaluate,
            double quaternionX,
            double quaternionY,
            double quaternionZ,
            double quaternionW,
            double perspective,
            bool closed = false)
        {
            normaliseQuaternion(ref quaternionX, ref quaternionY, ref quaternionZ, ref quaternionW);
            perspective = Math.Clamp(perspective, 0, 1);

            return parametric(start, end, samples, t => project3D(evaluate(t), quaternionX, quaternionY, quaternionZ, quaternionW, perspective), closed);
        }

        private static Vector2 project3D(Vector3 point, double quaternionX, double quaternionY, double quaternionZ, double quaternionW, double perspective)
        {
            Vector3 rotated = rotate(point, quaternionX, quaternionY, quaternionZ, quaternionW);
            double depthScale = 1 / (1 - perspective * rotated.Z * 0.45);
            return new Vector2((float)(rotated.X * depthScale), (float)(-rotated.Y * depthScale));
        }

        private static List<Vector2> parametric(double start, double end, int samples, Func<double, Vector2> evaluate, bool closed = false)
        {
            int pointCount = closed ? samples + 1 : samples;
            var result = new List<Vector2>(pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                double progress = i / (double)(samples - (closed ? 0 : 1));
                result.Add(evaluate(start + (end - start) * progress));
            }

            if (closed && result.Count > 0)
                result[^1] = result[0];

            return result;
        }

        private static List<Vector2> functionCurve(int samples, Func<double, double> evaluate)
            => parametric(-1, 1, samples, x => new Vector2((float)x, (float)evaluate(x)));

        private static Vector2 polar(double radius, double angle)
            => new Vector2((float)(radius * Math.Cos(angle)), (float)(radius * Math.Sin(angle)));

        private static Vector2 ellipse(double t, double aspectParameter)
        {
            float aspectRatio = aspect(aspectParameter);
            return new Vector2((float)Math.Cos(t) * aspectRatio, (float)Math.Sin(t));
        }

        private static double sinc(double value) => Math.Abs(value) < 0.000001 ? 1 : Math.Sin(value) / value;

        private static double width(double parameter) => Math.Clamp(parameter / 8, 0.08, 2.5);

        private static double gaussian(double x, double sigma) => Math.Exp(-(x * x) / (2 * sigma * sigma));

        private static double logistic(double value) => 1 / (1 + Math.Exp(-value));

        private static double betaDensity(double x, double alphaParameter, double betaParameter)
        {
            x = Math.Clamp(x, 0.0001, 0.9999);
            double alpha = Math.Clamp(alphaParameter / 2, 0.2, 16);
            double beta = Math.Clamp(betaParameter / 2, 0.2, 16);
            return Math.Pow(x, alpha - 1) * Math.Pow(1 - x, beta - 1);
        }

        private static double gammaDensity(double x, double shapeParameter, double scaleParameter)
        {
            double shape = Math.Clamp(shapeParameter / 4 + 0.25, 0.35, 8.25);
            double scale = Math.Clamp(scaleParameter / 8 + 0.08, 0.15, 4.1);
            return Math.Pow(x, shape - 1) * Math.Exp(-x / scale);
        }

        private static double logNormalDensity(double x, double locationParameter, double spreadParameter)
        {
            double location = Math.Log(Math.Clamp(locationParameter / 6, 0.18, 5.5));
            double spread = Math.Clamp(spreadParameter / 10, 0.12, 3.2);
            double z = Math.Log(x) - location;
            return Math.Exp(-(z * z) / (2 * spread * spread)) / x;
        }

        private static double weibullDensity(double x, double shapeParameter, double scaleParameter)
        {
            double shape = Math.Clamp(shapeParameter / 4 + 0.25, 0.35, 8.25);
            double scale = Math.Clamp(scaleParameter / 4 + 0.2, 0.45, 8.2);
            double scaled = x / scale;
            return shape / scale * Math.Pow(scaled, shape - 1) * Math.Exp(-Math.Pow(scaled, shape));
        }

        private static Vector2 heart(double t)
        {
            double sin = Math.Sin(t);
            return new Vector2(
                (float)(16 * sin * sin * sin),
                (float)-(13 * Math.Cos(t) - 5 * Math.Cos(2 * t) - 2 * Math.Cos(3 * t) - Math.Cos(4 * t)));
        }

        private static Vector2 lemniscate(double t)
        {
            double sin = Math.Sin(t);
            double denominator = 1 + sin * sin;
            return new Vector2(
                (float)(Math.Cos(t) / denominator),
                (float)(sin * Math.Cos(t) / denominator));
        }

        private static float cube(double value) => (float)(value * value * value);

        private static Vector2 superellipse(double t, double exponent)
        {
            exponent = Math.Max(0.2, exponent);
            double cosine = Math.Cos(t);
            double sine = Math.Sin(t);

            return new Vector2(
                signedPower(cosine, 2 / exponent),
                signedPower(sine, 2 / exponent));
        }

        private static float signedPower(double value, double exponent) =>
            (float)(Math.Sign(value) * Math.Pow(Math.Abs(value), exponent));

        private static Vector2 epiHypocycloid(double t, double lobes, bool inside)
        {
            double radius = lobes - 1;

            if (inside)
            {
                return new Vector2(
                    (float)(radius * Math.Cos(t) + Math.Cos(radius * t)),
                    (float)(radius * Math.Sin(t) - Math.Sin(radius * t)));
            }

            radius = lobes + 1;
            return new Vector2(
                (float)(radius * Math.Cos(t) - Math.Cos(radius * t)),
                (float)(radius * Math.Sin(t) - Math.Sin(radius * t)));
        }

        private static Vector2 trochoid(double t, double lobes, double offset, bool inside)
        {
            double rollingRadius = 1;
            double fixedRadius = inside ? lobes : lobes + 1;
            double centreRadius = inside ? fixedRadius - rollingRadius : fixedRadius + rollingRadius;
            double rotation = centreRadius / rollingRadius * t;

            return new Vector2(
                (float)(centreRadius * Math.Cos(t) + (inside ? 1 : -1) * offset * Math.Cos(rotation)),
                (float)(centreRadius * Math.Sin(t) - offset * Math.Sin(rotation)));
        }

        private static List<Vector2> eulerSpiral(int samples, double turns)
        {
            double extent = Math.Sqrt(Math.Tau * turns);
            double step = extent * 2 / (samples - 1);
            var result = new List<Vector2>(samples) { Vector2.Zero };
            Vector2 position = Vector2.Zero;
            double previous = -extent;

            for (int i = 1; i < samples; i++)
            {
                double current = -extent + step * i;
                double midpoint = (previous + current) / 2;
                position += new Vector2((float)(Math.Cos(midpoint * midpoint) * step), (float)(Math.Sin(midpoint * midpoint) * step));
                result.Add(position);
                previous = current;
            }

            return result;
        }

        private static List<Vector2> maurerRose(double petals, double stepDegrees)
        {
            double step = Math.PI / 180 * stepDegrees;
            var result = new List<Vector2>(361);

            for (int i = 0; i <= 360; i++)
            {
                double angle = i * step;
                result.Add(polar(Math.Sin(petals * angle), angle));
            }

            return result;
        }

        private static List<Vector2> harmonograph(int samples, double xFrequency, double yFrequency)
            => parametric(0, Math.Tau * 6, samples, t =>
            {
                double decay = Math.Exp(-0.045 * t);
                return new Vector2(
                    (float)(decay * (Math.Sin(xFrequency * t) + 0.35 * Math.Sin((xFrequency + 1) * t + Math.PI / 2))),
                    (float)(decay * (Math.Sin(yFrequency * t + Math.PI / 3) + 0.35 * Math.Sin((yFrequency + 1) * t))));
            });

        private static Vector3 torusKnot(double t, double majorWinding, double minorWinding)
        {
            double major = Math.Max(1, majorWinding);
            double minor = Math.Max(1, minorWinding);
            double radius = 1.25 + 0.45 * Math.Cos(minor * t);

            return new Vector3(
                (float)(radius * Math.Cos(major * t)),
                (float)(radius * Math.Sin(major * t)),
                (float)(0.45 * Math.Sin(minor * t)));
        }

        private static Vector3 sphericalHelix(double t, double turns, double latitudeWaves)
        {
            double longitude = turns * t;
            double latitude = 0.72 * Math.Sin(latitudeWaves * t);
            double latitudeCosine = Math.Cos(latitude);

            return new Vector3(
                (float)(latitudeCosine * Math.Cos(longitude)),
                (float)Math.Sin(latitude),
                (float)(latitudeCosine * Math.Sin(longitude)));
        }

        private static Vector2 butterfly(double t)
        {
            double radius = Math.Exp(Math.Cos(t)) - 2 * Math.Cos(4 * t) + Math.Pow(Math.Sin(t / 12), 5);
            return polar(radius, t);
        }

        private static Vector2 superformula(double angle, double symmetry, double roundness)
        {
            double exponent = Math.Max(0.2, roundness / 3);
            double first = Math.Pow(Math.Abs(Math.Cos(symmetry * angle / 4)), exponent);
            double second = Math.Pow(Math.Abs(Math.Sin(symmetry * angle / 4)), exponent);
            double radius = Math.Pow(first + second, -1 / exponent);
            return polar(radius, angle);
        }

        public static IReadOnlyList<Vector2> MoveHeadAlongClosedPath(IReadOnlyList<Vector2> source, double position)
        {
            if (source.Count < 3 || Vector2.DistanceSquared(source[0], source[^1]) > 0.001f)
                return source;

            position -= Math.Floor(position);

            if (position <= 0.000001)
                return source;

            double totalLength = 0;

            for (int i = 0; i < source.Count - 1; i++)
                totalLength += Vector2.Distance(source[i], source[i + 1]);

            if (totalLength <= 0.000001)
                return source;

            double targetDistance = totalLength * position;
            double traversed = 0;

            for (int i = 0; i < source.Count - 1; i++)
            {
                double segmentLength = Vector2.Distance(source[i], source[i + 1]);

                if (traversed + segmentLength <= targetDistance)
                {
                    traversed += segmentLength;
                    continue;
                }

                float progress = segmentLength <= 0.000001
                    ? 0
                    : (float)((targetDistance - traversed) / segmentLength);
                Vector2 head = Vector2.Lerp(source[i], source[i + 1], progress);
                var result = new List<Vector2>(source.Count + 1) { head };

                for (int j = i + 1; j < source.Count; j++)
                    result.Add(source[j]);

                for (int j = 1; j <= i; j++)
                    result.Add(source[j]);

                result.Add(head);
                return result;
            }

            return source;
        }

        public static IReadOnlyList<Vector2> TransformObject(IReadOnlyList<Vector2> source, double scale, double rotationDegrees, double offsetX, double offsetY)
        {
            var playfieldCentre = new Vector2(256, 192);
            var offset = new Vector2((float)offsetX, (float)offsetY);
            double radians = Math.PI / 180 * rotationDegrees;
            float cosine = (float)Math.Cos(radians);
            float sine = (float)Math.Sin(radians);

            return source.Select(point =>
            {
                Vector2 relative = (point - playfieldCentre) * (float)scale;
                var rotated = new Vector2(
                    relative.X * cosine - relative.Y * sine,
                    relative.X * sine + relative.Y * cosine);

                return playfieldCentre + rotated + offset;
            }).ToArray();
        }

        private static List<Vector2> polygon(int vertices)
        {
            var result = new List<Vector2>(vertices + 1);

            for (int i = 0; i <= vertices; i++)
            {
                float angle = -MathF.PI / 2 + i * MathF.Tau / vertices;
                result.Add(new Vector2(MathF.Cos(angle), MathF.Sin(angle)));
            }

            return result;
        }

        private static List<Vector2> mathGlyph(int codepoint, int samples)
            => MathSymbolFontPath.Create(codepoint, samples) ?? new List<Vector2>();

        private static int symbolCodepoint(ParametricSliderShape shape, string symbolText)
        {
            string value = string.IsNullOrWhiteSpace(symbolText) ? defaultSymbolText(shape) : symbolText.Trim();
            int directCodepoint = firstCodepoint(value, 'A');

            return shape switch
            {
                ParametricSliderShape.SymbolLatinUppercase => char.ToUpperInvariant(firstAsciiLetter(value, 'A')),
                ParametricSliderShape.SymbolLatinLowercase => char.ToLowerInvariant(firstAsciiLetter(value, 'a')),
                ParametricSliderShape.SymbolItalicUppercase => 0x1D434 + letterIndex(value, 'A'),
                ParametricSliderShape.SymbolItalicLowercase => italicLowercaseCodepoint(value),
                ParametricSliderShape.SymbolBoldUppercase => 0x1D400 + letterIndex(value, 'A'),
                ParametricSliderShape.SymbolBoldLowercase => 0x1D41A + letterIndex(value, 'a'),
                ParametricSliderShape.SymbolScriptUppercase => scriptUppercaseCodepoint(value),
                ParametricSliderShape.SymbolScriptLowercase => scriptLowercaseCodepoint(value),
                ParametricSliderShape.SymbolFrakturUppercase => frakturUppercaseCodepoint(value),
                ParametricSliderShape.SymbolFrakturLowercase => 0x1D51E + lowercaseLetterIndex(value, 'x'),
                ParametricSliderShape.SymbolBoldItalicUppercase => 0x1D468 + letterIndex(value, 'A'),
                ParametricSliderShape.SymbolBoldItalicLowercase => 0x1D482 + lowercaseLetterIndex(value, 'x'),
                ParametricSliderShape.SymbolSansSerifUppercase => 0x1D5A0 + letterIndex(value, 'A'),
                ParametricSliderShape.SymbolSansSerifLowercase => 0x1D5BA + lowercaseLetterIndex(value, 'x'),
                ParametricSliderShape.SymbolSansSerifBoldUppercase => 0x1D5D4 + letterIndex(value, 'A'),
                ParametricSliderShape.SymbolSansSerifBoldLowercase => 0x1D5EE + lowercaseLetterIndex(value, 'x'),
                ParametricSliderShape.SymbolMonospaceUppercase => 0x1D670 + letterIndex(value, 'A'),
                ParametricSliderShape.SymbolMonospaceLowercase => 0x1D68A + lowercaseLetterIndex(value, 'x'),
                ParametricSliderShape.SymbolBlackboardLetters => blackboardCodepoint(value),
                ParametricSliderShape.SymbolGreekUppercase => greekUppercaseCodepoint(value),
                ParametricSliderShape.SymbolGreekLowercase => greekLowercaseCodepoint(value),
                ParametricSliderShape.SymbolHebrewLetters => hebrewCodepoint(value),
                ParametricSliderShape.SymbolNamedMathLetters => lookupSymbol(value, namedMathLetterSymbols, 0x2113),
                ParametricSliderShape.SymbolCalculus => lookupSymbol(value, calculusSymbols, 0x222B),
                ParametricSliderShape.SymbolOperators => lookupSymbol(value, operatorSymbols, 0x00D7),
                ParametricSliderShape.SymbolRelations => lookupSymbol(value, relationSymbols, 0x2248),
                ParametricSliderShape.SymbolSetLogic => lookupSymbol(value, setLogicSymbols, 0x2208),
                ParametricSliderShape.SymbolArrows => lookupSymbol(value, arrowSymbols, 0x2192),
                ParametricSliderShape.SymbolDelimiters => lookupSymbol(value, delimiterSymbols, 0x27E8),
                _ => directCodepoint
            };
        }

        private static string defaultSymbolText(ParametricSliderShape shape) => shape switch
        {
            ParametricSliderShape.SymbolLatinLowercase => "a",
            ParametricSliderShape.SymbolItalicLowercase => "x",
            ParametricSliderShape.SymbolBoldLowercase => "x",
            ParametricSliderShape.SymbolScriptLowercase => "x",
            ParametricSliderShape.SymbolFrakturLowercase => "x",
            ParametricSliderShape.SymbolBoldItalicLowercase => "x",
            ParametricSliderShape.SymbolSansSerifLowercase => "x",
            ParametricSliderShape.SymbolSansSerifBoldLowercase => "x",
            ParametricSliderShape.SymbolMonospaceLowercase => "x",
            ParametricSliderShape.SymbolBlackboardLetters => "R",
            ParametricSliderShape.SymbolGreekUppercase => "Omega",
            ParametricSliderShape.SymbolGreekLowercase => "alpha",
            ParametricSliderShape.SymbolHebrewLetters => "aleph",
            ParametricSliderShape.SymbolNamedMathLetters => "ell",
            ParametricSliderShape.SymbolCalculus => "int",
            ParametricSliderShape.SymbolOperators => "times",
            ParametricSliderShape.SymbolRelations => "approx",
            ParametricSliderShape.SymbolSetLogic => "in",
            ParametricSliderShape.SymbolArrows => "to",
            ParametricSliderShape.SymbolDelimiters => "angle",
            _ => "A"
        };

        private static int letterIndex(string value, char fallback)
        {
            char letter = char.ToUpperInvariant(firstAsciiLetter(value, fallback));
            return Math.Clamp(letter - 'A', 0, 25);
        }

        private static int lowercaseLetterIndex(string value, char fallback)
        {
            char letter = char.ToLowerInvariant(firstAsciiLetter(value, fallback));
            return Math.Clamp(letter - 'a', 0, 25);
        }

        private static int italicLowercaseCodepoint(string value)
        {
            int index = Math.Clamp(char.ToLowerInvariant(firstAsciiLetter(value, 'x')) - 'a', 0, 25);

            if (index == 7)
                return 0x210E;

            return 0x1D44E + (index > 7 ? index - 1 : index);
        }

        private static int scriptUppercaseCodepoint(string value)
        {
            char letter = char.ToUpperInvariant(firstAsciiLetter(value, 'A'));
            return letter switch
            {
                'A' => 0x1D49C,
                'B' => 0x212C,
                'C' => 0x1D49E,
                'D' => 0x1D49F,
                'E' => 0x2130,
                'F' => 0x2131,
                'G' => 0x1D4A2,
                'H' => 0x210B,
                'I' => 0x2110,
                'J' => 0x1D4A5,
                'K' => 0x1D4A6,
                'L' => 0x2112,
                'M' => 0x2133,
                'N' => 0x1D4A9,
                'O' => 0x1D4AA,
                'P' => 0x1D4AB,
                'Q' => 0x1D4AC,
                'R' => 0x211B,
                'S' => 0x1D4AE,
                'T' => 0x1D4AF,
                'U' => 0x1D4B0,
                'V' => 0x1D4B1,
                'W' => 0x1D4B2,
                'X' => 0x1D4B3,
                'Y' => 0x1D4B4,
                'Z' => 0x1D4B5,
                _ => 0x1D49C
            };
        }

        private static int scriptLowercaseCodepoint(string value)
        {
            char letter = char.ToLowerInvariant(firstAsciiLetter(value, 'x'));
            return letter switch
            {
                'a' => 0x1D4B6,
                'b' => 0x1D4B7,
                'c' => 0x1D4B8,
                'd' => 0x1D4B9,
                'e' => 0x212F,
                'f' => 0x1D4BB,
                'g' => 0x210A,
                'h' => 0x1D4BD,
                'i' => 0x1D4BE,
                'j' => 0x1D4BF,
                'k' => 0x1D4C0,
                'l' => 0x1D4C1,
                'm' => 0x1D4C2,
                'n' => 0x1D4C3,
                'o' => 0x2134,
                'p' => 0x1D4C5,
                'q' => 0x1D4C6,
                'r' => 0x1D4C7,
                's' => 0x1D4C8,
                't' => 0x1D4C9,
                'u' => 0x1D4CA,
                'v' => 0x1D4CB,
                'w' => 0x1D4CC,
                'x' => 0x1D4CD,
                'y' => 0x1D4CE,
                'z' => 0x1D4CF,
                _ => 0x1D4CD
            };
        }

        private static int frakturUppercaseCodepoint(string value)
        {
            char letter = char.ToUpperInvariant(firstAsciiLetter(value, 'A'));
            return letter switch
            {
                'A' => 0x1D504,
                'B' => 0x1D505,
                'C' => 0x212D,
                'D' => 0x1D507,
                'E' => 0x1D508,
                'F' => 0x1D509,
                'G' => 0x1D50A,
                'H' => 0x210C,
                'I' => 0x2111,
                'J' => 0x1D50D,
                'K' => 0x1D50E,
                'L' => 0x1D50F,
                'M' => 0x1D510,
                'N' => 0x1D511,
                'O' => 0x1D512,
                'P' => 0x1D513,
                'Q' => 0x1D514,
                'R' => 0x211C,
                'S' => 0x1D516,
                'T' => 0x1D517,
                'U' => 0x1D518,
                'V' => 0x1D519,
                'W' => 0x1D51A,
                'X' => 0x1D51B,
                'Y' => 0x1D51C,
                'Z' => 0x2128,
                _ => 0x1D504
            };
        }

        private static int blackboardCodepoint(string value)
        {
            int directCodepoint = firstCodepoint(value, 'R');

            if (directCodepoint > 0x7f)
                return directCodepoint;

            char rawLetter = firstAsciiLetter(value, 'R');

            if (rawLetter >= 'a' && rawLetter <= 'z')
                return 0x1D552 + Math.Clamp(rawLetter - 'a', 0, 25);

            char letter = char.ToUpperInvariant(rawLetter);
            return letter switch
            {
                'A' => 0x1D538,
                'B' => 0x1D539,
                'C' => 0x2102,
                'D' => 0x1D53B,
                'E' => 0x1D53C,
                'F' => 0x1D53D,
                'G' => 0x1D53E,
                'H' => 0x210D,
                'I' => 0x1D540,
                'J' => 0x1D541,
                'K' => 0x1D542,
                'L' => 0x1D543,
                'M' => 0x1D544,
                'N' => 0x2115,
                'O' => 0x1D546,
                'P' => 0x2119,
                'Q' => 0x211A,
                'R' => 0x211D,
                'S' => 0x1D54A,
                'T' => 0x1D54B,
                'U' => 0x1D54C,
                'V' => 0x1D54D,
                'W' => 0x1D54E,
                'X' => 0x1D54F,
                'Y' => 0x1D550,
                'Z' => 0x2124,
                _ => 0x211D
            };
        }

        private static int greekUppercaseCodepoint(string value)
        {
            string key = normaliseSymbolKey(value);

            if (greekUppercase.TryGetValue(key, out int codepoint))
                return codepoint;

            int directCodepoint = firstCodepoint(value, 0x03A9);

            if (directCodepoint > 0x7f)
                return directCodepoint <= char.MaxValue ? char.ToUpperInvariant((char)directCodepoint) : directCodepoint;

            char letter = char.ToUpperInvariant(firstAsciiLetter(value, 'O'));
            return greekUppercaseLetters.TryGetValue(letter, out codepoint) ? codepoint : 0x03A9;
        }

        private static int greekLowercaseCodepoint(string value)
        {
            string key = normaliseSymbolKey(value);

            if (greekLowercase.TryGetValue(key, out int codepoint))
                return codepoint;

            int directCodepoint = firstCodepoint(value, 0x03B1);

            if (directCodepoint > 0x7f)
                return directCodepoint <= char.MaxValue ? char.ToLowerInvariant((char)directCodepoint) : directCodepoint;

            char letter = char.ToLowerInvariant(firstAsciiLetter(value, 'a'));
            return greekLowercaseLetters.TryGetValue(letter, out codepoint) ? codepoint : 0x03B1;
        }

        private static int hebrewCodepoint(string value)
        {
            string key = normaliseSymbolKey(value);

            if (hebrewSymbols.TryGetValue(key, out int codepoint))
                return codepoint;

            return firstCodepoint(value, 0x2135);
        }

        private static int lookupSymbol(string value, IReadOnlyDictionary<string, int> table, int fallback)
        {
            string key = normaliseSymbolKey(value);

            if (table.TryGetValue(key, out int codepoint))
                return codepoint;

            return firstCodepoint(value, fallback);
        }

        private static int firstCodepoint(string value, int fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            string trimmed = value.Trim();

            if (trimmed.Length >= 2 && char.IsHighSurrogate(trimmed[0]) && char.IsLowSurrogate(trimmed[1]))
                return char.ConvertToUtf32(trimmed[0], trimmed[1]);

            return trimmed[0];
        }

        private static char firstAsciiLetter(string value, char fallback)
        {
            foreach (char c in value)
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    return c;
            }

            return fallback;
        }

        private static string normaliseSymbolKey(string value)
            => new string(value.Trim().Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

        private static readonly IReadOnlyDictionary<char, int> greekUppercaseLetters = new Dictionary<char, int>
        {
            ['A'] = 0x0391, ['B'] = 0x0392, ['G'] = 0x0393, ['D'] = 0x0394, ['E'] = 0x0395, ['Z'] = 0x0396,
            ['H'] = 0x0397, ['T'] = 0x0398, ['I'] = 0x0399, ['K'] = 0x039A, ['L'] = 0x039B, ['M'] = 0x039C,
            ['N'] = 0x039D, ['X'] = 0x039E, ['O'] = 0x039F, ['P'] = 0x03A0, ['R'] = 0x03A1, ['S'] = 0x03A3,
            ['U'] = 0x03A5, ['F'] = 0x03A6, ['C'] = 0x03A7, ['Y'] = 0x03A8, ['W'] = 0x03A9
        };

        private static readonly IReadOnlyDictionary<char, int> greekLowercaseLetters = new Dictionary<char, int>
        {
            ['a'] = 0x03B1, ['b'] = 0x03B2, ['g'] = 0x03B3, ['d'] = 0x03B4, ['e'] = 0x03B5, ['z'] = 0x03B6,
            ['h'] = 0x03B7, ['t'] = 0x03B8, ['i'] = 0x03B9, ['k'] = 0x03BA, ['l'] = 0x03BB, ['m'] = 0x03BC,
            ['n'] = 0x03BD, ['x'] = 0x03BE, ['o'] = 0x03BF, ['p'] = 0x03C0, ['r'] = 0x03C1, ['s'] = 0x03C3,
            ['u'] = 0x03C5, ['f'] = 0x03C6, ['c'] = 0x03C7, ['y'] = 0x03C8, ['w'] = 0x03C9
        };

        private static readonly IReadOnlyDictionary<string, int> greekUppercase = new Dictionary<string, int>
        {
            ["alpha"] = 0x0391, ["beta"] = 0x0392, ["gamma"] = 0x0393, ["delta"] = 0x0394, ["epsilon"] = 0x0395,
            ["zeta"] = 0x0396, ["eta"] = 0x0397, ["theta"] = 0x0398, ["iota"] = 0x0399, ["kappa"] = 0x039A,
            ["lambda"] = 0x039B, ["mu"] = 0x039C, ["nu"] = 0x039D, ["xi"] = 0x039E, ["omicron"] = 0x039F,
            ["pi"] = 0x03A0, ["rho"] = 0x03A1, ["sigma"] = 0x03A3, ["tau"] = 0x03A4, ["upsilon"] = 0x03A5,
            ["phi"] = 0x03A6, ["chi"] = 0x03A7, ["psi"] = 0x03A8, ["omega"] = 0x03A9
        };

        private static readonly IReadOnlyDictionary<string, int> greekLowercase = new Dictionary<string, int>
        {
            ["alpha"] = 0x03B1, ["beta"] = 0x03B2, ["gamma"] = 0x03B3, ["delta"] = 0x03B4, ["epsilon"] = 0x03B5,
            ["zeta"] = 0x03B6, ["eta"] = 0x03B7, ["theta"] = 0x03B8, ["iota"] = 0x03B9, ["kappa"] = 0x03BA,
            ["lambda"] = 0x03BB, ["mu"] = 0x03BC, ["nu"] = 0x03BD, ["xi"] = 0x03BE, ["omicron"] = 0x03BF,
            ["pi"] = 0x03C0, ["rho"] = 0x03C1, ["sigma"] = 0x03C3, ["finalsigma"] = 0x03C2, ["tau"] = 0x03C4,
            ["upsilon"] = 0x03C5, ["phi"] = 0x03C6, ["varphi"] = 0x03D5, ["chi"] = 0x03C7, ["psi"] = 0x03C8, ["omega"] = 0x03C9
        };

        private static readonly IReadOnlyDictionary<string, int> hebrewSymbols = new Dictionary<string, int>
        {
            ["aleph"] = 0x2135, ["beth"] = 0x2136, ["bet"] = 0x2136, ["gimel"] = 0x2137, ["gimmel"] = 0x2137, ["daleth"] = 0x2138, ["dalet"] = 0x2138,
            ["he"] = 0x05D4, ["vav"] = 0x05D5, ["zayin"] = 0x05D6, ["chet"] = 0x05D7, ["tet"] = 0x05D8, ["yod"] = 0x05D9,
            ["kaf"] = 0x05DB, ["lamed"] = 0x05DC, ["mem"] = 0x05DE, ["nun"] = 0x05E0, ["samekh"] = 0x05E1, ["ayin"] = 0x05E2,
            ["pe"] = 0x05E4, ["tsadi"] = 0x05E6, ["qof"] = 0x05E7, ["resh"] = 0x05E8, ["shin"] = 0x05E9, ["tav"] = 0x05EA
        };

        private static readonly IReadOnlyDictionary<string, int> namedMathLetterSymbols = new Dictionary<string, int>
        {
            ["ell"] = 0x2113, ["hbar"] = 0x210F, ["hslash"] = 0x210F, ["planck"] = 0x210E, ["weierstrass"] = 0x2118, ["wp"] = 0x2118,
            ["real"] = 0x211C, ["re"] = 0x211C, ["imaginary"] = 0x2111, ["imag"] = 0x2111, ["im"] = 0x2111,
            ["angstrom"] = 0x212B, ["kelvin"] = 0x212A, ["ohm"] = 0x2126, ["mho"] = 0x2127,
            ["prime"] = 0x2032, ["doubleprime"] = 0x2033, ["tripleprime"] = 0x2034, ["degree"] = 0x00B0
        };

        private static readonly IReadOnlyDictionary<string, int> calculusSymbols = new Dictionary<string, int>
        {
            ["int"] = 0x222B, ["integral"] = 0x222B, ["iint"] = 0x222C, ["iiint"] = 0x222D, ["oint"] = 0x222E,
            ["sum"] = 0x2211, ["sigma"] = 0x2211, ["prod"] = 0x220F, ["product"] = 0x220F, ["coprod"] = 0x2210,
            ["partial"] = 0x2202, ["nabla"] = 0x2207, ["grad"] = 0x2207, ["sqrt"] = 0x221A, ["infty"] = 0x221E, ["infinity"] = 0x221E
        };

        private static readonly IReadOnlyDictionary<string, int> operatorSymbols = new Dictionary<string, int>
        {
            ["plus"] = 0x002B, ["minus"] = 0x2212, ["times"] = 0x00D7, ["div"] = 0x00F7, ["pm"] = 0x00B1, ["mp"] = 0x2213,
            ["cdot"] = 0x22C5, ["star"] = 0x22C6, ["circ"] = 0x2218, ["bullet"] = 0x2219, ["oplus"] = 0x2295, ["otimes"] = 0x2297,
            ["wedge"] = 0x2227, ["vee"] = 0x2228, ["angle"] = 0x2220, ["parallel"] = 0x2225, ["perp"] = 0x22A5
        };

        private static readonly IReadOnlyDictionary<string, int> relationSymbols = new Dictionary<string, int>
        {
            ["eq"] = 0x003D, ["neq"] = 0x2260, ["ne"] = 0x2260, ["approx"] = 0x2248, ["sim"] = 0x223C, ["simeq"] = 0x2243,
            ["cong"] = 0x2245, ["equiv"] = 0x2261, ["le"] = 0x2264, ["ge"] = 0x2265, ["ll"] = 0x226A, ["gg"] = 0x226B,
            ["propto"] = 0x221D, ["models"] = 0x22A8, ["vdash"] = 0x22A2, ["dashv"] = 0x22A3
        };

        private static readonly IReadOnlyDictionary<string, int> setLogicSymbols = new Dictionary<string, int>
        {
            ["in"] = 0x2208, ["notin"] = 0x2209, ["ni"] = 0x220B, ["empty"] = 0x2205, ["emptyset"] = 0x2205,
            ["union"] = 0x222A, ["cup"] = 0x222A, ["intersection"] = 0x2229, ["cap"] = 0x2229, ["subset"] = 0x2282, ["superset"] = 0x2283,
            ["subseteq"] = 0x2286, ["superseteq"] = 0x2287, ["forall"] = 0x2200, ["exists"] = 0x2203, ["nexists"] = 0x2204,
            ["and"] = 0x2227, ["or"] = 0x2228, ["not"] = 0x00AC, ["therefore"] = 0x2234, ["because"] = 0x2235
        };

        private static readonly IReadOnlyDictionary<string, int> arrowSymbols = new Dictionary<string, int>
        {
            ["left"] = 0x2190, ["up"] = 0x2191, ["to"] = 0x2192, ["right"] = 0x2192, ["down"] = 0x2193, ["leftright"] = 0x2194,
            ["mapsto"] = 0x21A6, ["doubleleft"] = 0x21D0, ["doubleright"] = 0x21D2, ["doubleleftright"] = 0x21D4,
            ["hookleft"] = 0x21A9, ["hookright"] = 0x21AA, ["uparrow"] = 0x2191, ["downarrow"] = 0x2193, ["ne"] = 0x2197, ["se"] = 0x2198, ["sw"] = 0x2199, ["nw"] = 0x2196
        };

        private static readonly IReadOnlyDictionary<string, int> delimiterSymbols = new Dictionary<string, int>
        {
            ["leftparen"] = 0x0028, ["rightparen"] = 0x0029, ["leftbracket"] = 0x005B, ["rightbracket"] = 0x005D,
            ["leftbrace"] = 0x007B, ["rightbrace"] = 0x007D, ["angle"] = 0x27E8, ["leftangle"] = 0x27E8, ["rightangle"] = 0x27E9,
            ["floor"] = 0x230A, ["leftfloor"] = 0x230A, ["rightfloor"] = 0x230B, ["ceil"] = 0x2308, ["leftceil"] = 0x2308, ["rightceil"] = 0x2309,
            ["vertical"] = 0x007C, ["vert"] = 0x007C, ["doublevertical"] = 0x2016, ["norm"] = 0x2016
        };

        private static List<Vector2> symbolPath(params Vector2[] points) => points.ToList();

        private static void addPath(List<Vector2> target, IEnumerable<Vector2> points)
        {
            foreach (Vector2 point in points)
            {
                if (target.Count == 0 || Vector2.DistanceSquared(target[^1], point) > 0.000001f)
                    target.Add(point);
            }
        }

        private static List<Vector2> ellipseArc(Vector2 centre, float radiusX, float radiusY, double start, double end, int samples, bool closed = false)
        {
            int segmentCount = Math.Max(2, samples);
            int pointCount = closed ? segmentCount + 1 : segmentCount;
            var result = new List<Vector2>(pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                double progress = i / (double)(segmentCount - (closed ? 0 : 1));
                double angle = start + (end - start) * progress;
                result.Add(new Vector2(
                    centre.X + radiusX * (float)Math.Cos(angle),
                    centre.Y + radiusY * (float)Math.Sin(angle)));
            }

            if (closed && result.Count > 0)
                result[^1] = result[0];

            return result;
        }

        private static List<Vector2> cubicBezier(Vector2 first, Vector2 second, Vector2 third, Vector2 fourth, int samples)
        {
            int pointCount = Math.Max(2, samples);
            var result = new List<Vector2>(pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (float)(pointCount - 1);
                float inverse = 1 - t;
                result.Add(
                    first * (inverse * inverse * inverse)
                    + second * (3 * inverse * inverse * t)
                    + third * (3 * inverse * t * t)
                    + fourth * (t * t * t));
            }

            return result;
        }

        private static List<Vector2> blackboardN() => symbolPath(
            new Vector2(-0.9f, 1),
            new Vector2(-0.9f, -1),
            new Vector2(-0.62f, -1),
            new Vector2(-0.62f, 0.55f),
            new Vector2(0.62f, 1),
            new Vector2(0.62f, -1),
            new Vector2(0.9f, -1),
            new Vector2(0.9f, 1),
            new Vector2(0.62f, 1),
            new Vector2(0.62f, -0.55f),
            new Vector2(-0.62f, -1),
            new Vector2(-0.62f, 1),
            new Vector2(-0.9f, 1));

        private static List<Vector2> blackboardZ() => symbolPath(
            new Vector2(-0.95f, -1),
            new Vector2(0.95f, -1),
            new Vector2(0.95f, -0.72f),
            new Vector2(-0.45f, 0.72f),
            new Vector2(0.95f, 0.72f),
            new Vector2(0.95f, 1),
            new Vector2(-0.95f, 1),
            new Vector2(-0.95f, 0.72f),
            new Vector2(0.45f, -0.72f),
            new Vector2(-0.95f, -0.72f),
            new Vector2(-0.95f, -1));

        private static List<Vector2> blackboardQ(int samples)
        {
            var result = ellipseArc(Vector2.Zero, 0.82f, 1, 0, Math.Tau, samples, true);
            addPath(result, ellipseArc(new Vector2(-0.12f, 0), 0.56f, 0.76f, -Math.PI * 0.82, Math.PI * 0.82, samples / 2, false));
            addPath(result, symbolPath(new Vector2(0.28f, 0.42f), new Vector2(0.9f, 1.05f)));
            return result;
        }

        private static List<Vector2> blackboardR(int samples)
        {
            var result = symbolPath(
                new Vector2(-0.85f, 1),
                new Vector2(-0.85f, -1),
                new Vector2(-0.56f, -1),
                new Vector2(-0.56f, 1),
                new Vector2(-0.85f, 1),
                new Vector2(-0.38f, -1));

            addPath(result, ellipseArc(new Vector2(-0.15f, -0.42f), 0.78f, 0.58f, -Math.PI * 0.95, Math.PI * 0.58, samples / 2, false));
            addPath(result, symbolPath(new Vector2(-0.42f, 0.05f), new Vector2(0.88f, 1)));
            return result;
        }

        private static List<Vector2> blackboardC(int samples)
        {
            var result = ellipseArc(Vector2.Zero, 0.9f, 1, Math.PI * 0.18, Math.PI * 1.82, samples, false);
            addPath(result, ellipseArc(new Vector2(0.08f, 0), 0.58f, 0.68f, Math.PI * 0.22, Math.PI * 1.78, samples / 2, false));
            return result;
        }

        private static List<Vector2> aleph(int samples)
        {
            var result = cubicBezier(
                new Vector2(-0.72f, -1),
                new Vector2(-0.18f, -0.55f),
                new Vector2(0.25f, 0.35f),
                new Vector2(0.75f, 1),
                samples / 3);
            addPath(result, cubicBezier(
                new Vector2(0.62f, -0.92f),
                new Vector2(0.15f, -0.45f),
                new Vector2(-0.35f, 0.45f),
                new Vector2(-0.76f, 1),
                samples / 3));
            addPath(result, cubicBezier(
                new Vector2(-0.62f, -0.25f),
                new Vector2(-0.2f, -0.58f),
                new Vector2(0.32f, -0.58f),
                new Vector2(0.66f, -0.1f),
                samples / 4));
            return result;
        }

        private static List<Vector2> beth(int samples)
        {
            var result = symbolPath(new Vector2(-0.75f, -1), new Vector2(-0.75f, 1), new Vector2(0.72f, 1));
            addPath(result, cubicBezier(
                new Vector2(-0.72f, -1),
                new Vector2(0.62f, -1),
                new Vector2(0.78f, -0.35f),
                new Vector2(-0.2f, -0.08f),
                samples / 3));
            addPath(result, cubicBezier(
                new Vector2(-0.2f, -0.08f),
                new Vector2(0.9f, 0.08f),
                new Vector2(0.82f, 0.78f),
                new Vector2(-0.75f, 1),
                samples / 3));
            return result;
        }

        private static List<Vector2> infinity(int samples)
            => parametric(0, Math.Tau, samples, t =>
            {
                double denominator = 1 + Math.Sin(t) * Math.Sin(t);
                return new Vector2((float)(Math.Cos(t) / denominator), (float)(Math.Sin(t) * Math.Cos(t) / denominator));
            }, true);

        private static List<Vector2> integral(int samples)
            => cubicBezier(
                new Vector2(0.48f, -1.05f),
                new Vector2(-0.7f, -0.92f),
                new Vector2(0.7f, 0.92f),
                new Vector2(-0.48f, 1.05f),
                samples);

        private static List<Vector2> contourIntegral(int samples)
        {
            var result = ellipseArc(Vector2.Zero, 0.72f, 0.72f, 0, Math.Tau, samples / 2, true);
            addPath(result, integral(samples / 2));
            return result;
        }

        private static List<Vector2> summationSigma() => symbolPath(
            new Vector2(0.9f, -1),
            new Vector2(-0.9f, -1),
            new Vector2(0.08f, 0),
            new Vector2(-0.9f, 1),
            new Vector2(0.9f, 1),
            new Vector2(0.72f, 0.68f),
            new Vector2(-0.24f, 0.68f),
            new Vector2(0.42f, 0),
            new Vector2(-0.24f, -0.68f),
            new Vector2(0.72f, -0.68f),
            new Vector2(0.9f, -1));

        private static List<Vector2> productPi() => symbolPath(
            new Vector2(-0.88f, -1),
            new Vector2(0.88f, -1),
            new Vector2(0.88f, -0.68f),
            new Vector2(0.62f, -0.68f),
            new Vector2(0.62f, 1),
            new Vector2(0.28f, 1),
            new Vector2(0.28f, -0.68f),
            new Vector2(-0.28f, -0.68f),
            new Vector2(-0.28f, 1),
            new Vector2(-0.62f, 1),
            new Vector2(-0.62f, -0.68f),
            new Vector2(-0.88f, -0.68f),
            new Vector2(-0.88f, -1));

        private static List<Vector2> partialDerivative(int samples)
        {
            var result = ellipseArc(new Vector2(0.04f, 0.18f), 0.72f, 0.72f, -Math.PI * 0.15, Math.Tau * 0.9, samples, false);
            addPath(result, cubicBezier(
                new Vector2(0.48f, -1),
                new Vector2(-0.08f, -0.86f),
                new Vector2(-0.08f, -0.16f),
                new Vector2(0.26f, 0.12f),
                samples / 3));
            return result;
        }

        private static List<Vector2> nabla() => symbolPath(
            new Vector2(-0.9f, -1),
            new Vector2(0.9f, -1),
            new Vector2(0, 1),
            new Vector2(-0.9f, -1));

        private static List<Vector2> forAll() => symbolPath(
            new Vector2(-0.86f, 1),
            new Vector2(0, -1),
            new Vector2(0.86f, 1),
            new Vector2(0.54f, 0.22f),
            new Vector2(-0.54f, 0.22f));

        private static List<Vector2> exists() => symbolPath(
            new Vector2(-0.82f, -1),
            new Vector2(0.82f, -1),
            new Vector2(0.82f, 1),
            new Vector2(-0.82f, 1),
            new Vector2(-0.82f, 0.68f),
            new Vector2(0.46f, 0.68f),
            new Vector2(0.46f, 0),
            new Vector2(-0.54f, 0),
            new Vector2(0.46f, 0),
            new Vector2(0.46f, -0.68f),
            new Vector2(-0.82f, -0.68f));

        private static List<Vector2> emptySet(int samples)
        {
            var result = ellipseArc(Vector2.Zero, 0.78f, 1, 0, Math.Tau, samples, true);
            addPath(result, symbolPath(new Vector2(-0.78f, 1), new Vector2(0.78f, -1)));
            return result;
        }

        private static List<Vector2> elementOf(int samples)
        {
            var result = ellipseArc(new Vector2(0.2f, 0), 0.72f, 0.82f, Math.PI * 0.34, Math.PI * 1.66, samples, false);
            addPath(result, symbolPath(new Vector2(-0.52f, 0), new Vector2(0.6f, 0)));
            return result;
        }

        private static List<Vector2> subset(int samples)
        {
            var result = ellipseArc(new Vector2(0.16f, -0.18f), 0.78f, 0.58f, Math.PI * 0.22, Math.PI * 1.78, samples, false);
            addPath(result, symbolPath(new Vector2(-0.72f, 0.72f), new Vector2(0.72f, 0.72f)));
            return result;
        }

        private static List<Vector2> superset(int samples)
        {
            var result = ellipseArc(new Vector2(-0.16f, -0.18f), 0.78f, 0.58f, -Math.PI * 0.78, Math.PI * 0.78, samples, false);
            addPath(result, symbolPath(new Vector2(-0.72f, 0.72f), new Vector2(0.72f, 0.72f)));
            return result;
        }

        private static List<Vector2> union(int samples)
            => ellipseArc(Vector2.Zero, 0.78f, 0.95f, 0, Math.PI, samples, false);

        private static List<Vector2> intersection(int samples)
            => ellipseArc(Vector2.Zero, 0.78f, 0.95f, Math.PI, Math.Tau, samples, false);

        private static List<Vector2> logicalAnd() => symbolPath(
            new Vector2(-0.82f, 1),
            new Vector2(0, -1),
            new Vector2(0.82f, 1));

        private static List<Vector2> logicalOr() => symbolPath(
            new Vector2(-0.82f, -1),
            new Vector2(0, 1),
            new Vector2(0.82f, -1));

        private static List<Vector2> approximatelyEqual(int samples)
        {
            var result = parametric(-1, 1, samples / 2, x => new Vector2((float)x, (float)(-0.28 + 0.12 * Math.Sin(Math.PI * (x + 1)))));
            addPath(result, parametric(-1, 1, samples / 2, x => new Vector2((float)x, (float)(0.28 + 0.12 * Math.Sin(Math.PI * (x + 1))))));
            return result;
        }

        private static List<Vector2> proportionalTo(int samples)
        {
            var result = ellipseArc(new Vector2(-0.32f, 0), 0.42f, 0.58f, 0, Math.Tau, samples / 2, true);
            addPath(result, ellipseArc(new Vector2(0.42f, 0), 0.54f, 0.58f, Math.PI, -Math.PI, samples / 2, false));
            return result;
        }

        private static List<Vector2> therefore(int samples)
        {
            var result = ellipseArc(new Vector2(0, -0.68f), 0.11f, 0.11f, 0, Math.Tau, samples / 3, true);
            addPath(result, ellipseArc(new Vector2(-0.48f, 0.48f), 0.11f, 0.11f, 0, Math.Tau, samples / 3, true));
            addPath(result, ellipseArc(new Vector2(0.48f, 0.48f), 0.11f, 0.11f, 0, Math.Tau, samples / 3, true));
            return result;
        }

        private static List<Vector2> because(int samples)
        {
            var result = ellipseArc(new Vector2(-0.48f, -0.48f), 0.11f, 0.11f, 0, Math.Tau, samples / 3, true);
            addPath(result, ellipseArc(new Vector2(0.48f, -0.48f), 0.11f, 0.11f, 0, Math.Tau, samples / 3, true));
            addPath(result, ellipseArc(new Vector2(0, 0.68f), 0.11f, 0.11f, 0, Math.Tau, samples / 3, true));
            return result;
        }

        private static List<Vector2> rectangle(double aspectParameter)
        {
            float halfWidth = aspect(aspectParameter);
            return polygonFromPoints(
                new Vector2(-halfWidth, -1),
                new Vector2(halfWidth, -1),
                new Vector2(halfWidth, 1),
                new Vector2(-halfWidth, 1));
        }

        private static List<Vector2> parallelogram(double aspectParameter, double skewParameter)
        {
            float halfWidth = aspect(aspectParameter);
            float skew = (float)Math.Clamp((skewParameter - 16) / 16, -0.95, 1.0);
            return polygonFromPoints(
                new Vector2(-halfWidth + skew, -1),
                new Vector2(halfWidth + skew, -1),
                new Vector2(halfWidth - skew, 1),
                new Vector2(-halfWidth - skew, 1));
        }

        private static List<Vector2> rhombus(double angleParameter)
        {
            double angle = Math.Clamp(angleParameter / 32, 0.08, 1) * Math.PI * 0.78 + Math.PI * 0.11;
            float halfWidth = (float)Math.Cos(angle / 2);
            float halfHeight = (float)Math.Sin(angle / 2);
            return polygonFromPoints(
                new Vector2(0, -halfHeight),
                new Vector2(halfWidth, 0),
                new Vector2(0, halfHeight),
                new Vector2(-halfWidth, 0));
        }

        private static List<Vector2> trapezoid(double topWidthParameter, double heightParameter)
        {
            float top = (float)Math.Clamp(topWidthParameter / 6, 0.16, 1.8);
            float bottom = 1.45f;
            float height = (float)Math.Clamp(heightParameter / 3, 0.35, 2.5);
            return polygonFromPoints(
                new Vector2(-top, -height),
                new Vector2(top, -height),
                new Vector2(bottom, height),
                new Vector2(-bottom, height));
        }

        private static List<Vector2> kite(double upperParameter, double waistParameter)
        {
            float upper = (float)Math.Clamp(upperParameter / 6, 0.2, 2.2);
            float waist = (float)Math.Clamp(waistParameter / 4, 0.18, 1.8);
            return polygonFromPoints(
                new Vector2(0, -upper),
                new Vector2(waist, 0),
                new Vector2(0, 1.35f),
                new Vector2(-waist, 0));
        }

        private static List<Vector2> star(int points, double innerRadiusParameter, bool sharp)
        {
            float inner = sharp
                ? (float)Math.Clamp(innerRadiusParameter / 24, 0.06, 0.55)
                : (float)Math.Clamp(innerRadiusParameter / 10, 0.1, 0.85);
            var result = new List<Vector2>(points * 2 + 1);

            for (int i = 0; i <= points * 2; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / points;
                float radius = i % 2 == 0 ? 1 : inner;
                result.Add(new Vector2((float)(Math.Cos(angle) * radius), (float)(Math.Sin(angle) * radius)));
            }

            result[^1] = result[0];
            return result;
        }

        private static List<Vector2> cross(double armParameter, double stemParameter)
        {
            float arm = (float)Math.Clamp(armParameter / 16, 0.08, 0.8);
            float stem = (float)Math.Clamp(stemParameter / 16, 0.08, 0.8);
            return polygonFromPoints(
                new Vector2(-stem, -1),
                new Vector2(stem, -1),
                new Vector2(stem, -arm),
                new Vector2(1, -arm),
                new Vector2(1, arm),
                new Vector2(stem, arm),
                new Vector2(stem, 1),
                new Vector2(-stem, 1),
                new Vector2(-stem, arm),
                new Vector2(-1, arm),
                new Vector2(-1, -arm),
                new Vector2(-stem, -arm));
        }

        private static List<Vector2> chevron(double notchParameter, double depthParameter)
        {
            float notch = (float)Math.Clamp(notchParameter / 16, 0.08, 0.85);
            float depth = (float)Math.Clamp(depthParameter / 12, 0.16, 1.2);
            return polygonFromPoints(
                new Vector2(-1, -1),
                new Vector2(0, -1 + depth),
                new Vector2(1, -1),
                new Vector2(1, 1 - depth),
                new Vector2(0, 1),
                new Vector2(-1, 1 - depth),
                new Vector2(-notch, 0));
        }

        private static List<Vector2> arrowHead(double widthParameter, double tailParameter)
        {
            float halfWidth = (float)Math.Clamp(widthParameter / 12, 0.18, 1.5);
            float tail = (float)Math.Clamp(tailParameter / 16, 0.08, 0.75);
            return polygonFromPoints(
                new Vector2(0, -1.25f),
                new Vector2(halfWidth, 0),
                new Vector2(tail, 0),
                new Vector2(tail, 1.15f),
                new Vector2(-tail, 1.15f),
                new Vector2(-tail, 0),
                new Vector2(-halfWidth, 0));
        }

        private static List<Vector2> polygonFromPoints(params Vector2[] points)
        {
            var result = new List<Vector2>(points.Length + 1);
            result.AddRange(points);

            if (points.Length > 0)
                result.Add(points[0]);

            return result;
        }

        private static float aspect(double parameter) => (float)Math.Clamp(parameter / 3, 0.35, 4);

        private static List<Vector2> lSystem(
            string axiom,
            IReadOnlyDictionary<char, string> rules,
            int iterations,
            float angleDegrees,
            string drawSymbols,
            bool backtrackBranches = false)
        {
            string current = axiom;

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                var next = new System.Text.StringBuilder(current.Length * 4);

                foreach (char symbol in current)
                    next.Append(rules.TryGetValue(symbol, out string? replacement) ? replacement : symbol);

                current = next.ToString();

                if (current.Length > max_control_points * 8)
                    break;
            }

            var points = new List<Vector2> { Vector2.Zero };
            Vector2 position = Vector2.Zero;
            float angle = 0;
            float turn = float.DegreesToRadians(angleDegrees);
            var branches = new Stack<(Vector2 Position, float Angle)>();

            foreach (char symbol in current)
            {
                if (drawSymbols.Contains(symbol))
                {
                    position += new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                    points.Add(position);
                }
                else if (symbol == '+')
                    angle += turn;
                else if (symbol == '-')
                    angle -= turn;
                else if (symbol == '[')
                    branches.Push((position, angle));
                else if (symbol == ']' && branches.Count > 0)
                {
                    (position, angle) = branches.Pop();

                    if (backtrackBranches && Vector2.DistanceSquared(points[^1], position) > 0.000001f)
                        points.Add(position);
                }
            }

            return points;
        }

        private static List<Vector2> levyCurve(int iterations)
        {
            var points = new List<Vector2> { new Vector2(-1, 0) };
            appendLevy(points, new Vector2(-1, 0), new Vector2(1, 0), iterations);
            return points;

            static void appendLevy(List<Vector2> target, Vector2 start, Vector2 end, int depth)
            {
                if (depth == 0 || target.Count >= max_control_points)
                {
                    target.Add(end);
                    return;
                }

                Vector2 midpoint = (start + end) / 2;
                Vector2 delta = end - start;
                Vector2 apex = midpoint + new Vector2(-delta.Y, delta.X) / 2;
                appendLevy(target, start, apex, depth - 1);
                appendLevy(target, apex, end, depth - 1);
            }
        }

        private static List<Vector2> mengerCarpet(int depth)
        {
            var points = new List<Vector2>();
            addSquare(points, Vector2.Zero, 2);
            addHoles(Vector2.Zero, 2, depth);
            return points;

            void addHoles(Vector2 centre, float size, int remaining)
            {
                if (remaining <= 0 || points.Count >= max_control_points)
                    return;

                float childSize = size / 3;
                addSquare(points, centre, childSize);

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        addHoles(centre + new Vector2(x, y) * childSize, childSize, remaining - 1);
                    }
                }
            }

            static void addSquare(List<Vector2> target, Vector2 centre, float size)
            {
                float half = size / 2;
                target.Add(centre + new Vector2(-half, -half));
                target.Add(centre + new Vector2(half, -half));
                target.Add(centre + new Vector2(half, half));
                target.Add(centre + new Vector2(-half, half));
                target.Add(centre + new Vector2(-half, -half));
            }
        }

        private static List<Vector2> barnsleyFern(int count)
        {
            var result = new List<Vector2>(count);
            var random = new Random(1337);
            double x = 0;
            double y = 0;

            for (int i = 0; i < count + 20; i++)
            {
                double choice = random.NextDouble();
                double nextX;
                double nextY;

                if (choice < 0.01)
                {
                    nextX = 0;
                    nextY = 0.16 * y;
                }
                else if (choice < 0.86)
                {
                    nextX = 0.85 * x + 0.04 * y;
                    nextY = -0.04 * x + 0.85 * y + 1.6;
                }
                else if (choice < 0.93)
                {
                    nextX = 0.2 * x - 0.26 * y;
                    nextY = 0.23 * x + 0.22 * y + 1.6;
                }
                else
                {
                    nextX = -0.15 * x + 0.28 * y;
                    nextY = 0.26 * x + 0.24 * y + 0.44;
                }

                x = nextX;
                y = nextY;

                if (i >= 20)
                    result.Add(new Vector2((float)x, (float)-y));
            }

            return result;
        }

        private static List<Vector2> escapeTimeContour(int resolution, int maxIterations, EscapeFractal fractal, double parameterReal, double parameterImaginary)
        {
            var inside = new bool[resolution + 1, resolution + 1];
            const double minX = -2.1;
            const double maxX = 1.1;
            const double minY = -1.55;
            const double maxY = 1.55;

            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    double real = minX + (maxX - minX) * x / resolution;
                    double imaginary = minY + (maxY - minY) * y / resolution;
                    inside[x, y] = doesNotEscape(real, imaginary, maxIterations, fractal, parameterReal, parameterImaginary);
                }
            }

            var segments = new List<(GridPoint A, GridPoint B)>();

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int index = (inside[x, y] ? 8 : 0)
                                | (inside[x + 1, y] ? 4 : 0)
                                | (inside[x + 1, y + 1] ? 2 : 0)
                                | (inside[x, y + 1] ? 1 : 0);

                    GridPoint top = new GridPoint(x * 2 + 1, y * 2);
                    GridPoint right = new GridPoint((x + 1) * 2, y * 2 + 1);
                    GridPoint bottom = new GridPoint(x * 2 + 1, (y + 1) * 2);
                    GridPoint left = new GridPoint(x * 2, y * 2 + 1);

                    switch (index)
                    {
                        case 1: segments.Add((left, bottom)); break;
                        case 2: segments.Add((bottom, right)); break;
                        case 3: segments.Add((left, right)); break;
                        case 4: segments.Add((top, right)); break;
                        case 5:
                            segments.Add((top, left));
                            segments.Add((bottom, right));
                            break;
                        case 6: segments.Add((top, bottom)); break;
                        case 7: segments.Add((top, left)); break;
                        case 8: segments.Add((top, left)); break;
                        case 9: segments.Add((top, bottom)); break;
                        case 10:
                            segments.Add((top, right));
                            segments.Add((bottom, left));
                            break;
                        case 11: segments.Add((top, right)); break;
                        case 12: segments.Add((left, right)); break;
                        case 13: segments.Add((bottom, right)); break;
                        case 14: segments.Add((left, bottom)); break;
                    }
                }
            }

            List<GridPoint> contour = longestConnectedPath(segments);
            return contour.Select(point => new Vector2(
                (float)(minX + (maxX - minX) * point.X / (resolution * 2.0)),
                (float)(minY + (maxY - minY) * point.Y / (resolution * 2.0)))).ToList();
        }

        private static bool doesNotEscape(double pointReal, double pointImaginary, int maxIterations, EscapeFractal fractal, double parameterReal, double parameterImaginary)
        {
            double real = fractal == EscapeFractal.Julia ? pointReal : 0;
            double imaginary = fractal == EscapeFractal.Julia ? pointImaginary : 0;
            double constantReal = fractal == EscapeFractal.Julia ? parameterReal : pointReal;
            double constantImaginary = fractal == EscapeFractal.Julia ? parameterImaginary : pointImaginary;

            for (int i = 0; i < maxIterations; i++)
            {
                if (fractal == EscapeFractal.BurningShip)
                {
                    real = Math.Abs(real);
                    imaginary = Math.Abs(imaginary);
                }

                double nextReal = real * real - imaginary * imaginary + constantReal;
                imaginary = 2 * real * imaginary + constantImaginary;
                real = nextReal;

                if (real * real + imaginary * imaginary > 4)
                    return false;
            }

            return true;
        }

        private static List<Vector2> newtonBoundary(int resolution, int iterations)
        {
            var roots = new int[resolution + 1, resolution + 1];
            const double range = 1.6;

            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    double real = -range + range * 2 * x / resolution;
                    double imaginary = -range + range * 2 * y / resolution;
                    roots[x, y] = newtonRoot(real, imaginary, iterations);
                }
            }

            var boundary = new List<Vector2>();

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int root = roots[x, y];

                    if (roots[x + 1, y] != root || roots[x, y + 1] != root || roots[x + 1, y + 1] != root)
                    {
                        boundary.Add(new Vector2(
                            (float)(-range + range * 2 * (x + 0.5) / resolution),
                            (float)(-range + range * 2 * (y + 0.5) / resolution)));
                    }
                }
            }

            if (boundary.Count > max_control_points)
            {
                double step = boundary.Count / (double)max_control_points;
                boundary = Enumerable.Range(0, max_control_points)
                                     .Select(i => boundary[(int)(i * step)])
                                     .ToList();
            }

            return orderByNearestNeighbour(boundary);
        }

        private static int newtonRoot(double real, double imaginary, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                double r2 = real * real + imaginary * imaginary;
                double denominator = 3 * r2 * r2;

                if (denominator < 1e-12)
                    break;

                double cubeReal = real * real * real - 3 * real * imaginary * imaginary;
                double cubeImaginary = 3 * real * real * imaginary - imaginary * imaginary * imaginary;
                double quotientReal = ((cubeReal - 1) * (real * real - imaginary * imaginary) + cubeImaginary * 2 * real * imaginary) / denominator;
                double quotientImaginary = (cubeImaginary * (real * real - imaginary * imaginary) - (cubeReal - 1) * 2 * real * imaginary) / denominator;
                real -= quotientReal;
                imaginary -= quotientImaginary;
            }

            double distance0 = (real - 1) * (real - 1) + imaginary * imaginary;
            double distance1 = (real + 0.5) * (real + 0.5) + (imaginary - Math.Sqrt(3) / 2) * (imaginary - Math.Sqrt(3) / 2);
            double distance2 = (real + 0.5) * (real + 0.5) + (imaginary + Math.Sqrt(3) / 2) * (imaginary + Math.Sqrt(3) / 2);
            return distance0 < distance1 ? (distance0 < distance2 ? 0 : 2) : (distance1 < distance2 ? 1 : 2);
        }

        private static List<Vector2> orderByNearestNeighbour(List<Vector2> source)
        {
            if (source.Count < 2)
                return source;

            var remaining = new List<Vector2>(source);
            var result = new List<Vector2>(source.Count);
            int currentIndex = 0;

            for (int i = 1; i < remaining.Count; i++)
            {
                if (remaining[i].X < remaining[currentIndex].X)
                    currentIndex = i;
            }

            result.Add(remaining[currentIndex]);
            remaining.RemoveAt(currentIndex);

            while (remaining.Count > 0 && result.Count < max_control_points)
            {
                Vector2 current = result[^1];
                int nearestIndex = 0;
                float nearestDistance = Vector2.DistanceSquared(current, remaining[0]);

                for (int i = 1; i < remaining.Count; i++)
                {
                    float distance = Vector2.DistanceSquared(current, remaining[i]);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestIndex = i;
                    }
                }

                result.Add(remaining[nearestIndex]);
                remaining.RemoveAt(nearestIndex);
            }

            return result;
        }

        private static List<GridPoint> longestConnectedPath(List<(GridPoint A, GridPoint B)> segments)
        {
            var adjacency = new Dictionary<GridPoint, List<int>>();

            for (int i = 0; i < segments.Count; i++)
            {
                add(segments[i].A, i);
                add(segments[i].B, i);
            }

            var unused = new HashSet<int>(Enumerable.Range(0, segments.Count));
            var longest = new List<GridPoint>();

            while (unused.Count > 0)
            {
                int first = unused.First();
                unused.Remove(first);
                var path = new List<GridPoint> { segments[first].A, segments[first].B };
                extend(path, false);
                extend(path, true);

                if (path.Count > longest.Count)
                    longest = path;
            }

            return longest;

            void add(GridPoint point, int segmentIndex)
            {
                if (!adjacency.TryGetValue(point, out List<int>? connected))
                    adjacency[point] = connected = new List<int>();

                connected.Add(segmentIndex);
            }

            void extend(List<GridPoint> path, bool prepend)
            {
                while (true)
                {
                    GridPoint endpoint = prepend ? path[0] : path[^1];

                    if (!adjacency.TryGetValue(endpoint, out List<int>? connected))
                        return;

                    int nextIndex = connected.FirstOrDefault(unused.Contains, -1);

                    if (nextIndex < 0)
                        return;

                    unused.Remove(nextIndex);
                    GridPoint next = segments[nextIndex].A.Equals(endpoint) ? segments[nextIndex].B : segments[nextIndex].A;

                    if (prepend)
                        path.Insert(0, next);
                    else
                        path.Add(next);
                }
            }
        }

        private static List<Vector2> normaliseAndLimit(List<Vector2> source, float targetSize)
        {
            var clean = new List<Vector2>(source.Count);

            foreach (Vector2 point in source)
            {
                if (float.IsNaN(point.X) || float.IsNaN(point.Y) || float.IsInfinity(point.X) || float.IsInfinity(point.Y))
                    continue;

                if (clean.Count == 0 || Vector2.DistanceSquared(clean[^1], point) > 0.000001f)
                    clean.Add(point);
            }

            if (clean.Count > max_control_points)
            {
                double step = (clean.Count - 1) / (double)(max_control_points - 1);
                clean = Enumerable.Range(0, max_control_points)
                                  .Select(i => clean[(int)Math.Round(i * step)])
                                  .ToList();
            }

            if (clean.Count < 2)
                return clean;

            float minX = clean.Min(p => p.X);
            float maxX = clean.Max(p => p.X);
            float minY = clean.Min(p => p.Y);
            float maxY = clean.Max(p => p.Y);
            float largestDimension = Math.Max(maxX - minX, maxY - minY);

            if (largestDimension <= 0.0001f)
                return new List<Vector2>();

            Vector2 centre = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
            float scale = targetSize / largestDimension;
            return clean.Select(point => (point - centre) * scale + new Vector2(256, 192)).ToList();
        }

        private enum EscapeFractal
        {
            Mandelbrot,
            Julia,
            BurningShip
        }

        private readonly struct GridPoint : IEquatable<GridPoint>
        {
            public readonly int X;
            public readonly int Y;

            public GridPoint(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Equals(GridPoint other) => X == other.X && Y == other.Y;
            public override bool Equals(object? obj) => obj is GridPoint other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(X, Y);
        }
    }
}
