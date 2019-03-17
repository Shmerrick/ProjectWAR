using System;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Map;

namespace WorldServer.World.Battlefronts.Keeps
{
    public class KeepNpcCreature
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public RegionMgr Region;
        public KeepCreature Creature;
        public Keep_Creature Info;
        public BattleFrontKeep Keep;

        public KeepNpcCreature(RegionMgr region, Keep_Creature info, BattleFrontKeep keep)
        {
            Region = region;
            Info = info;
            Keep = keep;
        }


        public void SpawnGuard(Realms realm)
        {
            if (Creature != null)
            {
                Creature.Destroy();
                Creature = null;

            }

            if (realm != Realms.REALMS_REALM_NEUTRAL)
            {
                if (Info.DestroId == 0 && realm == Realms.REALMS_REALM_DESTRUCTION)
                    _logger.Trace($"Creature Id = 0, no spawning");
                else
                {
                    Creature_proto proto = CreatureService.GetCreatureProto(realm == Realms.REALMS_REALM_ORDER ? Info.OrderId : Info.DestroId);

                    if (proto == null)
                    {
                        Log.Error("KeepNPC", "No FlagGuard Proto");
                        return;
                    }
                    _logger.Trace($"Spawning Guard {proto.Name} ({proto.Entry})");

                    Creature_spawn spawn = new Creature_spawn();
                    spawn.BuildFromProto(proto);
                    spawn.WorldO = Info.O;
                    spawn.WorldX = Info.X;
                    spawn.WorldY = Info.Y;
                    spawn.WorldZ = Info.Z;
                    spawn.ZoneId = Info.ZoneId;

                    Creature = new KeepCreature(spawn, this, Keep);
                    Creature.WaypointGUID = Convert.ToUInt32(Info.WaypointGUID);
                    if (Info.WaypointGUID > 0)
                        Creature.AiInterface.Waypoints =  WaypointService.GetKeepNpcWaypoints(Info.WaypointGUID);


                    Region.AddObject(Creature, spawn.ZoneId);
                }
            }
        }

        //public void SpawnGuardNear(Realms realm, KeepNpcCreature nearPatrol)
        //{
        //    if (Creature != null)
        //    {
        //        Creature.Destroy();
        //        Creature = null;
        //    }

        //    if (realm != Realms.REALMS_REALM_NEUTRAL)
        //    {
        //        Creature_proto proto = CreatureService.GetCreatureProto(realm == Realms.REALMS_REALM_ORDER ? Info.OrderId : Info.DestroId);

        //        _logger.Trace($"Spawning Guard Near {proto.Name} ({proto.Entry})");

        //        if (proto == null)
        //        {
        //            Log.Error("KeepNPC", "No FlagGuard Proto");
        //            return;
        //        }

        //        Creature_spawn spawn = new Creature_spawn();
        //        spawn.BuildFromProto(proto);
        //        spawn.WorldO = nearPatrol.Info.O;
        //        spawn.WorldX = nearPatrol.Creature.WorldPosition.X + (nearPatrol.Info.X - Info.X);
        //        spawn.WorldY = nearPatrol.Creature.WorldPosition.Y + (nearPatrol.Info.Y - Info.Y);
        //        //ushort height = (ushort)ClientFileMgr.GetHeight(nearPatrol.Info.ZoneId, spawn.WorldX, spawn.WorldY);
        //        //spawn.WorldZ = Info.IsPatrol ? ((height <= 0) ? Info.Z : height) : Info.Z;
        //        spawn.WorldZ = nearPatrol.Creature.WorldPosition.Z;
        //        spawn.ZoneId = nearPatrol.Info.ZoneId;

        //        Creature = new KeepCreature(spawn, this, Keep)
        //        {
        //            WaypointGUID = Convert.ToUInt32(Info.WaypointGUID),
        //            NearAiInterface = nearPatrol.Creature.AiInterface
        //        };

        //        Region.AddObject(Creature, spawn.ZoneId);
        //    }
        //}

        public void DespawnGuard()
        {
            if (Creature != null)
            {
                Creature.Destroy();
                Creature = null;
            }
        }
    }
}