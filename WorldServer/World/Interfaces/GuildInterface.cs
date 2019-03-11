using System;
using System.Collections.Generic;
using SystemData;
using FrameWork;
using WorldServer.World.Guild;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class GuildInterface : BaseInterface
    {
        public Guild.Guild Guild;
        public GuildInvitation invite;
        public Guild.Guild invitedTo;
        public uint AllianceinvitedTo;
        public string AllianceFormName;
        public uint AllianceFormGuildId;

        public GuildInterface Load(Guild.Guild Guild)
        {
            this.Guild = Guild;

            if(IsInGuild())
                Guild.AddOnlineMember(_Owner.GetPlayer());

            _Owner.EvtInterface.AddEventNotify(EventName.Leave, OnPlayerLeave, true);

            return this;
        }

        public bool IsInGuild()
        {
            return Guild != null;
        }

        public string GetGuildName()
        {
            return Guild != null ? Guild.Info.Name : "";
        }

        public byte GetGuildLevel()
        {
            return Guild != null ? Guild.Info.Level : (byte)0;
        }

        public bool OnPlayerLeave(Object Sender, object Args)
        {
            if (IsInGuild())
                Guild.RemoveOnlineMember(_Owner.GetPlayer());
            return false;
        
        }

        public void Say(string Message)
        {
            if (IsInGuild())
            {
                lock (Guild.OnlineMembers)
                    foreach (Player player in Guild.OnlineMembers)
                    {
                        if (!player.IsBanned)
                            player.SendMessage(_Owner, Message, ChatLogFilters.CHATLOGFILTERS_GUILD);
                    }

                if (Guild.Logger != null)
                    Guild.LogLine(_Owner.Name + ": " + Message);
            }
        }

        public void OfficerSay(string Message)
        {
            if (IsInGuild() && _Owner.GetPlayer().GldInterface.Guild.Info.Members[_Owner.GetPlayer().CharacterId].RankId >= 8)
            {
                lock (Guild.OnlineMembers)
                    foreach (Player Plr in Guild.OnlineMembers)
                        if (Guild.Info.Members[Plr.CharacterId].RankId >= 8)
                            Plr.SendMessage(_Owner, Message, ChatLogFilters.CHATLOGFILTERS_GUILD_OFFICER);

                if (Guild.Logger != null)
                    Guild.LogLine(_Owner.Name + ": " + Message);
            }
        }

        public void AllianceSay(string Message)
        {
            if (IsInGuild() && _Owner.GetPlayer().GldInterface.Guild.Info.AllianceId != 0 && _Owner.GetPlayer().GldInterface.Guild.Info.Members[_Owner.GetPlayer().CharacterId].RankId >= 1)
            {
                foreach (uint alli in Alliance.Alliances[Guild.Info.AllianceId].Members)
                {
                    Guild.Guild gl = World.Guild.Guild.GetGuild(alli);

                    // Filter "+" spam 
                    bool isAdd = Message.StartsWith("+");

                    lock (gl.OnlineMembers)
                        foreach (Player plr in gl.OnlineMembers)
                        {
                            if (!isAdd || plr == _Owner || (plr.WorldGroup != null && plr.WorldGroup.Leader == plr))
                                plr.SendMessage(_Owner, Message, ChatLogFilters.CHATLOGFILTERS_ALLIANCE);
                        }
                }
                if (Guild.Logger != null)
                    Guild.LogLine(_Owner.Name + ": " + Message);
            }
        }

        public void AllianceOfficerSay(string Message)
        {
            if (IsInGuild() && _Owner.GetPlayer().GldInterface.Guild.Info.AllianceId != 0 && (_Owner.GetPlayer().GldInterface.Guild.Info.Members[_Owner.GetPlayer().CharacterId].AllianceOfficer || _Owner.GetPlayer().GldInterface.Guild.Info.LeaderId == _Owner.GetPlayer().CharacterId))
            {
                foreach (uint alli in Alliance.Alliances[Guild.Info.AllianceId].Members)
                {
                    Guild.Guild gl = World.Guild.Guild.GetGuild(alli);
                    lock (gl.OnlineMembers)
                    foreach (Player Plr in gl.OnlineMembers)
                            if(gl.Info.Members[Plr.CharacterId].RankId >= 9 || gl.Info.Members[Plr.CharacterId].AllianceOfficer)
                                Plr.SendMessage(_Owner, Message, ChatLogFilters.CHATLOGFILTERS_ALLIANCE_OFFICER);
                }
                if (Guild.Logger != null)
                    Guild.LogLine(_Owner.Name + ": " + Message);
            }
        }


        public void ApplyTaxTithe(ref uint Money)
        {

            if (Guild.Info.Money + Money > ulong.MaxValue)
                return;

                if (Guild.Info.Tax > 0)
            {
                uint gmoney = Money * Guild.Info.Tax / 100;
                Guild.Info.Money += gmoney;
                Money -= gmoney;
            }

            if (Guild.Info.Members[_Owner.GetPlayer().CharacterId].Tithe > 0)
            {
                uint tithemoney = Money * Guild.Info.Members[_Owner.GetPlayer().CharacterId].Tithe / 100;
                Guild.Info.Money += tithemoney;
                Guild.Info.Members[_Owner.GetPlayer().CharacterId].TitheContributed += tithemoney;
                Money -= tithemoney;
            }
        }

        const int MAX_GUILD_SEND = 80;

        public void SendGuilds(List<Guild.Guild> guilds)
        {
            if (!HasPlayer())
                return;

            Player player = GetPlayer();

            int toSend = Math.Min(MAX_GUILD_SEND, guilds.Count);

            PacketOut Out = new PacketOut((byte) Opcodes.F_GUILD_DATA);
            Out.WriteByte(0x20);
            Out.WriteByte((byte)toSend);

            for (int i = 0; i < toSend; ++i)
                World.Guild.Guild.BuildGuild(ref Out, guilds[i]);

            player.SendPacket(Out);
        }
    }
}
