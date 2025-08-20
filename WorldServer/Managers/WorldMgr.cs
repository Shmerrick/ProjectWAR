using Common;
using Common.Database.World.Characters;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using SystemData;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Objects.Instances;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios;
using WorldServer.World.Scripting;
using WorldServer.World.WorldSettings;
using BattleFrontConstants = WorldServer.World.Battlefronts.Apocalypse.BattleFrontConstants;
using Item = WorldServer.World.Objects.Item;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.Managers
{
    // This is the World Manager. It's like the brain of the entire game world.
    // It knows about everything that's happening, from the different lands (regions) and areas (zones)
    // to the players, monsters, and big battles. It starts up all the different parts of the game
    // and makes sure they all work together.
    [Service(
        typeof(AnnounceService), // For sending messages to everyone
        typeof(BattleFrontService), // For the big Realm vs. Realm battles
        typeof(BountyService), // For putting bounties on players
        typeof(CellSpawnService), // For making monsters and objects appear in the world
        typeof(ChapterService), // For the story chapters in each area
        typeof(CreatureService), // For all the monsters and animals
        typeof(DyeService), // For changing the color of your armor
        typeof(GameObjectService), // For things you can interact with, like doors and chests
        typeof(GuildService), // For player guilds
        typeof(ItemService), // For all the items in the game
        typeof(PQuestService), // For Public Quests
        typeof(QuestService), // For regular quests
        typeof(RallyPointService), // For rally points that let you respawn closer to the action
        typeof(RVRProgressionService), // For tracking the progress of the big war
        typeof(RewardService), // For giving out rewards
        typeof(ScenarioService), // For the small, instanced battles
        typeof(TokService), // For the "Tome of Knowledge" achievements
        typeof(VendorService), // For the shopkeepers
        typeof(WaypointService), // For flight paths
        typeof(XpRenownService), // For experience and renown points
        typeof(ZoneService))] // For all the different zones in the world
    public static class WorldMgr
    {
        // This is our connection to the world database, where all the information about the world is stored.
        public static IObjectDatabase Database;
        // These are special "threads" that run in the background to keep the world and player groups updated.
        private static Thread _worldThread;
        private static Thread _groupThread;
        // This is a switch to tell the threads to keep running or to stop.
        private static bool _running = true;
        // This decides which big battle pairing is active when the server starts.
        public static long StartingPairing;

        // This tells us if the server is in "Developer" mode or "Production" (live) mode.
        public static string ServerMode;

        // These manage the big campaigns for high-level and low-level players.
        public static UpperTierCampaignManager UpperTierCampaignManager;
        public static LowerTierCampaignManager LowerTierCampaignManager;
        // This is for writing messages to the server's log file.
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        // This keeps track of the big RvR areas.
        public static RVRArea RVRArea = new RVRArea();

        #region Region

        // This is a list of all the big regions in our game world.
        public static List<RegionMgr> _Regions = new List<RegionMgr>();
        // This helps us safely add or remove regions from the list without causing problems.
        private static ReaderWriterLockSlim RegionsRWLock = new ReaderWriterLockSlim();

        // This gets a specific region from our list. If it doesn't exist, it can create a new one.
        public static RegionMgr GetRegion(ushort RegionId, bool Create, string name = "")
        {
            // We safely read from the list to find the region.
            RegionsRWLock.EnterReadLock();
            RegionMgr Mgr = _Regions.Find(region => region != null && region.RegionId == RegionId);
            RegionsRWLock.ExitReadLock();

            // If we didn't find it and we're allowed to create it, we make a new one.
            if (Mgr == null && Create)
            {
                Mgr = new RegionMgr(RegionId, ZoneService.GetZoneRegion(RegionId), name, new ApocCommunications());
                // We safely write to the list to add the new region.
                RegionsRWLock.EnterWriteLock();
                _Regions.Add(Mgr);
                RegionsRWLock.ExitWriteLock();
            }

            return Mgr;
        }

        // This stops the entire world. It's called when the server is shutting down.
        public static void Stop()
        {
            Log.Success("WorldMgr", "Stop");
            // It tells every region to stop.
            foreach (RegionMgr Mgr in _Regions)
                Mgr.Stop();

            // It also stops the scenarios and the background threads.
            ScenarioMgr.Stop();
            _running = false;
        }

        #endregion Region

        #region Zones

        // This figures out where a player should respawn when they die.
        public static SpawnPoint GetZoneRespawn(ushort zoneId, byte realm, Player player)
        {
            // If we don't know who the player is, we use the default respawn point for their realm.
            if (player == null)
            {
                return new SpawnPoint(ZoneService.GetZoneRespawn(zoneId, realm));
            }

            // If the player is in a specific area...
            if (player.CurrentArea != null)
            {
                // If the player is in a Public Quest, they respawn at the PQ's special respawn point.
                if (player.QtsInterface.PublicQuest != null)
                {
                    var pqRespawns = ZoneService.GetZoneRespawns(zoneId);
                    foreach (var res in pqRespawns)
                        if (res.Realm == 0 &&
                            res.ZoneID == zoneId &&
                            res.RespawnID == player.QtsInterface.PublicQuest.Info.RespawnID)
                            return new SpawnPoint(res);
                }

                // If the player is in a Scenario, they respawn at one of the scenario's starting points.
                if (player.ScnInterface.Scenario != null)
                {
                    List<Zone_Respawn> respawns = ZoneService.GetZoneRespawns(zoneId);
                    List<Zone_Respawn> options = new List<Zone_Respawn>();
                    foreach (Zone_Respawn res in respawns)
                    {
                        if (res.Realm != realm)
                            continue;

                        options.Add(res);
                    }

                    return new SpawnPoint(options.Count == 1 ? options[0] : options[StaticRandom.Instance.Next(options.Count)]);
                }

                // If the player is near a Keep they control, they might respawn there.
                if (player.CurrentKeep != null)
                {
                    _logger.Debug($"Player {player.Name} is attached to keep {player.CurrentKeep.Name} - using Keep respawn");
                    return player.CurrentKeep.GetSpawnPoint(player);
                }

                // Otherwise, we use the respawn point defined for their current area.
                ushort respawnId = realm == 1
                    ? player.CurrentArea.OrderRespawnId
                    : player.CurrentArea.DestroRespawnId;

                if (respawnId > 0)
                {
                    // PVE respawn
                    _logger.Debug($"Player {player.Name} Area:{player.CurrentArea.AreaId} PVE respawn {respawnId}");
                    return new SpawnPoint(ZoneService.GetZoneRespawn(respawnId));
                }
                else
                {
                    // Crude patch - if no currentarea, respawn into current zoneid
                    _logger.Warn(
                        $"Respawning player {player.Name} from respawnId=0 area {player.CurrentArea.AreaId}");
                    return new SpawnPoint(ZoneService.GetZoneRespawn(zoneId, realm));
                }
            }
            else
            {
                // If we don't know the player's area, we find the closest valid respawn point to where they died.
                List<Zone_Respawn> respawns = ZoneService.GetZoneRespawns(zoneId);
                float lastDistance = float.MaxValue;

                foreach (Zone_Respawn res in respawns)
                {
                    if (res.Realm != realm)
                        continue;

                    var pos = new Point3D(res.PinX, res.PinY, res.PinZ);
                    float distance = pos.GetDistance(player);

                    if (distance < lastDistance)
                    {
                        lastDistance = distance;
                        _logger.Debug($"Player {player.Name} Zone {zoneId} World respawn");
                        return new SpawnPoint(res);
                    }
                }

                // Crude patch - if no currentarea, respawn into current zoneid
                _logger.Warn($"Respawning player {player.Name} from NULL area");
                return new SpawnPoint(ZoneService.GetZoneRespawn(zoneId, realm));
            }
        }

        // This gets a list of all the flight masters (taxis) that a player can use.
        public static List<Zone_Taxi> GetTaxis(Player Plr)
        {
            List<Zone_Taxi> L = new List<Zone_Taxi>();

            Zone_Taxi[] Taxis;
            foreach (KeyValuePair<ushort, Zone_Taxi[]> Kp in ZoneService._Zone_Taxi)
            {
                Taxis = Kp.Value;
                if (Taxis[(byte)Plr.Realm] == null || Taxis[(byte)Plr.Realm].WorldX == 0)
                    continue;

                if (Taxis[(byte)Plr.Realm].Info == null)
                    Taxis[(byte)Plr.Realm].Info = ZoneService.GetZone_Info(Taxis[(byte)Plr.Realm].ZoneID);

                if (Taxis[(byte)Plr.Realm].Info == null)
                    continue;

                if (Taxis[(byte)Plr.Realm].Enable == false)
                    continue;

                if (Taxis[(byte)Plr.Realm].Tier > 0)
                {
                    switch (Taxis[(byte)Plr.Realm].Tier)
                    {
                        case 2:
                            if (!(Plr.TokInterface.HasTok(11) || Plr.TokInterface.HasTok(44) || Plr.TokInterface.HasTok(75) || Plr.TokInterface.HasTok(140) || Plr.TokInterface.HasTok(171) || Plr.TokInterface.HasTok(107)))
                                continue;
                            break;

                        case 3:
                            if (!(Plr.TokInterface.HasTok(12) || Plr.TokInterface.HasTok(50) || Plr.TokInterface.HasTok(81) || Plr.TokInterface.HasTok(108) || Plr.TokInterface.HasTok(146) || Plr.TokInterface.HasTok(177)))
                                continue;
                            break;

                        case 4:
                            if (!(Plr.TokInterface.HasTok(18) || Plr.TokInterface.HasTok(55) || Plr.TokInterface.HasTok(86) || Plr.TokInterface.HasTok(114) || Plr.TokInterface.HasTok(182) || Plr.TokInterface.HasTok(151)))
                                continue;
                            break;
                    }
                }
                L.Add(Taxis[(byte)Plr.Realm]);
            }

            return L;
        }

        #endregion Zones

        #region Xp / Renown

        // This part handles Experience (XP) and Renown points, which are rewards for defeating enemies.

        // This calculates how much experience a player should get for defeating a monster or another player.
        // It's like a formula that considers the player's level and the enemy's level.
        public static uint GenerateXPCount(Player plr, Unit victim)
        {
            // Get the levels of the player (killer) and the enemy (victim).
            uint KLvl = plr.AdjustedLevel;
            uint VLvl = victim.AdjustedLevel;

            // If the player is much higher level than the victim, they get no XP.
            if (KLvl > VLvl + 8)
                return 0;

            // The base XP is the victim's level times 100.
            uint XP = VLvl * 100;

            // If the victim is a creature, we might give bonus XP for tougher ones.
            if (victim is Creature)
            {
                switch (victim.Rank)
                {
                    case 1: // Champion monsters give 4x XP.
                        XP *= 4; break;
                    case 2: // Hero monsters give 12x XP, but only if you're in a group.
                        if (plr.WorldGroup != null)
                            XP *= 12;
                        break;
                }
            }

            // If the player is a higher level, reduce the XP they get.
            if (KLvl > VLvl)
                XP -= (uint)((XP / (float)100) * (KLvl - VLvl + 1)) * 5;
            // If there's a special XP rate set on the server, apply it.
            else if (Core.Config.XpRate > 0)
                XP *= (uint)Core.Config.XpRate;

            // Return the final XP amount.
            return XP;
        }

        // This gives the calculated experience to the player or their group.
        public static void GenerateXP(Player killer, Unit victim, float bonusMod)
        {
            _logger.Trace($"Killer : {killer.Name} Victim : {victim.Name} Bonus : {bonusMod}");
            if (killer == null) return;

            // If the player's level is bolstered (temporarily increased), they don't get bonus XP.
            if (killer.Level != killer.EffectiveLevel)
                bonusMod = 0.0f;

            // If the player is not in a group, give the XP directly to them.
            if (killer.PriorityGroup == null)
            {
                killer.AddXp((uint)(GenerateXPCount(killer, victim) * bonusMod), true, true);
            }
            // If they are in a group, the group handles splitting the XP among members.
            else
            {
                _logger.Trace($"Priority Group : {killer.Name} Victim : {victim.Name} Bonus : {bonusMod}");
                killer.PriorityGroup.AddXpFromKill(killer, victim, bonusMod);
            }
        }

        // This calculates how much renown a player should get for defeating another player.
        // Renown is like XP, but for Realm vs. Realm combat.
        public static uint GenerateRenownCount(Player killer, Player victim)
        {
            // Can't get renown for killing yourself or nothing.
            if (killer == null || victim == null || killer == victim)
                return 0;

            // The formula for renown points.
            uint renownPoints = (uint)(7.31f * (victim.AdjustedRenown + victim.AdjustedLevel));

            // Players in a certain level range get a small bonus.
            if (killer.AdjustedLevel > 15 && killer.AdjustedLevel < 31)
                renownPoints = (uint)(renownPoints * 1.0f); // Note: This currently multiplies by 1, so no change.

            return renownPoints;
        }

        #endregion Xp / Renown

        #region items

        // This section handles everything related to items, especially buying and selling from vendors.

        // Sends a list of items from a "dynamic" vendor to a player.
        // A dynamic vendor has a list of items that can change, like special reward vendors.
        public static void SendDynamicVendorItems(Player plr, List<Vendor_items> items)
        {
            if (plr == null)
                return;

            byte Page = 0;
            int Count = items.Count;
            // Vendors can have multiple pages of items. This loop sends them one page at a time.
            while (Count > 0)
            {
                // Figure out how many items to send on this page.
                byte ToSend = (byte)Math.Min(Count, VendorService.MAX_ITEM_PAGE);
                if (ToSend <= Count)
                    Count -= ToSend;
                else
                    Count = 0;

                // Send the actual page of items.
                WorldMgr.SendVendorPage(plr, ref items, ToSend, Page);

                ++Page;
            }
            // After sending all the items, also send the player's "buyback" list (items they recently sold).
            plr.ItmInterface.SendBuyBack();
        }

        // Sends the item list of a regular, static vendor to a player.
        public static void SendVendor(Player Plr, ushort id)
        {
            if (Plr == null)
                return;

            // First, get all the items this vendor sells.
            List<Vendor_items> Itemsprecheck = VendorService.GetVendorItems(id).ToList();
            List<Vendor_items> Items = new List<Vendor_items>();

            // Check if the player meets the requirements for each item (like guild level).
            foreach (Vendor_items vi in Itemsprecheck)
            {
                if (vi.ReqGuildlvl > 0 && Plr.GldInterface.IsInGuild() && vi.ReqGuildlvl > Plr.GldInterface.Guild.Info.Level)
                    continue; // Skip this item if the player's guild level is too low.
                Items.Add(vi);
            }

            // Now, send the filtered list of items to the player, page by page.
            byte Page = 0;
            int Count = Items.Count;
            while (Count > 0)
            {
                byte ToSend = (byte)Math.Min(Count, VendorService.MAX_ITEM_PAGE);
                if (ToSend <= Count)
                    Count -= ToSend;
                else
                    Count = 0;

                SendVendorPage(Plr, ref Items, ToSend, Page);

                ++Page;
            }

            Plr.ItmInterface.SendBuyBack();
        }

        // This constructs and sends the actual network packet for a single page of vendor items.
        public static void SendVendorPage(Player Plr, ref List<Vendor_items> items, byte Count, byte Page)
        {
            Count = (byte)Math.Min(Count, items.Count);

            // Create a new network packet.
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_STORE, 256);
            Out.WriteByte(3);
            Out.WriteByte(0);
            Out.WriteByte(Page);
            Out.WriteByte(Count);
            Out.WriteByte((byte)(Page > 0 ? 0 : 1));
            Out.WriteByte(1);
            Out.WriteByte(0);

            if (Page == 0)
                Out.WriteByte(0);

            // Add each item's data to the packet.
            for (byte i = 0; i < Count; ++i)
            {
                Out.WriteByte(i);
                Out.WriteByte(1);
                Out.WriteUInt32(items[i].Price); // How much it costs.
                Item.BuildItem(ref Out, null, items[i].Info, null, 0, 1); // The item's stats and info.

                // If the item is part of a set, send the set info.
                if (Plr != null && Plr.ItmInterface != null && items[i].Info != null && items[i].Info.ItemSet != 0)
                    Plr.ItmInterface.SendItemSetInfoToPlayer(Plr, items[i].Info.ItemSet);

                // If the item requires other items to buy (like tokens), add that info.
                if ((byte)items[i].ItemsReq.Count > 0)
                {
                    Out.WriteByte(1);
                    foreach (KeyValuePair<uint, ushort> Kp in items[i].ItemsReq)
                    {
                        Item_Info item = ItemService.GetItem_Info(Kp.Key);
                        Out.WriteUInt32(Kp.Key);
                        Out.WriteUInt16((ushort)item.ModelId);
                        Out.WritePascalString(item.Name);
                        Out.WriteUInt16(Kp.Value);
                    }
                }
                // These are just for filling out the packet to the right size.
                if ((byte)items[i].ItemsReq.Count == 1)
                    Out.Fill(0, 18);
                else if ((byte)items[i].ItemsReq.Count == 2)
                    Out.Fill(0, 9);
                else
                    Out.Fill(0, 1);
            }

            Out.WriteByte(0);
            // Send the packet to the player.
            Plr.SendPacket(Out);

            // Remove the items we just sent from the list.
            items.RemoveRange(0, Count);
        }

        // This handles the logic when a player tries to buy an item from a regular vendor.
        public static void BuyItemVendor(Player Plr, InteractMenu Menu, ushort id)
        {
            // Figure out which item the player clicked on.
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            // Again, get the vendor's items and filter them based on requirements.
            List<Vendor_items> Itemsprecheck = VendorService.GetVendorItems(id).ToList();
            List<Vendor_items> Vendors = new List<Vendor_items>();

            foreach (Vendor_items vi in Itemsprecheck)
            {
                if (vi.ReqGuildlvl > 0 && Plr.GldInterface.IsInGuild() && vi.ReqGuildlvl > Plr.GldInterface.Guild.Info.Level)
                    continue;
                Vendors.Add(vi);
            }

            if (Vendors.Count <= Num)
                return; // Invalid item selected.

            // Check if the player has enough money.
            if (!Plr.HasMoney((Vendors[Num].Price) * Count))
            {
                Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_MONEY_TO_BUY);
                return;
            }

            // Check if the player has the required items (like tokens).
            foreach (KeyValuePair<uint, ushort> Kp in Vendors[Num].ItemsReq)
            {
                if (!Plr.ItmInterface.HasItemCountInInventory(Kp.Key, (ushort)(Kp.Value * Count)))
                {
                    Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_FAIL_PURCHASE_REQUIREMENT);
                    return;
                }
            }

            // Check for other requirements, like unlocking something in the Tome of Knowledge.
            if (Vendors[Num].ReqTokUnlock > 0 && !Plr.TokInterface.HasTok(Vendors[Num].ReqTokUnlock))
                return;

            // Try to create the item and put it in the player's inventory.
            ItemResult result = Plr.ItmInterface.CreateItem(Vendors[Num].Info, Count);
            if (result == ItemResult.RESULT_OK) // Success!
            {
                // Take the player's money and required items.
                Plr.RemoveMoney(Vendors[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in Vendors[Num].ItemsReq)
                    Plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));
            }
            else if (result == ItemResult.RESULT_MAX_BAG) // Inventory is full.
            {
                Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_SPACE_TO_BUY);
            }
            else if (result == ItemResult.RESULT_ITEMID_INVALID) // Something is wrong with the item itself.
            {
            }
        }

        // Handles buying from a special Honor reward vendor.
        public static void BuyItemHonorDynamicVendor(Player plr, InteractMenu Menu, List<Vendor_items> items)
        {
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            if (items.Count <= Num)
                return;

            // Check if this is a valid honor reward for the player.
            var honorVendor = new HonorVendorItem(plr);
            var reward = HonorService.HonorRewards.SingleOrDefault(x => x.ItemId == items[Num].Info.Entry);
            if (reward == null)
                return;
            if (!honorVendor.IsValidItemForPlayer(plr, reward))
                return;

            // Try to create the item.
            ItemResult result = plr.ItmInterface.CreateItem(items[Num].Info, (ushort)reward.ItemCount);
            if (result == ItemResult.RESULT_OK)
            {
                // Take money and required items.
                plr.RemoveMoney(items[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in items[Num].ItemsReq)
                    plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));

                // The item is removed from the vendor list after purchase.
                items.Remove(items[Num]);

                // Set a cooldown so the player can't buy it again right away.
                // First, remove any old cooldowns for this item.
                var existingRewards = plr.Info.HonorCooldowns?.Where(x => x.ItemId == reward.ItemId);
                foreach (var existingReward in existingRewards)
                {
                    plr.Info.HonorCooldowns.Remove(existingReward);
                    CharMgr.Database.DeleteObject(existingReward);
                }

                // Add the new cooldown to the database.
                var honorRewardCooldown = new HonorRewardCooldown
                {
                    CharacterId = plr.CharacterId,
                    Cooldown = FrameWork.TCPManager.GetTimeStamp() + reward.Cooldown,
                    ItemId = reward.ItemId
                };

                plr.Info.HonorCooldowns.Add(honorRewardCooldown);
                CharMgr.Database.AddObject(honorRewardCooldown);
            }
            else if (result == ItemResult.RESULT_MAX_BAG)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_SPACE_TO_BUY);
            }
            else if (result == ItemResult.RESULT_ITEMID_INVALID)
            {
            }
        }

        // Handles buying special buffs from a Realm Captain vendor.
        public static void BuyItemRealmCaptainDynamicVendor(Player plr, InteractMenu Menu, List<Vendor_items> items)
        {
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            if (items.Count <= Num)
                return;

            // Check if the player is actually a Realm Captain.
            if (RealmCaptainManager.IsPlayerRealmCaptain(plr.CharacterId))
            {
                // If so, apply the buff they bought.
                RealmCaptainManager.ApplyRealmCaptainBuff(plr, items[Num].Info.SpellId);
            }
        }

        // This seems to be a placeholder or an event handler for when a buff is assigned.
        private static void BuffAssigned(NewBuff buff)
        {
            var newBuff = buff;
        }

        #endregion items

        #region Quests

        // This section handles quest logic, specifically how quest objectives are created and understood by the server.

        // This function takes a quest objective from the database and fills in the details,
        // like the name of the creature to kill or the NPC to talk to.
        // It makes the raw data from the database into something the game can use.
        public static void GenerateObjective(Quest_Objectives Obj, Quest Q)
        {
            // The logic depends on what type of objective it is.
            switch ((Objective_Type)Obj.ObjType)
            {
                // Objective: Kill a certain number of enemy players.
                case Objective_Type.QUEST_KILL_PLAYERS:
                    {
                        if (Obj.Description.Length < 1)
                            Obj.Description = "Enemy Players";
                    }
                    break;

                // Objective: Talk to a specific NPC.
                case Objective_Type.QUEST_SPEAK_TO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Creature = CreatureService.GetCreatureProto(ObjID); // Find the NPC's data.

                        if (Obj.Creature == null)
                        {
                            Obj.Description = "Invalid NPC - " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            // If no custom description is set, create a default one.
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Creature.Name.Length)
                                Obj.Description = "Speak to " + Obj.Creature.Name;
                        }
                    }
                    break;

                // Objective: Use a specific Game Object (like a lever or a chest).
                case Objective_Type.QUEST_USE_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID); // Find the object's data.

                        if (Obj.GameObject == null)
                        {
                            Obj.Description = "Invalid GameObject - QuestID " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.GameObject.Name.Length)
                                Obj.Description = "Find " + Obj.GameObject.Name;
                        }
                    }
                    break;

                // Objective: Kill a specific type of monster.
                case Objective_Type.QUEST_KILL_MOB:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Creature = CreatureService.GetCreatureProto(ObjID); // Find the monster's data.

                        if (Obj.Creature == null)
                        {
                            Obj.Description = "Invalid Creature - QuestID " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Creature.Name.Length)
                                Obj.Description = "Kill " + Obj.Creature.Name;
                        }
                    }
                    break;

                // Objective: Destroy a specific Game Object.
                case Objective_Type.QUEST_KILL_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID); // Find the object's data.

                        if (Obj.GameObject == null)
                        {
                            Obj.Description = "Invalid GameObject - QuestID " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.GameObject.Name.Length)
                                Obj.Description = "Destroy " + Obj.GameObject.Name;
                        }
                    }
                    break;

                // Objective: Use a quest item or collect a quest item.
                case Objective_Type.QUEST_USE_ITEM:
                case Objective_Type.QUEST_GET_ITEM:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
                            Obj.Item = ItemService.GetItem_Info(ObjID); // Find the item's data.
                            if (Obj.Item == null)
                            {
                                // This is a fallback for quests where the item data is missing.
                                // It tries to guess the item from the quest text. This is very specific and might be old code.
                                int a = Obj.Quest.Particular.IndexOf("kill the ", StringComparison.OrdinalIgnoreCase);
                                if (a >= 0)
                                {
                                    string[] RestWords = Obj.Quest.Particular.Substring(a + 9).Split(' ');
                                    string Name = RestWords[0] + " " + RestWords[1];
                                    Creature_proto Proto = CreatureService.GetCreatureProtoByName(Name) ?? CreatureService.GetCreatureProtoByName(RestWords[0]);
                                    if (Proto != null)
                                    {
                                        // If it finds a creature, it creates a temporary item on the fly.
                                        Obj.Item = new Item_Info();
                                        Obj.Item.Entry = ObjID;
                                        Obj.Item.Name = Obj.Description;
                                        Obj.Item.MaxStack = 20;
                                        Obj.Item.ModelId = 531;
                                        ItemService._Item_Info.Add(Obj.Item.Entry, Obj.Item);

                                        Log.Info("WorldMgr", "Creating Quest(" + Obj.Entry + ") Item : " + Obj.Item.Entry + ",  " + Obj.Item.Name + "| Adding Loot to : " + Proto.Name);
                                        /*Creature_loot loot = new Creature_loot();
                                        loot.Entry = Proto.Entry;
                                        loot.ItemId = Obj.Item.Entry;
                                        loot.Info = Obj.Item;
                                        loot.Pct = 0;
                                        GetCreatureSpecificLootFor(Proto.Entry).Add(loot);*/
                                    }
                                }
                            }
                        }
                    }
                    break;

                // Objective: Win a specific scenario (a small, instanced battle).
                case Objective_Type.QUEST_WIN_SCENARIO:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Scenario = ScenarioService.GetScenario_Info(ObjID); // Find the scenario's data.

                        if (Obj.Scenario == null)
                            Obj.Description = "Invalid Scenario - QuestID=" + Obj.Entry + ", ObjId=" + Obj.ObjID;
                        else
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Scenario.Name.Length)
                            Obj.Description = "Win " + Obj.Scenario.Name;
                    }
                    break;

                // Objective: Capture a Battlefield Objective in RvR.
                case Objective_Type.QUEST_CAPTURE_BO:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
                            // Search through all battlefield objectives to find the right one.
                            foreach (List<BattleFront_Objective> boList in BattleFrontService._BattleFrontObjectives.Values)
                            {
                                foreach (BattleFront_Objective bo in boList)
                                {
                                    if (bo.Entry == ObjID)
                                    {
                                        Obj.BattleFrontObjective = bo;
                                        break;
                                    }
                                }

                                if (Obj.BattleFrontObjective != null)
                                    break;
                            }
                        }

                        if (Obj.BattleFrontObjective == null)
                            Obj.Description = "Invalid Battlefield Objective - QuestID=" + Obj.Entry + ", ObjId=" + Obj.ObjID;
                        else
                            if (Obj.Description == null || Obj.Description.Length <= Obj.BattleFrontObjective.Name.Length)
                            Obj.Description = "Capture " + Obj.Scenario.Name;
                    }
                    break;

                // Objective: Capture a Keep in RvR.
                case Objective_Type.QUEST_CAPTURE_KEEP:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
                            // Search through all keeps to find the right one.
                            foreach (List<Keep_Info> keepList in BattleFrontService._KeepInfos.Values)
                            {
                                foreach (Keep_Info keep in keepList)
                                {
                                    if (keep.KeepId == ObjID)
                                    {
                                        Obj.Keep = keep;
                                        break;
                                    }
                                }

                                if (Obj.Keep != null)
                                    break;
                            }
                        }

                        if (Obj.Keep == null)
                            Obj.Description = "Invalid Keep - QuestID=" + Obj.Entry + ", ObjId=" + Obj.ObjID;
                        else
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Keep.Name.Length)
                            Obj.Description = "Capture " + Obj.Keep.Name;
                    }
                    break;
            }
        }

        #endregion Quests

        #region Relation

        // This "Relation" section is all about connecting different pieces of game data together when the server starts.
        // It's like building a web of information, so a quest knows about the monster it needs you to kill,
        // and an item knows which race can use it. This is a critical part of the server's startup process.

        [LoadingFunction(false)]
        public static void LoadRelation()
        {
            Log.Success("LoadRelation", "Loading Relations");

            // Go through every item in the game.
            foreach (Item_Info info in ItemService._Item_Info.Values)
            {
                // If an item is restricted to a specific career (class)...
                if (info.Career != 0)
                {
                    // ...figure out which realm (Order or Destruction) that career belongs to.
                    foreach (KeyValuePair<byte, CharacterInfo> Kp in CharMgr.CharacterInfos)
                    {
                        if ((info.Career & (1 << (Kp.Value.CareerLine - 1))) == 0)
                            continue;

                        info.Realm = Kp.Value.Realm;
                        break;
                    }
                }
                // If an item is restricted to a specific race...
                else if (info.Race > 0)
                {
                    // ...figure out if it's an Order race or a Destruction race.
                    if (((Constants.RaceMaskDwarf + Constants.RaceMaskHighElf + Constants.RaceMaskEmpire) & info.Race) > 0)
                        info.Realm = 1; // Order
                    else info.Realm = 2; // Destruction
                }
            }

            // Load and connect all the different data types.
            LoadChapters();
            LoadPublicQuests();
            LoadQuestsRelation();
            LoadScripts(false);

            // Connect Public Quests to their corresponding Keeps.
            foreach (List<Keep_Info> keepInfos in BattleFrontService._KeepInfos.Values)
                foreach (Keep_Info keepInfo in keepInfos)
                    if (PQuestService._PQuests.ContainsKey(keepInfo.PQuestId))
                        keepInfo.PQuest = PQuestService._PQuests[keepInfo.PQuestId];

            // Make sure all characters are marked as offline in the database when the server starts.
            CharMgr.Database.ExecuteNonQuery($"UPDATE `{CharMgr.Database.GetSchemaName()}`.characters_value SET Online = 0");

            // Pre-load the game regions for the different tiers of play to speed things up later.
            Log.Info("Regions", "Preloading pairing regions...");
            // Tier 1
            GetRegion(1, true, Constants.RegionName[1]); // dw/gs
            GetRegion(3, true, Constants.RegionName[3]); // he/de
            GetRegion(8, true, Constants.RegionName[8]); // em/ch

            // Tier 2
            GetRegion(12, true, Constants.RegionName[12]); // dw/gs
            GetRegion(15, true, Constants.RegionName[15]); // he/de
            GetRegion(14, true, Constants.RegionName[14]); // em/ch

            // Tier 3
            GetRegion(10, true, Constants.RegionName[10]); // dw/gs
            GetRegion(16, true, Constants.RegionName[16]); // he/de
            GetRegion(6, true, Constants.RegionName[6]); // em/ch

            // Tier 4
            GetRegion(2, true, Constants.RegionName[2]); // dw/gs
            GetRegion(4, true, Constants.RegionName[4]);  // he/de
            GetRegion(11, true, Constants.RegionName[11]); // em/ch

            // removed for now, as this will also trigger an attempt to load BOs for the region.
            //GetRegion(9, true, Constants.RegionName[9]); // lotd
            Log.Success("Regions", "Preloaded pairing regions.");
        }

        // Loads all the chapter data (story progression within a zone) and connects it to zones and rewards.
        public static void LoadChapters()
        {
            Log.Success("LoadChapters", "Loading Zone from Chapters");

            long InvalidChapters = 0;

            Zone_Info Zone = null;
            Chapter_Info Info;
            foreach (KeyValuePair<uint, Chapter_Info> Kp in ChapterService._Chapters)
            {
                Info = Kp.Value;
                Zone = ZoneService.GetZone_Info(Info.ZoneId);

                // Check for bad data.
                if (Zone == null || (Info.PinX <= 0 && Info.PinY <= 0))
                {
                    _logger.Warn("LoadChapters Chapter (" + Info.Entry + ")[" + Info.Name + "] Invalid");
                    ++InvalidChapters;
                }

                // Make sure the reward lists exist.
                if (Info.T1Rewards == null)
                    Info.T1Rewards = new List<Chapter_Reward>();
                if (Info.T2Rewards == null)
                    Info.T2Rewards = new List<Chapter_Reward>();
                if (Info.T3Rewards == null)
                    Info.T3Rewards = new List<Chapter_Reward>();

                // Get the rewards for this chapter from the database.
                List<Chapter_Reward> Rewards;
                if (ChapterService._Chapters_Reward.TryGetValue(Info.Entry, out Rewards))
                {
                    // Assign rewards to the correct influence tier.
                    foreach (Chapter_Reward CW in Rewards)
                    {
                        if (Info.Tier1InfluenceCount == CW.InfluenceCount)
                        {
                            Info.T1Rewards.Add(CW);
                        }
                        else if (Info.Tier2InfluenceCount == CW.InfluenceCount)
                        {
                            Info.T2Rewards.Add(CW);
                        }
                        else if (Info.Tier3InfluenceCount == CW.InfluenceCount)
                        {
                            Info.T3Rewards.Add(CW);
                        }
                    }
                }

                // Connect each reward to its item data.
                foreach (Chapter_Reward Reward in Info.T1Rewards.ToArray())
                {
                    Reward.Item = ItemService.GetItem_Info(Reward.ItemId);
                    Reward.Chapter = Info;

                    if (Reward.Item == null)
                        Info.T1Rewards.Remove(Reward);
                }

                foreach (Chapter_Reward Reward in Info.T2Rewards.ToArray())
                {
                    Reward.Item = ItemService.GetItem_Info(Reward.ItemId);
                    Reward.Chapter = Info;

                    if (Reward.Item == null)
                        Info.T2Rewards.Remove(Reward);
                }
                foreach (Chapter_Reward Reward in Info.T3Rewards.ToArray())
                {
                    Reward.Item = ItemService.GetItem_Info(Reward.ItemId);
                    Reward.Chapter = Info;

                    if (Reward.Item == null)
                        Info.T3Rewards.Remove(Reward);
                }

                // Add the chapter to the correct cell in the world map for efficient lookups.
                CellSpawnService.GetRegionCell(Zone.Region, (ushort)((float)(Info.PinX / 4096) + Zone.OffX), (ushort)((float)(Info.PinY / 4096) + Zone.OffY)).AddChapter(Info);
            }

            if (InvalidChapters > 0)
                _logger.Warn("LoadChapters", "[" + InvalidChapters + "] Invalid Chapter(s)");
        }

        // Loads all the Public Quest data and connects it to zones and objectives.
        public static void LoadPublicQuests()
        {
            Zone_Info Zone = null;
            PQuest_Info Info;
            List<string> skippedPQs = new List<string>();

            foreach (KeyValuePair<uint, PQuest_Info> Kp in PQuestService._PQuests)
            {
                Info = Kp.Value;
                Zone = ZoneService.GetZone_Info(Info.ZoneId);
                if (Zone == null)
                    continue;

                // Get the objectives for this PQ.
                if (!PQuestService._PQuest_Objectives.TryGetValue(Info.Entry, out Info.Objectives))
                    Info.Objectives = new List<PQuest_Objective>();
                else
                {
                    foreach (PQuest_Objective Obj in Info.Objectives)
                    {
                        Obj.Quest = Info;
                        PQuestService.GeneratePQuestObjective(Obj, Obj.Quest); // Process the objective data.

                        // Get the creature/object spawns for this objective stage.
                        if (!PQuestService._PQuest_Spawns.TryGetValue(Obj.Guid, out Obj.Spawns))
                            Obj.Spawns = new List<PQuest_Spawn>();
                    }
                }

                Log.Info("LoadPublicQuests", "Loaded public quest " + Info.Entry + " to region " + Zone.Region + " cell at X: " + ((float)(Info.PinX / 4096) + Zone.OffX) + " " + (float)(Info.PinY / 4096) + Zone.OffY);

                bool skipLoad = false;

                // Don't load PQs that are part of a Keep, because the Keep will manage them.
                foreach (List<Keep_Info> keepInfos in BattleFrontService._KeepInfos.Values)
                {
                    if (keepInfos.Any(keep => keep.PQuestId == Kp.Key))
                    {
                        skippedPQs.Add(Kp.Value.Name);
                        skipLoad = true;
                        break;
                    }
                }

                if (!skipLoad)
                    CellSpawnService.GetRegionCell(Zone.Region, (ushort)((float)(Info.PinX / 4096) + Zone.OffX), (ushort)((float)(Info.PinY / 4096) + Zone.OffY)).AddPQuest(Info);
            }

            if (skippedPQs.Count > 0)
                Log.Info("Skipped PQs", string.Join(", ", skippedPQs));
        }

        // Loads all the regular Quest data and connects it to creatures, objectives, and rewards.
        public static void LoadQuestsRelation()
        {
            // Find out which creatures start and end which quests.
            QuestService.LoadQuestCreatureStarter();
            QuestService.LoadQuestCreatureFinisher();

            foreach (KeyValuePair<uint, Creature_proto> Kp in CreatureService.CreatureProtos)
            {
                Kp.Value.StartingQuests = QuestService.GetStartQuests(Kp.Key);
                Kp.Value.FinishingQuests = QuestService.GetFinishersQuests(Kp.Key);
            }

            Quest quest;

            // Connect objectives to their parent quests.
            int MaxGuid = 0;
            foreach (KeyValuePair<int, Quest_Objectives> Kp in QuestService._Objectives)
            {
                if (Kp.Value.Guid >= MaxGuid)
                    MaxGuid = Kp.Value.Guid;
            }

            foreach (KeyValuePair<int, Quest_Objectives> Kp in QuestService._Objectives)
            {
                quest = Kp.Value.Quest = QuestService.GetQuest(Kp.Value.Entry);
                if (quest == null)
                    continue;

                quest.Objectives.Add(Kp.Value);
            }

            // Connect map coordinates to their quests.
            foreach (Quest_Map Q in QuestService._QuestMaps)
            {
                quest = QuestService.GetQuest(Q.Entry);
                if (quest == null)
                    continue;

                quest.Maps.Add(Q);
            }

            // For quests that are missing objectives, try to create a default "speak to finisher" objective.
            foreach (KeyValuePair<ushort, Quest> Kp in QuestService._Quests)
            {
                quest = Kp.Value;

                if (quest.Objectives.Count == 0)
                {
                    uint Finisher = QuestService.GetQuestCreatureFinisher(quest.Entry);
                    if (Finisher != 0)
                    {
                        Quest_Objectives NewObj = new Quest_Objectives();
                        NewObj.Guid = ++MaxGuid;
                        NewObj.Entry = quest.Entry;
                        NewObj.ObjType = (uint)Objective_Type.QUEST_SPEAK_TO;
                        NewObj.ObjID = Finisher.ToString();
                        NewObj.ObjCount = 1;
                        NewObj.Quest = quest;

                        quest.Objectives.Add(NewObj);
                        QuestService._Objectives.Add(NewObj.Guid, NewObj);

                        Log.Debug("WorldMgr", "Creating Objective for quest with no objectives: " + Kp.Value.Entry + " " + Kp.Value.Name);
                    }
                }
            }

            // Now that all objectives are connected, generate the detailed objective info.
            foreach (KeyValuePair<int, Quest_Objectives> Kp in QuestService._Objectives)
            {
                if (Kp.Value.Quest == null)
                    continue;
                GenerateObjective(Kp.Value, Kp.Value.Quest);
            }

            // Parse the quest reward strings and connect them to the actual item data.
            string sItemID, sCount;
            uint ItemID, Count;
            Item_Info Info;
            foreach (KeyValuePair<ushort, Quest> Kp in QuestService._Quests)
            {
                if (Kp.Value.Choice.Length <= 0)
                    continue;

                // Example format: [5154,12],[128,1]
                string[] Rewards = Kp.Value.Choice.Split('[');
                foreach (string Reward in Rewards)
                {
                    if (Reward.Length <= 0)
                        continue;

                    sItemID = Reward.Substring(0, Reward.IndexOf(','));
                    sCount = Reward.Substring(sItemID.Length + 1, Reward.IndexOf(']') - sItemID.Length - 1);

                    ItemID = uint.Parse(sItemID);
                    Count = uint.Parse(sCount);

                    Info = ItemService.GetItem_Info(ItemID);
                    if (Info == null)
                        continue;

                    if (!Kp.Value.Rewards.ContainsKey(Info))
                        Kp.Value.Rewards.Add(Info, Count);
                    else
                        Kp.Value.Rewards[Info] += Count;
                }
            }
        }

        #endregion Relation

        #region Scripts
        
        // This section manages all the custom scripts that can change game behavior.
        // Scripts can be attached to creatures, game objects, or run globally to add special logic.

        // These dictionaries store the different types of scripts.
        public static Dictionary<string, Type> LocalScripts = new Dictionary<string, Type>(); // Scripts that run on a specific object when triggered.
        public static Dictionary<string, AGeneralScript> GlobalScripts = new Dictionary<string, AGeneralScript>(); // Scripts that are always running.
        public static Dictionary<uint, Type> CreatureScripts = new Dictionary<uint, Type>(); // Scripts attached to a specific creature ID.
        public static Dictionary<uint, Type> GameObjectScripts = new Dictionary<uint, Type>(); // Scripts attached to a specific game object ID.
        public static ScriptsInterface GeneralScripts; // A helper to manage the scripts.

        // This function scans the code for all the script files and registers them so the game can use them.
        public static void LoadScripts(bool Reload)
        {
            GeneralScripts = new ScriptsInterface();
                     
            GeneralScripts.ClearScripts();

            // Look through all the code files in the project.
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass != true)
                        continue;

                    // Find classes that are scripts.
                    if (!type.IsSubclassOf(typeof(AGeneralScript)))
                        continue;

                    // Read the script's attributes to know what it does.
                    foreach (GeneralScriptAttribute at in type.GetCustomAttributes(typeof(GeneralScriptAttribute), true))
                    {
                        if (!string.IsNullOrEmpty(at.ScriptName))
                            at.ScriptName = at.ScriptName.ToLower();

                        Log.Success("Scripting", "Registering Script :" + at.ScriptName);

                        // If it's a global script, create an instance of it and add it to the list.
                        if (at.GlobalScript)
                        {
                            AGeneralScript Script = Activator.CreateInstance(type) as AGeneralScript;
                            Script.ScriptName = at.ScriptName;
                            GeneralScripts.RemoveScript(Script.ScriptName);
                            GeneralScripts.AddScript(Script);
                            GlobalScripts[at.ScriptName] = Script;
                        }
                        else
                        {
                            // If it's attached to a specific creature, register it.
                            if (at.CreatureEntry != 0)
                            {
                                Log.Success("Scripts", "Registering Creature Script :" + at.CreatureEntry);

                                if (!CreatureScripts.ContainsKey(at.CreatureEntry))
                                {
                                    CreatureScripts[at.CreatureEntry] = type;
                                }
                                else
                                {
                                    CreatureScripts[at.CreatureEntry] = type;
                                }
                            }
                            // If it's attached to a specific game object, register it.
                            else if (at.GameObjectEntry != 0)
                            {
                                Log.Success("Scripts", "Registering GameObject Script :" + at.GameObjectEntry);

                                if (!GameObjectScripts.ContainsKey(at.GameObjectEntry))
                                {
                                    GameObjectScripts[at.GameObjectEntry] = type;
                                }
                                else
                                {
                                    GameObjectScripts[at.GameObjectEntry] = type;
                                }
                            }
                            // If it's a local script called by name, register it.
                            else if (!string.IsNullOrEmpty(at.ScriptName))
                            {
                                Log.Success("Scripts", "Registering Name Script :" + at.ScriptName);

                                if (!LocalScripts.ContainsKey(at.ScriptName))
                                {
                                    LocalScripts[at.ScriptName] = type;
                                }
                                else
                                {
                                    LocalScripts[at.ScriptName] = type;
                                }
                            }
                        }
                    }
                }
            }

            Log.Success("Scripting", "Loaded  : " + (GeneralScripts.Scripts.Count + LocalScripts.Count) + " Scripts");

            // If we are reloading scripts while the server is running, we also need to reload the network packet handlers.
            if (Reload)
            {
                if (Core.Server != null)
                    Core.Server.LoadPacketHandler();
            }
        }

        // Gets a script for a specific game object.
        public static AGeneralScript GetScript(Object Obj, string ScriptName)
        {
            // If a script name is provided, try to find it.
            if (!string.IsNullOrEmpty(ScriptName))
            {
                ScriptName = ScriptName.ToLower();

                if (GlobalScripts.ContainsKey(ScriptName))
                    return GlobalScripts[ScriptName];
                if (LocalScripts.ContainsKey(ScriptName))
                {
                    AGeneralScript Script = Activator.CreateInstance(LocalScripts[ScriptName]) as AGeneralScript;
                    Script.ScriptName = ScriptName;
                    return Script;
                }
            }
            // If no name is provided, check if the object has a script attached by its ID.
            else
            {
                if (Obj.IsCreature() && CreatureScripts.ContainsKey(Obj.GetCreature().Spawn.Entry))
                {
                    AGeneralScript Script = Activator.CreateInstance(CreatureScripts[Obj.GetCreature().Spawn.Entry]) as AGeneralScript;
                    Script.ScriptName = Obj.GetCreature().Spawn.Entry.ToString();
                    return Script;
                }

                if (Obj.IsGameObject() && GameObjectScripts.ContainsKey(Obj.GetGameObject().Spawn.Entry))
                {
                    AGeneralScript Script = Activator.CreateInstance(GameObjectScripts[Obj.GetGameObject().Spawn.Entry]) as AGeneralScript;
                    Script.ScriptName = Obj.GetGameObject().Spawn.Entry.ToString();
                    return Script;
                }
            }

            return null;
        }

        // This is called on every server tick to update any scripts that need continuous processing.
        public static void UpdateScripts(long Tick)
        {
            GeneralScripts.Update(Tick);
        }

        #endregion Scripts

        #region Scenarios

        // This section handles Scenarios (small, instanced PvP battles) and Instances (dungeons).

        public static ScenarioMgr ScenarioMgr; // Manages all the active scenarios.

        public static InstanceMgr InstanceMgr; // Manages all the active instances/dungeons.

        // This starts up the Scenario Manager when the server boots.
        [LoadingFunction(true)]
        public static void StartScenarioMgr()
        {
            ScenarioMgr = new ScenarioMgr(ScenarioService.ActiveScenarios);
        }

        // This starts up the Instance Manager when the server boots.
        [LoadingFunction(true)]
        public static void StartInstanceMgr()
        {
            InstanceMgr = new InstanceMgr();
        }

        #endregion Scenarios

        #region Settings

        // This section handles loading any special settings that affect the entire world.

        public static WorldSettingsMgr WorldSettingsMgr; // Manages world-wide settings.

        // This starts up the World Settings Manager when the server boots.
        [LoadingFunction(true)]
        public static void StartWorldSettingsMgr()
        {
            WorldSettingsMgr = new WorldSettingsMgr();
        }

        #endregion Settings

        #region Campaign

        // This section handles the RvR (Realm vs. Realm) campaign, which is the large-scale war.
        // It tracks which zones are being fought over and updates players and groups.

        // Starts a background thread that periodically updates the world state, like zone control.
        public static void WorldUpdateStart()
        {
            Log.Debug("WorldMgr", "Starting World Monitor...");

            _worldThread = new Thread(WorldUpdate);
            _worldThread.Start();
        }

        // Starts a background thread that periodically updates player groups.
        public static void GroupUpdateStart()
        {
            Log.Debug("WorldMgr", "Starting Group Updater...");

            _groupThread = new Thread(GroupUpdate);
            _groupThread.Start();
        }

        // Gets a dictionary of zones and how intense the fighting is in each one.
        public static Dictionary<int, int> GetZonesFightLevel()
        {
            var level = new Dictionary<int, int>();
            foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
            {
                foreach (var zone in region.ZonesMgr.ToList())
                {
                    var hotspots = zone.GetHotSpots();
                    if (hotspots.Count > 0)
                        level[zone.ZoneId] = hotspots.Where(e => e.Item2 >= ZoneMgr.LOW_FIGHT).Max(e => e.Item2);
                }
            }
            return level;
        }

        /// <summary>
        /// Show swords on world map if zone has people fighting in it.
        /// This sends a packet to players to update the crossed-swords icon on their world map.
        /// </summary>
        public static void SendZoneFightLevel(Player player = null)
        {
            var fightLevel = GetZonesFightLevel();

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_HOT_SPOT);
            Out.WriteByte((byte)fightLevel.Count);
            Out.WriteByte(2); //world hotspots
            Out.WriteByte(0);

            // These values determine the size of the swords icon on the map.
            uint none = 0x00000000;
            uint small = 0x01000000;
            uint large = 0x01020000;
            uint huge = 0x01020100;

            foreach (var zoneId in fightLevel.Keys)
            {
                Out.WriteByte((byte)zoneId);

                if (fightLevel[zoneId] >= ZoneMgr.LARGE_FIGHT)
                    Out.WriteUInt32(huge);
                else if (fightLevel[zoneId] > ZoneMgr.MEDIUM_FIGHT)
                    Out.WriteUInt32(large);
                else if (fightLevel[zoneId] > ZoneMgr.LOW_FIGHT)
                    Out.WriteUInt32(small);
                else
                    Out.WriteUInt32(none);
            }

            // If a specific player is given, only send it to them.
            if (player != null)
                player.SendPacket(Out);
            // Otherwise, send it to all players in the world.
            else
            {
                lock (Player._Players)
                {
                    foreach (Player pPlr in Player._Players)
                    {
                        if (pPlr == null || pPlr.IsDisposed || !pPlr.IsInWorld())
                            continue;

                        pPlr.SendCopy(Out);
                    }
                }
            }

            // Also update the local "hotspots" within each zone.
            foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
            {
                foreach (var zone in region.ZonesMgr.ToList())
                {
                    zone.SendHotSpots(player);
                }
            }
        }

        // The main loop for the world update thread. Runs continuously.
        private static void WorldUpdate()
        {
            while (_running)
            {
                if (ZoneService._Zone_Info != null)
                {
                    // Every 15 seconds, update the fight levels on the map.
                    SendZoneFightLevel();

                    // And decay the "hotspot" values so they don't stay high forever.
                    foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
                    {
                        foreach (var zone in region.ZonesMgr.ToList())
                        {
                            zone.DecayHotspots();
                        }
                    }
                }
                Thread.Sleep(15000); // Wait 15 seconds.
            }
        }

        // The main loop for the group update thread. Runs very frequently.
        private static void GroupUpdate()
        {
            while (_running)
            {
                // Get a list of all groups in the world.
                List<Group> _groups = new List<Group>();
                lock (Group.WorldGroups)
                {
                    foreach (Group g in Group.WorldGroups)
                    {
                        try
                        {
                            _groups.Add(g);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                // Get a list of pending actions for groups (like inviting someone).
                List<KeyValuePair<uint, GroupAction>> _worldActions = new List<KeyValuePair<uint, GroupAction>>();
                lock (Group._pendingGroupActions)
                {
                    foreach (KeyValuePair<uint, GroupAction> kp in Group._pendingGroupActions)
                    {
                        _worldActions.Add(kp);
                    }
                    Group._pendingGroupActions.Clear();
                }

                // Process all pending actions and update each group.
                foreach (Group g in _groups)
                {
                    try
                    {
                        foreach (KeyValuePair<uint, GroupAction> grpAction in _worldActions)
                        {
                            if (g.GroupId == grpAction.Key)
                                g.EnqueuePendingGroupAction(grpAction.Value);
                        }

                        g.Update(TCPManager.GetTimeStampMS());
                    }
                    catch (Exception e)
                    {
                        Log.Error("Caught exception", "Exception thrown: " + e);
                        continue;
                    }
                }

                _worldActions.Clear();
                _groups.Clear();

                Thread.Sleep(100); // Wait 100 milliseconds.
            }
        }

        #endregion Campaign

        #region Keep registry, to remove it's static bullshit

        // This section manages all the Keeps in the world. Keeps are major fortresses in RvR.
        // The comment "to remove it's static bullshit" suggests a developer intended to refactor this
        // to be more flexible and less reliant on global static access, which can cause problems.

        // This holds a list of all active keeps in the world.
        public static Dictionary<uint, BattleFrontKeep> _Keeps = new Dictionary<uint, BattleFrontKeep>();

        // Sends the status of all keeps to a specific player when they log in or enter a new zone.
        public static void SendKeepStatus(Player Plr)
        {
            foreach (List<Keep_Info> list in BattleFrontService._KeepInfos.Values)
            {
                foreach (Keep_Info KeepInfo in list)
                {
                    // If the keep is active and loaded in the world...
                    if (_Keeps.ContainsKey(KeepInfo.KeepId))
                    {
                        // ...send its current, live status.
                        _Keeps[KeepInfo.KeepId].KeepCommunications.SendKeepStatus(Plr, _Keeps[KeepInfo.KeepId]);
                    }
                    else
                    {
                        // If the keep isn't active (e.g., in a zone that's not loaded), send a default status.
                        PacketOut Out = new PacketOut((byte)Opcodes.F_KEEP_STATUS, 26);
                        Out.WriteByte(KeepInfo.KeepId);
                        Out.WriteByte(1); // status
                        Out.WriteByte(0); // ?
                        Out.WriteByte(KeepInfo.Realm);
                        Out.WriteByte(KeepInfo.DoorCount);
                        Out.WriteByte(0); // Rank
                        Out.WriteByte(100); // Door health
                        Out.WriteByte(0); // Next rank %
                        Out.Fill(0, 18);
                        Plr.SendPacket(Out);
                    }
                }
            }
        }

        #endregion Keep registry, to remove it's static bullshit

        #region Logging

        // This section contains utility functions related to server logging.

        // This is a safety measure that runs when the server starts.
        // It turns off detailed packet logging for all accounts.
        // This is to prevent developers or GMs from accidentally leaving it on,
        // which can create huge log files (20GB+).
        [LoadingFunction(true)]
        public static void ResetPacketLogSettings()
        {
            //turn off user specific packet logging when server restarts. This is because devs/gm forget to turn it off and log file grows > 20GB
            Log.Debug("WorldMgr", "Resetting user packet log settings...");
            Database.ExecuteNonQuery($"update `{Core.AccountConfig.AccountDB.Database}`.accounts set PacketLog = 0");
        }

        #endregion Logging

        // Attaches the correct campaign manager (e.g., for Tier 1 or Tier 4) to each game region.
        public static void AttachCampaignsToRegions()
        {
            foreach (var regionMgr in _Regions)
            {
                var objectiveList = LoadObjectives(regionMgr);
                switch (regionMgr.RegionId)
                {
                    // Tier 1 pairings are attached to the Lower Tier Campaign Manager.
                    case 1: // t1 dw/gs
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new ApocCommunications());
                        break;

                    case 3: // t1 he/de
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new ApocCommunications());
                        break;

                    case 8: // t1 em/ch
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new ApocCommunications());
                        break;
                    // Tier 4 pairings are attached to the Upper Tier Campaign Manager.
                    case 11:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new ApocCommunications());
                        break;

                    case 2:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new ApocCommunications());
                        break;

                    case 4:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new ApocCommunications());
                        break;

                    default: // Other regions don't have a campaign attached.
                        break;
                }
            }
        }

        // Loads the Battlefield Objectives for a specific region from the database.
        public static List<BattlefieldObjective> LoadObjectives(RegionMgr regionMgr)
        {
            List<BattleFront_Objective> objectives = BattleFrontService.GetBattleFrontObjectives(regionMgr.RegionId);
            if (objectives == null)
            {
                _logger.Warn($"Region = {regionMgr.RegionId} has no objectives");
                return null;
            }
            var resultList = new List<BattlefieldObjective>();
            _logger.Debug($"Region = {regionMgr.RegionId} ObjectiveCount = {objectives.Count}");
            // Create a new BattlefieldObjective object for each one found in the database.
            foreach (BattleFront_Objective obj in objectives.Where(x => x.KeepSpawn == false))
            {
                BattlefieldObjective flag = new BattlefieldObjective(regionMgr, obj);
                resultList.Add(flag);
            }

            return resultList;
        }

        /// <summary>
        /// Inform the server of the change in the RVR Progression across all regions.
        /// This sends a big packet to all players to update the campaign status UI.
        /// </summary>
        public static void UpdateRegionCaptureStatus(LowerTierCampaignManager lowerTierCampaignManager, UpperTierCampaignManager upperTierCampaignManager)
        {
            if ((lowerTierCampaignManager == null) || (upperTierCampaignManager == null))
                return;
            _logger.Trace("F_CAMPAIGN_STATUS1");
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAMPAIGN_STATUS, 159);
            Out.WriteHexStringBytes("0005006700CB00"); // Packet header

            // This part of the packet contains the lock percentages for Tier 1 zones.
            // Dwarfs vs Greenskins T1
            Out.WriteByte(0);    // 0 and ignored
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND).DestructionVictoryPointPercentage);    // % Dest lock
            // Dwarfs vs Greenskins T2 (Not implemented)
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(12, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Dwarfs vs Greenskins T3 (Not implemented)
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(10, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Dwarfs vs Greenskins T4 (Handled later)
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(2, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Empire vs Chaos T1
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(8, false), realm);
            Out.WriteByte(0);
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND).DestructionVictoryPointPercentage);    // % Dest lock
            // Empire vs Chaos T2 (Not implemented)
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(14, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Empire vs Chaos T3 (Not implemented)
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(6, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(45);  // % Order lock
            Out.WriteByte(55);    // % Dest lock
            // Empire vs Chaos T4 (Handled later)
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(11, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(40);  // % Order lock
            Out.WriteByte(60);    // % Dest lock
            // High Elves vs Dark Elves T1
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(3, false), realm);
            Out.WriteByte(0);
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE).DestructionVictoryPointPercentage);    // % Dest lock
            // High Elves vs Dark Elves T2 (Not implemented)
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(15, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // High Elves vs Dark Elves T3 (Not implemented)
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(16, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // High Elves vs Dark Elves T4 (Handled later)
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(4, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock

            Out.Fill(0, 83); // Padding

            // This part of the packet contains the lock status (e.g., Order controlled, Destruction controlled) for Tier 4 zones.
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_STONEWATCH).LockStatus);  //  Dwarf Fort
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_KADRIN_VALLEY).LockStatus);  // (ZONE_STATUS_ORDER_LOCKED/ZONE_STATUS_DESTRO_LOCKED)
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_THUNDER_MOUNTAIN).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BLACK_CRAG).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BUTCHERS_PASS).LockStatus);   // greenskin Fort

            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKWALD).LockStatus);// Empire Fort
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKLAND).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_PRAAG).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_CHAOS_WASTES).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_THE_MAW).LockStatus);  // Chaos Fort

            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_SHINING_WAY).LockStatus);   //elf fortress
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_EATAINE).LockStatus);  // (ZONE_STATUS_ORDER_LOCKED/ZONE_STATUS_DESTRO_LOCKED)
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_DRAGONWAKE).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_CALEDOR).LockStatus);
            Out.WriteByte((byte)upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_FELL_LANDING).LockStatus);   //Dark elf Fortress

            Out.WriteByte(0); // Order underdog rating
            Out.WriteByte(0); // Destruction underdog rating

            /*Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);

            Out.WriteByte(00);

            Out.Fill(0, 4);*/

            //For debugging purposes
            var lockStr = upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_BLACK_CRAG).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_THUNDER_MOUNTAIN).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER4_KADRIN_VALLEY).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_CHAOS_WASTES).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_PRAAG).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKLAND).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_CALEDOR).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_DRAGONWAKE).LockStatus.ToString();
            lockStr += upperTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER4_EATAINE).LockStatus.ToString();

            byte[] buffer = Out.ToArray();
            _logger.Trace("WorldMgr : " + lockStr);

            // Send this massive packet to every player online.
            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player == null || player.IsDisposed || !player.IsInWorld())
                        continue;

                    player.SendPacket(Out);

                    // A second packet is sent with some player-specific campaign status, but it seems mostly empty here.
                    PacketOut playerCampaignStatus = new PacketOut(0, 159) { Position = 0 };
                    playerCampaignStatus.Write(buffer, 0, buffer.Length);

                    if (player.Region?.Campaign != null)
                    {
                        Out.WriteByte((byte)75);
                        Out.WriteByte((byte)25);

                        //Out.WriteByte((byte) player.Region?.Campaign.VictoryPointProgress.OrderVictoryPointPercentage);
                        //Out.WriteByte((byte) player.Region?.Campaign.VictoryPointProgress.DestructionVictoryPointPercentage);
                    }
                    else
                    {
                        playerCampaignStatus.Fill(0, 9);
                    }
                    playerCampaignStatus.Fill(0, 4);

                    player.SendPacket(playerCampaignStatus);
                }
            }
        }

        // Handles buying items from the Black Market vendor, which likely has special rules.
        public static void BuyItemBlackMarketVendor(Player plr, InteractMenu menu, List<Vendor_items> items)
        {
            int Num = (menu.Page * VendorService.MAX_ITEM_PAGE) + menu.Num;
            ushort Count = menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            if (items.Count <= Num)
                return;

            ItemResult result = plr.ItmInterface.CreateItem(items[Num].Info, (ushort)1);
            if (result == ItemResult.RESULT_OK)
            {
                // Take the player's money and required items.
                plr.RemoveMoney(items[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in items[Num].ItemsReq)
                    plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));

                // Remove the item from the vendor list after purchase.
                items.Remove(items[Num]);
            }
            else if (result == ItemResult.RESULT_MAX_BAG)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_SPACE_TO_BUY);
            }
            else if (result == ItemResult.RESULT_ITEMID_INVALID)
            {
            }
        }
    }
}