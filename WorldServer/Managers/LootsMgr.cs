using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.Managers
{
    public class LootInfo
    {
        public LootInfo(Item_Info Item)
        {
            this.Item = Item;
        }

        public Item_Info Item;
    }

    public class LootContainer
    {
        public uint Money;
        public List<LootInfo> LootInfo;

        public LootContainer()
        {
            LootInfo = new List<LootInfo>();
        }

        public bool IsLootable()
        {
            return LootCount > 0 || Money > 0;
        }

        public int LootCount => LootInfo?.Count ?? 0;

        public void SendInteract(Player player, InteractMenu menu, Creature crea = null)
        {
            if (Money > 0)
            {
                if (player.PriorityGroup == null)
                {
                    if (player.GldInterface.IsInGuild())
                        player.GldInterface.ApplyTaxTithe(ref Money);

                    player.AddMoney(Money);
                }
                else
                    player.PriorityGroup.AddMoney(player, Money);
                Money = 0;
            }

            switch (menu.Menu)
            {
                case 15: // Closing loot window.
                    return;

                case 13:
                    TakeAll(player, false);
                    if (crea != null && crea.Spawn.Proto.LairBoss == false) crea.IsActive = false;
                    break;

                case 12:
                    if (TakeLoot(player, menu.Num))
                        ClientUpdateLoot(player);

                    if (crea != null && LootCount < 1)
                        if (crea != null && crea.Spawn.Proto.LairBoss == false) crea.IsActive = false;
                    break;

                default:
                    ClientUpdateLoot(player);

                    if (crea != null && LootCount < 1)
                        if (crea != null && crea.Spawn.Proto.LairBoss == false) crea.IsActive = false;
                    break;
            }
        }

        public bool TakeLoot(Player player, byte slot)
        {
            if (slot >= LootInfo.Count || LootInfo[slot] == null)
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_CANT_LOOT_THAT);
                return false;
            }

            ItemResult result = player.ItmInterface.CreateItem(LootInfo[slot].Item, 1);

            if (result == ItemResult.RESULT_OK)
            {
                if (LootInfo[slot].Item.Rarity > 0)
                    player.SendLocalizeString(new[] { LootInfo[slot].Item.Name, "1" }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                LootInfo.RemoveAt(slot);
                return true;
            }

            if (result == ItemResult.RESULT_MAX_BAG)
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_OVERAGE_CANT_LOOT);

            return false;
        }

        public void TakeAll(Player player, bool announce)
        {
            if (player.ItmInterface.GetTotalFreeInventorySlot() < LootCount)
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_OVERAGE_CANT_LOOT);
                return;
            }

            int initialCount = LootInfo.Count;

            while (LootInfo.Count > 0)
            {
                if (!TakeLoot(player, 0))
                    break;
            }

            if (initialCount > LootInfo.Count)
                ClientUpdateLoot(player);
        }

        private void ClientUpdateLoot(Player player)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 32);
            Out.WriteByte(4);
            Out.WriteByte((byte)LootCount);
            for (byte i = 0; i < LootInfo.Count; ++i)
            {
                if (LootInfo[i] == null)
                    continue;

                Out.WriteByte(i);
                Item.BuildItem(ref Out, null, LootInfo[i].Item, null, 0, 1);
            }
            player.SendPacket(Out);
        }
    }

    public static class LootsMgr
    {
        /*private const float DROP_BONUS_CHAMP = 0.2f;
        private const float DROP_BONUS_HERO = 0.04f;

        private const float RARITY_COMMON_GEAR = 20f;

        private static readonly float[] RARITIES = { 5f, 0.4f, 0.02f };

        private const byte MAX_EASE_IN_LEVEL = 7;*/

        public static LootContainer GenerateLoot(Unit corpse, Unit looter, float dropMod)
        {
            // Declare kill event constants for bitfields
            const byte KILL_EVENT_SCENARIO = 1;
            const byte KILL_EVENT_RVR = 2;
            const byte KILL_EVENT_PVE = 4;

            // If the killer isn't a player or a pet, don't bother looting.
            Player player = null;

            if (looter == null)
                return null;

            if (looter.IsPet())
                player = (looter as Pet).Owner;
            else if (looter.IsPlayer())
                player = looter as Player;

            if (player == null)
                return null;

            // Initialize lootgroups we'll be searching through
            List<Loot_Group> lootGroups;

            Player deadPlayer = corpse as Player;

            // If the corpse is a player, and you've made it this far, this was a PVP kill
            if (deadPlayer != null)
            {
                List<LootInfo> lootList = new List<LootInfo>();

                uint corpseCareer = (uint) Math.Pow(2, deadPlayer.Info.CareerLine - 1);
                uint corpseLevel = deadPlayer.AdjustedLevel;
                uint corpseRenown = deadPlayer.AdjustedRenown;

                // Scenario zones all have a war_world.zone_infos.type value of 1.
                // Note - if "1" refers to "instance" instead of "scenario", there could be problems
                // with players receiving scenario-restricted loot for a PVP kill made in an instance 
                lootGroups = CreatureService.GetLootGroupsByEvent(deadPlayer.Zone.Info.Type == 1 ? KILL_EVENT_SCENARIO : KILL_EVENT_RVR);

                // This will be our narrowed down list of loot groups that are relevant to the kill in question.
                List<Loot_Group> candidateLootGroups = new List<Loot_Group>();

                // Whittle down the loot groups into a new candidate list, based on the killed player's career, 
                // and whether the kill occurred in the correct zone (if any) and whether the player has the required quest (if any)
                foreach (Loot_Group lg in lootGroups)
                {
                    if (lg == null)
                        continue;

                    if (lg.ReqActiveQuest > 0)
                    {
                        Character_quest quest = player.QtsInterface.GetQuest(lg.ReqActiveQuest);
                        if (quest == null || quest.IsDone())
                            continue;
                    }

                    if ((corpseCareer & lg.CreatureID) != corpseCareer)
                        continue;
                        
                    if (lg.SpecificZone != 0 && deadPlayer.Zone.Info.ZoneId != lg.SpecificZone)
                        continue;

                    candidateLootGroups.Add(lg);
                }

                // Generate items from remaining loot groups
                foreach (Loot_Group lg in candidateLootGroups)
                {
                    if (lg == null)
                        continue;
                    // roll for drops.
                    for (int groupIndex = 0; groupIndex < lg.DropCount; groupIndex++)
                    {
                        float roll = (float)StaticRandom.Instance.Next(0, 10000) / 10000;

                        if (roll <= lg.DropChance)
                        {
                            // Assemble valid drops from the group
                            List<Loot_Group_Item> candidateItems = new List<Loot_Group_Item>();

                            // This whole if is horrible spaghetti, we need to add each new medallion type here
                            if (Constants.DoomsdaySwitch > 0 && lg.Entry == 1 && WorldMgr.WorldSettingsMgr.GetMedallionsSetting() == 1 && looter.GetPlayer().ScnInterface.Scenario == null && deadPlayer.Level > 15)
                            {
                                foreach (Loot_Group_Item lgi in lg.LootGroupItems)
                                {
                                    if (lgi == null)
                                        continue;
                                    if (deadPlayer.Level < 41 && deadPlayer.RenownRank < 41 && corpseRenown < looter.GetPlayer().RenownRank && looter.RenownRank > 40)
                                    {
                                        if (lgi.ItemID == 208470 || lgi.ItemID == 208470 || lgi.ItemID == 208470)
                                            candidateItems.Add(lgi);
                                    }
                                    else
                                    {
                                        if (corpseLevel < lgi.MinRank || corpseLevel > lgi.MaxRank)
                                            continue;

                                        if (corpseRenown < lgi.MinRenown || corpseRenown > lgi.MaxRenown)
                                            continue;

                                        Item_Info itemDef = ItemService.GetItem_Info(lgi.ItemID);

                                        if (itemDef.Realm != 0 && itemDef.Realm != (byte)player.Realm)
                                            continue;

                                        candidateItems.Add(lgi);
                                    }
                                }
                            }
                            else
                            {
                                foreach (Loot_Group_Item lgi in lg.LootGroupItems)
                                {
                                    if (lgi == null)
                                        continue;

                                    if (corpseLevel < lgi.MinRank || corpseLevel > lgi.MaxRank)
                                        continue;

                                    if (corpseRenown < lgi.MinRenown || corpseRenown > lgi.MaxRenown)
                                        continue;

                                    Item_Info itemDef = ItemService.GetItem_Info(lgi.ItemID);

                                    if (itemDef.Realm != 0 && itemDef.Realm != (byte)player.Realm)
                                        continue;

                                    candidateItems.Add(lgi);
                                }
                            }

                            // If the loot group requires that the dropped loot be usable by a member of 
                            // the killer's party, then remove non-compatible loot from the candidate list.
                            if (lg.ReqGroupUsable)
                            {
                                List<Player> members;

                                if (player.PriorityGroup != null)
                                    members = player.PriorityGroup.GetPlayerListCopy();
                                else
                                {
                                    members = new List<Player> { player };
                                }
                                for (int itemIndex = 0; itemIndex < candidateItems.Count; ++itemIndex)
                                {
                                    bool valid = false;

                                    Item_Info curItem = ItemService.GetItem_Info(candidateItems[itemIndex].ItemID);

                                    foreach (Player member in members)
                                    {
                                        if (!ItemsInterface.CanUse(curItem, member, true, false))
                                            continue;

                                        // Usable by at least one member
                                        valid = true;
                                        break;
                                    }

                                    if (!valid)
                                    {
                                        candidateItems.RemoveAt(itemIndex);
                                        --itemIndex;
                                    }
                                }
                            }

                            // Now roll for an item from the candidate item list, and add it to the loots.
                            if (candidateItems.Count > 0)
                            {
                                Item_Info winningItem = ItemService.GetItem_Info(candidateItems[StaticRandom.Instance.Next(0, candidateItems.Count)].ItemID);
                                lootList.Add(new LootInfo(winningItem));
                            }
                        }
                    }
                }

                // Generate money as normal, and pass the generated loot back to the system.
                uint money = corpseLevel * 5 + corpseRenown * 5;

                if (lootList.Count > 0 || money > 0)
                {
                    LootContainer lt = new LootContainer
                    {
                        Money = money,
                        LootInfo = lootList
                    };
                    //corpse.EvtInterface.Notify(EventName.ON_GENERATE_LOOT, looter, Lt);
                    return lt;
                }

            }

            else if (corpse.IsCreature())
            {
                Creature deadCreature = corpse.GetCreature();
                List<LootInfo> Loots = new List<LootInfo>();

                // All creatures are by definition PVE kills
                lootGroups = CreatureService.GetLootGroupsByEvent(KILL_EVENT_PVE);

                // This will be our narrowed down list of loot groups that are relevant to the kill in question.
                List<Loot_Group> candidateLootGroups = new List<Loot_Group>();

                // Whittle down the loot groups into a new candidate list, based on the killed creature's ID,
                // or the CreatureSubType if the loot group's CreatureID is 0.
                foreach (Loot_Group lg in lootGroups)
                {
                    if (lg == null)
                        continue;

                    if (lg.ReqActiveQuest != 0 && !player.QtsInterface.HasQuest(lg.ReqActiveQuest))
                        continue;
                    if (lg.SpecificZone != 0 && deadCreature.Zone.Info.ZoneId != lg.SpecificZone)
                        continue;

                    if (lg.CreatureID != 0)
                    {
                        if (deadCreature.Entry == lg.CreatureID)
                            candidateLootGroups.Add(lg);
                    }
                    else
                    {
                        if (lg.CreatureSubType == 0 || CreatureService.GetCreatureProto(deadCreature.Entry).CreatureSubType == lg.CreatureSubType)
                            candidateLootGroups.Add(lg);
                    }
                }



                // Generate items from remaining loot groups
                foreach (Loot_Group lg in candidateLootGroups)
                { 
                    try
                    {
                        if (lg == null)
                            continue;
                        // roll for drops.
                        for (int groupIndex = 0; groupIndex < lg.DropCount; groupIndex++)
                        {
                            float roll = (float)StaticRandom.Instance.Next(0, 10000) / 10000;

                            if (roll <= lg.DropChance)
                            {
                                // Assemble valid drops from the group
                                List<Loot_Group_Item> candidateItems = new List<Loot_Group_Item>();

                                int effectiveLevel = Math.Min(Program.Config.RankCap, deadCreature.Level);

                                if (lg.LootGroupItems != null)
                                {
                                    foreach (Loot_Group_Item lgi in lg.LootGroupItems)
                                    {
                                        try
                                        {
                                            if (lgi == null)
                                                continue;

                                            if (effectiveLevel < lgi.MinRank || effectiveLevel > lgi.MaxRank)
                                                continue;

                                            Item_Info itemDef = ItemService.GetItem_Info(lgi.ItemID);

                                            if (itemDef.Realm != 0 && itemDef.Realm != (byte)player.Realm)
                                                continue;

                                            candidateItems.Add(lgi);
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }

                                // If the loot group requires that the dropped loot be usable by a member of 
                                // the killer's party, then remove non-compatible loot from the candidate list.
                                if (lg.ReqGroupUsable)
                                {
                                    List<Player> members;

                                    if (player.PriorityGroup != null)
                                        members = player.PriorityGroup.GetPlayerListCopy();
                                    else
                                        members = new List<Player> { player };


                                    for (int itemIndex = 0; itemIndex < candidateItems.Count; ++itemIndex)
                                    {
                                        bool valid = false;

                                        Item_Info curItem = ItemService.GetItem_Info(candidateItems[itemIndex].ItemID);

                                        foreach (Player member in members)
                                        {
                                            if (!ItemsInterface.CanUse(curItem, member, true, false))
                                                continue;

                                            valid = true;
                                            break;
                                        }

                                        if (!valid)
                                        {
                                            candidateItems.RemoveAt(itemIndex);
                                            --itemIndex;
                                        }
                                    }
                                }

                                // Now roll for an item from the candidate item list, and add it to the loots.
                                if (candidateItems.Count > 0)
                                {
                                    Item_Info winningItem = ItemService.GetItem_Info(candidateItems[StaticRandom.Instance.Next(0, candidateItems.Count)].ItemID);
                                    Loots.Add(new LootInfo(winningItem));
                                }
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                // Generate money as normal, and pass the generated loot back to the system.
                uint money = corpse.Level * (uint)7 + corpse.Rank * (uint)50;

                if (Loots.Count > 0 || money > 0)
                {
                    LootContainer lt = new LootContainer
                    {
                        Money = money,
                        LootInfo = Loots
                    };
                    //corpse.EvtInterface.Notify(EventName.ON_GENERATE_LOOT, looter, Lt);
                    return lt;
                }
            }

            else if (corpse.IsGameObject())
            {
                // This will generate gameobject loot. Currently this only shows loot
                // if a player needs an item it holds for a quest. If an object has
                // been looted already or has no loot this will return null.
                // Todo: Currently object loot always is 100%. Make this support non quest related loot.

                GameObject gameObj = corpse.GetGameObject();
                List<GameObject_loot> gameObjectLoots = GameObjectService.GetGameObjectLoots(gameObj.Spawn.Entry);
                if (gameObjectLoots.Count <= 0 || gameObj.Looted)
                    return null;

                QuestsInterface Interface = looter.QtsInterface;

                if (Interface == null)
                    return null;

                List<LootInfo> lootInfo = new List<LootInfo>();

                foreach (GameObject_loot loot in gameObjectLoots)
                {
                    foreach (KeyValuePair<ushort, Character_quest> kp in Interface.Quests)
                    {
                        if (kp.Value.Done || kp.Value.IsDone())
                            continue;

                        foreach (Character_Objectives obj in kp.Value._Objectives)
                        {
                            if (obj.IsDone() || obj.Objective.Item == null || obj.Objective.Item.Entry != loot.ItemId)
                                continue;

                            lootInfo.Add(new LootInfo(loot.Info));
                            break;
                        }
                    }
                }

                LootContainer lt = new LootContainer
                {
                    Money = 0,
                    LootInfo = lootInfo
                };
                return lt;
            }

            return null;
        }

		public static LootContainer GetScenarioLoot(Player player, int scenTier, ZoneMgr zone)
        {
            List<LootInfo> lootList = new List<LootInfo>();

            int corpseLevel = scenTier * 10;
            int corpseRenown = scenTier * 10;

            // Scenario zones all have a war_world.zone_infos.type value of 1.
            // Note - if "1" refers to "instance" instead of "scenario", there could be problems
            // with players receiving scenario-restricted loot for a PVP kill made in an instance 
            List<Loot_Group>lootGroups = CreatureService.GetLootGroupsByEvent(1);

            // This will be our narrowed down list of loot groups that are relevant to the kill in question.
            List<Loot_Group> candidateLootGroups = new List<Loot_Group>();

            // Whittle down the loot groups into a new candidate list, based on the killed player's career, 
            // and whether the kill occurred in the correct zone (if any) and whether the player has the required quest (if any)
            foreach (Loot_Group lg in lootGroups)
            {
                if (lg == null)
                    continue;

                if (lg.ReqActiveQuest > 0)
                {
                    Character_quest quest = player.QtsInterface.GetQuest(lg.ReqActiveQuest);
                    if (quest == null || quest.IsDone())
                        continue;
                }

                if (lg.SpecificZone != 0 && zone.Info.ZoneId != lg.SpecificZone)
                    continue;

                candidateLootGroups.Add(lg);
            }

            // Generate items from remaining loot groups
            foreach (Loot_Group lg in candidateLootGroups)
            {
                if (lg == null)
                    continue;
                // roll for drops.
                for (int groupIndex = 0; groupIndex < lg.DropCount; groupIndex++)
                {
                    float roll = (float)StaticRandom.Instance.Next(0, 10000) / 10000;

                    if (roll <= lg.DropChance)
                    {
                        // Assemble valid drops from the group
                        List<Loot_Group_Item> candidateItems = new List<Loot_Group_Item>();

                        foreach (Loot_Group_Item lgi in lg.LootGroupItems)
                        {
                            if (lgi == null)
                                continue;

                            if (corpseLevel < lgi.MinRank || corpseLevel > lgi.MaxRank)
                                continue;

                            if (corpseRenown < lgi.MinRenown || corpseRenown > lgi.MaxRenown)
                                continue;

                            Item_Info itemDef = ItemService.GetItem_Info(lgi.ItemID);

                            if (itemDef.Realm != 0 && itemDef.Realm != (byte)player.Realm)
                                continue;

                            candidateItems.Add(lgi);
                        }

                        // If the loot group requires that the dropped loot be usable by a member of 
                        // the killer's party, then remove non-compatible loot from the candidate list.
                        if (lg.ReqGroupUsable)
                        {
                            List<Player> members;

                            if (player.PriorityGroup != null)
                                members = player.PriorityGroup.GetPlayerListCopy();
                            else
                            {
                                members = new List<Player> { player };
                            }
                            for (int itemIndex = 0; itemIndex < candidateItems.Count; ++itemIndex)
                            {
                                bool valid = false;

                                Item_Info curItem = ItemService.GetItem_Info(candidateItems[itemIndex].ItemID);

                                foreach (Player member in members)
                                {
                                    if (!ItemsInterface.CanUse(curItem, member, true, false))
                                        continue;

                                    // Usable by at least one member
                                    valid = true;
                                    break;
                                }

                                if (!valid)
                                {
                                    candidateItems.RemoveAt(itemIndex);
                                    --itemIndex;
                                }
                            }
                        }

                        // Now roll for an item from the candidate item list, and add it to the loots.
                        if (candidateItems.Count > 0)
                        {
                            Item_Info winningItem = ItemService.GetItem_Info(candidateItems[StaticRandom.Instance.Next(0, candidateItems.Count)].ItemID);
                            lootList.Add(new LootInfo(winningItem));
                        }
                    }

                }
            }

            return lootList.Count <= 0 ? null : new LootContainer { LootInfo = lootList };
        }
    }
}
