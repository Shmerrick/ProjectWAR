using System.Collections.Generic;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 38234, 0)]
    class GarrolathThePoxbearer : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            // Terror
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            Creature creature = Obj as Creature;

            foreach (Creature c in addList)
                c.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            Obj.EvtInterface.RemoveEvent(SpawnNurglings);
            Obj.EvtInterface.RemoveEvent(SpawnAdds);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }

            c.EvtInterface.AddEvent(SpawnNurglings, 20 * 1000, 0);

            //prms = new List<object>() { 2000890, 863682, 855249, 26237, Obj.Heading }; // Spikestabbin'Squigs
            //c.EvtInterface.AddEvent(SpawnAdds, 30 * 1000, 0, prms);

            return false;
        }

        public void SpawnNurglings()
        {
            var prms = new List<object>() { 2000890, 863573, 855069, 26216, Obj.Heading }; // Nurgling

            switch (random.Next(1, 5))
            {
                case 1:
                    prms = new List<object>() { 2000890, 863573, 855069, 26216, Obj.Heading }; // Nurgling
                    Obj.EvtInterface.AddEvent(SpawnAdds, 20 * 1000, 1, prms);
                    break;
                case 2:
                    prms = new List<object>() { 2000890, 8664337, 856140, 26128, Obj.Heading }; // Nurgling
                    Obj.EvtInterface.AddEvent(SpawnAdds, 20 * 1000, 1, prms);
                    break;
                case 3:
                    prms = new List<object>() { 2000890, 863241, 854360, 26170, Obj.Heading }; // Nurgling
                    Obj.EvtInterface.AddEvent(SpawnAdds, 20 * 1000, 1, prms);
                    break;
                case 4:
                    prms = new List<object>() { 2000890, 865081, 855663, 26252, Obj.Heading }; // Nurgling
                    Obj.EvtInterface.AddEvent(SpawnAdds, 20 * 1000, 1, prms);
                    break;
            }
        }

        public override bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Obj.EvtInterface.RemoveEvent(SpawnNurglings);
            Obj.EvtInterface.RemoveEvent(SpawnAdds);

            Creature c = Obj as Creature;

            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            return false;
        }

        public override void OnRemoveObject(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            Obj.EvtInterface.RemoveEvent(SpawnNurglings);
            Obj.EvtInterface.RemoveEvent(SpawnAdds);

            foreach (Creature c in addList)
                c.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();
        }

        public override void OnRemoveFromWorld(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            Obj.EvtInterface.RemoveEvent(SpawnNurglings);
            Obj.EvtInterface.RemoveEvent(SpawnAdds);

            foreach (Creature c in addList)
                c.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();
        }
    }

    [GeneralScript(false, "", 2000890, 0)]
    class NurglingGarrolath : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;

            var prms = new List<object>() { (uint)38234 }; // Here is mommy...
            Obj.EvtInterface.AddEvent(GoToMommy, 200, 1, prms);
        }

    }
}
