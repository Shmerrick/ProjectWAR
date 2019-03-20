using System.Collections.Generic;
using System.Linq;
using Common;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 42207, 0)]
    class WightLordSolithex : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
            // Terror
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;

            c.Say("Come, mortals, and bathe in my Power!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            c.BuffInterface.RemoveBuffByEntry(14897); // Removing Iron Body
            NewBuff newBuff = c.BuffInterface.GetBuff(14897, null);
            if (newBuff != null)
                newBuff.RemoveBuff(true);

            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;

                var prms = new List<object>() { 100523, 1110218, 1118213, 19460, (int)Obj.Heading, (uint)0 }; // Solithex Soulstone
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 100524, 1110226, 1118211, 19459, (int)Obj.Heading, (uint)0 }; // Solithex Barrier
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);
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
            foreach (Object obj in c.ObjectsInRange.ToList())
            {
                GameObject go = obj as GameObject;
                if (go != null && go.Entry == 2000561)
                    go.Destroy();

                if (go != null && go.Entry == 100523)
                    go.Destroy();

                if (go != null && go.Entry == 100524)
                    go.Destroy();

                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 2000876)
                    creature.Destroy();

                if (creature != null && creature.Entry == 42211)
                    creature.Destroy();
            }

            addList = new List<Creature>();
            goList = new List<GameObject>();

            c.BuffInterface.RemoveBuffByEntry(14897); // Removing Iron Body
            NewBuff newBuff = c.BuffInterface.GetBuff(14897, null);
            if (newBuff != null)
                newBuff.RemoveBuff(true);

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

            /*foreach (Object obj in Obj.ObjectsInRange.ToList())
            {
                GameObject go = obj as GameObject;
                if (go != null && go.Entry == 100523)
                {
                    go.Health = 0;

                    go.States.Add(3); // Death State

                    PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH);
                    Out.WriteUInt16(go.Oid);
                    Out.WriteByte(1);
                    Out.WriteByte(0);
                    Out.WriteUInt16(0);
                    Out.Fill(0, 6);
                    go.DispatchPacket(Out, true);
                    break;
                }
            }*/

            addList = new List<Creature>();
            goList = new List<GameObject>();

            CreateExitPortal(spawnWorldX, spawnWorldY, spawnWorldZ, Obj.Heading);
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }
            else if (c.Health < c.TotalHealth * 0.2 && Stage < 3 && !c.IsDead)
            {
                var prms = new List<object>() {c, (ushort)14897, "This battle may be yours but my will persists. If we meet again, it is you that will return to the soil and eternally serve the Mourkain!" }; // Iron Body
                c.EvtInterface.AddEvent(DelayedBuff, 100, 1, prms);

                foreach (Object o in c.ObjectsInRange.ToList())
                {
                    GameObject go = o as GameObject;
                    if (go != null && go.Entry == 100524)
                    {
                        go.Say("*** Magical barrier collaps and the gem is in your reach... ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_EMOTE);
                        go.Destroy();
                    }

                    if (go != null && go.Entry == 100523)
                    {
                        go.IsInvulnerable = false;
                        go.IsAttackable = 1;
                        go.Health = go.TotalHealth;

                        //go.Say("*** Cracks start to show upon surface of the gem... ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_EMOTE);

                        foreach (Player plr in go.PlayersInRange.ToList())
                        {
                            if (plr != null)
                            {
                                BuffInfo b = AbilityMgr.GetBuffInfo(13058, c, plr); // Killer dot
                                plr.BuffInterface.QueueBuff(new BuffQueueInfo(c, c.Level, b));
                                go.SendMeTo(plr);
                            }
                        }

                        break;
                    }
                }

                Stage = 3;

                //c.Say("Stage 3");
            }
            else if (c.Health < c.TotalHealth * 0.4 && Stage < 2 && !c.IsDead)
            {
                var prms = new List<object>() { 2000561, 1109983, 1119476, 19138, (int)Obj.Heading, (uint)0 }; // Deathshadow Drudge
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000561, 1110752, 1119141, 19134, (int)Obj.Heading, (uint)0 }; // Deathshadow Drudge
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000561, 1111016, 1119874, 19145, (int)Obj.Heading, (uint)0 }; // Deathshadow Drudge
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000561, 1110467, 1120222, 19146, (int)Obj.Heading, (uint)0 }; // Deathshadow Drudge
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                c.Say("Arise, my servants, for it is time the Mourkain retake this word!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                //c.IsInvulnerable = true;

                //c.EvtInterface.AddEvent(RemoveBuffs, 30000, 1);

                Stage = 2;

                //c.Say("Stage 2");
            }
            else if (c.Health < c.TotalHealth * 0.7 && Stage < 1 && !c.IsDead)
            {
                var prms = new List<object>() { 2000876, 1109983, 1119476, 19138, Obj.Heading }; // Deathshadow Construct
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000876, 1110752, 1119141, 19134, Obj.Heading }; // Deathshadow Construct
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000876, 1111016, 1119874, 19145, Obj.Heading }; // Deathshadow Construct
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000876, 1110467, 1120222, 19146, Obj.Heading }; // Deathshadow Construct
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                c.Say("Arise, my servants, for it is time the Mourkain retake this word!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                //c.IsInvulnerable = true;

                //c.EvtInterface.AddEvent(RemoveBuffs, 30000, 1);

                Stage = 1;

                //c.Say("Stage 1");
            }

            return false;
        }

        public override void RemoveBuffs()
        {
            Creature c = this.Obj as Creature;
            c.Say("Feel my wrath!",SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            c.IsInvulnerable = false;
            foreach (Player plr in c.PlayersInRange.ToList())
                c.SendMeTo(plr);
        }
    }

    [GeneralScript(false, "", 2000876, 0)] // Deathshadow Construct
    class SolithexDeathshadowConstruct : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }

    [GeneralScript(false, "", 42211, 0)] // Deathshadow Drudge
    class SolithexDeathshadowDrudge : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }

    [GeneralScript(false, "", 0, 100523)]
    class SolithexArtifact : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;

            Stage = -1;

            GameObject go = Obj as GameObject;
            go.Respawn = 0;
            go.IsInvulnerable = true;
            go.IsAttackable = 0;

            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);

            /*foreach (Player plr in go.PlayersInRange.ToList())
            {
                go.SendMeTo(plr);
            }*/
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            GameObject go = this.Obj as GameObject; // We are casting the script initiator as a Creature

            if (Stage < 0 && !go.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }
            if (go.Health < go.TotalHealth * 0.1)
            {
                go.PlayEffect(2185);

                go.Say("*** The cursed gem breaks into milion pieces! ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_EMOTE);

                foreach (Object o in go.ObjectsInRange.ToList())
                {
                    Player plr = o as Player;
                    if (plr != null)
                    {
                        plr.BuffInterface.RemoveBuffByEntry(13058); // Removing killer dot
                        NewBuff newBuff = plr.BuffInterface.GetBuff(13058, null);
                        if (newBuff != null)
                            newBuff.RemoveBuff(true);

                        plr.SendClientMessage("The sinister force that consumed your soul is lifted from you!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_EMOTE);
                    }

                    Creature c = o as Creature;
                    if (c != null && c.Entry == 42207)
                    {
                        c.BuffInterface.RemoveBuffByEntry(14897); // Removing Iron Body
                        NewBuff newBuff = c.BuffInterface.GetBuff(14897, null);
                        if (newBuff != null)
                            newBuff.RemoveBuff(true);
                    }

                    go.Destroy();
                }

                Stage = 1;
            }

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
        }
    }

    [GeneralScript(false, "", 0, 100524)]
    class SolithexBarrier : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;

            GameObject go = Obj as GameObject;
            go.Respawn = 0;
            go.IsAttackable = 0;
            go.IsInvulnerable = true;

            foreach (Player plr in go.PlayersInRange.ToList())
            {
                go.SendMeTo(plr);
            }
        }
    }

    [GeneralScript(false, "", 0, 2000561)]
    class SolithexMourkainPillar : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;

            GameObject go = Obj as GameObject;
            go.Respawn = 0;

            var prms = new List<object>() { 42211, Obj.WorldPosition.X, Obj.WorldPosition.Y, Obj.WorldPosition.Z, Obj.Heading }; 
            Obj.EvtInterface.AddEvent(SpawnAdds,5000,1,prms);
        }

        public override void SpawnAdds(object crea)
        {
            var Params = (List<object>)crea;

            int Entry = (int)Params[0];
            int X = (int)Params[1];
            int Y = (int)Params[2];
            int Z = (int)Params[3];
            ushort O = (ushort)Params[4];
            GameObject go = Obj as GameObject;

            if (go != null && !go.IsDead)
            {
                Creature_proto Proto = CreatureService.GetCreatureProto((uint)Entry);

                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = (int)O;
                Spawn.WorldX = X;
                Spawn.WorldY = Y;
                Spawn.WorldZ = Z;
                Spawn.ZoneId = (ushort)Obj.ZoneId;
                //Spawn.Level = 40;
                Creature c = Obj.Region.CreateCreature(Spawn);
                c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds); // We are removing spawns from server when adds die
                addList.Add(c); // Adding adds to the list for easy removal

                go.EvtInterface.AddEvent(SpawnAdds, 15000, 1, Params);
            }
            else
                go.EvtInterface.RemoveEvent(SpawnAdds);
        }
    }
}
