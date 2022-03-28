using System;
using Common;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Scripting;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer
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
    [GeneralScript(false, "", 46, 0)]
    public class FatherSigwaldQuestScript : AGeneralScript
    {
        private Object Creature;
        private int Quote = 0;
        public override void OnObjectLoad(Object Creature)
        {
            this.Creature = Creature;

            Creature.EvtInterface.AddEvent(SpawnBaddies, 9000, 5);
        }

        public void SpawnBaddies()
        {
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)43);

            if (Proto == null)
                return;

            if (Quote == 0)
                Creature.Say("Hold your ground, hold your ground! ", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 1)
                Creature.Say("I see in your eyes the same fear that would take the heart of me.", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 2)
                Creature.Say("A day may come when the courage of men fails, when we forsake our friends and break all bonds of fellowship, but it is not this day.", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 3)
                Creature.Say("An hour of wolves and shattered shields, when the age of men comes crashing down!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 4)
                Creature.Say("But it is not this day! This day we fight!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            Quote++;

            Random rand = new Random();

            for (int i = 0; i < 3; i++)
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Creature.Heading;
                Spawn.WorldX = (int)(Creature.WorldPosition.X + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(Creature.WorldPosition.Y + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = Creature.WorldPosition.Z;
                Spawn.ZoneId = Creature.Zone.ZoneId;
                Creature c = Creature.Region.CreateCreature(Spawn);
                //c.GetCreature().MvtInterface.WalkTo(Creature.WorldPosition.X, Creature.WorldPosition.Y, Creature.WorldPosition.Z, MovementInterface.CREATURE_SPEED);
                c.EvtInterface.AddEvent(c.Destroy, 20000, 1);
            }
            
        }

        public override void OnDie(Object Creature)
        {
            if (!(Creature is PQuestCreature))
                return;

            PQuestCreature PQCreature = Creature as PQuestCreature;

            Creature.Say("You fools!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            base.OnDie(Creature);
        }
    }
}
