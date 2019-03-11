using System.Collections.Generic;
using FrameWork;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;

namespace WorldServer.NetWork.Handler
{
    public class QuestHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_QUEST, (int)eClientState.WorldEnter, "onQuest")]
        public static void F_QUEST(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            ushort QuestID = packet.GetUint16();
            ushort State = packet.GetUint16();
            ushort Unk1 = packet.GetUint16();
            byte Unk2 = packet.GetUint8();
            byte Unk3 = packet.GetUint8();
            ushort Unk4 = packet.GetUint16();
            ushort CreatureOID = packet.GetUint16();

            Creature Crea = cclient?.Plr?.Region?.GetObject(CreatureOID) as Creature;

            if (Crea == null)
                return;

            switch (State)
            {
                case 1: // Show Quest
                    {
                        if (Crea.QtsInterface.HasQuestStarter(QuestID))
                            Crea.QtsInterface.BuildQuest(QuestID, cclient.Plr);

                    } break;

                case 2: // Accept Quest
                    {
                        if (Crea.QtsInterface.HasQuestStarter(QuestID))
                        {
                            if (cclient.Plr.QtsInterface.AcceptQuest(QuestID))
                            {
                                if (!Crea.QtsInterface.CreatureHasStartQuest(cclient.Plr))
                                {
                                    Crea.SendRemove(cclient.Plr);
                                    Crea.SendMeTo(cclient.Plr);
                                }
                            }
                        }

                    }break;

                case 3: // Quest Done
                    {
                        if (Crea.QtsInterface.HasQuestFinisher(QuestID))
                        {
                            if (cclient.Plr.QtsInterface.DoneQuest(QuestID))
                            {
                                Crea.SendRemove(cclient.Plr);
                                Crea.SendMeTo(cclient.Plr);
                            }
                        }

                    } break;

                case 4: // Quest Done Info
                    {

                        if (Crea.QtsInterface.HasQuestFinisher(QuestID))
                            Crea.QtsInterface.SendQuestDoneInfo(cclient.Plr, QuestID);
                        else if (Crea.QtsInterface.HasQuestStarter(QuestID))
                        {
                            Crea.QtsInterface.SendQuestInProgressInfo(cclient.Plr, QuestID);
                        }

                    } break;

                case 5: // Select Quest Reward
                    {
                        if (Crea.QtsInterface.HasQuestFinisher(QuestID))
                            cclient.Plr.QtsInterface.SelectRewards(QuestID, Unk3);

                    } break;

            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_REQUEST_QUEST, (int)eClientState.WorldEnter, "onRequestQuest")]
        public static void F_REQUEST_QUEST(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            ushort questID = packet.GetUint16();
            byte state = packet.GetUint8();

            switch (state)
            {
                case 0: // Show Quest
                    {
                        cclient.Plr.QtsInterface.SendQuest(questID);
                    } break;

                case 1: // Decline Quest
                    {
                        cclient.Plr.QtsInterface.AbandonQuest(questID);
                    } break;

                case 2: // Send To Group
                    {
                        if(cclient.Plr.WorldGroup != null)
                        {
                            foreach(Player member in cclient.Plr.WorldGroup.GetPlayersCloseTo(cclient.Plr, 25))
                            {
                                if (member != cclient.Plr)
                                {
                                    if (Services.World.QuestService._Quests[questID].Shareable)
                                    {
                                        member.QtsInterface.AcceptQuest(questID);
                                        cclient.Plr.SendClientMessage("You shared quest '" + Services.World.QuestService._Quests[questID].Name + "' with " + member.Name);
                                        member.SendClientMessage(cclient.Plr.Name + " shared quest '" + Services.World.QuestService._Quests[questID].Name + "' with you");
                                    }
                                    else
                                    {
                                        member.SendClientMessage("Quest '" + Services.World.QuestService._Quests[questID].Name + "' cannot be shared.");
                                        cclient.Plr.SendClientMessage("You cannot share quest '" + Services.World.QuestService._Quests[questID].Name + "' with " + member.Name);
                                    }
                                }
                            }
                            //WorldServer.Services.World.QuestService._Quests[questID].Shareable = 1;
                            //WorldServer.Services.World.QuestService._
                        }
                    } break;
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_PLAY_VOICE_OVER, (int)eClientState.WorldEnter, "onPlayVoiceOver")]
        public static void F_PLAY_VOICE_OVER(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            packet.GetUint16(); // Unk
            ushort ZoneId = packet.GetUint16();

            Player Plr = cclient.Plr;

            if (Plr.Region == null)
                return;

            ZoneMgr Zone = Plr.Zone;

            if (Zone == null)
                return;

            List<PublicQuest> PQuests = Zone.PQuests;

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 64);
            Out.WriteUInt32(0);
            Out.WriteByte(6);
            Out.WriteUInt32(ZoneId);
            Out.WriteUInt16((ushort)PQuests.Count);

            foreach (PublicQuest PQuest in PQuests)
            {
                Out.WriteUInt32(PQuest.Info.Entry);
                Out.WriteUInt16((ushort)PQuest.ActivePlayers.Count);
            }

            cclient.Plr.SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_OBJECTIVE_UPDATE, (int)eClientState.WorldEnter, "onObjectiveUpdate")]
        public static void F_OBJECTIVE_UPDATE(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            uint pQuestID = packet.GetUint32();
            byte op = packet.GetUint8();

            switch (op)
            {
                case 8: // switch opt out state
                    byte optOutType = packet.GetUint8();
                    PublicQuest pQuest = cclient.Plr.QtsInterface.PublicQuest;

                    if (pQuest == null || pQuest.Info.Entry != pQuestID)
                        return;

                    pQuest.ProcessOptOut(cclient.Plr, optOutType);
                    break;
                default:
                    Log.Error("F_OBJECTIVE_UPDATE", $"Received unknown op {op} from client");
                    break;
            }
        }
    }
}
