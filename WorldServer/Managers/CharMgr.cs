using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SystemData;
using Common;
using Common.Database.World.Characters;
using FrameWork;
using GameData;
using NLog;
using WorldServer.NetWork;
using WorldServer.Services.World;
using WorldServer.World.Auction;
using WorldServer.World.Guild;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;

namespace WorldServer.Managers
{
    public class AccountChars
    {
        public int AccountId;
        public Realms Realm = Realms.REALMS_REALM_NEUTRAL;

        public bool Loaded { get; set; }

        public AccountChars(int accountId)
        {
            AccountId = accountId;
        }

        public Character[] Chars = new Character[CharMgr.MaxSlot];

        public byte GenerateFreeSlot()
        {
            for (byte i = 0; i < Chars.Length; i++)
                if (Chars[i] == null)
                    return i;

            return CharMgr.MaxSlot;
        }

        public bool AddChar(Character Char)
        {
            if (Char == null)
                return false;

            Realm = (Realms)Char.Realm;

            Chars[Char.SlotId] = Char;

            return true;
        }
        public uint RemoveCharacter(byte slot)
        {
            uint characterId = 0;
            if (Chars[slot] != null)
                characterId = Chars[slot].CharacterId;

            Chars[slot] = null;
            Realm = Realms.REALMS_REALM_NEUTRAL;

            foreach (Character Char in Chars)
                if (Char != null)
                {
                    Realm = (Realms)Char.Realm;
                    break;
                }

            return characterId;
        }
        public Character GetCharacterBySlot(byte slot)
        {
            if (slot > Chars.Length)
                return null;

            return Chars[slot];
        }
    };

    [Service(
        typeof(ZoneService),
        typeof(PQuestService),
        typeof(ChapterService),
        typeof(ItemService))]
    public static class CharMgr
    {
        public static IObjectDatabase Database = null;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        #region CharacterInfo

        public static Dictionary<byte, CharacterInfo> CharacterInfos = new Dictionary<byte, CharacterInfo>();
        public static ConcurrentDictionary<byte, List<CharacterInfo_item>> CharacterStartingItems = new ConcurrentDictionary<byte, List<CharacterInfo_item>>();
        public static Dictionary<byte, List<CharacterInfoRenown>> RenownAbilityInfo = new Dictionary<byte, List<CharacterInfoRenown>>();
        public static Dictionary<byte, List<CharacterInfo_stats>> CharacterBaseStats = new Dictionary<byte, List<CharacterInfo_stats>>();
        public static Dictionary<byte, List<PetStatOverride>> PetOverrideStats = new Dictionary<byte, List<PetStatOverride>>();
        public static Dictionary<byte, List<PetMasteryModifiers>> PetMasteryMods = new Dictionary<byte, List<PetMasteryModifiers>>();
        public static List<Random_name> RandomNameList = new List<Random_name>();

        [LoadingFunction(true)]
        public static void LoadCharacterInfos()
        {
            IList<CharacterInfo> chars = WorldMgr.Database.SelectAllObjects<CharacterInfo>();
            foreach (CharacterInfo info in chars)
                if (!CharacterInfos.ContainsKey(info.Career))
                    CharacterInfos.Add(info.Career, info);

            RandomNameList = WorldMgr.Database.SelectAllObjects<Random_name>() as List<Random_name>;

            Log.Success("CharacterMgr", "Loaded " + chars.Count + " CharacterInfo");
        }

        [LoadingFunction(true)]
        public static void LoadDefaultCharacterItems()
        {
            IList<CharacterInfo_item> referenceListStartingItems = WorldMgr.Database.SelectAllObjects<CharacterInfo_item>();


            if (referenceListStartingItems != null)
            {
                foreach (CharacterInfo_item item in referenceListStartingItems)
                {
                    CharacterStartingItems.AddOrUpdate(
                        item.CareerLine, new List<CharacterInfo_item>{item}, 
                        (k, v) =>
                        {
                            v.Add(item);
                            return v;
                        });
                }
            }

            //if (!CharacterStartingItems.ContainsKey(info.CareerLine))
            //{
            //    List<CharacterInfo_item> items = new List<CharacterInfo_item>(1);
            //    items.Add(info);
            //    CharacterStartingItems.Add(info.CareerLine, items);
            //}
            //else CharacterStartingItems[info.CareerLine].Add(info);

            Log.Success("CharacterMgr", "Loaded " + CharacterStartingItems.Count + " CharacterInfo_Item");
        }

        [LoadingFunction(true)]
        public static void LoadCharacterRenownInfo()
        {
            IList<CharacterInfoRenown> characterRenownInfo = WorldMgr.Database.SelectAllObjects<CharacterInfoRenown>().OrderBy(x => x.ID).ToList();

            foreach (CharacterInfoRenown renInfo in characterRenownInfo)
                if (!RenownAbilityInfo.ContainsKey(renInfo.Tree))
                {
                    List<CharacterInfoRenown> items = new List<CharacterInfoRenown>(1) { renInfo };
                    RenownAbilityInfo.Add(renInfo.Tree, items);
                }
                else RenownAbilityInfo[renInfo.Tree].Add(renInfo);

            Log.Success("CharacterMgr", "Loaded " + RenownAbilityInfo.Count + " CharacterInfo_renown");
        }

        [LoadingFunction(true)]
        public static void LoadCharacterBaseStats()
        {
            IList<CharacterInfo_stats> characterStatInfo = WorldMgr.Database.SelectAllObjects<CharacterInfo_stats>();
            foreach (CharacterInfo_stats statInfo in characterStatInfo)
            {
                if (!CharacterBaseStats.ContainsKey(statInfo.CareerLine))
                {
                    List<CharacterInfo_stats> stats = new List<CharacterInfo_stats>(1) { statInfo };
                    CharacterBaseStats.Add(statInfo.CareerLine, stats);
                }
                else CharacterBaseStats[statInfo.CareerLine].Add(statInfo);
            }

            Log.Success("CharacterMgr", "Loaded " + characterStatInfo.Count + " CharacterInfo_Stats");
        }

        public static CharacterInfo GetCharacterInfo(byte career)
        {
            lock (CharacterInfos)
                if (CharacterInfos.ContainsKey(career))
                    return CharacterInfos[career];

            return null;
        }

        public static Dictionary<ushort, List<CharacterInfo_stats>> CareerLevelStats = new Dictionary<ushort, List<CharacterInfo_stats>>();

        public static List<CharacterInfo_stats> GetCharacterInfoStats(byte careerLine, byte level)
        {
            List<CharacterInfo_stats> stats = new List<CharacterInfo_stats>();
            if (!CareerLevelStats.TryGetValue((ushort)((careerLine << 8) + level), out stats))
            {
                stats = new List<CharacterInfo_stats>();

                List<CharacterInfo_stats> infoStats;
                if (CharacterBaseStats.TryGetValue(careerLine, out infoStats))
                {
                    foreach (CharacterInfo_stats stat in infoStats)
                        if (stat.CareerLine == careerLine && stat.Level == level)
                            stats.Add(stat);

                    stats = stats.OrderBy(x => x.StatId).ToList();
                }

                CareerLevelStats[(ushort)((careerLine << 8) + level)] = stats;
            }
            return stats;
        }
        public static void ReloadPetModifiers()
        {
            PetOverrideStats.Clear();
            PetMasteryMods.Clear();
            CharacterBaseStats.Clear();

            LoadPetStatOverrides();
            LoadPetMasteryMods();
            LoadCharacterBaseStats();
        }
        [LoadingFunction(true)]
        public static void LoadPetStatOverrides()
        {
            IList<PetStatOverride> overrides = WorldMgr.Database.SelectAllObjects<PetStatOverride>();
            foreach (PetStatOverride ovrInfo in overrides)
            {
                if (!PetOverrideStats.ContainsKey(ovrInfo.CareerLine))
                {
                    List<PetStatOverride> ovr = new List<PetStatOverride>(1) { ovrInfo };
                    PetOverrideStats.Add(ovrInfo.CareerLine, ovr);
                }
                else PetOverrideStats[ovrInfo.CareerLine].Add(ovrInfo);
                Log.Success("CharacterMgr", "Loaded " + overrides.Count + " PetStatOverrides");
            }
        }

