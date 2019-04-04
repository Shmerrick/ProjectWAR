using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.Threading;
using Common.Database.World.Creatures;
using NLog;
using WorldServer.Managers;
using WorldServer.World.Objects;

namespace WorldServer.Services.World
{
    [Service(typeof(ChapterService), typeof(ItemService))]
    public class CreatureService : ServiceBase
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static Dictionary<uint, Creature_proto> CreatureProtos;
        public static IList<BossSpawn> BossSpawns;
        public static IList<BossSpawnAbilities> BossSpawnAbilities;
        public static IList<BossSpawnPhase> BossSpawnPhases;
        public static List<CreatureSmartAbilities> CreatureSmartAbilities;

        #region Creature Proto
        [LoadingFunction(true)]
        public static void LoadCreatureProto()
        {
            Log.Debug("WorldMgr", "Loading Creature_Protos...");

            CreatureProtos = Database.MapAllObjects<uint, Creature_proto>("Entry");

            _logger.Trace($"Loading Creature Proto...");
            
            foreach (Creature_proto proto in CreatureProtos.Values)
            {
                if (proto.Model1 == 0 && proto.Model2 == 0)
                    proto.Model1 = proto.Model2 = 1;

                if (proto.CreatureType == 0 && proto.CreatureSubType == 0)
                    proto.SetCreatureTypesAndSubTypes();

                if (proto.MinLevel > proto.MaxLevel)
                    proto.MinLevel = proto.MaxLevel;

                if (proto.MaxLevel <= proto.MinLevel)
                    proto.MaxLevel = proto.MinLevel;

                if (proto.MaxLevel == 0) proto.MaxLevel = 1;
                if (proto.MinLevel == 0) proto.MinLevel = 1;

                if (proto.MinLevel == proto.MaxLevel && proto.MinLevel > 1)
                    proto.MaxLevel = (byte)(proto.MinLevel + 1);
                else if (proto.MaxLevel - proto.MinLevel > 3)
                    proto.MaxLevel = (byte)(proto.MinLevel + 2);

                _logger.Trace($"Proto details. " +
                              $"Entry: {proto.Entry} M1: {proto.Model1} M2: {proto.Model2} " +
                              $"CreatureType/SubType: {proto.CreatureType}/{proto.CreatureSubType} " +
                              $"Min/Max Level: {proto.MinLevel}/{proto.MaxLevel}");

                AssignProtoStates(proto);
            }

            _logger.Trace($"Finished Loading Creature Proto...");
            Log.Success("LoadCreatureProto", "Loaded " + CreatureProtos.Count + " Creature_Protos");
        }

        private static byte[] _defaultData = { 1, 10 };

