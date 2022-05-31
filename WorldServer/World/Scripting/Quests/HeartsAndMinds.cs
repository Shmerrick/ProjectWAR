using Common;
using FrameWork;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Scripting;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

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
	 * - fix Model1 and Model2 spawns - male and female.// if Fermer has Model1 then Marauder must be Model1, same with Model2
	 */

    [GeneralScript(false, "", 32, 0)] // 32 - Farmer
    public class HeartsAndMindsSpawnQuestScript : AGeneralScript
    {
        public override void OnInteract(Object Creature, Player Target, InteractMenu Menu)
        {
            // Make sure the player has the quest and hasn't already finished the objectives.
            if (!Target.GetPlayer().QtsInterface.HasQuest(30003) || Target.GetPlayer().QtsInterface.HasFinishQuest(30003))
            {
                return;
            }
            // Spawn Marauder Sympathizer

            Creature_proto MarauderSympathizer = CreatureService.GetCreatureProto(31); // 31 - Marauder
            if (MarauderSympathizer == null)
            {
                return;
            }
            MarauderSympathizer.Model1 = Creature.GetCreature().Spawn.Proto.Model1;

            Creature.UpdateWorldPosition();

            Creature_spawn Spawn = new Creature_spawn(MarauderSympathizer,
                                                      (uint)CreatureService.GenerateCreatureSpawnGUID(),
                                                      Creature.Zone.ZoneId,
                                                      Creature.WorldPosition.X,
                                                      Creature.WorldPosition.Y,
                                                      Creature.WorldPosition.Z,
                                                      Creature.Heading);
            //Spawn.Faction = 129;

            Creature cr = Creature.Region.CreateCreature(Spawn);
            cr.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);

            // Remove the old npc
            Creature.Destroy();

            return;
        }

        // This remove adds from game
        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature creature = npc as Creature;
            creature.EvtInterface.AddEvent(creature.Destroy, 20000, 1);

            return false;
        }
    }

    [GeneralScript(false, "", 31, 0)]
    public class HeartsAndMindsDieQuestScript : AGeneralScript
    {
        public override void OnDie(Object Creature)
        {
            // Respawn the orginal npc
            Creature_proto GrimmenhagenFarmer = CreatureService.GetCreatureProto(32);
            if (GrimmenhagenFarmer == null)
                return;
            GrimmenhagenFarmer.Model1 = Creature.GetCreature().Spawn.Proto.Model1;

            Creature.UpdateWorldPosition();
            Creature_spawn Spawn = new Creature_spawn(GrimmenhagenFarmer,
                                                      (uint)CreatureService.GenerateCreatureSpawnGUID(),
                                                      Creature.Zone.ZoneId,
                                                      Creature.WorldPosition.X,
                                                      Creature.WorldPosition.Y,
                                                      Creature.WorldPosition.Z,
                                                      Creature.Heading)
            {
                Faction = 65
            };

            Creature cr = Creature.Region.CreateCreature(Spawn);

            //  Set the new NPC to dead, there should be a method to do this perhaps.
            cr.Health = 0;

            cr.States.Add(3); // Death State

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH);
            Out.WriteUInt16(cr.Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);
            cr.DispatchPacket(Out, true);

            //  Make it respawn
            cr.EvtInterface.AddEvent(cr.RezUnit, 30000 + cr.Level * 1000, 1); // 30 seconde Rez

            // Remove the old npc
            Creature.Destroy();

            return;
        }

        // This remove adds from game
        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature creature = npc as Creature;
            creature.EvtInterface.AddEvent(creature.Destroy, 20000, 1);

            return false;
        }
    }
}