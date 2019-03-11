using System;
using System.Collections.Generic;
using Common;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.PublicQuests
{
    class BasicPublicQuest : AGeneralScript
    {
        protected Object Obj; // This is creature
        public Random random = new Random();

        protected Point3D spawnPoint;
        protected int spawnWorldX;
        protected int spawnWorldY;
        protected int spawnWorldZ;
        protected int spawnWorldO;
        protected List<Object> stuffInRange = new List<Object>(); // This list keeps all objects in range
        protected List<Creature> addList = new List<Creature>(); // this list keeps all adds spawned by boss
        protected List<Objects.GameObject> goList = new List<Objects.GameObject>(); // this list keeps all adds spawned by boss
        protected int Stage = -1; // This is variable that controls combat Stage

        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;
            spawnWorldX = (int)Obj.WorldPosition.X;
            spawnWorldY = (int)Obj.WorldPosition.Y;
            spawnWorldZ = (int)Obj.WorldPosition.Z;
            spawnWorldO = (int)Obj.Heading;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
        }

        public bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;
            return false;
        }

        public bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            return false;
        }

        public virtual void SpawnAdds(object crea)
        {
            var Params = (List<object>)crea;

            int Entry = (int)Params[0];
            int X = (int)Params[1];
            int Y = (int)Params[2];
            int Z = (int)Params[3];
            ushort O = (ushort)Params[4];

            Creature_proto Proto = CreatureService.GetCreatureProto((uint)Entry);

            Creature_spawn Spawn = new Creature_spawn();
            Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = (int)O;
            Spawn.WorldX = X;
            Spawn.WorldY = Y;
            Spawn.WorldZ = Z;
            Spawn.ZoneId = (ushort)Obj.ZoneId;
            Spawn.Level = 3;
            Creature c = Obj.Region.CreateCreature(Spawn);
            c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds); // We are removing spawns from server when adds die
            addList.Add(c); // Adding adds to the list for easy removal
        }

        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 10000, 1);
            return false;
        }

    }
}
