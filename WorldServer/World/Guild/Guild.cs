using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using SystemData;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Item = WorldServer.World.Objects.Item;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Guild
{

    public class Alliance
    {
        public static int MaxAllianceGUID = 1;

        public static Dictionary<uint, Guild_Alliance_info> Alliances = new Dictionary<uint, Guild_Alliance_info>();

        public static uint CreateAlliance(string name)
        {
            Guild_Alliance_info alli = new Guild_Alliance_info
            {
                Name = name,
                AllianceId = (uint)Interlocked.Increment(ref MaxAllianceGUID)
            };
            Alliances.Add(alli.AllianceId, alli);
            CharMgr.Database.AddObject(alli);
            return alli.AllianceId;
        }
    }
    public class GuildInvitation
    {
        private static readonly Logger GuildLogger = LogManager.GetLogger("GuildLogger");
        private Player _owner;
        private Dictionary<Player, bool> _invites = new Dictionary<Player, bool>();
        private string _name;

        public GuildInvitation(Player plr, string name)
        {
            Regex r = new Regex("^[a-zA-Z0-9 :]*$");
            // if a gm makes a group with no players
            if (plr.WorldGroup == null)
            {
                _invites.Add(plr, false);
                plr.GldInterface.invite = this;
            }
            if (!r.IsMatch(name) || String.IsNullOrWhiteSpace(name))
            {
                plr.SendClientMessage("The specified guild name is not valid. It may only contain alphanmumeric letters and spaces");
                return;
            }
            else
            {
                foreach (Player groupedPlayer in plr.WorldGroup.GetPlayerList())
                {
                    _invites.Add(groupedPlayer, false);
                    groupedPlayer.GldInterface.invite = this;
                }

            }

            _owner = plr;
            if (name.EndsWith(" ") || name.StartsWith(" "))
            {
                name = name.Trim();
            }
            this._name = name;

            SendInvites();
        }

        public void SendInvites()
        {
            foreach (Player plr in _invites.Keys)
            {
                LogGuildBug(plr);
                PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYERORG_APPROVAL, 32);
                Out.WriteUInt16(0);
                Out.WriteStringToZero(_name);
                plr.SendPacket(Out);
            }
        }
        private void LogGuildBug(Player plr)
        {
            //if (plr.Name.Contains("arena") || plr.Name.Contains("poko") || plr.Name.Contains("bram") || plr.Name.Contains("zaru") || plr.Name.Contains("niffils") ||
            //    plr.Name.Contains("ikdorf") || plr.Name.Contains("grimjob"))
            //{
            //    var l_CurrentStack = new System.Diagnostics.StackTrace(true);

            //    GuildLogger.Debug($"{plr.Name} {l_CurrentStack.ToString()}");
            //}

        }
        public void InviteResponse(Player plr, bool accepted)
        {
            LogGuildBug(plr);
            if (!_invites.Keys.Contains(plr))
            {
                foreach (Player invtPlr in _invites.Keys)
                {
                    invtPlr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GUILDNPC_GROUPCHANGED);
                    invtPlr.GldInterface.invite = null;
                }
            }

            _invites[plr] = accepted;

            if (!accepted)
            {
                foreach (Player invtPlr in _invites.Keys)
                {
                    invtPlr.SendLocalizeString(plr.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GUILD_APPROVAL_DECLINE);
                    invtPlr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GUILDNPC_UNAPPROVED);
                    invtPlr.GldInterface.invite = null;
                }
            }
            else
            {
                foreach (Player invtPlr in _invites.Keys)
                {
                    invtPlr.SendLocalizeString(plr.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GUILD_APPROVAL_ACCEPT);
                }
                CheckInvites();
            }
        }

        private void CheckInvites()
        {
           
            foreach (KeyValuePair<Player, bool> invite in _invites)
            {
                if (invite.Value == false)
                    return;
            }

            CreateGuild();
        }

        private void CreateGuild()
        {
            Guild_info info = new Guild_info
            {
                Name = _name,
                Motd = "",
                LeaderId = _owner.Info.CharacterId,
                AboutUs = "",
                Level = 1,
                Xp = 0,
                CreateDate = TCPManager.GetTimeStamp(),
                GuildId = Guild.GenerateMaxGuildId(),
                Members = new Dictionary<uint, Guild_member>(),
                Ranks = new Dictionary<byte, Guild_rank>(),
                Logs = new List<Guild_log>()
            };
            CharMgr.Database.AddObject(info);

            Guild_rank rank0 = new Guild_rank
            {
                GuildId = info.GuildId,
                RankId = 0,
                Name = "Initiate",
                Permissions = "00 30 00 02 89 44 20 10",
                Enabled = true
            };
            CharMgr.Database.AddObject(rank0);
            info.Ranks.Add(0, rank0);

            Guild_rank rank1 = new Guild_rank
            {
                GuildId = info.GuildId,
                RankId = 1,
                Name = "Member",
                Permissions = "00 B0 80 06 8F CF 60 10",
                Enabled = true
            };
            CharMgr.Database.AddObject(rank1);
            info.Ranks.Add(1, rank1);

            for (byte i = 2; i < 8; i++)
            {
                Guild_rank rankUnused = new Guild_rank
                {
                    GuildId = info.GuildId,
                    RankId = i,
                    Name = "Unused Status",
                    Permissions = "00 00 00 00 08 00 00 00",
                    Enabled = false
                };
                CharMgr.Database.AddObject(rankUnused);
                info.Ranks.Add(i, rankUnused);
            }

            Guild_rank rank8 = new Guild_rank
            {
                GuildId = info.GuildId,
                RankId = 8,
                Name = "Officer",
                Permissions = "2F F0 BF 9E 9F DF E5 33",
                Enabled = true
            };
            CharMgr.Database.AddObject(rank8);
            info.Ranks.Add(8, rank8);

            Guild_rank rank9 = new Guild_rank
            {
                GuildId = info.GuildId,
                RankId = 9,
                Name = "Guild Leader",
                Permissions = "FF FF FF FF FF FF FF 3F",
                Enabled = true
            };
            CharMgr.Database.AddObject(rank9);
            info.Ranks.Add(9, rank9);

            foreach (Player plr in _invites.Keys)
            {
                Guild_member member = new Guild_member
                {
                    CharacterId = plr.Info.CharacterId,
                    GuildId = info.GuildId
                };
                if (_owner == plr)
                    member.RankId = 9;
                else
                    member.RankId = 1;
                member.PublicNote = "";
                member.OfficerNote = "";
                member.Member = plr.Info;
                member.JoinDate = (uint)TCPManager.GetTimeStamp();
                info.Members.Add(member.CharacterId, member);
                CharMgr.Database.AddObject(member);
            }

            Guild guild = new Guild(info);
            Guild.Guilds.Add(guild);

            foreach (Player plr in _invites.Keys)
            {
                plr.GldInterface.Guild = guild;
                guild.AddOnlineMember(plr);
            }

            guild.AddGuildLog(12, info.Name);

            foreach (Player plr in _invites.Keys)
            {
                guild.AddGuildLog(0, plr.Name);

                if (_owner == plr)
                    guild.AddGuildLog(11, plr.Name);
            }

            foreach (Player plr in _invites.Keys)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GUILDNPC_PROCESS_COMPLETE);
                guild.SendGuildInfo(plr);
                plr.GldInterface.invite = null;
            }
        }
    }

    public class Guild
    {
        #region Statics (Global guild management)
        private static readonly Logger GuildLogger = LogManager.GetLogger("GuildLogger");

        public static List<Guild> Guilds = new List<Guild>();
        public static int MaxGuildGUID = 1;
        public static byte MaxGuildLevel = 40;
        public static byte MaxEvents = 10;

        public static Guild GetGuild(string name)
        {
            foreach (Guild guild in Guilds)
            {
                if (guild.Info.Name == name)
                    return guild;
            }

            return null;
        }

        public static Guild GetGuild(uint guildId)
        {
            foreach (Guild guild in Guilds)
            {
                if (guild.Info.GuildId == guildId)
                    return guild;
            }

            return null;
        }

        public static Guild GetGuildFromLeader(uint id)
        {
            foreach (Guild guild in Guilds)
            {
                foreach (Guild_member mem in guild.Info.Members.Values)
                {
                    if (mem.CharacterId == id)
                        return guild;
                }
            }

            return null;
        }

        public static List<Guild> GetGuilds(Realms realm, byte style, byte atmostphere, byte myLevelCareer, byte level, byte career, ushort pop, byte online, byte guildRank)
        {
            List<Guild> guilds = new List<Guild>();

            foreach (Guild guild in Guild.Guilds)
            {
                if (guild.Inactive)
                {
                    continue;
                }

                if (guild.Info.Members[guild.Info.LeaderId].Member.Realm == (int)realm)
                {
                    if (pop == 0 || guild.Info.Members.Count > pop)
                    {
                        if (online == 0 || guild.OnlineMembers.Count > online)
                        {
                            if (guild.Info.Level > guildRank)
                            {
                                if (style == 0 || guild.Info.PlayStyle == style)
                                {
                                    if (atmostphere == 0 || guild.Info.Atmosphere == atmostphere)
                                    {
                                        // todo: check the rest of the criteria
                                        guilds.Add(guild);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return guilds;
        }

        public static uint GenerateMaxGuildId()
        {
            return (uint)Interlocked.Increment(ref MaxGuildGUID);
        }

        public static void BuildGuild(ref PacketOut Out, Guild guild)
        {

            Out.WriteUInt32(guild.Info.GuildId);
            Out.WriteByte(guild.Info.Level); //level
            Out.WriteShortString(guild.Info.Name);
            if (guild.Info.Members[guild.Info.LeaderId] != null)
                Out.WriteShortString(guild.Info.Members[guild.Info.LeaderId].Member.Name);
            else
            {
                Out.WriteShortString("UNKNOWN");
            }
            Out.WriteShortString(guild.Info.BriefDescription);
            Out.WriteShortString(guild.Info.Summary);
            Out.WriteByte(guild.Info.PlayStyle);
            Out.WriteByte(guild.Info.Atmosphere); // atmosphere
            Out.WriteUInt32(guild.Info.CareersNeeded);
            Out.WriteUInt32(guild.Info.RanksNeeded);
            Out.WriteUInt32(guild.Info.Interests);
            Out.WriteUInt32(guild.Info.ActivelyRecruiting);
            Out.WriteUInt32((uint)guild.Info.Members.Count); //total members
            Out.WriteUInt32((uint)guild.OnlineMembers.Count); // online
            Out.WriteShortString("");
            List<Guild_member> recruiters = guild.Info.Members.Values.Where(m => m.GuildRecruiter).Take(5).ToList();
            Out.WriteByte((byte)recruiters.Count); // Recruiters Count
            foreach (Guild_member recruiter in recruiters)
            {
                Out.WriteShortString(recruiter.Member.Name);
            }
            Out.WriteUInt32(0);
        }

        public static void SendNullGuild(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA, 2);
            Out.WriteUInt16(0);
            plr.SendPacket(Out);
        }

        public static void HasPermissionsByte(ref int Byte, ref int bit, byte command)
        {
            Byte++;
            for (int i = 0; i < command; i++)
            {
                bit++;
                if (bit == 8)
                {
                    Byte++;
                    bit = 0;
                }
            }
        }
        public static void HasPermissionsByte(ref int Byte, ref int bit, GuildPermissions permission)
        {
            HasPermissionsByte(ref Byte, ref bit, (byte)permission);
        }

        public static bool HasPermissions(Guild_rank rank, byte command)
        {
            int Byte = 0;
            int bit = 0;

            HasPermissionsByte(ref Byte, ref bit, command);

            byte[] permissionBytes = ConvertHexStringToByteArray(rank.Permissions.Replace(" ", ""));

            return (permissionBytes[Byte - 1] & (1 << bit)) != 0;
        }

        public static bool HasPermissions(Guild_rank rank, GuildPermissions permission)
        {
            return HasPermissions(rank, (byte)permission);
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                return null;
            }

            byte[] hexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexAsBytes.Length; index++)
            {
                hexAsBytes[index] = Convert.ToByte(hexString.Substring(index * 2, 2), 16);
            }

            return hexAsBytes;
        }



        #endregion

        public byte StandardBearerMax;
        public List<Player> OnlineMembers = new List<Player>();
        public List<Player> GuildVaultUser = new List<Player>();
        public Guild_info Info;
        ushort[,] _banners = { { 1, 0, 0, 0 }, { 1, 0, 0, 0 }, { 1, 0, 0, 0 } };
        long[] _bannerlock = { 0, 0, 0 };

        public bool Inactive = false;


        //heraldry
        ushort _heraldryEmblem = 2;
        ushort _heraldryPattern = 100;
        byte _heraldryColor1 = 2;
        byte _heraldryColor2 = 2;
        byte _heraldryShape = 1;



        public Guild(Guild_info info)
        {
            this.Info = info;

            //info.Banner.Split(";")[0];

            if (info.Realm == 0)
            {
                info.Realm = CharMgr.GetCharacter(info.LeaderId, false).Realm;
            }


            if (string.IsNullOrEmpty(info.Heraldry))
            {
                Character tmp = CharMgr.GetCharacter(info.LeaderId, false);
                if (tmp.Realm == 1)
                {
                    _heraldryEmblem = 1;
                    _heraldryPattern = 1;
                    _heraldryColor1 = 1;
                    _heraldryColor2 = 1;
                    _heraldryShape = 1;
                }
                if (tmp.Realm == 2)
                {
                    _heraldryEmblem = 2;
                    _heraldryPattern = 100;
                    _heraldryColor1 = 2;
                    _heraldryColor2 = 2;
                    _heraldryShape = 1;
                }
                info.Heraldry = _heraldryEmblem + ";" + _heraldryPattern + ";" + _heraldryColor1 + ";" + _heraldryColor2 + ";" + _heraldryShape;
            }
            else
            {
                _heraldryEmblem = ushort.Parse(info.Heraldry.Split(';')[0]);
                _heraldryPattern = ushort.Parse(info.Heraldry.Split(';')[1]);
                _heraldryColor1 = byte.Parse(info.Heraldry.Split(';')[2]);
                _heraldryColor2 = byte.Parse(info.Heraldry.Split(';')[3]);
                _heraldryShape = byte.Parse(info.Heraldry.Split(';')[4]);
            }

            if (string.IsNullOrEmpty(info.Banners))
            {
                info.Banners = "1, 0, 0, 0; 1, 0, 0, 0; 1, 0, 0, 0";
            }
            else
            {
                string[] tmp = info.Banners.Split(';');
                _banners = new[,] { { ushort.Parse(tmp[0].Split(',')[0]), ushort.Parse(tmp[0].Split(',')[1]), ushort.Parse(tmp[0].Split(',')[2]), ushort.Parse(tmp[0].Split(',')[3]) }, { ushort.Parse(tmp[1].Split(',')[0]), ushort.Parse(tmp[1].Split(',')[1]), ushort.Parse(tmp[1].Split(',')[2]), ushort.Parse(tmp[1].Split(',')[3]) }, { ushort.Parse(tmp[2].Split(',')[0]), ushort.Parse(tmp[2].Split(',')[1]), ushort.Parse(tmp[2].Split(',')[2]), ushort.Parse(tmp[2].Split(',')[3]) } };
            }
            if (info.Level >= 3 && info.guildvaultpurchased[0] == 0)
                info.guildvaultpurchased[0] = 10;
            if (info.Level >= 11 && info.guildvaultpurchased[1] == 0)
                info.guildvaultpurchased[1] = 10;
            if (info.Level >= 23 && info.guildvaultpurchased[2] == 0)
                info.guildvaultpurchased[2] = 10;
            if (info.Level >= 33 && info.guildvaultpurchased[3] == 0)
                info.guildvaultpurchased[3] = 10;

            CharMgr.Database.SaveObject(info);

            CalcMaxStandardBearers();

        }

        #region Packets 

        public void SendGuildInfo(Player plr)
        {
            LogGuildBug(plr);
            // This clears the guild interface
            SendNullGuild(plr);

            SendGuildProfile(plr);

            SendAllMembers(plr);

            SendGuildRanks(plr);

            SendGuildXp(plr);

            SendGuildHeraldry(plr);

            SendGuildBanner(plr);

            SendGuildTactics(plr);

            SendGuildTax(plr);

            SendEvents(plr);

            SendGuildTacticsPurchased(plr);



            if (Info.AllianceId > 0)
                SendAlliance(plr);

            foreach (Guild_log log in Info.Logs)
                SendGuildLog(log, false, plr);


            SendGuildRecruitment(plr);



            SendGuildPlayerContributed(plr, Info.Members[plr.CharacterId]);
        }

        public void SendGuildProfile(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA, 256);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WriteStringToZero(Info.Name);
            Out.WriteUInt16(0);
            Out.WriteUInt16((ushort)Info.GuildId);
            Out.WriteByte(2);
            Out.WriteUInt32((uint)Info.CreateDate);
            Out.WriteByte(0);
            Out.WriteStringToZero(Info.Motd);
            Out.WriteByte(0);
            Out.WriteStringToZero("");
            Out.WriteByte(0);
            Out.WriteStringToZero(Info.AboutUs);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);

            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);
        }

        public void SendGuildRanks(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA, 3 + Info.Ranks.Count * 20);
            Out.WriteByte(4);
            Out.WriteByte(1);
            Out.WriteByte((byte)Info.Ranks.Count);

            foreach (Guild_rank rank in Info.Ranks.Values)
            {
                Out.WriteByte(rank.RankId);
                Out.WriteByte(0);
                Out.WriteStringToZero(rank.Name);
                Out.WriteHexStringBytes(rank.Permissions.Replace(" ", ""));
                Out.WriteByte((byte)(rank.Enabled ? 1 : 0));
            }

            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);
        }

        public void SendAllMembers(Player plr)
        {
            LogGuildBug(plr);
            byte count = 0;

            PacketOut buffer = new PacketOut(0);
            buffer.Position = 0;
            bool first = true;

            foreach (Guild_member member in Info.Members.Values)
            {
                if (member.Member == null)
                    continue;

                if (count >= 40)
                {
                    SendBuffer(plr, ref buffer, 0x1F, ref count, first);
                    if (first)
                        first = false;
                }

                Player onlinePlr = GetGuildPlayer(member.CharacterId);

                buffer.WriteUInt32(member.CharacterId);
                buffer.WriteUInt16((ushort)(onlinePlr == null ? 0 : onlinePlr.IsInWorld() ? onlinePlr.Zone.ZoneId : 0));
                buffer.WriteByte((byte)(member.StandardBearer ? 1 : 0)); // 1 - standard bearer
                buffer.WriteByte((byte)(member.RealmCaptain ? 1 : 0)); // 1 -realm captain
                buffer.WriteByte(member.RankId);
                buffer.WriteByte(0);
                buffer.WriteStringToZero(Info.Ranks[member.RankId].Name);
                buffer.WriteByte((byte)((member.RankId == 9 ? 4 : member.AllianceOfficer ? 3 : member.RankId > 0 ? 2 : 0)));   //Alliance rank
                buffer.WriteByte(0);
                buffer.WriteStringToZero(member.Member.Name + (member.Member.Sex == 1 ? "^F" : "^M"));
                buffer.WriteByte(member.Member.Value.Level);
                buffer.WriteByte(0);
                buffer.WriteByte(member.Member.Career);
                buffer.WriteByte(0);
                buffer.WriteUInt32(member.LastSeen);
                //Buffer.WriteUInt32(0x01010100);
                buffer.WriteByte((byte)(member.GuildRecruiter ? 1 : 0)); // 1 - guild recruiter (red) 2 - guild recruiter (blue)
                //Buffer.WriteByte(0x1);
                buffer.WriteByte((byte)(onlinePlr == null ? 0 : onlinePlr.WorldGroup == null ? 0 : onlinePlr.WorldGroup.IsWarband ? 2 : 1)); // 1 - party 2 - warband
                buffer.WriteByte((byte)(onlinePlr == null ? 0 : onlinePlr.WorldGroup == null ? 0 : onlinePlr.WorldGroup.PartyOpen ? 2 : 0)); // 0 - closed 2 - open
                buffer.WriteByte(0);
                count++;
            }

            if (count > 0)
                SendBuffer(plr, ref buffer, 0x1F, ref count, first);

            // Send all notes tooooo
            SendAllNotes(plr);
        }

        public void SendAllNotes(Player plr)
        {
            LogGuildBug(plr);
            byte count = 0;

            PacketOut buffer = new PacketOut(0);
            buffer.Position = 0;
            bool first = true;

            foreach (Guild_member member in Info.Members.Values)
            {
                if (count >= 40)
                {
                    SendBuffer(plr, ref buffer, 0x05, ref count, first);
                    if (first)
                        first = false;
                }

                buffer.WriteUInt32(member.CharacterId);
                buffer.WriteShortStringToZero(member.PublicNote);
                buffer.WriteShortStringToZero(member.OfficerNote);
                count++;
            }

            if (count > 0)
                SendBuffer(plr, ref buffer, 0x05, ref count, first);
        }

        private void SendBuffer(Player plr, ref PacketOut buffer, byte type, ref byte count, bool first)
        {
            LogGuildBug(plr);
            byte[] arrayBuf = buffer.ToArray();
            PacketOut packet = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            packet.WriteByte(type);
            packet.WriteByte(count);

            // We need to indicate the start and end of the guild member stuff 1 - start 2 - end 3 - only 1 packet
            if (first && count < 40) // Only 1 packet
                packet.WriteByte(3);
            else if (first) // First of many
                packet.WriteByte(1);
            else if (count < 40) // Last of many
                packet.WriteByte(2);
            else // One of many
                packet.WriteByte(0);

            packet.Write(arrayBuf, 0, arrayBuf.Length);

            if (plr == null)
                SendToGuild(packet);
            else
                plr.SendPacket(packet);

            // On Remet le compteur a zero
            count = 0;

            // On Initalise un nouveau buffer
            buffer = new PacketOut(0);
            buffer.Position = 0;
        }

        public void SendMember(Player plr, Guild_member guildPlr)
        {
            
            if (plr != null)
                GuildLogger.Debug($"{plr.Name}");
            Player onlinePlr = GetGuildPlayer(guildPlr.CharacterId);

            LogGuildBug(onlinePlr);

            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(2);
            Out.WriteByte(1);
            Out.WriteUInt32(guildPlr.CharacterId);
            Out.WriteUInt16((ushort)(onlinePlr == null ? 0 : onlinePlr.IsInWorld() ? onlinePlr.Zone.ZoneId : 0));
            Out.WriteByte((byte)(guildPlr.StandardBearer ? 1 : 0)); // 1 - standard bearer
            Out.WriteByte((byte)(guildPlr.RealmCaptain ? 1 : 0)); // 1 -realm captain
            Out.WriteByte(guildPlr.RankId);
            Out.WriteByte(0);
            Out.WriteStringToZero(Info.Ranks[guildPlr.RankId].Name);
            Out.WriteByte((byte)((guildPlr.RankId == 9 ? 4 : guildPlr.AllianceOfficer ? 3 : guildPlr.RankId > 0 ? 2 : 0)));   //todo Alliance rank
            Out.WriteByte(0);
            Out.WriteStringToZero(guildPlr.Member.Name + (guildPlr.Member.Sex == 1 ? "^F" : "^M"));
            Out.WriteByte(guildPlr.Member.Value.Level);
            Out.WriteByte(0);
            Out.WriteByte(guildPlr.Member.Career);
            Out.WriteByte(0);
            Out.WriteUInt32(guildPlr.LastSeen);
            Out.WriteByte((byte)(guildPlr.GuildRecruiter ? 1 : 0)); // 1 - guild recruiter (red) 2 - guild recruiter (blue)
            Out.WriteByte((byte)(onlinePlr == null ? 0 : onlinePlr.WorldGroup == null ? 0 : onlinePlr.WorldGroup.IsWarband ? 2 : 1)); // 1 - party 2 - warband
            Out.WriteByte((byte)(onlinePlr == null ? 0 : onlinePlr.WorldGroup == null ? 0 : onlinePlr.WorldGroup.PartyOpen ? 2 : 0)); // 0 - closed 2 - open
            Out.WriteByte(1);
            Out.WriteShortStringToZero(guildPlr.PublicNote);
            Out.WriteShortStringToZero(guildPlr.OfficerNote);


            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);
        }

        public void SendGuildXp(Player plr)
        {
            LogGuildBug(plr);
            Guild_Xp xpCurr = GuildService.GetGuild_Xp(Info.Level);
            Guild_Xp xpNext = GuildService.GetGuild_Xp((byte)(Info.Level + 1));

            uint soFar = Info.Level >= MaxGuildLevel ? Info.Xp : (Info.Xp - xpCurr.Xp);
            uint next = Info.Level >= MaxGuildLevel ? Info.Xp : xpNext.Xp;

            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x03);
            Out.WriteUInt32(0);
            Out.WriteByte(Info.Level);
            Out.WriteUInt32(Info.Xp); // Total Guild Xp
            Out.WriteUInt32(next);
            Out.WriteUInt64(Info.Renown); // Renown
            Out.WriteUInt32(soFar); // Next Guild Rank

            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);

        }

        public void SendGuildTax(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x1E);
            Out.WriteByte(0x1E);
            Out.WriteByte(Info.Tax);
            Out.WriteByte(Info.Members[plr.CharacterId].Tithe);  //tithe

            plr.SendPacket(Out);
        }

        public void SendAlliance(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x07);
            Out.WriteByte(0x0);
            Out.WriteUInt32(Info.AllianceId);
            Out.WriteByte(0x0);
            Out.WritePascalString(Alliance.Alliances[Info.AllianceId].Name);
            Out.WriteByte(0);
            Out.WriteByte(0x4e);  // no clue what those are
            Out.WriteByte(0x44);
            Out.WriteByte(0x5f);
            Out.WriteByte(0);

            plr.SendPacket(Out);

            SendAllianceGuilds(plr);

            // SendAllianceGuilds(Plr);
            /*
                        00 00 00 00 DF 00 0C 4C 65 20 4F 6C |...........Le Ol|
            |64 20 4F 6E 65 73 00 4E 44 5F 00                |...........     |
            -------------------------------------------------------------------
            */

        }

        public void SendAllianceGuilds(Player Plr)
        {
            foreach (uint guildid in Alliance.Alliances[Info.AllianceId].Members)
            {
                List<Guild_member> officers = new List<Guild_member>();

                Guild gl = GetGuild(guildid);
                if (gl == null)
                {
                    continue;
                }

                foreach (KeyValuePair<uint, Guild_member> gm in gl.Info.Members)
                {
                    if (gm.Value.AllianceOfficer || gm.Value.CharacterId == gl.Info.LeaderId)
                        officers.Add(gm.Value);
                }


                PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
                Out.WriteByte(0x08);
                Out.WriteByte(0x01);
                Out.WriteUInt32(gl.Info.GuildId);
                Out.WriteByte(0x00);
                Out.WriteByte((byte)(gl.Info.Name.Length + 1));
                Out.WriteStringBytes(gl.Info.Name);
                Out.WriteByte(0x00);
                Out.WriteByte((byte)officers.Count);  //number of alliance officers / leaders 
                gl.BuildHeraldry(Out);
                Out.WriteByte(0x00);
                Out.WritePascalString(CharMgr.GetCharacter(gl.Info.LeaderId, true).Name + (CharMgr.GetCharacter(gl.Info.LeaderId, true).Sex == 1 ? "^F" : "^M"));
                Out.WriteUInt32((uint)gl.Info.CreateDate);   // created
                Out.WriteByte(gl.Info.Level);   // guild rank
                Out.Fill(0, 4);
                foreach (Guild_member member in officers)
                {
                    Out.WriteUInt32(member.CharacterId);
                    Out.WriteByte(0x00);
                    Out.WriteByte((byte)(CharMgr.GetCharacter(member.CharacterId, false).Name.Length + 1));
                    Out.WriteStringBytes((CharMgr.GetCharacter(member.CharacterId, false).Name));
                    Out.WriteByte(0x00);
                    Out.WriteByte((byte)(member.RankId == 9 ? 4 : 3));   // rank  3 alli officer   4 alli leader
                    Out.WriteByte(0x00);
                    Out.WriteByte(0x69);  // no clue yet
                }
                if (Plr != null)
                    Plr.SendPacket(Out);
                else
                {
                    foreach (uint alli in Alliance.Alliances[Info.AllianceId].Members)
                    {
                        Guild gli = GetGuild(alli);
                        foreach (Player plr in gli.OnlineMembers)
                        {
                            plr.SendPacket(Out);
                        }
                    }
                }
            }
            if (Plr != null)
                SendAlliancePlayerCount(Plr);
        }

        public void SendAlliancePlayerCount(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x09);
            Out.WriteByte((byte)Alliance.Alliances[Info.AllianceId].Members.Count);
            foreach (uint alli in Alliance.Alliances[Info.AllianceId].Members)
            {
                Guild gl = GetGuild(alli);
                if (gl == null)
                {
                    continue;
                }
                Out.WriteUInt32(gl.Info.GuildId);
                Out.WriteUInt32((UInt32)(gl.Info.Members.Count - gl.OnlineMembers.Count));  // offline members without online one
                Out.WriteUInt32((UInt32)gl.OnlineMembers.Count);  //online members
            }
            plr.SendPacket(Out);
        }

        #region event

        public void CreateEvent(uint player, uint begin, uint end, string name, string description, byte alliance, byte locked)
        {
            lock (Info.Event)
            {
                if (Info.Event.Count >= MaxEvents)
                    return;

                byte freeslot = 1;
                Guild_event val;

                for (byte i = 1; i < MaxEvents + 1; i++)
                {
                    if (!Info.Event.TryGetValue(i, out val))
                    {
                        freeslot = i;
                        break;
                    }
                }


                Guild_event gev = new Guild_event();
                gev.SlotId = freeslot;
                gev.GuildId = Info.GuildId;
                gev.CharacterId = player;
                gev.Begin = begin;
                gev.End = end;
                gev.Name = name;
                gev.Description = description;
                gev.Alliance = alliance == 1 ? true : false;
                gev.Locked = locked == 1 ? true : false;

                Info.Event.Add(freeslot, gev);

                CharMgr.Database.AddObject(gev);

                SendEvents(null);
            }
        }

        public void DeleteEvent(Player plr, byte key, uint guildid)
        {
            lock (Info.Event)
            {
                Guild_event events;

                if (Info.Event.TryGetValue(key, out events))

                    if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_ALL_EVENTS) && !(HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_YOUR_EVENTS) && plr.CharacterId == Info.Event[key].CharacterId))
                    {
                        plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                        return;
                    }

                CharMgr.Database.DeleteObject(events);

                Info.Event.Remove(key);
            }
            SendEvents(null);
        }

        public void EventSignup(Player plr, byte key, uint guildid)
        {
            lock (Info.Event)
            {
                Guild_event events;

                if (!Info.Event.TryGetValue(key, out events))
                    return;

                if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_CALENDAR_SIGNUP))
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                    return;
                }
                bool notsightup = true;
                foreach (KeyValuePair<uint, bool> sign in events._Signups)
                {
                    if (sign.Key == plr.CharacterId)
                    {
                        notsightup = false;
                        break;
                    }
                }

                if (notsightup)
                {
                    events._Signups.Add(new KeyValuePair<uint, bool>(plr.CharacterId, false));

                    SendEvents(null);
                }
                events.Dirty = true;
                CharMgr.Database.SaveObject(events);
            }
        }

        public void EventSignupCancel(Player plr, byte key, uint guildid)
        {
            lock (Info.Event)
            {
                Guild_event events;

                if (!Info.Event.TryGetValue(key, out events))
                    return;

                if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_CALENDAR_SIGNUP))
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                    return;
                }

                for (int i = 0; i < events._Signups.Count; i++)
                {
                    if (events._Signups[i].Key == plr.CharacterId)
                        events._Signups.RemoveAt(i);
                }
                events.Dirty = true;
                CharMgr.Database.SaveObject(events);

                SendEvents(null);

            }
        }

        public void EventSignupKick(Player plr, byte key, uint guildid, uint characterid)
        {
            lock (Info.Event)
            {
                Guild_event events;

                if (!Info.Event.TryGetValue(key, out events))
                    return;

                if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_ALL_EVENTS) && !(HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_YOUR_EVENTS) && plr.CharacterId == Info.Event[key].CharacterId))
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                    return;
                }

                for (int i = 0; i < events._Signups.Count; i++)
                {
                    if (events._Signups[i].Key == characterid)
                        events._Signups.RemoveAt(i);
                }
                events.Dirty = true;
                CharMgr.Database.SaveObject(events);

                SendEvents(null);

            }
        }

        public void EventEdit(Player plr, byte eventid, uint guildid, uint begin, uint end, string name, string description, byte alliance, byte locked)
        {
            lock (Info.Event)
            {
                Guild_event events;

                if (!Info.Event.TryGetValue(eventid, out events))
                    return;

                if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_ALL_EVENTS) && !(HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_YOUR_EVENTS) && plr.CharacterId == Info.Event[eventid].CharacterId))
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                    return;
                }
                events.Begin = begin;
                events.Begin = begin;
                events.End = end;
                events.Name = name;
                events.Description = description;
                events.Alliance = alliance == 1 ? true : false;
                events.Locked = locked == 1 ? true : false;

                CharMgr.Database.SaveObject(events);
            }

            SendEvents(null);
        }

        public void EventSignupApproved(Player plr, byte key, uint guildid, uint charId)
        {
            lock (Info.Event)
            {
                Guild_event events;

                if (!Info.Event.TryGetValue(key, out events))
                    return;

                if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_ALL_EVENTS) && !(HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_YOUR_EVENTS) && plr.CharacterId == Info.Event[key].CharacterId))
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                    return;
                }

                for (int i = 0; i < events._Signups.Count; i++)
                {
                    if (events._Signups[i].Key == charId)
                    {
                        if (events._Signups[i].Value)
                            events._Signups[i] = new KeyValuePair<uint, bool>(charId, false);
                        else
                            events._Signups[i] = new KeyValuePair<uint, bool>(charId, true);
                    }
                }
                CharMgr.Database.SaveObject(events);
                SendEvents(null);

            }
        }

        public void SendEvents(Player Plr)
        {
            LogGuildBug(Plr);
            List<Guild_event> allievents = new List<Guild_event>();

            if (Info.AllianceId > 0)
                foreach (uint alli in Alliance.Alliances[Info.AllianceId].Members)
                {
                    if (alli == Info.GuildId)
                        continue;

                    Guild gl = GetGuild(alli);

                    foreach (KeyValuePair<byte, Guild_event> evn in gl.Info.Event)
                    {
                        if (evn.Value.Alliance)
                        {
                            allievents.Add(evn.Value);
                        }
                    }
                }

            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x10);
            Out.WriteByte((byte)(Info.Event.Count + allievents.Count));
            Out.WriteByte(0);
            foreach (KeyValuePair<byte, Guild_event> evn in Info.Event)
            {
                Out.WriteByte(evn.Key);
                Out.WriteUInt32(Info.GuildId);
                Out.WriteUInt32(evn.Value.CharacterId);
                Out.WriteUInt32R(evn.Value.Begin);
                Out.WriteUInt32R(evn.Value.End);
                Out.WriteByte((byte)(evn.Value.Alliance ? 1 : 0));
                Out.WriteByte((byte)(evn.Value.Locked ? 1 : 0));
                Out.WriteByte(0);
                Out.WritePascalString(evn.Value.Name);
                Out.WriteByte(0);
                Out.WritePascalString(evn.Value.Description);
                Out.WriteByte((byte)evn.Value._Signups.Count);
                foreach (KeyValuePair<uint, bool> plr in evn.Value._Signups)
                {
                    Out.WriteUInt32(plr.Key);
                    Out.WriteByte((byte)(plr.Value ? 1 : 0));
                }
                Out.WriteByte(0);
            }
            if (Info.AllianceId > 0)
                foreach (Guild_event evn in allievents)
                {
                    Out.WriteByte(evn.SlotId);
                    Out.WriteUInt32(evn.GuildId);
                    Out.WriteUInt32(evn.CharacterId);
                    Out.WriteUInt32R(evn.Begin);
                    Out.WriteUInt32R(evn.End);
                    Out.WriteByte((byte)(evn.Alliance ? 1 : 0));
                    Out.WriteByte((byte)(evn.Locked ? 1 : 0));
                    Out.WriteByte(0);
                    Out.WritePascalString(evn.Name);
                    Out.WriteByte(0);
                    Out.WritePascalString(evn.Description);
                    Out.WriteByte((byte)evn._Signups.Count);
                    foreach (KeyValuePair<uint, bool> plr in evn._Signups)
                    {
                        Out.WriteUInt32(plr.Key);
                        Out.WriteByte((byte)(plr.Value ? 1 : 0));
                    }
                    Out.WriteByte(0);
                }


            if (Plr == null)
            {

                foreach (Player plro in OnlineMembers)
                {
                    plro.SendPacket(Out);
                }
            }
            else
            {
                Plr.SendPacket(Out);

            }

        }
        #endregion

        public void SendGuildVault(Player plr)
        {
            LogGuildBug(plr);
            if (Info.Level < 3)
                return;

            GuildVaultUser.Add(plr);
            SendVaultUpdate(plr);
        }

        public void SendVaultUpdate()
        {
            foreach (Player plr in GuildVaultUser)
                SendVaultUpdate(plr);
        }

        public void SendVaultUpdate(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x18);

            byte vaults = 0;
            if (Info.Level >= 3 || Info.guildvaultpurchased[0] > 0)
                vaults++;
            if (Info.Level >= 11 || Info.guildvaultpurchased[1] > 0)
                vaults++;
            if (Info.Level >= 23 || Info.guildvaultpurchased[2] > 0)
                vaults++;
            if (Info.Level >= 33 || Info.guildvaultpurchased[3] > 0)
                vaults++;
            vaults++;
            Out.WriteByte(vaults);
            Out.WriteUInt64(Info.Money);
            for (byte i = 0; i < 5; i++)
            {
                bool hide = false;
                Out.WriteByte(0);

                if (i == 0 && !HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_VAULT1_VIEW))
                {
                    hide = true;
                }
                if (i == 1 && !HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_VAULT2_VIEW))
                {
                    hide = true;
                }
                if (i == 2 && !HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_VAULT3_VIEW))
                {
                    hide = true;
                }
                if (i == 3 && !HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_VAULT4_VIEW))
                {
                    hide = true;
                }
                if (i == 4 && !HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_VAULT5_VIEW))
                {
                    hide = true;
                }
                if (vaults < i + 1)
                    hide = true;
                if (hide)
                {
                    Out.WriteByte(0);
                    Out.WriteInt32R(0);
                }
                else
                {
                    Out.WriteByte(Info.guildvaultpurchased[i]);
                    if (Info.guildvaultpurchased[i] == 60)
                        Out.WriteInt32R(0);
                    else if (Info.guildvaultpurchased[i] < 60 && Info.guildvaultpurchased[i] == 0)
                    {
                        switch (i)
                        {
                            case 0: Out.WriteInt32R(100000); break;
                            case 1: Out.WriteInt32R(1000000); break;
                            case 2: Out.WriteInt32R(6000000); break;
                            case 3: Out.WriteInt32R(12000000); break;
                            case 4: Out.WriteInt32R(20000000); break;
                        }
                    }

                    else
                        switch (i)
                        {
                            case 0: Out.WriteInt32R(20000 * (i * 6 + ((Info.guildvaultpurchased[i]) / 10))); break;
                            case 1: Out.WriteInt32R(100000 * (i * 6 + ((Info.guildvaultpurchased[i]) / 10))); break;
                            case 2: Out.WriteInt32R(200000 * (i * 6 + ((Info.guildvaultpurchased[i]) / 10))); break;
                            case 3: Out.WriteInt32R(400000 * (i * 6 + ((Info.guildvaultpurchased[i]) / 10))); break;
                            case 4: Out.WriteInt32R(800000 * (i * 6 + ((Info.guildvaultpurchased[i]) / 10))); break;
                        }

                }

            }

            plr.SendPacket(Out);


            #region sendvaults

            for (byte i = 0; i < 5; i++)
            {
                if (Info.Vaults[i].Count > 0)
                {
                    Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
                    Out.WriteByte(0x1A);
                    Out.WriteByte(1);
                    Out.WriteByte((byte)(i + 1));
                    Out.WriteByte(0);
                    Out.WriteByte((byte)Info.Vaults[i].Count);
                    Out.WriteByte(0);
                    foreach (KeyValuePair<ushort, GuildVaultItem> gvi in Info.Vaults[i])
                    {
                        Out.WriteByte((byte)gvi.Key);
                        Item.BuildItem(ref Out, null, null, new MailItem(gvi.Value.Entry, gvi.Value._Talismans, gvi.Value.PrimaryDye, gvi.Value.SecondaryDye, gvi.Value.Counts), 0, 0);
                        Out.WriteByte(0);
                    }
                    plr.SendPacket(Out);
                }
            }

            #endregion
            /*
                        Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
                        Out.WritePacketString(@" | 1D 00 00 00 00                         |........        |");

                            Plr.SendPacket(Out);
                            */
        }

        public void SendGuildTactics(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WritePacketString(@"|0C 48 38 D1 00 00 38 D2 38 D1 38 D3 38 |...H8...8.8.8.8|
|D2 38 D4 00 00 38 D5 38 D4 38 D6 38 D5 38 D7 00 |.8...8.8.8.8.8..|
|00 38 D8 38 D7 38 D9 38 D8 38 DA 00 00 38 DB 38 |.8.8.8.8.8...8.8|
|DA 38 DC 38 DB 38 DD 00 00 38 DE 38 DD 38 DF 38 |.8.8.8...8.8.8.8|
|DE 38 E0 00 00 38 E1 38 E0 38 E2 38 E1 38 E3 00 |.8...8.8.8.8.8..|
|00 38 E4 38 E3 38 E5 38 E4 38 E6 00 00 38 E7 38 |.8.8.8.8.8...8.8|
|E6 38 E8 38 E7 38 E9 00 00 38 EA 38 E9 38 EB 38 |.8.8.8...8.8.8.8|
|EA 38 EC 00 00 38 ED 38 EC 38 EE 38 ED 38 EF 00 |.8...8.8.8.8.8..|
|00 38 F0 38 EF 38 F1 38 F0 38 F2 00 00 38 F3 38 |.8.8.8.8.8...8.8|
|F2 38 F4 38 F3 38 F5 00 00 38 F6 38 F5 38 F7 38 |.8.8.8...8.8.8.8|
|F6 38 F8 00 00 38 F9 38 F8 38 FA 38 F9 38 FB 00 |.8...8.8.8.8.8..|
|00 38 FC 38 FB 38 FD 38 FC 38 FE 00 00 38 FF 38 |.8.8.8.8.8...8.8|
|FE 39 00 38 FF 39 33 00 00 39 34 39 33 39 35 39 |.9.8.93..9493959|
|34 39 36 00 00 39 37 39 36 39 38 39 37 39 39 00 |496..9796989799.|
|00 39 3A 39 39 39 3B 39 3A 39 3C 00 00 39 3D 39 |.9:999;9:9<..9=9|
|3C 39 3E 39 3D 39 3F 00 00 39 40 39 3F 39 41 39 |<9>9=9?..9@9?9A9|
|40 39 42 00 00 39 43 39 42 39 44 39 43 39 45 00 |@9B..9C9B9D9C9E.|
|00 39 46 39 45 39 47 39 46 39 48 00 00 39 49 39 |.9F9E9G9F9H..9I9|
|48 39 4A 39 00                                  |.....           |");

            plr.SendPacket(Out);
        }

        public void SendGuildTacticsPurchased(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x0B);
            Out.WriteByte(0);
            Out.WriteByte(0x6);

            byte numberofspells = 0;
            for (byte i = 5; i < 40; i++)
            {
                if (Info.GuildTacticsPurchased[i] != 0)
                    numberofspells++;
            }

            bool first = true;
            byte why = 0;
            for (byte i = 5; i < 40; i++)
            {
                if (Info.GuildTacticsPurchased[i] == 0)
                    continue;
                Out.WriteByte(first ? numberofspells : (byte)(why == 2 ? 0 : 1));  // ?? 
                Out.WriteUInt16(Info.GuildTacticsPurchased[i]);    // spell id 
                Out.WriteByte(i);           // tactic slot
                if (first)
                {
                    first = false;
                    continue;
                }
                why++;
                if (why == 3)
                    why = 0;
            }
            Out.WriteByte(0);


            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);

        }

        public void GuildsTacticRespec(Player plr)
        {
            LogGuildBug(plr);
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_BANNER_MANAGEMENT))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }


            uint money = 0;

            for (int i = 0; i < 40; i++)
            {
                if (Info.GuildTacticsPurchased[i] > 0)
                {
                    money += 100000;
                }
            }
            if (plr.HasMoney(money))
            {
                plr.RemoveMoney(money);

                Info.GuildTacticsPurchased = new ushort[40];
                _banners[0, 1] = 0;
                _banners[0, 2] = 0;
                _banners[0, 3] = 0;
                _banners[1, 1] = 0;
                _banners[1, 2] = 0;
                _banners[1, 3] = 0;
                _banners[2, 1] = 0;
                _banners[2, 2] = 0;
                _banners[2, 3] = 0;
                Info.Banners = _banners[0, 0] + "," + _banners[0, 1] + "," + _banners[0, 2] + "," + _banners[0, 3] + ";" + _banners[1, 0] + "," + _banners[1, 1] + "," + _banners[1, 2] + "," + _banners[1, 3] + ";" + _banners[2, 0] + "," + _banners[2, 1] + "," + _banners[2, 2] + "," + _banners[2, 3];
                CharMgr.Database.SaveObject(Info);
                SendGuildTacticsPurchased(null);
            }
            else
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.ERROR_CAREER_PACKAGE_NOT_ENOUGH_MONEY);
        }

        public void TrainGuildTactics(byte slot, ushort spell)
        {
            Info.GuildTacticsPurchased[slot] = spell;
            CharMgr.Database.SaveObject(Info);
            SendGuildTacticsPurchased(null);
        }

        public void SendGuildHeraldry(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x13);
            Out.WriteByte(0);
            BuildHeraldry(Out);
            Out.WriteByte(0);
            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);
        }

        public void BuildHeraldry(PacketOut Out)
        {
            if (Info.Level < 20 && Info.Realm == 1)
                Out.WriteUInt16(1);  // chaos
            else if (Info.Level < 20 && Info.Realm == 2)
                Out.WriteUInt16(2);  // chaos
            else
                Out.WriteUInt16(_heraldryEmblem);
            Out.WriteUInt16(_heraldryPattern);
            Out.WriteByte(_heraldryColor1);
            Out.WriteByte(_heraldryColor2);
            Out.WriteByte(_heraldryShape);
        }

        public void SendGuildBanner(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x0D);
            Out.WriteByte(0);
            Out.WriteByte(0x10);   //?? 10
            Out.WriteByte(0);
            Out.WriteByte(0x1F);   //??1f
            Out.WriteByte(0x4A);   //??4a


            if (Info.Level >= 30)
                Out.WriteByte(3);
            else if (Info.Level >= 18)
                Out.WriteByte(2);
            else if (Info.Level >= 5)
                Out.WriteByte(1);
            else
                Out.WriteByte(0); // standart unlock 1 lvl 5 2 lvl 18 3 lvl 30

            // banner 1
            Out.WriteByte(1);
            if (Info.Level >= 13)
                Out.WriteByte(3);
            else if (Info.Level >= 9)
                Out.WriteByte(2);
            else if (Info.Level >= 5)
                Out.WriteByte(1);
            else
                Out.WriteByte(0);
            Out.WriteByte(0);   // trophy slot not used

            // banner 2
            Out.WriteByte(2);
            if (Info.Level >= 26)
                Out.WriteByte(3);
            else if (Info.Level >= 22)
                Out.WriteByte(2);
            else if (Info.Level >= 18)
                Out.WriteByte(1);
            else
                Out.WriteByte(0);
            Out.WriteByte(0);   // trophy slot not used
            // banner 2
            Out.WriteByte(3);
            if (Info.Level >= 38)
                Out.WriteByte(3);
            else if (Info.Level >= 34)
                Out.WriteByte(2);
            else if (Info.Level >= 30)
                Out.WriteByte(1);
            else
                Out.WriteByte(0);
            Out.WriteByte(0);   // trophy slot not used

            // avaible posts
            if (Info.Level >= 31)
                Out.WriteByte(3);
            else if (Info.Level >= 19)
                Out.WriteByte(2);
            else if (Info.Level >= 5)
                Out.WriteByte(1);

            Out.WriteByte(0);

            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);


            Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x0E);
            Out.WriteByte(0);
            Out.WriteByte(3);
            for (byte i = 0; i < 3; i++)
            {
                Out.WriteByte((byte)(i + 1));
                Out.WriteByte((byte)_banners[i, 0]); // post

                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte((byte)(_bannerlock[i] + 86400000 < TCPManager.GetTimeStampMS() ? 0 : 1));   // 24 h locked
                //Log.Info("", "" + TCPManager.GetTimeStampMS());

                Out.WriteByte(3);
                Out.Fill(0, 2);
                Out.WriteUInt16(_banners[i, 1]);
                Out.Fill(0, 2);
                Out.WriteUInt16(_banners[i, 2]);
                Out.Fill(0, 2);
                Out.WriteUInt16(_banners[i, 3]);
                Out.WriteByte(1);
                Out.Fill(0, 4);
            }
            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);


            /*
            Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WritePacketString(@"|15 01 00                              |......          |");
            if (Plr == null)
                SendToGuild(Out);
            else
                Plr.SendPacket(Out);

            Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WritePacketString(@"|10 02 00                                 |.....           |");
            if (Plr == null)
                SendToGuild(Out);
            else
                Plr.SendPacket(Out);
             */
        }

        private int _bannerReserveAttemptTime;

        public void ReserveBanner(Player plr, ushort emblem, ushort pattern, byte color1, byte color2, byte shape)
        {
            int curTime = TCPManager.GetTimeStamp();

            if (curTime - _bannerReserveAttemptTime > 30)
            {
                _bannerReserveAttemptTime = curTime;
                plr.SendClientMessage("You are attempting to change your guild heraldry. This action will cost you 100 Gold to perform. Please repeat the action after at least 5 seconds and no more than 30 have passed to confirm it.");
                return;
            }

            if (curTime - _bannerReserveAttemptTime < 5)
            {
                plr.SendClientMessage($"You have attempted to confirm a change to your guild heraldry too soon. Please wait { 5 - (curTime - _bannerReserveAttemptTime)} seconds.");
                return;
            }

            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_HERALDRY))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            uint money = 0;
            Character tmp = CharMgr.GetCharacter(Info.LeaderId, false);
            if ((tmp.Realm == 1 && _heraldryEmblem == 1 && _heraldryPattern == 1 && _heraldryColor1 == 1 && _heraldryColor2 == 1 && _heraldryShape == 1) || (tmp.Realm == 2 && _heraldryEmblem == 2 && _heraldryPattern == 100 && _heraldryColor1 == 2 && _heraldryColor2 == 2 && _heraldryShape == 1))
                money = 100000;
            else
                money = 1000000;

            if (plr.HasMoney(money))
            {

                _heraldryEmblem = emblem;
                _heraldryPattern = pattern;
                _heraldryColor1 = color1;
                _heraldryColor2 = color2;
                _heraldryShape = shape;
                Info.Heraldry = emblem + ";" + pattern + ";" + color1 + ";" + color2 + ";" + shape;
                CharMgr.Database.SaveObject(Info);
                SendGuildHeraldry(null);
                plr.RemoveMoney(money);
            }
            else
            {
                plr.SendMessage(0, "", "Not enough Money you need: " + (money / 10000) + " Gold ", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
            }
        }

        public void SaveBanner(byte banner, byte post, ushort spell1, ushort spell2, ushort spell3)
        {
            _banners[banner, 0] = post;
            _banners[banner, 1] = spell1;
            _banners[banner, 2] = spell2;
            _banners[banner, 3] = spell3;
            _bannerlock[banner] = TCPManager.GetTimeStampMS();
            Info.Banners = _banners[0, 0] + "," + _banners[0, 1] + "," + _banners[0, 2] + "," + _banners[0, 3] + ";" + _banners[1, 0] + "," + _banners[1, 1] + "," + _banners[1, 2] + "," + _banners[1, 3] + ";" + _banners[2, 0] + "," + _banners[2, 1] + "," + _banners[2, 2] + "," + _banners[2, 3];
            CharMgr.Database.SaveObject(Info);
            SendGuildBanner(null);
        }

        public uint[] GetBannerBuffs(byte banner)
        {
            uint[] buffs = new uint[3];
            buffs[0] = _banners[banner, 1];
            buffs[1] = _banners[banner, 2];
            buffs[2] = _banners[banner, 3];

            return buffs;
        }

        public byte GetBannerPost(byte banner)
        {
            return (byte)_banners[banner, 0];
        }


        public void SendGuildLog(Guild_log log, bool all, Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x0F);
            Out.WriteByte(1);
            Out.WriteUInt32(log.Time);
            Out.WriteByte(log.Type);
            Out.WriteByte(0);
            Out.WriteStringToZero(log.Text);
            Out.WriteHexStringBytes("00 01 00 00 00 00 00 00 00 00 00".Replace(" ", ""));

            if (all)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);
        }

        public void SendGuildRecruitment(Player plr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x14); // 20 - Guild Recruitment Info
            Out.WriteByte(1); // Count?

            BuildGuild(ref Out, this);

            if (plr == null)
                SendToGuild(Out);
            else
                plr.SendPacket(Out);
        }

        public void SendGuildPlayerContributed(Player plr, Guild_member guildPlr)
        {
            LogGuildBug(plr);
            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x21);
            Out.WriteByte(1);
            Out.WriteUInt64(guildPlr.RenownContributed);
            Out.WriteUInt64(guildPlr.TitheContributed);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt32(guildPlr.JoinDate);
            plr.SendPacket(Out);
        }

        #endregion

        #region Guild Vault

        public bool CanAddTo(Player player, int vaultIndex)
        {
            LogGuildBug(player);
            GuildPermissions permission = GuildPermissions.GUILDPERMISSONS_VAULT1_ADD_ITEM;

            switch (vaultIndex)
            {
                case 1:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT1_ADD_ITEM;
                    break;
                case 2:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT2_ADD_ITEM;
                    break;
                case 3:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT3_ADD_ITEM;
                    break;
                case 4:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT4_ADD_ITEM;
                    break;
                case 5:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT5_ADD_ITEM;
                    break;
            }

            if (!HasPermissions(Info.Ranks[Info.Members[player.CharacterId].RankId], permission))
            {
                player.SendLocalizeString(new[] { Info.Ranks[Info.Members[player.CharacterId].RankId].Name, vaultIndex.ToString() }, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_RANK_X_NO_DEPOSIT_VAULT_Y);
                return false;
            }

            return true;
        }

        public bool CanTakeFrom(Player player, int vaultIndex)
        {
            LogGuildBug(player);
            GuildPermissions permission = GuildPermissions.GUILDPERMISSONS_VAULT1_TAKE_ITEM; ;

            switch (vaultIndex)
            {
                case 1:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT1_TAKE_ITEM;
                    break;
                case 2:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT2_TAKE_ITEM;
                    break;
                case 3:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT3_TAKE_ITEM;
                    break;
                case 4:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT4_TAKE_ITEM;
                    break;
                case 5:
                    permission = GuildPermissions.GUILDPERMISSONS_VAULT5_TAKE_ITEM;
                    break;
            }

            if (!HasPermissions(Info.Ranks[Info.Members[player.CharacterId].RankId], permission))
            {
                player.SendLocalizeString(new[] { Info.Ranks[Info.Members[player.CharacterId].RankId].Name, vaultIndex.ToString() }, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_RANK_X_NO_WITHDRAW_VAULT_Y);
                return false;
            }

            return true;
        }

        public void BuyVaultSlot(Player plr, byte vault, uint money)
        {
            LogGuildBug(plr);
            if (!plr.RemoveMoney(money))
            {
                plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_EXPANSION_NO_MONEY);
                return;
            }

            Info.guildvaultpurchased[vault - 1] += 10;
            SendVaultUpdate();
            CharMgr.Database.SaveObject(Info);
        }

        public void DepositMoney(Player plr, uint money)
        {
            LogGuildBug(plr);
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_VAULT_DEPOSIT))
            {
                plr.SendLocalizeString(Info.Ranks[Info.Members[plr.CharacterId].RankId].Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_RANK_X_NO_DEPOSIT_MONEY);
                return;
            }

            if (!plr.HasMoney(money))
            {
                plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_PLAYER_OVERDRAFT);
                return;
            }

            if (Info.Money + money >= ulong.MaxValue)
            {
                plr.SendLocalizeString(money.ToString(), ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_NO_ROOM_FOR_X_MONEY);
                return;
            }

            plr.RemoveMoney(money);
            Info.Money += money;
            CharMgr.Database.SaveObject(Info);
            SendVaultUpdate();
        }

        public void WithdrawMoney(Player plr, uint money)
        {
            LogGuildBug(plr);
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_VAULT_WITHDRAW))
            {
                plr.SendLocalizeString(Info.Ranks[Info.Members[plr.CharacterId].RankId].Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_RANK_X_NO_WITHDRAW_MONEY);
                return;
            }

            if (Info.Money < money)
            {
                plr.SendLocalizeString(money.ToString(), ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_OVERDRAFT_X_MONEY);
                return;
            }

            Info.Money -= money;
            plr.AddMoney(money);
            CharMgr.Database.SaveObject(Info);

            SendVaultUpdate();
        }

        public void MoveVaultItem(Player plr, byte sourceVault, ushort sourceSlot, byte destVault, ushort destSlot)
        {
            LogGuildBug(plr);
            if (!CanTakeFrom(plr, sourceVault) || !CanAddTo(plr, destVault))
                return;

            lock (Info.Vaults)
            {
                if (Info.Vaults[destVault - 1].ContainsKey(destSlot))
                {
                    plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_SLOT_ALREADY_OCCUPIED);
                    return;
                }

                GuildVaultItem gv;

                if (!Info.Vaults[sourceVault - 1].TryGetValue(sourceSlot, out gv))
                {
                    plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_SLOT_EMPTY);
                    return;
                }

                Info.Vaults[sourceVault - 1].Remove(sourceSlot);
                CharMgr.Database.DeleteObject(gv);

                GuildVaultItem newItem = new GuildVaultItem
                {
                    Counts = gv.Counts,
                    GuildId = Info.GuildId,
                    VaultId = destVault,
                    SlotId = destSlot,
                    Entry = gv.Entry,
                    PrimaryDye = gv.PrimaryDye,
                    SecondaryDye = gv.SecondaryDye,
                    _Talismans = gv._Talismans
                };

                Info.Vaults[destVault - 1].Add(destSlot, newItem);
                CharMgr.Database.AddObject(newItem);
            }

            SendVaultUpdate();
        }

        public void DepositVaultItem(Player plr, byte destVault, ushort destSlot, ushort itemSlot)
        {

            LogGuildBug(plr);

            Item item = plr.ItmInterface.GetItemInSlot(itemSlot);

            if (item == null)
                return;

            if (item.Info.Bind == 1 || item.BoundtoPlayer)
            {
                plr.SendLocalizeString(item.Info.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ITEM_NO_MOVE_GUILD_VAULT);
                return;
            }

            if (!CanAddTo(plr, destVault))
                return;

            lock (Info.Vaults)
            {
                bool freeslot = false;
                if (destSlot == (int)Inventory.INVENTORY_FIRST_AVAILABLE_GUILD_VAULT_SLOT)
                {
                    for (byte i = 0; i < 5 && !freeslot; i++)
                    {
                        if (Info.guildvaultpurchased[i] == 0)
                            continue;

                        destVault = (byte)(i + 1);
                        for (byte y = 0; y < Info.guildvaultpurchased[i] && !freeslot; y++)
                        {
                            bool taken = false;
                            foreach (KeyValuePair<ushort, GuildVaultItem> gvl in Info.Vaults[destVault - 1])
                                if (y == gvl.Key)
                                {
                                    taken = true;
                                    break;
                                }
                            if (!taken)
                            {
                                destSlot = y;
                                freeslot = true;
                            }
                        }
                    }

                }
                if (destSlot == (int)Inventory.INVENTORY_FIRST_AVAILABLE_GUILD_VAULT_SLOT)
                    return;

                if (Info.Vaults[destVault - 1].ContainsKey(destSlot))
                {
                    plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_SLOT_ALREADY_OCCUPIED);
                    return;
                }

                GuildVaultItem gv = new GuildVaultItem
                {
                    Counts = item.Count,
                    GuildId = Info.GuildId,
                    VaultId = destVault,
                    SlotId = destSlot,
                    Entry = item.Info.Entry,
                    PrimaryDye = item.GetPrimaryDye(),
                    SecondaryDye = item.GetSecondaryDye(),
                    _Talismans = item.GetTalismans()
                };

                Info.Vaults[destVault - 1].Add(destSlot, gv);
                CharMgr.Database.AddObject(gv);
                plr.ItmInterface.DeleteItem(itemSlot, item.Count);
            }
            SendVaultUpdate();
        }

        private void LogGuildBug(Player plr)
        {
            if (plr == null)
                return;


   //         if (plr.Name.Contains("arena") || plr.Name.Contains("poko") || plr.Name.Contains("bram") || plr.Name.Contains("zaru") || plr.Name.Contains("niffils") ||
   //             plr.Name.Contains("ikdorf") || plr.Name.Contains("grimjob")
			//	|| plr.GldInterface.GetGuildName().Contains("Afk"))
   //         {
   //             var l_CurrentStack = new System.Diagnostics.StackTrace(true);

   //             GuildLogger.Debug($"{plr.Name} {l_CurrentStack.ToString()}");
			//	GuildLogger.Debug($"{plr.Name} Initialized= {plr.Initialized.ToString()}");
			//}

        }

        public void WithdrawVaultItem(Player plr, byte sourceVault, ushort sourceSlot, ushort itemSlot)
        {
            LogGuildBug(plr);
            if (!CanTakeFrom(plr, sourceVault))
                return;

            lock (Info.Vaults)
            {
                GuildVaultItem gv;

                if (!Info.Vaults[sourceVault - 1].TryGetValue(sourceSlot, out gv))
                {
                    plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_SLOT_EMPTY);
                    return;
                }

                if (gv.LockedPlayerId != plr.CharacterId)
                {
                    plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_VAULT_SLOT_LOCKED);
                    return;
                }

                if (plr.ItmInterface.CreateItem(ItemService.GetItem_Info(gv.Entry), gv.Counts, gv._Talismans, gv.PrimaryDye, gv.SecondaryDye, false, (itemSlot != 600 ? itemSlot : (ushort)0)) == ItemResult.RESULT_OK)
                {
                    Info.Vaults[sourceVault - 1].Remove(sourceSlot);
                    CharMgr.Database.DeleteObject(gv);
                }
            }
            SendVaultUpdate();
        }

        public void LockVaultItem(Player plr, byte vault, byte slot, byte itemslot)
        {
            LogGuildBug(plr);
            if (!CanTakeFrom(plr, vault))
                return;

            GuildVaultItem gv;

            lock (Info.Vaults)
            {
                if (Info.Vaults[vault - 1].TryGetValue(slot, out gv))
                    if (gv.LockedPlayerId == 0)
                        gv.LockedPlayerId = plr.CharacterId;
            }
        }

        public void ReleaseVaultItemLock(Player plr, byte vault, byte slot)
        {
            LogGuildBug(plr);
            GuildVaultItem gv;

            lock (Info.Vaults)
            {
                if (Info.Vaults[vault - 1].TryGetValue(slot, out gv))
                {
                    if (gv.LockedPlayerId == plr.CharacterId)
                        gv.LockedPlayerId = 0;
                }
            }
        }

        public void GuildVaultClosed(Player plr)
        {
            LogGuildBug(plr);
            GuildVaultUser.Remove(plr);
        }

        #endregion

        #region Alliance

        public void JoinAlliance(uint allianceid)
        {
            Info.AllianceId = allianceid;
            CharMgr.Database.SaveObject(Info);
            Alliance.Alliances[allianceid].Members.Add(Info.GuildId);

            foreach (uint alli in Alliance.Alliances[Info.AllianceId].Members)
            {
                Guild gl = GetGuild(alli);
                if (gl == null)
                {
                    continue;
                }
                gl.AddGuildLog((byte)GuildLogEvent.GUILD_EVENT_ALLIANCE_JOIN, Info.Name);
                foreach (Player plr in gl.OnlineMembers)
                    SendAlliance(plr);
            }
        }

        public void FormAlliance(string name, uint guildid)
        {
            uint alliid = Alliance.CreateAlliance(name);

            Info.AllianceId = alliid;
            GetGuild(guildid).Info.AllianceId = alliid;

            Alliance.Alliances[alliid].Members.Add(Info.GuildId);
            Alliance.Alliances[alliid].Members.Add(guildid);

            CharMgr.Database.SaveObject(Info);
            CharMgr.Database.SaveObject(GetGuild(guildid).Info);

            AddGuildLog((byte)GuildLogEvent.GUILD_EVENT_ALLIANCE_JOIN, Info.Name);
            AddGuildLog((byte)GuildLogEvent.GUILD_EVENT_ALLIANCE_JOIN, GetGuild(guildid).Info.Name);

            GetGuild(guildid).AddGuildLog((byte)GuildLogEvent.GUILD_EVENT_ALLIANCE_JOIN, Info.Name);
            GetGuild(guildid).AddGuildLog((byte)GuildLogEvent.GUILD_EVENT_ALLIANCE_JOIN, GetGuild(guildid).Info.Name);

            UpdateAlliance();

        }

        public void UpdateAlliance()
        {
            foreach (uint alli in Alliance.Alliances[Info.AllianceId].Members)
            {
                Guild gl = GetGuild(alli);
                if (gl == null)
                {
                    continue;
                }
                gl.AddGuildLog(3, Info.Name);
                foreach (Player plr in gl.OnlineMembers)
                {

                    SendAlliance(plr);
                }
            }
        }

        public void UpdateAlliance_OnlineCount()
        {

            PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x09);
            Out.WriteByte(1);
            Out.WriteUInt32(Info.GuildId);
            Out.WriteUInt32((UInt32)(Info.Members.Count - OnlineMembers.Count));  // offline members without online one
            Out.WriteUInt32((UInt32)OnlineMembers.Count);  //online members

            foreach (uint alli in Alliance.Alliances[Info.AllianceId].Members)
            {
                Guild gl = GetGuild(alli);
                if (gl == null)
                {
                    continue;
                }
                foreach (Player plr in gl.OnlineMembers)
                    plr.SendPacket(Out);
            }
        }

        public void LeaveAlliance()
        {
            uint allianceid = Info.AllianceId;
            Info.AllianceId = 0;
            CharMgr.Database.SaveObject(Info);

            Alliance.Alliances[allianceid].Members.Remove(Info.GuildId);
            AddGuildLog((byte)GuildLogEvent.GUILD_EVENT_ALLICANCE_LEFT, Info.Name);


            if (Alliance.Alliances[allianceid].Members.Count == 0)
            {
                CharMgr.Database.DeleteObject(Alliance.Alliances[allianceid]);
            }
            else
            {

                PacketOut Out = new PacketOut((byte)Opcodes.F_GUILD_DATA);
                Out.WriteByte(0x08);
                Out.WriteByte(0x01);
                Out.WriteUInt32(Info.GuildId);

                foreach (uint alli in Alliance.Alliances[allianceid].Members)
                {
                    Guild gl = GetGuild(alli);
                    if (gl == null)
                        continue;
                    gl.AddGuildLog((byte)GuildLogEvent.GUILD_EVENT_ALLICANCE_LEFT, Info.Name);
                    foreach (Player plr in gl.OnlineMembers)
                    {
                        plr.SendPacket(Out);
                    }
                }
            }
            PacketOut out1 = new PacketOut((byte)Opcodes.F_GUILD_DATA);
            out1.WriteByte(0x07);
            out1.WriteByte(0x0);
            out1.WriteByte(0x0);

            foreach (Player plr in OnlineMembers)
                plr.SendPacket(out1);

        }

        public void APromote(Player Plr, string text)
        {
            lock (Info.Members)
            {
                Guild_member player;
                byte officers = 0;

                foreach (KeyValuePair<uint, Guild_member> gm in Info.Members)
                {
                    if (gm.Value.AllianceOfficer)
                        officers++;
                }

                if (officers < 5)
                {
                    if (Info.Members.TryGetValue(CharMgr.GetCharacter(Player.AsCharacterName(text), true).CharacterId, out player))
                    {
                        player.AllianceOfficer = true;
                        CharMgr.Database.SaveObject(player);

                        foreach (Player plr in OnlineMembers)
                            SendMember(plr, player);

                        SendAllianceGuilds(null);
                    }
                }
            }

        }

        public void ADemote(Player Plr, string text)
        {
            Guild_member player;
            if (Info.Members.TryGetValue(CharMgr.GetCharacter(Player.AsCharacterName(text), true).CharacterId, out player))
            {
                if (player.AllianceOfficer)
                {
                    player.AllianceOfficer = false;
                    CharMgr.Database.SaveObject(player);
                    foreach (Player plr in OnlineMembers)
                        SendMember(plr, player);
                    SendAllianceGuilds(null);
                }
            }
        }

        #endregion

        #region RosterMangement

        public Player GetGuildPlayer(uint id)
        {
            foreach (Player plr in OnlineMembers)
            {
                if (plr.Info.CharacterId == id)
                    return plr;
            }

            return null;
        }

        public void SendToGuild(PacketOut Out)
        {
            foreach (Player plr in OnlineMembers)
            {
                plr.SendCopy(Out);
            }
        }

        public void AddOnlineMember(Player plr)
        {
            LogGuildBug(plr);
            lock (OnlineMembers)
            {
                OnlineMembers.Add(plr);
                plr.EvtInterface.AddEventNotify(EventName.OnAddXP, OnAddXp);
                plr.EvtInterface.AddEventNotify(EventName.OnAddRenown, OnAddRenown);

                SendMember(null, Info.Members[plr.CharacterId]);

                if (Info.AllianceId > 0)
                    UpdateAlliance_OnlineCount();
            }
        }

        public void RemoveOnlineMember(Player plr)
        {
            LogGuildBug(plr);
            lock (OnlineMembers)
            {
                OnlineMembers.Remove(plr);
                plr.EvtInterface.RemoveEventNotify(EventName.OnAddXP, OnAddXp);
                plr.EvtInterface.RemoveEventNotify(EventName.OnAddRenown, OnAddRenown);

                // This is used on logout
                if (Info.Members.ContainsKey(plr.CharacterId))
                {
                    Info.Members[plr.CharacterId].LastSeen = (uint)TCPManager.GetTimeStamp();
                    CharMgr.Database.SaveObject(Info.Members[plr.CharacterId]);
                    SendMember(null, Info.Members[plr.CharacterId]);
                }
            }

            if (Info.AllianceId > 0)
                UpdateAlliance_OnlineCount();

        }

        public void JoinGuild(Player plr)
        {
            LogGuildBug(plr);
            Guild_member member = new Guild_member
            {
                CharacterId = plr.Info.CharacterId,
                GuildId = plr.GldInterface.Guild.Info.GuildId,
                RankId = 1,
                PublicNote = "",
                OfficerNote = "",
                Member = plr.Info,
                JoinDate = (uint)TCPManager.GetTimeStamp()
            };
            Info.Members.Add(plr.CharacterId, member);
            CharMgr.Database.AddObject(member);

            AddGuildLog(0, plr.Name);
            AddOnlineMember(plr);
            SendGuildInfo(plr);
        }

        public void LeaveGuild(Guild_member member, bool kicked)
        {
            Info.Members.Remove(member.CharacterId);
            CharMgr.Database.DeleteObject(member);

            if (kicked)
                AddGuildLog(2, member.Member.Name);
            else
                AddGuildLog(1, member.Member.Name);

            Player onlinePlr = GetGuildPlayer(member.CharacterId);
            if (onlinePlr != null)
            {
                if (kicked)
                    onlinePlr.SendLocalizeString(Info.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GUILD_PLAYERKICKED);
                else
                    onlinePlr.SendLocalizeString(Info.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_YOU_LEFT_GUILD);
                onlinePlr.GldInterface.Guild.RemoveOnlineMember(onlinePlr);
                onlinePlr.GldInterface.Guild = null;
                SendNullGuild(onlinePlr);

                //check if the player is in vipers pit or sigmars hammer and if so port them out
                if (onlinePlr.ZoneId == 178)
                {
                    onlinePlr.Teleport(161, 442895, 127638, 17353, 0);
                }
                if (onlinePlr.ZoneId == 198)
                {
                    onlinePlr.Teleport(162, 116236, 147801, 14121, 0);
                }

            }
            // if not online set the cords
            if (onlinePlr == null)
            {
                var existingChar = CharMgr.GetCharacter(Player.AsCharacterName(member.Member.Name), false);
                if (existingChar.Value.ZoneId == 178)
                {
                    existingChar.Value.WorldX = 442895;
                    existingChar.Value.WorldY = 127638;
                    existingChar.Value.WorldZ = 17353;
                    existingChar.Value.ZoneId = 161;
                    existingChar.Value.RegionId = 17;

                    CharMgr.Database.SaveObject(existingChar.Value);
                    CharMgr.Database.ForceSave();
                }
                if (existingChar.Value.ZoneId == 198)
                {
                    existingChar.Value.WorldX = 116236;
                    existingChar.Value.WorldY = 147801;
                    existingChar.Value.WorldZ = 14121;
                    existingChar.Value.ZoneId = 162;
                    existingChar.Value.RegionId = 7;

                    CharMgr.Database.SaveObject(existingChar.Value);
                    CharMgr.Database.ForceSave();
                }
            }
            /*
            // Sustain check.
            // If there are less than 6 members in this guild, it folds immediately.
            if (Info.Members.Count < 6)
            {
                foreach (var member1 in Info.Members)
                    CharMgr.Database.DeleteObject(member1.Value);

                CharMgr.DeleteGuild(Info);
                Guilds.Remove(this);

                foreach (Player guildPlr in OnlineMembers)
                {
                    guildPlr.SendClientMessage($"Your guild, {Info.Name}, has failed a sustain check and has been removed.");
                    guildPlr.GldInterface.Guild = null;
                    SendNullGuild(guildPlr);
                }
            }
			*/

            // Assign a new leader!
            else if (member.CharacterId == Info.LeaderId)
            {
                Guild_member newLeader = Info.Members.Count > 1 ? Info.Members.Values.OrderByDescending(m => m.RankId).First() : null;
                if (newLeader != null)
                {
                    newLeader.RankId = 9;
                    Info.LeaderId = newLeader.CharacterId;
                    CharMgr.Database.SaveObject(newLeader);
                    CharMgr.Database.SaveObject(Info);
                    AddGuildLog(11, member.Member.Name);
                }
                else
                {
                    // No one else left :( lets remove the guild
                    CharMgr.DeleteGuild(Info);
                    Guilds.Remove(this);

                    // This shouldn't need to happen
                    foreach (Player guildPlr in OnlineMembers)
                    {
                        guildPlr.GldInterface.Guild = null;
                        SendNullGuild(guildPlr);
                    }
                }
            }

            SendAllMembers(null);
        }

        #endregion

        #region XP/Level
        /// <summary>Timer in minutes between each guild xp/renown push</summary>
        private const int PUSH_TIMER = 30 * 60;

        /// <summary>Timestamp in seconds of the next guild xp push</summary>
        private int _nextXpPushTimestamp = TCPManager.GetTimeStamp();
        /// <summary>Timestamps in seconds of the next member renown contribution save,
        /// indexed by character id</summary>
        private Dictionary<uint, int> _nextMemberRenownSaveTimestamp = new Dictionary<uint, int>();

        /// <summary>Registers xp contribution to the guild</summary>
        public bool OnAddXp(Object obj, object args)
        {
            AddXp((uint)(((uint)(args)) / OnlineMembers.Count));
            return false;
        }

        /// <summary>Registers renown contribution to the guild</summary>
        public bool OnAddRenown(Object obj, object args)
        {
            Player plr = obj.GetPlayer();
            uint amount = (uint)(((uint)(args)) / OnlineMembers.Count);

            Info.Members[plr.CharacterId].RenownContributed += amount;
            Info.Renown += amount;

            PushGuildXp(false);

            // Save data every PUSH_TIMER seconds
            int nextTimestamp;
            _nextMemberRenownSaveTimestamp.TryGetValue(plr.CharacterId, out nextTimestamp);
            int now = TCPManager.GetTimeStamp();
            if (now < nextTimestamp)
                return false;
            _nextMemberRenownSaveTimestamp[plr.CharacterId] = now + PUSH_TIMER;

            CharMgr.Database.SaveObject(Info.Members[plr.CharacterId]);

            return false;
        }

        public void AddXp(uint xp)
        {
            if (Info.Level >= MaxGuildLevel)
                return;

            // Added to stop guilds levelling like crazy due to xp farming / buffer overruns.
            if (xp > 5000)
                return;

            Info.Xp += xp;
            Guild_Xp xpNext = GuildService.GetGuild_Xp((byte)(Info.Level + 1));

            bool force = false;
            if (Info.Xp >= xpNext.Xp)
            {
                Info.Level++;
                if (Info.Level >= 3 && Info.guildvaultpurchased[0] == 0)
                    Info.guildvaultpurchased[0] = 10;
                if (Info.Level >= 11 && Info.guildvaultpurchased[1] == 0)
                    Info.guildvaultpurchased[1] = 10;
                if (Info.Level >= 23 && Info.guildvaultpurchased[2] == 0)
                    Info.guildvaultpurchased[2] = 10;
                if (Info.Level >= 33 && Info.guildvaultpurchased[3] == 0)
                    Info.guildvaultpurchased[3] = 10;
                SendGuildBanner(null);

                CalcMaxStandardBearers();
                force = true;
            }

            PushGuildXp(force);
        }

        public void AddRenown(uint renown)
        {
            Info.Renown += renown;
            PushGuildXp(false);
        }

        /// <summary>
        /// Pushs current guild xp to members and in database.
        /// </summary>
        /// <param name="force">True to force data push (on guild level change for example)</param>
        /// <remarks>Does nothing if previous push </remarks>
        private void PushGuildXp(bool force)
        {
            int now = TCPManager.GetTimeStamp();
            if (!force && now < _nextXpPushTimestamp)
                return;
            _nextXpPushTimestamp = now + PUSH_TIMER;

            CharMgr.Database.SaveObject(Info);
            SendGuildXp(null);
        }

        #endregion

        #region Commands

        public void AddGuildLog(byte type, string text)
        {
            Guild_log log = new Guild_log();
            log.GuildId = Info.GuildId;
            log.Time = (uint)TCPManager.GetTimeStamp();
            log.Type = type;
            log.Text = text;

            Info.Logs.Add(log);
            SendGuildLog(log, true, null);

            CharMgr.Database.AddObject(log);
        }

        public void SetRecruitmentInfo(string briefDescription, string summary, byte playStyle, byte atsmosphere, uint careersNeeded, byte ranksNeeded, byte interests, byte activelyRecruiting)
        {
            Info.BriefDescription = briefDescription;
            Info.Summary = summary;
            Info.PlayStyle = playStyle;
            Info.Atmosphere = atsmosphere;
            Info.CareersNeeded = careersNeeded;
            Info.RanksNeeded = ranksNeeded;
            Info.Interests = interests;
            Info.ActivelyRecruiting = activelyRecruiting;
            CharMgr.Database.SaveObject(Info);

            SendGuildRecruitment(null);
        }

        public void RankToggle(Player plr, byte rankId, bool enable)
        {

            // You cant disable theese
            if (rankId == 0 || rankId == 1 || rankId == 8 || rankId == 9)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_RANK_ERR_NOTOGGLE);
                return;
            }

            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_ENABLE_RANKS))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            Guild_rank rank = Info.Ranks[rankId];

            if (rank == null)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_RANKNUM_MISSING);
                return;
            }

            if (enable)
                rank.Enabled = true;
            else
            {
                foreach (Guild_member guildPlr in Info.Members.Values)
                {
                    if (guildPlr.RankId == rank.RankId)
                    {
                        plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_RANK_ERR_NOTEMPTY);
                        return;
                    }
                }
                rank.Enabled = false;
            }

            CharMgr.Database.SaveObject(rank);
            SendGuildRanks(null);
        }

        public void PromoteMember(Player plr, uint characterId)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_PROMOTE))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }


            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Guild_member member = Info.Members[characterId];

            // Can only promote people who are 2 ranks below you
            if (member.RankId > Info.Members[plr.CharacterId].RankId - 2)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            member.RankId++;

            CharMgr.Database.SaveObject(member);

            SendMember(null, member);
        }

        public void DemoteMember(Player plr, uint characterId)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_DEMOTE))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }


            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Guild_member member = Info.Members[characterId];

            // Can only demote people who are below you
            if (member.RankId >= Info.Members[plr.CharacterId].RankId)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            // Little check
            if (member.RankId > 0)
                member.RankId--;

            CharMgr.Database.SaveObject(member);

            SendMember(null, member);
        }

        public void PlayerKick(Player plr, string name)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_KICK))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            Character Char = CharMgr.GetCharacter(Player.AsCharacterName(name), false);
            uint characterId = Char?.CharacterId ?? 0;
            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Guild_member member = Info.Members[characterId];

            // Can only kick people who are below you
            if (member.RankId >= Info.Members[plr.CharacterId].RankId)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            LeaveGuild(member, true);
        }

        public void AssignLeader(Player plr, uint characterId)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_SET_GUILD_LEADER))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }


            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Guild_member member = Info.Members[characterId];
            Guild_member us = Info.Members[plr.CharacterId];
            // Double check that we are the leader
            if (us.RankId != 9 && Info.LeaderId != us.CharacterId)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            member.RankId = 9;
            Info.LeaderId = member.CharacterId;
            AddGuildLog(11, member.Member.Name);

            us.RankId = 8;

            CharMgr.Database.SaveObject(member);
            CharMgr.Database.SaveObject(us);
            CharMgr.Database.SaveObject(Info);

            SendMember(null, member);
            SendMember(null, us);
        }

        private readonly object _leaderChange = new object();

        public void AssignLeader(Player gameMaster, string newLeaderName)
        {
            lock (_leaderChange)
            {
                Character chara = CharMgr.GetCharacter(newLeaderName, false);

                if (chara == null)
                {
                    gameMaster.SendClientMessage("The requested character does not exist.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }

                Guild_member newLeader;

                // Forced leader assignment, so we will add the character to this guild if required.
                if (!Info.Members.ContainsKey(chara.CharacterId))
                {
                    int isinGuild = CharMgr.Database.GetObjectCount<Guild_member>($"CharacterId={chara.CharacterId}");

                    if (isinGuild > 0)
                    {
                        gameMaster.SendClientMessage("The requested character is in a different guild and must leave that guild first.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return;
                    }

                    // Add the player directly to the guild
                    newLeader = new Guild_member
                    {
                        CharacterId = chara.CharacterId,
                        GuildId = Info.GuildId,
                        RankId = 9,
                        PublicNote = "",
                        OfficerNote = "",
                        Member = chara,
                        JoinDate = (uint)TCPManager.GetTimeStamp()
                    };
                    Info.Members.Add(chara.CharacterId, newLeader);
                    CharMgr.Database.AddObject(newLeader);

                    AddGuildLog(11, chara.Name);
                    SendMember(null, newLeader);

                    Player plr = Player.GetPlayer(chara.CharacterId);

                    if (plr != null)
                    {
                        AddOnlineMember(plr);
                        SendGuildInfo(plr);
                    }
                }

                newLeader = Info.Members[chara.CharacterId];

                // Demote any existing guild leaders (hooray for bugs)
                foreach (Guild_member member in Info.Members.Values)
                {
                    if (member.RankId == 9)
                    {
                        member.RankId = 8;
                        CharMgr.Database.SaveObject(member);
                        SendMember(null, member);
                    }
                }

                newLeader.RankId = 9;
                CharMgr.Database.SaveObject(newLeader);

                Info.LeaderId = newLeader.CharacterId;
                CharMgr.Database.SaveObject(Info);

                AddGuildLog(11, newLeader.Member.Name);

                gameMaster.SendClientMessage("Changed leader of " + Info.Name + " to " + newLeader.Member.Name, ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

                SendMember(null, newLeader);
            }
        }

        public void SetMotd(Player plr, string text)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_PROFILE))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            Info.Motd = text;

            CharMgr.Database.SaveObject(Info);

            SendGuildProfile(null);
        }

        public void SetDetails(Player plr, string text)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_PROFILE))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            Info.AboutUs = text;

            CharMgr.Database.SaveObject(Info);

            SendGuildProfile(null);
        }

        public void SetPublicNote(Player plr, string name, string note)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_ANYONES_PUBLIC_NOTES))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            uint characterId = CharMgr.GetCharacterId(Player.AsCharacterName(name));

            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Guild_member member = Info.Members[characterId];

            member.PublicNote = note;

            CharMgr.Database.SaveObject(member);

            SendMember(null, member);
        }

        public void SetOfficerNote(Player plr, string name, string note)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_ANYONES_OFFICER_NOTE))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            uint characterId = CharMgr.GetCharacterId(Player.AsCharacterName(name));
            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Guild_member member = Info.Members[characterId];

            member.OfficerNote = note;

            CharMgr.Database.SaveObject(member);

            SendMember(null, member);
        }

        public void RecruiterToggle(Player plr, string name)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_SET_RECRUITERS))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            uint characterId = CharMgr.GetCharacterId(Player.AsCharacterName(name));

            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Guild_member member = Info.Members[characterId];

            member.GuildRecruiter = !member.GuildRecruiter;

            CharMgr.Database.SaveObject(member);

            SendMember(null, member);
            SendGuildRecruitment(null);
        }

        public void SetRankName(Player plr, byte rankId, string name)
        {

            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_TITLES))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            Guild_rank rank = Info.Ranks[rankId];

            if (rank == null)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_RANKNUM_MISSING);
                return;
            }

            rank.Name = name;

            CharMgr.Database.SaveObject(rank);
            SendGuildRanks(null);
        }

        public void SetPermissions(Player plr, string text)
        {
            byte rank = (byte)int.Parse(text.Split(' ')[0]);
            int command = int.Parse(text.Split(' ')[1]);

            int Byte = 0;
            int bit = 0;

            HasPermissionsByte(ref Byte, ref bit, (byte)command);
            byte[] permissionBytes = ConvertHexStringToByteArray(Info.Ranks[rank].Permissions.Replace(" ", ""));
            if ((permissionBytes[Byte - 1] & (1 << bit)) == 0)
            {
                //set byte
                permissionBytes[Byte - 1] = (byte)((1 << (bit)) | permissionBytes[Byte - 1]);
            }
            else
            {
                // removebyte
                permissionBytes[Byte - 1] = (byte)(permissionBytes[Byte - 1] & ~(1 << (bit)));
            }
            string hex = BitConverter.ToString(permissionBytes);
            Info.Ranks[rank].Permissions = hex.Replace("-", " ");
            CharMgr.Database.SaveObject(Info.Ranks[rank]);
            SendGuildRanks(null);
        }

        public void SetTax(Player plr, string value)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_EDIT_TAX_RATE))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
            }
            if (Info.Level < 4)
                return;

            if (int.Parse(value) < 0 || int.Parse(value) > 100)
                return;

            Info.Tax = byte.Parse(value);

            CharMgr.Database.SaveObject(Info);
            foreach (Player pl in OnlineMembers)
            {
                SendGuildTax(pl);
            }
        }

        public void SetTithe(Player plr, string value)
        {
            if (Info.Level < 4)
                return;

            if (int.Parse(value) < 0 || int.Parse(value) > 100)
                return;

            Guild_member member = Info.Members[plr.CharacterId];
            member.Tithe = (byte)int.Parse(value);
            CharMgr.Database.SaveObject(member);

            SendMember(null, member);
            SendGuildPlayerContributed(plr, member);

            foreach (Player pl in OnlineMembers)
            {
                SendGuildTax(pl);
            }
        }

        public void CalcMaxStandardBearers()
        {
            if (Info.Level >= 30)
                StandardBearerMax = 6;
            if (Info.Level >= 18)
                StandardBearerMax = 4;
            if (Info.Level >= 5)
                StandardBearerMax = 2;
        }

        public void RemoveStandardBearer(Player plr, string name)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_UNASSIGN_BANNERS))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            uint characterId = CharMgr.GetCharacterId(Player.AsCharacterName(name));

            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            Player plrrem;

            lock (Player._Players)
                Player.PlayersByCharId.TryGetValue(characterId, out plrrem);

            if (plrrem != null && plrrem.WeaponStance == WeaponStance.Standard)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_BANNER_WARNING_ACTIVE);
                return;
            }

            Guild_member member = Info.Members[characterId];

            member.StandardBearer = !member.StandardBearer;

            CharMgr.Database.SaveObject(member);

            SendMember(null, member);
            //SendGuildRecruitment(null);
        }

        public void SetStandardBearer(Player plr, string name)
        {
            if (!HasPermissions(Info.Ranks[Info.Members[plr.CharacterId].RankId], GuildPermissions.GUILDPERMISSONS_ASSIGN_BANNERS))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_GENERAL_NO_PERMISSION);
                return;
            }

            uint characterId = CharMgr.GetCharacterId(Player.AsCharacterName(name));

            if (!Info.Members.ContainsKey(characterId))
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_ERR_MEMBERNOTFOUND);
                return;
            }

            byte beares = 0;

            lock (Info.Members)
            {

                foreach (KeyValuePair<uint, Guild_member> gm in Info.Members)
                {
                    if (gm.Value.StandardBearer)
                        beares++;
                }


                if (beares >= StandardBearerMax)
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_TOO_MANY_BANNER_CARRIERS_ERR);
                    return;
                }

                Guild_member member = Info.Members[characterId];

                member.StandardBearer = !member.StandardBearer;

                CharMgr.Database.SaveObject(member);

                SendMember(null, member);
                //SendGuildRecruitment(null);
            }
        }

        #endregion

        /*
         * This region contains commands for logging guild chat to a file.
         * I've added this as it may be useful to discover the identity of some players who are evading bans.
         */

        #region Interception

        public StreamWriter Logger { get; private set; }

        public void StartLogging()
        {
            if (Logger != null)
                return;

            Directory.SetCurrentDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Logs"));

            string pathString = "GuildLog_" + DateTime.UtcNow.Day + "_" + DateTime.UtcNow.Month + "_" + DateTime.UtcNow.Year + "_" + Info.Name + ".txt";

            Logger = new StreamWriter(pathString.Replace(" ", string.Empty));
        }

        public void LogLine(string line)
        {
            lock (Logger)
            {
                Logger.WriteLine(line);
                Logger.Flush();
            }
        }

        public void EndLogging()
        {
            if (Logger == null)
                return;

            lock (Logger)
            {
                if (Logger == null)
                    return;
                Logger.Dispose();

                Logger = null;
            }
        }
        #endregion
    }
}