        public static Dictionary<ushort, List<PetStatOverride>> PetOverriddenStats = new Dictionary<ushort, List<PetStatOverride>>();
        public static List<PetStatOverride> GetPetStatOverride(byte careerLine)
        {
            List<PetStatOverride> overrides = new List<PetStatOverride>();

            // if (!PetOverriddenStats.TryGetValue((ushort)(careerLine << 8), out overrides))
            // {
            overrides = new List<PetStatOverride>();

            List<PetStatOverride> infoOverrides;
            if (PetOverrideStats.TryGetValue(careerLine, out infoOverrides))
            {
                foreach (PetStatOverride ovr in infoOverrides)
                    if (ovr.CareerLine == careerLine && ovr.Active == true)
                        overrides.Add(ovr);

                overrides = overrides.OrderBy(x => x.PrimaryValue).ToList();
            }

            PetOverriddenStats[(ushort)(careerLine << 8)] = overrides;
            // }  

            return overrides;
        }

        [LoadingFunction(true)]
        public static void LoadPetMasteryMods()
        {
            IList<PetMasteryModifiers> modifiers = WorldMgr.Database.SelectAllObjects<PetMasteryModifiers>();
            foreach (PetMasteryModifiers modInfo in modifiers)
            {
                if (!PetMasteryMods.ContainsKey(modInfo.CareerLine))
                {
                    List<PetMasteryModifiers> mod = new List<PetMasteryModifiers>(1) { modInfo };
                    PetMasteryMods.Add(modInfo.CareerLine, mod);
                }
                else PetMasteryMods[modInfo.CareerLine].Add(modInfo);
            }
            Log.Success("CharacterMgr", "Loaded " + modifiers.Count + " PetMosteryModifiers");
        }

        public static Dictionary<ushort, List<PetMasteryModifiers>> PetModifiedMastery = new Dictionary<ushort, List<PetMasteryModifiers>>();
        public static List<PetMasteryModifiers> GetPetMasteryModifiers(byte careerLine)
        {
            List<PetMasteryModifiers> modifiers = new List<PetMasteryModifiers>();

            modifiers = new List<PetMasteryModifiers>();

            List<PetMasteryModifiers> infoModifiers;
            if (PetMasteryMods.TryGetValue(careerLine, out infoModifiers))
            {
                foreach (PetMasteryModifiers mod in infoModifiers)
                    if (mod.CareerLine == careerLine && mod.Active == true)
                        modifiers.Add(mod);

                modifiers = modifiers.OrderBy(x => x.PrimaryValue).ToList();
            }
            PetModifiedMastery[(ushort)(careerLine << 8)] = modifiers;

            return modifiers;
        }

        public static List<CharacterInfo_item> GetCharacterInfoItem(byte careerLine)
        {
            List<CharacterInfo_item> items;
            if (!CharacterStartingItems.TryGetValue(careerLine, out items))
            {
                items = new List<CharacterInfo_item>();
                CharacterStartingItems.TryAdd(careerLine, items);
            }
            return items;
        }

        public static List<Random_name> GetRandomNames()
        {
            return RandomNameList;
        }

        #endregion

        #region Characters

        // Only 20 will work
        public static byte MaxSlot = 20;
        private static long _maxCharGuid = 1;
        public static Dictionary<uint, Character> Chars = new Dictionary<uint, Character>();
        public static Dictionary<string, uint> CharIdLookup = new Dictionary<string, uint>();
        public static Dictionary<int, AccountChars> AcctChars = new Dictionary<int, AccountChars>();

        public static long RecentHistoryTime = (TCPManager.GetTimeStamp() - ((60 * 60 * 24 * 7 * 4 * 2)));

        [LoadingFunction(true)]
        public static void LoadCharacters()
        {
            if (Program.Config.PreloadAllCharacters)
            {
                List<Character> chars = (List<Character>)Database.SelectAllObjects<Character>();
                Dictionary<uint, Character_value> charValues = Database.SelectAllObjects<Character_value>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
                Dictionary<uint, CharacterClientData> charClientData = Database.SelectAllObjects<CharacterClientData>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
                Dictionary<uint, List<Character_social>> charSocials = Database.SelectAllObjects<Character_social>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<Character_tok>> charToks = Database.SelectAllObjects<Character_tok>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<Character_tok_kills>> charToksKills = Database.SelectAllObjects<Character_tok_kills>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<Character_quest>> charQuests = Database.SelectAllObjects<Character_quest>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<Characters_influence>> charInfluences = Database.SelectAllObjects<Characters_influence>().GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());
                Dictionary<uint, List<Characters_bag_pools>> charBagPools = Database.SelectAllObjects<Characters_bag_pools>().GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());
                Dictionary<uint, List<Character_mail>> charMail = Database.SelectAllObjects<Character_mail>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<CharacterSavedBuff>> charBuffs = Database.SelectAllObjects<CharacterSavedBuff>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<HonorRewardCooldown>> charHonorCooldowns = Database.SelectAllObjects<HonorRewardCooldown>().GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());

                int count = 0;
                foreach (Character Char in chars)
                {
                    if (charValues.ContainsKey(Char.CharacterId)) Char.Value = charValues[Char.CharacterId];
                    if (charClientData.ContainsKey(Char.CharacterId)) Char.ClientData = charClientData[Char.CharacterId];
                    if (charSocials.ContainsKey(Char.CharacterId)) Char.Socials = charSocials[Char.CharacterId];
                    if (charToks.ContainsKey(Char.CharacterId)) Char.Toks = charToks[Char.CharacterId];
                    if (charToksKills.ContainsKey(Char.CharacterId)) Char.TokKills = charToksKills[Char.CharacterId];
                    if (charQuests.ContainsKey(Char.CharacterId)) Char.Quests = charQuests[Char.CharacterId];
                    if (charInfluences.ContainsKey(Char.CharacterId)) Char.Influences = charInfluences[Char.CharacterId];
                    if (charBagPools.ContainsKey(Char.CharacterId)) Char.Bag_Pools = charBagPools[Char.CharacterId];
                    if (charMail.ContainsKey(Char.CharacterId)) Char.Mails = charMail[Char.CharacterId];
                    if (charBuffs.ContainsKey(Char.CharacterId)) Char.Buffs = charBuffs[Char.CharacterId];
                    if (charHonorCooldowns.ContainsKey(Char.CharacterId)) Char.HonorCooldowns = charHonorCooldowns[Char.CharacterId];

                    // Mail list must never be null
                    if (Char.Mails == null)
                        Char.Mails = new List<Character_mail>();
                    if (Char.HonorCooldowns == null)
                        Char.HonorCooldowns = new List<HonorRewardCooldown>();
                    AddChar(Char);
                    ++count;
                }

