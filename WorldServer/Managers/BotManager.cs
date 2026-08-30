using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.NetWork;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.Services.World;
using GameData;

namespace WorldServer.Managers
{
    public class BotFaction
    {
        public string Name;
        public byte[] Careers; // Healer, Ranged, Tank, Melee
    }

    public class BotManager
    {
        private struct BotAppearance
        {
            public byte Sex;
            public byte ModelId;
            public byte[] Traits;
        }

        private static BotManager _instance;
        public static BotManager Instance => _instance ??= new BotManager();

        private Account _botAccount;
        private const string BOT_ACCOUNT_NAME = "BotAccount";
        private const int BOT_ACCOUNT_ID = 9999;
        private static readonly string[] BotGroupSuffixes = { "_H", "_R", "_MT", "_OT", "_M1", "_M2" };

        private static readonly BotFaction[] OrderFactions = new BotFaction[]
        {
            new BotFaction { Name = "Empire", Careers = new byte[] { 12, 11, 10, 9 } },
            new BotFaction { Name = "Dwarfs", Careers = new byte[] { 3, 4, 1, 2 } },
            new BotFaction { Name = "High Elves", Careers = new byte[] { 20, 18, 17, 19 } }
        };

        private static readonly BotFaction[] DestroFactions = new BotFaction[]
        {
            new BotFaction { Name = "Chaos", Careers = new byte[] { 15, 16, 13, 14 } },
            new BotFaction { Name = "Greenskins", Careers = new byte[] { 7, 8, 5, 6 } },
            new BotFaction { Name = "Dark Elves", Careers = new byte[] { 23, 24, 21, 22 } }
        };

        public void Initialize()
        {
            BotLoadoutManager.Initialize();
            _botAccount = Program.AcctMgr.GetAccount(BOT_ACCOUNT_NAME);
            if (_botAccount == null)
            {
                Log.Info("BotManager", $"Creating bot account: {BOT_ACCOUNT_NAME}");
                if (Program.AcctMgr.CreateAccount(BOT_ACCOUNT_NAME, "botpassword", 1))
                {
                    _botAccount = Program.AcctMgr.GetAccount(BOT_ACCOUNT_NAME);
                    if (_botAccount != null)
                        Log.Info("BotManager", "Created BotAccount.");
                    else
                        Log.Error("BotManager", "Created BotAccount but failed to load it!");
                }
                else
                {
                    Log.Error("BotManager", "Failed to create BotAccount!");
                }
            }

            // Lock down the bot account so no player can log in with it.
            // CreateAccount clamps GmLevel to a minimum of 1, so we must set
            // the system-account sentinel (-2) explicitly after creation.
            if (_botAccount != null && _botAccount.GmLevel != Common.AccountMgr.SYSTEM_ACCOUNT_GMLEVEL)
            {
                _botAccount.GmLevel = Common.AccountMgr.SYSTEM_ACCOUNT_GMLEVEL;
                Program.AcctMgr.UpdateAccount(_botAccount);
                Log.Info("BotManager", "BotAccount locked against player login (GmLevel set to system sentinel).");
            }

            RepairPersistedBotAppearances();
        }

        public IReadOnlyList<Character> GetAllBotCharacters()
        {
            if (_botAccount == null)
                return new List<Character>();

            if (Program.Config != null && !Program.Config.PreloadAllCharacters)
            {
                return (CharMgr.Database.SelectObjects<Character>($"AccountId={_botAccount.AccountId} AND Name LIKE 'Bot_%'") ?? new List<Character>())
                    .Where(character => character != null)
                    .OrderBy(character => character.Name)
                    .ToList();
            }

            lock (CharMgr.Chars)
            {
                return CharMgr.Chars.Values
                    .Where(character => character != null
                        && character.AccountId == _botAccount.AccountId
                        && character.Name.StartsWith("Bot_", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(character => character.Name)
                    .ToList();
            }
        }

        public IReadOnlyList<Player> GetLoadedBots()
        {
            return Player.GetPlayersSnapshot()
                .Where(player => player != null && player.IsBot && !player.IsDisposed && !player.PendingDisposal)
                .OrderBy(player => player.Name)
                .ToList();
        }

        public Player GetLoadedBot(uint characterId)
        {
            return GetLoadedBots().FirstOrDefault(player => player.CharacterId == characterId);
        }

        public bool TryReapplyBotLoadout(uint characterId)
        {
            Player bot = GetLoadedBot(characterId);
            if (bot?.Info?.Value == null)
                return false;

            ResolveLoadoutDyes(bot.Name, out ushort primaryDye, out ushort secondaryDye);
            ApplyLoadout(bot, GetTierFromLevel(bot.Info.Value.Level), bot.Info.Value.RenownRank, primaryDye, secondaryDye);
            return true;
        }

        public static bool TryGetRoleFromBotName(string name, out BotRole role)
        {
            role = BotRole.MeleeDPS;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.EndsWith("_H", StringComparison.OrdinalIgnoreCase))
            {
                role = BotRole.Healer;
                return true;
            }

            if (name.EndsWith("_R", StringComparison.OrdinalIgnoreCase))
            {
                role = BotRole.RangedDPS;
                return true;
            }

            if (name.EndsWith("_MT", StringComparison.OrdinalIgnoreCase))
            {
                role = BotRole.MainTank_Shield;
                return true;
            }

            if (name.EndsWith("_OT", StringComparison.OrdinalIgnoreCase))
            {
                role = BotRole.OffTank_2H;
                return true;
            }

            if (name.EndsWith("_M1", StringComparison.OrdinalIgnoreCase) || name.EndsWith("_M2", StringComparison.OrdinalIgnoreCase))
            {
                role = BotRole.MeleeDPS;
                return true;
            }

            return false;
        }

        public static string GetGroupPrefixFromBotName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            foreach (string suffix in BotGroupSuffixes)
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return name.Substring(0, name.Length - suffix.Length);
            }

