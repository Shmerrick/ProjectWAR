using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.Managers
{
    public class BotLoadoutManager
    {
        public enum BotTier
        {
            T1,
            T2,
            T3,
            T4,
            SC  // Scenario — falls back to T4 at runtime if no explicit template is set
        }

        public class Loadout
        {
            public Dictionary<ushort, uint> SlotItems { get; } = new Dictionary<ushort, uint>();

            public Loadout Clone()
            {
                Loadout clone = new Loadout();
                foreach (KeyValuePair<ushort, uint> entry in SlotItems)
                    clone.SlotItems[entry.Key] = entry.Value;

                return clone;
            }
        }

        private static readonly ushort[] _managedEquipmentSlots =
        {
            (ushort)EquipSlot.MAIN_HAND,
            (ushort)EquipSlot.OFF_HAND,
            (ushort)EquipSlot.RANGED_WEAPON,
            (ushort)EquipSlot.BODY,
            (ushort)EquipSlot.GLOVES,
            (ushort)EquipSlot.BOOTS,
            (ushort)EquipSlot.HELM,
            (ushort)EquipSlot.SHOULDER,
            (ushort)EquipSlot.POCKET_1,
            (ushort)EquipSlot.POCKET_2,
            (ushort)EquipSlot.BACK,
            (ushort)EquipSlot.BELT,
            (ushort)EquipSlot.JEWELLERY_1,
            (ushort)EquipSlot.JEWELLERY_2,
            (ushort)EquipSlot.JEWELLERY_3,
            (ushort)EquipSlot.JEWELLERY_4
        };

        // Templates keyed by (careerLine, tier, variantIndex).
        // Variant indices are open-ended; 0 and 1 are the common defaults used by current bot groups.
        private static readonly Dictionary<TemplateKey, Loadout> _templates = new Dictionary<TemplateKey, Loadout>();

        public static IReadOnlyList<ushort> ManagedEquipmentSlots => _managedEquipmentSlots;

        // Legacy default when no explicit bot-profile row has been assigned yet.
        public static byte GetDefaultVariantIndex(BotRole role) => role == BotRole.OffTank_2H ? (byte)1 : (byte)0;

        public static byte GetResolvedVariantIndex(uint characterId, BotRole role)
        {
            return BotTemplateProfileService.ResolveVariantIndex(characterId, role);
        }

        public static void Initialize()
        {
            _templates.Clear();
            RegisterAllTemplates();
        }

        // ---- Public read API ----

        public static Loadout GetLoadout(Player bot, BotTier tier, BotRole role)
        {
            if (bot?.Info == null)
                return null;

            Character_value value = bot.Info.Value ?? bot._Value;
            if (value == null)
                return GetBaseLoadout(bot.CharacterId, bot.Info.CareerLine, tier, role);

            return GetLoadout(bot.CharacterId, bot.Info.CareerLine, bot.Info.Race, value.Skills, value.Level, value.RenownRank, tier, role);
        }

        public static Loadout GetLoadout(uint characterId, byte careerLine, byte race, long playerSkills, byte level, byte renownRank, BotTier tier, BotRole role)
        {
            Loadout baseLoadout = GetBaseLoadout(characterId, careerLine, tier, role);
            if (baseLoadout == null)
            {
                Log.Error("BotLoadoutManager", $"No explicit template or starter fallback found for CharacterId {characterId} ({careerLine}/{role}).");
                return null;
            }

            Loadout effectiveLoadout = baseLoadout.Clone();
            ApplyOverrides(effectiveLoadout, characterId, careerLine, race, playerSkills, level, renownRank);
            return effectiveLoadout;
        }

        public static Loadout GetBaseLoadout(uint characterId, byte careerLine, BotTier tier, BotRole role)
        {
            return GetBaseLoadout(careerLine, tier, GetResolvedVariantIndex(characterId, role));
        }

        // Legacy bridge: maps (tier, role) to the default (tier, variantIndex) when no explicit bot assignment is available.
        public static Loadout GetBaseLoadout(byte careerLine, BotTier tier, BotRole role)
        {
            return GetBaseLoadout(careerLine, tier, GetDefaultVariantIndex(role));
        }

        public static Loadout GetBaseLoadout(byte careerLine, BotTier tier, byte variantIndex)
        {
            // Direct lookup.
            if (_templates.TryGetValue(new TemplateKey(careerLine, tier, variantIndex), out Loadout t))
                return t;

            // Try variant 0 for the same tier.
            if (variantIndex != 0 && _templates.TryGetValue(new TemplateKey(careerLine, tier, 0), out t))
                return t;

            // SC falls back to T4.
            if (tier == BotTier.SC)
            {
                if (_templates.TryGetValue(new TemplateKey(careerLine, BotTier.T4, variantIndex), out t))
                    return t;
                if (variantIndex != 0 && _templates.TryGetValue(new TemplateKey(careerLine, BotTier.T4, 0), out t))
                    return t;
            }

            return BuildStartingLoadout(careerLine);
        }

        // Returns the explicitly registered template (no fallback); null if not registered.
        public static Loadout GetTemplate(byte careerLine, BotTier tier, byte variantIndex)
        {
            _templates.TryGetValue(new TemplateKey(careerLine, tier, variantIndex), out Loadout loadout);
            return loadout;
        }

        public static IReadOnlyList<(byte CareerLine, BotTier Tier, byte VariantIndex, Loadout Loadout)> GetAllTemplates()
        {
            return _templates
                .Select(kv => (kv.Key.CareerLine, kv.Key.Tier, kv.Key.VariantIndex, kv.Value))
                .OrderBy(t => t.CareerLine)
                .ThenBy(t => (int)t.Tier)
                .ThenBy(t => t.VariantIndex)
                .ToList();
        }

        // ---- Patch API ----

        // Bridge for the bot-centric patch route: maps role to the legacy default variant index.
        public static void PatchTemplate(byte careerLine, BotTier tier, BotRole role, Dictionary<ushort, uint> updates)
        {
            PatchTemplate(careerLine, tier, GetDefaultVariantIndex(role), updates);
        }

        public static void PatchTemplate(byte careerLine, BotTier tier, byte variantIndex, Dictionary<ushort, uint> updates)
        {
            TemplateKey key = new TemplateKey(careerLine, tier, variantIndex);
            if (!_templates.TryGetValue(key, out Loadout template))
            {
                template = BuildStartingLoadout(careerLine) ?? new Loadout();
                _templates[key] = template;
            }

            foreach (KeyValuePair<ushort, uint> update in updates)
            {
                if (update.Value == 0)
                    template.SlotItems.Remove(update.Key);
                else
                    template.SlotItems[update.Key] = update.Value;
            }
        }

        // ---- Slot utility ----

        public static bool IsManagedEquipmentSlot(ushort slotId)
        {
            return _managedEquipmentSlots.Contains(slotId);
        }

        public static bool IsManagedItemInfo(Item_Info info)
        {
            return info != null && IsManagedSlotCandidate(info.SlotId);
        }

        private static bool IsManagedSlotCandidate(ushort slotId)
        {
            if (slotId == (ushort)EquipSlot.EITHER_HAND)
                return true;

            return _managedEquipmentSlots.Contains(slotId);
        }

        public static bool CanOccupySlot(Item_Info item, ushort slotId)
        {
            if (item == null)
                return false;

            EquipSlot itemSlot = (EquipSlot)item.SlotId;
            EquipSlot equipSlot = (EquipSlot)slotId;

            if (slotId == item.SlotId)
                return true;

            if (itemSlot == EquipSlot.EITHER_HAND && (equipSlot == EquipSlot.MAIN_HAND || equipSlot == EquipSlot.OFF_HAND))
                return true;

            if ((itemSlot == EquipSlot.POCKET_1 || itemSlot == EquipSlot.POCKET_2) && (equipSlot == EquipSlot.POCKET_1 || equipSlot == EquipSlot.POCKET_2))
                return true;

            if (itemSlot >= EquipSlot.JEWELLERY_1 && itemSlot <= EquipSlot.JEWELLERY_4
                && equipSlot >= EquipSlot.JEWELLERY_1 && equipSlot <= EquipSlot.JEWELLERY_4)
                return true;

            return false;
        }

        public static bool CanUseItemInLoadout(byte careerLine, byte race, long playerSkills, byte level, byte renownRank, ushort slotId, Item_Info item, ISet<uint> uniqueEquipped = null)
        {
            if (item == null || !IsManagedItemInfo(item))
                return false;

            if (!CanOccupySlot(item, slotId))
                return false;

            if (!ItemsInterface.CanUseForCharacter(item, careerLine, race, playerSkills, level, renownRank))
                return false;

            return ItemsInterface.CanUseItemTypeForCareer(item, careerLine, race, playerSkills, slotId, uniqueEquipped);
        }

        // ---- Private helpers ----

        private static Loadout BuildStartingLoadout(byte careerLine)
        {
            Loadout loadout = new Loadout();

            foreach (CharacterInfo_item templateItem in CharMgr.GetCharacterInfoItem(careerLine))
            {
                Item_Info info = ItemService.GetItem_Info(templateItem.Entry);
                if (!IsManagedItemInfo(info))
                    continue;

                ushort slotId = IsManagedEquipmentSlot(templateItem.SlotId) ? templateItem.SlotId : info.SlotId;
                if (!CanOccupySlot(info, slotId))
                    continue;

                loadout.SlotItems[slotId] = templateItem.Entry;
            }

            return loadout.SlotItems.Count > 0 ? loadout : null;
        }

        private static void ApplyOverrides(Loadout loadout, uint characterId, byte careerLine, byte race, long playerSkills, byte level, byte renownRank)
        {
            Dictionary<ushort, uint> overrides = BotGearOverrideService.GetOverrideEntries(characterId);
            if (overrides.Count == 0)
                return;

            HashSet<uint> uniqueEquipped = BuildUniqueEntrySet(loadout);
            foreach (KeyValuePair<ushort, uint> overrideEntry in overrides.OrderBy(entry => entry.Key))
            {
                ushort slotId = overrideEntry.Key;
                if (!IsManagedEquipmentSlot(slotId))
                {
                    Log.Error("BotLoadoutManager", $"Ignoring bot gear override for CharacterId {characterId}: slot {slotId} is not managed.");
                    continue;
                }

                uint previousEntry = 0;
                Item_Info previousInfo = null;
                if (loadout.SlotItems.TryGetValue(slotId, out previousEntry))
                {
                    previousInfo = ItemService.GetItem_Info(previousEntry);
                    if (previousInfo?.UniqueEquiped == 1)
                        uniqueEquipped.Remove(previousEntry);
                }

                Item_Info itemInfo = ItemService.GetItem_Info(overrideEntry.Value);
                if (!CanUseItemInLoadout(careerLine, race, playerSkills, level, renownRank, slotId, itemInfo, uniqueEquipped))
                {
                    Log.Error("BotLoadoutManager", $"Ignoring invalid bot gear override for CharacterId {characterId}: item {overrideEntry.Value} in slot {slotId}.");
                    if (previousInfo?.UniqueEquiped == 1)
                        uniqueEquipped.Add(previousEntry);
                    continue;
                }

                loadout.SlotItems[slotId] = overrideEntry.Value;
                if (itemInfo.UniqueEquiped == 1)
                    uniqueEquipped.Add(itemInfo.Entry);
            }
        }

        private static HashSet<uint> BuildUniqueEntrySet(Loadout loadout)
        {
            HashSet<uint> uniqueEquipped = new HashSet<uint>();
            foreach (uint itemEntry in loadout.SlotItems.Values)
            {
                Item_Info info = ItemService.GetItem_Info(itemEntry);
                if (info?.UniqueEquiped == 1)
                    uniqueEquipped.Add(itemEntry);
            }

            return uniqueEquipped;
        }

        // ---- Template registration ----

        private static void RegisterAllTemplates()
        {
            // All explicit templates are registered under BotTier.T4.
            // T1/T2/T3 fall back to BuildStartingLoadout at runtime.
            // SC starts empty — the editor is the intended way to populate it.

            // Rune Priest — Healer A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_RUNE_PRIEST, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 129838067),
                Entry(EquipSlot.HELM, 129838068),
                Entry(EquipSlot.SHOULDER, 129838069),
                Entry(EquipSlot.GLOVES, 129838070),
                Entry(EquipSlot.BOOTS, 129838071),
                Entry(EquipSlot.BELT, 5850423),
                Entry(EquipSlot.BACK, 473671),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838516),
                Entry(EquipSlot.JEWELLERY_2, 129838623),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 5850421),
                Entry(EquipSlot.MAIN_HAND, 2000157));

            // Rune Priest — Healer B (cloned from A; user can customise)
            CloneTemplate((byte)CareerLine.CAREERLINE_RUNE_PRIEST, BotTier.T4, 0, 1);

            // Engineer — Ranged A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_ENGINEER, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 475376),
                Entry(EquipSlot.HELM, 472976),
                Entry(EquipSlot.SHOULDER, 477008),
                Entry(EquipSlot.GLOVES, 2017018),
                Entry(EquipSlot.BOOTS, 434219),
                Entry(EquipSlot.BELT, 476936),
                Entry(EquipSlot.BACK, 473648),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 5850417),
                Entry(EquipSlot.MAIN_HAND, 2000112),
                Entry(EquipSlot.RANGED_WEAPON, 2000165));

            // Engineer — Ranged B (cloned from A)
            CloneTemplate((byte)CareerLine.CAREERLINE_ENGINEER, BotTier.T4, 0, 1);

            // Iron Breaker — Shield Tank A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_IRON_BREAKER, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 435680),
                Entry(EquipSlot.HELM, 435704),
                Entry(EquipSlot.SHOULDER, 435692),
                Entry(EquipSlot.GLOVES, 435728),
                Entry(EquipSlot.BOOTS, 435716),
                Entry(EquipSlot.BELT, 435740),
                Entry(EquipSlot.BACK, 473669),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 129838781),
                Entry(EquipSlot.MAIN_HAND, 475085),
                Entry(EquipSlot.OFF_HAND, 472901));

            // Iron Breaker — 2H DPS B (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_IRON_BREAKER, BotTier.T4, 1,
                Entry(EquipSlot.BODY, 435680),
                Entry(EquipSlot.HELM, 472973),
                Entry(EquipSlot.SHOULDER, 435692),
                Entry(EquipSlot.GLOVES, 435728),
                Entry(EquipSlot.BOOTS, 435716),
                Entry(EquipSlot.BELT, 435740),
                Entry(EquipSlot.BACK, 473645),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 472877),
                Entry(EquipSlot.MAIN_HAND, 2000149));

            // Slayer — Melee DPS A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_SLAYER, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 475374),
                Entry(EquipSlot.HELM, 472974),
                Entry(EquipSlot.SHOULDER, 477006),
                Entry(EquipSlot.GLOVES, 435729),
                Entry(EquipSlot.BOOTS, 434217),
                Entry(EquipSlot.BELT, 435741),
                Entry(EquipSlot.BACK, 473646),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 472878),
                Entry(EquipSlot.MAIN_HAND, 2000149));

            // Slayer — Melee DPS B (cloned from A)
            CloneTemplate((byte)CareerLine.CAREERLINE_SLAYER, BotTier.T4, 0, 1);

            // Shaman — Healer A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_SHAMAN, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 2017032),
                Entry(EquipSlot.HELM, 2017030),
                Entry(EquipSlot.SHOULDER, 2017031),
                Entry(EquipSlot.GLOVES, 2017033),
                Entry(EquipSlot.BOOTS, 2017034),
                Entry(EquipSlot.BELT, 5850423),
                Entry(EquipSlot.BACK, 473719),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838516),
                Entry(EquipSlot.JEWELLERY_2, 129838623),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 5850421),
                Entry(EquipSlot.MAIN_HAND, 2000157));

            // Shaman — Healer B (cloned from A)
            CloneTemplate((byte)CareerLine.CAREERLINE_SHAMAN, BotTier.T4, 0, 1);

            // Squig Herder — Ranged A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_SQUIG_HERDER, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 475424),
                Entry(EquipSlot.HELM, 473024),
                Entry(EquipSlot.SHOULDER, 477056),
                Entry(EquipSlot.GLOVES, 418471),
                Entry(EquipSlot.BOOTS, 434291),
                Entry(EquipSlot.BELT, 476984),
                Entry(EquipSlot.BACK, 473696),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 5850417),
                Entry(EquipSlot.MAIN_HAND, 2000155),
                Entry(EquipSlot.OFF_HAND, 129838247),
                Entry(EquipSlot.RANGED_WEAPON, 2000164));

            // Squig Herder — Ranged B (cloned from A)
            CloneTemplate((byte)CareerLine.CAREERLINE_SQUIG_HERDER, BotTier.T4, 0, 1);

            // Black Orc — Shield Tank A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_BLACK_ORC, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 435752),
                Entry(EquipSlot.HELM, 435776),
                Entry(EquipSlot.SHOULDER, 435764),
                Entry(EquipSlot.GLOVES, 435800),
                Entry(EquipSlot.BOOTS, 435788),
                Entry(EquipSlot.BELT, 435812),
                Entry(EquipSlot.BACK, 473717),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 129838781),
                Entry(EquipSlot.MAIN_HAND, 475133),
                Entry(EquipSlot.OFF_HAND, 472949));

            // Black Orc — 2H DPS B (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_BLACK_ORC, BotTier.T4, 1,
                Entry(EquipSlot.BODY, 435752),
                Entry(EquipSlot.HELM, 473021),
                Entry(EquipSlot.SHOULDER, 435764),
                Entry(EquipSlot.GLOVES, 435800),
                Entry(EquipSlot.BOOTS, 435788),
                Entry(EquipSlot.BELT, 435812),
                Entry(EquipSlot.BACK, 473693),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 472925),
                Entry(EquipSlot.MAIN_HAND, 2000144));

            // Choppa — Melee DPS A (T4)
            RegisterTemplate((byte)CareerLine.CAREERLINE_CHOPPA, BotTier.T4, 0,
                Entry(EquipSlot.BODY, 2017027),
                Entry(EquipSlot.HELM, 2017025),
                Entry(EquipSlot.SHOULDER, 2017026),
                Entry(EquipSlot.GLOVES, 2017028),
                Entry(EquipSlot.BOOTS, 2017029),
                Entry(EquipSlot.BELT, 476982),
                Entry(EquipSlot.BACK, 473694),
                Entry(EquipSlot.POCKET_1, 129838806),
                Entry(EquipSlot.POCKET_2, 129838811),
                Entry(EquipSlot.JEWELLERY_1, 129838623),
                Entry(EquipSlot.JEWELLERY_2, 129838516),
                Entry(EquipSlot.JEWELLERY_3, 129838737),
                Entry(EquipSlot.JEWELLERY_4, 472926),
                Entry(EquipSlot.MAIN_HAND, 2000144));

            // Choppa — Melee DPS B (cloned from A)
            CloneTemplate((byte)CareerLine.CAREERLINE_CHOPPA, BotTier.T4, 0, 1);
        }

        private static void RegisterTemplate(byte careerLine, BotTier tier, byte variantIndex, params SlotEntry[] entries)
        {
            _templates[new TemplateKey(careerLine, tier, variantIndex)] = CreateLoadout(entries);
        }

        private static void CloneTemplate(byte careerLine, BotTier tier, byte sourceVariant, byte destVariant)
        {
            TemplateKey srcKey = new TemplateKey(careerLine, tier, sourceVariant);
            if (_templates.TryGetValue(srcKey, out Loadout source))
                _templates[new TemplateKey(careerLine, tier, destVariant)] = source.Clone();
        }

        private static Loadout CreateLoadout(IEnumerable<SlotEntry> entries)
        {
            Loadout loadout = new Loadout();

            foreach (SlotEntry entry in entries)
            {
                Item_Info itemInfo = ItemService.GetItem_Info(entry.Entry);
                if (itemInfo == null)
                {
                    Log.Error("BotLoadoutManager", $"Template item {entry.Entry} is missing from item_infos.");
                    continue;
                }

                if (!CanOccupySlot(itemInfo, entry.SlotId))
                {
                    Log.Error("BotLoadoutManager", $"Template item {entry.Entry} cannot occupy slot {entry.SlotId}.");
                    continue;
                }

                loadout.SlotItems[entry.SlotId] = entry.Entry;
            }

            return loadout;
        }

        private static SlotEntry Entry(EquipSlot slot, uint entry)
        {
            return new SlotEntry((ushort)slot, entry);
        }

        private readonly struct SlotEntry
        {
            public SlotEntry(ushort slotId, uint entry)
            {
                SlotId = slotId;
                Entry = entry;
            }

            public ushort SlotId { get; }
            public uint Entry { get; }
        }

        private readonly struct TemplateKey : IEquatable<TemplateKey>
        {
            public TemplateKey(byte careerLine, BotTier tier, byte variantIndex)
            {
                CareerLine = careerLine;
                Tier = tier;
                VariantIndex = variantIndex;
            }

            public byte CareerLine { get; }
            public BotTier Tier { get; }
            public byte VariantIndex { get; }

            public bool Equals(TemplateKey other)
            {
                return CareerLine == other.CareerLine && Tier == other.Tier && VariantIndex == other.VariantIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is TemplateKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = CareerLine * 397;
                    h = (h ^ (int)Tier) * 397;
                    return h ^ VariantIndex;
                }
            }
        }
    }
}






