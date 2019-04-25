using System.Linq;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 2000893, 0)]
    class Logazor : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, CheckFriendInCombat);

            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);

            Obj.EvtInterface.AddEvent(CheckDespawn, 30 * 1000, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            c.Say("It may have been these creatures that awoken us, but it is all those who live who shall be made to serve the Mourkain! We shall arise once more!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            return false;
        }

        public bool CheckFriendInCombat(Object Obj, object instigator)
        {
            SetRandomTargetToNPC(37964); // Checking for Herald
            return false;
        }

        public void CheckDespawn()
        {
            bool despawn = true;
            foreach (Object o in Obj.ObjectsInRange.ToList())
            {
                Creature c = o as Creature;
                if (c != null && c.Entry == 37964 && !c.IsDead)
                {
                    despawn = false;
                    break;
                }
            }

            if (despawn)
            {
                Obj.EvtInterface.RemoveEvent(CheckDespawn);
                Obj.EvtInterface.AddEvent(Obj.Destroy, 100, 1);
            }
        }
    }
}