                Log.Success("LoadCharacters", count + " characters loaded.");
            }

            else
            {
                string whereString = $"CharacterId IN (SELECT CharacterId FROM {Database.GetSchemaName()}.characters t1 WHERE t1.AccountId IN (SELECT AccountId FROM {Program.AcctMgr.GetAccountSchemaName()}.accounts t2 WHERE t2.LastLogged >= {RecentHistoryTime}))";
                /*_maxCharGuid = Database.GetMaxColValue<Character>("CharacterId");

                Log.Success("LoadCharacters", _maxCharGuid + " is the max char GUID.");

                List<Character> auctionSellers = (List<Character>)Database.SelectObjects<Character>("CharacterId IN (SELECT SellerId FROM war_characters.auctions)");

                foreach (Character seller in auctionSellers)
                {
                    if (!Chars.ContainsKey(seller.CharacterId))
                        AddChar(seller);
                }*/

                // Full load
                List<Character> chars = (List<Character>)Database.SelectAllObjects<Character>();
                Dictionary<uint, List<Character_social>> charSocials = Database.SelectAllObjects<Character_social>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());

                // Partial load
                Dictionary<uint, Character_value> charValues = Database.SelectObjects<Character_value>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
                Dictionary<uint, CharacterClientData> charClientData = Database.SelectAllObjects<CharacterClientData>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
                Dictionary<uint, List<Character_tok>> charToks = Database.SelectObjects<Character_tok>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<Character_tok_kills>> charToksKills = Database.SelectObjects<Character_tok_kills>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<Character_quest>> charQuests = Database.SelectObjects<Character_quest>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<Characters_influence>> charInfluences = Database.SelectObjects<Characters_influence>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());
                Dictionary<uint, List<Characters_bag_pools>> charBagPools = Database.SelectObjects<Characters_bag_pools>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());
                Dictionary<uint, List<Character_mail>> charMail = Database.SelectObjects<Character_mail>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<CharacterSavedBuff>> charBuffs = Database.SelectObjects<CharacterSavedBuff>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<uint, List<HonorRewardCooldown>> charHonorCooldowns = Database.SelectAllObjects<HonorRewardCooldown>().GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());

                int count = 0;
                foreach (Character Char in chars)
                {
                    if (charValues.ContainsKey(Char.CharacterId)) Char.Value = charValues[Char.CharacterId];
                    if (charClientData.ContainsKey(Char.CharacterId)) Char.ClientData = charClientData[Char.CharacterId];
                    if (charSocials.ContainsKey(Char.CharacterId)) Char.Socials = charSocials[Char.CharacterId];
                    if (charToks.ContainsKey(Char.CharacterId)) Char.Toks = charToks[Char.CharacterId];
                    if (charToksKills.ContainsKey(Char.CharacterId)) Char.TokKills = charToksKills[Char.CharacterId];
                    if (charQuests.ContainsKey(Char.CharacterId)) Char.Quests = charQuests[Char.CharacterId];
                    if (charInfluences.ContainsKey(Char.CharacterId)) Char.Influences = charInfluences[Char.CharacterId];
                    if (charBagPools.ContainsKey(Char.CharacterId)) Char.Bag_Pools = charBagPools[Char.CharacterId];
                    if (charMail.ContainsKey(Char.CharacterId)) Char.Mails = charMail[Char.CharacterId];
                    if (charBuffs.ContainsKey(Char.CharacterId)) Char.Buffs = charBuffs[Char.CharacterId];
                    if (charHonorCooldowns.ContainsKey(Char.CharacterId)) Char.HonorCooldowns = charHonorCooldowns[Char.CharacterId];

                    // Mail list must never be null
                    if (Char.Mails == null)
                        Char.Mails = new List<Character_mail>();
                    if (Char.HonorCooldowns == null)
                        Char.HonorCooldowns = new List<HonorRewardCooldown>();


                    AddChar(Char);
                    if (Char.Value != null)
                        GetAccountChar(Char.AccountId).Loaded = true;

                    ++count;
                }

                Log.Success("LoadCharacters", $"{count} characters loaded, of which {charValues.Count} fully precached.");
            }

            AuctionHouse.LoadAuctions();
            LoadAlliances();
        }

        public static Character LoadCharacterInfo(string name, bool fullLoad)
        {
            Character Char = Database.SelectObject<Character>("Name='" + Database.Escape(name) + "'");
            if (Char != null)
            {
                if (Char.Value == null)
                    Char.Value = Database.SelectObject<Character_value>("CharacterId=" + Char.CharacterId);
                if (Char.ClientData == null)
                    Char.ClientData = Database.SelectObject<CharacterClientData>("CharacterId=" + Char.CharacterId);
                if (fullLoad)
                    LoadAdditionalCharacterInfo(Char);

                AddChar(Char);
                Log.Info("LoadCharacter (Name)", Char.Name);
            }

            return Char;

        }
        public static Character LoadCharacterInfo(uint id, bool fullLoad)
        {
            Character Char = Database.SelectObject<Character>("CharacterId='" + id + "'");
            if (Char != null)
            {
                if (Char.Value == null)
                    Char.Value = Database.SelectObject<Character_value>("CharacterId=" + Char.CharacterId);
                if (Char.ClientData == null)
                    Char.ClientData = Database.SelectObject<CharacterClientData>("CharacterId=" + Char.CharacterId);

                if (fullLoad)
                    LoadAdditionalCharacterInfo(Char);

                AddChar(Char);
                Log.Info("LoadCharacter (Name)", Char.Name);
            }

            return Char;
        }

        private static void LoadAdditionalCharacterInfo(Character Char)
        {
            Char.Socials = (List<Character_social>)Database.SelectObjects<Character_social>("CharacterId=" + Char.CharacterId);
            Char.Toks = (List<Character_tok>)Database.SelectObjects<Character_tok>("CharacterId=" + Char.CharacterId);
            Char.TokKills = (List<Character_tok_kills>)Database.SelectObjects<Character_tok_kills>("CharacterId=" + Char.CharacterId);
            Char.Quests = (List<Character_quest>)Database.SelectObjects<Character_quest>("CharacterId=" + Char.CharacterId);
            Char.Influences = (List<Characters_influence>)Database.SelectObjects<Characters_influence>("CharacterId=" + Char.CharacterId);
            Char.Bag_Pools = (List<Characters_bag_pools>)Database.SelectObjects<Characters_bag_pools>("CharacterId=" + Char.CharacterId);
            Char.Buffs = (List<CharacterSavedBuff>)Database.SelectObjects<CharacterSavedBuff>("CharacterId=" + Char.CharacterId);
            Char.HonorCooldowns = (List<HonorRewardCooldown>)Database.SelectObjects<HonorRewardCooldown>("CharacterId=" + Char.CharacterId);

            if (Char.Mails == null)
                Char.Mails = (List<Character_mail>)Database.SelectObjects<Character_mail>("CharacterId=" + Char.CharacterId);
        }

        public static uint GenerateMaxCharId()
        {
            return (uint)Interlocked.Increment(ref _maxCharGuid);
        }

        public static bool CreateChar(Character Char)
        {
            AccountChars chars = GetAccountChar(Char.AccountId);
            Char.SlotId = chars.GenerateFreeSlot();

            lock (Chars)
            {
                uint charId = GenerateMaxCharId();

                while (Chars.ContainsKey(charId))
                    charId = GenerateMaxCharId();

                if (charId >= uint.MaxValue || charId <= 0)
                {
                    Log.Error("CreateChar", "Failed: maximum number of characters reached.");
                    return false;
                }

                Char.CharacterId = charId;
                Chars[Char.CharacterId] = Char;
                chars.AddChar(Char);
            }

            lock (CharIdLookup)
                CharIdLookup.Add(Char.Name, Char.CharacterId);

            Database.AddObject(Char);

            return true;
        }

        public static void AddChar(Character Char)
        {
            lock (Chars)
            {
                if (Chars.ContainsKey(Char.CharacterId))
                    return;

                Chars.Add(Char.CharacterId, Char);

                GetAccountChar(Char.AccountId).AddChar(Char);

                if (Char.CharacterId > _maxCharGuid)
                    _maxCharGuid = Char.CharacterId;
            }

            lock (CharIdLookup)
                CharIdLookup.Add(Char.Name, Char.CharacterId);
        }

        public static uint GetCharacterId(string name)
        {
            lock (CharIdLookup)
                return CharIdLookup.ContainsKey(name) ? CharIdLookup[name] : 0;
        }

        public static void UpdateCharacterName(Character chara, string newName)
        {
            lock (CharIdLookup)
            {
                CharIdLookup.Remove(chara.Name);
                chara.OldName = chara.Name;
                chara.Name = newName;
                CharIdLookup.Add(chara.Name, chara.CharacterId);
                Database.SaveObject(chara);
                Database.ForceSave();
            }

        }

        public static Character GetCharacter(string name, bool fullLoad)
        {
            uint characterId = GetCharacterId(name);

            lock (Chars)
            {
                if (characterId > 0)
                    return Chars[characterId];

                foreach (Character chara in Chars.Values)
                    if (chara != null && name.Equals(chara.Name, StringComparison.OrdinalIgnoreCase))
                        return chara;
            }

            return LoadCharacterInfo(name, fullLoad);
        }

        public static Character GetCharacter(uint id, bool fullLoad)
        {
            lock (Chars)
                if (Chars.ContainsKey(id))
                    return Chars[id];

            return LoadCharacterInfo(id, fullLoad);
        }

        public static void GetCharactersWithName(string name, List<Character> inList)
        {
            uint characterId = GetCharacterId(name);

            lock (Chars)
            {
                if (characterId > 0)
                    inList.Add(Chars[characterId]);

                foreach (Character chara in Chars.Values)
                    if (!string.IsNullOrEmpty(chara?.OldName) && name.Equals(chara.OldName, StringComparison.OrdinalIgnoreCase))
                        inList.Add(chara);
            }
        }

        public static void RemoveCharacter(byte slot, GameClient client)
        {
            int accountId = client._Account.AccountId;

            uint characterId = GetAccountChar(accountId).RemoveCharacter(slot);

            lock (Chars)
                if (characterId > 0 && Chars.ContainsKey(characterId))
                {

                    CharacterDeletionRecord record = new CharacterDeletionRecord
                    {
                        DeletionIP = client.GetIp(),
                        AccountID = client._Account.AccountId,
                        AccountName = client._Account.Username,
                        CharacterID = characterId,
                        CharacterName = Chars[characterId].Name,
                        DeletionTimeSeconds = TCPManager.GetTimeStamp()

                    };

                    Database.AddObject(record);

                    Character Char = Chars[characterId];
                    Chars.Remove(characterId);
                    RemoveItemsFromCharacterId(characterId);
                    DeleteChar(Char);

                    Program.AcctMgr.UpdateRealmCharacters(Program.Rm.RealmId, (uint)Database.GetObjectCount<Character>(" Realm=1"), (uint)Database.GetObjectCount<Character>(" Realm=2"));
                }
        }

        public static void RemoveCharacter(Player deleter, int accountId, byte slot)
        {
            uint characterId = GetAccountChar(accountId).RemoveCharacter(slot);

            lock (Chars)
                if (characterId > 0 && Chars.ContainsKey(characterId))
                {

                    CharacterDeletionRecord record = new CharacterDeletionRecord
                    {
                        DeletionIP = deleter.Client.GetIp(),
                        AccountID = accountId,
                        AccountName = deleter.Client._Account.Username,
                        CharacterID = characterId,
                        CharacterName = Chars[characterId].Name,
                        DeletionTimeSeconds = TCPManager.GetTimeStamp()
                    };

                    Database.AddObject(record);

                    Character Char = Chars[characterId];
                    Chars.Remove(characterId);
                    RemoveItemsFromCharacterId(characterId);
                    DeleteChar(Char);

                    Program.AcctMgr.UpdateRealmCharacters(Program.Rm.RealmId, (uint)Database.GetObjectCount<Character>(" Realm=1"), (uint)Database.GetObjectCount<Character>(" Realm=2"));
                }
        }

        public static bool DeleteChar(Character Char)
        {
            lock (CharIdLookup)
                CharIdLookup.Remove(Char.Name);

            Database.DeleteObject(Char);
            Database.DeleteObject(Char.Value);
            Database.DeleteObject(Char.ClientData);

            if (Char.Socials != null)
                foreach (Character_social obj in Char.Socials)
                    Database.DeleteObject(obj);

            if (Char.Toks != null)
                foreach (Character_tok obj in Char.Toks)
                    Database.DeleteObject(obj);

            if (Char.Quests != null)
                foreach (Character_quest obj in Char.Quests)
                    Database.DeleteObject(obj);

            if (Char.Influences != null)
                foreach (Characters_influence obj in Char.Influences)
                    Database.DeleteObject(obj);
            if (Char.Bag_Pools != null)
                foreach (Characters_bag_pools obj in Char.Bag_Pools)
                    Database.DeleteObject(obj);
            foreach (Guild gld in Guild.Guilds)
            {
                // Shouldnt be more than 1, but might as well check
                List<Guild_member> toRemove = new List<Guild_member>();
                foreach (Guild_member member in gld.Info.Members.Values)
                    if (member.CharacterId == Char.CharacterId)
                        toRemove.Add(member);

                foreach (Guild_member member in toRemove)
                    gld.LeaveGuild(member, false);
            }

            return true;

        }

        public static AccountChars GetAccountChar(int accountId)
        {
            lock (AcctChars)
            {
                if (!AcctChars.ContainsKey(accountId))
                    AcctChars[accountId] = new AccountChars(accountId);

                return AcctChars[accountId];
            }
        }
        public static bool NameIsUsed(string name)
        {
            return Database.SelectObject<Character>("Name='" + Database.Escape(name) + "'") != null;
        }
        /// <summary>
        /// To check if the characters name is deleted and a boot has not happened yet
        /// </summary>
        public static bool NameIsDeleted(string name)
        {
            Realm Rm = Program.Rm;
            return Database.SelectObject<CharacterDeletionRecord>("CharacterName='" + Database.Escape(name) + "' AND DeletionTimeSeconds > " + Rm.BootTime) != null;
        }

        /// <summary>
        /// Precaches the characters for an account that is logging in.
        /// </summary>
        public static void LoadPendingCharacters()
        {
            AccountMgr mgr = Program.AcctMgr;

            if (mgr == null)
            {
                Log.Error("LoadPendingCharacters", "AccountMgr not available!");
                return;
            }

            List<int> accountIds = mgr.GetPendingAccounts();

            if (accountIds == null)
                return;

            StringBuilder sb = new StringBuilder($"CharacterId IN (SELECT CharacterId FROM {Database.GetSchemaName()}.characters t1 WHERE t1.AccountId IN (SELECT AccountId FROM {Program.AcctMgr.GetAccountSchemaName()}.accounts t2 WHERE t2.AccountId IN (");

            for (int i = 0; i < accountIds.Count; ++i)
            {
                AccountChars chars = GetAccountChar(accountIds[i]);
                if (chars != null && chars.Loaded)
                {
                    accountIds.RemoveAt(i);
                    --i;
                }
            }

            if (accountIds.Count == 0)
                return;

            for (int i = 0; i < accountIds.Count; ++i)
            {
                if (i > 0)
                    sb.Append(",");
                sb.Append(accountIds[i]);
            }
            sb.Append(")))");

            string whereString = sb.ToString();

            Dictionary<uint, Character_value> charValues = Database.SelectObjects<Character_value>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
            Dictionary<uint, CharacterClientData> charClientData = Database.SelectObjects<CharacterClientData>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
            Dictionary<uint, List<Character_social>> charSocials = Database.SelectAllObjects<Character_social>().GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
            Dictionary<uint, List<Character_tok>> charToks = Database.SelectObjects<Character_tok>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
            Dictionary<uint, List<Character_tok_kills>> charToksKills = Database.SelectObjects<Character_tok_kills>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
            Dictionary<uint, List<Character_quest>> charQuests = Database.SelectObjects<Character_quest>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
            Dictionary<uint, List<Characters_influence>> charInfluences = Database.SelectObjects<Characters_influence>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());
            Dictionary<uint, List<Characters_bag_pools>> charBagPools = Database.SelectObjects<Characters_bag_pools>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());
            Dictionary<uint, List<Character_mail>> charMail = Database.SelectObjects<Character_mail>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
            Dictionary<uint, List<CharacterSavedBuff>> charBuffs = Database.SelectObjects<CharacterSavedBuff>(whereString).GroupBy(v => v.CharacterId).ToDictionary(g => g.Key, g => g.ToList());
            Dictionary<uint, List<HonorRewardCooldown>> charHonorCooldowns = Database.SelectAllObjects<HonorRewardCooldown>().GroupBy(v => v.CharacterId).ToDictionary(g => (uint)g.Key, g => g.ToList());

            List<CharacterItem> charItems = (List<CharacterItem>)Database.SelectObjects<CharacterItem>(whereString);

            int count = 0;
            foreach (uint characterId in charValues.Keys)
            {
                Character chara = GetCharacter(characterId, false);

                if (charValues.ContainsKey(characterId)) chara.Value = charValues[characterId];
                if (charClientData.ContainsKey(characterId)) chara.ClientData = charClientData[characterId];
                if (charSocials.ContainsKey(characterId)) chara.Socials = charSocials[characterId];
                if (charToks.ContainsKey(characterId)) chara.Toks = charToks[characterId];
                if (charToksKills.ContainsKey(characterId)) chara.TokKills = charToksKills[characterId];
                if (charQuests.ContainsKey(characterId)) chara.Quests = charQuests[characterId];
                if (charInfluences.ContainsKey(characterId)) chara.Influences = charInfluences[characterId];
                if (charBagPools.ContainsKey(characterId)) chara.Bag_Pools = charBagPools[characterId];
                if (charMail.ContainsKey(characterId)) chara.Mails = charMail[characterId];
                if (charBuffs.ContainsKey(characterId)) chara.Buffs = charBuffs[characterId];
                if (charHonorCooldowns.ContainsKey(characterId)) chara.HonorCooldowns = charHonorCooldowns[characterId];
                // Mail list must never be null
                if (chara.Mails == null)
                    chara.Mails = new List<Character_mail>();
                if (chara.HonorCooldowns == null)
                    chara.HonorCooldowns = new List<HonorRewardCooldown>();
                ++count;
            }

            foreach (CharacterItem item in charItems)
                LoadItem(item);

            foreach (int accountid in accountIds)
                GetAccountChar(accountid).Loaded = true;

            Log.Success("LoadPendingCharacters", $"{count} characters loaded.");
        }

        // I'm not sure whether multiple threads can load characters at once, so here be semaphores
        private static readonly Semaphore CharLoadSemaphore = new Semaphore(5, 5);

        /// <summary>
        /// Returns the characters associated with the given account ID, loading them if they are not yet cached.
        /// </summary>
        public static Character[] LoadCharacters(int accountId)
        {
            AccountChars accountChars = GetAccountChar(accountId);

            if (accountChars.Loaded || Program.Config.PreloadAllCharacters)
                return accountChars.Chars;

            string whereString = "CharacterId IN (SELECT CharacterId from war_characters.characters WHERE AccountId = '" + accountId + "')";
            CharLoadSemaphore.WaitOne();

            Log.Info("LoadCharacters", "Forced to load from connection thread for account ID " + accountId);

            List<Character_tok> charactersTok = (List<Character_tok>)Database.SelectObjects<Character_tok>(whereString);
            List<Character_quest> charactersQuest = (List<Character_quest>)Database.SelectObjects<Character_quest>(whereString);
            List<Characters_influence> charactersInf = (List<Characters_influence>)Database.SelectObjects<Characters_influence>(whereString);
            List<Characters_bag_pools> charactersBgPls = (List<Characters_bag_pools>)Database.SelectObjects<Characters_bag_pools>(whereString);
            List<CharacterItem> charactersItems = (List<CharacterItem>)Database.SelectObjects<CharacterItem>(whereString);
            List<Character_tok_kills> charToksKills = (List<Character_tok_kills>)Database.SelectObjects<Character_tok_kills>(whereString);
            List<Character_mail> charMail = (List<Character_mail>)Database.SelectObjects<Character_mail>(whereString);
            List<CharacterSavedBuff> charBuffs = (List<CharacterSavedBuff>)Database.SelectObjects<CharacterSavedBuff>(whereString);

            CharLoadSemaphore.Release();

            for (int i = 0; i < accountChars.Chars.Length; ++i)
            {
                Character character = accountChars.Chars[i];
                if (character == null)
                    continue;

                if (character.Value == null)
                    character.Value = Database.SelectObject<Character_value>("CharacterId=" + character.CharacterId);
                if (character.ClientData == null)
                    character.ClientData = Database.SelectObject<CharacterClientData>("CharacterId=" + character.CharacterId);
                character.Toks = charactersTok.FindAll(tok => tok.CharacterId == character.CharacterId);
                character.TokKills = charToksKills.FindAll(tok => tok.CharacterId == character.CharacterId);
                character.Quests = charactersQuest.FindAll(quest => quest.CharacterId == character.CharacterId);
                character.Influences = charactersInf.FindAll(inf => inf.CharacterId == character.CharacterId);
                character.Bag_Pools = charactersBgPls.FindAll(bgPls => bgPls.CharacterId == character.CharacterId);
                character.Mails = charMail.FindAll(mail => mail.CharacterId == character.CharacterId);
                character.Buffs = charBuffs.FindAll(buff => buff.CharacterId == character.CharacterId);

                List<CharacterItem> charItm = charactersItems.FindAll(item => item.CharacterId == character.CharacterId);

                lock (CharItems)
                {
                    if (!CharItems.ContainsKey(character.CharacterId))
                        CharItems.Add(character.CharacterId, charItm);
                }
            }

            Log.Info("LoadCharacters", "Character loading for account ID " + accountId + " completed.");

            accountChars.Loaded = true;

            return accountChars.Chars;
        }

        /// <summary>
        /// Loads the player's characters and builds a packet describing them.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static byte[] BuildCharacters(int accountId)
        {
            Log.Debug("BuildCharacters", "AccountId = " + accountId);

            Character[] chars = GetAccountChar(accountId).Chars;

            PacketOut Out = new PacketOut(0) { Position = 0 };

            Out.WriteByte(0x00); // in packetlogs this is 0

            for (int i = 0; i < MaxSlot; ++i)
            {
                Character Char = chars[i];

                if (Char == null)
                    Out.Fill(0, 284); // 284
                else
                {
                    if (Char.Value == null)
                        throw new NullReferenceException("Character " + Char.Name + " with ID " + Char.CharacterId + " is missing its character values!");

                    List<CharacterItem> items;

                    CharItems.TryGetValue(Char.CharacterId, out items);

                    if (items == null)
                        items = new List<CharacterItem>();

                    // The first and last name strings are each up to 24 bytes in length,
                    // and need to be null-terminated in the packet, allowing for 23 characters total.
                    Out.FillString(Char.Name, 23);
                    Out.WriteByte(0);
                    Out.FillString(Char.Surname, 23);
                    Out.WriteByte(0);
                    Out.WriteByte(Char.Value.Level);
                    Out.WriteByte(Char.Career);
                    Out.WriteByte(Char.Realm);
                    Out.WriteByte(Char.Sex);
                    Out.WriteByte(Char.ModelId);
                    Out.WriteByte(0);
                    Out.WriteUInt16R(Char.Value.ZoneId);
                    Out.Fill(0, 4);

                    CharacterItem Item;
                    for (ushort slotId = 19; slotId < 37; ++slotId)
                    {
                        Item = items.Find(item => item != null && item.SlotId == slotId);
                        if (Item == null)
                        {
                            Out.WriteUInt32R(0); // ModelId
                            Out.WriteUInt16R(0); // PrimaryDye
                            Out.WriteUInt16R(0); // SecondaryDye
                        }
                        else
                        {
                            if (Item.Alternate_AppereanceEntry > 0 && ItemService.GetItem_Info(Item.Alternate_AppereanceEntry) != null)
                                Out.WriteUInt32R(ItemService.GetItem_Info(Item.Alternate_AppereanceEntry).ModelId); // ModelId
                            else
                                Out.WriteUInt32R(Item.ModelId); // ModelId

                            if (Item.PrimaryDye == 0)
                                Out.WriteUInt16R(ItemService.GetItem_Info(Item.Entry).BaseColor1); // PrimaryDye
                            else
                                Out.WriteUInt16R(Item.PrimaryDye); // PrimaryDye
                            if (Item.SecondaryDye == 0)
                                Out.WriteUInt16R(ItemService.GetItem_Info(Item.Entry).BaseColor2); // PrimaryDye
                            else
                                Out.WriteUInt16R(Item.SecondaryDye); // SecondaryDye
                        }
                    }

                    for (int j = 0; j < 4; ++j)
                    {
                        Out.Fill(0, 4);
                        Out.WriteUInt16(0xFF00);
                        Out.Fill(0, 2);
                    }

                    for (ushort slotId = 10; slotId < 13; ++slotId)
                    {
                        Item = items.Find(item => item != null && item.SlotId == slotId);
                        Out.WriteUInt32R(Item?.ModelId ?? 0); // ModelId
                    }

                    Out.WriteHexStringBytes("0000000000000000010000"); // 05 || 0000 || 0B07000A03000602 || 00 00 00 00 00 00 00 || 00 00 00 00 00 00 00
                                                                       /*Out.Fill(0, 10);
                                                                       Out.WriteUInt16(0xFF00);
                                                                       Out.WriteByte(0);
                                                                       */
                    Out.WriteByte(Char.Race);
                    Out.WriteUInt16(0); // (Char.Value.TitleId); // title canot be seen in char selection this cause crashes
                    Out.Write(Char.bTraits, 0, Char.bTraits.Length);
                    Out.Fill(0, 14);// 272*/
                }
            }
            return Out.ToArray();
        }

        public static Realms GetAccountRealm(int accountId) => GetAccountChar(accountId).Realm;

        #endregion

        #region Name filtering

        private static List<BannedNameRecord> BannedNameRecords;

        [LoadingFunction(false)]
        public static void LoadBannedNames()
        {
            BannedNameRecords = (List<BannedNameRecord>)Database.SelectAllObjects<BannedNameRecord>();
        }

        public static bool AddBannedName(string name, NameFilterType filtertype)
        {
            if (!AllowName(name))
                return false;

            lock (BannedNameRecords)
            {
                foreach (var record in BannedNameRecords)
                {
                    if (record.NameString.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                BannedNameRecord newRecord = new BannedNameRecord { NameString = name, FilterType = filtertype };
                Database.AddObject(newRecord);
                BannedNameRecords.Add(newRecord);
            }

            return true;
        }

        public static bool RemoveBannedName(string name)
        {
            lock (BannedNameRecords)
            {
                BannedNameRecord record = BannedNameRecords.Find(rec => rec.NameString == name);

                if (record == null)
                    return false;

                Database.DeleteObject(record);
                BannedNameRecords.Remove(record);
            }

            return true;
        }

        public static void ListBlockedNames(Player requester)
        {
            lock (BannedNameRecords)
            {
                requester.SendClientMessage("The following names are prohibited:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                PrintBlockedNamesMatching(requester, NameFilterType.Equals);
                requester.SendClientMessage("The following are prohibited at the beginning of a name:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                PrintBlockedNamesMatching(requester, NameFilterType.StartsWith);
                requester.SendClientMessage("The following are prohibited within a name:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                PrintBlockedNamesMatching(requester, NameFilterType.Contains);
            }
        }

        private static void PrintBlockedNamesMatching(Player requester, NameFilterType filter)
        {
            int count = 0;

            StringBuilder names = new StringBuilder(1024);

            foreach (var record in BannedNameRecords)
            {
                if (record.FilterType != filter)
                    continue;

                if (count > 0)
                    names.Append(", ");

                names.Append(record.NameString);
                ++count;

                if (count == 16)
                {
                    requester.SendClientMessage(names.ToString());
                    count = 0;
                    names.Clear();
                }
            }

            if (count > 0)
                requester.SendClientMessage(names.ToString());
        }

        public static bool AllowName(string name)
        {
            lock (BannedNameRecords)
            {
                foreach (BannedNameRecord rec in BannedNameRecords)
                {
                    switch (rec.FilterType)
                    {
                        case NameFilterType.Equals:
                            if (name.Equals(rec.NameString, StringComparison.OrdinalIgnoreCase))
                                return false;
                            break;
                        case NameFilterType.StartsWith:
                            if (name.StartsWith(rec.NameString, StringComparison.OrdinalIgnoreCase))
                                return false;
                            break;
                        case NameFilterType.Contains:
                            if (name.IndexOf(rec.NameString, StringComparison.OrdinalIgnoreCase) != -1)
                                return false;
                            break;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Guilds

        public static void LoadAlliances()
        {
            Log.Info("LoadGuildAllianes", "Loading guild Allianes...");

            if (Program.Config.PreloadAllCharacters)
            {
                List<Guild_Alliance_info> Alliances = (List<Guild_Alliance_info>)Database.SelectAllObjects<Guild_Alliance_info>();
                foreach (Guild_Alliance_info ali in Alliances)
                {
                    Alliance.Alliances.Add(ali.AllianceId, ali);

                }
            }
            Log.Success("LoadGuildAlliance", Alliance.Alliances.Count + " Alliances loaded.");
            LoadGuilds();
        }

        public static void LoadGuilds()
        {
            Log.Info("LoadGuilds", "Loading guilds...");
            List<Guild_info> guilds = (List<Guild_info>)Database.SelectAllObjects<Guild_info>();
            List<Guild_member> guildMembers = (List<Guild_member>)Database.SelectAllObjects<Guild_member>();
            List<Guild_rank> guildRanks = (List<Guild_rank>)Database.SelectAllObjects<Guild_rank>();
            List<Guild_log> guildLogs = (List<Guild_log>)Database.SelectAllObjects<Guild_log>();
            List<Guild_event> guildEvents = (List<Guild_event>)Database.SelectAllObjects<Guild_event>();
            List<GuildVaultItem> guildVault = (List<GuildVaultItem>)Database.SelectAllObjects<GuildVaultItem>();


            if (Program.Config.PreloadAllCharacters)
            {
                List<Guild_member> toRemove = new List<Guild_member>();

                foreach (Guild_member gldMem in guildMembers)
                {
                    if (Chars.ContainsKey(gldMem.CharacterId))
                        gldMem.Member = Chars[gldMem.CharacterId];
                    else
                        toRemove.Add(gldMem);
                }

                if (toRemove.Count > 0)
                    foreach (Guild_member mem in toRemove)
                    {
                        Database.DeleteObject(mem);
                        guildMembers.Remove(mem);
                    }

                foreach (Guild_info guild in guilds)
                {
                    if (guild.AllianceId > 0)
                    {
                        if (Alliance.Alliances.ContainsKey(guild.AllianceId))
                            Alliance.Alliances[guild.AllianceId].Members.Add(guild.GuildId);
                        else
                        {
                            Log.Error("LoadGuilds", guild.Name + " has an invalid allianceID");
                            guild.AllianceId = 0;
                            Database.SaveObject(guild);
                        }
                    }

                    try
                    {
                        guild.Members = guildMembers.FindAll(info => info.GuildId == guild.GuildId).ToDictionary(x => x.CharacterId, x => x);
                        guild.Ranks = guildRanks.FindAll(info => info.GuildId == guild.GuildId).OrderBy(info => info.RankId).ToDictionary(x => x.RankId, x => x);
                        guild.Logs = guildLogs.FindAll(info => info.GuildId == guild.GuildId).OrderBy(info => info.Time).ThenByDescending(info => info.Type).ToList();
                        guild.Event = guildEvents.FindAll(info => info.GuildId == guild.GuildId).OrderBy(info => info.SlotId).ToDictionary(x => x.SlotId, x => x);
                    }

                    catch (Exception)
                    {
                        Log.Error("LoadGuilds", "Failed load of guild: " + guild.Name);
                    }

                    guild.Vaults[0] = guildVault.FindAll(info => info.GuildId == guild.GuildId && info.VaultId == 1).OrderBy(info => info.SlotId).ToDictionary(x => x.SlotId, x => x);
                    guild.Vaults[1] = guildVault.FindAll(info => info.GuildId == guild.GuildId && info.VaultId == 2).OrderBy(info => info.SlotId).ToDictionary(x => x.SlotId, x => x);
                    guild.Vaults[2] = guildVault.FindAll(info => info.GuildId == guild.GuildId && info.VaultId == 3).OrderBy(info => info.SlotId).ToDictionary(x => x.SlotId, x => x);
                    guild.Vaults[3] = guildVault.FindAll(info => info.GuildId == guild.GuildId && info.VaultId == 4).OrderBy(info => info.SlotId).ToDictionary(x => x.SlotId, x => x);
                    guild.Vaults[4] = guildVault.FindAll(info => info.GuildId == guild.GuildId && info.VaultId == 5).OrderBy(info => info.SlotId).ToDictionary(x => x.SlotId, x => x);

                    Guild.Guilds.Add(new Guild(guild));

                    List<Guild_member> members = guild.Members.Values.OrderByDescending(x => x.RankId).ToList();

                    //checks if theres more then 1 guildmember with guild rank of 9 (leader)

                    int rank9count = 0;

                    for (int x = 0; x < members.Count; x++)
                    {
                        if (members[x].RankId == 9)
                            rank9count++;
                        else
                            break;
                    }

                    if (rank9count > 1)
                    {
                        Log.Info("LoadGuilds", guild.Name + "More then 1 guildleader found removeing all rank 9 players");
                        for (int x = 0; x < members.Count; x++)
                        {
                            if (members[x].RankId == 9)
                            {
                                members[x].RankId = 8;
                                Database.SaveObject(members[x]);
                            }
                            else
                                break;
                        }


                        if (guild.Members.ContainsKey(guild.LeaderId))
                        {
                            Guild_member mem;
                            guild.Members.TryGetValue(guild.LeaderId, out mem);
                            mem.RankId = 9;
                            Database.SaveObject(mem);
                        }
                    }





                    //checks for guild leader id player not found guildleader banned or guild leader inactive is so tryes to set a new guild leader if no guildleader can be found guild is set to inactive
                    Account accountEntity = null;
                    var characterEntity = CharMgr.GetCharacter(guild.LeaderId, true);
                    if (characterEntity != null)
                        accountEntity= Program.AcctMgr.GetAccountById(characterEntity.AccountId);

                    if ((characterEntity != null) && (accountEntity != null))
                    {

                        if (!guild.Members.ContainsKey(guild.LeaderId)
                            || guild.Members[guild.LeaderId].RankId != 9
                            || accountEntity.Banned == 1
                            || accountEntity.GmLevel == -1
                            || CharMgr.GetCharacter(guild.LeaderId, true).Value.LastSeen + 2246400 <
                            TCPManager.GetTimeStamp())
                        {


                            bool newleaderfound = false;

                            for (int i = 0; i < members.Count; i++)
                            {
                                if (CharMgr.GetCharacter(members[i].CharacterId, true).Value.LastSeen + 2246400 >
                                    TCPManager.GetTimeStamp())
                                {
                                    newleaderfound = true;
                                    guild.LeaderId = members[i].CharacterId;
                                    members[i].RankId = 9;
                                    CharMgr.Database.SaveObject(members[i]);
                                    Database.SaveObject(guild);
                                    Log.Info("LoadGuilds",
                                        guild.Name +
                                        " Leader not found banned or not loged in for 26 days, set to new leader " +
                                        members[i].CharacterId);
                                    break;
                                }
                            }

                            if (!newleaderfound)
                                Guild.GetGuild(guild.Name).Inactive = true;
                        }
                    }
                    else
                    {
                        Log.Error("Missing Guild Leader", $"Guild Leader {guild.LeaderId} is missing in the the DB");
                    }

                    if (guild.GuildId > Guild.MaxGuildGUID)
                        Guild.MaxGuildGUID = (int)guild.GuildId;
                }
            }
            else
            {
                foreach (Guild_info gld in guilds)
                {
                    //Log.Success("LoadGuilds", "Loading guild " + gld.Name);

                    List<Character> guildCharacters = (List<Character>)Database.SelectObjects<Character>("CharacterId IN (SELECT CharacterId FROM war_characters.guild_members WHERE GuildId = " + gld.GuildId + ")");

                    foreach (Character gChar in guildCharacters)
                    {
                        if (!Chars.ContainsKey(gChar.CharacterId))
                        {
                            AddChar(gChar);
                            gChar.Value = Database.SelectObject<Character_value>("CharacterId=" + gChar.CharacterId);
                        }
                    }

                    gld.Members = guildMembers.FindAll(info => info.GuildId == gld.GuildId).ToDictionary(x => x.CharacterId, x => x);

                    foreach (Guild_member m in gld.Members.Values)
                        m.Member = GetCharacter(m.CharacterId, false);

                    gld.Ranks = guildRanks.FindAll(info => info.GuildId == gld.GuildId).OrderBy(info => info.RankId).ToDictionary(x => x.RankId, x => x);
                    gld.Logs = guildLogs.FindAll(info => info.GuildId == gld.GuildId).OrderBy(info => info.Time).ThenByDescending(info => info.Type).ToList();
                    Guild.Guilds.Add(new Guild(gld));

                    if (gld.GuildId > Guild.MaxGuildGUID)
                        Guild.MaxGuildGUID = (int)gld.GuildId;
                }
            }

        }

        public static bool ChangeGuildName(Guild_info guild, string newName)
        {
            guild.Name = newName;
            Database.SaveObject(guild);
            Database.ForceSave();
            return true;
        }

        public static bool DeleteGuild(Guild_info guild)
        {
            Database.DeleteObject(guild);

            if (guild.Members != null)
                foreach (Guild_member obj in guild.Members.Values)
                    Database.DeleteObject(obj);

            if (guild.Ranks != null)
                foreach (Guild_rank obj in guild.Ranks.Values)
                    Database.DeleteObject(obj);

            if (guild.Logs != null)
                foreach (Guild_log obj in guild.Logs)
                    Database.DeleteObject(obj);

            return true;
        }
        #endregion

        #region CharacterItems

        public static Dictionary<uint, List<CharacterItem>> CharItems = new Dictionary<uint, List<CharacterItem>>();

        [LoadingFunction(true)]
        public static void LoadItems()
        {
            long myCount;

            Log.Info("LoadItems", "Loading items...");
            lock (CharItems)
            {
                CharItems.Clear();
            }

            IList<CharacterItem> charItems;

            if (Program.Config.PreloadAllCharacters)
                charItems = Database.SelectAllObjects<CharacterItem>();
            else
            {
                string whereString = $"CharacterId IN (SELECT CharacterId FROM {Database.GetSchemaName()}.characters t1 WHERE t1.AccountId IN (SELECT AccountId FROM {Program.AcctMgr.GetAccountSchemaName()}.accounts t2 WHERE t2.LastLogged >= {RecentHistoryTime}))";
                charItems = Database.SelectObjects<CharacterItem>(whereString);
            }

            myCount = charItems.Count;

            lock (CharItems)
                foreach (CharacterItem itm in charItems)
                    LoadItem(itm);


            Log.Success("LoadItems", $"{myCount} inventory items {(Program.Config.PreloadAllCharacters ? "loaded" : "precached")}.");
        }

        public static void CreateItem(CharacterItem item)
        {
            LoadItem(item);
            Database.AddObject(item);
            Database.ForceSave();

        }
        public static void LoadItem(CharacterItem charItem)
        {
            lock (CharItems)
            {
                if (!CharItems.ContainsKey(charItem.CharacterId))
                    CharItems.Add(charItem.CharacterId, new List<CharacterItem> { charItem });
                else
                    CharItems[charItem.CharacterId].Add(charItem);
            }
        }

        public static List<CharacterItem> GetItemsForCharacter(Character chara)
        {
            try
            {
                _logger.Debug($"GetItemsForCharacter ==> {chara.Name}");
                lock (CharItems)
                {
                    if (CharItems.ContainsKey(chara.CharacterId))
                        return CharItems[chara.CharacterId];
                }

                Log.Info("GetItemsForChar", "Loading items for CharacterId: " + chara.CharacterId);
                _logger.Debug($"Loading items for CharacterId ==> {chara.Name}");

                List<CharacterItem> myItems = (List<CharacterItem>)Database.SelectObjects<CharacterItem>("CharacterId='" + chara.CharacterId + "'");

                if (myItems != null && myItems.Count > 0)
                {
                    lock (CharItems)
                    {
                        if (!CharItems.ContainsKey(chara.CharacterId))
                        {
                            _logger.Debug($"Adding items for CharacterId ==> {chara.Name}");
                            CharItems.Add(chara.CharacterId, myItems);
                        }
                        _logger.Debug($"Returning items for CharacterId ==> {chara.Name}");
                        return CharItems[chara.CharacterId];
                    }
                }
                _logger.Debug($"Getting CharacterInfoItem for {chara.Name} career {chara.CareerLine}");
                List<CharacterInfo_item> Items = GetCharacterInfoItem(chara.CareerLine);
                _logger.Debug($"Found CharacterInfoItem Count={Items.Count} for {chara.Name}");
                foreach (CharacterInfo_item Itm in Items)
                {
                    if (Itm == null)
                        continue;

                    _logger.Debug($"Adding item {Itm.Entry} to character {chara.Name}");

                    CharacterItem Citm = new CharacterItem
                    {
                        Counts = Itm.Count,
                        CharacterId = chara.CharacterId,
                        Entry = Itm.Entry,
                        ModelId = Itm.ModelId,
                        SlotId = Itm.SlotId,
                        PrimaryDye = 0,
                        SecondaryDye = 0
                    };
                    CreateItem(Citm);
                }

                lock (CharItems)
                {
                    if (CharItems.ContainsKey(chara.CharacterId))
                        return CharItems[chara.CharacterId];
                    else
                    {
                        _logger.Warn($"Returning empty char item list for character {chara.Name}");
                        return new List<CharacterItem>();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Debug($"Exception creating items for character {e.Message} {e.StackTrace}");
                throw;
            }

        }

        public static void SaveItems(uint characterId, List<Item> oldItems)
        {
            List<CharacterItem> newItems = new List<CharacterItem>();
            for (int i = 0; i < oldItems.Count; ++i)
                if (oldItems[i] != null)
                    newItems.Add(oldItems[i].Save(characterId));

            lock (CharItems)
            {
                CharItems.Remove(characterId);
                CharItems.Add(characterId, newItems);
            }
        }

        public static void DeleteItem(CharacterItem itm)
        {
            lock (CharItems)
            {
                if (CharItems.ContainsKey(itm.CharacterId))
                    CharItems[itm.CharacterId].Remove(itm);
            }

            Database.DeleteObject(itm);
        }

        public static void RemoveItemsFromCharacterId(uint characterId, bool excludeBook = false)
        {
            lock (CharItems)
            {
                CharacterItem book = null;

                if (!CharItems.ContainsKey(characterId))
                    return;

                foreach (CharacterItem item in CharItems[characterId])
                {
                    if (item.Entry == 11919 && excludeBook)
                    {
                        book = item;
                        continue;
                    }
                    Database.DeleteObject(item);

                }

                if (book == null)
                    CharItems.Remove(characterId);

                else
                {
                    CharItems[characterId].Clear();
                    CharItems[characterId].Add(book);
                }
            }
        }

        #endregion

        #region CharacterMail
        private static int _maxMailGuid = 1;

        public static int GenerateMailGuid()
        {
            return Interlocked.Increment(ref _maxMailGuid);
        }

        [LoadingFunction(true)]
        public static void LoadMailCount()
        {
            if (Program.Config.PreloadAllCharacters)
            {
                Log.Debug("WorldMgr", "Loading Character_mails...");

                List<Character_mail> mails = (List<Character_mail>)Database.SelectAllObjects<Character_mail>();
                int count = 0;
                if (mails != null)
                {
                    List<Character_mail> expired = mails.FindAll(mail => MailInterface.TimeToExpire(mail) <= 0);

                    if (expired.Count > 0)
                    {
                        foreach (var mail in expired)
                        {
                            Database.DeleteObject(mail);
                            mails.Remove(mail);
                        }

                        Log.Success("LoadMails", "Removed " + expired.Count + " expired mails.");
                    }

                    foreach (Character_mail mail in mails)
                    {
                        if (mail.Guid > _maxMailGuid)
                            _maxMailGuid = mail.Guid;
                        count++;
                    }
                }
                Log.Success("LoadMails", "Loaded " + count + " items of mail.");
            }

            else
            {
                _maxMailGuid = Database.GetObjectCount<Character_mail>();
                Log.Success("LoadMails", _maxMailGuid + " existing mails.");
            }
        }

        public static void AddMail(Character_mail mail)
        {
            Character character = GetCharacter(mail.CharacterId, false);

            if (character == null)
                return;

            if (character.Mails == null)
            {
                _logger.Info("Mail System loading mail for " + character.Name);
                character.Mails = (List<Character_mail>)Database.SelectObjects<Character_mail>("CharacterId='" + mail.CharacterId + "'");
            }

            character.Mails.Add(mail);
            Database.AddObject(mail);

            _logger.Info($"Mail System mail count = {character.Mails.Count}");
            Player receiver = Player.GetPlayer(mail.ReceiverName);
            _logger.Debug($"Mail Receiver : {mail.ReceiverName}");

            receiver?.MlInterface?.AddMail(mail);
        }

        public static void DeleteMail(Character_mail mail)
        {
            Chars[mail.CharacterId].Mails.Remove(mail);
            Database.DeleteObject(mail);

            Player receiver = Player.GetPlayer(mail.ReceiverName);
            receiver?.MlInterface?.RemoveMail(mail);
        }

        public static void RemoveMailFromCharacter(Character chara)
        {
            if (chara.Mails == null)
                return;

            foreach (Character_mail mail in chara.Mails)
                Database.DeleteObject(mail);

            chara.Mails.Clear();
        }
        #endregion

        #region Support Tickets

        public static List<Bug_report> _report = new List<Bug_report>();

        [LoadingFunction(true)]
        public static void LoadTickets()
        {
            List<Bug_report> reports = (List<Bug_report>)Database.SelectAllObjects<Bug_report>();

            foreach (Bug_report report in reports)
                _report.Add(report);

            Log.Success("CharacterMgr", "Loaded " + _report.Count + " Support Tickets");
        }

        public static Bug_report GetReport(string reportID)
        {
            var ticket = _report.Find(x => x.ObjectId == reportID);

            if (ticket == null)
                return null;

            return ticket;
        }

        #endregion

        public static void RemoveQuestsFromCharacter(Character chara)
        {
            if (chara.Quests == null)
                return;
            foreach (Character_quest quest in chara.Quests)
                Database.DeleteObject(quest);

            chara.Quests.Clear();
        }

        public static void RemoveToKsFromCharacter(Character chara)
        {
            if (chara.Toks == null)
                return;
            foreach (Character_tok tok in chara.Toks)
                Database.DeleteObject(tok);

            chara.Toks.Clear();
        }

        public static void RemoveToKKillsFromCharacter(Character chara)
        {
            if (chara.TokKills == null)
                return;

            foreach (Character_tok_kills tokKill in chara.TokKills)
                Database.DeleteObject(tokKill);

            chara.TokKills.Clear();
        }
    }
}
