using System.Collections.Generic;
using Common;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.Events.BrightWizardCollegeReopen
{
    [GeneralScript(false, "", 1989, 0)]
    public class ThyrusGorman : AGeneralScript
    {
        private Object Obj; // This is creature 1989
        List<Object> stuffInRange = new List<Object>(); // This list keeps all objects in range

        // With this we can do some stuff when creature 1990 spawns
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
        }

        // When something gets in range (I think it is 350 or 400) we want 
        // to add it to correct lists and set some events
        public override void OnEnterRange(Object Obj, Object DistObj)
        {
            if (DistObj.IsPlayer())
            {
                stuffInRange.Add(DistObj);
            }
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (c.Health < c.TotalHealth * 0.1 && !c.IsDead && c.IsCreature() && c.Entry == 1989) // At 10% HP magic happens
            {
                c.Say("Curse you!");

                c.AbtInterface.StartCast(c, 4338, 1);
                c.Health = c.MaxHealth;
                c.CbtInterface.LeaveCombat();
                c.EvtInterface.AddEvent(c.CbtInterface.LeaveCombat, 1000, 2);
                FinishQuestAndTeleportOutside(8003);
            }

            return false;
        }

        public void RemoveBuffs()
        {
            Creature c = this.Obj as Creature;
            c.IsInvulnerable = false;
        }

        public void FinishQuestAndTeleportOutside(ushort Quest)
        {
            foreach (Object inRange in stuffInRange)
            {
                if (inRange != null)
                {
                    Player player = inRange as Player;
                    if (player != null && player.IsPlayer() && player.CbtInterface.IsInCombat)
                    {
                        if (player.QtsInterface.GetQuest(Quest) != null)
                        {
                            Character_quest quest = player.QtsInterface.GetQuest(Quest);

                            foreach (KeyValuePair<ushort, Character_quest> questKp in player.QtsInterface.Quests)
                            {
                                if (questKp.Value == quest)
                                {
                                    foreach (Character_Objectives objective in questKp.Value._Objectives)
                                    {
                                        if (objective.Objective == null)
                                            continue;

                                        objective.Count = 1;
                                        questKp.Value.Dirty = true;
                                        player.QtsInterface.SendQuestUpdate(questKp.Value);
                                        CharMgr.Database.SaveObject(questKp.Value);

                                        if (objective.IsDone())
                                        {
                                            Creature finisher;

                                            foreach (Object obj in player.ObjectsInRange)
                                            {
                                                if (obj.IsCreature())
                                                {
                                                    finisher = obj.GetCreature();
                                                    if (QuestService.HasQuestToFinish(finisher.Entry, questKp.Value.Quest.Entry))
                                                        finisher.SendMeTo(player.GetPlayer());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        var prms = new List<object>() { player };
                        if (!player.IsInvulnerable)
                            player.EvtInterface.AddEvent(KickPlayerOutside, 1500, 1, prms);
                    }
                }
            }
        }

        public void KickPlayerOutside(object plr)
        {
            if (plr != null)
            {
                var Params = (List<object>)plr;

                Player player = Params[0] as Player;

                if (player != null && player.IsPlayer())
                    player.Teleport(161, 438245, 136537, 16865, 1044);
            }


        }
    }
}