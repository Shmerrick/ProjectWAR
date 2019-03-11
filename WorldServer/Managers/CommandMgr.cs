using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers.Commands;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Guild;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.Managers
{
    public class CommandHandler
    {
        public delegate void ComHandler(Player plr, string text);
        public CommandHandler(string name, ComHandler handler, CommandHandler[] sub)
        {
            Name = name;
            SubHandler = sub;
            Handler = handler;
        }

        public string Name;
        public ComHandler Handler;
        public CommandHandler[] SubHandler;
    }

    public static class CommandMgr
    {
        #region Handlers

        public static CommandHandler[] FriendHandler = {
            new CommandHandler("add", AddFriend, null ),
            new CommandHandler("list", null, null ),
            new CommandHandler("remove", RemoveFriend, null ),
            new CommandHandler("toggle", null, null )
        };

        public static CommandHandler[] IgnoreHandler = {
            new CommandHandler("toggle", ToggleIgnore, null ),
            new CommandHandler("add", ToggleIgnore, null ),
            new CommandHandler("remove", ToggleIgnore, null )
        };

        public static CommandHandler[] AllianceHandler = {
            new CommandHandler("form", AllianceForm, null ),
            new CommandHandler("invite", AllianceInvite, null ),
            new CommandHandler("leave", AllianceLeave, null ),
            new CommandHandler("list", AllianceList, null ),
            new CommandHandler("say", AllianceSay, null )
        };

        public static CommandHandler[] SocialHandler = {
            new CommandHandler("anon", SocialAnon, null ),
            new CommandHandler("hide", SocialHide, null )
        };

        public static CommandHandler[] GuildHandler = {
            new CommandHandler("invite", GuildInvite, null ),
            new CommandHandler("disablerank", GuildRankDisable, null ),
            new CommandHandler("enablerank", GuildRankEnable, null ),
            new CommandHandler("kick", GuildKick, null ),
            new CommandHandler("motd", GuildMotd, null),
            new CommandHandler("details", GuildDetails, null),
            new CommandHandler("toggleperm", GuildPermission, null),
            new CommandHandler("note", GuildNote, null),
            new CommandHandler("onote", GuildOfficerNote, null),
            new CommandHandler("recruiter", GuildRecruiter, null),
            new CommandHandler("rwcadd", null, null),
            new CommandHandler("setrankname", GuildRankRename, null),
            new CommandHandler("removestandardbearer", GuildRemoveStandardBearer, null),
            new CommandHandler("setstandardbearer", GuildSetStandardBearer, null),
            new CommandHandler("tax", GuildSetTax, null),
            new CommandHandler("tithe", GuildSetTithe, null),
            new CommandHandler("apromote",APromote , null),
            new CommandHandler("ademote",ADemote , null)
        };

        public static CommandHandler[] Handlers = {
            new CommandHandler("/afk", null, null ),
            new CommandHandler("/alliance", null, AllianceHandler ),
            new CommandHandler("/a", AllianceSay, null ),
            new CommandHandler("/as", AllianceSay, null ),
            new CommandHandler("/allianceofficersay", AllianceOfficerSay, null ),
            new CommandHandler("/ao", AllianceOfficerSay, null ),
            new CommandHandler("/aos", AllianceOfficerSay, null ),
            new CommandHandler("/anonymous", null, null ),
            new CommandHandler("/appeal", null, null ),
            new CommandHandler("/appealview", null, null ),
            new CommandHandler("/appealcancel", null, null ),
            new CommandHandler("/assist", null, null ),
            new CommandHandler("/aid", null, null ),
            new CommandHandler("/bug", null, null ),
            new CommandHandler("/channel", null, null ),
            new CommandHandler("/chan", PlayerChan, null ),
            new CommandHandler("/2", null, null ),
            new CommandHandler("/3", null, null ),
            new CommandHandler("/4", null, null ),
            new CommandHandler("/5", null, null ),
            new CommandHandler("/6", null, null ),
            new CommandHandler("/7", null, null ),
            new CommandHandler("/8", null, null ),
            new CommandHandler("/9", null, null ),
            new CommandHandler("/channelwho", null, null ),
            new CommandHandler("/cloaktoggle", ToggleCloak, null ),
            new CommandHandler("/cloak", null, null ),
            new CommandHandler("/count", null, null ),
            new CommandHandler("/debugwindow", null, null ),
            new CommandHandler("/duelchallenge", DuelChallenge, null ),
            new CommandHandler("/duel", DuelChallenge, null ),
            new CommandHandler("/duelsurrender", null, null ),
            new CommandHandler("/emote", PlayerEmoteSay,null),
            new CommandHandler("::", null, null ),
            new CommandHandler("/emotelist", null, null ),
            new CommandHandler("/friend", null, FriendHandler ),
            new CommandHandler("/guild", GuildSay, null ),
            new CommandHandler("/g", GuildSay, null ),
            new CommandHandler("/gc", null, GuildHandler ),
            new CommandHandler("/o", GuildOfficerSay, null ),
            new CommandHandler("/follow", null, null ),
            new CommandHandler("/helmtoggle", ToggleHelm, null ),
            new CommandHandler("/helm", null, null ),
            new CommandHandler("/help", PlayerAd, null ),
            new CommandHandler("/hide", null, null ),
            new CommandHandler("/ignoreadd", null, null ),
            new CommandHandler("/ignorelist", null, null ),
            new CommandHandler("/ignoreremove", null, null ),
            new CommandHandler("/ignoretoggle", null, null ),
            new CommandHandler("/ignore", null, IgnoreHandler ),
            new CommandHandler("/inspect", null, null ),
            new CommandHandler("/inspectable", null, null ),
            new CommandHandler("/inspectablebraggingrights", null, null ),
            new CommandHandler("/inspectabletradeskills", null, null ),
            new CommandHandler("/join", PartyJoin, null ),
            new CommandHandler("/language", null, null ),
            new CommandHandler("/lfguild", null, null ),
            new CommandHandler("/lfm", null, null ),
            new CommandHandler("/lfp", null, null ),
            new CommandHandler("/lfg", null, null ),
            new CommandHandler("/location", null, null ),
            new CommandHandler("/loc", null, null ),
            new CommandHandler("/lockouts", null, null ),
            new CommandHandler("/logout", null, null ),
            new CommandHandler("/camp", null, null ),
            new CommandHandler("/openlist", null, null ),
            new CommandHandler("/openpartyinterest", null, null ),
            new CommandHandler("/openjoin", null, null ),
            new CommandHandler("/partyroll", PartyRoll, null ),
            new CommandHandler("/partyrandom", PartyRoll, null ),
            new CommandHandler("/partyresetinstance", null, null ),
            new CommandHandler("/partynote", null, null ),
            new CommandHandler("/openpartynote", null, null ),
            new CommandHandler("/lfpnote", null, null ),
            new CommandHandler("/partysay", PartySay, null ),
            new CommandHandler("/p", PartySay, null),
            new CommandHandler("/partyjoin", PartyJoin, null ),
            new CommandHandler("/partyinvite", PartyInvite, null ),
            new CommandHandler("/invite", PartyInvite , null),
            new CommandHandler("/partyinviteopen", null, null ),
            new CommandHandler("/oinvite", null, null ),
            new CommandHandler("/partyremove", PartyKick, null ),
            new CommandHandler("/partykick", PartyKick, null ),
            new CommandHandler("/partyboot", PartyKick, null ),
            new CommandHandler("/insult", Insult, null ),
            new CommandHandler("/kick", PartyKick, null ),
            new CommandHandler("/partyleader", PartyChangeLeader, null ),
            new CommandHandler("/makeleader", PartyChangeLeader, null ),
            new CommandHandler("/mainassist", PartyChangeMainAssist, null ),
            new CommandHandler("/makemainassist", PartyChangeMainAssist, null ),
            new CommandHandler("/partyleave", PartyLeave, null ),
            new CommandHandler("/partyquit", PartyLeave, null ),
            new CommandHandler("/leave", PartyLeave, null),
            new CommandHandler("/disband", PartyLeave, null ),
            new CommandHandler("/pc", PartyCommand, null ),
            new CommandHandler("/partylfwarband", null, null ),
            new CommandHandler("/partylfw", null, null ),
            new CommandHandler("/partyopen", PartyOpen, null ),
            new CommandHandler("/partyprivate", PartyClose, null ),
            new CommandHandler("/partyclose", PartyClose, null ),
            new CommandHandler("/partypassword", null, null ),
            new CommandHandler("/partyguildonly", null, null ),
            new CommandHandler("/partyallianceonly", null, null ),
            new CommandHandler("/partylist", null, null ),
            new CommandHandler("/petname", SetPetName, null ),
            new CommandHandler("/played", null, null ),
            new CommandHandler("/random", Roll, null ),
            new CommandHandler("/roll", Roll, null ),
            new CommandHandler("/reply", null, null ),
            new CommandHandler("/r", null, null ),
            new CommandHandler("/transferguild", null, null ),
            new CommandHandler("/reportgoldseller", null, null ),
            new CommandHandler("/rgs", null, null ),
            new CommandHandler("/rg", null, null ),
            new CommandHandler("/reloadui", null, null ),
            new CommandHandler("/refund", null, null ),
            new CommandHandler("/respec", null, null ),
            new CommandHandler("/rvr", null, null ),
            new CommandHandler("/pvp", null, null ),
            new CommandHandler("/rvrmap", null, null ),
            new CommandHandler("/quit", PlayerQuit, null),
            new CommandHandler("/exit", PlayerExit, null ),
            new CommandHandler("/q", PlayerQuit, null ),
            new CommandHandler("/rude", Rude, null ),
            new CommandHandler("/say", PlayerSay, null ),
            new CommandHandler("/s", PlayerSay, null ),
            new CommandHandler("'", null, null ),
            new CommandHandler("/scenariosay", PlayerScenarioSay, null ),
            new CommandHandler("/sc", PlayerScenarioSay, null ),
            new CommandHandler("/scenariotell", null, null ),
            new CommandHandler("/sp", ScenarioPartySay, null ),
            new CommandHandler("/sp1", ScenarioPartySay, null ),
            new CommandHandler("/sp2", ScenarioPartySay, null ),
            new CommandHandler("/sp3", ScenarioPartySay, null ),
            new CommandHandler("/sp4", ScenarioPartySay, null ),
            new CommandHandler("/sp5", ScenarioPartySay, null ),
            new CommandHandler("/sp6", ScenarioPartySay, null ),
            new CommandHandler("/sp7", ScenarioPartySay, null ),
            new CommandHandler("/sp8", ScenarioPartySay, null ),
            new CommandHandler("/sp9", ScenarioPartySay, null ),
            new CommandHandler("/sp10", ScenarioPartySay, null ),
            new CommandHandler("/script", null, null ),
            new CommandHandler("/shout", PlayerShout, null),
            new CommandHandler("/showcloakheraldry", ToggleCloakHeraldry, null),
            new CommandHandler("/skol", Skol, null),
            new CommandHandler("/social", SocialCommand, null ),
            new CommandHandler("/stuck", PlayerStuck, null ),
            new CommandHandler("/target", Target, null ),
            new CommandHandler("/tell", PlayerWisp, null),
            new CommandHandler("/t", PlayerWisp, null ),
            new CommandHandler("/whisper", PlayerWisp, null),
            new CommandHandler("/w", PlayerWisp, null ),
            new CommandHandler("/msg", PlayerWisp, null ),
            new CommandHandler("/send", PlayerWisp, null ),
            new CommandHandler("/time", Time, null ),
            new CommandHandler("/togglecloakheraldry", null, null ),
            new CommandHandler("/togglealwaysformprivate", null, null ),
            new CommandHandler("/warband", WarbandSay, null ),
            new CommandHandler("/war", WarbandSay, null ),
            new CommandHandler("/wbc", WarbandCommand, null ),
            new CommandHandler("/wb", WarbandSay, null ),
            new CommandHandler("/ra", WarbandSay, null ),
            new CommandHandler("/who", null, null ),
            new CommandHandler("/advs", PlayerAd, null ),
            new CommandHandler("/schan", ScenarioPartySay, null ),
            new CommandHandler("/rws", RealmWarSay, null ),
            new CommandHandler("/", null, null),

            new CommandHandler("/guildinvite", GuildInvite, null)
        };

        #endregion

        #region Commands

        public static void HandleText(Player plr, string text)
        {
            if (text.Length <= 0)
                return;

            if (text[0] != '&' && text[0] != '/')
                return;

            text = text.Remove(0, 1);
            text = text.Insert(0, "/");

            if (WorldMgr.GeneralScripts.OnPlayerCommand(plr, text))
                GetCommand(plr, text, Handlers);
        }
        public static void GetCommand(Player plr, string text, CommandHandler[] handlers)
        {
            string command = "";
            int pos = text.IndexOf(' ');
            if (pos > 0)
            {
                command = text.Substring(0, pos);
                text = text.Remove(0, pos < text.Length ? pos + 1 : pos);
            }
            else
            {
                command = text;
                text = "";
            }

            command = command.Replace("^M", string.Empty).Replace("^F", string.Empty);
            text = text.Replace("^M", string.Empty).Replace("^F", string.Empty);

            for (int i = 0; i < handlers.Length; ++i)
            {
                if (handlers[i].Name == command)
                {
                    if (handlers[i].Handler != null)
                    {
                        if (BaseCommands.HandleCommand(plr, command, text))
                            handlers[i].Handler.Invoke(plr, text);

                        break;
                    }
                    if (handlers[i].SubHandler != null)
                    {
                        GetCommand(plr, text, handlers[i].SubHandler);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Functions

        public static void PlayerQuit(Player plr, string text) { if (!plr.Leaving) plr.Quit(true, false); }
        public static void PlayerExit(Player plr, string text) { plr.DisconnectTime = 0; plr.Quit(); }
        public static void PlayerStuck(Player plr, string text)
        {
            plr.HandleStuck();
        }

        #region Text Chat

        public static void PlayerSay(Player plr, string text)
        {
            if (!plr.IsDead)
                plr.Say(text);
        }

        public static void Skol(Player plr, string text)
        {
            if (!plr.IsDead)
                plr.Say($"Yardy wants you to buy him another beer. {text}" );
        }

        public static void Rude(Player plr, string text)
        {
            if (!plr.IsDead)
                plr.Say($"{plr.Name} makes some rude gestures.");
        }

        public static void Insult(Player plr, string text)
        {
            if (!plr.IsDead)
                plr.Say($"{plr.Name} compares you to something that does not bear repeating.");
        }


        public static void PlayerWisp(Player plr, string text)
        {
            if (plr.IsBanned)
            {
                plr.SendClientMessage("You suddenly remember that in space, no one can hear you scream.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            int pos = text.IndexOf(' ');
            if (pos < 0)
                return;

            string receiverName = text.Substring(0, pos);
            text = text.Remove(0, pos + 1);

            Player receiver = Player.GetPlayer(receiverName);

            if (receiver == null || !receiver.IsInWorld() || (plr.GmLevel == 1 && receiver.GmLevel == 1 && !Program.Config.ChatBetweenRealms && plr.Realm != receiver.Realm))
                plr.SendLocalizeString(receiverName, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
            else if (receiver == plr)
                plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_TELL_ERR_TO_SELF);
            else if (plr.GmLevel < 1 && receiver.IsBanned)
                plr.SendClientMessage("You get the feeling that trying to talk to " + receiver.Name + " wouldn't be a good use of your time.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
            else
            {
                if (plr.GmLevel == 1 && receiver.SocInterface.HasIgnore(plr.CharacterId))
                {
                    plr.SendLocalizeString(receiverName, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_IGNORED_WARNING);
                }
                else if (receiver.SocInterface.blocksTells && plr.GmLevel == 1 && (plr.GldInterface.Guild == null || receiver.GldInterface.Guild == null || plr.GldInterface.Guild != receiver.GldInterface.Guild))
                {
                    if (receiver.GmLevel > 1)
                    {
                        plr.SendClientMessage("This player is a staff member and is currently rejecting communications.");
                        plr.SendClientMessage("To report bugs, use the Bugtracker - do not report bugs via in-game chat.");
                        plr.SendClientMessage("For GM assistance, please ask on the forum.");
                    }

                    else
                    {
                        plr.SendClientMessage("This player is currently blocking tells from all players outside of their guild.");
                        plr.SendClientMessage("Please do not attempt to communicate directly with this player using global chat after having seen this message.");
                    }
                }
                else
                {
                    receiver.SendMessage(plr.Oid, plr.Name, text, plr.BroadcastRank && plr.GmLevel > 1 ? ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE : ChatLogFilters.CHATLOGFILTERS_TELL_RECEIVE, 0);
                    plr.SendMessage(plr.Oid, receiver.Name, text, ChatLogFilters.CHATLOGFILTERS_TELL_SEND, 0);
                }
            }
        }
        public static void PlayerShout(Player plr, string text) { if (!plr.IsDead) plr.Say(text, ChatLogFilters.CHATLOGFILTERS_SHOUT); }
        public static void PlayerEmoteSay(Player plr, string text) { if (!plr.IsDead && plr.Info.TempFirstName == null) plr.Say(text, ChatLogFilters.CHATLOGFILTERS_EMOTE); }

        public static void PlayerAd(Player sender, string messageString)
        {
            if (sender.IsBanned)
            {
                sender.SendClientMessage("You get the feeling that it might have been better to ask for advice BEFORE being sent here.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            if (sender.ShouldThrottle())
                return;

            if (sender.AdviceBlocked)
            {
                sender.SendClientMessage("[System] Due to previous abuse, you're currently blocked from using the Advice channel.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return;
            }

            // Delete the Say
            messageString = messageString.Remove(0, 4);

            string lowerString = messageString.ToLower();

            if (sender.GmLevel == 1)
                foreach (string banned in _forbiddenLinks)
                {
                    if (lowerString.IndexOf(banned, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        sender.SendClientMessage("[System] Links are not permitted in global chats due to advertisement issues. Evasion of this rule will result in punishment. Refer to the site by its name instead.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                        return;
                    }
                }

            // Catch common abuses
            if (sender.GmLevel == 1)
            {
                if (lowerString.StartsWith("gm on") || lowerString.StartsWith("any gm") || lowerString.StartsWith("any gamemaster") || lowerString.StartsWith("any game master") || lowerString.StartsWith("any g m"))
                {
                    sender.SendClientMessage("[System] Please use the ingame function to report an issue by heading to the helpmenu and submitting a CSR ticket.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    sender.SendClientMessage("[System] If you feel this is not enough, contact a GM via the forums. If you need to report someone, be sure to include screenshots of the full chat log, or video if possible.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    List<string> dummyList = new List<string>();
                    GmMgr.ListGameMasters(sender, ref dummyList);
                    return;
                }

                if (lowerString == "any dev online")
                {
                    sender.SendClientMessage("[System] Developers will not answer queries.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    sender.SendClientMessage("[System] If you have a bug to report, use the Bugtracker.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    sender.SendClientMessage("[System] For anything else, use the forum.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    return;
                }

                if (messageString.Length < 10 && !messageString.Any(char.IsLetterOrDigit))
                {
                    sender.SendClientMessage("[System] A message with no text was detected. Using Advice for 'add me' and '+' messages is forbidden.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    sender.SendClientMessage("[System] Bypassing this filter by using a fully text-based message for the same purpose will result in an instant 30 day advice block.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    return;
                }
            }

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player.Realm == sender.Realm && !player.BlocksChatFrom(sender))
                        player.SendMessage(sender.Oid, sender.ChatName, messageString, ChatLogFilters.CHATLOGFILTERS_HELP_MESSAGE);
                }
            }
        }

        private static string[] _forbiddenLinks = { "http:", "https:", "www.", ".com", ".org", ".net" };

        public static void PlayerChan(Player sender, string messageString)
        {
            string originalMessage = messageString;

            if (messageString.Length == 0)
                return;

            int index = messageString.IndexOf(" ");

            if (index > 0)
            {
                string chanName = messageString.Substring(0, index);
                messageString = messageString.Remove(0, index + 1);

                if (messageString.Length == 0)
                    return;

                index = messageString.IndexOf(" ");

                if (index < 1)
                    return;

                byte channelNumber;

                try
                {
                    if (!byte.TryParse(messageString.Substring(0, index), out channelNumber) || channelNumber == 0)
                        return;
                }
                catch (Exception e)
                {
                    Log.Info("Channel System", e.GetType() + " with original message of " + originalMessage);
                    return;
                }

                messageString = messageString.Remove(0, index + 1);

                if (sender.GmLevel == 1)
                    foreach (string banned in _forbiddenLinks)
                    {
                        if (messageString.IndexOf(banned, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            sender.SendClientMessage("[System] Links are not permitted in global chats due to advertisement issues. Evasion of this rule will result in punishment. Refer to the site by its name instead.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                            return;
                        }
                    }

                // We assume for now that Channel 1 is always Region and Channel 2 is always Region RvR, and drop the rest
                switch (channelNumber)
                {
                    case 0: // redirected to Scenario Party Say for some reason
                        ScenarioPartySay(sender, messageString);
                        break;

                    case 1: // Region
                        if (sender.IsBanned)
                        {
                            sender.SendClientMessage("It dawns on you that there's no one else here to talk to.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                            return;
                        }

                        if (sender.ShouldThrottle())
                            return;

                        lock (Player._Players)
                            foreach (Player player in Player._Players)
                            {
                                if (player.Realm != sender.Realm || player.Region != sender.Region || player.BlocksChatFrom(sender) || player.ScnInterface.Scenario != null)
                                    continue;
                                player.SendMessage(sender.Oid, sender.ChatName, messageString, ChatLogFilters.CHATLOGFILTERS_CHANNEL_BASE, channelNumber);
                            }
                        break;

                    case 2: // Region-RvR
                        if (sender.IsBanned)
                        {
                            sender.SendClientMessage("You consider the possibility of there being RvR battles on the moon, and find it to be zero.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                            return;
                        }

                        if (sender.ShouldThrottle())
                            return;

                        if (!sender.CbtInterface.IsPvp)
                        {
                            sender.SendClientMessage("You can't interact with the Region-RvR channel when not flagged.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                            return;
                        }
                        if (sender.ScnInterface.Scenario != null)
                        {
                            sender.SendClientMessage("Region-RvR is not accessible from within scenarios.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                            return;
                        }
                        lock (Player._Players)
                            foreach (Player player in Player._Players)
                            {
                                if (player.Realm != sender.Realm || player.BlocksChatFrom(sender) || player.Region != sender.Region || !player.CbtInterface.IsPvp || player.ScnInterface.Scenario != null)
                                    continue;
                                player.SendMessage(sender.Oid, sender.ChatName, messageString, ChatLogFilters.CHATLOGFILTERS_CHANNEL_BASE, channelNumber);
                            }
                        break;

                    case 3: // General Chat
                    case 6: // Off-Topic
                    case 7: // Roleplaying
                    case 8: // Russian
                        if (sender.ShouldThrottle())
                            return;

                        lock (Player._Players)
                            foreach (Player player in Player._Players)
                            {
                                if (player.Realm != sender.Realm || player.BlocksChatFrom(sender))
                                    continue;
                                player.SendMessage(sender.Oid, sender.ChatName, messageString, ChatLogFilters.CHATLOGFILTERS_CHANNEL_BASE, channelNumber);
                            }
                        break;
                    case 4: // Trade
                        if (sender.IsBanned)
                        {
                            sender.SendClientMessage("You realize that it would be difficult to ply your wares from here and think better of doing so.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                            return;
                        }

                        if (sender.ShouldThrottle())
                            return;

                        lock (Player._Players)
                            foreach (Player player in Player._Players)
                            {
                                if (player.Realm != sender.Realm || player.BlocksChatFrom(sender))
                                    continue;

                                player.SendMessage(sender.Oid, sender.ChatName, messageString, ChatLogFilters.CHATLOGFILTERS_CHANNEL_BASE, channelNumber);
                            }
                        break;
                    case 5: // LFG
                        if (sender.ShouldThrottle())
                            return;

                        // Filter "+" spam 
                        bool isAdd = messageString.StartsWith("+");

                        lock (Player._Players)
                            foreach (Player player in Player._Players)
                            {
                                if (player.Realm != sender.Realm || player.BlocksChatFrom(sender))
                                    continue;

                                if (!isAdd || player == sender || (player.WorldGroup != null && player.WorldGroup.Leader == player))
                                    player.SendMessage(sender.Oid, sender.ChatName, messageString, ChatLogFilters.CHATLOGFILTERS_CHANNEL_BASE, channelNumber);
                            }
                        break;
                }

            }
        }

        public static void PlayerScenarioSay(Player plr, string messageString)
        {
            plr.ScnInterface.Scenario?.SayToTeam(plr, messageString);
        }

        // Input is "say <int tier> <string message>"
        public static void RealmWarSay(Player sender, string messageString)
        {
            if (sender.IsBanned)
            {
                sender.SendClientMessage("You feel somewhat disconnected from the affairs of your realm at the moment.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            if (sender.ShouldThrottle())
                return;

            // Delete the Say
            messageString = messageString.Remove(0, 4);

            byte tier;
            if (!byte.TryParse(messageString.Substring(0, 1), out tier))
            {
                sender.SendClientMessage("Invalid tier input.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            messageString = messageString.Remove(0, 2);

            if (!sender.ValidInTier(tier, true))
            {
                sender.SendClientMessage("You must be in Tier " + tier + " to use its Realm War channel.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player.Realm != sender.Realm || !player.ValidInTier(tier, true) || player.BlocksChatFrom(sender))
                        continue;

                    player.SendMessage(sender.Oid, sender.ChatName, messageString, (ChatLogFilters)(35 + tier), 0);
                }
            }
        }

        #endregion

        #region Friends

        public static void AddFriend(Player plr, string text) { plr.SocInterface.AddFriend(text); }
        public static void RemoveFriend(Player plr, string text) { plr.SocInterface.RemoveFriend(text); }

        #endregion

        #region Ignore

        public static void ToggleIgnore(Player plr, string text) { plr.SocInterface.Ignore(text); }

        #endregion

        #region Social

        public static void SocialAnon(Player plr, string text) { plr.SocInterface.Anon = !plr.SocInterface.Anon; }
        public static void SocialHide(Player plr, string text) { plr.SocInterface.Hide = !plr.SocInterface.Hide; }

        #endregion

        #region Duelling

        public static void DuelChallenge(Player plr, string text)
        {
            switch (text)
            {
                case "accept": plr.RespondToDuel(true); break;
                case "decline": plr.RespondToDuel(false); break;
                case "cancel": plr.RescindDuelOffer(); break;
                default: plr.OfferDuel(); break;
            }
        }

        #endregion

        #region Group

        public static void PartyInvite(Player plr, string name)
        {
            if (plr.WorldGroup != null)
            {
                if (plr.WorldGroup.RejectsMembers)
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_PARTY_IS_FULL);
                    return;
                }

                if (plr.WorldGroup.GetLeader() != plr)
                {
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                    return;
                }
            }

            Player receiver = Player.GetPlayer(name);
            if (receiver == null)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
            else if (receiver.Name == plr.Name)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_INVITE_ERR_SELF);
            else if (receiver.Realm != plr.Realm)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_INVITE_ERR_ENEMY);
            else
                plr.GrpInterface.TryInvite(receiver);
        }

        public static void PartyLeave(Player plr, string text)
        {
            Group worldGroup = plr.WorldGroup;

            uint groupId = 0;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                        }
                    }
                }
            }

            if (groupId != 0)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.PlayerLeave, plr));
        }

        public static void PartySay(Player plr, string text)
        {
            plr.WorldGroup?.SendMessageToGroup(plr, text);
        }

        public static void ScenarioPartySay(Player plr, string messageString)
        {
            string originalMessage = messageString;

            if (messageString.Length == 0)
                return;

            int index = messageString.IndexOf(" ");

            if (index > 0)
            {
                messageString = messageString.Remove(0, index + 1);

                if (messageString.Length == 0)
                    return;

                plr.ScenarioGroup?.SendMessageToGroup(plr, messageString);
            }
        }

        public static void WarbandSay(Player plr, string text)
        {
            plr.WorldGroup?.SendMessageToWarband(plr, text);
        }

        public static void PartyJoin(Player plr, string text)
        {
            if (plr.WorldGroup != null)
            {
                plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, plr.WorldGroup.IsWarband ? Localized_text.TEXT_BG_ALREADY_IN : Localized_text.TEXT_GROUP_ALREADY_IN_ONE);
                return;
            }

            if (plr.IsBanned)
            {
                plr.SendClientMessage("You call out to your allies. But there was no one to listen.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            foreach (Group g in Group.GetWorldGroups(plr.Info.Realm))
            {
                if (!g.PartyOpen || g.Leader?.Name == null || !g.Leader.Name.Equals(text))
                    continue;

                Group worldGroup = g;
                uint groupId = 0;
                if (worldGroup != null)
                {
                    lock (worldGroup)
                    {
                        if (worldGroup != null)
                        {
                            if (worldGroup._warbandHandler != null)
                            {
                                lock (worldGroup._warbandHandler)
                                {
                                    if (worldGroup._warbandHandler != null)
                                    {
                                        groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                    }
                                }
                            }
                            else
                            {
                                groupId = worldGroup.GroupId;
                            }
                        }
                    }
                }
                if (groupId == 0)
                    continue;

                if (plr.GrpInterface.AttemptGroupStateChange(EGroupJoinState.PendingJoin))
                    Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.PlayerJoin, plr));
                break;
            }
        }

        public static void PartyKick(Player plr, string text)
        {

            Group worldGroup = plr.WorldGroup;

            uint groupId = 0;
            bool isLeader = false;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                    isLeader = worldGroup._warbandHandler.Leader == plr;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                            isLeader = worldGroup.Leader == plr;
                        }
                    }
                }
            }

            if (groupId != 0 && isLeader)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.PlayerKick, plr, text));
        }

        public static void PartyOpen(Player plr, string text)
        {

            Group worldGroup = plr.WorldGroup;

            uint groupId = 0;
            bool isLeader = false;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                    isLeader = worldGroup._warbandHandler.Leader == plr;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                            isLeader = worldGroup.Leader == plr;
                        }
                    }
                }
            }

            if (groupId != 0 && isLeader)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.OpenParty, plr));
            else if (groupId == 0)
            {
                Group group = new Group();

                if (group != null)
                {
                    lock (group)
                    {
                        if (group != null)
                        {
                            if (group._warbandHandler != null)
                            {
                                lock (group._warbandHandler)
                                {
                                    if (group._warbandHandler != null)
                                    {
                                        group.InitializeSolo(plr);
                                        groupId = group._warbandHandler.ZeroIndexGroupId;
                                        isLeader = group._warbandHandler.Leader == plr;
                                        group.PartyOpen = true;
                                    }
                                }
                            }
                            else
                            {
                                group.InitializeSolo(plr);
                                groupId = group.GroupId;
                                isLeader = group.Leader == plr;
                                group.PartyOpen = true;
                            }
                        }
                    }
                }
            }
        }

        public static void PartyClose(Player plr, string text)
        {
            Group worldGroup = plr.WorldGroup;

            uint groupId = 0;
            bool isLeader = false;
            bool emptyGroup = true;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                    isLeader = worldGroup._warbandHandler.Leader == plr;
                                    emptyGroup = worldGroup._warbandHandler.MemberCount <= 1;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                            isLeader = worldGroup.Leader == plr;
                            emptyGroup = worldGroup.MemberCount <= 1;
                        }
                    }
                }
            }

            if (groupId != 0 && isLeader && emptyGroup)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.PlayerLeave, plr));
            else if (groupId != 0 && isLeader)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.CloseParty, plr));
        }

        public static void PartyChangeLeader(Player plr, string text)
        {
            Group worldGroup = plr.WorldGroup;

            uint groupId = 0;
            bool isLeader = false;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                    isLeader = worldGroup._warbandHandler.Leader == plr;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                            isLeader = worldGroup.Leader == plr;
                        }
                    }
                }
            }

            if (groupId != 0 && isLeader)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeLeader, plr, text));
        }

        public static void PartyChangeMainAssist(Player plr, string text)
        {
            Group worldGroup = plr.WorldGroup;

            uint groupId = 0;
            bool isLeader = false;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                    isLeader = worldGroup._warbandHandler.Leader == plr;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                            isLeader = worldGroup.Leader == plr;
                        }
                    }
                }
            }

            if (groupId != 0 && isLeader)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeMainAssist, plr, text));
        }

        public static void PartyRoll(Player plr, string text)
        {
            plr.WorldGroup?.PartyRoll(plr, text);
        }

        public static void GuildRemoveStandardBearer(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.RemoveStandardBearer(plr, text);
        }

        public static void GuildSetStandardBearer(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.SetStandardBearer(plr, text);
        }

        public static void PartyCommand(Player plr, string text)
        {
            if (text.StartsWith("open"))
                PartyOpen(plr, text);
            else if (text.StartsWith("private"))
                PartyClose(plr, text);
        }

        public static void WarbandCommand(Player plr, string text)
        {
            Group worldGroup = plr.WorldGroup;

            uint groupId = 0;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                        }
                    }
                }
            }

            if (groupId == 0)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_NOT_IN_BG);
                return;
            }
            if (text.StartsWith("move"))
            {
                int pos = text.IndexOf(' ');
                if (pos > 0)
                {
                    text = text.Remove(0, pos + 1);



                    Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.WarbandMove, plr, text));
                }
            }
            else if (text.StartsWith("swap"))
            {
                int pos = text.IndexOf(' ');
                if (pos > 0)
                {
                    text = text.Remove(0, pos + 1);
                    Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.WarbandSwap, plr, text));
                }
            }
            else if (text.StartsWith("convert 1"))
                PartyOpen(plr, text);
            else if (text.StartsWith("convert 0"))
                PartyClose(plr, text);
            else if (text.StartsWith("convert"))
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.FormWarband, plr));
            else if (text.StartsWith("promote"))
                PartyChangeMainAssist(plr, text.Split(' ')[1]);
            else if (text.StartsWith("makeleader"))
                PartyChangeLeader(plr, text.Split(' ')[1]);
            else
            {
                if (groupId != 0)
                    Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.FormWarband, plr));
                else
                    plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_NOT_IN_GROUP);
            }
        }

        public static void SocialCommand(Player plr, string text)
        {
            if (text.StartsWith("showcloakheraldry"))
                ToggleCloakHeraldry(plr, text);
            else if (text.StartsWith("anon"))
                SocialAnon(plr, text);
            else if (text.StartsWith("hide"))
                SocialHide(plr, text);
            else if (text.StartsWith("openparty"))
            {
                uint groupId = 0;
                bool isLeader = false;
                bool PartyState = true; //is closed, must open
                if (plr.WorldGroup != null)
                {
                    lock (plr.WorldGroup)
                    {
                        if (plr.WorldGroup != null)
                        {
                            if (plr.WorldGroup._warbandHandler != null)
                            {
                                lock (plr.WorldGroup._warbandHandler)
                                {
                                    if (plr.WorldGroup._warbandHandler != null)
                                    {
                                        groupId = plr.WorldGroup._warbandHandler.ZeroIndexGroupId;
                                        isLeader = plr.WorldGroup._warbandHandler.Leader == plr;
                                        PartyState = !plr.WorldGroup._warbandHandler.PartyOpen;
                                    }
                                }
                            }
                            else
                            {
                                groupId = plr.WorldGroup.GroupId;
                                isLeader = plr.WorldGroup.Leader == plr;
                                PartyState = !plr.WorldGroup.PartyOpen;
                            }
                        }
                    }
                }
                if (PartyState == true) // is closed, must open
                    PartyOpen(plr, text);
                else
                    PartyClose(plr, text);
            }
        }

        #endregion

        #region Guild

        public static void GuildSay(Player sender, string text)
        {
            if (sender.GldInterface.IsInGuild())
            {
                if (sender.IsBanned)
                {
                    sender.SendClientMessage("You reach out to your guild for help. But they can't hear you from here...", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                    return;
                }
                sender.GldInterface.Say(text);
            }
        }

        public static void GuildOfficerSay(Player plr, string text)
        {
            if (plr.GldInterface.IsInGuild())
                plr.GldInterface.OfficerSay(text);
        }

        public static void APromote(Player plr, string text)
        {
            if (plr.GldInterface.IsInGuild() && plr.GldInterface.Guild.Info.LeaderId == plr.CharacterId && plr.GldInterface.Guild.Info.AllianceId > 0)
                plr.GldInterface.Guild.APromote(plr, text);
        }

        public static void ADemote(Player plr, string text)
        {
            if (plr.GldInterface.IsInGuild() && plr.GldInterface.Guild.Info.LeaderId == plr.CharacterId && plr.GldInterface.Guild.Info.AllianceId > 0)
                plr.GldInterface.Guild.ADemote(plr, text);
        }

        public static void AllianceSay(Player plr, string text)
        {
            plr.GldInterface.AllianceSay(text);
        }

        public static void AllianceOfficerSay(Player plr, string text)
        {
            plr.GldInterface.AllianceOfficerSay(text);
        }

        public static void AllianceInvite(Player plr, string text)
        {
            Player receiver = Player.GetPlayer(text);

            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_INV_ERR_NOGUILD);
                return;
            }
            if (receiver == null)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
            else if (receiver.Name == plr.Name)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_DECLINE_SELF);
            else if (receiver.Realm != plr.Realm)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_INV_ERR_ENEMY);
            else if (plr.GldInterface.Guild.Info.AllianceId == 0)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_ERR_NOALLIANCE);
            else if (Alliance.Alliances[plr.GldInterface.Guild.Info.AllianceId].Members.Count >= 10)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_INV_ERR_MAX);
            else if (receiver.GldInterface.Guild.Info.AllianceId > 0)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_TALLIED);
            else if (receiver.GldInterface.Guild.Info.Level < 6)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_NEWBIE_GUILD);
            else if (receiver.GldInterface.Guild.Info.LeaderId != receiver.CharacterId)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_TLEADER);
            else if (receiver.GldInterface.AllianceinvitedTo > 0)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_PLAYER_PENDING_ANOTHER);
            else
            {
                receiver.GldInterface.AllianceinvitedTo = plr.GldInterface.Guild.Info.AllianceId;
                plr.SendLocalizeString(text, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_ALLIANCE_INVITE_BEGIN);
                receiver.SendDialog(Dialog.AllianceInvite, plr.Name, Alliance.Alliances[plr.GldInterface.Guild.Info.AllianceId].Name);
            }

        }

        public static void AllianceForm(Player plr, string text)
        {
            String[] tmp = text.Split(' ');

            Player receiver = Player.GetPlayer(tmp[0]);
            String alliname = "";
            for (int i = 1; i < tmp.Length; i++)
            {
                alliname += tmp[i];
                if (i + 1 < tmp.Length)
                    alliname += " ";
            }

            if (receiver == null)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
                return;
            }
            if (!plr.GldInterface.IsInGuild() || !receiver.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_INV_ERR_NOGUILD);
                return;
            }
            else if (receiver.Name == plr.Name)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_DECLINE_SELF);
            else if (receiver.Realm != plr.Realm)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_INV_ERR_ENEMY);
            else if (plr.GldInterface.Guild.Info.AllianceId > 0)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_ALLIED);
            else if (receiver.GldInterface.Guild.Info.AllianceId > 0)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_TALLIED);
            else if (receiver.GldInterface.Guild.Info.Level < 6)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_NEWBIE_GUILD);
            else if (plr.GldInterface.Guild.Info.LeaderId != plr.CharacterId)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_LEADER);
            else if (receiver.GldInterface.Guild.Info.LeaderId != receiver.CharacterId)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_TLEADER);
            else if (receiver.GldInterface.AllianceinvitedTo > 0)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_PLAYER_PENDING_ANOTHER);
            else
            {
                Boolean unique = true;
                foreach (KeyValuePair<uint, Guild_Alliance_info> alli in Alliance.Alliances)
                {
                    if (alli.Value.Name == alliname)
                        unique = false;
                }

                if (unique == false)
                {
                    // Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, "Name already taken" );
                    return;
                }

                receiver.GldInterface.AllianceFormName = alliname;
                receiver.GldInterface.AllianceFormGuildId = plr.GldInterface.Guild.Info.GuildId;

                receiver.GldInterface.AllianceinvitedTo = plr.GldInterface.Guild.Info.AllianceId;
                plr.SendLocalizeString(text, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_ALLIANCE_FORM_BEGIN);
                receiver.SendLocalizeString(text, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_ALLIANCE_FORM_DIALOG);
                receiver.SendDialog(Dialog.AllianceInvite, plr.Name, alliname);
            }
        }
        public static void AllianceLeave(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_INV_ERR_NOGUILD);
                return;
            }
            if (plr.GldInterface.Guild.Info.LeaderId != plr.CharacterId)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ALLIANCE_FORM_ERR_LEADER);
            else
                plr.GldInterface.Guild.LeaveAlliance();

        }
        public static void AllianceList(Player plr, string text)
        {

            // todo AllianceList

        }




        public static void GuildInvite(Player plr, string text)
        {
            Player receiver = Player.GetPlayer(text);

            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            if (receiver == null)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
            else if (receiver.Name == plr.Name)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_INVITE_ERR_SELF);
            else if (receiver.Realm != plr.Realm)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_INVITE_ERR_ENEMY);
            else if (receiver.GldInterface.IsInGuild())
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_INVITE_ERR_GUILDED);
            else if (receiver.GldInterface.invitedTo != null)
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_PLAYER_PENDING_ANOTHER);
            else
            {
                receiver.GldInterface.invitedTo = plr.GldInterface.Guild;
                plr.SendLocalizeString(text, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GUILD_INVITE_BEGIN);
                receiver.SendDialog(Dialog.GuildInvite, plr.Name, plr.GldInterface.GetGuildName());
            }
        }

        public static void GuildRankDisable(Player plr, string text)
        {
            byte rank = byte.Parse(text);

            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.RankToggle(plr, rank, false);
        }

        public static void GuildRankEnable(Player plr, string text)
        {
            byte rank = byte.Parse(text);

            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.RankToggle(plr, rank, true);
        }

        public static void GuildKick(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.PlayerKick(plr, text);
        }

        public static void GuildMotd(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.SetMotd(plr, text);
        }

        public static void GuildDetails(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.SetDetails(plr, text);
        }

        public static void GuildPermission(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            string[] strings = text.Split(' ');

            plr.GldInterface.Guild.SetPermissions(plr, text);

        }

        public static void GuildNote(Player plr, string Text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            string[] tmp = Text.Split(' ');

            string text = "";
            for (int i = 1; i < tmp.Length; i++)
            {
                text += tmp[i];
                if (i + 1 < tmp.Length)
                    text += " ";
            }

            plr.GldInterface.Guild.SetPublicNote(plr, tmp[0], text);
        }

        public static void GuildOfficerNote(Player plr, string Text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            string[] tmp = Text.Split(' ');

            string text = "";
            for (int i = 1; i < tmp.Length; i++)
            {
                text += tmp[i];
                if (i + 1 < tmp.Length)
                    text += " ";
            }

            plr.GldInterface.Guild.SetOfficerNote(plr, tmp[0], text);
        }

        public static void GuildRecruiter(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }

            plr.GldInterface.Guild.RecruiterToggle(plr, text);
        }

        public static void GuildRankRename(Player plr, string Text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }
            string[] tmp = Text.Split(' ');

            string text = "";
            for (int i = 1; i < tmp.Length; i++)
            {
                text += tmp[i];
                if (i + 1 < tmp.Length)
                    text += " ";
            }
            plr.GldInterface.Guild.SetRankName(plr, byte.Parse(tmp[0]), text);
        }
        public static void GuildSetTax(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }
            string[] strings = text.Split(' ');
            plr.GldInterface.Guild.SetTax(plr, text);
        }
        public static void GuildSetTithe(Player plr, string text)
        {
            if (!plr.GldInterface.IsInGuild())
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_NOT_IN_A_GUILD);
                return;
            }
            string[] strings = text.Split(' ');
            plr.GldInterface.Guild.SetTithe(plr, text);
        }

        #endregion

        #region Appearance

        public static void ToggleHelm(Player plr, string text)
        {
            byte toggle;
            if (byte.TryParse(text, out toggle))
                plr.SetGearShowing(1, toggle > 0);
        }

        public static void ToggleCloak(Player plr, string text)
        {
            byte toggle;
            if (byte.TryParse(text, out toggle))
                plr.SetGearShowing(2, toggle > 0);
        }

        public static void ToggleCloakHeraldry(Player plr, string text)
        {
            if (text.StartsWith("showcloakheraldry"))
                plr.SetGearShowing(4, (plr._Value.GearShow & 4) <= 0);
        }

       
        #endregion

        #region Pets

        public static void SetPetName(Player plr, string text)
        {
            CareerInterface_WhiteLion petInterface = plr.CrrInterface as CareerInterface_WhiteLion;

            if (petInterface != null && !string.IsNullOrEmpty(text) && text.Length <= 20 && text.All(char.IsLetterOrDigit) && plr.Info.PetName != text)
            {
                plr.Info.PetName = text;
                petInterface.myPetName = text;
                plr.SaveCharacterInfo();
                plr.ForceSave();
            }
        }

        #endregion

        public static void Target(Player plr, string text)
        {

        }

        public static void Time(Player plr, string text)
        {
            plr.SendClientMessage("The current server time is " + DateTime.Now.ToShortTimeString() + ".");
        }

        public static void Roll(Player plr, string text)
        {
            if (TCPManager.GetTimeStampMS() < plr.LastEmoteTime + 2000)
                return;

            if (string.IsNullOrEmpty(text))
                return;

            ushort max;

            if (!ushort.TryParse(text, out max) || max == 0)
                max = 100;

            lock (plr.PlayersInRange)
            {
                foreach (Player player in plr.PlayersInRange)
                    player.SendLocalizeString(new[] { plr.Name, "1", max.ToString(), StaticRandom.Instance.Next(1, max + 1).ToString() }, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_ROLL_PUBLIC_BEGIN);
            }

            plr.SendLocalizeString(new[] { plr.Name, "1", max.ToString(), StaticRandom.Instance.Next(1, max + 1).ToString() }, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_ROLL_PUBLIC_BEGIN);

            plr.LastEmoteTime = TCPManager.GetTimeStampMS();
        }

        #endregion
    }
}