        private static void AssignProtoStates(Creature_proto proto)
        {
            proto.InteractType = Unit.GenerateInteractType(proto);

            // Interact type
            switch (proto.TitleId)
            {
                case CreatureTitle.Apothecary: // Trade skill start
                case CreatureTitle.Butcher:
                case CreatureTitle.Salvager:
                case CreatureTitle.Scavenger:
                case CreatureTitle.Cultivator:
                case CreatureTitle.HedgeWizard:
                    proto.InteractTrainerType = 1;
                    break;
                case CreatureTitle.Trainer:
                case CreatureTitle.CareerTrainer:
                case CreatureTitle.ApprenticeCareerTrainer:
                    proto.InteractTrainerType = 6;
                    break;
                case CreatureTitle.RenownTrainer:
                case CreatureTitle.ApprenticeRenownTrainer:
                    proto.InteractTrainerType = 8;
                    break;
            }

            List<byte> states = proto.States != null ? new List<byte>(proto.States) : new List<byte>();

            if (proto.VendorID > 0)
                states.Add((byte)CreatureState.Merchant);

            switch (proto.InteractType)
            {
                case InteractType.INTERACTTYPE_TRAINER:
                    if (proto.InteractTrainerType == 1)
                        states.Add((byte)CreatureState.Merchant);
                    states.Add((byte)CreatureState.SkillTrainer);
                    if (proto.InteractTrainerType == 1)
                        states.Add((byte)CreatureState.ConsistentAppearance);
                    break;
                case InteractType.INTERACTTYPE_BANKER:
                    states.Add((byte)CreatureState.Banker);
                    break;
                case InteractType.INTERACTTYPE_AUCTIONEER:
                    states.Add((byte)CreatureState.Auctioneer);
                    break;
                case InteractType.INTERACTTYPE_GUILD_REGISTRAR:
                    states.Add((byte)CreatureState.GuildRegistrar);
                    break;
                case InteractType.INTERACTTYPE_FLIGHT_MASTER:
                    states.Add((byte)CreatureState.FlightMaster);
                    break;
                case InteractType.INTERACTTYPE_HEALER:
                    states.Add((byte)CreatureState.Healer);
                    break;
                case InteractType.INTERACTTYPE_LASTNAMESHOP:
                    states.Add((byte)CreatureState.NameRegistrar);
                    break;
            }

            if (proto.TitleId == CreatureTitle.KillCollector)
            {
                states.Add((byte)CreatureState.KillCollector);
                states.Add((byte)CreatureState.ConsistentAppearance);
            }

            if (proto.TitleId == CreatureTitle.RallyMaster) // Rally Master
            {
                byte chapterId = (byte)ChapterService.GetChapterByNPCID(proto.Entry);
                if (chapterId > 0)
                    states.Add((byte)CreatureState.Influence);

                states.Add((byte)CreatureState.RallyMasterIcon);
                states.Add((byte)CreatureState.ConsistentAppearance);
            }

            if (proto.CreatureType == (byte)GameData.CreatureTypes.SIEGE)
                states.Add((byte)CreatureState.RvRFlagged);

            if (proto.VendorID == 0 && proto.InteractType == InteractType.INTERACTTYPE_DYEMERCHANT)
            {
                states.Add((byte)CreatureState.Merchant);
                states.Add((byte)CreatureState.DyeMerchant);
            }

            // Seems omnipresent, function unknown
            else
                states.Add((byte)CreatureState.UnkOmnipresent);

            proto.States = states.ToArray();

            /* Assign fig leaf data here.

            Notes:

            It seems as though in a 0, 0, 0, X, 1, 10 chain, the X byte is a bitfield. Possible values:

            8 - Non-combatant (hides name, shows in white - used widely in capital city maps)
            4 - Cannot be targeted
            2 - ?
            1 - ?

            */

            if (proto.FigLeafData == null)
            {
                if (proto.CreatureType == (byte)GameData.CreatureTypes.SIEGE)
                    proto.FigLeafData = new byte[] { 0, 0, 0, 11, 1, 10 };
                else
                    switch (proto.Title)
                    {
                        case 1:
                        case 2:
                        case 40:
                        case 144: // Career Trainer
                            proto.FigLeafData = new byte[] { 0, 0, 0, 1, 1, 10 };
                            break;
                        case 17: // Rally Master
                            byte chapterId = (byte)ChapterService.GetChapterByNPCID(proto.Entry);
                            if (chapterId > 0)
                                proto.FigLeafData = new byte[] { 0, 0, 0, 3, 0, chapterId, 1, 10 };
                            else
                                proto.FigLeafData = _defaultData;
                            break;
                        case 4: // Trade skill start
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9: // Trade skill end
                        case 18: // Flight Master
                        case 20: // Healer
                        case 32: // Kill collector
                            proto.FigLeafData = new byte[] { 0, 0, 0, 3, 1, 10 };
                            break;
                        default:
                            proto.FigLeafData = _defaultData;
                            break;
                    }
            }
        }

        public static Creature_proto GetCreatureProto(uint Entry)
        {
            Creature_proto Proto;
            CreatureProtos.TryGetValue(Entry, out Proto);
            return Proto;
        }

        public static Creature_proto GetCreatureProtoByName(string Name)
        {
            foreach (KeyValuePair<uint, Creature_proto> Kp in CreatureProtos)
                if (Name.Equals(Kp.Value.Name, StringComparison.OrdinalIgnoreCase))
                    return Kp.Value;
            return null;
        }
		#endregion

		//#region patrol calculations
		
		//public static Point3D GetNewRandomGuardPosition(uint zoneId, Point3D mid)
		//{
		//	Random random = new Random();
		//	int x = mid.X + 250 - (50 * random.Next(1, 11));
		//	int y = mid.Y + 250 - (50 * random.Next(1, 11));
		//	int z = ClientFileMgr.GetHeight((int)zoneId, x, y);

