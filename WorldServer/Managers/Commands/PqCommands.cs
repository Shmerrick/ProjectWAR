using Common;
using System;
using System.Collections.Generic;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.Managers.Commands
{
    /// <summary>Public Quest commands under .pq</summary>
    internal class PqCommands
    {

        /// <summary>
        /// Spawn a PQ NPC <object id> <objective id> <type 1 = NPC>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool PqSpawn(Player plr, ref List<string> values)
        {
            int entry = GetInt(ref values);
            int objective = GetInt(ref values);
            int type = GetInt(ref values);

            plr.UpdateWorldPosition();

            PQuest_Spawn spawn = new PQuest_Spawn();
            //Added pquest_spawns_ID here as it is required to correctly save it to DB after this field was 
            //added in PQuest_spawn.cs
            spawn.pquest_spawns_ID = Guid.NewGuid().ToString();
            spawn.Entry = (uint)entry;
            spawn.Objective = (uint)objective;
            spawn.Type = (byte)type;
            spawn.WorldO = plr._Value.WorldO;
            spawn.WorldY = plr._Value.WorldY;
            spawn.WorldZ = plr._Value.WorldZ;
            spawn.WorldX = plr._Value.WorldX;
            spawn.ZoneId = plr.Zone.ZoneId;

            WorldMgr.Database.AddObject(spawn);

            return true;
        }

        /// <summary>
        /// Converts selected Object to a PQ spawn <objective id>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool PqConvert(Player plr, ref List<string> values)
        {
            int objective = GetInt(ref values);
            int type = GetInt(ref values);

            //creatures only atm
            Object obj = GetObjectTarget(plr);

            switch (type)
            {
                case 1:
                    if (!obj.IsCreature())
                        return false;
                    obj.Dispose();

                    Creature_spawn spawn = obj.GetCreature().Spawn;
                    WorldMgr.Database.DeleteObject(spawn);

                    PQuest_Spawn newSpawn = new PQuest_Spawn();
                    //Added pquest_spawns_ID here as it is required to correctly save it to DB after this field was 
                    //added in PQuest_spawn.cs
                    newSpawn.pquest_spawns_ID = Guid.NewGuid().ToString();
                    newSpawn.Entry = spawn.Entry;
                    newSpawn.Objective = (uint)objective;
                    newSpawn.Type = 1; // npc
                    newSpawn.WorldO = spawn.WorldO;
                    newSpawn.WorldY = spawn.WorldY;
                    newSpawn.WorldZ = spawn.WorldZ;
                    newSpawn.WorldX = spawn.WorldX;
                    newSpawn.ZoneId = spawn.ZoneId;
                    newSpawn.Emote = spawn.Emote;
                    newSpawn.Level = spawn.Level;

                    WorldMgr.Database.AddObject(newSpawn);
                    break;
                case 2:
                    if (!obj.IsGameObject())
                        return false;
                    obj.Dispose();

                    GameObject_spawn gospawn = obj.GetGameObject().Spawn;
                    WorldMgr.Database.DeleteObject(gospawn);

                    PQuest_Spawn newSpawngo = new PQuest_Spawn();
                    newSpawngo.pquest_spawns_ID = Guid.NewGuid().ToString();
                    newSpawngo.Entry = gospawn.Entry;
                    newSpawngo.Objective = (uint)objective;
                    newSpawngo.Type = 2; // go
                    newSpawngo.WorldO = gospawn.WorldO;
                    newSpawngo.WorldY = gospawn.WorldY;
                    newSpawngo.WorldZ = gospawn.WorldZ;
                    newSpawngo.WorldX = gospawn.WorldX;
                    newSpawngo.ZoneId = gospawn.ZoneId;
                    newSpawngo.Unks = gospawn.Unks;

                    WorldMgr.Database.AddObject(newSpawngo);
                    break;

                case 3:
                    if (!obj.IsGameObject())
                        return false;

                    GameObject_spawn gointspawn = obj.GetGameObject().Spawn;

                    PQuest_Spawn newSpawnintgo = new PQuest_Spawn();
                    newSpawnintgo.pquest_spawns_ID = Guid.NewGuid().ToString();
                    newSpawnintgo.Entry = ((GameObject)obj).Spawn.Guid;
                    newSpawnintgo.Objective = (uint)objective;
                    newSpawnintgo.Type = 3; // change interaction of a spawned go
                    WorldMgr.Database.AddObject(newSpawnintgo);
                    break;

            }





            return true;
        }

        /// <summary>
        /// Lets go onto the next pq stage
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool PqNextStage(Player plr, ref List<string> values)
        {
            if (plr.QtsInterface.PublicQuest != null)
            {
                plr.QtsInterface.PublicQuest.NextStage();
            }

            return true;
        }

        /// <summary>
        /// Despawns all npc of the current stage stage
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool PqClear(Player plr, ref List<string> values)
        {
            if (plr.QtsInterface.PublicQuest != null)
            {
                plr.QtsInterface.PublicQuest.Stage.Cleanup();
            }

            return true;
        }

        /// <summary>
        /// Resets the current pq
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool PqReset(Player plr, ref List<string> values)
        {
            if (plr.QtsInterface.PublicQuest != null)
            {
                plr.QtsInterface.PublicQuest.Stage.Cleanup();
                plr.QtsInterface.PublicQuest.Reset();
            }

            return true;
        }
    }
}
