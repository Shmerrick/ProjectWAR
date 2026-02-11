using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Common;
using FrameWork;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    public static class CommandDocumentation
    {
        private static readonly Dictionary<string, string> PlayerCommandDescriptions = new Dictionary<string, string>
        {
            { "/afk", "Sets your status to Away From Keyboard." },
            { "/alliance", "Alliance chat." },
            { "/a", "Alliance chat." },
            { "/as", "Alliance chat." },
            { "/allianceofficersay", "Alliance officer chat." },
            { "/ao", "Alliance officer chat." },
            { "/aos", "Alliance officer chat." },
            { "/anonymous", "Toggles anonymous mode." },
            { "/appeal", "Opens the appeal window." },
            { "/appealview", "Views an appeal." },
            { "/appealcancel", "Cancels an appeal." },
            { "/assist", "Assists your target." },
            { "/aid", "Assists your target." },
            { "/bug", "Opens the bug report window." },
            { "/channel", "Channel command." },
            { "/chan", "Channel command." },
            { "/channelwho", "Shows who is in a channel." },
            { "/cloaktoggle", "Toggles your cloak." },
            { "/cloak", "Toggles your cloak." },
            { "/count", "Shows a count of players in your area." },
            { "/debugwindow", "Opens the debug window." },
            { "/duelchallenge", "Challenges a player to a duel." },
            { "/duel", "Challenges a player to a duel." },
            { "/duelsurrender", "Surrenders a duel." },
            { "/emote", "Performs an emote." },
            { "::", "Performs an emote." },
            { "/emotelist", "Lists all emotes." },
            { "/friend", "Friend command." },
            { "/guild", "Guild chat." },
            { "/g", "Guild chat." },
            { "/gc", "Guild command." },
            { "/o", "Guild officer chat." },
            { "/follow", "Follows your target." },
            { "/helmtoggle", "Toggles your helm." },
            { "/helm", "Toggles your helm." },
            { "/hide", "Hides you from other players." },
            { "/ignoreadd", "Adds a player to your ignore list." },
            { "/ignorelist", "Lists all players on your ignore list." },
            { "/ignoreremove", "Removes a player from your ignore list." },
            { "/ignoretoggle", "Toggles ignoring a player." },
            { "/ignore", "Ignore command." },
            { "/inspect", "Inspects your target." },
            { "/inspectable", "Toggles whether you can be inspected." },
            { "/inspectablebraggingrights", "Toggles whether your bragging rights can be inspected." },
            { "/inspectabletradeskills", "Toggles whether your trade skills can be inspected." },
            { "/join", "Joins a group." },
            { "/language", "Sets your language." },
            { "/lfguild", "Looks for a guild." },
            { "/lfm", "Looks for more." },
            { "/lfp", "Looks for a party." },
            { "/lfg", "Looks for a group." },
            { "/location", "Shows your current location." },
            { "/loc", "Shows your current location." },
            { "/lockouts", "Shows your instance lockouts." },
            { "/logout", "Logs you out of the game." },
            { "/camp", "Logs you out of the game." },
            { "/openlist", "Opens a list." },
            { "/openpartyinterest", "Opens the party interest window." },
            { "/openjoin", "Joins an open party." },
            { "/partyroll", "Rolls for loot in a party." },
            { "/partyrandom", "Rolls for loot in a party." },
            { "/partyresetinstance", "Resets a party instance." },
            { "/partynote", "Sets a party note." },
            { "/openpartynote", "Sets an open party note." },
            { "/lfpnote", "Sets a looking for party note." },
            { "/partysay", "Party chat." },
            { "/p", "Party chat." },
            { "/partyjoin", "Joins a party." },
            { "/partyinvite", "Invites a player to a party." },
            { "/invite", "Invites a player to a party." },
            { "/partyinviteopen", "Invites a player to an open party." },
            { "/oinvite", "Invites a player to an open party." },
            { "/partyremove", "Removes a player from a party." },
            { "/partykick", "Kicks a player from a party." },
            { "/partyboot", "Kicks a player from a party." },
            { "/insult", "Insults your target." },
            { "/kick", "Kicks a player from a party." },
            { "/partyleader", "Sets the party leader." },
            { "/makeleader", "Sets the party leader." },
            { "/mainassist", "Sets the main assist." },
            { "/makemainassist", "Sets the main assist." },
            { "/partyleave", "Leaves a party." },
            { "/partyquit", "Leaves a party." },
            { "/leave", "Leaves a party." },
            { "/disband", "Disbands a party." },
            { "/pc", "Party command." },
            { "/partylfwarband", "Looks for a warband." },
            { "/partylfw", "Looks for a warband." },
            { "/partyopen", "Opens a party." },
            { "/partyprivate", "Closes a party." },
            { "/partyclose", "Closes a party." },
            { "/partypassword", "Sets a party password." },
            { "/partyguildonly", "Sets a party to guild only." },
            { "/partyallianceonly", "Sets a party to alliance only." },
            { "/partylist", "Lists all parties." },
            { "/petname", "Sets your pet's name." },
            { "/played", "Shows your played time." },
            { "/random", "Rolls a random number." },
            { "/roll", "Rolls a random number." },
            { "/reply", "Replies to a whisper." },
            { "/r", "Replies to a whisper." },
            { "/transferguild", "Transfers guild leadership." },
            { "/reportgoldseller", "Reports a gold seller." },
            { "/rgs", "Reports a gold seller." },
            { "/rg", "Reports a gold seller." },
            { "/reloadui", "Reloads your UI." },
            { "/refund", "Refunds an item." },
            { "/respec", "Respecializes your character." },
            { "/rvr", "Toggles your RvR flag." },
            { "/pvp", "Toggles your PvP flag." },
            { "/rvrmap", "Opens the RvR map." },
            { "/quit", "Quits the game." },
            { "/exit", "Exits the game." },
            { "/q", "Quits the game." },
            { "/rude", "Makes a rude gesture." },
            { "/say", "Say something." },
            { "/s", "Say something." },
            { "'", "Say something." },
            { "/scenariosay", "Scenario chat." },
            { "/sc", "Scenario chat." },
            { "/scenariotell", "Scenario tell." },
            { "/sp", "Scenario party chat." },
            { "/script", "Executes a script." },
            { "/shout", "Shouts something." },
            { "/showcloakheraldry", "Toggles your cloak heraldry." },
            { "/skol", "Skol!" },
            { "/social", "Social command." },
            { "/stuck", "Gets you unstuck." },
            { "/target", "Targets a player." },
            { "/tell", "Sends a whisper." },
            { "/t", "Sends a whisper." },
            { "/whisper", "Sends a whisper." },
            { "/w", "Sends a whisper." },
            { "/msg", "Sends a whisper." },
            { "/send", "Sends a whisper." },
            { "/time", "Shows the current time." },
            { "/togglecloakheraldry", "Toggles your cloak heraldry." },
            { "/togglealwaysformprivate", "Toggles always forming a private party." },
            { "/warband", "Warband chat." },
            { "/war", "Warband chat." },
            { "/wbc", "Warband command." },
            { "/wb", "Warband chat." },
            { "/ra", "Warband chat." },
            { "/who", "Shows who is online." },
            { "/advs", "Advice chat." },
            { "/schan", "Scenario party chat." },
            { "/rws", "Realm war say." },
            { "/", "Command." },
        };

        public static string GenerateMarkdown()
        {
            var gmCommands = GetAllGmCommands();
            var playerCommands = GetAllSlashCommands();
            var sb = new StringBuilder();

            sb.AppendLine("# Game Commands Documentation");
            sb.AppendLine();

            sb.AppendLine("## GM Commands");
            foreach (var command in gmCommands.OrderBy(c => c.Name))
            {
                sb.AppendLine($"### .{command.Name}");
                sb.AppendLine($"**Description:** {command.Description}  ");
                sb.AppendLine($"**Access Level:** {command.AccessRequired}  ");
                if (command.SubCommands != null && command.SubCommands.Count > 0)
                {
                    sb.AppendLine("**Sub-commands:**  ");
                    foreach (var sub in command.SubCommands.OrderBy(s => s.Name))
                    {
                        sb.AppendLine($"- **.{command.Name} {sub.Name}**: {sub.Description}");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine("## Player Commands");
            foreach (var command in playerCommands.OrderBy(c => c.Name))
            {
                sb.AppendLine($"### {command.Name}");
                sb.AppendLine($"**Description:** {command.Description}  ");
                if (command.SubCommands != null && command.SubCommands.Count > 0)
                {
                    sb.AppendLine("**Sub-commands:**  ");
                    foreach (var sub in command.SubCommands.OrderBy(s => s.Name))
                    {
                        sb.AppendLine($"- **{command.Name} {sub.Name}**: {sub.Description}");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static List<string> GetCommandHelp(Player plr, bool gm)
        {
            if (gm)
            {
                var commands = GetAllGmCommands();
                return BuildGmHelpLines(commands, plr);
            }
            else
            {
                var commands = GetAllSlashCommands();
                return BuildPlayerHelpLines(commands);
            }
        }

        private static List<GmCommandHandler> GetAllGmCommands()
        {
            var commandHandlers = new List<GmCommandHandler>();
            var commandDeclarationsType = typeof(CommandDeclarations);
            var fields = commandDeclarationsType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.GetValue(null) is List<GmCommandHandler> list)
                {
                    commandHandlers.AddRange(list);
                }
            }
            return commandHandlers;
        }

        private static List<GmCommandHandler> GetAllSlashCommands()
        {
            var commandHandlers = new List<GmCommandHandler>();
            var commandMgrType = typeof(CommandMgr);
            var fields = commandMgrType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.GetValue(null) is CommandHandler[] list)
                {
                    foreach (var handler in list)
                    {
                        PlayerCommandDescriptions.TryGetValue(handler.Name, out var desc);
                        var cmd = new GmCommandHandler(handler.Name, null, null, EGmLevel.Anyone, 0, desc ?? "");
                        if (handler.SubHandler != null)
                        {
                            cmd.SubCommands = new List<GmCommandHandler>();
                            foreach (var sub in handler.SubHandler)
                            {
                                PlayerCommandDescriptions.TryGetValue(sub.Name, out var subDesc);
                                cmd.SubCommands.Add(new GmCommandHandler(sub.Name, null, null, EGmLevel.Anyone, 0, subDesc ?? ""));
                            }
                        }
                        commandHandlers.Add(cmd);
                    }
                }
            }
            return commandHandlers;
        }

        private static List<string> BuildGmHelpLines(List<GmCommandHandler> commands, Player plr)
        {
            var lines = new List<string>();
            lines.Add("Available GM commands:");

            foreach (var command in commands.OrderBy(c => c.Name))
            {
                if (plr.GmLevel >= (int)command.AccessRequired)
                {
                    lines.Add($".{command.Name} - {command.Description}");
                    if (command.SubCommands != null && command.SubCommands.Count > 0)
                    {
                        foreach (var sub in command.SubCommands.OrderBy(s => s.Name))
                        {
                             if (plr.GmLevel >= (int)sub.AccessRequired)
                             {
                                lines.Add($"  .{command.Name} {sub.Name} - {sub.Description}");
                             }
                        }
                    }
                }
            }
            return lines;
        }

        private static List<string> BuildPlayerHelpLines(List<GmCommandHandler> commands)
        {
            var lines = new List<string>();
            lines.Add("Available commands:");

            foreach (var command in commands.OrderBy(c => c.Name))
            {
                lines.Add($"{command.Name} - {command.Description}");
                if (command.SubCommands != null && command.SubCommands.Count > 0)
                {
                    foreach (var sub in command.SubCommands.OrderBy(s => s.Name))
                    {
                        lines.Add($"  {command.Name} {sub.Name} - {sub.Description}");
                    }
                }
            }
            return lines;
        }
    }
}
