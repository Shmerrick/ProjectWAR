using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Resolves the client's ability tooltip tokens to the numbers the client would print.
    ///
    /// THE TOKEN IS A COMPLETE ADDRESS. A tooltip in `data/strings/english/abilitydesc.txt` reads
    ///
    ///     "Your Squig flings deadly spines at it's target, dealing {COM_1_VAL0_DAMAGE}
    ///      every second for {COM_0_DURA_SECONDS}."
    ///
    /// and the token shape is
    ///
    ///     { [ ABIL_&lt;abilityId&gt; _ ] COM_&lt;componentIndex&gt; _ &lt;field&gt; _ &lt;meaning&gt; }
    ///
    /// `componentIndex` indexes **that ability's own ordered component list** -- not a component id,
    /// and not a sorted list. `field` is `VAL&lt;n&gt;` for the nth entry of the component's `Values`
    /// array, or one of the scalar fields (`DURA`, `FREQ`, `RADI`). `meaning` is presentation only:
    /// `DAMAGE`, `SPIRITDAMAGE`, `TOD_DAMAGE`, `SECONDS`, `FEET`. The optional `ABIL_&lt;id&gt;` prefix
    /// addresses a *different* ability's components; `abilitydesc.txt` row 9 uses `ABIL_3881`.
    ///
    /// So finding an ability's damage needs no operation table and no decoded component semantics.
    /// The ability's own description says which component and which slot.
    ///
    /// WHERE THE ORDERED LIST LIVES, AND THE TRAP. `mythic_bin_ability.MythicComponentData` is a
    /// JSON array whose entries carry `ComponentID`, an explicit `Index`, and the component's own
    /// `Values`, `Multipliers`, `Duration`, `Interval` and `Radius`. Order by `Index`.
    ///
    /// Do **not** use the ability report's "Related component IDs" line: it is a *sorted* set. For
    /// ability 7 it prints `2, 3301` while the real order is `3301, 2`, which makes `COM_0` resolve
    /// to the wrong component and silently corrupts every token above index 0. Ability 1 agrees
    /// under both readings, so a single-ability check proves nothing -- verify against one whose
    /// sorted and ordered lists differ.
    /// </summary>
    public static class AbilityTokenResolver
    {
        /// <summary>One component of one ability, in the ability's own order.</summary>
        public sealed class Component
        {
            public long ComponentId;
            public int Index;
            public long[] Values = new long[0];
            public long[] Multipliers = new long[0];
            public long Duration;
            public long Interval;
            public long Radius;
        }

        public sealed class Token
        {
            public string Raw;
            public long? ForeignAbilityId;
            public int ComponentIndex;
            public string Field;      // VAL0..VAL7, DURA, FREQ, RADI
            public string Meaning;    // DAMAGE, SECONDS, FEET, ...
            public bool IsDamage;
        }

        public sealed class Resolution
        {
            public Token Token;
            public bool Resolved;

            /// <summary>The number the client prints: <see cref="RawValue"/> scaled by the multiplier.</summary>
            public long Value;

            /// <summary>`Values[slot]` before the multiplier, kept so a report can show the working.</summary>
            public long RawValue;

            /// <summary>`Multipliers[slot]`, a percentage. 100 means the raw value stands.</summary>
            public long MultiplierPercent = 100;

            public string Failure;
        }

        // Deliberately permissive on the meaning suffix: the client uses dozens (DAMAGE,
        // SPIRITDAMAGE, TOD_DAMAGE, CORPOREALDAMAGE, HEAL...) and enumerating them would mean
        // silently skipping tokens whenever a new one appeared.
        private static readonly Regex TokenPattern = new Regex(
            @"\{(?:ABIL_(?<abil>\d+)_)?COM_(?<idx>\d+)_(?<field>VAL\d+|DURA|FREQ|RADI)(?:_(?<meaning>[A-Z0-9_]+))?\}",
            RegexOptions.Compiled);

        public static List<Token> Parse(string description)
        {
            var tokens = new List<Token>();
            if (string.IsNullOrEmpty(description))
                return tokens;

            foreach (Match match in TokenPattern.Matches(description))
            {
                string meaning = match.Groups["meaning"].Success ? match.Groups["meaning"].Value : string.Empty;

                tokens.Add(new Token
                {
                    Raw = match.Value,
                    ForeignAbilityId = match.Groups["abil"].Success
                        ? (long?)long.Parse(match.Groups["abil"].Value, CultureInfo.InvariantCulture)
                        : null,
                    ComponentIndex = int.Parse(match.Groups["idx"].Value, CultureInfo.InvariantCulture),
                    Field = match.Groups["field"].Value,
                    Meaning = meaning,
                    // "DAMAGE" as a substring covers SPIRITDAMAGE, TOD_DAMAGE, CORPOREALDAMAGE and
                    // the rest without listing them.
                    IsDamage = meaning.IndexOf("DAMAGE", StringComparison.Ordinal) >= 0
                });
            }

            return tokens;
        }

        /// <summary>
        /// Reads `MythicComponentData` into the ability's ordered component list. The column stores
        /// an array whose `Data` member is itself a JSON *string*, so it is parsed twice.
        /// </summary>
        public static List<Component> ParseComponents(string mythicComponentData)
        {
            var components = new List<Component>();
            if (string.IsNullOrWhiteSpace(mythicComponentData) || mythicComponentData == "[]")
                return components;

            JArray outer;
            try
            {
                outer = JArray.Parse(mythicComponentData);
            }
            catch (JsonException)
            {
                return components;
            }

            foreach (JToken entry in outer)
            {
                var inner = entry["Data"] as JValue;
                if (inner == null || inner.Value == null)
                    continue;

                JObject data;
                try
                {
                    data = JObject.Parse(Convert.ToString(inner.Value, CultureInfo.InvariantCulture));
                }
                catch (JsonException)
                {
                    continue;
                }

                components.Add(new Component
                {
                    ComponentId = Number(data, "ComponentID"),
                    Index = (int)Number(data, "Index"),
                    Values = Numbers(data, "Values"),
                    Multipliers = Numbers(data, "Multipliers"),
                    Duration = Number(data, "Duration"),
                    Interval = Number(data, "Interval"),
                    Radius = Number(data, "Radius")
                });
            }

            components.Sort((a, b) => a.Index.CompareTo(b.Index));
            return components;
        }

        /// <summary>
        /// The number the client would print for a token, given the owning ability's components and
        /// a lookup for the foreign abilities an `ABIL_` prefix can reach.
        /// </summary>
        public static Resolution Resolve(Token token, List<Component> own,
            Func<long, List<Component>> foreignLookup)
        {
            var resolution = new Resolution { Token = token };

            List<Component> components = own;
            if (token.ForeignAbilityId.HasValue)
            {
                components = foreignLookup == null ? null : foreignLookup(token.ForeignAbilityId.Value);
                if (components == null)
                {
                    resolution.Failure = "ability " + token.ForeignAbilityId.Value + " not found";
                    return resolution;
                }
            }

            // Two very different failures, kept apart because they mean opposite things. No
            // components at all is an import gap in mythic_bin_ability -- the token is fine and we
            // simply have nothing to resolve it against. An index past a non-empty list would mean
            // the ordering is wrong, which would indict the reading itself.
            if (components == null || components.Count == 0)
            {
                resolution.Failure = "no components imported for this ability";
                return resolution;
            }

            if (token.ComponentIndex >= components.Count)
            {
                resolution.Failure = "component index past a non-empty list";
                return resolution;
            }

            Component component = components[token.ComponentIndex];

            if (token.Field.StartsWith("VAL", StringComparison.Ordinal))
            {
                int slot;
                if (!int.TryParse(token.Field.Substring(3), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out slot))
                {
                    resolution.Failure = "unparseable slot " + token.Field;
                    return resolution;
                }

                if (slot >= component.Values.Length)
                {
                    resolution.Failure = "slot " + slot + " of " + component.Values.Length;
                    return resolution;
                }

                // THE MULTIPLIER IS PART OF THE VALUE. `Multipliers[slot]` is a percentage applied
                // to `Values[slot]`, and reading the value alone is wrong for every ability whose
                // multiplier is not 100. Three cases that settle it against our own damage table:
                //
                //   627 Sever Nerve      40 x 400% = 160   MinDamage 160
                //   670 Mage Bolt        40 x 200% =  80   MinDamage  80
                //   5   KABOOM! (COM_3)  50 x 110% =  55   MinDamage  55
                //
                // Ignoring it produced clean integer ratios -- 21 abilities off by exactly 4x, 15 by
                // 2x, 10 by 6x, 8 by 8x -- which is what sent us looking. A tidy ratio across
                // unrelated abilities is never drift; it is a missing factor.
                long raw = component.Values[slot];
                long multiplier = slot < component.Multipliers.Length ? component.Multipliers[slot] : 100;

                resolution.RawValue = raw;
                resolution.MultiplierPercent = multiplier;
                resolution.Value = raw * multiplier / 100;
                resolution.Resolved = true;
                return resolution;
            }

            switch (token.Field)
            {
                // The client prints these as seconds; the data is milliseconds.
                case "DURA": resolution.Value = component.Duration; break;
                case "FREQ": resolution.Value = component.Interval; break;
                case "RADI": resolution.Value = component.Radius; break;
                default:
                    resolution.Failure = "unknown field " + token.Field;
                    return resolution;
            }

            resolution.Resolved = true;
            return resolution;
        }

        private static long Number(JObject data, string name)
        {
            JToken value = data[name];
            if (value == null || value.Type == JTokenType.Null)
                return 0;

            try
            {
                return value.Value<long>();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static long[] Numbers(JObject data, string name)
        {
            var array = data[name] as JArray;
            if (array == null)
                return new long[0];

            var numbers = new long[array.Count];
            for (int i = 0; i < array.Count; ++i)
            {
                try
                {
                    numbers[i] = array[i].Value<long>();
                }
                catch (Exception)
                {
                    numbers[i] = 0;
                }
            }

            return numbers;
        }
    }
}