		//	return new Point3D(x, y, z);
		//}

		//#endregion

		#region Creature spawn
		public static Dictionary<uint, Creature_spawn> CreatureSpawns;
        public static int MaxCreatureGUID;

        public static int GenerateCreatureSpawnGUID()
        {
            return Interlocked.Increment(ref MaxCreatureGUID);
        }

        [LoadingFunction(true)]
        public static void LoadBossSpawns()
        {
            Log.Debug("WorldMgr", "Loading Boss_Spawns...");

            BossSpawns = Database.SelectAllObjects<BossSpawn>();
            Log.Success("LoadBossSpawns", "Loaded " + BossSpawns.Count + " BossSpawns");

            BossSpawnAbilities = Database.SelectAllObjects<BossSpawnAbilities>();
            Log.Success("LoadBossSpawns", "Loaded " + BossSpawnAbilities.Count + " BossSpawnAbilities");

            BossSpawnPhases = Database.SelectAllObjects<BossSpawnPhase>();
            Log.Success("LoadBossSpawnPhases", "Loaded " + BossSpawnPhases.Count + " BossSpawnPhase");

        }





        [LoadingFunction(true)]
        public static void LoadCreatureSpawns()
        {
            Log.Debug("WorldMgr", "Loading Creature_Spawns...");

            //Added parameter Enabled = 1, this will allow to disable spawn without removing it from DB. 
            //Enabled = 1 means that creature will spawn, Enabled = 0 means it will not spawn.
            CreatureSpawns = Database.MapAllObjects<uint, Creature_spawn>("Guid", "Enabled = 1", 100000);
            
            foreach (Creature_spawn Spawn in CreatureSpawns.Values)
            {
                if (Spawn.Guid > MaxCreatureGUID)
                    MaxCreatureGUID = (int)Spawn.Guid;
            }

            Log.Success("LoadCreatureSpawns", "Loaded " + CreatureSpawns.Count + " Creature_Spawns");

            CreatureSmartAbilities = (List<CreatureSmartAbilities>) Database.SelectAllObjects<CreatureSmartAbilities>();
            Log.Success("CreatureSmartAbilities", "Loaded " + CreatureSmartAbilities.Count + " CreatureSmartAbilities");

        }
        #endregion

        #region CreatureText
        private static Dictionary<uint, List<Creature_text>> _creatureTexts = new Dictionary<uint, List<Creature_text>>();

        [LoadingFunction(true)]
        public static void LoadCreatureTexts()
        {
            _creatureTexts = new Dictionary<uint, List<Creature_text>>();

            Log.Debug("WorldMgr", "Loading Creature_texts...");

            IList<Creature_text> texts = Database.SelectAllObjects<Creature_text>();

            int count = 0;
            foreach (Creature_text text in texts)
            {
                if (!_creatureTexts.ContainsKey(text.Entry))
                    _creatureTexts.Add(text.Entry, new List<Creature_text>());

                _creatureTexts[text.Entry].Add(text);
                ++count;
            }

            Log.Success("WorldMgr", "Loaded " + count + " Creature Texts");
        }

        public static void AddCreatureText(uint protoEntry, string text)
        {
            Creature_text creatureText = new Creature_text
            {
                Text = text,
                Entry = protoEntry
            };

            Database.AddObject(creatureText);

            if (!_creatureTexts.ContainsKey(protoEntry))
                _creatureTexts.Add(protoEntry, new List<Creature_text>());
            _creatureTexts[protoEntry].Add(creatureText);
        }

        public static string GetCreatureText(uint protoEntry)
        {
            string text = "";

            if (_creatureTexts.ContainsKey(protoEntry))
            {
                int randomNum = StaticRandom.Instance.Next(_creatureTexts[protoEntry].Count);
                text = _creatureTexts[protoEntry][randomNum].Text;
            }

            return text;
        }
        #endregion
        
        #region CreatureItems

        public static Dictionary<uint, List<Creature_item>> _CreatureItems;

