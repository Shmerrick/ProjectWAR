using FrameWork;
using System.Collections.Generic;
using static WorldServer.Managers.Commands.BaseCommands;
using static WorldServer.Managers.Commands.RespawnCommands;
using static WorldServer.Managers.Commands.SearchCommands;
using static WorldServer.Managers.Commands.NpcCommands;
using static WorldServer.Managers.Commands.StatesCommand;
using static WorldServer.Managers.Commands.EquipCommands;
using static WorldServer.Managers.Commands.WaypointCommands;
using static WorldServer.Managers.Commands.GoCommands;
using static WorldServer.Managers.Commands.ChapterCommands;
using static WorldServer.Managers.Commands.ModifyCommands;
using static WorldServer.Managers.Commands.AddCommands;
using static WorldServer.Managers.Commands.AbilityCommands;
using static WorldServer.Managers.Commands.CampaignCommands;
using static WorldServer.Managers.Commands.CheckCommands;
using static WorldServer.Managers.Commands.RespecCommands;
using static WorldServer.Managers.Commands.ScenarioCommands;
using static WorldServer.Managers.Commands.TeleportCommands;
using static WorldServer.Managers.Commands.TicketCommands;
using static WorldServer.Managers.Commands.MountCommands;
using static WorldServer.Managers.Commands.DatabaseCommands;
using static WorldServer.Managers.Commands.PqCommands;
using static WorldServer.Managers.Commands.InstanceCommands;
using static WorldServer.Managers.Commands.EventCommands;
using static WorldServer.Managers.Commands.SettingCommands;

namespace WorldServer.Managers.Commands
{
    /// <summary>
    /// Contains all commands declarations tree.
    /// </summary>
    /// <remarks>
    /// Should be replaced with annotations.
    /// </remarks>
    public class CommandDeclarations
    {
        public CommandDeclarations()
        {
        }

        /// <summary>Ability commands under .ability</summary>
        public static List<GmCommandHandler> AbilityCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("exmode", ExperimentalMode, null, 0, 0, "Enables experimental mode on the current target if the current class supports it."),
            new GmCommandHandler("changelist", CareerChangeList, null, 0, 0, "Displays a list of changes made to the career."),

            new GmCommandHandler("addstat", AddStatBonus, null, EGmLevel.DatabaseDev, 2, "Increases a given stat by a given value."),
            new GmCommandHandler("buff", SendBuffAppearance, null, EGmLevel.DatabaseDev, 1, "Sends a fake buff start packet (int buffId)"),
            new GmCommandHandler("effect", SendCastPlayerEffect, null, EGmLevel.DatabaseDev, 2, "Sends a cast player effect packet."),
            new GmCommandHandler("start", SendCastPlayerStart, null, EGmLevel.DatabaseDev, 2, "Sends a buff effect start packet."),
            new GmCommandHandler("end", SendCastPlayerEnd, null, EGmLevel.DatabaseDev, 2, "Send a buff effect end packet."),
            new GmCommandHandler("send", SendTestAbility, null, EGmLevel.DatabaseDev, 1, "Send an ability to the client."),
            new GmCommandHandler("cast", InvokeAbility, null, EGmLevel.DatabaseDev, 1, "If possible, casts the ability of the specified ID."),
            new GmCommandHandler("buffcast", InvokeBuff, null, EGmLevel.DatabaseDev, 2, "Invokes the buff of the specified ID."),
            new GmCommandHandler("zerostats", SendZeroStats, null, EGmLevel.DatabaseDev, 0, "Sends zero stats to the client for debug purposes."),
            new GmCommandHandler("list", GetAbilityList, null, EGmLevel.GM, 0, "Gets the complete ability list of target (creature).")

        };



