// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderSelectionBlueprint
    {
        private readonly Bindable<bool> classicSymmetryEnabled = new BindableBool();
        private readonly Bindable<EditorClassicSymmetryAxis> classicSymmetryAxis = new Bindable<EditorClassicSymmetryAxis>();
        private ClassicSliderSymmetryPreview classicSymmetryPreview = null!;

        public ClassicSliderSymmetryPreview ClassicSymmetryPreview => classicSymmetryPreview;

        private void loadClassicSymmetry(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.EditorClassicSymmetry, classicSymmetryEnabled);
            config.BindWith(OsuSetting.EditorClassicSymmetryAxis, classicSymmetryAxis);
            classicSymmetryEnabled.BindValueChanged(_ => updateClassicSymmetryPreview());
            classicSymmetryAxis.BindValueChanged(_ => updateClassicSymmetryPreview());
        }

        private void updateClassicSymmetryPreview()
        {
            classicSymmetryPreview.UpdateFrom(HitObject, classicSymmetryAxis.Value,
                classicSymmetryEnabled.Value && !anchorMode.Value && IsSelected && selectedObjects.Count <= 1);
        }
    }
}
