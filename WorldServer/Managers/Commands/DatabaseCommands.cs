using Common;
using System.Collections.Generic;
using SystemData;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Database commands under .database</summary>
    internal class DatabaseCommands
    {

        /// <summary>
        /// Reload items information
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ReloadItems(Player plr, ref List<string> values)
        {
            ItemService.LoadItem_Info();
            plr.SendClientMessage("RELOADITEMS: Items Loaded : " + ItemService._Item_Info.Count);
            return true;
        }

        /// <summary>
        /// Reload character <name>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ReloadCharacter(Player plr, ref List<string> values)
        {
            string name = GetString(ref values);
            Character Char = CharMgr.LoadCharacterInfo(name, true);
            if (Char != null)
                plr.SendClientMessage("RELOADCHARACTER: Character Loaded : " + Char.CharacterId);
            else
                plr.SendClientMessage("RELOADCHARACTER: Invalid character specified: " + name);
            return true;
        }

        /// <summary>
        /// Reload creatures in your region
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ReloadCreatures(Player plr, ref List<string> values)
        {
            CreatureService.LoadCreatureItems();
            plr.SendClientMessage("RELOADCREATURES: NPC Items Loaded : " + CreatureService._CreatureItems.Count);

            CreatureService.LoadCreatureProto();
            plr.SendClientMessage("RELOADCREATURES: NPCs Loaded : " + CreatureService.CreatureProtos.Count);

            CreatureService.LoadCreatureStats();
            plr.SendClientMessage("RELOADCREATURES: Stats Loaded : " + CreatureService._CreatureStats.Count);

            BattleFrontService.LoadKeepCreatures();
            plr.SendClientMessage("RELOADCREATURES: Keep Creatures Loaded : " + BattleFrontService._KeepCreatures.Count);

            CreatureService.LoadBossSpawns();
            plr.SendClientMessage("RELOADCREATURES: Bosses Loaded : " + CreatureService.BossSpawns.Count);

            List<Object> allCells = new List<Object>();
            allCells.AddRange(plr._Cell.Objects);
            foreach (Object obj in allCells)
            {
                if (obj.IsCreature())
                {
                    Creature crea = obj.GetCreature();
                    try
                    {
                        var proto = CreatureService.CreatureProtos[crea.Entry];
                        crea.Spawn.Proto = proto;
                    }
                    catch
                    {
                        plr.SendClientMessage("RELOADCREATURES: NPC with Entry " + crea.Entry + " not found in CreatureProtos, removing NPC");
                        crea.Spawn.Proto = null;
                    }
                    crea.Region.CreateCreature(crea.Spawn);
                    crea.Dispose();
                }
            }
            plr.SendClientMessage("RELOADCREATURES: NPC spawn's Loaded : " + CreatureService.CreatureSpawns.Count);
            return true;
        }

        /// <summary>
        /// Reload game objects in your region
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ReloadGameObjects(Player plr, ref List<string> values)
        {
            GameObjectService.LoadGameObjectProtos();
            plr.SendClientMessage("RELOADGAMEOBJECTS: Game objects loaded : " + GameObjectService.GameObjectProtos.Count);

            List<Object> allCells = new List<Object>();
            allCells.AddRange(plr._Cell.Objects);
            foreach (Object obj in allCells)
            {
                GameObject gameObject = obj as GameObject;

                if (gameObject != null)
                {
                    try
                    {
                        GameObject_proto proto = GameObjectService.GameObjectProtos[gameObject.Entry];
                        gameObject.Spawn.Proto = proto;
                    }
                    catch
                    {
                        plr.SendClientMessage("RELOADGAMEOBJECTS: GameObject with Entry " + gameObject.Entry + " not found in GameObjects, removing GameObject");
                        gameObject.Spawn.Proto = null;
                    }
                    gameObject.Region.CreateGameObject(gameObject.Spawn);
                    gameObject.Dispose();
                }
            }
            plr.SendClientMessage("RELOADGAMEOBJECTS: GameObject spawns loaded : " + GameObjectService.GameObjectSpawns.Count);
            return true;
        }

        /// <summary>
        /// Reload abilities.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ReloadAbilities(Player plr, ref List<string> values)
        {
            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                    player.SendClientMessage("[System] Recaching ability tables...", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            }

            AbilityMgr.ReloadAbilities();
            AbilityMgr.LoadCreatureAbilities();
            AbilityModifierInvoker.LoadModifierCommands();
            BuffEffectInvoker.LoadBuffCommands();

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                    player.SendClientMessage("[System] Ability tables successfully recached.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            }

            return true;
        }
        public static bool ReloadPetModifiers(Player plr, ref List<string> values)
        {
            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                    player.SendClientMessage("[System] Recaching pet tables...", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            }
            CharMgr.ReloadPetModifiers();

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    player.SendClientMessage("[System] Pet tables successfully recached.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    if (player != null && player.CrrInterface != null)
                    {
                        if (player.CrrInterface.GetTargetOfInterest() != null && player.CrrInterface.GetTargetOfInterest() is Pet)
                        {
                            Pet myPet = (Pet)player.CrrInterface.GetTargetOfInterest();
                            if (myPet != null)
                            {
                                myPet.Dismiss(null, null);
                                player.SendClientMessage("[System] Pet tables were recached. Your pet has been dismissed.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
