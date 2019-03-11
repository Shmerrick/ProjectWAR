using System.Collections.Generic;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 36608, 0)]
    class BroodMotherSzikalax : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            SetInvisible("Invisible Creature");

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;

            SetVisible(c.Name);

            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }

            var prms = new List<object>() { 2000904, 844260, 857649, 28536, Obj.Heading }; //Spider adds
            c.EvtInterface.AddEvent(SpawnAdds, 30 * 1000, 0, prms);
            prms = new List<object>() { 2000904, 843486, 857279, 28447, Obj.Heading }; //Spider adds
            c.EvtInterface.AddEvent(SpawnAdds, 30 * 1000, 0, prms);
            prms = new List<object>() { 2000904, 843982, 856909, 28558, Obj.Heading }; //Spider adds
            c.EvtInterface.AddEvent(SpawnAdds, 30 * 1000, 0, prms);

            return false;
        }

        public override bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            if (!c.IsDead)
                SetInvisible("Invisible Creature");

            c.EvtInterface.RemoveEvent(SpawnAdds);
            c.EvtInterface.RemoveEvent(SpawnAdds);
            c.EvtInterface.RemoveEvent(SpawnAdds);

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            Creature crea = Obj as Creature;

            foreach (Creature c in addList)
                c.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            crea.EvtInterface.RemoveEvent(SpawnAdds);
            crea.EvtInterface.RemoveEvent(SpawnAdds);
            crea.EvtInterface.RemoveEvent(SpawnAdds);
        }
    }

    [GeneralScript(false, "", 36597, 0)]
    class SpiderBroodMotherSzikalax : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            SetInvisible("Invisible Creature");

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;

            SetVisible(c.Name);

            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }
            return false;
        }

        public override bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            if (!c.IsDead)
                SetInvisible("Invisible Creature");

            return false;
        }
    }

    [GeneralScript(false, "", 2000904, 0)]
    class SpiderAddBroodMotherSzikalax : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            SetInvisible("Invisible Creature");

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);

            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;

            SetVisible(c.Name);

            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }
            return false;
        }

        public override bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            if (!c.IsDead)
                SetInvisible("Invisible Creature");

            return false;
        }
    }

}