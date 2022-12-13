using Common;
using FrameWork;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Scripting;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scripting.Quests
{
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

        // This remove spawned npc from game
        public new bool RemoveAdds(Object npc = null, object instigator = null)
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
                                                      Creature.Heading);

            // Remove the old npc
            Creature.Destroy();

            return;
        }
    }
}