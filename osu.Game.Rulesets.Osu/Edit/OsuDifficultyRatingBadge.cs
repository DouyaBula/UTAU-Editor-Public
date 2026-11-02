// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuDifficultyRatingBadge : CompositeDrawable
    {
        private readonly IBindable<double?> rating;
        private OsuSpriteText label = null!;
        private Bindable<bool> visible = null!;
        private Bindable<bool> onLeft = null!;

        public OsuDifficultyRatingBadge(IBindable<double?> rating)
        {
            this.rating = rating;
            Anchor = Origin = Anchor.TopRight;
            Y = 12;
            X = -52;
            AutoSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuColour colours)
        {
            visible = config.GetBindable<bool>(OsuSetting.EditorDifficultyRatingVisible);
            onLeft = config.GetBindable<bool>(OsuSetting.EditorDifficultyRatingOnLeft);

            InternalChild = new CircularContainer
            {
                Masking = true,
                AutoSizeAxes = Axes.Both,
                Colour = colours.Gray7,
                Alpha = 0.9f,
                Child = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 10, Vertical = 6 },
                    Child = label = new OsuSpriteText
                    {
                        Font = OsuFont.Default.With(size: 18, weight: FontWeight.Bold),
                        Text = "SR  —",
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            rating.BindValueChanged(_ => update(), true);
            visible.BindValueChanged(_ => update(), true);
            onLeft.BindValueChanged(_ => update(), true);
        }

        private void update()
        {
            Alpha = visible.Value ? 1 : 0;
            Anchor = Origin = onLeft.Value ? Anchor.TopLeft : Anchor.TopRight;
            X = onLeft.Value ? 52 : -52;
            label.Text = rating.Value.HasValue
                ? $"SR  {rating.Value.Value.ToString("0.00", CultureInfo.InvariantCulture)}★"
                : "SR  …";
        }
    }
}
