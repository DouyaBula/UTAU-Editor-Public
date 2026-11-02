// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// Serialises and applies user-defined toolbox ordering without coupling the UI to a
    /// particular set of ruleset-provided groups. Unknown or newly-added groups are appended
    /// in their default order, making settings forward-compatible.
    /// </summary>
    internal static class EditorToolboxOrder
    {
        private const char separator = '|';

        public static IReadOnlyList<string> Parse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();

            return value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                        .Where(key => !string.IsNullOrWhiteSpace(key))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();
        }

        public static string Serialize(IEnumerable<string> keys)
            => string.Join(separator, keys.Where(key => !string.IsNullOrWhiteSpace(key))
                                          .Distinct(StringComparer.Ordinal));

        public static IReadOnlyList<T> Apply<T>(IReadOnlyList<T> current, Func<T, string> keySelector, string? savedOrder)
        {
            var order = Parse(savedOrder);
            var rank = order.Select((key, index) => (key, index))
                            .ToDictionary(item => item.key, item => item.index, StringComparer.Ordinal);

            return current.Select((item, index) => (item, index, key: keySelector(item)))
                          .OrderBy(item => rank.TryGetValue(item.key, out int savedIndex) ? savedIndex : int.MaxValue)
                          .ThenBy(item => item.index)
                          .Select(item => item.item)
                          .ToArray();
        }
    }
}
