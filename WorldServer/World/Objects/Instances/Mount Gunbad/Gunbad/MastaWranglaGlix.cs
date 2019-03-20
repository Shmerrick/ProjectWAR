using System.Collections.Generic;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 36615, 0)]
    class MastaWranglaGlix : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, CheckFriendInCombat);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.MvtInterface.SetBaseSpeed(400); // We want him to be fast :)
            c.IsInvulnerable = false;
            Stage = -1;

            int rand = random.Next(0, 11);
            rand = 1;

            List<Creature> aa = addList;

            Obj.EvtInterface.AddEvent(SpawnTrolls, 100, 1);
            //Obj.EvtInterface.AddEvent(MatureTrolls, 20 * 1000, 0);
            Obj.EvtInterface.AddEvent(SpawnTrolls, 35 * 1000 , 0);

            return false;
        }

        public void SpawnTrolls()
        {
            Creature c = Obj as Creature;
            if (!c.IsDead)
            {
                var prms = new List<object>() { 2000881, 841050, 854487, 26185, Obj.Heading }; // Young Troll
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
                prms = new List<object>() { 2000881, 840814, 854223, 26181, Obj.Heading }; // Young Troll
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
            }
            else
            { 
                //c.EvtInterface.RemoveEvent(MatureTrolls);
                c.EvtInterface.RemoveEvent(SpawnTrolls);
            }
        }

        public void SpawnAdultTrolls()
        {
            Creature c = Obj as Creature;
            if (!c.IsDead)
            {
                var prms = new List<object>() { 2000882, 841050, 854487, 26185, Obj.Heading }; // Adult Troll
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
                prms = new List<object>() { 2000882, 840814, 854223, 26181, Obj.Heading }; // Adult Troll
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
            }
            else
            {
                //c.EvtInterface.RemoveEvent(MatureTrolls);
                c.EvtInterface.RemoveEvent(SpawnTrolls);
            }
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }
            else if (c.Health < c.TotalHealth * 0.25 && Stage < 4 && !c.IsDead)
            {
                c.EvtInterface.RemoveEvent(SpawnTrolls);
                c.EvtInterface.RemoveEvent(SpawnTrolls);
                c.EvtInterface.RemoveEvent(SayStuff);

                SayStuff();

                c.EvtInterface.AddEvent(SpawnAdultTrolls, 100, 1);

                c.EvtInterface.AddEvent(SpawnAdultTrolls, 20 * 1000, 0);
                c.EvtInterface.AddEvent(SpawnAdultTrolls, 25 * 1000, 0);

                Stage = 4;
            }
            else if (c.Health < c.TotalHealth * 0.4 && Stage < 3 && !c.IsDead)
            {
                c.EvtInterface.RemoveEvent(SpawnTrolls);
                c.EvtInterface.RemoveEvent(SpawnTrolls);
                c.EvtInterface.RemoveEvent(SayStuff);

                SayStuff();

                c.EvtInterface.AddEvent(SpawnTrolls, 100, 1);
                c.EvtInterface.AddEvent(SpawnTrolls, 100, 1);

                c.EvtInterface.AddEvent(SpawnTrolls, 20 * 1000, 0);
                c.EvtInterface.AddEvent(SpawnTrolls, 20 * 1000, 0);

                Stage = 3;
            }
            else if (c.Health < c.TotalHealth * 0.6 && Stage < 2 && !c.IsDead)
            {

                c.EvtInterface.RemoveEvent(SpawnTrolls);
                c.EvtInterface.RemoveEvent(SpawnTrolls);
                c.EvtInterface.RemoveEvent(SayStuff);

                SayStuff();

                c.EvtInterface.AddEvent(SpawnTrolls, 100, 1);
                c.EvtInterface.AddEvent(SpawnTrolls, 100, 1);

                c.EvtInterface.AddEvent(SpawnTrolls, 25 * 1000, 0);
                c.EvtInterface.AddEvent(SpawnTrolls, 25 * 1000, 0);

                Stage = 2;
            }
            else if (c.Health < c.TotalHealth * 0.8 && Stage < 1 && !c.IsDead)
            {
                c.EvtInterface.RemoveEvent(SpawnTrolls);
                c.EvtInterface.RemoveEvent(SpawnTrolls);

                SayStuff();

                c.EvtInterface.AddEvent(SpawnTrolls, 100, 1);
                c.EvtInterface.AddEvent(SpawnTrolls, 100, 1);

                c.EvtInterface.AddEvent(SpawnTrolls, 30 * 1000, 0);
                c.EvtInterface.AddEvent(SpawnTrolls, 30 * 1000, 0);
                c.EvtInterface.AddEvent(SayStuff, 30 * 1000, 0);

                Stage = 1;
            }

            return false;
        }

        public void SayStuff()
        {
            Creature c = Obj as Creature;

            if (c != null && !c.IsDead)
            {
                switch (random.Next(1, 4))
                {
                    case 1:
                        c.Say("Git 'ere! Take 'em! Eat 'em!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        break;
                    case 2:
                        c.Say("Dose gits ain't nuffink! Get 'em, trolls!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        break;
                    case 3:
                        c.Say("“Now youse jus' makin' me mad!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        break;
                }
            }
        }

        public new bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            //c.EvtInterface.RemoveEvent(MatureTrolls);
            c.EvtInterface.RemoveEvent(SpawnTrolls);
            c.EvtInterface.RemoveEvent(SpawnTrolls);

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();
            foreach (Object obj in c.ObjectsInRange)
            {
                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 2000881)
                    creature.Destroy();

                if (creature != null && creature.Entry == 2000882)
                    creature.Destroy();
            }

            addList = new List<Creature>();
            goList = new List<GameObject>();

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Creature c = Obj as Creature;

            c.Say("Youse... kilt... me... but da Mixa make short work of youse! Unnghhh!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            Stage = -1;
            stuffInRange.Clear();

            //Obj.EvtInterface.RemoveEvent(MatureTrolls);
            c.EvtInterface.RemoveEvent(SpawnTrolls);
            c.EvtInterface.RemoveEvent(SpawnTrolls);

            c.EvtInterface.RemoveEvent(SpawnAdultTrolls);
            c.EvtInterface.RemoveEvent(SpawnAdultTrolls); 

            foreach (Creature crea in addList)
                crea.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();
        }

        public bool CheckFriendInCombat(Object Obj, object instigator)
        {
            SetRandomTargetToNPC(2000881);
            SetRandomTargetToNPC(2000882);
            return false;
        }
    }

    [GeneralScript(false, "", 2000881, 0)]
    class GlixYoungTroll : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200 + random.Next(100, 201), 1);
        }

        public void MatureTroll()
        {
            Creature c = Obj as Creature;
            if (c != null && !c.IsDead)
            {
                var prms = new List<object>() { 2000882, c.WorldPosition.X, c.WorldPosition.Y, c.WorldPosition.Z, c.Heading }; // Adult Troll
                c.EvtInterface.AddEvent(SpawnAdds, 1000, 1, prms);
                c.PlayEffect(2185);
                c.EvtInterface.AddEvent(c.Destroy, 1500, 1);
            }
        }
    }

    [GeneralScript(false, "", 2000882, 0)]
    class GlixAdultTroll : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200 + random.Next(100,201), 1);
        }
    }
}
