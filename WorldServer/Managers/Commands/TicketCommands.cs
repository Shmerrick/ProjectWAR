using Common;
using System;
using FrameWork;
using System.Collections.Generic;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Addition commands under .ticket</summary>
    internal class TicketCommands
    {   
        /// <summary>
        /// Lists all tickets that are open
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool ListTickets(Player plr, ref List<string> values)
        {
            foreach(Bug_report report in CharMgr._report)
            {
                string assignee="nobody";
                if (report.Assigned != null)
                    assignee = report.Assigned;

                string subtype = "0";

                if (report.ReportType == "General")
                    subtype = "1";

                else if (report.ReportType == "Harassment")
                    subtype = "2";

                else if (report.ReportType == "Zone Disruption")
                    subtype = "3";

                else if (report.ReportType == "XP or Renown Farming")
                    subtype = "4";

                else if (report.ReportType == "Speed Hacking")
                    subtype = "5";

                else if (report.ReportType == "Macroing")
                    subtype = "6";

                else if (report.ReportType == "Kill Stealing")
                    subtype = "7";

                else if (report.ReportType == "Cross Realming")
                    subtype = "8";


                /*  How the category work
                 *  report.type     report.category     what it means
                 *      3               18              Goldseller
                 *      0               1               Stuck in world
                 *      0               2               Missing Character
                 *      0               3               Missing Item
                 *      2               4               Violation report, it also have a subtype defined in report.reporttype
                 *      1               5               Naming Violation
                 *      0               6               Character Issues
                 *      0               19              Paid Item Issues
                 *      0               20              Paid Character Transfer
                 *      0               21              Paid Namechange
                 *      0               7               PQ Issue
                 *      0               8               SC Issue
                 *      0               10              Monster Issue
                 *      0               9               BattlefieldObjective and Keep Issue
                 *      0               11              Quest and Quest Item Issue
                 *      0               12              Combat Issue
                 *      0               13              TOK Issue
                 *      0               14              Mail Issue
                 *      0               15              AH Issue
                 *      0               16              Interface Issue
                 *      0               17              Tradeskill Issue
                 *      THIS IS THE END OF GM TICKETS, HERE BUGREPORTS START
                 *      4               6               Bugreport: Other
                 *      4               1               Bugreport: Art
                 *      4               5               Bugreport: Item
                 *      4               0               Bugreport: Character
                 *      4               2               Bugrepoort: Monster
                 *      4               3               Bugreport: Crash
                 *      4               4               Bugreport: Quest and PQ
                 *      THIS IS THE END OF BUGREPORTS, HERE FEEDBACK STARTS
                 *      5               7               Feedback: Cities
                 *      5               8               Feedback: TOK
                 *      5               9               Feedback: Quests and PQ
                 *      5               10              Feedback: Career
                 *      5               11              Feedback: Combat
                 *      5               12              Feedback: Tradeskill
                 *      5               13              Feedback: UI
                 *      5               14              Feedback: Constructive Criticism
                 *      5               15              Feedback: Positive Feedback
                 * */

                string field = "0";
                if (report.FieldSting != "")
                    field = report.FieldSting.ToString();

                long time = report.Time - TCPManager.GetTimeStamp();

                //0 means that the character has been deleted
                string name = "0";
                //0 means that the ticket is older then when we added accountid to the tickets or that it was not sent for whatever reason into the ticket.
                string account = "0";
                if (CharMgr.GetCharacter(report.CharacterId, false) != null)
                {
                    if (report.AccountId != 0)
                    {
                        if (CharMgr.GetCharacter(report.CharacterId, false).AccountId == report.AccountId)
                        {
                            name = CharMgr.GetCharacter(report.CharacterId, false).Name;
                            account = Program.AcctMgr.GetAccountById((int)report.AccountId).Username;
                        }
                    }
                }

                if (CharMgr.GetCharacter(report.CharacterId, false) == null && report.AccountId != 0)
                {
                    account = Program.AcctMgr.GetAccountById((int)report.AccountId).Username;
                }


                //for whatever reason ^ cannot be sent to the client, it will break the rest of the string.
                string Message = report.Message;
                if (Message.Contains("^"))
                    Message = Message.Replace('^', ' ');

                plr.SendClientMessage("TICKET_REPORT:" + report.ObjectId + ";" + name + ";" + report.Type + ";" + report.Category + ";" + subtype + ";" + field + ";" + assignee + ";" + Message + ";" + time + ";" + account, SystemData.ChatLogFilters.CHATLOGFILTERS_CHANNEL_9);
            }
            plr.SendClientMessage("TICKET_END", SystemData.ChatLogFilters.CHATLOGFILTERS_CHANNEL_9);
            return true;
        }
        /// <summary>
        /// Assigns a person to a ticket
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool Assign(Player plr, ref List<string> values)
        {
            if (values.Count < 2)
            {
                plr.SendClientMessage("Usage: .ticket assign <accountname> <bugtrackerID>");
                return true;
            }

            string account = GetString(ref values);
            string reportID = GetTotalString(ref values).Trim();

            if (string.IsNullOrEmpty(account))
            {
                plr.SendClientMessage("you need to specify a person for the ticket");
                return true;
            }

            if (string.IsNullOrEmpty(reportID))
            {
                plr.SendClientMessage("you need to specify the ticketID");
                return true;
            }

            Bug_report report = CharMgr.GetReport(reportID);

            if (report == null)
            {
                plr.SendClientMessage("The Specified report does not exist");
                return true;
            }

            else
            {
                if (account == "nobody")
                    plr.SendClientMessage("You have unassigned yourself from ticket: " + reportID);
                else
                    plr.SendClientMessage("You have assigned " + account + " to ticket: " + reportID);

                lock (CharMgr._report)
                {
                    report.Assigned = account;
                    CharMgr.Database.SaveObject(report);
                    CharMgr.Database.ForceSave();
                }
                return true;
            }
        }
        /// <summary>
        /// assigns the current player as the person responsible for the ticket
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool AssignMe(Player plr, ref List<string> values)
        {
            if (values.Count < 1)
            {
                plr.SendClientMessage("Usage: .ticket assignme <bugtrackerID>");
                return true;
            }

            string reportID = GetTotalString(ref values).Trim();

            if (string.IsNullOrEmpty(reportID))
            {
                plr.SendClientMessage("you need to specify the ticketID");
                return true;
            }

            Bug_report report = CharMgr.GetReport(reportID);

            if (report == null)
            {
                plr.SendClientMessage("The Specified report does not exist");
                return true;
            }

            else
            {
                plr.SendClientMessage("You have assigned yourself to ticket: " + reportID);
                lock (CharMgr._report)
                {
                    report.Assigned = plr.Client._Account.Username;
                    CharMgr.Database.SaveObject(report);
                    CharMgr.Database.ForceSave();
                }
                return true;
            }
        }
        /// <summary>
        /// returns how many tickets there are in the DB to handle
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool NumberOfTickets(Player plr, ref List<string> values)
        {
            plr.SendClientMessage("TICKET_NUMBER:" + CharMgr._report.Count, SystemData.ChatLogFilters.CHATLOGFILTERS_CHANNEL_9);
            return true;
        }
        /// <summary>
        /// Delete the specified ticket
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool DeleteTicket(Player plr, ref List<string> values)
        {
            if (values.Count < 1)
            {
                plr.SendClientMessage("Usage: .ticket deleteticket <bugtrackerID>");
                return true;
            }

            string reportID = GetTotalString(ref values).Trim();

            if (string.IsNullOrEmpty(reportID))
            {
                plr.SendClientMessage("you need to specify the ticketID");
                return true;
            }

            Bug_report report = CharMgr.GetReport(reportID);

            if (report == null)
            {
                plr.SendClientMessage("The Specified report does not exist");
                return true;
            }

            if (report.Assigned != plr.Client._Account.Username)
            {
                plr.SendClientMessage("You cannot close a ticket not assigned to you(username), assign it to yourself first if you fixed the ticket");
                return true;
            }

            else
            {
                plr.SendClientMessage("You have deleted ticket: " + reportID);

                GMCommandLog log = new GMCommandLog
                {
                    PlayerName = plr.Client._Account.Username,
                    AccountId = (uint)plr.Client._Account.AccountId,
                    Command = $"Removed Ticket: {reportID} from characterID: {report.CharacterId}. containing the following message: {report.Message} {report.FieldSting}",
                    Date = DateTime.Now
                };

                CharMgr.Database.AddObject(log);

                lock (CharMgr._report)
                {
                    CharMgr._report.Remove(report);
                    CharMgr.Database.DeleteObject(report);
                    CharMgr.Database.ForceSave();
                }
                return true;
            }
        }
        /// <summary>
        /// To answer and close a ticket (answer will be sent as an ingame mail)
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool Answer(Player plr, ref List<string> values)
        {
            if (values.Count < 2)
            {
                plr.SendClientMessage("Usage: .ticket answer <bugtrackerID> <message>");
                return true;
            }

            string reportID = GetString(ref values);
            string message = GetTotalString(ref values).Trim();

            if (string.IsNullOrEmpty(message))
            {
                plr.SendClientMessage("you need to specify a message to send");
                return true;
            }

            if (string.IsNullOrEmpty(reportID))
            {
                plr.SendClientMessage("you need to specify the ticketID");
                return true;
            }

            Bug_report report = CharMgr.GetReport(reportID);

            if (report == null)
            {
                plr.SendClientMessage("The Specified report does not exist");
                return true;
            }

            if (report.Assigned != plr.Client._Account.Username)
            {
                plr.SendClientMessage("You cannot answer a ticket not assigned to you(username), assign it to yourself first if you want to answer this ticket");
                return true;
            }

            if (CharMgr.GetCharacter(report.CharacterId, false) == null)
            {
                plr.SendClientMessage("The player who created this ticket is deleted or has not logged in for over the preload period, as such we cannot send a mail to the character.");
                return true;
            }

            else
            {
                plr.SendClientMessage("You have answered ticket: " + reportID);

                GMCommandLog log = new GMCommandLog
                {
                    PlayerName = plr.Client._Account.Username,
                    AccountId = (uint)plr.Client._Account.AccountId,
                    Command = $"Answered Ticket: {reportID} from characterID: {report.CharacterId}. Containing message: {report.Message} {report.FieldSting} with the following reply: {message}",
                    Date = DateTime.Now
                };

                Character chara = CharMgr.GetCharacter(report.CharacterId, false);
                Character_mail ticketMail = new Character_mail
                {
                    Guid = CharMgr.GenerateMailGuid(),
                    CharacterId = chara.CharacterId,
                    CharacterIdSender = chara.CharacterId,
                    SenderName = chara.Name,
                    ReceiverName = chara.Name,
                    SendDate = (uint)TCPManager.GetTimeStamp(),
                    Title = "Answered Ticket",
                    Content = $"Your ticket has been answered by: {report.Assigned} with the following message: \n \n {message}",
                    Money = 0,
                    Opened = false
                };
                
                CharMgr.AddMail(ticketMail);
                CharMgr.Database.AddObject(log);

                lock (CharMgr._report)
                {
                    CharMgr._report.Remove(report);
                    CharMgr.Database.DeleteObject(report);
                    CharMgr.Database.ForceSave();
                }
                return true;
            }
        }

    }
}