        [LoadingFunction(true)]
        public static void LoadCreatureItems()
        {
            Log.Debug("WorldMgr", "Loading Creature_Items...");

            _CreatureItems = new Dictionary<uint, List<Creature_item>>();
            IList<Creature_item> Items = Database.SelectAllObjects<Creature_item>();

            if (Items != null)
                foreach (Creature_item Item in Items)
                {
                    if (!_CreatureItems.ContainsKey(Item.Entry))
                        _CreatureItems.Add(Item.Entry, new List<Creature_item>());

                    _CreatureItems[Item.Entry].Add(Item);
                }

            Log.Success("LoadCreatureItems", "Loaded " + (Items != null ? Items.Count : 0) + " Creature_Items");
        }

        public static List<Creature_item> GetCreatureItems(uint Entry)
        {
            List<Creature_item> L;

            lock (_CreatureItems)
            {
                if (!_CreatureItems.TryGetValue(Entry, out L))
                {
                    L = new List<Creature_item>();
                    _CreatureItems.Add(Entry, L);
                }
            }

            return L;
        }

        public static void RemoveCreatureItem(uint Entry, ushort Slot)
        {
            List<Creature_item> Items = GetCreatureItems(Entry);
            Items.RemoveAll(info =>
            {
                if (info.SlotId == Slot)
                {
                    Database.DeleteObject(info);
                    return true;
                }
                return false;
            });
        }

        public static void AddCreatureItem(Creature_item Item)
        {
            RemoveCreatureItem(Item.Entry, Item.SlotId);

            List<Creature_item> Items = GetCreatureItems(Item.Entry);
            Items.Add(Item);
            Database.AddObject(Item);
        }

        #endregion

        #region CreatureStats

        public static Dictionary<uint, List<Creature_stats>> _CreatureStats;

        [LoadingFunction(true)]
        public static void LoadCreatureStats()
        {
            Log.Debug("WorldMgr", "Loading Creature_stats...");

            _CreatureStats = new Dictionary<uint, List<Creature_stats>>();
            IList<Creature_stats> Stats = Database.SelectAllObjects<Creature_stats>();

            if (Stats != null)
            { 
                foreach (Creature_stats statInfo in Stats)
                {
                    if (!_CreatureStats.ContainsKey(statInfo.ProtoEntry))
                    {
                        List<Creature_stats> stat = new List<Creature_stats>(1) { statInfo };
                        _CreatureStats.Add(statInfo.ProtoEntry, stat);
                    }
                    else
                        _CreatureStats[statInfo.ProtoEntry].Add(statInfo);
                }
            }

            Log.Success("LoadCreatureStats", "Loaded " + (Stats != null ? Stats.Count : 0) + " Creature_stats");
        }

        public static List<Creature_stats> GetCreatureStats(uint CreatureProto)
        {
            List<Creature_stats> L;

            lock (_CreatureStats)
            {
                if (!_CreatureStats.TryGetValue(CreatureProto, out L))
                {
                    L = new List<Creature_stats>();
                    _CreatureStats.Add(CreatureProto, L);
                }
            }

            return L;
        }

        #endregion

        #region LootGroups

        public static Dictionary<uint, Loot_Group> LootGroups = new Dictionary<uint, Loot_Group>();
        public static Dictionary<uint, List<Loot_Group_Item>> LootGroupItems = new Dictionary<uint, List<Loot_Group_Item>>();

        [LoadingFunction(true)]
        public static void LoadLootGroups()
        {
            IList<Loot_Group> dbLootGroups = Database.SelectAllObjects<Loot_Group>();
            IList<Loot_Group_Item> dbLootGroupItems = Database.SelectAllObjects<Loot_Group_Item>();

            if (dbLootGroupItems != null)
            {
                // Pre-load each loot group item into a dictionary, where the key is the LootGroupID
                // and the value is a List of Loot Group Items belonging to the keyed Loot Group.
                foreach (Loot_Group_Item lgi in dbLootGroupItems)
                {
                    if (!LootGroupItems.ContainsKey(lgi.LootGroupID))
                        LootGroupItems.Add(lgi.LootGroupID, new List<Loot_Group_Item>());
                    LootGroupItems[lgi.LootGroupID].Add(lgi);
                }
            }

            if (dbLootGroups != null)
            {
                foreach (Loot_Group lg in dbLootGroups)
                {
                    Log.Debug("WorldMgr.LoadLootGroups", "Loading LootGroup #" + lg.Entry + " ...");
                    // Since not all loot groups will have items assigned, we have to make sure we don't load empty loot groups into 
                    if (LootGroupItems.ContainsKey(lg.Entry))
                    {
                        // Since we already have a list of loot group items in the dictionary, we can copy/reference it directly.
                        lg.LootGroupItems = LootGroupItems[lg.Entry];
                    }

                    else
                        lg.LootGroupItems = new List<Loot_Group_Item>();

                    LootGroups.Add(lg.Entry, lg);
                }

                Log.Success("WorldMgr", $"Successfully Loaded {dbLootGroups.Count} loot groups.");
            }
        }

