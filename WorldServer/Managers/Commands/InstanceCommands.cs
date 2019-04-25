using Common;
using System;
using FrameWork;
using System.Collections.Generic;
using WorldServer.World.Battlefronts.Keeps;
using static WorldServer.Managers.Commands.GMUtils;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.Managers.Commands
{
    /// <summary>Commands under .</summary>
    internal class InstanceCommands
    {
           
        public static bool InISpawn(Player plr, ref List<string> values)
        {
            int entry = GetInt(ref values);
            int bossId = GetInt(ref values);
            int spawngroup = GetInt(ref values);
            int realm = GetInt(ref values);

            plr.UpdateWorldPosition();

            Instance_Spawn spawn = new Instance_Spawn();
            // "Spawn a Instance NPC <object id> <BossId> <spawngroup> <realm>"
            spawn.Instance_spawns_ID = Guid.NewGuid().ToString();
            spawn.Entry = (uint)entry;
            spawn.WorldO = (uint)plr._Value.WorldO;
            spawn.WorldY = plr._Value.WorldY;
            spawn.WorldZ = plr._Value.WorldZ;
            spawn.WorldX = plr._Value.WorldX;
            spawn.ZoneID = plr.Zone.ZoneId;
            spawn.SpawnGroupID = (uint)spawngroup;
            spawn.ConnectedbossId = (uint)bossId;
            spawn.Realm = (byte)realm;


            WorldMgr.Database.AddObject(spawn);

            return true;
        }

        public static bool InIinfo(Player plr, ref List<string> values)
        {
            ushort InstanceID = (ushort)GetInt(ref values);

            WorldMgr.InstanceMgr.SendInstanceInfo(plr, InstanceID);

            return true;
        }

        public static bool InIBossSpawn(Player plr, ref List<string> values)
        {
            int entry = GetInt(ref values);
            int bossId = GetInt(ref values);
            int spawngroup = GetInt(ref values);
            int InstanceID = GetInt(ref values);

            plr.UpdateWorldPosition();

            Instance_Boss_Spawn spawn = new Instance_Boss_Spawn();
            // "Spawn a Instance NPC <object id> <BossId> <spawngroup> <realm>"
            spawn.Instance_spawns_ID = Guid.NewGuid().ToString();
            spawn.Entry = (uint)entry;
            spawn.WorldO = (uint)plr._Value.WorldO;
            spawn.WorldY = plr._Value.WorldY;
            spawn.WorldZ = plr._Value.WorldZ;
            spawn.WorldX = plr._Value.WorldX;
            spawn.ZoneID = plr.Zone.ZoneId;
            spawn.SpawnGroupID = (uint)spawngroup;
            spawn.bossId = (uint)bossId;
            spawn.InstanceID = (ushort)InstanceID;

            WorldMgr.Database.AddObject(spawn);

            return true;
        }

        /// <summary>
        /// Converts selected Object to a Instance spawn <objective id>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool InstanceConvert(Player plr, ref List<string> values)
        {
            //<BossId> <spawngroup> <realm>

            int bossId = GetInt(ref values);
            int spawngroup = GetInt(ref values);
            int realm = GetInt(ref values);

            //creatures only atm
            Object obj = GetObjectTarget(plr);
                    if (!obj.IsCreature())
                        return false;
                    obj.Dispose();

                    Creature_spawn spawn = obj.GetCreature().Spawn;
                    WorldMgr.Database.DeleteObject(spawn);

                    Instance_Spawn newSpawn = new Instance_Spawn();
 
                    newSpawn.Instance_spawns_ID = Guid.NewGuid().ToString();
                    newSpawn.Entry = spawn.Entry;
                    newSpawn.WorldO = (ushort)spawn.WorldO;
                    newSpawn.WorldY = spawn.WorldY;
                    newSpawn.WorldZ = spawn.WorldZ;
                    newSpawn.WorldX = spawn.WorldX;
                    newSpawn.ZoneID = (ushort)spawn.ZoneId;
                    newSpawn.Emote = spawn.Emote;
                    newSpawn.Level = spawn.Level;
                    newSpawn.SpawnGroupID = (uint)spawngroup;
                    newSpawn.ConnectedbossId = (uint)bossId;
                    newSpawn.Realm = (byte)realm;


                    WorldMgr.Database.AddObject(newSpawn);

            return true;
        }

        public static bool InstanceBossConvert(Player plr, ref List<string> values)
        {
            //<BossId> <spawngroup> <realm>

            int bossId = GetInt(ref values);
            int spawngroup = GetInt(ref values);
            int instanceid = GetInt(ref values);

            //creatures only atm
            Object obj = GetObjectTarget(plr);
            if (!obj.IsCreature())
                return false;
            obj.Dispose();

            Creature_spawn spawn = obj.GetCreature().Spawn;
            WorldMgr.Database.DeleteObject(spawn);

            Instance_Boss_Spawn newSpawn = new Instance_Boss_Spawn();

            newSpawn.Instance_spawns_ID = Guid.NewGuid().ToString();
            newSpawn.Entry = spawn.Entry;
            newSpawn.WorldO = (ushort)spawn.WorldO;
            newSpawn.WorldY = spawn.WorldY;
            newSpawn.WorldZ = spawn.WorldZ;
            newSpawn.WorldX = spawn.WorldX;
            newSpawn.ZoneID = (ushort)spawn.ZoneId;
            newSpawn.Emote = spawn.Emote;
            newSpawn.Level = spawn.Level;
            newSpawn.SpawnGroupID = (uint)spawngroup;
            newSpawn.bossId = (uint)bossId;
            newSpawn.InstanceID = (byte)instanceid;


            WorldMgr.Database.AddObject(newSpawn);

            return true;
        }

        /// <summary>
        /// Resets the current pq
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool InstanceReset(Player plr, ref List<string> values)
        {
            //TODO
            return true;
            if (plr.QtsInterface.PublicQuest != null)
            {
                plr.QtsInterface.PublicQuest.Stage.Cleanup();
                plr.QtsInterface.PublicQuest.Reset();
            }

            return true;
        }

        /// <summary>
        /// Opens door inside instance (uniqueID, instanceID, open=1/close=0)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool InstanceOpenDoor(Player plr, ref List<string> values)
        {

            int uniqueID = 0;
            int instanceId = 0;
            int open = 1;
            if (values.Count >= 2)
            {
                int.TryParse(values[0], out uniqueID);
                int.TryParse(values[1], out instanceId);

                if (values.Count >= 3)
                    int.TryParse(values[2], out open);

                var Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);

                if (open == 0)
                {
                    Occlusion.SetFixtureVisible((uint)uniqueID, true);
                    Out.WritePacketString(@"|17 55 00 00 08 00 38 49 00 16 09 4B 00 |.<q.U....8I...K.|
                                        |0C B7 6C FF FF 1E 02 00 01 67 2E 00 00 06 00 00 |..l......g......|
                                        |00 00 52 BF 67 55 BB 00 00 00 00 0E 47 61 74 65 |..R.gU......Gate|
                                        |68 6F 75 73 65 20 44 6F 6F 72 04   |............... |");
                }
                else
                {
                    Occlusion.SetFixtureVisible((uint)uniqueID, false);
                    Out.WritePacketString(@"|17 55 00 01 08 00 38 49 00 16 09 4B 00 |.<q.U....8I...K.|
                                        |0C B7 6C FF FF 1E 02 00 01 67 2E 00 00 06 00 00 |..l......g......|
                                        |00 00 52 BF 67 55 BB 00 00 00 00 0E 47 61 74 65 |..R.gU......Gate|
                                        |68 6F 75 73 65 20 44 6F 6F 72 04   |............... |");
                }



                uint result = (uint)
                    (
                        (int)((uniqueID & 0xC000) << 16) |
                        (int)((plr.Zone.ZoneId & 0x3FF) << 20) |
                        (int)((uniqueID & 0x3FFF) << 6) |
                        (int)(0x28 + instanceId)
                    );

                Out.WriteUInt32(result);

                plr.DispatchPacket(Out, true);

                plr.SendClientMessage("DoorID=" + result);

                return true;
            }
            return false;
            //this does not correct, wont work for uniqueId above 16k, 2 high orrder bits neeed to go before zoneId

            //InstanceDoor door = null;

            //var objID = GetInt(ref values);
            //if (objID != 0 && plr.Region.GetObject((ushort)objID) is InstanceDoor)
            //{
            //    door = plr.Region.GetObject((ushort)objID) as InstanceDoor;
            //}
            //else
            //door = plr.GetInRange<InstanceDoor>(100).FirstOrDefault();

            //var t = plr.GetInRange<InstanceObject>(100);

            //if (door != null)
            //{
            //    door.IsOpen = !door.IsOpen;
            //}
            //return true;
        }

        /// <summary>
        /// Returns LOS information about keep door
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool DoorInfo(Player plr, ref List<string> values)
        {
            if (plr.CbtInterface.GetCurrentTarget() is KeepDoor.KeepGameObject)
            {
                var door = (KeepDoor.KeepGameObject)plr.CbtInterface.GetCurrentTarget();

                plr.SendClientMessage("DoorID=" + door.DoorId);
                plr.SendClientMessage("Occlusion_Visible=" + Occlusion.GetFixtureVisible(door.DoorId));

            }
            return true;
        }
    }
}
