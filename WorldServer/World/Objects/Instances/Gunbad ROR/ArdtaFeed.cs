using System.Collections.Generic;
using System.Linq;
using Common;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 15102, 0)]
    class ArdtaFeed : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
            // Terror

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);
            //Obj.EvtInterface.AddEvent(ApplyTerrorToEveryoneInRadius, 1000, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public new bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;
            CloseDoor(2094703); // Door Spawn GUID

            c.BuffInterface.RemoveBuffByEntry(20364);

            var prms = new List<object>() { 2000579, 1261116, 1256925, 20496, (int)Obj.Heading, (uint)0 }; // Solithex Mourkain Gem
            Obj.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

            prms = new List<object>() { 2000602, 1261341, 1257348, 20499, (int)Obj.Heading, (uint)0 }; // Slime Pound
            c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

            prms = new List<object>() { 2000602, 1260966, 1257823, 20450, (int)Obj.Heading, (uint)0 }; // Slime Pound
            c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

            prms = new List<object>() { 2000602, 1261887, 1257203, 20505, (int)Obj.Heading, (uint)0 }; // Slime Pound
            c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

            prms = new List<object>() { 2000602, 1262216, 1257734, 20444, (int)Obj.Heading, (uint)0 }; // Slime Pound
            c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

            c.EvtInterface.AddEvent(CastCloudOfSquigs, 30 * 1000, 0);

            foreach (Object o in Obj.ObjectsInRange.ToList())
            {
                GameObject go = o as GameObject;
                if (go != null && go.Entry == 98876) // Mourkain Henge
                {
                    go.VfxState = 0;
                    foreach (Player player in go.PlayersInRange.ToList())
                    {
                        go.SendMeTo(player);
                    }
                }
            }

            return false;
        }

        public new bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;
            OpenDoor(2094703); // Door Spawn GUID
            c.BuffInterface.RemoveBuffByEntry(20364); // Squig Frenzy

            c.EvtInterface.RemoveEvent(CastCloudOfSquigs);

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();
            foreach (Object o in c.ObjectsInRange)
            {
                GameObject go = o as GameObject;

                if (go != null && go.Entry == 98876) // Mourkain Henge
                {
                    go.VfxState = 0;
                    foreach (Player player in go.PlayersInRange.ToList())
                    {
                        go.SendMeTo(player);
                    }
                }

                Creature creature = o as Creature;
                if (creature != null && creature.Entry == 2000945) // Mourkain Henge
                {
                    creature.Destroy();
                }

                if (creature != null && creature.Entry == 2000941) // Mourkain Henge
                {
                    creature.Destroy();
                }
            }

            addList = new List<Creature>();
            goList = new List<GameObject>();

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

            foreach (Object o in Obj.ObjectsInRange)
            {
                GameObject go = o as GameObject;
                /*if (go != null && go.Entry == 2000579) // This is the Mourkain Gem
                {
                    //go.Interactable = false;
                    foreach (Player player in go.PlayersInRange.ToList())
                    {
                        go.SendMeTo(player);
                    }
                }*/
                if (go != null && go.Entry == 98876)
                {
                    go.VfxState = 0;
                    foreach (Player player in go.PlayersInRange.ToList())
                    {
                        go.SendMeTo(player);
                    }
                }
            }

            crea.EvtInterface.RemoveEvent(CastCloudOfSquigs);
            Obj.EvtInterface.RemoveEvent(ApplyTerrorToEveryoneInRadius);

            CreateExitPortal(spawnWorldX, spawnWorldY, spawnWorldZ, Obj.Heading);
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }
            else if (c.Health < c.TotalHealth * 0.2 && Stage < 7 && !c.IsDead)
            {
                EnableGems();

                var prms = new List<object>() { c, (ushort)20364, "'Ard ta Feed roars in rage!" }; // Ard Squig Frenzy Buff
                c.EvtInterface.AddEvent(DelayedBuff, 2000, 1, prms);

                Stage = 7;
            }
            else if (c.Health < c.TotalHealth * 0.25 && Stage < 6 && !c.IsDead)
            {
                foreach (Object o in c.ObjectsInRange.ToList())
                {
                    GameObject go = o as GameObject;
                    if (go != null && go.Entry == 2000602)
                    {
                        go.Destroy();
                    }
                }

                var prms = new List<object>() { 2000602, 1261341, 1257348, 20499, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1260966, 1257823, 20450, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1261887, 1257203, 20505, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1262216, 1257734, 20444, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                Stage = 6;
            }
            else if (c.Health < c.TotalHealth * 0.4 && Stage < 5 && !c.IsDead)
            {
                EnableGems();

                var prms = new List<object>() { c, (ushort)20364, "'Ard ta Feed roars in rage!" }; // Ard Squig Frenzy Buff
                c.EvtInterface.AddEvent(DelayedBuff, 2000, 1, prms);

                Stage = 5;
            }
            else if (c.Health < c.TotalHealth * 0.5 && Stage < 4 && !c.IsDead)
            {
                foreach (Object o in c.ObjectsInRange.ToList())
                {
                    GameObject go = o as GameObject;
                    if (go != null && go.Entry == 2000602)
                    {
                        go.Destroy();
                    }
                }

                var prms = new List<object>() { 2000602, 1261341, 1257348, 20499, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1260966, 1257823, 20450, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1261887, 1257203, 20505, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1262216, 1257734, 20444, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                Stage = 4;
            }
            else if (c.Health < c.TotalHealth * 0.6 && Stage < 3 && !c.IsDead)
            {
                EnableGems();

                var prms = new List<object>() { c, (ushort)20364, "'Ard ta Feed roars in rage!" }; // Ard Squig Frenzy Buff
                c.EvtInterface.AddEvent(DelayedBuff, 2000, 1, prms);

                Stage = 3;
            }
            else if (c.Health < c.TotalHealth * 0.75 && Stage < 2 && !c.IsDead)
            {
                foreach (Object o in c.ObjectsInRange.ToList())
                {
                    GameObject go = o as GameObject;
                    if (go != null && go.Entry == 2000602)
                    {
                        go.Destroy();
                    }
                }

                var prms = new List<object>() { 2000602, 1261341, 1257348, 20499, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1260966, 1257823, 20450, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1261887, 1257203, 20505, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                prms = new List<object>() { 2000602, 1262216, 1257734, 20444, (int)Obj.Heading, (uint)0 }; // Slime Pound
                c.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                Stage = 2;
            }
            else if (c.Health < c.TotalHealth * 0.8 && Stage < 1 && !c.IsDead)
            {
                EnableGems();

                //BuffInfo b = AbilityMgr.GetBuffInfo(20364, c, c); // Rage - Squig Frenzy
                //c.BuffInterface.QueueBuff(new BuffQueueInfo(c, c.Level, b));
                //c.AbtInterface.StartCast(c, 20364, 1);
                //c.EvtInterface.AddEvent(DelayedBuff, 3000, 1);

                var prms = new List<object>() { c, (ushort)20364, "'Ard ta Feed roars in rage!" }; // Ard Squig Frenzy Buff
                c.EvtInterface.AddEvent(DelayedBuff, 2000, 1, prms);

                Stage = 1;
            }

            return false;
        }

        public void EnableGems()
        {
            foreach (Object o in Obj.ObjectsInRange.ToList())
            {
                GameObject go = o as GameObject;
                if (go != null && go.Entry == 2000579) // This is the Mourkain Gem
                {
                    go.LastUsedTimestamp = 0;
                    go.Say("*** As anger of 'Ard ta Feed start to boil the gem fills with wicked energy... LastUpdatedTime is of the essence! ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                    go.PlayEffect(784);
                    //go.Interactable = true;
                    /*foreach (Player player in go.PlayersInRange.ToList())
                    {
                        go.SendMeTo(player);
                    }*/
                }

                if (go != null && go.Entry == 98876) // Mourkain Henge
                {
                    go.VfxState = 1;
                    foreach (Player player in go.PlayersInRange.ToList())
                    {
                        go.SendMeTo(player);
                    }
                }
            }
        }

        public void CastCloudOfSquigs()
        {
            Creature c = Obj as Creature;
            if (c != null)
            {
                //c.Say("Gork will smash' ya! Or Mork?!");

                if (c.PlayersInRange.Count > 0)
                {
                    bool haveTarget = false;
                    int playersInRange = c.PlayersInRange.Count();
                    Player player;
                    while (!haveTarget)
                    {
                        int rndmPlr = random.Next(1, playersInRange + 1);
                        Object obj = c.PlayersInRange.ElementAt(rndmPlr - 1);
                        player = obj as Player;
                        if (player != null && !player.IsDead && !player.IsInvulnerable)
                        {
                            haveTarget = true;

                            var prms = new List<object>() { 2000941, player.WorldPosition.X, player.WorldPosition.Y, player.WorldPosition.Z, Obj.Heading }; // Cloud of Squigs
                            c.EvtInterface.AddEvent(SpawnAdds, 200, 1, prms);

                            break;
                        }
                    }
                }
            }
        }
    }

    [GeneralScript(false, "", 2000945, 0)]
    class ArdSquigFood : BasicGunbad
    {
        List<Creature> ArdList = new List<Creature>();

        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;

            foreach (Object o in Obj.ObjectsInRange.ToList())
            {
                Creature c = o as Creature;
                if (c != null && c.Entry == 15102)
                {
                    ArdList.Add(c);
                    break;
                }
            }

            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
            Obj.EvtInterface.AddEventNotify(EventName.OnDie, HealArd);
        }

        public bool HealArd(Object Obj, object instigator)
        {
            Creature Ard = Obj as Creature;

            Ard = instigator as Creature;

            if (Ard != null && Ard.Entry == 15102 && Ard.TotalHealth - Ard.Health > 5001)
            {
                Ard.Health = Ard.Health + 5000;
                Ard.Say("'Ard ta Feed just swallowed one of the smaller squigs!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_EMOTE);
            }

            return true;
        }

    }

    [GeneralScript(false, "", 2000941, 0)]
    class ArdCloudOfSquigs : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);

            Creature c = Obj as Creature;

            var prms = new List<object>() { Obj, 213 }; // Squig Cloud
            Obj.EvtInterface.AddEvent(LoopVfx, 100, 1, prms);
            Obj.EvtInterface.AddEvent(LoopVfx, 1 * 500, 0, prms); // every 1.5 s
            Obj.EvtInterface.AddEvent(DispellCloudOfSquigs, 30 * 1000, 1);

            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            return false;
        }

        public override bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            foreach (Object obj in c.ObjectsInRange.ToList())
            {
                GameObject go = obj as GameObject;
                if (go != null && go.Entry == 2000602)
                    go.Destroy();

                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 2000945)
                    creature.Destroy();

                if (creature != null && creature.Entry == 2000941)
                    creature.Destroy();
            }

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Obj.EvtInterface.RemoveEvent(LoopVfx);
            Obj.EvtInterface.RemoveEvent(LoopVfx);
        }

        public override void SetRandomTarget()
        {
            Creature c = Obj as Creature;
            if (c != null)
            {
                if (c.PlayersInRange.Count > 0)
                {
                    bool haveTarget = false;
                    int playersInRange = c.PlayersInRange.Count();
                    Player player;
                    while (!haveTarget)
                    {
                        int rndmPlr = random.Next(1, playersInRange + 1);
                        Object obj = c.PlayersInRange.ElementAt(rndmPlr - 1);
                        player = obj as Player;
                        if (player != null && !player.IsDead)
                        {
                            haveTarget = true;
                            c.MvtInterface.TurnTo(player);
                            c.StsInterface.Speed = 123;
                            c.MvtInterface.SetBaseSpeed(123);
                            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All); // This should grant immunity to CC
                            c.MvtInterface.Follow(player, 5, 10);
                            c.AiInterface.CurrentBrain.AddHatred(player, true, 100000);

                            c.Say("*** A horde of wild squigs chase " + player.Name + "! ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                            break;
                        }
                    }
                }
            }
        }

        public void DispellCloudOfSquigs()
        {
            //var Params = (List<object>)parameters;

            //Creature c = Params[0] as Creature;
            Obj.EvtInterface.RemoveEvent(LoopVfx);
            Obj.EvtInterface.RemoveEvent(LoopVfx);
            Obj.Destroy();
        }
    }

    [GeneralScript(false, "", 0, 2000579)]
    class MourkainGemArdtaFeed : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.CaptureDuration = 4;

            GameObject go = Obj as GameObject;
            go.LastUsedTimestamp = 0;
            //go.Interactable = false;

            foreach (Player player in go.PlayersInRange.ToList())
            {
                go.SendMeTo(player);
            }
        }
    }

    [GeneralScript(false, "", 0, 2000602)]
    class ArdSlime : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;

            GameObject go = Obj as GameObject;
            go.Respawn = 0;

            var prms = new List<object>() { 2000945, Obj.WorldPosition.X, Obj.WorldPosition.Y, Obj.WorldPosition.Z, Obj.Heading };
            Obj.EvtInterface.AddEvent(SpawnAdds, 5000, 1, prms);
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

                int count = 0;

                foreach (Player player in Obj.PlayersInRange)
                {
                    if (player.Realm == GameData.Realms.REALMS_REALM_ORDER)
                        count++;
                    else
                        count--;
                }

                if (count > 0)
                    Spawn.Faction = 131;
                else
                    Spawn.Faction = 67;

                //Spawn.Level = 40;
                Creature c = Obj.Region.CreateCreature(Spawn);
                c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds); // We are removing spawns from server when adds die
                addList.Add(c); // Adding adds to the list for easy removal

                go.EvtInterface.AddEvent(SpawnAdds, 20 * 1000, 1, Params);
            }
            else
                go.EvtInterface.RemoveEvent(SpawnAdds);
        }
    }

}
