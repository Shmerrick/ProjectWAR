using Common;
using WorldServer.NetWork.Handler;
using WorldServer.World.AI;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.Quests
{
    [GeneralScript(false, "", 0, 2)]
    public class GrimmenhagenBurningDoor : AGeneralScript
    {
        private Creature Villager;
        private Creature Marauder;
        private Objects.GameObject pObject;

        private enum GrimmenhagenNPC
        {
            Villager = 135,  // Grimmenhagen Villager
            Torchbearer = 98324 // Marauder Torchbearer
        }

        private enum GrimmenhagenObjectives
        {
            MarauderKilled = 1313,  // Marauder Torchbearers Killed
            HouseSearched = 1314    // House searched door
        }

        public override void OnInteract(Object Obj, Player Target, InteractMenu Menu)
        {
            pObject = Obj.GetGameObject();
            switch (pObject.Spawn.Guid)
            {
                // GrimmenhagenBurningDoor
                case 245902:
                case 245903:
                case 245904:
                case 245905:
                case 245906:
                case 245907:
                case 245908:
                case 245909:
                case 2459012:
                case 2459013:

                    // update quest status
                    Target.GetPlayer().QtsInterface.HandleEvent(Objective_Type.QUEST_UNKNOWN, (uint)GrimmenhagenObjectives.HouseSearched, 1, true);

                    foreach (Object pObj in pObject.ObjectsInRange)
                    {
                        if (!pObj.IsCreature())
                            continue;

                        Creature pCreature = pObj.ToCreature();
                        if (pObject.GetDistanceToObject(pCreature) <= 20.0f)
                        {
                            if (pCreature.Entry == (uint)GrimmenhagenNPC.Villager)
                                Villager = pCreature;
                            else if (pCreature.Entry == (uint)GrimmenhagenNPC.Torchbearer)
                                Marauder = pCreature;
                        }
                    }

                    // Make the nearest Marauder attack the player
                    if (Marauder != null)
                    {
                        Marauder.AiInterface.SetBrain(new AggressiveBrain(Marauder));
                        Marauder.CbtInterface.SetTarget(Target.Oid, GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY);
                        Marauder.AiInterface.ProcessCombatStart(Target);
                    }

                    if (Villager != null)
                    {
                        Villager.SendAnimation(0);
                        Villager.UpdateWorldPosition(); // useful ?
                        Villager.MvtInterface.Move(pObject.WorldPosition);

                        //  Make the villager say something
                        double chance = RandomHelper.NextDouble();
                        if (chance >= 0.8)
                            Villager.Say("Thanks for saving me!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        else if (chance >= 0.6)
                            Villager.Say("Phew, that was close!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        else if (chance >= 0.4)
                            Villager.Say("Ahh, run for your life!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        else if (chance >= 0.2)
                            Villager.Say("I could have taken him myself!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        else
                            Villager.Say("Please help my friend!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                        //  Make the villager run to either the start or the village
                        Villager.EvtInterface.AddEvent(VillagerRunAway, 4000, 1);
                        Villager.EvtInterface.AddEvent(VillagerRespawn, 56000, 1);
                    }
                    break;
            }
        }

        private void VillagerRespawn()
        {
            Villager.MvtInterface.Teleport(Villager.WorldSpawnPoint);
            Villager.SendAnimation(53);
        }

        private void VillagerRunAway()
        {
            double chance = RandomHelper.NextDouble();
            if (chance >= 0.5)
                Villager.MvtInterface.Move(834477, 935686, 7002);
            else
                Villager.MvtInterface.Move(844175, 939020, 7769);
        }
    }
}