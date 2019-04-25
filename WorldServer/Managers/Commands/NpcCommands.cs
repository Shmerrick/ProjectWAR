using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Services.World;
using WorldServer.World.AI;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Positions;
using static WorldServer.Managers.Commands.GMUtils;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.Managers.Commands
{
    /// <summary>NPC commands under .npc</summary>
    internal class NpcCommands
    {

        /// <summary>
        /// Spawn an npc
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcSpawn(Player plr, ref List<string> values)
        {
            int entry = GetInt(ref values);

            Creature_proto proto = CreatureService.GetCreatureProto((uint)entry);
            if (proto == null)
            {
                proto = WorldMgr.Database.SelectObject<Creature_proto>("Entry=" + entry);

                if (proto != null)
                    plr.SendClientMessage("NPC SPAWN: Npc Entry is valid but npc stats are empty. No sniff data about this npc");
                else
                    plr.SendClientMessage("NPC SPAWN:  Invalid npc entry(" + entry + ")");

                return false;
            }

            plr.UpdateWorldPosition();

            Creature_spawn spawn = new Creature_spawn();
            spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
            spawn.BuildFromProto(proto);
            spawn.WorldO = plr._Value.WorldO;
            spawn.WorldY = plr._Value.WorldY;
            spawn.WorldZ = plr._Value.WorldZ;
            spawn.WorldX = plr._Value.WorldX;
            spawn.ZoneId = plr.Zone.ZoneId;
            spawn.Enabled = 1;

            WorldMgr.Database.AddObject(spawn);

            var c = plr.Region.CreateCreature(spawn);
            c.AiInterface.SetBrain(new PassiveBrain(c));


            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "SPAWN CREATURE " + spawn.Entry + " " + spawn.Guid + " AT " + spawn.ZoneId + " " + plr._Value.WorldX + " " + plr._Value.WorldY;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        

        /// <summary>
        /// Move a keep npc 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool MoveKeepSpawn(Player plr, ref List<string> values)
        {
            Object obj = GetObjectTarget(plr);
            if (!obj.IsCreature())
                return false;

            Creature creature = (Creature)obj;

            var selectedEntry = creature.Entry;

            var oldX = creature.WorldPosition.X;
            var oldY = creature.WorldPosition.Y;
            var oldZ = creature.WorldPosition.Z;
            var oldO = creature.Heading;


            var keep = plr.Region.Campaign.GetClosestKeep(plr.WorldPosition, (ushort)plr.ZoneId);

            plr.SendClientMessage(keep.Info.Name);

            KeepNpcCreature keepNpcCreature = null;

            if (keep.Realm == Realms.REALMS_REALM_DESTRUCTION)
            {
                keepNpcCreature = keep.Creatures.SingleOrDefault(x => x.Info.DestroId == selectedEntry && x.Info.X == oldX);
            }
            if (keep.Realm == Realms.REALMS_REALM_ORDER)
            {
                keepNpcCreature = keep.Creatures.SingleOrDefault(x => x.Info.OrderId == selectedEntry && x.Info.X == oldX);
            }

            if (keepNpcCreature == null)
            {
                plr.SendClientMessage($"Could not locate selected target");
                return true;
            }

            keepNpcCreature.Info.X = plr.WorldPosition.X;
            keepNpcCreature.Info.Y = plr.WorldPosition.Y;
            keepNpcCreature.Info.Z = plr.WorldPosition.Z;
            keepNpcCreature.Info.O = plr.Oid;
            keepNpcCreature.Info.WaypointGUID = Convert.ToInt32(values[0]);
            var sql = $"UPDATE war_world.keep_creatures " +
                      $"SET X={keepNpcCreature.Info.X}, Y={keepNpcCreature.Info.Y}, Z = {keepNpcCreature.Info.Z}, O={keepNpcCreature.Info.O}, " +
                      $"WaypointGUID = {keepNpcCreature.Info.WaypointGUID} " +
                      $"where KeepId = {keep.Info.KeepId} and X={oldX} and Y={oldY} and Z={oldZ}";
            WorldMgr.Database.ExecuteNonQuery(sql);

            keepNpcCreature.Creature.X = plr.WorldPosition.X;
            keepNpcCreature.Creature.Y = plr.WorldPosition.Y;

            plr.SendClientMessage($"Moved keep creature to new position");

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "MOVE KEEP CREATURE " + selectedEntry + " " + " AT " + keepNpcCreature.Info.ZoneId + " " + plr._Value.WorldX + " " + plr._Value.WorldY;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);
            return true;
        }

        /// <summary>
        /// Spawn an npc +Keep
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcKeepSpawn(Player plr, ref List<string> values)
        {
            var destroId = values[0];
            var orderId = values[1];
            
            Creature_proto proto = null;
            Creature_proto protoOrder = null;

            if (destroId == "0")
                plr.SendClientMessage("NPC SPAWN:  Setting Destro Id -> 0");
            else
            {
                proto = CreatureService.GetCreatureProto(Convert.ToUInt32(destroId));
                if (proto == null)
                {
                    proto = WorldMgr.Database.SelectObject<Creature_proto>("Entry=" + destroId);

                    if (proto != null)
                        plr.SendClientMessage("NPC SPAWN: Npc Entry is valid but npc stats are empty. No sniff data about this npc");
                    else
                        plr.SendClientMessage("NPC SPAWN:  Invalid npc entry(" + destroId + ")");

                    return false;
                }
            }
            if (orderId == "0")
                plr.SendClientMessage("NPC SPAWN:  Setting Order Id -> 0");
            else
            {
                protoOrder = CreatureService.GetCreatureProto(Convert.ToUInt32(orderId));
                if (protoOrder == null)
                {
                    protoOrder = WorldMgr.Database.SelectObject<Creature_proto>("Entry=" + orderId);

                    if (protoOrder != null)
                        plr.SendClientMessage("NPC SPAWN: Npc Entry is valid but npc stats are empty. No sniff data about this npc");
                    else
                        plr.SendClientMessage("NPC SPAWN:  Invalid npc entry(" + orderId + ")");

                    return false;
                }
            }

           plr.UpdateWorldPosition();

            //Creature_spawn spawn = new Creature_spawn { Guid = (uint)CreatureService.GenerateCreatureSpawnGUID() };
            //spawn.BuildFromProto(proto);
            //spawn.WorldO = plr._Value.WorldO;
            //spawn.WorldY = plr._Value.WorldY;
            //spawn.WorldZ = plr._Value.WorldZ;
            //spawn.WorldX = plr._Value.WorldX;
            //spawn.ZoneId = plr.Zone.ZoneId;
            //spawn.Enabled = 1;

            //WorldMgr.Database.AddObject(spawn);

            var kc = new Keep_Creature();

            kc.ZoneId = plr.Zone.ZoneId;
            kc.DestroId = Convert.ToUInt32(destroId);
            kc.OrderId = Convert.ToUInt32(orderId);
            

            kc.IsPatrol = false;
                var keep = plr.Region.Campaign.GetClosestKeep(plr.WorldPosition, (ushort) plr.ZoneId);
                kc.KeepId = keep.Info.KeepId;

            kc.KeepLord = false;
            kc.X = plr._Value.WorldX;
            kc.Y = plr._Value.WorldY;
            kc.Z = plr._Value.WorldZ;
            kc.O = plr._Value.WorldO;
            kc.WaypointGUID = 0;

            kc.Dirty = true;

            WorldMgr.Database.AddObject(kc);
            WorldMgr.Database.ForceSave();
            plr.SendClientMessage("Created keep creature");

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "SPAWN KEEP CREATURE "+ kc.ZoneId + " " + plr._Value.WorldX + " " + plr._Value.WorldY;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Delete the target <(0=World,1=Database)>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcRemove(Player plr, ref List<string> values)
        {
            Object obj = GetObjectTarget(plr);
            if (!obj.IsCreature())
                return false;

            int database = GetInt(ref values);

            obj.Dispose();

            if (database > 0)
            {
                Creature_spawn spawn = obj.GetCreature().Spawn;
                WorldMgr.Database.DeleteObject(spawn);

                GMCommandLog log = new GMCommandLog();
                log.PlayerName = plr.Name;
                log.AccountId = (uint)plr.Client._Account.AccountId;
                log.Command = "REMOVE CREATURE " + spawn.Entry + " " + spawn.Guid + " AT " + spawn.ZoneId + " " + spawn.WorldX + " " + spawn.WorldY;
                log.Date = DateTime.Now;
                CharMgr.Database.AddObject(log);
            }

            plr.SendClientMessage("NPC REMOVE: Removed " + obj.GetCreature().Spawn.Guid);

            return true;
        }

        /// <summary>
        /// Npc Go To Target <X,Y,Z>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcGoTo(Player plr, ref List<string> values)
        {
            Point3D dest = new Point3D(GetInt(ref values), GetInt(ref values), GetInt(ref values));

            if (dest.X == 0)
                dest.X = plr.WorldPosition.X;

            if (dest.Y == 0)
                dest.Y = plr.WorldPosition.Y;

            if (dest.Z == 0)
                dest.Z = plr.WorldPosition.Z;

            Unit T = plr.CbtInterface.GetCurrentTarget();

            T?.MvtInterface.Move(dest);

            return true;
        }

        /// <summary>
        /// Move target to my position
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcCome(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || !target.IsCreature())
                return false;

            target.GetCreature().MvtInterface.Move(plr.WorldPosition);
            return true;

        }

        /// <summary>
        /// Modify a column value <columnname,value,0 target- 1 all>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcModify(Player plr, ref List<string> values)
        {
            string column = GetString(ref values);
            string value = GetString(ref values);
            int target = GetInt(ref values);

            plr.SendClientMessage("NPC MODIFY: Command not ready");
            return true;
        }

        /// <summary>
        /// Adds speech to the targeted NPC, by spawn (string text)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcQuote(Player plr, ref List<string> values)
        {
            Creature npc = plr.CbtInterface.GetCurrentTarget() as Creature;

            if (npc == null || npc.Spawn.Entry == 0 || npc is Pet || npc is PQuestCreature || npc is KeepCreature)
            {
                plr.SendClientMessage("NPC QUOTE: This command may only be used on a static creature.");
                return true;
            }

            CreatureService.AddCreatureText(npc.Spawn.Entry, GetTotalString(ref values));
            return true;
        }

        /// <summary>
        /// Sets armor piece color <slotIndex (0=all), pri_tint, sec_tint (from tintpalette_equipment.csv)>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcTint(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || !target.IsCreature())
                return false;

            int slotIndex = GetInt(ref values);
            int pri = GetInt(ref values);
            int sec = GetInt(ref values);
            var creature = target.GetCreature();

            foreach (var item in creature.ItmInterface.Items.Where(e => e != null && e.SlotId != 0))
            {
                if (slotIndex != 0 && item.SlotId != slotIndex)
                    continue;

                item._PrimaryColor = (ushort)pri;
                item._SecondaryColor = (ushort)sec;
                item.CreatureItem.PrimaryColor = (ushort)pri;
                item.CreatureItem.SecondaryColor = (ushort)sec;
                WorldMgr.Database.SaveObject(item.CreatureItem);
            }
            WorldMgr.Database.ForceSave();
            creature.ItmInterface.SendEquipped(null);

            return true;
        }

        /// <summary>
        /// Sets monster's animation script <animID> (animID from anim_scripts.csv. 0 to remove).
        /// </summary>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcAnimScript(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null)// || !target.IsCreature())
                return false;
            int animID = GetInt(ref values);

            var Out = new PacketOut((byte)Opcodes.F_ANIMATION);

            Out.WriteUInt16(target.Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)animID);

            plr.DispatchPacket(Out, true);

            return true;
        }

        public static bool NpcPermaAnimScript(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null)
                return false;
            int animID = GetInt(ref values);

            var Out = new PacketOut((byte)Opcodes.F_ANIMATION);

            Out.WriteUInt16(target.Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)animID);

            plr.DispatchPacket(Out, true);

            Creature creature = target.GetCreature();
            creature.Spawn.Emote = (byte)animID;

            if (creature.PQSpawnId == null)
                WorldMgr.Database.SaveObject(creature.Spawn);
            else
            {
                PQuest_Spawn pQSpawn = WorldMgr.Database.SelectObject<PQuest_Spawn>("pquest_spawns_ID='" + creature.PQSpawnId + "'");
                pQSpawn.Emote = (byte)animID;
                WorldMgr.Database.SaveObject(pQSpawn);
            }
            return true;
        }

        /// <summary>
        /// This method allow setting level for selected NPC.
        /// Works for normal NPC and PQ NPCs :)
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// </summary>
        public static bool NpcLevel(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null)
                return false;
            int nPCLevel = GetInt(ref values);

            Creature creature = target.GetCreature();
            creature.Spawn.Level = (byte)nPCLevel;

            if (creature.PQSpawnId == null)
                WorldMgr.Database.SaveObject(creature.Spawn);
            else
            {
                PQuest_Spawn pQSpawn = WorldMgr.Database.SelectObject<PQuest_Spawn>("pquest_spawns_ID='" + creature.PQSpawnId + "'");
                pQSpawn.Level = (byte)nPCLevel;
                WorldMgr.Database.SaveObject(pQSpawn);
            }

            creature.RezUnit();
            return true;
        }

        /// <summary>
        /// This method allow setting current health of NPC or GO
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// </summary>
        public static bool NpcHealth(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null)
                return false;
            int healthPercent = GetInt(ref values);

            Creature creature = target.GetCreature();
            GameObject go = target.GetGameObject();
            if (creature != null)
            {
                creature.Health = Convert.ToUInt32(creature.TotalHealth * healthPercent / 100);
            }
            if (go != null)
            {
                go.Health = Convert.ToUInt32(go.TotalHealth * healthPercent / 100);
            }

            return true;
        }

        /// <summary>
        /// This method allow disabling NPC spawn for selected NPC.
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// </summary>
        public static bool NpcDisable(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null)
                return false;
            int enable = 0;

            Creature creature = target.GetCreature();

            if (creature != null)
            {
                creature.Spawn.Dirty = true;
                creature.Spawn.Enabled = (byte)enable;
                WorldMgr.Database.SaveObject(creature.Spawn);
                creature.Dispose();
            }

            return true;
        }

        /// <summary>
        /// This method allow comversion of normal NPC to Event NPC.
        /// Not working yet.
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// </summary>
        public static bool NpcEventConvert(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null)
                return false;

            Creature creature = target.GetCreature();

            return false;

            /*if (creature != null)
            {
                creature.Spawn.Enabled = (byte)enable;
                WorldMgr.Database.SaveObject(creature.Spawn);
                creature.Dispose();
            }

            return true;*/
        }

        public static bool NpcChangeSpawnPlace(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null && !target.IsCreature())
                return false;

            target.GetCreature().MvtInterface.Move(plr.WorldPosition);

            Creature creature = target.GetCreature();

            if (creature != null)
            {
                plr.UpdateWorldPosition();

                if (creature.PQSpawnId == null)
                {
                    creature.Spawn.Dirty = true;
                    creature.Spawn.WorldX = plr._Value.WorldX;
                    creature.Spawn.WorldY = plr._Value.WorldY;
                    creature.Spawn.WorldZ = plr._Value.WorldZ;
                    creature.Spawn.WorldO = plr._Value.WorldO;

                    WorldMgr.Database.SaveObject(creature.Spawn);
                }
                else
                {
                    PQuest_Spawn pQSpawn = WorldMgr.Database.SelectObject<PQuest_Spawn>("pquest_spawns_ID='" + creature.PQSpawnId + "'");

                    pQSpawn.Dirty = true;
                    pQSpawn.WorldX = plr._Value.WorldX;
                    pQSpawn.WorldY = plr._Value.WorldY;
                    pQSpawn.WorldZ = plr._Value.WorldZ;
                    pQSpawn.WorldO = plr._Value.WorldO;

                    WorldMgr.Database.SaveObject(pQSpawn);
                }
            }

            return false;
        }

    }
}
