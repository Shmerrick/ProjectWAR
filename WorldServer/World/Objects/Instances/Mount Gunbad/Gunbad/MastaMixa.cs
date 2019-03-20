using System.Collections.Generic;
using System.Linq;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 37967, 0)]
    class MastaMixa : BasicGunbad
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

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            SpawnDaStaff();

            Obj.EvtInterface.AddEvent(SpawnDaStaff, (50 + random.Next(1,41)) * 1000, 0); // Spawns staff

            var prms = new List<object>() { 2000899, 977879, 994054, 26299, Obj.Heading }; // Masta Mixa Fanatic
            c.EvtInterface.AddEvent(SpawnAdds, 30 * 1000, 0, prms);
            prms = new List<object>() { 2000899, 977127, 995380, 26307, Obj.Heading }; // Masta Mixa Fanatic
            c.EvtInterface.AddEvent(SpawnAdds, 30 * 1000, 0, prms);

            c.EvtInterface.AddEvent(CastFistOfGork, 30 * 1000, 0);

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
            foreach (Object obj in c.ObjectsInRange)
            {
                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 2000851)
                    creature.Destroy();
            }

            Obj.EvtInterface.RemoveEvent(SpawnDaStaff);

            addList = new List<Creature>();
            goList = new List<GameObject>();

            c.EvtInterface.RemoveEvent(SpawnAdds);
            c.EvtInterface.RemoveEvent(SpawnAdds);
            Obj.EvtInterface.RemoveEvent(CastFistOfGork);

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            Obj.EvtInterface.RemoveEvent(SpawnDaStaff);

            foreach (Creature c in addList)
                c.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            Obj.EvtInterface.RemoveEvent(SpawnAdds);
            Obj.EvtInterface.RemoveEvent(SpawnAdds);
            Obj.EvtInterface.RemoveEvent(CastFistOfGork); 

            CreateExitPortal(spawnWorldX, spawnWorldY, spawnWorldZ, Obj.Heading);
        }

        public void SpawnDaStaff()
        {
            Creature c = Obj as Creature;

            if (!c.IsDead)
            { 
                bool staffActive = false;

                foreach (Object obj in Obj.ObjectsInRange.ToList())
                {
                    Creature creature = obj as Creature;
                    if (creature != null && creature.Entry == 2000851 && !creature.IsDead)
                    {
                        staffActive = true;
                        break;
                    }
                }

                if (!staffActive)
                {
                    c.Say("Where 'z my stick?!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                    var prms = new List<object>() { 2000851, c.WorldPosition.X, c.WorldPosition.Y, c.WorldPosition.Z, Obj.Heading }; // Gitzappa da Stick
                    c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
                }
            }
        }

        public void CastFistOfGork()
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

                        if (player != null && !player.IsDead && !player.IsInvulnerable)
                        {
                            NewBuff newBuff = player.BuffInterface.GetBuff(5239, null);
                            if (newBuff == null)
                            {
                                haveTarget = true;

                                var prms = new List<object>() { 2000902, player.WorldPosition.X, player.WorldPosition.Y, player.WorldPosition.Z, Obj.Heading }; // Fist of Gork
                                c.EvtInterface.AddEvent(SpawnAdds, 200, 1, prms);

                                c.Say("Gork will smash' ya " + player.Name + "! Or Mork?!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    [GeneralScript(false, "", 2000851, 0)]
    class DaStaffMastaMixa : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);

            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Stage = -1;
            //DisablePlayer();
            //Obj.EvtInterface.AddEvent(DisablePlayer, 60 * 1000, 0);

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

            //Obj.EvtInterface.RemoveEvent(DisablePlayer);

            foreach (Player player in Obj.PlayersInRange.ToList())
            {
                player.BuffInterface.RemoveBuffByEntry(5240); // Removing Disable
                //NewBuff newBuff = player.BuffInterface.GetBuff(5240, null);
                //if (newBuff != null)
                    //newBuff.RemoveBuff(true);

                player.BuffInterface.RemoveBuffByEntry(5239); // Removing Disable
                //newBuff = player.BuffInterface.GetBuff(5239, null);
                //if (newBuff != null)
                    //newBuff.RemoveBuff(true);
            }

            Obj.EvtInterface.AddEvent(Obj.Destroy, 100, 1);
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
                            c.MvtInterface.SetBaseSpeed(400);
                            c.MvtInterface.Follow(player, 5, 50);
                            c.AiInterface.CurrentBrain.AddHatred(player, true, 5000);
                            break;
                        }
                    }
                }
            }
        }

        public void DisablePlayer()
        {
            Creature creature = Obj as Creature;
            if (creature != null)
            {
                if (creature.PlayersInRange.Count > 0)
                {
                    bool haveTarget = false;
                    int playersInRange = creature.PlayersInRange.Count();
                    Player player;
                    while (!haveTarget)
                    {
                        int rndmPlr = random.Next(1, playersInRange + 1);
                        Object obj = creature.PlayersInRange.ElementAt(rndmPlr - 1);
                        player = obj as Player;
                        if (player != null && !player.IsDead)
                        {
                            haveTarget = true;
                            creature.MvtInterface.TurnTo(player);
                            creature.MvtInterface.Follow(player, 5, 50);
                            creature.AiInterface.CurrentBrain.AddHatred(player, true, 5000);

                            creature.Say("*** Zapping " + player.Name + " ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                            BuffInfo b = AbilityMgr.GetBuffInfo(5240, creature, player); // Disable
                            creature.BuffInterface.QueueBuff(new BuffQueueInfo(creature, creature.Level, b));

                            player.PlayEffect(1390);

                            var prms = new List<object>() { player, 165 };
                            PlayAnimation(prms);

                            prms = new List<object>() { player, 0 };
                            player.EvtInterface.AddEvent(PlayAnimation, 595 * 100, 1, prms);

                            break;
                        }
                    }
                }
            }
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = Obj as Creature;

            if (c != null)
            {
                if (Stage < 0 && !c.IsDead)
                {
                    Stage = 0; // Setting control value to 0
                }
                else if (c.Health < c.TotalHealth * 0.05 && Stage < 1 && !c.IsDead)
                {
                    foreach (Player player in Obj.PlayersInRange.ToList())
                    {
                        player.BuffInterface.RemoveBuffByEntry(5240); // Removing Disable
                        NewBuff newBuff = player.BuffInterface.GetBuff(5240, null);
                        if (newBuff != null)
                            newBuff.RemoveBuff(true);

                        player.BuffInterface.RemoveBuffByEntry(5239); // Removing Disable
                        newBuff = player.BuffInterface.GetBuff(5239, null);
                        if (newBuff != null)
                            newBuff.RemoveBuff(true);
                    }

                    Stage = 1;
                }
            }
            
            return false;
        }
    }

    [GeneralScript(false, "", 2000899, 0)]
    class MixaFanatics : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }

    [GeneralScript(false, "", 2000902, 0)]
    class MixaFistOfGork : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);

            Creature c = Obj as Creature;

            var prms = new List<object>() { Obj, 239 }; // Fist of Gork
            Obj.EvtInterface.AddEvent(LoopVfx, 100, 1, prms);
            Obj.EvtInterface.AddEvent(LoopVfx, 15 * 100, 0, prms); // every 1.5 s
            Obj.EvtInterface.AddEvent(DispellFistOfGork, 30 * 1000, 1);

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
                            break;
                        }
                    }
                }
            }
        }

        public void DispellFistOfGork()
        {
            //var Params = (List<object>)parameters;

            //Creature c = Params[0] as Creature;
            Obj.EvtInterface.RemoveEvent(LoopVfx);
            Obj.EvtInterface.RemoveEvent(LoopVfx);
            Obj.Destroy();
        }
    }
}
