using System;
using SystemData;
using Common;
using FrameWork;
using WorldServer.World.Guild;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class GuildHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_GUILD_COMMAND, "onGuildCommand")]
        public static void F_GUILD_COMMAND(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player Plr = cclient.Plr;
            packet.Skip(2);
            UInt16 Target = packet.GetUint16(); // ?
            uint CharacterId = packet.GetUint32();
            byte State = packet.GetUint8();

            //Log.Info("state ",""+State);
            switch (State)
            {
                case 1: // Accepted Invite
                    {
                        if (Plr.GldInterface.invitedTo == null)
                        {
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILD_INVITE_ERR_NOINVITE);
                            return;
                        }

                        Plr.GldInterface.Guild = Plr.GldInterface.invitedTo;
                        Plr.GldInterface.invitedTo = null;

                        Plr.GldInterface.Guild.JoinGuild(Plr);
                    } break;
                case 2: // Declined Invite
                    {
                        Plr.GldInterface.invitedTo = null;
                    } break;
                case 3: // Leave Guild
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        if (Plr.GldInterface.Guild.Info.LeaderId == Plr.CharacterId)
                        {
                            // wrong message
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_ALLIANCE_INVITE_ERROR);
                            return;
                        }

                        Guild_member GldMem = Plr.GldInterface.Guild.Info.Members[Plr.CharacterId];

                        Plr.GldInterface.Guild.LeaveGuild(GldMem, false);

                    } break;
                case 5: // Promote
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        Plr.GldInterface.Guild.PromoteMember(Plr, CharacterId);
                    } break;
                case 6: // Demote
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        Plr.GldInterface.Guild.DemoteMember(Plr, CharacterId);
                    } break;
                case 7: // Assign as leader
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        Plr.GldInterface.Guild.AssignLeader(Plr, CharacterId);
                    } break;
                case 9: // Guild NPC
                    {
                        PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 4);
                        Out.WriteByte(0x0e);
                        Out.WriteByte(1);
                        Out.WriteByte(1);
                        Out.WriteByte(0);
                        Plr.SendPacket(Out);
                        Log.Info("GUILd", "CREATE");
                    } break;
                case 10: // Create guild
                    {
                        packet.Skip(1);
                        String name = packet.GetStringToZero();
                        if (Plr.GldInterface.IsInGuild())
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILDNPC_INVALIDREQ_GUILDED);
                        else if (Guild.GetGuild(name) != null)
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILDNPC_BAD_NAME);
                        else if (Plr.GmLevel > 0)
                            new GuildInvitation(Plr, name);
                        else if (Plr.WorldGroup == null)
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILDNPC_INVALIDREQ_NOTGROUPED);
                        else if (Plr.WorldGroup.GetLeader() != Plr)
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILDNPC_INVALIDREQ_NOTLEADER);
                        else if (!Plr.WorldGroup.HasMaxMembers)
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILDNPC_INVALIDREQ_NUMPLAYERS);
                        else
                        {
                            foreach (Player groupMember in Plr.WorldGroup.GetPlayerList())
                            {
                                if (groupMember.GldInterface.IsInGuild())
                                {
                                    Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILDNPC_INVALIDREQ_GUILDED);
                                    return;
                                }
                            }

                            new GuildInvitation(Plr, name);
                        }
                    } break;

                case 16: // Accepted Alliance Invite
                    {
                        if (Plr.GldInterface.AllianceinvitedTo > 0)  // alli invite
                        {
                            Plr.GldInterface.Guild.JoinAlliance(Plr.GldInterface.AllianceinvitedTo);
                            Plr.GldInterface.AllianceinvitedTo = 0;
                        }
                        else if(Plr.GldInterface.AllianceFormGuildId > 0 )  // alli form
                        {
                            Plr.GldInterface.Guild.FormAlliance(Plr.GldInterface.AllianceFormName, Plr.GldInterface.AllianceFormGuildId);
                            Plr.GldInterface.AllianceFormGuildId = 0;
                            Plr.GldInterface.AllianceFormName = "";
                        }
                        else
                        {
                            Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_GUILD_INVITE_ERR_NOINVITE);
                            return;
                        }
                    }
                    break;
                case 17: // Declined Alliance Invite
                    {
                        //CharMgr.GetCharacter().SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, GameData.Localized_text.TEXT_ALLIANCE_INVITE_DECLINED);
                        Plr.GldInterface.AllianceinvitedTo = 0;
                    }
                    break;

                case 21:  // buy tactic
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;
                        packet.Skip(1);
                        string spell = packet.GetStringToZero();
                        Plr.GldInterface.Guild.TrainGuildTactics((byte)CharacterId, ushort.Parse(spell));
                    } break;

                case 22:  // calendar create
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;
                        packet.Skip(156);
                        UInt32 begin = packet.GetUint32();
                        UInt32 end = packet.GetUint32();
                        String name = packet.GetPascalString();
                        String description = packet.GetPascalString();
                        byte alliance = packet.GetUint8();
                        byte locked = packet.GetUint8();
                        Plr.GldInterface.Guild.CreateEvent(Plr.GetPlayer().CharacterId,begin,end,name,description,alliance,locked);

                    } break;

                case 23:  // calendar save
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;
                        packet.Skip(151);
                        byte eventid = packet.GetUint8();
                        uint guildid = packet.GetUint32R();

                        UInt32 begin = packet.GetUint32();
                        UInt32 end = packet.GetUint32();
                        String name = packet.GetPascalString();
                        String description = packet.GetPascalString();
                        byte alliance = packet.GetUint8();
                        byte locked = packet.GetUint8();
                        Plr.GldInterface.Guild.EventEdit(Plr,eventid,guildid, begin, end, name, description, alliance, locked);
                    }
                    break;

                case 24:  // calendar delete
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        packet.Skip(151);
                        byte eventid = packet.GetUint8();
                        uint guildid = packet.GetUint32R();
                        Plr.GldInterface.Guild.DeleteEvent(Plr,eventid,guildid);

                    }
                    break;

                case 25:  // calendar signup
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        packet.Skip(151);
                        byte eventid = packet.GetUint8();
                        uint guildid = packet.GetUint32R();
                        Plr.GldInterface.Guild.EventSignup(Plr, eventid, guildid);
                    }
                    break;
                case 26:  // calendar signup cancel
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        packet.Skip(151);
                        byte eventid = packet.GetUint8();
                        uint guildid = packet.GetUint32R();
                        Plr.GldInterface.Guild.EventSignupCancel(Plr, eventid, guildid);
                    }
                    break;

                case 27:  // calendar signup kick
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        packet.Skip(151);
                        byte eventid = packet.GetUint8();
                        uint guildid = packet.GetUint32R();
                        uint characterid = packet.GetUint32R();
                        Plr.GldInterface.Guild.EventSignupKick(Plr, eventid, guildid,characterid);
                    }
                    break;

                case 28:  // calendar signup approved
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        packet.Skip(151);
                        byte eventid = packet.GetUint8();
                        uint guildid = packet.GetUint32R();
                        uint charid = packet.GetUint32R();
                        Plr.GldInterface.Guild.EventSignupApproved(Plr, eventid, guildid,charid);
                    }
                    break;

                case 29:  // banner save
                    {
                        packet.Skip(151);
                        byte banner = packet.GetUint8();
                        byte post = packet.GetUint8();
                        packet.Skip(1);
                        ushort spell1 = packet.GetUint16R();
                        packet.Skip(2);
                        ushort spell2 = packet.GetUint16R();
                        packet.Skip(2);
                        ushort spell3 = packet.GetUint16R();
                        Plr.GldInterface.Guild.SaveBanner((byte)(banner-1),post,spell1,spell2,spell3);
                    } break;

                case 30:  // reserve banner
                    {
                        packet.Skip(151);
                        ushort emblem =packet.GetUint16R();
                        ushort pattern = packet.GetUint16R();
                        byte color1 =packet.GetUint8();
                        byte color2 =packet.GetUint8();    
                        byte shape =packet.GetUint8();    
                        Plr.GldInterface.Guild.ReserveBanner(Plr,emblem,  pattern, color1, color2, shape);
                    } break;
                case 37:  //close guild vault
                    {
                        Plr.GldInterface.Guild.GuildVaultClosed(Plr);
                    } break;
                case 38:  // Drop item to guild vault
                    {
                        packet.Skip(151);
                        byte sourceVault = packet.GetUint8();   // will be > 0 if item transfer from player to vault
                        ushort itemSlot = packet.GetUint16R();
                        packet.Skip(2);
                        byte destVault = packet.GetUint8();
                        ushort slot = packet.GetUint16R();
                        //Log.Info("slot", "" + slot + "  vault " + Vault+"         unk:"+Itemslot);
                        if (sourceVault != 0 && destVault != 0)
                            Plr.GldInterface.Guild.MoveVaultItem(Plr, sourceVault, itemSlot, destVault, slot);
                        else if (sourceVault == 0)
                            Plr.GldInterface.Guild.DepositVaultItem(Plr, destVault, slot, itemSlot);
                        else
                            Plr.GldInterface.Guild.WithdrawVaultItem(Plr, sourceVault, itemSlot, slot);
                    } break;
                case 39:  // Deposit money to guild vault
                    {
                        packet.Skip(151);
                        uint Money = packet.GetUint32R();
                        Plr.GldInterface.Guild.DepositMoney(Plr , Money);
                    } break;
                case 40:  // Withdraw money to guild vault
                    {
                        packet.Skip(151);
                        uint Money = packet.GetUint32R();
                        Plr.GldInterface.Guild.WithdrawMoney(Plr , Money);
                    } break;
                case 41:  // Lock item in guild vault
                    {
                        packet.Skip(151);
                        byte Vault = packet.GetUint8();
                        byte slot = packet.GetUint8();
                     //   Log.Info("slot", "" + slot + "  vault " + Vault);
                        Plr.GldInterface.Guild.LockVaultItem(Plr, Vault, slot, 0);
                    } break;
                case 42:  // cancel guild vault item move
                    {
                        packet.Skip(151);
                        byte Vault = packet.GetUint8();
                        byte slot = packet.GetUint8();
                     //   Log.Info("slot", "" + slot + "  vault " + Vault);
                        Plr.GldInterface.Guild.ReleaseVaultItemLock(Plr, Vault,slot);
                    } break;
                case 46: // Update recruit page save
                    {
                        if (!Plr.GldInterface.IsInGuild())
                            return;

                        packet.Skip(152);

                        String BriefDescription = packet.GetPascalString(); //might be ushort for size
                        String Summary = packet.GetString(packet.GetUint16());
                        byte PlayStyle = packet.GetUint8();
                        byte Atmosphere = packet.GetUint8();
                        uint CareersNeeded = packet.GetUint32();
                        packet.Skip(3);
                        byte RanksNeeded = packet.GetUint8();
                        packet.Skip(3);
                        byte Interests = packet.GetUint8();
                        packet.Skip(3);
                        byte ActivelyRecruiting = packet.GetUint8();
                        //packet.Skip(6);

                        Plr.GldInterface.Guild.SetRecruitmentInfo(BriefDescription, Summary, PlayStyle, Atmosphere, CareersNeeded, RanksNeeded, Interests, ActivelyRecruiting);

                    } break;
                case 47: // Search guilds
                    {
                        packet.Skip(151);
                        byte Style = packet.GetUint8();
                        byte Atmosphere = packet.GetUint8();
                        packet.Skip(7);
                        byte MyLevelCareer = packet.GetUint8();
                        packet.Skip(2);
                        ushort Pop = packet.GetUint16();
                        packet.Skip(3);
                        byte Online = packet.GetUint8();
                        packet.Skip(3);
                        byte Rank = packet.GetUint8();

                        Plr.GldInterface.SendGuilds(Guild.GetGuilds(Plr.Realm, Style, Atmosphere, MyLevelCareer, Plr.Level, Plr.Info.Career, Pop, Online, Rank));
                    } break;
                case 52: // guild tactic respec
                    {
                        Plr.GldInterface.Guild.GuildsTacticRespec(Plr);
                    } break;
                case 53: // Buy Guild Vault Slot
                    {
                        packet.Skip(151);
                        byte Vault = packet.GetUint8();
                        uint Money = packet.GetUint32R();
                   //     Log.Info("", "vault " + Vault+"   prize"+Money);
                        Plr.GldInterface.Guild.BuyVaultSlot(Plr,Vault,Money);
                    } break;
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_PLAYERORG_APPROVAL, "onGuildCreation")]
        public static void F_PLAYERORG_APPROVAL(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            packet.Skip(1);

            Boolean accepted = packet.GetUint8() == 1;

            Player Plr = cclient.Plr;

            if (Plr.GldInterface.invite == null)
                return;

            Plr.GldInterface.invite.InviteResponse(Plr, accepted);
        }
    }
}