        /// <summary>Addition commands under .add</summary>
        public static List<GmCommandHandler> AddCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("item", AddItem, null, EGmLevel.AnyGM, 1, "Add item to player"),
            new GmCommandHandler("money", AddMoney, null, EGmLevel.DatabaseDev, 1, "Add money to player"),
            new GmCommandHandler("tok", AddTok, null, EGmLevel.GM, 1, "Add tok to player"),
            new GmCommandHandler("renown", AddRenown, null, EGmLevel.TrustedStaff, 1, "Add renown to player"),
            new GmCommandHandler("influence", AddInf, null, EGmLevel.TrustedStaff, 1, "Add Influence to player"),
            new GmCommandHandler("xp", AddXp, null, EGmLevel.TrustedStaff, 1, "Add xp to player"),
            new GmCommandHandler("eligibility", AddRewardEligibility, null, EGmLevel.TrustedStaff, 0, "Reports on players eligibility"),
            new GmCommandHandler("zonelockbags", AddZoneLockBags, null, EGmLevel.TrustedStaff, 0, "Adds Zone Lock Bags"),
        };



        /// <summary>RvR campaign commmands under .campaign</summary>
        public static List<GmCommandHandler> CampaignCommands = CommandsBuilder.BuildCommands(typeof(CampaignCommands));

        /// <summary>World settings command under .setting</summary>
        public static List<GmCommandHandler> SettingCommands = CommandsBuilder.BuildCommands(typeof(SettingCommands));

        /// <summary>Chapter modification commands under .chapter</summary>
        public static List<GmCommandHandler> ChapterCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("save", ChapterSave, null, EGmLevel.DatabaseDev, 1, "Save chapter position"),
            new GmCommandHandler("explore", ChapterTokExplore, null, EGmLevel.DatabaseDev, 2, "Set tok explore"),
            new GmCommandHandler("tokentry", ChapterTok, null, EGmLevel.DatabaseDev, 2, "Set tok entry")
        };

        /// <summary>Debugging commands under .check</summary>
        public static List<GmCommandHandler> CheckCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("groups", CheckGroups, null, EGmLevel.SourceDev, 0, "Check how many groups exist on the server."),
            new GmCommandHandler("objects", CheckObjects, null, EGmLevel.SourceDev, 0, "Check how many objects exist in the current region."),
            new GmCommandHandler("players", CheckPlayersInRange, null, EGmLevel.GM, 0, "Finds all players currently in range."),
            new GmCommandHandler("respawn", FindClosestRespawn, null, EGmLevel.DatabaseDev, 0, "Find the closest respawn point for the specified realm."),
            new GmCommandHandler("logpackets", LogPackets, null, EGmLevel.SourceDev, 0, "Toggles logging outgoing packet volume."),
            new GmCommandHandler("readpackets", ReadPackets, null, EGmLevel.SourceDev, 0, "Displays the volume of outgoing packets over the defined period."),
            new GmCommandHandler("los", StartStopLosMonitor, null, EGmLevel.GM, 0, "Starts/Stops line of sight monitoring for selected target."),
            new GmCommandHandler("population", GetServerPopulation, null, EGmLevel.GM, 0, "Finds all players in the game."),
            new GmCommandHandler("eligibility", GetRewardEligibility, null, EGmLevel.GM, 0, "Reports on players eligibility"),
            new GmCommandHandler("contribution", GetPlayerContribution, null, EGmLevel.GM, 0, "Gets the contribution of the player."),
            new GmCommandHandler("bagbonus", GetBagBonus, null, EGmLevel.GM, 0, "Gets the bagbonus of the player."),
            new GmCommandHandler("bounty", GetPlayerBounty, null, EGmLevel.GM, 0, "Gets the bounty of the player."),
            new GmCommandHandler("impacts", GetPlayerImpactMatrix, null, EGmLevel.GM, 0, "Gets the bounty of the player."),
            new GmCommandHandler("allcontribution", GetBattleFrontContribution, null, EGmLevel.GM, 0, "Gets the contribution of all players in the battlefront."),
            new GmCommandHandler("keeps", CheckKeeps, null, EGmLevel.GM, 0, "Checks all keeps that they have the minimum required child table records."),
            new GmCommandHandler("captain", CheckCaptain, null, EGmLevel.GM, 0, "Returns captain for either realm in the current region"),
            new GmCommandHandler("oil", CheckOil, null, EGmLevel.GM, 0, "Returns information about the current keep/oil")
            
        };

        /// <summary>Database commands under .database</summary>
        public static List<GmCommandHandler> DatabaseCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("itemsreload", ReloadItems, null, EGmLevel.DatabaseDev, 0, "Reload items information"),
            new GmCommandHandler("characterreload", ReloadCharacter, null, EGmLevel.DatabaseDev, 0, "Reload character <name>"),
            new GmCommandHandler("creaturesreload", ReloadCreatures, null, EGmLevel.DatabaseDev, 0, "Reload creatures in your region"),
            new GmCommandHandler("gameobjectsreload", ReloadGameObjects, null, EGmLevel.DatabaseDev, 0, "Reload game objects in your region"),
            new GmCommandHandler("reloadabilities", ReloadAbilities, null, EGmLevel.DatabaseDev, 0, "Reload abilities."),
            new GmCommandHandler("reloadpetmodifiers", ReloadPetModifiers, null, EGmLevel.DatabaseDev, 0, "Reload pet modifiers")

        };

        /// <summary>Creature equipment modification commands under .equip</summary>
        public static List<GmCommandHandler> EquipCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("add", EquipAdd, null, EGmLevel.DatabaseDev, 3, "Add Equipement to target <Model,Slot,Save>"),
            new GmCommandHandler("remove", EquipRemove, null, EGmLevel.DatabaseDev, 2, "Remove Equipement to target <Slot,Save>"),
            new GmCommandHandler("clear", EquipClear, null, EGmLevel.DatabaseDev, 1, "Remove All Equipements to target <Save>"),
            new GmCommandHandler("list", EquipList, null, EGmLevel.GM, 0, "Draw Equipement list of target")

        };

        /// <summary>Game object commands under .go</summary>
        public static List<GmCommandHandler> GoCommands = CommandsBuilder.BuildCommands(typeof(GoCommands));

        /// <summary>PVE Instance commands</summary>
        public static List<GmCommandHandler> InstanceCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("doorinfo", DoorInfo, null, EGmLevel.AllStaff, 0, "Returns LOS information about keep door"),
            new GmCommandHandler("info", InIinfo, null, EGmLevel.AllStaff, 0, "instance infos optional: <instanceid>"),
            new GmCommandHandler("spawn", InISpawn, null, EGmLevel.DatabaseDev, 4, "Spawn a Instance NPC <object id> <BossId> <spawngroup> <realm>"),
            new GmCommandHandler("convert", InstanceConvert, null, EGmLevel.DatabaseDev, 3, "Converts selected Object to a Instance spawn <BossId> <spawngroup> <realm>"),
            new GmCommandHandler("bossspawn", InIBossSpawn, null, EGmLevel.DatabaseDev, 4, "Spawn a Instance Boss NPC <object id> <BossId> <spawngroup> <instanceid>"),
            new GmCommandHandler("bossconvert", InstanceBossConvert, null, EGmLevel.DatabaseDev, 3,
                "Converts selected Object to a Instance Boss spawn <BossId> <spawngroup> <instanceid>"),
            new GmCommandHandler("reset", InstanceReset, null, EGmLevel.DatabaseDev, 0, "Resets the current pq"),
            new GmCommandHandler("opendoor", InstanceOpenDoor, null, EGmLevel.DatabaseDev, 0, "Opens door inside instance (uniqueID, instanceID, open=1/close=0)"),

        };

        /// <summary>Unit modification commands under .modify</summary>
        public static List<GmCommandHandler> ModifyCommands = new List<GmCommandHandler>
        {

            new GmCommandHandler("speed", ModifySpeed, null, EGmLevel.AnyGM, 1, "Changes the speed of the targeted player (int Speed, 0-1000)"),
            new GmCommandHandler("playername", ModifyPlayerName, null, EGmLevel.AnyGM, 2, "Changes players name"),
            new GmCommandHandler("playernametemp", ModifyPlayerNameTemp, null, EGmLevel.EmpoweredStaff, 0, "Temporarily changes players name until server restart."),
            new GmCommandHandler("guildleader", ModifyGuildLeader, null, EGmLevel.EmpoweredStaff, 2, "Changes the leader of the guild (string newLeader, string guildName)"),
            new GmCommandHandler("guildnamebyid", ModifyGuildNameByID, null, EGmLevel.TrustedGM, 2, "Changes the name of the guild by ID (int guildID string guildName)"),

            new GmCommandHandler("level", ModifyLevel, null, EGmLevel.Management, 1, "Changes the level of the targeted player (int Rank)"),
            new GmCommandHandler("renown", ModifyRenown, null, EGmLevel.Management, 2, "Changes the renown rank of a player (string playerName, int RenownRank)"),
            new GmCommandHandler("stat", ModifyStat, null, EGmLevel.SourceDev, 2, "Changes your proficiency in your current crafting skill (byte Skill)"),

            new GmCommandHandler("access", ModifyAccess, null, EGmLevel.Management, 1, "Changes the access level of the designated account (string username, int newAccessLevel)"),
            new GmCommandHandler("morale", ModifyMorale, null, EGmLevel.SourceDev, 1, "Changes the morale of the selected player (int Morale)"),
            new GmCommandHandler("faction", ModifyFaction, null, EGmLevel.SourceDev, 1, "Changes the current faction of selected Unit (byte Faction)"),
            new GmCommandHandler("influence", ModifyInf, null, EGmLevel.SourceDev, 1, "Change the Influence Chaptter Value"),
            new GmCommandHandler("resource", ModifyCrrRes, null, EGmLevel.SourceDev, 1, "Modify your career resource value (byte careerResource)"),
            new GmCommandHandler("gatheringskill", ModifyGath, null, EGmLevel.AnyGM, 1, "Changes your proficiency in your current gathering skill (byte Skill)"),
            new GmCommandHandler("craftingskill", ModifyCraf, null, EGmLevel.AnyGM,1, "Changes your proficiency in your current crafting skill (byte Skill)"),
            new GmCommandHandler("keepguild", ModifyKeepGuild, null, EGmLevel.AnyGM, 1, "Claims or removes claim on keep for a guild"),
            new GmCommandHandler("contribution", ModifyContribution, null, EGmLevel.AnyGM, 0, "Sets a players contribution."),
            new GmCommandHandler("honorrank", ModifyHonorRank, null, EGmLevel.AnyGM, 1, "Sets a players honor rank.")
        };

        /// <summary>Mount commands under .mount</summary>
        public static List<GmCommandHandler> MountCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("set", SetMountCommand, null, EGmLevel.TrustedGM, 1, "Changes the mount of the selected unit (int Entry)"),
            new GmCommandHandler("add", AddMountCommand, null, EGmLevel.DatabaseDev, 3, "Adds a new mount to the database (int Entry, int Speed, string Name)"),
            new GmCommandHandler("remove", RemoveMountCommand, null, EGmLevel.TrustedGM, 0, "Removes the mount of the selected unit."),
            new GmCommandHandler("list", ListMountsCommand, null, EGmLevel.GM, 0, "Shows the list of all mounts.")
        };

        /// <summary>NPC commands under .npc</summary>
        public static List<GmCommandHandler> NpcCommands = new List<GmCommandHandler>
        {

            new GmCommandHandler("spawn",NpcSpawn, null, EGmLevel.DatabaseDev, 1, "Spawn an npc"),
            new GmCommandHandler("remove",NpcRemove, null, EGmLevel.DatabaseDev, 1, "Delete the target <(0=World,1=Database)>"),
            new GmCommandHandler("go",NpcGoTo, null, EGmLevel.DatabaseDev, 3, "Npc Go To Target <X,Y,Z>"),
            new GmCommandHandler("come",NpcCome, null, EGmLevel.DatabaseDev, 0, "Move target to my position"),
            new GmCommandHandler("modify",NpcModify, null, EGmLevel.DatabaseDev, 2, "Modify a column value <columnname,value,0 target- 1 all>"),
            new GmCommandHandler("quote",NpcQuote, null, EGmLevel.DatabaseDev, 1, "Adds speech to the targeted NPC, by spawn (string text)"),
            new GmCommandHandler("tint",NpcTint, null, EGmLevel.DatabaseDev, 3, "Sets armor piece color <slotIndex (0=all), pri_tint, sec_tint (from tintpalette_equipment.csv)>"),
            new GmCommandHandler("animscript", NpcAnimScript, null, EGmLevel.DatabaseDev, 1, "Sets monster's animation script <animID> (animID from anim_scripts.csv. 0 to remove)."),
            new GmCommandHandler("animationset", NpcPermaAnimScript, null, EGmLevel.DatabaseDev, 1, "Sets monster's animation script <animID> (animID from anim_scripts.csv. 0 to remove). This is permanent, updates DB."),
            new GmCommandHandler("level",NpcLevel, null, EGmLevel.DatabaseDev, 1, "Sets NPC Level to specified value"),
            new GmCommandHandler("disable",NpcDisable, null, EGmLevel.DatabaseDev, 0, "Disables NPC from spawns. Can be restored using the DB."),
            new GmCommandHandler("move",NpcChangeSpawnPlace, null, EGmLevel.DatabaseDev, 0, "Makes NPC come to player and updates his position in DB."),
            new GmCommandHandler("addtoevent",NpcEventConvert, null, EGmLevel.DatabaseDev, 1, "Adds NPC to event. Currently doesn't work."),
            new GmCommandHandler("health",NpcHealth, null, EGmLevel.DatabaseDev, 1, "Sets NPC or GO health to specified value percent."),
            new GmCommandHandler("keepspawn",NpcKeepSpawn, null, EGmLevel.DatabaseDev, 1, "Spawn an keep npc <destroId> <orderId> <keepId (0 for auto)>"),
            new GmCommandHandler("keepnpcmove",MoveKeepSpawn, null, EGmLevel.DatabaseDev, 1, "Moves an keep npc"),
        };

        /// <summary>Public Quest commands under .pq</summary>11
        public static List<GmCommandHandler> PqCommands = new List<GmCommandHandler>
        {

            new GmCommandHandler("spawn", PqSpawn, null, EGmLevel.DatabaseDev, 3, "Spawn a PQ NPC <object id> <objective id> <type 1 = NPC>"),
            new GmCommandHandler("convert", PqConvert, null, EGmLevel.DatabaseDev, 1, "Converts selected Object to a PQ spawn <objective id>"),
            new GmCommandHandler("next", PqNextStage, null, EGmLevel.AnyGM, 0, "Lets go onto the next pq stage"),
            new GmCommandHandler("clear", PqClear, null, EGmLevel.AnyGM, 0, "Despawns all npc of the current stage stage"),
            new GmCommandHandler("reset", PqReset, null, EGmLevel.AnyGM, 0, "Resets the current pq")

        };

        /// <summary>Respawn modification commands under .respawn</summary>
        public static List<GmCommandHandler> RespawnCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("add", RespawnAdd, null, EGmLevel.DatabaseDev, 0, "Add respawn point to your position <1=Order or 2=Destruction>"),
            new GmCommandHandler("modify", RespawnModify, null, EGmLevel.DatabaseDev, 1, "Modify existing point to you position <ID>"),
            new GmCommandHandler("remove", RespawnRemove, null, EGmLevel.DatabaseDev, 1, "Delete existing point <ID>")
        };

        /// <summary>Respecialization commands under .respec</summary>
        public static List<GmCommandHandler> RespecCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("gathering", RespecGathering, null, EGmLevel.SourceDev, 0, "Respec Gathering Skill"),
            new GmCommandHandler("crafting", RespecCrafting, null, EGmLevel.SourceDev, 0, "Respec Crafting Skill"),
            new GmCommandHandler("mastery", MasteryRespecialize, null, EGmLevel.AllStaff, 0, "Resets your career mastery points.")
        };

        /// <summary>Scenario commands under .scenario</summary>
        public static List<GmCommandHandler> ScenarioCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("balance", CheckBalance, null, EGmLevel.SourceDev, 0, "Checks the current scenario's balance internals."),
            new GmCommandHandler("domination", CheckDomination, null, EGmLevel.SourceDev, 0, "Checks the current scenario's domination internals.")

            // new GmCommandHandler("rotate", ScenarioRotate, null, EGmLevel.TrustedGM, 0, "Rotates the active scenarios.")
        };


        /// <summary>Search commands under .search</summary>
        public static List<GmCommandHandler> SearchCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("item", SearchItem, null, EGmLevel.GM, 1, "Search an item by name <name>"),
            new GmCommandHandler("npc", SearchNpc, null, EGmLevel.GM, 1, "Seach an npc by name <name>"),
            new GmCommandHandler("gameobject", SearchGameObject, null, EGmLevel.GM, 1, "Seach an gameobject by name <name>"),
            new GmCommandHandler("quest", SearchQuest, null, EGmLevel.GM, 1, "Seach an quest by name <name>"),
            new GmCommandHandler("zone", SearchZone, null, EGmLevel.GM, 1, "Seach an zone by name <name>"),
            new GmCommandHandler("inventory", SearchInventory, null, EGmLevel.GM, 0, "Search inventory of selected target. 1st param : <filter>")
        };

        /// <summary>State modification commands under .state</summary>
        public static List<GmCommandHandler> StatesCommand = new List<GmCommandHandler>
        {
            new GmCommandHandler("add", StatesAdd, null, EGmLevel.SourceDev, 1, "Add State To Target <Id>"),
            new GmCommandHandler("remove", StatesRemove, null, EGmLevel.SourceDev, 1, "Remove state from target <Id>"),
            new GmCommandHandler("list", StatesList, null, EGmLevel.GM, 0, "Show target States List")
        };

        /// <summary>Contains the list of teleportation commands under .teleport</summary>
        public static List<GmCommandHandler> TeleportCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("map", TeleportMap, null, EGmLevel.GM, 4,
                "Teleports you to the specified world coordinates in a given zone (byte ZoneID , uint WorldX, uint WorldY, uint WorldZ)"),
            new GmCommandHandler("center", TeleportCenter, null, EGmLevel.GM, 1, "Teleport to the centre of the given map."),
            new GmCommandHandler("appear", TeleportAppear, null, EGmLevel.GM, 1, "Teleports you to a player's location (string playerName)"),
            new GmCommandHandler("summon", TeleportSummon, null, EGmLevel.AnyGM, 1, "Summons a player/group to your location (string playerName optional GROUP)"),
            new GmCommandHandler("set", TeleportSet, null, EGmLevel.AnyGM, 5,
                "Sets offline/online players coordinates in database (player_name byte byte ZoneID , uint WorldX, uint WorldY, uint WorldZ)"),
            new GmCommandHandler("objective", TeleportObjective, null, EGmLevel.GM, 1, "Teleport player to BattlefieldObjective (ObjectiveId)"),

        };

        /// <summary>Ticket management commands under .ticket</summary>
        public static List<GmCommandHandler> TicketCommands = new List<GmCommandHandler>
        {
            new GmCommandHandler("listtickets", ListTickets, null, EGmLevel.DatabaseDev, 0, "Lists the current tickets"),
            new GmCommandHandler("assign", Assign, null, EGmLevel.DatabaseDev, 2, "Assigns a person to a ticket (string accountname, string bugreportID)"),
            new GmCommandHandler("assignme", AssignMe, null, EGmLevel.DatabaseDev, 1, "Assigns a person to a ticket (string bugreportID)"),
            new GmCommandHandler("numberoftickets", NumberOfTickets, null, EGmLevel.DatabaseDev, 0, "Shows how many tickets there currently is"),
            new GmCommandHandler("deleteticket", DeleteTicket, null, EGmLevel.DatabaseDev, 1, "Deletes the ticket (string bugreportID)"),
            new GmCommandHandler("answer", Answer, null, EGmLevel.DatabaseDev, 2, "Answers and closes a ticket (string reportID, string message)")
        };

        /// <summary>Waypoint commands under .warpoint</summary>
        public static List<GmCommandHandler> WaypointCommands = new List<GmCommandHandler>
        {

            new GmCommandHandler("add", NpcAddWaypoint, null, EGmLevel.DatabaseDev, 0, "Adds a waypoint on your current position to your current target."),
            new GmCommandHandler("remove", NpcRemoveWaypoint, null, EGmLevel.DatabaseDev, 1, "Remove a waypoint from the target (int Id)"),
            new GmCommandHandler("move", NpcMoveWaypoint, null, EGmLevel.DatabaseDev, 1, "Moves the specified waypoint of target to your position (int Id)"),
            new GmCommandHandler("list", NpcListWaypoint, null, EGmLevel.DatabaseDev, 0, "Shows the list of waypoints."),
            new GmCommandHandler("info", NpcInfoWaypoint, null, EGmLevel.DatabaseDev, 0, "Shows information about the current waypoint.")
        };

        /// <summary>Root commands list.</summary>
        public static List<GmCommandHandler> BaseCommand = new List<GmCommandHandler>
        {
            #region Command Group Handlers

            new GmCommandHandler("ability", null, AbilityCommands, 0, 0, "Ability commands."),
            new GmCommandHandler("add", null, AddCommands, EGmLevel.GM, 0, "Addition commands."),
            new GmCommandHandler("campaign", null, CampaignCommands, EGmLevel.AnyGM, 0, "RvR campaign commmands."),
            new GmCommandHandler("chapter", null, ChapterCommands, EGmLevel.DatabaseDev, 0, "Chapter modification commands."),
            new GmCommandHandler("check", null, CheckCommands, EGmLevel.GM, 0, "Debugging commands."),
            new GmCommandHandler("database", null, DatabaseCommands, EGmLevel.DatabaseDev, 0, "Database commands."),
            new GmCommandHandler("equip", null, EquipCommands, EGmLevel.GM, 0, "Creature equipment modification commands."),
            new GmCommandHandler("go", null, GoCommands, EGmLevel.DatabaseDev, 0, "Game object commands."),
            new GmCommandHandler("instance", null, InstanceCommands, EGmLevel.DatabaseDev, 0, "PVE Instance commands."),
            new GmCommandHandler("modify", null, ModifyCommands, EGmLevel.DatabaseDev, 0, "Unit modification commands."),
            new GmCommandHandler("mount", null, MountCommands, EGmLevel.GM, 0, "Mount commands."),
            new GmCommandHandler("npc", null, NpcCommands, EGmLevel.DatabaseDev, 0, "NPC commands."),
            new GmCommandHandler("pq", null, PqCommands, EGmLevel.DatabaseDev, 0, "Public Quest commands."),
            new GmCommandHandler("respawn", null, RespawnCommands, EGmLevel.DatabaseDev, 0, "Respawn modification commands."),
            new GmCommandHandler("respec", null, RespecCommands, 0, 0, "Respecialization commands."),
            new GmCommandHandler("scenario", null, ScenarioCommands, EGmLevel.AllStaff, 0, "Scenario commands."),
            new GmCommandHandler("search", null, SearchCommands, EGmLevel.GM, 0, "Search commands."),
            new GmCommandHandler("states", null, StatesCommand, EGmLevel.DatabaseDev, 0, "State modification commands."),
            new GmCommandHandler("teleport", null, TeleportCommands, EGmLevel.GM, 0, "Contains the list of teleportation commands."),
            new GmCommandHandler("ticket", null, TicketCommands, EGmLevel.GM, 0, "Contains the list of ticket commands"),
            new GmCommandHandler("waypoints", null, WaypointCommands, EGmLevel.DatabaseDev, 0, "Waypoint commands."),
            new GmCommandHandler("setting", null, SettingCommands, EGmLevel.SourceDev, 0, "World settings commands."),

            #endregion

            #region Standalone Commands

            // User commands
            new GmCommandHandler("grouprefresh", GroupRefresh, null, 0, 0, "Tries to rediscover all players within the selected players group."),
            new GmCommandHandler("ping", GetPing, null, EGmLevel.DatabaseDev, 0, "Shows player ping."),
            new GmCommandHandler("scenariostatus", ScenarioStatus, null, 0, 0, "Shows player count and score of all running scenarios."),
            new GmCommandHandler("scenarioscore", GetScenarioScore, null, 0, 0, "Returns targets scenario scores."),
            new GmCommandHandler("gmlist", GmMgr.ListGameMasters, null, 0, 0, "Lists available GMs."),
            new GmCommandHandler("rules", SendRules, null, 0, 0, "Sends a condensed list of in-game rules."),
            new GmCommandHandler("assist", Assist, null, 0, 1, "Switches to friendly target's target"),
            new GmCommandHandler("unlock", Unlock, null, 0, 0, "Used to fix stuck-in-combat problems preventing you from joining a scenario."),
            new GmCommandHandler("tellblock", TellBlock, null, 0, 0, "Allows you to block whispers from non-staff players who are outside of your guild."),
            new GmCommandHandler("getstats", GetStats, null, 0, 0, "Shows your own linear stat bonuses."),
            new GmCommandHandler("standard", AssignStandard, null, EGmLevel.GM, 0, "Assigns Standard Bearer Titel to the Player."),
            new GmCommandHandler("ror", RoRFeatures, null, EGmLevel.GM, 0, "Help Files for RoR-specific features."),
            new GmCommandHandler("changename", RequestNameChange, null, EGmLevel.GM, 1, "Requests a name change, one per account per month (string newName)"),
            new GmCommandHandler("ping", GetPing, null, 0, 0, "Returns an avg of 10 latency packets"),
            new GmCommandHandler("sorenable", SoREnable, null, 0, 0, "Enables SoR addon."),
            // new GmCommandHandler("pug", PugScenario, null, 0, 0, "Displays current PUG scenario."),
            new GmCommandHandler("sorenable", SoREnable, null, 0, 0, "Enables SoR addon."),
            //   new GmCommandHandler("version", GetVersion, null, 0, 0, "Gets the WorldServer version."),
            // Halloween event stuff
            new GmCommandHandler("spooky", Spooky, null, EGmLevel.GM, 0, "This command will make you spooky..."),
            new GmCommandHandler("notspooky", NotSpooky, null, EGmLevel.GM, 0,
                "You don't want to be spooky :(... You need to run this command upon logging on server, it do not disable spookieness if you are already spooky."),
            new GmCommandHandler("morph", Morph, null, EGmLevel.GM, 0, "This command will make you morph..."),
            // All staff
#if (DEBUG)
            new GmCommandHandler("debugmode", SetDebugMode, null, EGmLevel.SourceDev, 0, "Enables debugging messages (byte enableDebug)"),
#else
            new GmCommandHandler("debugmode", SetDebugMode, null, EGmLevel.AllStaff, 0, "Enables debugging messages (byte enableDebug)"),
            #endif
            new GmCommandHandler("togglerank", GmMgr.ToggleShowRank, null, EGmLevel.GM, 0, "Toggles whether or not to display your staff rank in chat messages."),
            new GmCommandHandler("name", SetSurname, null, EGmLevel.GM, 1, "Changes your last name (string Surname) - use 'clear' to clear the name"),
            new GmCommandHandler("info", Info, null, EGmLevel.GM, 0, "Prints general information about your current target."),
            new GmCommandHandler("doorinfo", KeepDoorInfo, null, EGmLevel.GM, 0, "Prints door information about your current target."),
            new GmCommandHandler("aiinfo", AIInfo, null, EGmLevel.GM, 0, "Sends information about the targeted creature's AI state."),
            new GmCommandHandler("prevpos", PreviousPosition, null, EGmLevel.AllStaff, 1,
                "Creates an object at the position which a player held a certain time ago (int millisecondDelta)"),
            new GmCommandHandler("latency", OverrideLatency, null, EGmLevel.AllStaff, 1, "Overrides the server's opinion of your latency (int latency)"),
            new GmCommandHandler("getguildlead", GetGuildLead, null, EGmLevel.AllStaff, 1, "Returns the guild leader of the guild specified (string guildname)"),


            // All empowered staff
            new GmCommandHandler("kill", Kill, null, EGmLevel.AnyGM, 0, "Slays the targeted Unit."),
            new GmCommandHandler("wound", Wound, null, EGmLevel.AnyGM, 0, "Wounds the targeted Unit."),
            new GmCommandHandler("nuke", Nuke, null, EGmLevel.EmpoweredStaff, 2,
                "Slays everyone in radius. Takes 2 parameters, realm and radius in ft. Realm 0 - all, 1 - order, 2 - destro"),

            new GmCommandHandler("boot", Reboot, null, EGmLevel.EmpoweredStaff, 0, "Reboots the server."),
            new GmCommandHandler("clearboot", ClearServer, null, EGmLevel.AnyGM, 0, "Removes all players from server."),
            new GmCommandHandler("revive", Revive, null, EGmLevel.AnyGM, 0, "Resurrects the targeted Unit."),
            new GmCommandHandler("fly", SetFlightState, null, EGmLevel.GM, 0, "Grants the ability to fly (byte enableFlight)"),

            new GmCommandHandler("announce", Announce, null, EGmLevel.GM, 1, "Sends a global message (string Message). SoundID can be specified .announce sound <id> <message>"),
            new GmCommandHandler("shroud", Shroud, null, EGmLevel.GM, 0, "Causes you to become invisible to other players."),
            new GmCommandHandler("invincible", InvincibleMe, null, EGmLevel.GM, 0, "Toggles invulnerability on the current target."),
            new GmCommandHandler("invmob", Invincible, null, EGmLevel.EmpoweredStaff, 0, "Toggles invulnerability on the character."),
            new GmCommandHandler("xpmode", XpMode, null, EGmLevel.EmpoweredStaff, 0, "Allows you to cease gaining experience points (byte Enabled)"),
            new GmCommandHandler("gps", Gps, null, EGmLevel.GM, 0, "Prints information about your current target's position."),
            new GmCommandHandler("setnpcmodel", SetNpcModel, null, EGmLevel.EmpoweredStaff, 1, "Temporary overlays npc model on selected target. To clear set modelID to 0."),
            new GmCommandHandler("setvfxstate", SetVfxState, null, EGmLevel.EmpoweredStaff, 2, "Set game objects effect state (ushort objectID, byte vfxID)"),
            new GmCommandHandler("getobjects", GetObjects, null, EGmLevel.EmpoweredStaff, 0, "List OIDs of all game objects in range"),
            new GmCommandHandler("effectstate", SetEffectState, null, EGmLevel.DatabaseDev, 1,
                "Set effect state of target player. To remove set second parameter to zero. (byte effectID). Ex: To set player as champ .set effect state 7"),
            new GmCommandHandler("setcore", SetCoreTester, null, EGmLevel.EmpoweredStaff, 1, "Toggles player as a core tester (string playername)"),
            new GmCommandHandler("packet2", Packet2, null, EGmLevel.EmpoweredStaff, 1, "Custom command to check how one variable works."),

            // GM commands
            new GmCommandHandler("advice", MessageAdvice, null, EGmLevel.GM, 0, "Messages Advice with a generic name."),
            new GmCommandHandler("csr", CSRMessage, null, EGmLevel.AnyGM, 0, "Sends a CSR-type message to your realm."),
            new GmCommandHandler("motd", SetMotd, null, EGmLevel.AnyGM, 1, "Changes the Message of the Day (string message)"),
            new GmCommandHandler("findip", FindIP, null, EGmLevel.GM, 1, "Lists all players with an IP starting with the specified string"),
            new GmCommandHandler("getonlinechar", GetOnlineChar, null, EGmLevel.GM, 1, "Looks for online characters from the given account (string accountName)"),
            new GmCommandHandler("getchar", GetChar, null, EGmLevel.GM, 1, "Lits all characters on the account for given character name (string charname)"),
            new GmCommandHandler("checklog", CheckLog, null, EGmLevel.GM, 1, "Checks the sanction history for a player (string characterName)"),
            new GmCommandHandler("note", Note, null, EGmLevel.GM, 2, "Creates a user note (string characterName, string Reason)"),
            new GmCommandHandler("warn", Warn, null, EGmLevel.GM, 2, "Issues a logged warning (string characterName, string Reason)"),
            new GmCommandHandler("blockadvice", BlockAdvice, null, EGmLevel.GM, 1, "Blocks the player from using advice (string characterName)"),
            new GmCommandHandler("mute", Mute, null, EGmLevel.GM, 1,
                "Mutes the player, or lifts if no duration is specified. The muted player will see their own chat, but no one else will. (string characterName, int duration, string durationString, string Reason)"),
            new GmCommandHandler("eject", Eject, null, EGmLevel.AnyGM, 2, "Closes the client of the player with the specified name (string characterName, string Reason)"),
            new GmCommandHandler("sever", Sever, null, EGmLevel.AnyGM, 1, "Force disconnects the player (string characterName)"),
            new GmCommandHandler("exile", Exile, null, EGmLevel.AnyGM, 4, "Exiles the player (string characterName, int duration, string durationString, string Reason)"),
            new GmCommandHandler("permaban", Ban, null, EGmLevel.AnyGM, 2, "Permanently bans the player (string characterName, string Reason)"),
            new GmCommandHandler("unban", Unban, null, EGmLevel.AnyGM, 2, "Lifts any type of ban on a player (string characterName, string Reason)"),
            new GmCommandHandler("annihilate", Annihilate, null, EGmLevel.TrustedGM, 1, "Wipes the player's account, after asking for confirmation (string accountName)"),
            new GmCommandHandler("getcharslots", GetCharSlots, null, EGmLevel.TrustedGM, 1,
                "Displays all character names and the slots that they occupy on the given account (string accountId)"),
            new GmCommandHandler("deletecharat", DeleteCharInSlot, null, EGmLevel.TrustedGM, 2, "Removes a character from the given account (string accountName, int slotId)"),
            new GmCommandHandler("togglequest", ToggleQuest, null, EGmLevel.AnyGM, 1, ".togglequest <QuestID> <0: Memory | 1: Database>, turns a quest on or off."),
            new GmCommandHandler("getguildid", GetGuildID, null, EGmLevel.TrustedGM, 1, "Get the guildID (string GuildName)"),
            new GmCommandHandler("blockname", BlockName, null, EGmLevel.TrustedGM, 2,
                "Prevents a name from being used by characters (string name, string <Equals|StartsWith|Contains>)"),
            new GmCommandHandler("unblockname", UnblockName, null, EGmLevel.TrustedGM, 1, "Removes a character name filter (string name)"),
            new GmCommandHandler("listblockednames", ListBlockedNames, null, EGmLevel.TrustedGM, 0, "Lists all blocked names"),
            new GmCommandHandler("removequests", RemoveQuests, null, EGmLevel.TrustedGM, 1, "Removes all the quests from a player (string player)"),
            new GmCommandHandler("hide", Hide, null, EGmLevel.AnyGM, 0, "Hides you from the gmlist"),

            // Database dev commands

            new GmCommandHandler("playeffect", PlayEffect, null, EGmLevel.DatabaseDev, 2, "Play effect from data/gamedata/effectlist.csv"),
            new GmCommandHandler("playability", PlayAbility, null, EGmLevel.DatabaseDev, 2, "Plays ability from data/gamedata/effect.csv (string playerName, ushort effectID)"),
            new GmCommandHandler("playsound", PlaySound, null, EGmLevel.DatabaseDev, 1,
                "Play sound from data/gamedata/audio_server.csv. Multiple sounds can be specified .playsound <soundID:delayInSeconds> ... (ex: .playsound 628:10 1010:15 1319:25)"),
            new GmCommandHandler("save", Save, null, EGmLevel.DatabaseDev, 0, "Performs a database save on the target."),
            new GmCommandHandler("objectivestate", ObjectiveState, null, EGmLevel.DatabaseDev, 1, "Set vfx for objective (int oid)."),
            new GmCommandHandler("previewmodel", PreviewItemModel, null, EGmLevel.TrustedGM, 1, "Temporary sets equipped item model (int slotIndex, int modelID)"),
            new GmCommandHandler("recreateplayer", CreatePlayer, null, EGmLevel.DatabaseDev, 1, "Requests player info is resent"),
            new GmCommandHandler("quest", QuestComplete, null, EGmLevel.DatabaseDev, 2,
                "Used to debug quests <QuestId> <Operation> Operation 1 - add, 2 - finish quest, 3 - delete quest from player"),
            new GmCommandHandler("givebag", GiveBag, null, EGmLevel.SourceDev, 0,
                "Used to give a character a bag <Rarity><Item1><Item2>.."), // Don't worry, this is only on DEV, dosen't go to Live
            //new GmCommandHandler("gunbad", Gunbad, null, EGmLevel.GM, 0, "Used to to set character for tester"), // Don't worry, this is only on DEV, dosen't go to Live

            // Source dev commands
            new GmCommandHandler("lockcasting", PreventCasting, null, EGmLevel.SourceDev, 0, "Prevents all casting (byte blockAllCasts)"),
            new GmCommandHandler("mindread", MindRead, null, 0, 0, "Causes the target creature to broadcast its AI."),
            new GmCommandHandler("collatelength", SetCollationLength, null, EGmLevel.SourceDev, 1,
                "Sets the maximum length to which a client's send buffer may be filled before Socket.Send() is invoked. 0 sends packets immediately."),
            new GmCommandHandler("scenariocmd", ScenarioCmd, null, EGmLevel.SourceDev, 0, "Send custom command into scenario handler."),
            new GmCommandHandler("scout", ScoutChamps, null, EGmLevel.SourceDev, 0, "Scouts for champions in the current region."),
            new GmCommandHandler("alltoks", AllTok, null, EGmLevel.SourceDev, 1, "enable all toks"),
            new GmCommandHandler("packetlog", PacketLog, null, EGmLevel.SourceDev, 1, "Turns on packet logging for player's account (string playername)"),
            new GmCommandHandler("tokbestary", AllTokbestary, null, EGmLevel.SourceDev, 1, "enable all toks"),
            new GmCommandHandler("guildlogon", LogGuild, null, EGmLevel.SourceDev, 1, "Log chat of a particular guild."),
            new GmCommandHandler("guildlogoff", CancelLogGuild, null, EGmLevel.SourceDev, 1, "Cease logging chat of a particular guild."),
            new GmCommandHandler("structure", CreateRvRObject, null, EGmLevel.SourceDev, 0, "Creates a structure."),
            new GmCommandHandler("bolsterlevel", BolsterLevel, null, EGmLevel.SourceDev, 1, "Changes max bolster level for T2 T3 and T4."),
            new GmCommandHandler("playerdrop", ChangePlayerDrop, null, EGmLevel.SourceDev, 1, "Switch that changes how medallions are generated for killed players."),

            new GmCommandHandler("beastmaster", Beastmaster, null, EGmLevel.SourceDev, 0, "Changes your whitelion to beastmaster."),
            new GmCommandHandler("setpet", SetPet, null, EGmLevel.SourceDev, 1, "Changes your pet model to number provided."),
            new GmCommandHandler("summonpet", SummonPet, null, EGmLevel.SourceDev, 1, "Summons the pet requested."),
            new GmCommandHandler("spawnmobinstance", SpawnMobInstance, null, EGmLevel.SourceDev, 1, "Spawns one or more mobs (does not save to db)"),
            new GmCommandHandler("sendkeepinfo", SendKeepInfoWrapper, null, EGmLevel.SourceDev, 0, "Sends a KeepInfo message"),
            new GmCommandHandler("sendkeepstatus", SendKeepStatusWrapper, null, EGmLevel.SourceDev, 0, "Sends a KeepInfo message"),
            new GmCommandHandler("sendcampaignstatus", SendCampaignStatusWrapper, null, EGmLevel.SourceDev, 0, "Sends a Campaign Status message"),
            new GmCommandHandler("dynamicvendor", CreateDynamicVendor, null, EGmLevel.SourceDev, 0, "Creates a dynamic vendor"),
            new GmCommandHandler("creategatehouseportal", CreateGateHousePortal, null, EGmLevel.SourceDev, 0, "Creates a portal to Southern Garrison gatehouse"),
            new GmCommandHandler("realmcaptain", MakeRealmCaptain, null, EGmLevel.SourceDev, 0, "Makes the targetted player a realm captain"),
            new GmCommandHandler("spawnboss", SpawnBossInstance, null, EGmLevel.SourceDev, 1, "Spawns a boss <protoid>"),
            new GmCommandHandler("summongoremane", SummonGoremane, null, EGmLevel.SourceDev, 0, "Spawns Goremane"),
            new GmCommandHandler("summonkokrit", SummonKokrit, null, EGmLevel.SourceDev, 0, "Spawns Kokrit"),
            new GmCommandHandler("summonbulbousone", SummonBulbousOne, null, EGmLevel.SourceDev, 0, "Spawns the Bulbous One"),
            new GmCommandHandler("summonazukthul", SummonAzukThul, null, EGmLevel.SourceDev, 0, "Spawns AzukThul"),
            new GmCommandHandler("summonborzhar", SummonBorzhar, null, EGmLevel.SourceDev, 0, "Spawns Borzhar"),
            new GmCommandHandler("summongahlvoth", SummonGahlvoth, null, EGmLevel.SourceDev, 0, "Spawns Gahlvoth"),
            new GmCommandHandler("summonzekaraz", SummonZekaraz, null, EGmLevel.SourceDev, 0, "Spawns Zekaraz"),
            new GmCommandHandler("summonlordslaurith", SummonLordSlaurith, null, EGmLevel.SourceDev, 0, "Spawns LordSlaurith"),
            new GmCommandHandler("summonorcapult", SummonOrcapult, null, EGmLevel.SourceDev, 0, "Its flying time"),
            new GmCommandHandler("creategoldchest", CreateGoldChest, null, EGmLevel.SourceDev, 0, "Summon a GoldChest"),
            new GmCommandHandler("forcelockzone", ForceLockZone, null, EGmLevel.SourceDev, 0, "Force Lock a Fort Zone"),
            new GmCommandHandler("honor", CheckPlayerHonor, null, EGmLevel.Anyone, 0, "Checks a player's honor rank"),
            new GmCommandHandler("mailitem", MailItem, null, EGmLevel.SourceDev, 3, "Mail an item to a character"),
            new GmCommandHandler("testscoreboard", Scoreboard, null, EGmLevel.SourceDev, 0, "test scoreboard"),
            new GmCommandHandler("title", SetTitle, null, EGmLevel.SourceDev, 1, "Set player title"),
            new GmCommandHandler("bulkmail", BulkMailItems, null, EGmLevel.SourceDev, 1, "Mail items from a file to characters"),
            
            


        };

        #endregion
    };
}
