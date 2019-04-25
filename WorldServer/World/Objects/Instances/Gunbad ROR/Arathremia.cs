using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 41620, 0)]
    class Arathremia : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }
            else if (c.Health < c.TotalHealth * 0.05 && Stage < 8 && !c.IsDead)
            {
                c.Say("Traitors!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                KillTraitors();

                Stage = 8;
            }
            else if (c.Health < c.TotalHealth * 0.1 && Stage < 7 && !c.IsDead)
            {
                TraitorousSouls();

                Stage = 7;
            }
            else if (c.Health < c.TotalHealth * 0.2 && Stage < 6 && !c.IsDead)
            {
                c.Say("I'd like you to meet some friends of mine!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                var prms = new List<object>() { 41619, 853039, 856219, 20257, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853204, 856279, 20247, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853329, 856246, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853558, 856098, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853518, 855937, 20254, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853390, 853827, 20263, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                Stage = 6;
            }
            else if (c.Health < c.TotalHealth * 0.25 && Stage < 5 && !c.IsDead)
            {
                c.Say("Traitors!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                KillTraitors();

                Stage = 5;
            }
            else if (c.Health < c.TotalHealth * 0.3 && Stage < 4 && !c.IsDead)
            {
                TraitorousSouls();

                Stage = 4;
            }
            else if (c.Health < c.TotalHealth * 0.4 && Stage < 3 && !c.IsDead)
            {
                c.Say("I am never alone!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                var prms = new List<object>() { 41619, 853039, 856219, 20257, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853204, 856279, 20247, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853329, 856246, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853558, 856098, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853518, 855937, 20254, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853390, 853827, 20263, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                Stage = 3;
            }
            else if (c.Health < c.TotalHealth * 0.6 && Stage < 2 && !c.IsDead)
            {
                c.Say("I'd like you to meet some friends of mine!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                var prms = new List<object>() { 41619, 853039, 856219, 20257, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853204, 856279, 20247, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853329, 856246, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853558, 856098, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853518, 855937, 20254, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853390, 853827, 20263, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                Stage = 2;
            }
            else if (c.Health < c.TotalHealth * 0.8 && Stage < 1 && !c.IsDead)
            {
                c.Say("I am never alone!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                var prms = new List<object>() { 41619, 853039, 856219, 20257, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853204, 856279, 20247, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853329, 856246, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853558, 856098, 20244, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853518, 855937, 20254, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 41619, 853390, 853827, 20263, Obj.Heading }; // Deceived Souls
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                Stage = 1;
            }

            return false;
        }

        public new bool OnLeaveCombat(Object npc = null, object instigator = null)
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
                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 41619)
                    creature.Destroy();

                if (creature != null && creature.Entry == 2000883)
                    creature.Destroy();

                if (creature != null && creature.Entry == 2000884)
                    creature.Destroy();
            }

            addList = new List<Creature>();
            goList = new List<GameObject>();

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            foreach (Creature c in addList)
                c.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();
        }

        public void TraitorousSouls()
        {
            int entry = 0;
            int count = 0;

            foreach (Player player in Obj.PlayersInRange)
            {
                if (player.Realm == GameData.Realms.REALMS_REALM_ORDER)
                    count++;
                else
                    count--;
            }

            if (count > 0)
                entry = 2000883;
            else
                entry = 2000884;

            foreach (Creature creature in addList.ToList())
            {
                if (creature != null && creature.Entry == 41619 && !creature.IsDead)
                {
                    creature.CbtInterface.LeaveCombat();

                    int i = random.Next(0, 2);
                    switch (i)
                    {
                        case 0:
                            creature.Say("She weakens... strike now sisters!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                            break;
                        case 1:
                            creature.Say("We will do your bidding no more!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                            break;
                    }

                    var prms = new List<object>() { entry, creature.WorldPosition.X, creature.WorldPosition.Y, creature.WorldPosition.Z, creature.Heading }; // Deceived Souls
                    Obj.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                    creature.Destroy();
                    //creature.MvtInterface.Follow((Creature)Obj, 5, 10);
                    //creature.CbtInterface.SetTarget(Obj.Oid, GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY);
                }
            }
        }

        public void KillTraitors()
        {
            foreach (Creature creature in addList.ToList())
            {
                if (!creature.IsDead)
                    creature.Health = 0;

                creature.States.Add(3); // Death State

                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH);
                Out.WriteUInt16(creature.Oid);
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WriteUInt16(0);
                Out.Fill(0, 6);
                creature.DispatchPacket(Out, true);

                creature.EvtInterface.AddEvent(creature.Destroy, 10 * 1000, 1);
            }
        }

        public void CaseSleep()
        {
            Creature c = Obj as Creature;

            bool proceed = false;
            int i = 0;
            Player player = null;
            while (!proceed)
            {
                i = random.Next(0, c.PlayersInRange.Count());
                player = c.PlayersInRange[i];
                if (c.GetDistanceToWorldPoint(player) < 151 && !player.IsDead && !player.IsInvulnerable)
                {
                    proceed = true;
                }
            }

            //BuffInfo b = AbilityMgr.GetBuffInfo(14897, c, c); // Iron Body
            //c.BuffInterface.QueueBuff(new BuffQueueInfo(c, c.Level, b)); ()

        }
    }

    [GeneralScript(false, "", 41619, 0)]
    class DeceivedSoulArathremia : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }

    [GeneralScript(false, "", 2000883, 0)]
    class OrderDeceivedSoulArathremia : BasicGunbad
    {
        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            Creature Arathremia = null;

            foreach (Object obj in Obj.ObjectsInRange)
            {
                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 41620)
                {
                    Arathremia = creature;
                }
            }

            Creature crea = Obj as Creature;
            if (crea != null && Arathremia != null)
            {
                crea.AiInterface.CurrentBrain.AddHatred(Arathremia, false, 5000);
                crea.CbtInterface.SetTarget(Arathremia.Oid, GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY);
            }

            return false;
        }
    }

    [GeneralScript(false, "", 2000884, 0)]
    class DestroDeceivedSoulArathremia : BasicGunbad
    {
        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            Creature Arathremia = null;

            foreach (Object obj in Obj.ObjectsInRange)
            {
                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 41620)
                {
                    Arathremia = creature;
                }
            }

            Creature crea = Obj as Creature;
            if (crea != null && Arathremia != null)
            {
                crea.AiInterface.CurrentBrain.AddHatred(Arathremia, false, 5000);
                crea.CbtInterface.SetTarget(Arathremia.Oid, GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY);
            }

            return false;
        }
    }

}
