// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using System.Linq;
using System.Reflection;
using osu.Framework.Testing;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class EditorToolboxOrderTest
    {
        [TestCase(OsuSetting.EditorLeftToolboxOrder)]
        [TestCase(OsuSetting.EditorRightToolboxOrder)]
        public void TestRestoreBeforeFirstFrame(OsuSetting setting)
        {
            using var storage = new TemporaryNativeStorage("toolbox-order");
            using (var config = new OsuConfigManager(storage))
            {
                config.SetValue(setting, "third|first|second");
                Assert.That(config.Save(), Is.True);
            }

            using var reloadedConfig = new OsuConfigManager(storage);
            using var toolbox = new ExpandingToolboxContainer(50, 200, setting);
            toolbox.Add(new EditorToolboxGroup("First", "first"));
            toolbox.Add(new EditorToolboxGroup("Second", "second"));
            toolbox.Add(new Container { Child = new EditorToolboxGroup("Third", "third"), Alpha = 0 });
            // Match the background loader without starting a renderer: no child is alive yet.
            typeof(ExpandingToolboxContainer).GetMethod("load", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(toolbox, new object[] { reloadedConfig });
            toolbox.RestoreGroupOrder();
            Assert.That(toolbox.ReorderableGroups.Select(g => g.OrderKey), Is.EqualTo(new[] { "third", "first", "second" }));
            Assert.That(toolbox.MoveGroup("second", -2), Is.True);
            Assert.That(reloadedConfig.Get<string>(setting), Is.EqualTo("second|third|first"));
            Assert.That(reloadedConfig.Save(), Is.True);
            using var savedConfig = new OsuConfigManager(storage);
            Assert.That(savedConfig.Get<string>(setting), Is.EqualTo("second|third|first"));
            using var recreated = new ExpandingToolboxContainer(50, 200, setting);
            recreated.Add(new EditorToolboxGroup("First", "first"));
            recreated.Add(new EditorToolboxGroup("Second", "second"));
            recreated.Add(new Container { Child = new EditorToolboxGroup("Third", "third") });
            typeof(ExpandingToolboxContainer).GetMethod("load", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(recreated, new object[] { savedConfig });
            recreated.RestoreGroupOrder();
            Assert.That(recreated.ReorderableGroups.Select(g => g.OrderKey), Is.EqualTo(new[] { "second", "third", "first" }));
        }

        [Test]
        public void TestSavedOrderIsAppliedAndNewGroupsAreAppended()
        {
            var current = new[] { "inspector", "generate", "transform" };

            var reordered = EditorToolboxOrder.Apply(current, value => value, "transform|inspector|missing");

            Assert.That(reordered, Is.EqualTo(new[] { "transform", "inspector", "generate" }));
        }

        [Test]
        public void TestSerialisationRemovesDuplicatesAndEmptyValues()
        {
            string serialized = EditorToolboxOrder.Serialize(new[] { "left", "", "left", "right" });

            Assert.That(serialized, Is.EqualTo("left|right"));
            Assert.That(EditorToolboxOrder.Parse(serialized), Is.EqualTo(new[] { "left", "right" }));
        }
    }
}
