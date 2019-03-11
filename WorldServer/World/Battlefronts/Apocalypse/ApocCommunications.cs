using System;
using System.Drawing;
using SystemData;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class ApocCommunications : IApocCommunications
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void SendFlagLeft(Player plr, int id)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 8);

            Out.WriteUInt32((uint)id);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        public void BuildCaptureStatus(PacketOut Out, RegionMgr region, Realms realm)
        {
            if (region == null)
                Out.Fill(0, 3);
            else
                region.Campaign?.WriteCaptureStatus(Out, realm);
        }

        public void BuildBattleFrontStatus(PacketOut Out, RegionMgr region)
        {
            //if (region == null)
            //    Out.Fill(0, 3);
            //else
            //{
                Out.WriteByte((byte)0);
                Out.WriteByte((byte)0);
                Out.WriteByte((byte)1);
            //}
            //region.Campaign.WriteBattleFrontStatus(Out);
        }

        public void ResetProgressionCommunications(Player plr, Realms realm, VictoryPointProgress vpp, string forceT4)
        {
            _logger.Warn("F_CAMPAIGN_STATUS");
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAMPAIGN_STATUS, 159);
            Out.WriteHexStringBytes("0005006700CB00"); // 7

            // Dwarfs vs Greenskins T1
            
            Out.WriteByte(0);    // 0 and ignored
            Out.WriteByte(100);  // % Order lock
            Out.WriteByte(0);    // % Dest lock
            // Dwarfs vs Greenskins T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(12, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(50);
            Out.WriteByte(50);
            // Dwarfs vs Greenskins T3
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(10, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(100);
            // Dwarfs vs Greenskins T4
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(2, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            // Empire vs Chaos T1
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(8, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(100);
            Out.WriteByte(0);
            // Empire vs Chaos T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(14, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(100);
            // Empire vs Chaos T3
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(6, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(100);
            // Empire vs Chaos T4
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(11, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            // High Elves vs Dark Elves T1
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(3, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            // High Elves vs Dark Elves T2
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(15, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            // High Elves vs Dark Elves T3
            // BuildCaptureStatus(Out, WorldMgr.GetRegion(16, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            // High Elves vs Dark Elves T4
            //BuildCaptureStatus(Out, WorldMgr.GetRegion(4, false), realm);
            Out.WriteByte(0);
            Out.WriteByte(100);
            Out.WriteByte(100);

            Out.Fill(0, 83);

            if (string.IsNullOrEmpty(forceT4))
            {

                Out.WriteByte(3); //dwarf fort
                Out.WriteByte((byte) 1); // KV 0 contested, 1 order, 2 dest
                Out.WriteByte((byte) 0); // TM
                Out.WriteByte((byte) 2); // BC
                Out.WriteByte(3); //fort

                Out.WriteByte(3); //emp fort
                Out.WriteByte((byte) 1); // reik
                Out.WriteByte((byte) 0); // praag
                Out.WriteByte((byte) 2); // cw
                Out.WriteByte(3); //fort

                Out.WriteByte(3); //elf fort
                Out.WriteByte((byte) 1); // Eataine
                Out.WriteByte((byte) 0); // DW
                Out.WriteByte((byte) 2); // Caledor
                Out.WriteByte(3); //fort
            }
            else
            {
                Out.WriteByte(3); //dwarf fort
                Out.WriteByte(Convert.ToByte(forceT4[0].ToString())); // KV 0 contested, 1 order, 2 dest
                Out.WriteByte(Convert.ToByte(forceT4[1].ToString())); // TM
                Out.WriteByte(Convert.ToByte(forceT4[2].ToString())); // BC
                Out.WriteByte(3); //fort

                Out.WriteByte(3); //emp fort
                Out.WriteByte(Convert.ToByte(forceT4[3].ToString())); // reik
                Out.WriteByte(Convert.ToByte(forceT4[4].ToString())); // praag
                Out.WriteByte(Convert.ToByte(forceT4[5].ToString())); // cw
                Out.WriteByte(3); //fort

                Out.WriteByte(3); //elf fort
                Out.WriteByte(Convert.ToByte(forceT4[6].ToString())); // Eataine
                Out.WriteByte(Convert.ToByte(forceT4[7].ToString())); // DW
                Out.WriteByte(Convert.ToByte(forceT4[8].ToString())); // Caledor
                Out.WriteByte(3); //fort
            }
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);

            Out.WriteByte(00);

            Out.Fill(0, 4);

            _logger.Debug("APOCCOMM:"+Out.ToString());

            plr.SendPacket(Out);
        }

        public void SendCampaignStatus(Player plr, VictoryPointProgress vpp, Realms realm)
        {
            _logger.Trace("Send Campaign Status");
            _logger.Warn("F_CAMPAIGN_STATUS");
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAMPAIGN_STATUS, 159);
            Out.WriteHexStringBytes("0005006700CB00"); // 7

            // Dwarfs vs Greenskins T1
            BuildCaptureStatus(Out, WorldMgr.GetRegion(1, false), realm);

            // Dwarfs vs Greenskins T2
            BuildCaptureStatus(Out, WorldMgr.GetRegion(12, false), realm);

            // Dwarfs vs Greenskins T3
            BuildCaptureStatus(Out, WorldMgr.GetRegion(10, false), realm);

            // Dwarfs vs Greenskins T4
            BuildCaptureStatus(Out, WorldMgr.GetRegion(2, false), realm);

            // Empire vs Chaos T1
            BuildCaptureStatus(Out, WorldMgr.GetRegion(8, false), realm);

            // Empire vs Chaos T2
            BuildCaptureStatus(Out, WorldMgr.GetRegion(14, false), realm);

            // Empire vs Chaos T3
            BuildCaptureStatus(Out, WorldMgr.GetRegion(6, false), realm);

            // Empire vs Chaos T4
            BuildCaptureStatus(Out, WorldMgr.GetRegion(11, false), realm);

            // High Elves vs Dark Elves T1
            BuildCaptureStatus(Out, WorldMgr.GetRegion(3, false), realm);

            // High Elves vs Dark Elves T2
            BuildCaptureStatus(Out, WorldMgr.GetRegion(15, false), realm);

            // High Elves vs Dark Elves T3
            BuildCaptureStatus(Out, WorldMgr.GetRegion(16, false), realm);

            // High Elves vs Dark Elves T4
            BuildCaptureStatus(Out, WorldMgr.GetRegion(4, false), realm);

            Out.Fill(0, 83);

            // RB   4/24/2016   Added logic for T4 campaign progression.
            //gs t4
            // 0 contested 1 order controled 2 destro controled 3 notcontroled locked
            Out.WriteByte(3);   //dwarf fort
            BuildBattleFrontStatus(Out, WorldMgr.GetRegion(2, false));   //kadrin valley
            Out.WriteByte(3);   //orc fort

            //chaos t4
            Out.WriteByte(3);   //empire fort

            Out.WriteByte((byte)3);
            BuildBattleFrontStatus(Out, WorldMgr.GetRegion(11, false));   //reikland
            Out.WriteByte(3);   //chaos fort

            //elf
            Out.WriteByte(3);   //elf fort
            BuildBattleFrontStatus(Out, WorldMgr.GetRegion(4, false));   //etaine
            Out.WriteByte(3);   //delf fort

            Out.WriteByte(0); // Order underdog rating
            Out.WriteByte(0); // Destruction underdog rating

            if (plr == null)
            {
                byte[] buffer = Out.ToArray();

                if (Player._Players.Count == 0)
                    plr.SendPacket(Out);
                else
                {

                    lock (Player._Players)
                    {
                        foreach (Player player in Player._Players)
                        {
                            if (player == null || player.IsDisposed || !player.IsInWorld())
                                continue;

                            PacketOut playerCampaignStatus = new PacketOut(0, 159) { Position = 0 };
                            playerCampaignStatus.Write(buffer, 0, buffer.Length);

                            if (player.Region?.Campaign != null)
                                WriteVictoryPoints(player.Realm, playerCampaignStatus, vpp);

                            else
                                playerCampaignStatus.Fill(0, 9);

                            playerCampaignStatus.Fill(0, 4);

                            player.SendPacket(playerCampaignStatus);
                        }
                    }
                }
            }
            else
            {
                if (plr.Region?.Campaign != null)
                    WriteVictoryPoints(plr.Realm, Out, vpp);

                else
                    Out.Fill(0, 9);

                Out.Fill(0, 4);

                plr.SendPacket(Out);
            }
        }

        public void WriteVictoryPoints(Realms realm, PacketOut Out, VictoryPointProgress vpp)
        {

            Out.WriteByte((byte)vpp.OrderVictoryPoints);
            Out.WriteByte((byte)vpp.DestructionVictoryPoints);

            //no clue but set to a value wont show the pool updatetimer
            Out.WriteByte(0);
            Out.WriteByte(0);

            Out.WriteByte(00);


            Out.WriteUInt32(0);
            //local timer for poolupdates
            //int curTimeSeconds = TCPManager.GetTimeStamp();

            //if (_nextVpUpdateTime == 0 || curTimeSeconds > _nextVpUpdateTime)
            //        Out.WriteUInt32(0);
            //    else
            //        Out.WriteUInt32((uint)(_nextVpUpdateTime - curTimeSeconds)); //in seconds
        }

        public void Broadcast(string message, int tier)
        {
            lock (Player._Players)
            {
                foreach (Player plr in Player._Players)
                {
                    if (!plr.ValidInTier(tier, true))
                        continue;

                    plr.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                    plr.SendLocalizeString(message, plr.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
                }
            }
        }

        public void Broadcast(string message, Realms realm, RegionMgr region, int tier)
        {
            foreach (Player plr in region.Players)
            {
                if (!plr.ValidInTier(tier, true))
                    continue;

                plr.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                plr.SendLocalizeString(message, realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
            }
        }


    }


}
