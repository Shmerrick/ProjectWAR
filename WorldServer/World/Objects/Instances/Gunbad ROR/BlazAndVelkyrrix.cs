using System.Collections.Generic;
using System.Linq;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 38909, 0)]
    class BlazDaTaminMasta : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, CheckFriendInCombat);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override void OnDie(Object Obj)
        {
            Creature c = Obj as Creature;
            Stage = -1;
            stuffInRange.Clear();

            foreach (Creature crea in addList)
                crea.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            foreach (Object o in c.ObjectsInRange)
            {
                Creature creature = o as Creature;
                if (creature != null && creature.Entry == 38907 && !creature.IsDead)
                {
                    creature.BuffInterface.RemoveBuffByEntry(14897);
                    NewBuff newBuff = creature.BuffInterface.GetBuff(14897, null);
                    if (newBuff != null)
                        newBuff.RemoveBuff(true);

                    break;
                }
            }
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            SetRandomTargetToNPC(38907); // Velkyrrix join fight

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

            if (c != null && !c.IsDead)
            {
                c.BuffInterface.RemoveBuffByEntry(13155);
                NewBuff newBuff = c.BuffInterface.GetBuff(13155, null);
                if (newBuff != null)
                    newBuff.RemoveBuff(true);
            }

            return false;
        }

        public bool CheckFriendInCombat(Object Obj, object instigator)
        {
            SetRandomTargetToNPC(38907); // Checking for Velkyrrix
            return false;
        }
    }

    [GeneralScript(false, "", 38907, 0)]
    class Velkyrrix : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, CheckFriendInCombat);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
            //Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            foreach (Creature crea in addList)
                crea.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            Creature c = Obj as Creature;
            foreach (Object o in c.ObjectsInRange.ToList())
            {
                Creature creature = o as Creature;
                if (creature != null && !creature.IsDead && creature.Entry == 38909)
                {
                    var prms = new List<object>() { creature, (ushort)13155, "No, it cannot be... Yu 'll pay for dis! Die!" }; // Rage
                    c.EvtInterface.AddEvent(DelayedBuff, 100, 1, prms);
                    break;
                }
            }

            Obj.EvtInterface.RemoveEvent(SpawnRandomGO);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            SpawnRandomGO();

            SetRandomTargetToNPC(38907); // Blaz join fight

            var prms = new List<object>() { c, (ushort)14897, "" }; // Iron Body
            c.EvtInterface.AddEvent(DelayedBuff, 100, 1, prms);

            Obj.EvtInterface.AddEvent(SpawnRandomGO, 15 * 1000, 0);

            return false;
        }

        public void SpawnRandomGO()
        {
            var prms = new List<object>() { 2000569, (int)(Obj.WorldPosition.X + 150 - 300 * random.NextDouble()), (int)(Obj.WorldPosition.Y + 150 - 300 * random.NextDouble()), Obj.WorldPosition.Z, (int)Obj.Heading, (uint)0 }; // Spider spawn
            Obj.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);
            prms = new List<object>() { 2000569, (int)(Obj.WorldPosition.X + 150 - 300 * random.NextDouble()), (int)(Obj.WorldPosition.Y + 150 - 300 * random.NextDouble()), Obj.WorldPosition.Z, (int)Obj.Heading, (uint)0 }; // Spider spawn
            Obj.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);
            prms = new List<object>() { 2000569, (int)(Obj.WorldPosition.X + 150 - 300 * random.NextDouble()), (int)(Obj.WorldPosition.Y + 150 - 300 * random.NextDouble()), Obj.WorldPosition.Z, (int)Obj.Heading, (uint)0 }; // Spider spawn
            Obj.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);
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
            foreach (Object obj in c.ObjectsInRange)
            {
                GameObject go = obj as GameObject;
                if (go != null && go.Entry == 2000569)
                    go.Destroy();

                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 38720)
                    creature.Destroy();
            }

            addList = new List<Creature>();
            goList = new List<GameObject>();

            Obj.EvtInterface.RemoveEvent(SpawnRandomGO);

            c.BuffInterface.RemoveBuffByEntry(14897); // Removing Iron Body
            NewBuff newBuff = c.BuffInterface.GetBuff(14897, null);
            if (newBuff != null)
                newBuff.RemoveBuff(true);

            return false;
        }

        public bool CheckFriendInCombat(Object Obj, object instigator)
        {
            SetRandomTargetToNPC(38909); // Checking for Blaz
            return false;
        }
    }

    [GeneralScript(false, "", 38720, 0)]
    class VelkyrrixSpawn : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }

    [GeneralScript(false, "", 0, 2000569)]
    class VelkyrrixEgg : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            GameObject go = Obj as GameObject;
            go.Respawn = 0;
        }

        public override void OnDie(Object Obj)
        {
            this.Obj = Obj;
            Obj.PlayEffect(2185);
            Obj.EvtInterface.AddEvent(Obj.Destroy, 1000, 1);
        }
    }
}