            return name;
        }

        public Player CreateOrLoadBot(string name, byte career, byte level, byte renownRank, Realms realm, BotRole role, byte? profileVariantIndex = null)
        {
            if (_botAccount == null)
            {
                Log.Error("BotManager", "Cannot create bot: BotAccount is null.");
                return null;
            }

            if (!TryGetBotTemplate(career, out CharacterInfo template, out CharacterIdentityRecord identity))
                return null;

            if (!TryResolveBotAppearance(career, out BotAppearance appearance))
                return null;

            Character character = CharMgr.GetCharacter(name);
            bool saveCharacter = false;
            bool saveValue = false;

            if (character == null)
            {
                character = new Character
                {
                    Anonymous = false,
                    Name = name,
                    Surname = string.Empty,
                    OldName = string.Empty,
                    PetName = string.Empty,
                    AccountId = _botAccount.AccountId,
                    RealmId = Program.Rm?.RealmId ?? 1,
                    Career = template.Career,
                    CareerLine = career,
                    Realm = identity.Realm,
                    Race = identity.Race,
                    ModelId = appearance.ModelId,
                    Sex = appearance.Sex,
                    Hidden = false,
                    PetModel = 0,
                    bTraits = appearance.Traits.ToArray(),
                    FirstConnect = false
                };

                if (!CharMgr.CreateChar(character, true))
                {
                    Log.Error("BotManager", $"Failed to create bot character {name}");
                    return null;
                }

                character.Value = CreateBotCharacterValue(character, template, level, renownRank, realm);
                CharMgr.Database.AddObject(character.Value);
                character.ClientData = new CharacterClientData { CharacterId = character.CharacterId };
                CharMgr.Database.AddObject(character.ClientData);
            }
            else
            {
                if (character.Value == null)
                    character.Value = CharMgr.Database.SelectObject<Character_value>("CharacterId=" + character.CharacterId);

                if (character.ClientData == null)
                    character.ClientData = CharMgr.Database.SelectObject<CharacterClientData>("CharacterId=" + character.CharacterId);

                saveCharacter = RepairBotCharacter(character, template, identity, appearance);

                if (character.Value == null)
                {
                    character.Value = CreateBotCharacterValue(character, template, level, renownRank, realm);
                    CharMgr.Database.AddObject(character.Value);
                }
                else
                    saveValue = RepairBotCharacterValue(character, template, level, renownRank, realm);

                if (character.ClientData == null)
                {
                    character.ClientData = new CharacterClientData { CharacterId = character.CharacterId };
                    CharMgr.Database.AddObject(character.ClientData);
                }
            }

            if (EnsureBotCollections(character))
                saveCharacter = true;

            if (saveCharacter)
                CharMgr.Database.SaveObject(character);

            if (saveValue)
                CharMgr.Database.SaveObject(character.Value);


            if (profileVariantIndex.HasValue)
                BotTemplateProfileService.SetVariantIndex(character.CharacterId, profileVariantIndex.Value);

            Player existingPlayer = Player.GetPlayersSnapshot()
                .FirstOrDefault(player => player.IsBot && player.Info != null && player.Info.CharacterId == character.CharacterId);

            if (existingPlayer != null)
            {
                existingPlayer.IsBot = true;
                existingPlayer.Role = role;
                EnsureBotPvpEnabled(existingPlayer);
                existingPlayer.Info.FirstConnect = false;
                if (existingPlayer.MaxActionPoints < 250)
                    existingPlayer.MaxActionPoints = 250;
                if (existingPlayer.ActionPoints == 0)
                    existingPlayer.ActionPoints = existingPlayer.MaxActionPoints;

                return existingPlayer;
            }

            BotClient client = new BotClient(Program.Server);
            client._Account = _botAccount;

            Player player = Player.CreatePlayer(client, character);
            if (player == null)
            {
                Log.Error("BotManager", $"Failed to create bot player object for {name}");
                return null;
            }

            player.IsBot = true;
            player.Role = role;
            EnsureBotPvpEnabled(player);
            player.Info.FirstConnect = false;
            if (player.MaxActionPoints < 250)
                player.MaxActionPoints = 250;
            if (player.ActionPoints == 0)
                player.ActionPoints = player.MaxActionPoints;

            return player;
        }

        private static Point3D ResolveSpawnPoint(ushort zoneId, Realms realm, Zone_Info zoneInfo)
        {
            // 1. Warcamp entrance (preferred).
            Point3D warcampPin = BattleFrontService.GetWarcampEntrance(zoneId, realm);
            if (warcampPin != null)
                return ZoneService.GetWorldPosition(zoneInfo, (ushort)warcampPin.X, (ushort)warcampPin.Y, (ushort)warcampPin.Z);

            // 2. Zone respawn point for this realm (or any realm as fallback).
            Zone_Respawn respawn = ZoneService.GetZoneRespawn(zoneId, (byte)realm);
            if (respawn != null)
                return ZoneService.GetWorldPosition(zoneInfo, respawn.PinX, respawn.PinY, respawn.PinZ);

            // 3. Zone grid center.
            Log.Info("BotManager", $"No spawn anchor found for zone {zoneId} realm {realm}; using zone center.");
            return new Point3D((zoneInfo.OffX + 4) << 12, (zoneInfo.OffY + 4) << 12, 0);
        }

