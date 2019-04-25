using System.Collections.Generic;
using WorldServer.World.Interfaces;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 37964, 0)]
    class HeraldofSolithex : BasicGunbad
    {
        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, CheckFriendInCombat);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);

            SetRandomTargetToNPC(2000893); // Logazor join fight

            return false;
        }

        public bool CheckFriendInCombat(Object Obj, object instigator)
        {
            SetRandomTargetToNPC(2000893); // Checking for Logazor
            return false;
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }
            else if (c.Health < c.TotalHealth * 0.5 && Stage < 1 && !c.IsDead)
            {
                c.Say("Rise, my royal servant! Protect those mains from those who want to steal them from you!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                var prms = new List<object>() { 2000893, 843373, 861016, 25691, Obj.Heading }; // Logazor
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                Stage = 1;
            }

            return false;
        }
    }
}