        public static Loot_Group GetLootGroup(uint entry)
        {
            if (LootGroups.ContainsKey(entry))
                return LootGroups[entry];
            return null;
        }

        public static List<Loot_Group> GetLootGroupsByEvent(byte killEvent)
        {
            List<Loot_Group> candidates = new List<Loot_Group>();

            foreach (Loot_Group lg in LootGroups.Values)
            {
                if ((lg.DropEvent & killEvent) == killEvent)
                    candidates.Add(lg);
            }

            return candidates;
        }

        #endregion

        #region Butchery

        public static Dictionary<byte, List<uint>> ButcheryEntries = new Dictionary<byte, List<uint>>();

        [LoadingFunction(true)]
        public static void LoadButcheryGroups()
        {
            IList<Loot_Group_Butcher> butcheryEntries = Database.SelectAllObjects<Loot_Group_Butcher>();

            foreach (Loot_Group_Butcher butcherInfo in butcheryEntries)
            {
                if (!ButcheryEntries.ContainsKey(butcherInfo.CreatureSubType))
                    ButcheryEntries.Add(butcherInfo.CreatureSubType, new List<uint>());
                ButcheryEntries[butcherInfo.CreatureSubType].Add(butcherInfo.ItemID);
            }
        }

        public static bool IsButcherable(byte creatureSubType)
        {
            return ButcheryEntries.ContainsKey(creatureSubType);
        }

        public static LootContainer GenerateButchery(byte creatureSubType, byte skillLv)
        {
            if (!ButcheryEntries.ContainsKey(creatureSubType))
                return null;

            if (skillLv > Program.Config.RankCap * 5)
                skillLv = (byte)(Program.Config.RankCap * 5);

            uint itemId = ButcheryEntries[creatureSubType][StaticRandom.Instance.Next(ButcheryEntries[creatureSubType].Count)] + (uint)Math.Min(8, skillLv / 25);

            return new LootContainer { LootInfo = new List<LootInfo> { new LootInfo(ItemService.GetItem_Info(itemId)) } };
        }

        public static bool IsScavengeable(byte creatureSubType)
        {
            return !ButcheryEntries.ContainsKey(creatureSubType) && creatureSubType > 5;
        }

        public static readonly uint[] _fragmentIDs = { 908261, 908221, 908181, 908101, 907901 };

        public static LootContainer GenerateScavenge(byte unitLevel, byte unitRank, byte skillLv)
        {
            uint itemId;

            if (skillLv > Program.Config.RankCap * 5)
                skillLv = (byte)(Program.Config.RankCap * 5);

            if (StaticRandom.Instance.Next(100) <= 40)
            {
                // Gold dust.
                if (StaticRandom.Instance.Next(100) >= 60)
                    itemId = (uint)(907638 + skillLv / 25);
                // Curios.
                else itemId = (uint)(907501 + skillLv / 25 * 4);
            }

            // Fragments.
            else
            {
                itemId = (uint)(_fragmentIDs[StaticRandom.Instance.Next(_fragmentIDs.Length)] + skillLv / 25 * 4);

                int rand = StaticRandom.Instance.Next(100);

                if (rand < 1 + unitRank * 7)
                    itemId += 3;
                else if (rand < 2 + unitRank * 21)
                    itemId += 2;
                else if (rand > 65 - (unitLevel / 2) - (unitRank * 10))
                    itemId += 1;
            }

            return new LootContainer { LootInfo = new List<LootInfo> { new LootInfo(ItemService.GetItem_Info(itemId)) } };
        }

        #endregion
    }
}
