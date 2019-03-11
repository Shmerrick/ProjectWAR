using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.Managers.Commands;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class HelpHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_HELP_DATA, (int)eClientState.WorldEnter, "onHelpData")]
        public static void F_HELP_DATA(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player Plr = cclient.Plr;

            GameData.HelpType Type = (GameData.HelpType)packet.GetUint8();

            switch (Type)
            {
                case GameData.HelpType.HELPTYPE_CREATE_APPEAL_VIOLATION_REPORT: // Violation Report
                case GameData.HelpType.HELPTYPE_CREATE_APPEAL_NAMING_VIOLATION: // Name Violation
                case GameData.HelpType.HELPTYPE_CREATE_APPEAL_GOLD_SELLER: // Gold Seller
                    {
                        GameData.AppealTopic Category = (GameData.AppealTopic)packet.GetUint8();
                        ushort ReportTypeSize = packet.GetUint16R();
                        packet.Skip(2);
                        string ReportType = "";
                        if (ReportTypeSize > 0)
                            ReportType = packet.GetString(ReportTypeSize - 1);
                        packet.Skip(1);
                        ushort MessageSize = packet.GetUint16R();
                        packet.Skip(2);
                        string Message = "";
                        if (MessageSize > 0)
                            Message = packet.GetString(MessageSize - 1);
                        packet.Skip(1);
                        ushort NameSize = packet.GetUint16R();
                        packet.Skip(2);
                        string Name = packet.GetString(NameSize - 1);

                        Bug_report report = new Bug_report();
                        report.Time = (uint)TCPManager.GetTimeStamp();
                        report.AccountId = (uint)Plr.Client._Account.AccountId;
                        report.CharacterId = Plr.CharacterId;

                        //fix for when someone right clicks chat to report or report spam on mails, because mythic thought it was good to classify these as goldsellers... 
                        if (Message.StartsWith("[") || Message.StartsWith($"\n[") || Message.StartsWith($"\n ["))
                        {
                            report.Type = 2; // Violation report
                            report.Category = 4; //Violation report
                            report.ReportType = "General"; //General subcategory of violation report
                        }
                        else
                        {
                            report.Type = (byte)Type;
                            report.Category = (byte)Category;
                            report.ReportType = ReportType;
                        }

                        if (Message.Contains(";"))
                            Message = Message.Replace(';', ':');

                        if (Message.Contains("^"))
                            Message = Message.Replace('^', ' ');

                        report.Message = Message;
                        report.ZoneId = Plr.Zone.Info.ZoneId;
                        report.X = (ushort)(Plr.X / 64);
                        report.Y = (ushort)(Plr.Y / 64);
                        report.Assigned = "nobody";
                        report.Fields.Add(new KeyValuePair<uint, string>(0, Name.Replace("|", "").Replace(":", "")));

                        //lets not allow players to report server automated mails
                        if (Message.Contains("[Mail Subject]: Public Quest Loot [Mail Message Body]: You won a Public Quest Loot Bag") || Message.Contains("[Mail Subject]: Medallion Reward [Mail Message Body]: You've received a medallion reward for your realm's victory in a recent battle in which you were a participant.") || Message.Contains(" [Mail Message Body]: Your mail expired and has been returned to you."))
                        {
                            Plr.SendClientMessage("This is a server generated mail, this ticket will be discarded.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                            break;
                        }


                        CharMgr.Database.AddObject(report);
                        lock(CharMgr._report)
                            CharMgr._report.Add(report);

                        Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_MISC, GameData.Localized_text.TEXT_COPTER_GENERAL_FEEDBACK_SENT);

                        foreach (Player plr in GmMgr.GmList)
                        {
                            plr.SendClientMessage("NEW_TICKET", ChatLogFilters.CHATLOGFILTERS_CHANNEL_9);
                        }
                    } break;
                case GameData.HelpType.HELPTYPE_CREATE_APPEAL_NON_VALIDATED: // CSR Appeal
                    {
                        GameData.AppealTopic Category = (GameData.AppealTopic)packet.GetUint8();
                        ushort MessageSize = packet.GetUint16R();
                        packet.Skip(2);
                        string Message = "";
                        if (MessageSize > 0)
                            Message = packet.GetString(MessageSize - 1);

                        packet.Skip(1);
                        byte FieldsCount = packet.GetUint8();

                        Bug_report report = new Bug_report();
                        report.Time = (uint)TCPManager.GetTimeStamp();
                        report.AccountId = (uint)Plr.Client._Account.AccountId;
                        report.CharacterId = Plr.CharacterId;
                        report.Type = (byte)Type;
                        report.Category = (byte)Category;

                        if (Message.Contains(";"))
                            Message = Message.Replace(';', ':');

                        if (Message.Contains("^"))
                            Message = Message.Replace('^', ' ');

                        report.Message = Message;
                        report.ZoneId = Plr.Zone.Info.ZoneId;
                        report.X = (ushort)(Plr.X / 64);
                        report.Y = (ushort)(Plr.Y / 64);
                        report.Assigned = "nobody";

                        for (int i = 0; i < FieldsCount; i++)
                        {
                            GameData.HelpField FieldType = (GameData.HelpField)packet.GetUint8();
                            ushort FieldSize = packet.GetUint16R();
                            packet.Skip(2);
                            string Field = packet.GetString(FieldSize - 1);
                            packet.Skip(1);
                            report.Fields.Add(new KeyValuePair<uint, string>((byte)FieldType, Field.Replace("|", "").Replace(":", "")));
                        }


                        CharMgr.Database.AddObject(report);
                        lock(CharMgr._report)
                            CharMgr._report.Add(report);

                        Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_MISC, GameData.Localized_text.TEXT_COPTER_GENERAL_FEEDBACK_SENT);

                        foreach (Player plr in GmMgr.GmList)
                        {
                            plr.SendClientMessage("NEW_TICKET", ChatLogFilters.CHATLOGFILTERS_CHANNEL_9);
                        }

                    } break;
                case GameData.HelpType.HELPTYPE_CREATE_BUG_REPORT: // Bug Report
                case GameData.HelpType.HELPTYPE_CREATE_FEEDBACK: // Feedback
                    {
                        byte Category = packet.GetUint8();
                        ushort MessageSize = packet.GetUint16R();
                        packet.Skip(2);
                        string Message = "";
                        if (MessageSize > 0)
                            Message = packet.GetString(MessageSize - 1);

                        Bug_report report = new Bug_report();
                        report.Time = (uint)TCPManager.GetTimeStamp();
                        report.AccountId = (uint)Plr.Client._Account.AccountId;
                        report.CharacterId = Plr.CharacterId;
                        report.Type = (byte)Type;
                        report.Category = Category;

                        if (Message.Contains(";"))
                            Message = Message.Replace(';', ':');

                        if (Message.Contains("^"))
                            Message = Message.Replace('^', ' ');

                        report.Message = Message;
                        report.ZoneId = Plr.Zone.Info.ZoneId;
                        report.X = (ushort)(Plr.X / 64);
                        report.Y = (ushort)(Plr.Y / 64);
                        report.Assigned = "nobody";

                        CharMgr.Database.AddObject(report);
                        lock(CharMgr._report)
                            CharMgr._report.Add(report);

                        Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_COPTER_GENERAL_FEEDBACK_SENT);

                        foreach (Player plr in GmMgr.GmList)
                        {
                            plr.SendClientMessage("NEW_TICKET", ChatLogFilters.CHATLOGFILTERS_CHANNEL_9);
                        }
                    } break;
            }
        }
    }
}
