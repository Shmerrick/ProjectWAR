using System;
using Common;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Interfaces;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.Quests
{
    [GeneralScript(false, "", 135, 0)]
    public class GrimmenhagenBurningVillagerQuestScript : AGeneralScript
    {
        public override void OnInteract(Object Obj, Player Target, InteractMenu Menu)
        {
            // Make sure the player has the quest and hasn't already finished the objectives.
            if (!Target.GetPlayer().QtsInterface.HasQuest(30001) || Target.GetPlayer().QtsInterface.HasFinishQuest(30001))
                return;

            Target.GetPlayer().QtsInterface.HandleEvent(Objective_Type.QUEST_UNKNOWN, 1314, 1, true);

            Creature c = Obj.GetCreature();
            Random rand = new Random();

            //  Make the villager say something
            Double chance = rand.NextDouble();
            if (chance >= 0.8)
                c.Say("Thanks for saving me!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (chance >= 0.6)
                c.Say("Phew, that was close!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (chance >= 0.4)
                c.Say("Ahh, run for your life!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (chance >= 0.2)
                c.Say("I could have taken him myself!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else
                c.Say("Please help my friend!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            //  Make the villager run to either the start or the village
            double runChance = RandomHelper.NextDouble();
            if (runChance >= 0.5)
            {
                c.MvtInterface.Move(837236, 935989, 6812);
                c.MvtInterface.Move(834477, 935686, 7002);
            }
            else
            {
                c.MvtInterface.Move(837236, 935989, 6812);
                c.MvtInterface.Move(844175, 939020, 7769);
            }
        }
    }

    [GeneralScript(false, "", 0, 2)]
    public class GrimmenhagenBurningTorchbearerQuestScript : AGeneralScript
    {
        public override void OnInteract(Object Obj, Player Target)
        {
            if (!Target.GetPlayer().QtsInterface.HasQuest(30001) || Target.GetPlayer().QtsInterface.HasFinishQuest(30001))
                return;

            Target.GetPlayer().QtsInterface.HandleEvent(Objective_Type.QUEST_UNKNOWN, 1314, 1, true);

            // Spawn villager
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)135);
            if (Proto == null)
                return;

            Creature_spawn Spawn = new Creature_spawn
            {
                Guid = (uint)CreatureService.GenerateCreatureSpawnGUID()
            };
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = Target.Heading;
            Spawn.WorldY = Target.WorldPosition.Y;
            Spawn.WorldZ = Target.WorldPosition.Z;
            Spawn.WorldX = Target.WorldPosition.X;
            Spawn.ZoneId = Target.Zone.ZoneId;
            Spawn.Faction = 72;
            Spawn.Emote = 53;

            Creature c = Target.Region.CreateCreature(Spawn);

            //  The villager will be disposed in 20 secs
            c.EvtInterface.AddEvent(c.Destroy, 20000, 1);
        }
    }
}