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
using WorldServer.World.Battlefronts.Apocalypse.Loot;
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
    [Service(
        typeof(AnnounceService),
        typeof(BattleFrontService),
        typeof(BountyService),
        typeof(CellSpawnService),
        typeof(ChapterService),
        typeof(CreatureService),
        typeof(DyeService),
        typeof(GameObjectService),
        typeof(GuildService),
        typeof(ItemService),
        typeof(PQuestService),
        typeof(QuestService),
        typeof(RallyPointService),
        typeof(RVRProgressionService),
        typeof(RewardService),
        typeof(ScenarioService),
        typeof(TokService),
        typeof(VendorService),
        typeof(WaypointService),
        typeof(XpRenownService),
        typeof(ZoneService))]
    public static class WorldMgr
    {
        public static IObjectDatabase Database;
        private static Thread _worldThread;
        private static Thread _groupThread;
        private static bool _running = true;
        public static long StartingPairing;
        // DEV - Development mode, PRD - Production Mode. 
        public static string ServerMode;

        public static UpperTierCampaignManager UpperTierCampaignManager;
        public static LowerTierCampaignManager LowerTierCampaignManager;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static RVRArea RVRArea = new RVRArea();


        //Log.Success("StartingPairing: ", StartingPairing.ToString());

        #region Region

        public static List<RegionMgr> _Regions = new List<RegionMgr>();
        private static ReaderWriterLockSlim RegionsRWLock = new ReaderWriterLockSlim();


        public static RegionMgr GetRegion(ushort RegionId, bool Create, string name = "")
        {
            RegionsRWLock.EnterReadLock();
            RegionMgr Mgr = _Regions.Find(region => region != null && region.RegionId == RegionId);
            RegionsRWLock.ExitReadLock();

            if (Mgr == null && Create)
            {
                Mgr = new RegionMgr(RegionId, ZoneService.GetZoneRegion(RegionId), name, new ApocCommunications());
                RegionsRWLock.EnterWriteLock();
                _Regions.Add(Mgr);
                RegionsRWLock.ExitWriteLock();
            }

            return Mgr;
        }

        public static void Stop()
        {
            Log.Success("WorldMgr", "Stop");
            foreach (RegionMgr Mgr in _Regions)
                Mgr.Stop();

            ScenarioMgr.Stop();
            _running = false;

        }


        #endregion

        #region Zones

        public static SpawnPoint GetZoneRespawn(ushort zoneId, byte realm, Player player)
        {
            if (player == null)
            {
                return new SpawnPoint(ZoneService.GetZoneRespawn(zoneId, realm));
            }

            if (player.CurrentArea != null)
            {
                // If player is in a Public Quest
                if (player.QtsInterface.PublicQuest != null)
                {
                    var pqRespawns = ZoneService.GetZoneRespawns(zoneId);
                    foreach (var res in pqRespawns)
                        if (res.Realm == 0 &&
                            res.ZoneID == zoneId &&
                            res.RespawnID == player.QtsInterface.PublicQuest.Info.RespawnID)
                            return new SpawnPoint(res);
                }

                // Scenario respawn - random if > 1 
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

                // Keep respawn.
                if (player.CurrentKeep != null)
                {
                    _logger.Debug($"Player {player.Name} is attached to keep {player.CurrentKeep.Name} - using Keep respawn");
                    return player.CurrentKeep.GetSpawnPoint(player);
                }

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
                // World Spawns
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



        #endregion

        #region Xp / Renown

        public static uint GenerateXPCount(Player plr, Unit victim)
        {
            uint KLvl = plr.AdjustedLevel;
            uint VLvl = victim.AdjustedLevel;

            if (KLvl > VLvl + 8)
                    return 0;

            uint XP = VLvl * 100;

            if (victim is Creature)
            {
                switch (victim.Rank)
                {
                    case 1:
                        XP *= 4; break;
                    case 2:
                        if (plr.WorldGroup != null)
                            XP *= 12;
                        break;
                }
            }

            if (KLvl > VLvl)
                XP -= (uint)((XP / (float)100) * (KLvl - VLvl + 1)) * 5;

            if (Program.Config.XpRate > 0)
                XP *= (uint)Program.Config.XpRate;

            return XP;
        }

        public static void GenerateXP(Player killer, Unit victim, float bonusMod)
        {
            _logger.Trace($"Killer : {killer.Name} Victim : {victim.Name} Bonus : {bonusMod}");
            if (killer == null) return;

            if (killer.Level != killer.EffectiveLevel)
                bonusMod = 0.0f;

            if (killer.PriorityGroup == null)
            {

                killer.AddXp((uint)(GenerateXPCount(killer, victim) * bonusMod), true, true);
            }
            else
            {
                _logger.Trace($"Priority Group : {killer.Name} Victim : {victim.Name} Bonus : {bonusMod}");
                killer.PriorityGroup.AddXpFromKill(killer, victim, bonusMod);
            }
        }

        public static uint GenerateRenownCount(Player killer, Player victim)
        {
            if (killer == null || victim == null || killer == victim)
                return 0;

            uint renownPoints = (uint)(7.31f * (victim.AdjustedRenown + victim.AdjustedLevel));

            if (killer.AdjustedLevel > 15 && killer.AdjustedLevel < 31)
                renownPoints = (uint)(renownPoints * 1.0f);

            return renownPoints;
        }

        #endregion

        #region items


        public static void SendDynamicVendorItems(Player plr, List<Vendor_items> items)
        {
            if (plr == null)
                return;


            byte Page = 0;
            int Count = items.Count;
            while (Count > 0)
            {
                byte ToSend = (byte)Math.Min(Count, VendorService.MAX_ITEM_PAGE);
                if (ToSend <= Count)
                    Count -= ToSend;
                else
                    Count = 0;

                WorldMgr.SendVendorPage(plr, ref items, ToSend, Page);

                ++Page;
            }
            plr.ItmInterface.SendBuyBack();
        }

        public static void SendVendor(Player Plr, ushort id)
        {
            if (Plr == null)
                return;

            //guildrank check
            List<Vendor_items> Itemsprecheck = VendorService.GetVendorItems(id).ToList();
            List<Vendor_items> Items = new List<Vendor_items>();

            foreach (Vendor_items vi in Itemsprecheck)
            {
                if (vi.ReqGuildlvl > 0 && Plr.GldInterface.IsInGuild() && vi.ReqGuildlvl > Plr.GldInterface.Guild.Info.Level)
                    continue;
                Items.Add(vi);
            }


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
        public static void SendVendorPage(Player Plr, ref List<Vendor_items> items, byte Count, byte Page)
        {
            Count = (byte)Math.Min(Count, items.Count);

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

            for (byte i = 0; i < Count; ++i)
            {
                Out.WriteByte(i);
                Out.WriteByte(1);
                Out.WriteUInt32(items[i].Price);
                Item.BuildItem(ref Out, null, items[i].Info, null, 0, 1);

                if (Plr != null && Plr.ItmInterface != null && items[i].Info != null && items[i].Info.ItemSet != 0)
                    Plr.ItmInterface.SendItemSetInfoToPlayer(Plr, items[i].Info.ItemSet);

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
                if ((byte)items[i].ItemsReq.Count == 1)
                    Out.Fill(0, 18);
                else if ((byte)items[i].ItemsReq.Count == 2)
                    Out.Fill(0, 9);
                else
                    Out.Fill(0, 1);

            }

            Out.WriteByte(0);
            Plr.SendPacket(Out);

            items.RemoveRange(0, Count);
        }

        public static void BuyItemVendor(Player Plr, InteractMenu Menu, ushort id)
        {
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            //guildrank check
            List<Vendor_items> Itemsprecheck = VendorService.GetVendorItems(id).ToList();
            List<Vendor_items> Vendors = new List<Vendor_items>();

            foreach (Vendor_items vi in Itemsprecheck)
            {
                if (vi.ReqGuildlvl > 0 && Plr.GldInterface.IsInGuild() && vi.ReqGuildlvl > Plr.GldInterface.Guild.Info.Level)
                    continue;
                Vendors.Add(vi);
            }

            if (Vendors.Count <= Num)
                return;

            if (!Plr.HasMoney((Vendors[Num].Price) * Count))
            {
                Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_MONEY_TO_BUY);
                return;
            }

            foreach (KeyValuePair<uint, ushort> Kp in Vendors[Num].ItemsReq)
            {
                if (!Plr.ItmInterface.HasItemCountInInventory(Kp.Key, (ushort)(Kp.Value * Count)))
                {
                    Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_FAIL_PURCHASE_REQUIREMENT);
                    return;
                }
            }

            if (Vendors[Num].ReqTokUnlock > 0 && !Plr.TokInterface.HasTok(Vendors[Num].ReqTokUnlock))
                return;

            ItemResult result = Plr.ItmInterface.CreateItem(Vendors[Num].Info, Count);
            if (result == ItemResult.RESULT_OK)
            {
                Plr.RemoveMoney(Vendors[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in Vendors[Num].ItemsReq)
                    Plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));
            }
            else if (result == ItemResult.RESULT_MAX_BAG)
            {
                Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MERCHANT_INSUFFICIENT_SPACE_TO_BUY);
            }
            else if (result == ItemResult.RESULT_ITEMID_INVALID)
            {

            }
        }

        public static void BuyItemHonorDynamicVendor(Player plr, InteractMenu Menu, List<Vendor_items> items)
        {
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            if (items.Count <= Num)
                return;


            var honorVendor = new HonorVendorItem(plr);
            var reward = HonorService.HonorRewards.SingleOrDefault(x => x.ItemId == items[Num].Info.Entry);
            if (reward == null)
                return;
            if (!honorVendor.IsValidItemForPlayer(plr, reward))
                return;

            ItemResult result = plr.ItmInterface.CreateItem(items[Num].Info, (ushort)reward.ItemCount);
            if (result == ItemResult.RESULT_OK)
            {
                plr.RemoveMoney(items[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in items[Num].ItemsReq)
                    plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));

                items.Remove(items[Num]);

                // Set the Honor Reward Cooldown
                // Remove all existing Honor Reward Cooldowns for this item
                var existingRewards = plr.Info.HonorCooldowns?.Where(x => x.ItemId == reward.ItemId);
                foreach (var existingReward in existingRewards)
                {
                    plr.Info.HonorCooldowns.Remove(existingReward);
                    CharMgr.Database.DeleteObject(existingReward);
                }

                // Add the definitive reward cooldown for this item
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

        public static void BuyItemRealmCaptainDynamicVendor(Player plr, InteractMenu Menu, List<Vendor_items> items)
        {
            int Num = (Menu.Page * VendorService.MAX_ITEM_PAGE) + Menu.Num;
            ushort Count = Menu.Packet.GetUint16();
            if (Count == 0)
                Count = 1;

            if (items.Count <= Num)
                return;

            if (RealmCaptainManager.IsPlayerRealmCaptain(plr.CharacterId))
            {
                RealmCaptainManager.ApplyRealmCaptainBuff(plr, items[Num].Info.SpellId);
            }
        }

        private static void BuffAssigned(NewBuff buff)
        {
            var newBuff = buff;
        }

        #endregion

        #region Quests
        // TODO move that to QuestService
        public static void GenerateObjective(Quest_Objectives Obj, Quest Q)
        {
            switch ((Objective_Type)Obj.ObjType)
            {
                case Objective_Type.QUEST_KILL_PLAYERS:
                    {
                        if (Obj.Description.Length < 1)
                            Obj.Description = "Enemy Players";
                    }
                    break;

                case Objective_Type.QUEST_SPEAK_TO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Creature = CreatureService.GetCreatureProto(ObjID);

                        if (Obj.Creature == null)
                        {
                            Obj.Description = "Invalid NPC - " + Obj.Entry + ",ObjId=" + Obj.ObjID;
                        }
                        else
                        {
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Creature.Name.Length)
                                Obj.Description = "Speak to " + Obj.Creature.Name;
                        }

                    }
                    break;

                case Objective_Type.QUEST_USE_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID);

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

                case Objective_Type.QUEST_KILL_MOB:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Creature = CreatureService.GetCreatureProto(ObjID);

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

                case Objective_Type.QUEST_KILL_GO:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.GameObject = GameObjectService.GetGameObjectProto(ObjID);

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

                case Objective_Type.QUEST_USE_ITEM:
                case Objective_Type.QUEST_GET_ITEM:
                    {
                        uint ObjID = 0;
                        uint.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
                            Obj.Item = ItemService.GetItem_Info(ObjID);
                            if (Obj.Item == null)
                            {
                                int a = Obj.Quest.Particular.IndexOf("kill the ", StringComparison.OrdinalIgnoreCase);
                                if (a >= 0)
                                {
                                    string[] RestWords = Obj.Quest.Particular.Substring(a + 9).Split(' ');
                                    string Name = RestWords[0] + " " + RestWords[1];
                                    Creature_proto Proto = CreatureService.GetCreatureProtoByName(Name) ?? CreatureService.GetCreatureProtoByName(RestWords[0]);
                                    if (Proto != null)
                                    {
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

                case Objective_Type.QUEST_WIN_SCENARIO:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                            Obj.Scenario = ScenarioService.GetScenario_Info(ObjID);

                        if (Obj.Scenario == null)
                            Obj.Description = "Invalid Scenario - QuestID=" + Obj.Entry + ", ObjId=" + Obj.ObjID;
                        else
                            if (Obj.Description == null || Obj.Description.Length <= Obj.Scenario.Name.Length)
                            Obj.Description = "Win " + Obj.Scenario.Name;
                    }
                    break;

                case Objective_Type.QUEST_CAPTURE_BO:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
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

                case Objective_Type.QUEST_CAPTURE_KEEP:
                    {
                        ushort ObjID = 0;
                        ushort.TryParse(Obj.ObjID, out ObjID);

                        if (ObjID != 0)
                        {
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
        #endregion

        #region Relation

        [LoadingFunction(false)]
        public static void LoadRelation()
        {
            Log.Success("LoadRelation", "Loading Relations");

            foreach (Item_Info info in ItemService._Item_Info.Values)
            {
                if (info.Career != 0)
                {
                    foreach (KeyValuePair<byte, CharacterInfo> Kp in CharMgr.CharacterInfos)
                    {
                        if ((info.Career & (1 << (Kp.Value.CareerLine - 1))) == 0)
                            continue;

                        info.Realm = Kp.Value.Realm;
                        break;

                    }
                }

                else if (info.Race > 0)
                {
                    if (((Constants.RaceMaskDwarf + Constants.RaceMaskHighElf + Constants.RaceMaskEmpire) & info.Race) > 0)
                        info.Realm = 1;
                    else info.Realm = 2;
                }
            }

            LoadChapters();
            LoadPublicQuests();
            LoadQuestsRelation();
            LoadScripts(false);

            foreach (List<Keep_Info> keepInfos in BattleFrontService._KeepInfos.Values)
                foreach (Keep_Info keepInfo in keepInfos)
                    if (PQuestService._PQuests.ContainsKey(keepInfo.PQuestId))
                        keepInfo.PQuest = PQuestService._PQuests[keepInfo.PQuestId];

            CharMgr.Database.ExecuteNonQuery("UPDATE war_characters.characters_value SET Online=0;");

            // Preload T4 regions
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

                if (Zone == null || (Info.PinX <= 0 && Info.PinY <= 0))
                {
                    _logger.Warn("LoadChapters Chapter (" + Info.Entry + ")[" + Info.Name + "] Invalid");
                    ++InvalidChapters;
                }

                if (Info.T1Rewards == null)
                    Info.T1Rewards = new List<Chapter_Reward>();
                if (Info.T2Rewards == null)
                    Info.T2Rewards = new List<Chapter_Reward>();
                if (Info.T3Rewards == null)
                    Info.T3Rewards = new List<Chapter_Reward>();

                List<Chapter_Reward> Rewards;
                if (ChapterService._Chapters_Reward.TryGetValue(Info.Entry, out Rewards))
                {
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

                CellSpawnService.GetRegionCell(Zone.Region, (ushort)((float)(Info.PinX / 4096) + Zone.OffX), (ushort)((float)(Info.PinY / 4096) + Zone.OffY)).AddChapter(Info);
            }



            if (InvalidChapters > 0)
                _logger.Warn("LoadChapters", "[" + InvalidChapters + "] Invalid Chapter(s)");
        }
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


                if (!PQuestService._PQuest_Objectives.TryGetValue(Info.Entry, out Info.Objectives))
                    Info.Objectives = new List<PQuest_Objective>();
                else
                {
                    foreach (PQuest_Objective Obj in Info.Objectives)
                    {
                        Obj.Quest = Info;
                        PQuestService.GeneratePQuestObjective(Obj, Obj.Quest);

                        if (!PQuestService._PQuest_Spawns.TryGetValue(Obj.Guid, out Obj.Spawns))
                            Obj.Spawns = new List<PQuest_Spawn>();
                    }
                }

                Log.Info("LoadPublicQuests", "Loaded public quest "+Info.Entry+" to region "+Zone.Region+" cell at X: "+ ((float)(Info.PinX / 4096) + Zone.OffX)+" "+ (float)(Info.PinY / 4096) + Zone.OffY);

                bool skipLoad = false;

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
        public static void LoadQuestsRelation()
        {
            QuestService.LoadQuestCreatureStarter();
            QuestService.LoadQuestCreatureFinisher();

            foreach (KeyValuePair<uint, Creature_proto> Kp in CreatureService.CreatureProtos)
            {
                Kp.Value.StartingQuests = QuestService.GetStartQuests(Kp.Key);
                Kp.Value.FinishingQuests = QuestService.GetFinishersQuests(Kp.Key);
            }

            Quest quest;

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

            foreach (Quest_Map Q in QuestService._QuestMaps)
            {
                quest = QuestService.GetQuest(Q.Entry);
                if (quest == null)
                    continue;

                quest.Maps.Add(Q);
            }

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

            foreach (KeyValuePair<int, Quest_Objectives> Kp in QuestService._Objectives)
            {
                if (Kp.Value.Quest == null)
                    continue;
                GenerateObjective(Kp.Value, Kp.Value.Quest);
            }

            string sItemID, sCount;
            uint ItemID, Count;
            Item_Info Info;
            foreach (KeyValuePair<ushort, Quest> Kp in QuestService._Quests)
            {
                if (Kp.Value.Choice.Length <= 0)
                    continue;

                // [5154,12],[128,1]
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


        #endregion

        #region Scripts

        public static CSharpScriptCompiler ScriptCompiler;
        public static Dictionary<string, Type> LocalScripts = new Dictionary<string, Type>();
        public static Dictionary<string, AGeneralScript> GlobalScripts = new Dictionary<string, AGeneralScript>();
        public static Dictionary<uint, Type> CreatureScripts = new Dictionary<uint, Type>();
        public static Dictionary<uint, Type> GameObjectScripts = new Dictionary<uint, Type>();
        public static ScriptsInterface GeneralScripts;

        public static void LoadScripts(bool Reload)
        {
            GeneralScripts = new ScriptsInterface();

            ScriptCompiler = new CSharpScriptCompiler();
            ScriptCompiler.LoadScripts();
            GeneralScripts.ClearScripts();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass != true)
                        continue;

                    if (!type.IsSubclassOf(typeof(AGeneralScript)))
                        continue;

                    foreach (GeneralScriptAttribute at in type.GetCustomAttributes(typeof(GeneralScriptAttribute), true))
                    {
                        if (!string.IsNullOrEmpty(at.ScriptName))
                            at.ScriptName = at.ScriptName.ToLower();

                        Log.Success("Scripting", "Registering Script :" + at.ScriptName);

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

            if (Reload)
            {
                if (Program.Server != null)
                    Program.Server.LoadPacketHandler();
            }
        }

        public static AGeneralScript GetScript(Object Obj, string ScriptName)
        {
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

        public static void UpdateScripts(long Tick)
        {
            GeneralScripts.Update(Tick);
        }

        #endregion

        #region Scenarios
        public static ScenarioMgr ScenarioMgr;

        public static InstanceMgr InstanceMgr;

        [LoadingFunction(true)]
        public static void StartScenarioMgr()
        {
            ScenarioMgr = new ScenarioMgr(ScenarioService.ActiveScenarios);
        }

        [LoadingFunction(true)]
        public static void StartInstanceMgr()
        {
            InstanceMgr = new InstanceMgr();
        }

        #endregion

        #region Settings

        public static WorldSettingsMgr WorldSettingsMgr;

        [LoadingFunction(true)]
        public static void StartWorldSettingsMgr()
        {
            WorldSettingsMgr = new WorldSettingsMgr();
        }

        #endregion



        #region Campaign

        public static void WorldUpdateStart()
        {
            Log.Debug("WorldMgr", "Starting World Monitor...");

            _worldThread = new Thread(WorldUpdate);
            _worldThread.Start();
        }

        public static void GroupUpdateStart()
        {
            Log.Debug("WorldMgr", "Starting Group Updater...");

            _groupThread = new Thread(GroupUpdate);
            _groupThread.Start();
        }


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
        /// Show swords on world map if zone has people fighting it
        /// </summary>
        public static void SendZoneFightLevel(Player player = null)
        {
            var fightLevel = GetZonesFightLevel();

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_HOT_SPOT);
            Out.WriteByte((byte)fightLevel.Count);
            Out.WriteByte(2); //world hotspots
            Out.WriteByte(0);

            //fight level
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

            if (player != null)
                player.SendPacket(Out);
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

            foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
            {
                foreach (var zone in region.ZonesMgr.ToList())
                {
                    zone.SendHotSpots(player);
                }
            }
        }


        private static void WorldUpdate()
        {
            while (_running)
            {
                if (ZoneService._Zone_Info != null)
                {
                    SendZoneFightLevel();

                    foreach (var region in WorldMgr._Regions.Where(e => e.Campaign != null).ToList())
                    {
                        foreach (var zone in region.ZonesMgr.ToList())
                        {
                            zone.DecayHotspots();
                        }
                    }
                }
                Thread.Sleep(15000);
            }

        }


        private static void GroupUpdate()
        {
            while (_running)
            {
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
                List<KeyValuePair<uint, GroupAction>> _worldActions = new List<KeyValuePair<uint, GroupAction>>();
                lock (Group._pendingGroupActions)
                {
                    foreach (KeyValuePair<uint, GroupAction> kp in Group._pendingGroupActions)
                    {
                        _worldActions.Add(kp);
                    }
                    Group._pendingGroupActions.Clear();
                }

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

                Thread.Sleep(100);
            }

        }



        #endregion

        #region Keep registry, to remove it's static bullshit
        public static Dictionary<uint, BattleFrontKeep> _Keeps = new Dictionary<uint, BattleFrontKeep>();

        public static void SendKeepStatus(Player Plr)
        {
            foreach (List<Keep_Info> list in BattleFrontService._KeepInfos.Values)
            {
                foreach (Keep_Info KeepInfo in list)
                {
                    if (_Keeps.ContainsKey(KeepInfo.KeepId))
                    {
                        _Keeps[KeepInfo.KeepId].KeepCommunications.SendKeepStatus(Plr, _Keeps[KeepInfo.KeepId]);
                    }
                    else
                    {
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
        #endregion

        #region Logging
        [LoadingFunction(true)]
        public static void ResetPacketLogSettings()
        {
            //turn off user specific packet logging when server restarts. This is because devs/gm forget to turn it off and log file grows > 20GB
            Log.Debug("WorldMgr", "Resetting user packet log settings...");
            Database.ExecuteNonQuery("update war_accounts.accounts set PacketLog = 0");
        }
        #endregion

        #region Other


        #endregion

        public static void AttachCampaignsToRegions()
        {

            foreach (var regionMgr in _Regions)
            {
                var objectiveList = LoadObjectives(regionMgr);
                switch (regionMgr.RegionId)
                {
                    case 1: // t1 dw/gs
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new ApocCommunications());
                        break;
                    case 3: // t1 he/de
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new ApocCommunications());
                        break;
                    case 8: // t1 em/ch
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.LowerTierCampaignManager, new ApocCommunications());
                        break;
                    // Tier 4
                    case 11:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new ApocCommunications());
                        break;
                    case 2:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new ApocCommunications());
                        break;
                    case 4:
                        regionMgr.Campaign = new Campaign(regionMgr, objectiveList, new HashSet<Player>(), WorldMgr.UpperTierCampaignManager, new ApocCommunications());
                        break;

                    default: // Everything else...
                        break;
                }
            }
        }

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
            foreach (BattleFront_Objective obj in objectives.Where(x => x.KeepSpawn == false))
            {
                BattlefieldObjective flag = new BattlefieldObjective(regionMgr, obj);
                resultList.Add(flag);
            }

            return resultList;
        }

        /// <summary>
        /// Inform the server of the change in the RVR Progression across all regions.
        /// </summary>
        public static void UpdateRegionCaptureStatus(LowerTierCampaignManager lowerTierCampaignManager, UpperTierCampaignManager upperTierCampaignManager)
        {
            if ((lowerTierCampaignManager == null) || (upperTierCampaignManager == null))
                return;
            _logger.Trace("F_CAMPAIGN_STATUS1");
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAMPAIGN_STATUS, 159);
            Out.WriteHexStringBytes("0005006700CB00"); // 7


            // Dwarfs vs Greenskins T1
            Out.WriteByte(0);    // 0 and ignored
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND).DestructionVictoryPointPercentage);    // % Dest lock
            // Dwarfs vs Greenskins T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(12, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Dwarfs vs Greenskins T3
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(10, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Dwarfs vs Greenskins T4
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(2, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Empire vs Chaos T1
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(8, false), realm);
            Out.WriteByte(0);
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND).DestructionVictoryPointPercentage);    // % Dest lock
            // Empire vs Chaos T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(14, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Empire vs Chaos T3
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(6, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(45);  // % Order lock
            Out.WriteByte(55);    // % Dest lock
            // Empire vs Chaos T4
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(11, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(40);  // % Order lock
            Out.WriteByte(60);    // % Dest lock
            // High Elves vs Dark Elves T1
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(3, false), realm);
            Out.WriteByte(0);
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE).OrderVictoryPointPercentage);  // % Order lock
            Out.WriteByte((byte)lowerTierCampaignManager.GetBattleFrontStatus(BattleFrontConstants.BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE).DestructionVictoryPointPercentage);    // % Dest lock
            // High Elves vs Dark Elves T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(15, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // High Elves vs Dark Elves T3
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(16, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // High Elves vs Dark Elves T4
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(4, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);  // % Order lock
            Out.WriteByte(0);    // % Dest lock

            Out.Fill(0, 83);

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

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player == null || player.IsDisposed || !player.IsInWorld())
                        continue;

                    player.SendPacket(Out);

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
                plr.RemoveMoney(items[Num].Price * Count);
                foreach (KeyValuePair<uint, ushort> Kp in items[Num].ItemsReq)
                    plr.ItmInterface.RemoveItems(Kp.Key, (ushort)(Kp.Value * Count));

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
