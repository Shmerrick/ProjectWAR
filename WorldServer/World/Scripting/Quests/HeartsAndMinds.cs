using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;
using FrameWork;
using WorldServer.Services.World;

namespace WorldServer
{
    /* Quest: Hearts And Minds
     * (needs verifing)
     * The quests requires you to talk to some farmers to get them to take up arms,
     * they turn agressive once you talk to them and once ou do enough damage to them
     * they are meant to turn back to allies and they'd be 'persuaded' to take up arms.
     * 
     * Whats working:
     * Currently once you talk to the farmer he will despawn and a new agressive mob will
     * be spawned. Allowing you to kill him and do the objective. Then the old mob will spawn
     * dead where he was. This allows the farmer to respawn in about 30 seconds so a player can't
     * spam the same mob.
     * 
     * Todo:
     * - Some text would be nice.
     * - make it more like the real quest.
     */

    [GeneralScript(false, "",  32, 0)]
    public class HeartsAndMindsSpawnQuestScript : BasicQuest
    {

        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;
        }

        public override void OnInteract(Object Obj, Player Target, InteractMenu Menu)
        {
			// Make sure the player has the quest and hasn't already finished the objectives.
            if (!Target.GetPlayer().QtsInterface.HasQuest(30003) || Target.GetPlayer().QtsInterface.HasFinishQuest(30003))
               return;

			// Spawn the bad npc 
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)31);
            if (Proto == null)
                return;

            Obj.UpdateWorldPosition();

            Creature_spawn Spawn = new Creature_spawn();
            Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
            Proto.Model1 = Obj.GetCreature().Spawn.Proto.Model1;
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = Obj.Heading;
            Spawn.WorldY = Obj.WorldPosition.Y;
            Spawn.WorldZ = Obj.WorldPosition.Z;
            Spawn.WorldX = Obj.WorldPosition.X;
            Spawn.ZoneId = Obj.Zone.ZoneId;
            //Spawn.Faction = 129;

            Creature c =  Obj.Region.CreateCreature(Spawn);
            c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);

            // Remove the old npc
            Obj.Destroy();

            return;
        }

        // This remove adds from game
        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 20000, 1);

            return false;
        }
    }

    [GeneralScript(false, "",  31,  0)]
    public class HeartsAndMindsDieQuestScript : BasicQuest
    {
        int X, Y, Z, O;
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            X = Obj.WorldPosition.X;
            Y = Obj.WorldPosition.Y;
            Z = Obj.WorldPosition.Z;
            O = Obj.Heading;
        }

        public override void OnDie(Object Obj)
        {
			// Respawn the orginal npc 
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)32);
            if (Proto == null)
                return;

            Obj.UpdateWorldPosition();

            Creature_spawn Spawn = new Creature_spawn();
            Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
            Proto.Model1 = Obj.GetCreature().Spawn.Proto.Model1;
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = O;
            Spawn.WorldY = Y;
            Spawn.WorldZ = Z;
            Spawn.WorldX = X;
            Spawn.ZoneId = Obj.Zone.ZoneId;
            //Spawn.Faction = 65;
			
            Creature c = Obj.Region.CreateCreature(Spawn);

			//  Set the new NPC to dead, there should be a method to do this perhaps.
            c.Health = 0;

            c.States.Add(3); // Death State

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH);
            Out.WriteUInt16(c.Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);
            c.DispatchPacket(Out, true);

            //  Make it respawn
            //var prms = new List<object>() { c };

            //c.EvtInterface.AddEvent(TeleportToSpawnPlace, 29000 + c.Level * 1000, 1, prms);
            c.EvtInterface.AddEvent(c.RezUnit, 30000 + c.Level * 1000, 1); // 30 seconde Rez

			// Remove the old npc
            Obj.Destroy();

            return;
        }

        public void TeleportToSpawnPlace(object obj)
        {
            var Params = (List<object>)obj;

            Creature c = Params[0] as Creature;

            c.MvtInterface.Teleport(spawnPoint);
        }

        // This remove adds from game
        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 20000, 1);

            return false;
        }

    }
}
