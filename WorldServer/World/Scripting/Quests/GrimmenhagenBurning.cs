using System;
using Common;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.Quests
{
    /* Quest: Grimmenhagen Burning
     * (needs verifing)
     * The quests requires you to kill 3 Tourchbearers and free 3 villagers from their burning
     * house by opening a door.
     * 
     * Whats working:
     * Because we cannot open doors. This will simply spawn a villager when a Tourchbearer dies.
     * You then click on the Villager and he will run away, thanking you for freeing him. However
     * you should 'free' him once you open the door to his house.
     * 
     * Todo:
     * - Support with doors
     */
    [GeneralScript(false, "", 135, 0)]
    public class GrimmenhagenBurningVillagerQuestScript : AGeneralScript
    {
        public override void OnInteract(Object Obj, Player Target, InteractMenu Menu)
        {
            // Make sure the player has the quest and hasn't already finished the objectives.
            if (!Target.GetPlayer().QtsInterface.HasQuest(30001) || Target.GetPlayer().QtsInterface.HasFinishQuest(30001))
                return;

            Target.GetPlayer().QtsInterface.HandleEvent(Objective_Type.QUEST_UNKNOWN, 1314, 1, true);

            Creature_proto Proto = CreatureService.GetCreatureProto((uint)135);
            if (Proto == null)
                return;

            Obj.UpdateWorldPosition();

            Creature_spawn Spawn = new Creature_spawn();
            Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = Obj.Heading;
            Spawn.WorldY = Obj.WorldPosition.Y;
            Spawn.WorldZ = Obj.WorldPosition.Z;
            Spawn.WorldX = Obj.WorldPosition.X;
            Spawn.ZoneId = Obj.Zone.ZoneId;
            Spawn.Faction = 72;
            Spawn.Emote = 53;

            Creature creature = Obj.Region.CreateCreature(Spawn);            
            Random rand = new Random();

			//  Make the villager say something
            Double chance = rand.NextDouble();
            if (chance >= 0.8)
                creature.Say("Thanks for saving me!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (chance >= 0.6)
                creature.Say("Phew, that was close!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (chance >= 0.4)
                creature.Say("Ahh, run for your life!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (chance >= 0.2)
                creature.Say("I could have taken him myself!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else
                creature.Say("Please help my friend!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);


			//  Make the villager run to either the start or the village
            chance = rand.NextDouble();
            if (chance >= 1)
                creature.GetCreature().MvtInterface.Move(22347, 53688, 7425);
            else
                creature.GetCreature().MvtInterface.Move(15548, 51104, 7228);

            creature.EvtInterface.AddEvent(creature.Destroy, 20000, 1);

        }
    }    
}
