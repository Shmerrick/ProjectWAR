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
    [GeneralScript(false, "", 36549, 0)]
    class GriblikdaStinka : BasicGunbad
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

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;

            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;

                SetRandomTargetToNPC(2000900); // Greenwingz join fight
            }
            return false;
        }

        public bool CheckFriendInCombat(Object Obj, object instigator)
        {
            SetRandomTargetToNPC(2000900);
            return false;
        }
    }

    [GeneralScript(false, "", 2000900, 0)]
    class OlGreenwingz : BasicGunbad
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

            //Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            SetRandomTargetToNPC(2000900); // Griblik da Stinka join fight

            Obj.EvtInterface.AddEvent(SpawnRandomGO, 1000, 1);

            Obj.EvtInterface.AddEvent(SpawnRandomGO, 30 * 1000, 0);

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
                GameObject go = obj as GameObject;
                if (go != null && go.Entry == 2000576)
                    go.Destroy();
            }

            addList = new List<Creature>();
            goList = new List<GameObject>();

            Obj.EvtInterface.RemoveEvent(SpawnRandomGO);

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

            Obj.EvtInterface.RemoveEvent(SpawnRandomGO);
        }

        public void SpawnRandomGO()
        {
            int X = Obj.WorldPosition.X;
            int Y = Obj.WorldPosition.Y;
            int Z = Obj.WorldPosition.Z;
            foreach (Object o in Obj.ObjectsInRange.ToList())
            {
                Creature crea = o as Creature;
                if (crea != null && crea.Entry == 36549)
                {
                    Z = crea.WorldPosition.Z;
                    break;
                }
            }



            Creature c = Obj as Creature;

            c.Say("*** Ol' Greenwingz shots rotten eggs in every direction! ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_EMOTE);

            if (c.CbtInterface.GetCurrentTarget() != null && (c.CbtInterface.GetCurrentTarget().IsPlayer() || c.CbtInterface.GetCurrentTarget().IsPet()))
            {
                foreach (Player player in c.PlayersInRange.ToList())
                {
                    if (!player.IsDead && !player.IsInvulnerable)
                    {
                        X = player.WorldPosition.X;
                        Y = player.WorldPosition.Y;
                        Z = player.WorldPosition.Z;

                        var prms = new List<object>() { 2000576, (X + 125 - (25 * random.Next(1, 11))), (Y + 125 - (25 * random.Next(1, 11))), Z, (int)Obj.Heading, (uint)0 }; // Rotten eggs
                        Obj.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                        //prms = new List<object>() { 2000576, (X + 125 - (25 * random.Next(1, 11))), (Y + 125 - (25 * random.Next(1, 11))), Z, (int)Obj.Heading, (uint)0 }; // Rotten eggs
                        //Obj.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);

                        //prms = new List<object>() { 2000576, (X + 125 - (25 * random.Next(1, 11))), (Y + 125 - (25 * random.Next(1, 11))), Z, (int)Obj.Heading, (uint)0 }; // Rotten eggs
                        //Obj.EvtInterface.AddEvent(SpawnGO, 100, 1, prms);
                    }
                }
            }
        }

        public bool CheckFriendInCombat(Object Obj, object instigator)
        {
            SetRandomTargetToNPC(36549);
            return false;
        }
    }

    [GeneralScript(false, "", 0, 2000576)]
    class RottenEggOlGreenwingz : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;
            Obj.EvtInterface.AddEvent(BreakEgg, (5 + random.Next(1,6)) * 1000, 1);

            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);

            Obj.PlayEffect(2185);
        }

        public override void OnDie(Object Obj)
        {
            //Obj.PlayEffect(2185);
            //BreakEgg();
            Obj.EvtInterface.RemoveEvent(BreakEgg);
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            BreakEgg();

            return false;
        }

        public void BreakEgg()
        {
            GameObject go = Obj as GameObject;

            go.PlayEffect(2185);

            go.Say("*** Terrible stench of rotten wyvern eggs fills the cave... ***", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_EMOTE);

            Creature OlGreenwingz = null;

            foreach (Object o in Obj.ObjectsInRange)
            {
                Creature c = o as Creature;
                if (c != null && c.Entry == 2000900)
                {
                    OlGreenwingz = c;
                }
            }

            if (OlGreenwingz != null)
            {
                foreach (Player player in Obj.PlayersInRange.ToList())
                {
                    if (player != null && !player.IsDead && !player.IsInvulnerable && go.GetDistanceToObject(player) < 31)
                    {
                        BuffInfo b = AbilityMgr.GetBuffInfo(20356, OlGreenwingz, player); // Bad Gaz
                        player.BuffInterface.QueueBuff(new BuffQueueInfo(OlGreenwingz, OlGreenwingz.Level, b));

                        b = AbilityMgr.GetBuffInfo(1927, OlGreenwingz, player); // Sticky Feetz
                        player.BuffInterface.QueueBuff(new BuffQueueInfo(OlGreenwingz, OlGreenwingz.Level, b));
                    }
                }
            }

            go.AbtInterface.StartCast(go, 1927, 1);

            Obj.EvtInterface.AddEvent(Obj.Destroy, 1000, 1);
        }
    }

}