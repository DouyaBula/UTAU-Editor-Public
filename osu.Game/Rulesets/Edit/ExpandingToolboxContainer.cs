// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public partial class ExpandingToolboxContainer : ExpandingContainer
    {
        protected override double HoverExpansionDelay => 250;

        protected override bool ExpandOnHover => expandOnHover;

        private readonly Bindable<bool> contractSidebars = new Bindable<bool>();
        private readonly OsuSetting orderSetting;

        private bool expandOnHover;
        private OffsetMaintainingScrollContainer scrollContainer = null!;
        private Bindable<string> savedOrder = null!;

        [Resolved]
        private Editor? editor { get; set; }

        /// <summary>
        /// Constructs an expandable toolbox using the left sidebar's persisted ordering.
        /// </summary>
        public ExpandingToolboxContainer(float contractedWidth, float expandedWidth)
            : this(contractedWidth, expandedWidth, OsuSetting.EditorLeftToolboxOrder)
        {
        }

        public ExpandingToolboxContainer(float contractedWidth, float expandedWidth, OsuSetting orderSetting)
            : base(contractedWidth, expandedWidth)
        {
            this.orderSetting = orderSetting;
            RelativeSizeAxes = Axes.Y;

            FillFlow.Spacing = new Vector2(5);
            FillFlow.Padding = new MarginPadding { Vertical = 5 };

            Expanded.Value = true;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.EditorContractSidebars, contractSidebars);
            savedOrder = config.GetBindable<string>(orderSetting);
        }

        protected override OsuScrollContainer CreateScrollContainer() => scrollContainer = new OffsetMaintainingScrollContainer();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var inputManager = GetContainingInputManager();

            if (inputManager != null)
            {
                Expanded.BindValueChanged(_ =>
                {
                    // When state changes from expanded -> collapsed the mouse is no longer within the toolbox so there would be no
                    // hovered children if we used the mouse position directly
                    var position = new Vector2(ScreenSpaceDrawQuad.Centre.X, inputManager.CurrentState.Mouse.Position.Y);

                    scrollContainer.TargetDrawable = Children.FirstOrDefault(it => it.Contains(position));
                });
            }

            RestoreGroupOrder();
        }

        /// <summary>
        /// Gets the groups currently available for ordering. This is also used by the editor's
        /// deterministic ordering panel; unlike mouse dragging it does not depend on which
        /// drawable happened to capture the pointer gesture.
        /// </summary>
        public IReadOnlyList<EditorToolboxGroup> ReorderableGroups
            => getReorderableChildren().Select(findGroup).Where(group => group != null).Cast<EditorToolboxGroup>().ToArray();

        /// <summary>
        /// Moves a group one or more positions and persists the result immediately.
        /// </summary>
        public bool MoveGroup(string orderKey, int offset)
        {
            var reorderable = getReorderableChildren();
            int currentIndex = reorderable.FindIndex(child => getOrderKey(child) == orderKey);

            if (currentIndex < 0 || offset == 0)
                return false;

            int targetIndex = Math.Clamp(currentIndex + offset, 0, reorderable.Count - 1);

            if (targetIndex == currentIndex)
                return false;

            var reordered = reorderable.ToList();
            var child = reordered[currentIndex];
            reordered.RemoveAt(currentIndex);
            reordered.Insert(targetIndex, child);

            // Reassign every layout position, rather than only moving one child. This avoids
            // temporary duplicate positions (which made FlowContainer ordering depend on update
            // timing and caused apparently intermittent button presses).
            applyGroupOrder(reordered);

            persistOrder(reordered.Select(getOrderKey));

            return true;
        }

        private void persistOrder(IEnumerable<string> keys)
        {
            string serialised = EditorToolboxOrder.Serialize(keys);

            if (savedOrder != null)
                savedOrder.Value = serialised;
        }

        /// <summary>
        /// Re-applies the persisted order to the groups currently attached to this toolbox.
        /// Rulesets may add groups from their own background dependency loaders after the base
        /// composer has loaded, so callers should invoke this once those groups are attached.
        /// </summary>
        public void RestoreGroupOrder()
        {
            if (savedOrder == null)
                return;

            var current = getReorderableChildren();
            var ordered = EditorToolboxOrder.Apply(current, getOrderKey, savedOrder.Value);

            applyGroupOrder(ordered);
        }

        private void applyGroupOrder(IReadOnlyList<Drawable> orderedGroups)
        {
            var flowing = getOrderedChildren().ToList();
            int groupIndex = 0;
            var finalOrder = flowing.Select(child => tryGetGroup(child, out _)
                ? orderedGroups[groupIndex++]
                : child).ToList();

            for (int i = 0; i < finalOrder.Count; i++)
                FillFlow.SetLayoutPosition(finalOrder[i], i);
        }

        private List<Drawable> getReorderableChildren()
            => getOrderedChildren().Where(child => tryGetGroup(child, out _)).ToList();

        // FlowingChildren excludes children which are not alive/present yet. During loading
        // this can be empty even though all groups are attached. Include every child and use
        // the flow's layout positions, preserving insertion order for equal positions.
        private IEnumerable<Drawable> getOrderedChildren()
            => FillFlow.Children.OrderBy(FillFlow.GetLayoutPosition);

        private static bool tryGetGroup(Drawable child, out EditorToolboxGroup group)
        {
            group = findGroup(child)!;
            return group != null;
        }

        private static EditorToolboxGroup? findGroup(Drawable drawable)
        {
            if (drawable is EditorToolboxGroup group)
                return group;

            if (drawable is Container container)
            {
                foreach (var child in container.Children)
                {
                    var nested = findGroup(child);

                    if (nested != null)
                        return nested;
                }
            }

            return null;
        }

        private static string getOrderKey(Drawable child)
            => tryGetGroup(child, out var group) ? group.OrderKey : child.GetType().FullName ?? child.GetType().Name;

        protected override void Update()
        {
            base.Update();

            bool requireContracting = contractSidebars.Value || editor?.DrawSize.X / editor?.DrawSize.Y < 1.7f;

            if (expandOnHover != requireContracting)
            {
                expandOnHover = requireContracting;
                Expanded.Value = !expandOnHover;
            }
        }

        protected override bool OnClick(ClickEvent e) => true;

        private partial class OffsetMaintainingScrollContainer : OsuScrollContainer
        {
            private Drawable? targetDrawable;
            private float targetPosition;

            public Drawable? TargetDrawable
            {
                get => targetDrawable;
                set
                {
                    targetDrawable = value;

                    if (value != null)
                        targetPosition = ToLocalSpace(value.ScreenSpaceDrawQuad.TopLeft).Y;
                }
            }

            protected override void UpdateAfterChildren()
            {
                if (targetDrawable != null)
                {
                    float currentPosition = ToLocalSpace(targetDrawable.ScreenSpaceDrawQuad.TopLeft).Y;

                    if (!Precision.AlmostEquals(targetPosition, currentPosition))
                    {
                        double offset = currentPosition - targetPosition;

                        double scrollTarget = Math.Clamp(Current + offset, 0, ScrollableExtent);

                        ScrollTo(scrollTarget, false, double.PositiveInfinity);
                    }
                }

                base.UpdateAfterChildren();
            }

            protected override void OnUserScroll(double value, bool animated = true, double? distanceDecay = null)
            {
                targetDrawable = null;

                base.OnUserScroll(value, animated, distanceDecay);
            }
        }
    }
}