        private void RepairPersistedBotAppearances()
        {
            if (_botAccount == null)
                return;

            IList<Character> botCharacters = CharMgr.Database.SelectObjects<Character>($"AccountId={_botAccount.AccountId}");
            if (botCharacters == null || botCharacters.Count == 0)
                return;

            int repairedCount = 0;

            foreach (Character character in botCharacters)
            {
                if (character == null || !character.Name.StartsWith("Bot_", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!TryGetBotTemplate(character.CareerLine, out CharacterInfo template, out CharacterIdentityRecord identity))
                    continue;

                if (!TryResolveBotAppearance(character.CareerLine, out BotAppearance appearance))
                    continue;

                if (!RepairBotCharacter(character, template, identity, appearance))
                    continue;

                CharMgr.Database.SaveObject(character);
                ++repairedCount;
            }

            if (repairedCount == 0)
                return;

            CharMgr.Database.ForceSave();
            Log.Info("BotManager", $"Queued persisted appearance repairs for {repairedCount} bot characters.");
        }

        public Group SpawnBotGroup(Realms realm, int tier, int rr, string groupPrefix, ushort zoneId, Point3D spawnOverride = null)
        {
            Zone_Info zoneInfo = ZoneService.GetZone_Info(zoneId);
            if (zoneInfo == null)
            {
                Log.Error("BotManager", $"No zone info found for zone {zoneId}");
                return null;
            }

            var region = WorldMgr.GetRegion(zoneInfo.Region, true);
            if (region == null)
            {
                Log.Error("BotManager", $"No region found for zone {zoneId}");
                return null;
            }

            Group existingGroup = GetLoadedBotGroup(groupPrefix);
            if (existingGroup != null)
            {
                Log.Info("BotManager", $"Bot group '{groupPrefix}' is already active. Refusing to spawn duplicate live bots.");
                return existingGroup;
            }

            if (HasAnyLoadedBotsWithPrefix(groupPrefix))
            {
                Log.Error("BotManager", $"Live bot prefix '{groupPrefix}' is already partially present. Refusing to spawn duplicates until the existing bots are cleaned up.");
                return null;
            }

            Point3D worldSpawn = spawnOverride ?? ResolveSpawnPoint(zoneId, realm, zoneInfo);
            BotFaction faction = ResolveDeterministicFaction(realm, tier);

            List<Player> bots = new List<Player>();
            byte[] careers = faction.Careers;

            byte healerCareer = careers[0];
            byte rangedCareer = careers[1];
            byte tankCareer = careers[2];
            byte meleeCareer = careers[3];

            // 1. Healer
            bots.Add(CreateOrLoadBot($"{groupPrefix}_H", healerCareer, GetMaxLevel(tier), (byte)rr, realm, BotRole.Healer, 0));
            // 2. Ranged DPS
            bots.Add(CreateOrLoadBot($"{groupPrefix}_R", rangedCareer, GetMaxLevel(tier), (byte)rr, realm, BotRole.RangedDPS, 0));
            // 3. Main Tank (Shield)
            bots.Add(CreateOrLoadBot($"{groupPrefix}_MT", tankCareer, GetMaxLevel(tier), (byte)rr, realm, BotRole.MainTank_Shield, 0));
            // 4. Off Tank (2H)
            bots.Add(CreateOrLoadBot($"{groupPrefix}_OT", tankCareer, GetMaxLevel(tier), (byte)rr, realm, BotRole.OffTank_2H, 1));
            // 5. Melee 1
            bots.Add(CreateOrLoadBot($"{groupPrefix}_M1", meleeCareer, GetMaxLevel(tier), (byte)rr, realm, BotRole.MeleeDPS, 0));
            // 6. Melee 2
            bots.Add(CreateOrLoadBot($"{groupPrefix}_M2", meleeCareer, GetMaxLevel(tier), (byte)rr, realm, BotRole.MeleeDPS, 1));

            if (bots.Any(bot => bot == null))
            {
                Log.Error("BotManager", $"Failed to create a full bot group for {groupPrefix}");
                return null;
            }

            Group group = new Group();
            Player leader = bots.FirstOrDefault(b => b != null);
            if (leader == null) return null;

            group.InitializeSolo(leader);
            foreach (var bot in bots.Skip(1))
            {
                if (bot != null)
                    group.AddMember(bot);
            }

            Player mt = bots[2];
            if (mt != null)
                group.SetMainAssist(mt);

            AssignToGuild(bots, faction.Name);

            ResolveLoadoutDyes(groupPrefix, out ushort primaryDye, out ushort secondaryDye);

            int botIndex = 0;
            foreach (var bot in bots)
            {
                double angle = botIndex * (Math.PI * 2 / bots.Count);
                int offsetDistance = 150; // 150 units offset
                int xOffset = (int)(Math.Cos(angle) * offsetDistance);
                int yOffset = (int)(Math.Sin(angle) * offsetDistance);
                
                Point3D spreadSpawn = new Point3D(worldSpawn.X + xOffset, worldSpawn.Y + yOffset, worldSpawn.Z);

                StageBotSpawn(bot, zoneInfo, spreadSpawn);
                ApplyLoadout(bot, tier, rr, primaryDye, secondaryDye);
                region.AddObject(bot, zoneId);
                botIndex++;
            }

            CharMgr.Database.ForceSave();

            return group;
        }

        public void TeleportBotGroup(Group group, ushort zoneId, Point3D spawnOverride = null)
        {
            if (group == null || group.Members == null)
                return;

            Zone_Info zoneInfo = ZoneService.GetZone_Info(zoneId);
            if (zoneInfo == null)
                return;
                
            var region = WorldMgr.GetRegion(zoneInfo.Region, true);
            if (region == null)
                return;

            // We assume Realm from the leader
            Player leader = group.Members.FirstOrDefault();
            if (leader == null) return;

            Point3D worldSpawn = spawnOverride ?? ResolveSpawnPoint(zoneId, leader.Realm, zoneInfo);

            int botIndex = 0;
            foreach (var bot in group.Members)
            {
                if (bot == null) continue;

                double angle = botIndex * (Math.PI * 2 / group.Members.Count);
                int offsetDistance = 150; 
                int xOffset = (int)(Math.Cos(angle) * offsetDistance);
                int yOffset = (int)(Math.Sin(angle) * offsetDistance);

                uint targetX = (uint)Math.Max(0, worldSpawn.X + xOffset);
                uint targetY = (uint)Math.Max(0, worldSpawn.Y + yOffset);
                ushort targetZ = (ushort)Math.Max(0, worldSpawn.Z);

                bot.Teleport(region, zoneId, targetX, targetY, targetZ, 0);
                botIndex++;
            }
        }

        public Group TryGetLoadedBotGroup(string prefix)
        {
            return GetLoadedBotGroup(prefix);
        }

        public bool HasLoadedBotsWithPrefix(string prefix)
        {
            return HasAnyLoadedBotsWithPrefix(prefix);
        }

        private void AssignToGuild(List<Player> bots, string guildName)
        {
            World.Guild.Guild guild = World.Guild.Guild.GetGuild(guildName);
            bool isNewGuild = false;
            
            Player firstBot = bots.FirstOrDefault(b => b != null);
            if (firstBot == null) return;

            if (guild == null)
            {
                isNewGuild = true;
                Guild_info info = new Guild_info
                {
                    Name = guildName,
                    Motd = "Bot Faction Guild",
                    LeaderId = firstBot.CharacterId,
                    AboutUs = "",
                    BriefDescription = string.Empty,
                    Summary = string.Empty,
                    Level = 40,
                    Realm = (byte)firstBot.Realm,
                    Xp = 0,
                    Tax = 0,
                    Money = 0,
                    Heraldry = string.Empty,
                    Banners = string.Empty,
                    guildvaultpurchased = new byte[5],
                    GuildTacticsPurchased = new ushort[40],
                    CreateDate = TCPManager.GetTimeStamp(),
                    GuildId = World.Guild.Guild.GenerateMaxGuildId(),
                    Members = new Dictionary<uint, Guild_member>(),
                    Ranks = new Dictionary<byte, Guild_rank>(),
                    Logs = new List<Guild_log>()
                };
                CharMgr.Database.AddObject(info);

                Guild_rank rank0 = new Guild_rank { GuildId = info.GuildId, RankId = 0, Name = "Initiate", Permissions = "00 30 00 02 89 44 20 10", Enabled = true };
                CharMgr.Database.AddObject(rank0); info.Ranks.Add(0, rank0);
                Guild_rank rank1 = new Guild_rank { GuildId = info.GuildId, RankId = 1, Name = "Member", Permissions = "00 B0 80 06 8F CF 60 10", Enabled = true };
                CharMgr.Database.AddObject(rank1); info.Ranks.Add(1, rank1);
                
                for (byte i = 2; i < 8; i++)
                {
                    Guild_rank rankUnused = new Guild_rank { GuildId = info.GuildId, RankId = i, Name = "Unused Status", Permissions = "00 00 00 00 08 00 00 00", Enabled = false };
                    CharMgr.Database.AddObject(rankUnused);
                    info.Ranks.Add(i, rankUnused);
                }

                Guild_rank rank8 = new Guild_rank { GuildId = info.GuildId, RankId = 8, Name = "Officer", Permissions = "2F F0 BF 9E 9F DF E5 33", Enabled = true };
                CharMgr.Database.AddObject(rank8); info.Ranks.Add(8, rank8);
                Guild_rank rank9 = new Guild_rank { GuildId = info.GuildId, RankId = 9, Name = "Guild Leader", Permissions = "FF FF FF FF FF FF FF 3F", Enabled = true };
                CharMgr.Database.AddObject(rank9); info.Ranks.Add(9, rank9);

                guild = new World.Guild.Guild(info);
                World.Guild.Guild.Guilds.Add(guild);
                guild.AddGuildLog(12, info.Name);
            }
            else if (!guild.Info.Members.ContainsKey(guild.Info.LeaderId))
            {
                guild.Info.LeaderId = firstBot.CharacterId;
                CharMgr.Database.SaveObject(guild.Info);
            }

            foreach (var bot in bots)
            {
                if (bot == null) continue;

                RemoveBotFromOtherGuilds(bot, guild);

                if (!guild.Info.Members.TryGetValue(bot.CharacterId, out Guild_member member))
                {
                    member = new Guild_member
                    {
                        CharacterId = bot.CharacterId,
                        GuildId = guild.Info.GuildId,
                        RankId = (bot == firstBot && isNewGuild) ? (byte)9 : (byte)1,
                        PublicNote = "",
                        OfficerNote = "",
                        Member = bot.Info,
                        JoinDate = (uint)TCPManager.GetTimeStamp()
                    };

                    guild.Info.Members.Add(bot.CharacterId, member);
                    CharMgr.Database.AddObject(member);
                    guild.AddGuildLog(0, bot.Name);
                    if (bot == firstBot && isNewGuild)
                        guild.AddGuildLog(11, bot.Name);
                }
                else
                {
                    member.Member = bot.Info;
                }

                bot.GldInterface.Guild = guild;
                bot.Info.Value.LeftSystemGuild = true;
            }
        }

        private void ApplyLoadout(Player bot, int tier, int rr, ushort primaryDye = 0, ushort secondaryDye = 0)
        {
            if (bot?.Info == null)
                return;

            BotLoadoutManager.BotTier bTier = GetBotTier(tier, rr);
            BotLoadoutManager.Loadout loadout = BotLoadoutManager.GetLoadout(bot, bTier, bot.Role);
            if (loadout == null)
                return;

            List<CharacterItem> existingItems = CharMgr.GetItemsForCharacter(bot.Info) ?? new List<CharacterItem>();
            if (bot.Loaded && bot.ItmInterface != null && bot.ItmInterface.IsLoad)
                ApplyLoadedBotLoadout(bot, loadout, primaryDye, secondaryDye);
            else
                ApplyPersistedBotLoadout(bot, loadout, existingItems, primaryDye, secondaryDye);
        }

        private BotLoadoutManager.BotTier GetBotTier(int tier, int rr)
        {
            return tier switch
            {
                1 => BotLoadoutManager.BotTier.T1,
                2 => BotLoadoutManager.BotTier.T2,
                3 => BotLoadoutManager.BotTier.T3,
                _ => BotLoadoutManager.BotTier.T4
            };
        }

        private byte GetMaxLevel(int tier)
        {
            return tier switch { 1 => 11, 2 => 21, 3 => 31, 4 => 40, _ => 40 };
        }

        private int GetTierFromLevel(byte level)
        {
            if (level <= 11)
                return 1;
            if (level <= 21)
                return 2;
            if (level <= 31)
                return 3;

            return 4;
        }

        private static void ResolveLoadoutDyes(string loadoutKey, out ushort primaryDye, out ushort secondaryDye)
        {
            primaryDye = 0;
            secondaryDye = 0;

            if (string.IsNullOrWhiteSpace(loadoutKey))
                return;

            if (loadoutKey.IndexOf("Red", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                primaryDye = 338;
                secondaryDye = 338;
            }
            else if (loadoutKey.IndexOf("Green", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                primaryDye = 336;
                secondaryDye = 336;
            }
            else if (loadoutKey.IndexOf("Blue", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                primaryDye = 314;
                secondaryDye = 314;
            }
            else if (loadoutKey.IndexOf("Yellow", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                primaryDye = 309;
                secondaryDye = 309;
            }
        }

        private bool TryGetBotTemplate(byte careerLine, out CharacterInfo template, out CharacterIdentityRecord identity)
        {
            template = CharMgr.GetCharacterInfo(careerLine);
            if (template == null)
                template = CharMgr.CharacterInfos.Values.FirstOrDefault(info => info.CareerLine == careerLine);

            if (template == null)
            {
                Log.Error("BotManager", $"No CharacterInfo found for career line {careerLine}");
                identity = default;
                return false;
            }

            if (!CharacterIdentityCatalog.TryGetByCareerLine(careerLine, out identity))
            {
                Log.Error("BotManager", $"No identity mapping found for career line {careerLine}");
                return false;
            }

            return true;
        }

        private bool RepairBotCharacter(Character character, CharacterInfo template, CharacterIdentityRecord identity, BotAppearance appearance)
        {
            bool dirty = false;

            if (character.AccountId != _botAccount.AccountId)
            {
                character.AccountId = _botAccount.AccountId;
                dirty = true;
            }

            if (character.RealmId != (Program.Rm?.RealmId ?? 1))
            {
                character.RealmId = Program.Rm?.RealmId ?? 1;
                dirty = true;
            }

            if (character.Career != template.Career)
            {
                character.Career = template.Career;
                dirty = true;
            }

            if (character.CareerLine != identity.CareerLine)
            {
                character.CareerLine = identity.CareerLine;
                dirty = true;
            }

            if (character.Realm != identity.Realm)
            {
                character.Realm = identity.Realm;
                dirty = true;
            }

            if (character.Race != identity.Race)
            {
                character.Race = identity.Race;
                dirty = true;
            }

            if (character.ModelId != appearance.ModelId)
            {
                character.ModelId = appearance.ModelId;
                dirty = true;
            }

            if (!TraitsEqual(character.bTraits, appearance.Traits))
            {
                character.bTraits = appearance.Traits.ToArray();
                dirty = true;
            }

            if (character.Surname == null)
            {
                character.Surname = string.Empty;
                dirty = true;
            }

            if (character.OldName == null)
            {
                character.OldName = string.Empty;
                dirty = true;
            }

            if (character.PetName == null)
            {
                character.PetName = string.Empty;
                dirty = true;
            }

            if (character.Sex != appearance.Sex)
            {
                character.Sex = appearance.Sex;
                dirty = true;
            }

            if (character.Anonymous)
            {
                character.Anonymous = false;
                dirty = true;
            }

            if (character.Hidden)
            {
                character.Hidden = false;
                dirty = true;
            }

            character.FirstConnect = false;
            return dirty;
        }

        private static void EnsureBotPvpEnabled(Player player)
        {
            if (player == null)
                return;

            if (player.CbtInterface is CombatInterface_Player playerCombat)
            {
                if (!playerCombat.IsPvp)
                    playerCombat.EnablePvp();
                else
                {
                    playerCombat.NextAllowedDisable = 0;
                    player.SetPVPFlag(true);
                }
            }
            else
                player.CbtInterface.IsPvp = true;
        }

        private static BotFaction ResolveDeterministicFaction(Realms realm, int tier)
        {
            BotFaction[] factions = realm == Realms.REALMS_REALM_ORDER ? OrderFactions : DestroFactions;
            int index = tier % factions.Length;
            return factions[index];
        }

        private static bool TryResolveBotAppearance(byte careerLine, out BotAppearance appearance)
        {
            // These model ids mirror the 1.4.8 client character creation defaults
            // in interface/interfacecore/source/characterselectwindow.lua.
            appearance = default;

            switch ((CareerLine)careerLine)
            {
                case CareerLine.CAREERLINE_IRON_BREAKER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 16, Traits = GetDefaultTraits((byte)Races.RACES_DWARF, 0) };
                    return true;
                case CareerLine.CAREERLINE_SLAYER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 18, Traits = GetDefaultTraits((byte)Races.RACES_DWARF, 0) };
                    return true;
                case CareerLine.CAREERLINE_RUNE_PRIEST:
                    appearance = new BotAppearance { Sex = 0, ModelId = 22, Traits = GetDefaultTraits((byte)Races.RACES_DWARF, 0) };
                    return true;
                case CareerLine.CAREERLINE_ENGINEER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 20, Traits = GetDefaultTraits((byte)Races.RACES_DWARF, 0) };
                    return true;
                case CareerLine.CAREERLINE_BLACK_ORC:
                    appearance = new BotAppearance { Sex = 0, ModelId = 12, Traits = GetDefaultTraits((byte)Races.RACES_ORC, 0) };
                    return true;
                case CareerLine.CAREERLINE_CHOPPA:
                    appearance = new BotAppearance { Sex = 0, ModelId = 13, Traits = GetDefaultTraits((byte)Races.RACES_ORC, 0) };
                    return true;
                case CareerLine.CAREERLINE_SHAMAN:
                    appearance = new BotAppearance { Sex = 0, ModelId = 14, Traits = GetDefaultTraits((byte)Races.RACES_GOBLIN, 0) };
                    return true;
                case CareerLine.CAREERLINE_SQUIG_HERDER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 15, Traits = GetDefaultTraits((byte)Races.RACES_GOBLIN, 0) };
                    return true;
                case CareerLine.CAREERLINE_WITCH_HUNTER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 34, Traits = GetDefaultTraits((byte)Races.RACES_EMPIRE, 0) };
                    return true;
                case CareerLine.CAREERLINE_KNIGHT_OF_THE_BLAZING_SUN:
                    appearance = new BotAppearance { Sex = 0, ModelId = 30, Traits = GetDefaultTraits((byte)Races.RACES_EMPIRE, 0) };
                    return true;
                case CareerLine.CAREERLINE_BRIGHT_WIZARD:
                    appearance = new BotAppearance { Sex = 0, ModelId = 32, Traits = GetDefaultTraits((byte)Races.RACES_EMPIRE, 0) };
                    return true;
                case CareerLine.CAREERLINE_WARRIOR_PRIEST:
                    appearance = new BotAppearance { Sex = 0, ModelId = 36, Traits = GetDefaultTraits((byte)Races.RACES_EMPIRE, 0) };
                    return true;
                case CareerLine.CAREERLINE_CHOSEN:
                    appearance = new BotAppearance { Sex = 0, ModelId = 24, Traits = GetDefaultTraits((byte)Races.RACES_CHAOS, 0) };
                    return true;
                case CareerLine.CAREERLINE_MARAUDER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 25, Traits = GetDefaultTraits((byte)Races.RACES_CHAOS, 0) };
                    return true;
                case CareerLine.CAREERLINE_ZEALOT:
                    appearance = new BotAppearance { Sex = 0, ModelId = 26, Traits = GetDefaultTraits((byte)Races.RACES_CHAOS, 0) };
                    return true;
                case CareerLine.CAREERLINE_MAGUS:
                    appearance = new BotAppearance { Sex = 0, ModelId = 28, Traits = GetDefaultTraits((byte)Races.RACES_CHAOS, 0) };
                    return true;
                case CareerLine.CAREERLINE_SWORDMASTER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 48, Traits = GetDefaultTraits((byte)Races.RACES_HIGH_ELF, 0) };
                    return true;
                case CareerLine.CAREERLINE_SHADOW_WARRIOR:
                    appearance = new BotAppearance { Sex = 0, ModelId = 50, Traits = GetDefaultTraits((byte)Races.RACES_HIGH_ELF, 0) };
                    return true;
                case CareerLine.CAREERLINE_WHITE_LION:
                    appearance = new BotAppearance { Sex = 0, ModelId = 46, Traits = GetDefaultTraits((byte)Races.RACES_HIGH_ELF, 0) };
                    return true;
                case CareerLine.CAREERLINE_ARCHMAGE:
                    appearance = new BotAppearance { Sex = 0, ModelId = 44, Traits = GetDefaultTraits((byte)Races.RACES_HIGH_ELF, 0) };
                    return true;
                case CareerLine.CAREERLINE_BLACK_GUARD:
                    appearance = new BotAppearance { Sex = 0, ModelId = 39, Traits = GetDefaultTraits((byte)Races.RACES_DARK_ELF, 0) };
                    return true;
                case CareerLine.CAREERLINE_WITCH_ELF:
                    appearance = new BotAppearance { Sex = 1, ModelId = 43, Traits = GetDefaultTraits((byte)Races.RACES_DARK_ELF, 1) };
                    return true;
                case CareerLine.CAREERLINE_DISCIPLE_OF_KHAINE:
                    appearance = new BotAppearance { Sex = 0, ModelId = 38, Traits = GetDefaultTraits((byte)Races.RACES_DARK_ELF, 0) };
                    return true;
                case CareerLine.CAREERLINE_SORCERER:
                    appearance = new BotAppearance { Sex = 0, ModelId = 41, Traits = GetDefaultTraits((byte)Races.RACES_DARK_ELF, 0) };
                    return true;
                default:
                    Log.Error("BotManager", $"No client model mapping found for bot career line {careerLine}");
                    return false;
            }
        }

        private static byte[] GetDefaultTraits(byte race, byte sex)
        {
            // Known-good live character payloads from this server. Prefer validated race/sex
            // combinations over guessed defaults because malformed trait bytes can make the
            // client fail to render the player body.
            if (race == (byte)Races.RACES_DWARF && sex == 0)
                return new byte[] { 5, 2, 6, 2, 5, 5, 3, 0 };

            if (race == (byte)Races.RACES_EMPIRE && sex == 0)
                return new byte[] { 1, 9, 4, 8, 3, 1, 9, 3 };

            if (race == (byte)Races.RACES_CHAOS && sex == 0)
                return new byte[] { 8, 13, 5, 6, 6, 2, 3, 0 };

            if (race == (byte)Races.RACES_DARK_ELF && sex == 1)
                return new byte[] { 5, 3, 0, 7, 0, 1, 4, 1 };

            return new byte[] { 1, 1, 1, 1, 1, 1, 1, 0 };
        }

        private static bool TraitsEqual(byte[] current, byte[] expected)
        {
            if (current == null || expected == null || current.Length != expected.Length)
                return false;

            for (int i = 0; i < current.Length; ++i)
            {
                if (current[i] != expected[i])
                    return false;
            }

            return true;
        }

        private static void ApplyPersistedBotLoadout(Player bot, BotLoadoutManager.Loadout loadout, List<CharacterItem> existingItems, ushort primaryDye, ushort secondaryDye)
        {
            RemoveManagedPersistedBotGear(existingItems);

            foreach (KeyValuePair<ushort, uint> entry in loadout.SlotItems)
            {
                Item_Info itemInfo = ItemService.GetItem_Info(entry.Value);
                if (itemInfo == null)
                {
                    Log.Error("BotManager", $"Missing persisted loadout item {entry.Value} for bot {bot.Name}");
                    continue;
                }

                CharacterItem newItem = new CharacterItem
                {
                    CharacterId = bot.CharacterId,
                    Entry = entry.Value,
                    ModelId = itemInfo.ModelId,
                    SlotId = entry.Key,
                    Counts = 1,
                    PrimaryDye = primaryDye,
                    SecondaryDye = secondaryDye,
                    BoundtoPlayer = false
                };

                CharMgr.CreateItem(newItem);
                existingItems.Add(newItem);
            }
        }

        private static void ApplyLoadedBotLoadout(Player bot, BotLoadoutManager.Loadout loadout, ushort primaryDye, ushort secondaryDye)
        {
            RemoveManagedLiveBotGear(bot);

            foreach (KeyValuePair<ushort, uint> entry in loadout.SlotItems)
            {
                Item_Info itemInfo = ItemService.GetItem_Info(entry.Value);
                if (itemInfo == null)
                {
                    Log.Error("BotManager", $"Missing live loadout item {entry.Value} for bot {bot.Name}");
                    continue;
                }

                ushort bagSlot = bot.ItmInterface.GetFreeInventorySlot(itemInfo);
                if (bagSlot == 0)
                {
                    Log.Error("BotManager", $"No free inventory slot while equipping {itemInfo.Name} for bot {bot.Name}");
                    continue;
                }

                ItemResult createResult = bot.ItmInterface.CreateItem(itemInfo, 1, new List<Talisman>(), primaryDye, secondaryDye, false, bagSlot);
                if (createResult != ItemResult.RESULT_OK)
                {
                    Log.Error("BotManager", $"CreateItem failed for bot {bot.Name} item {itemInfo.Entry} result {createResult}");
                    continue;
                }

                if (!bot.ItmInterface.MoveSlot(bagSlot, entry.Key, 1))
                {
                    Log.Error("BotManager", $"MoveSlot failed for bot {bot.Name} item {itemInfo.Entry} to slot {entry.Key}");
                    bot.ItmInterface.DeleteItem(bagSlot);
                }
            }

            if (bot.IsInWorld())
                bot.ItmInterface.SendEquipped(null);
        }

        private static void RemoveManagedPersistedBotGear(List<CharacterItem> existingItems)
        {
            foreach (CharacterItem existing in existingItems.ToList())
            {
                Item_Info existingInfo = ItemService.GetItem_Info(existing.Entry);
                bool managedSlot = BotLoadoutManager.IsManagedEquipmentSlot(existing.SlotId);
                bool managedItem = BotLoadoutManager.IsManagedItemInfo(existingInfo);
                if (!managedSlot && !managedItem)
                    continue;

                CharMgr.DeleteItem(existing);
                existingItems.Remove(existing);
            }
        }

        private static void RemoveManagedLiveBotGear(Player bot)
        {
            if (bot?.ItmInterface?.Items == null)
                return;

            List<ushort> slotsToDelete = new List<ushort>();

            for (ushort slotId = 0; slotId < bot.ItmInterface.Items.Length; ++slotId)
            {
                WorldServer.World.Objects.Item item = bot.ItmInterface.Items[slotId];
                if (item?.Info == null)
                    continue;

                bool managedSlot = BotLoadoutManager.IsManagedEquipmentSlot(slotId);
                bool managedItem = BotLoadoutManager.IsManagedItemInfo(item.Info);
                if (managedSlot || managedItem)
                    slotsToDelete.Add(slotId);
            }

            foreach (ushort slotId in slotsToDelete)
                bot.ItmInterface.DeleteItem(slotId);
        }

        private static Character_value CreateBotCharacterValue(Character character, CharacterInfo template, byte level, byte renownRank, Realms realm)
        {
            Character_value value = new Character_value
            {
                CharacterId = character.CharacterId,
                Level = level,
                Money = 0,
                Online = false,
                RallyPoint = template.RallyPt,
                RegionId = ZoneService.GetZone_Info(template.ZoneId)?.Region ?? template.Region,
                Renown = 0,
                RenownRank = renownRank,
                RestXp = 0,
                Skills = template.Skills,
                Speed = 100,
                PlayedTime = 0,
                WorldO = template.WorldO,
                WorldX = template.WorldX,
                WorldY = template.WorldY,
                WorldZ = template.WorldZ,
                Xp = 0,
                ZoneId = template.ZoneId,
                LeftSystemGuild = true
            };

            if (WorldMgr.NormalizeCharacterWorldPosition(value, (byte)realm, character.Name, out string spawnReason))
                Log.Info("BotManager", $"Normalized bot position for {character.Name}: {spawnReason}");

            return value;
        }

        private static bool RepairBotCharacterValue(Character character, CharacterInfo template, byte level, byte renownRank, Realms realm)
        {
            Character_value value = character.Value;
            bool dirty = false;

            if (value.CharacterId != character.CharacterId)
            {
                value.CharacterId = character.CharacterId;
                dirty = true;
            }

            if (value.Level != level)
            {
                value.Level = level;
                dirty = true;
            }

            if (value.RenownRank != renownRank)
            {
                value.RenownRank = renownRank;
                dirty = true;
            }

            if (value.Speed <= 0)
            {
                value.Speed = 100;
                dirty = true;
            }

            if (value.RallyPoint == 0)
            {
                value.RallyPoint = template.RallyPt;
                dirty = true;
            }

            if (value.RegionId <= 0)
            {
                value.RegionId = ZoneService.GetZone_Info(template.ZoneId)?.Region ?? template.Region;
                dirty = true;
            }

            if (value.ZoneId == 0)
            {
                value.ZoneId = template.ZoneId;
                dirty = true;
            }

            if (value.WorldX == 0 && value.WorldY == 0)
            {
                value.WorldX = template.WorldX;
                value.WorldY = template.WorldY;
                value.WorldZ = template.WorldZ;
                value.WorldO = template.WorldO;
                dirty = true;
            }

            if (value.Skills == 0)
            {
                value.Skills = template.Skills;
                dirty = true;
            }

            if (!value.LeftSystemGuild)
            {
                value.LeftSystemGuild = true;
                dirty = true;
            }

            if (WorldMgr.NormalizeCharacterWorldPosition(value, (byte)realm, character.Name, out string spawnReason))
            {
                dirty = true;
                Log.Info("BotManager", $"Repaired bot position for {character.Name}: {spawnReason}");
            }

            return dirty;
        }

        private static bool EnsureBotCollections(Character character)
        {
            bool dirty = false;

            if (character.Mails == null)
            {
                character.Mails = new List<Character_mail>();
                dirty = true;
            }

            if (character.Quests == null)
            {
                character.Quests = new List<Character_quest>();
                dirty = true;
            }

            if (character.Toks == null)
            {
                character.Toks = new List<Character_tok>();
                dirty = true;
            }

            if (character.TokKills == null)
            {
                character.TokKills = new List<Character_tok_kills>();
                dirty = true;
            }

            if (character.Socials == null)
            {
                character.Socials = new List<Character_social>();
                dirty = true;
            }

            if (character.Influences == null)
            {
                character.Influences = new List<Characters_influence>();
                dirty = true;
            }

            if (character.Bag_Pools == null)
            {
                character.Bag_Pools = new List<Characters_bag_pools>();
                dirty = true;
            }

            if (character.Buffs == null)
            {
                character.Buffs = new List<CharacterSavedBuff>();
                dirty = true;
            }

            return dirty;
        }

        private static void StageBotSpawn(Player bot, Zone_Info zoneInfo, Point3D worldSpawn)
        {
            if (bot?.Info?.Value == null || zoneInfo == null || worldSpawn == null)
                return;

            bot.Info.Value.ZoneId = zoneInfo.ZoneId;
            bot.Info.Value.RegionId = zoneInfo.Region;
            bot.Info.Value.WorldX = worldSpawn.X;
            bot.Info.Value.WorldY = worldSpawn.Y;
            bot.Info.Value.WorldZ = worldSpawn.Z;
            bot.Info.Value.WorldO = 0;
            bot.Info.Value.LeftSystemGuild = true;
        }

        private static List<Player> GetLiveBotsForPrefix(string prefix)
        {
            return Player.GetPlayersSnapshot()
                .Where(player => player != null
                    && player.IsBot
                    && !player.IsDisposed
                    && !player.PendingDisposal
                    && player.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static bool HasAnyLoadedBotsWithPrefix(string prefix)
        {
            return GetLiveBotsForPrefix(prefix).Count > 0;
        }

        private static Group GetLoadedBotGroup(string prefix)
        {
            List<Player> liveBots = GetLiveBotsForPrefix(prefix);
            if (!BotGroupSuffixes.All(suffix => liveBots.Any(player => player.Name.Equals(prefix + suffix, StringComparison.OrdinalIgnoreCase))))
                return null;

            return liveBots.FirstOrDefault(player => player.PriorityGroup != null)?.PriorityGroup;
        }

        private static void RemoveBotFromOtherGuilds(Player bot, World.Guild.Guild targetGuild)
        {
            if (bot == null)
                return;

            foreach (World.Guild.Guild guild in World.Guild.Guild.Guilds.ToList())
            {
                if (guild == null || guild == targetGuild)
                    continue;

                if (!guild.Info.Members.TryGetValue(bot.CharacterId, out Guild_member member))
                    continue;

                guild.Info.Members.Remove(bot.CharacterId);
                CharMgr.Database.DeleteObject(member);

                if (guild.IsSystemGuild() && bot.Info?.Value != null)
                    bot.Info.Value.LeftSystemGuild = true;
            }
        }
    }
}


