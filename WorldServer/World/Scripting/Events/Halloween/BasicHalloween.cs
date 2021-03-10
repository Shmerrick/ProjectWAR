using System;
using System.Collections.Generic;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.Events.Halloween
{
    internal class BasicHalloween : AGeneralScript
    {
        protected Object Obj; // This is creature
        public Random random = new Random();

        protected Point3D spawnPoint;
        protected int spawnWorldX;
        protected int spawnWorldY;
        protected int spawnWorldZ;
        protected int spawnWorldO;
        protected List<Object> stuffInRange = new List<Object>(); // This list keeps all objects in range
        protected List<Creature> addList = new List<Creature>(); // this list keeps all adds spawned by boss
        protected List<Objects.GameObject> goList = new List<Objects.GameObject>(); // this list keeps all adds spawned by boss
        protected int Stage = -1; // This is variable that controls combat Stage

        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;
            spawnWorldX = (int)Obj.WorldPosition.X;
            spawnWorldY = (int)Obj.WorldPosition.Y;
            spawnWorldZ = (int)Obj.WorldPosition.Z;
            spawnWorldO = (int)Obj.Heading;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
        }

        public virtual bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;
            return false;
        }

        public virtual bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            addList = new List<Creature>();
            goList = new List<Objects.GameObject>();

            return false;
        }

        public bool ApplyTerror(Object npc = null, object instigator = null)
        {
            Unit u = Obj as Unit;
            foreach (Player player in Obj.PlayersInRange)
            {
                BuffInfo b = AbilityMgr.GetBuffInfo(5968, u, player); // This is Terror buff
                player.BuffInterface.QueueBuff(new BuffQueueInfo(u, 40, b));
            }

            return true;
        }

        public bool RemoveTerror(Object npc = null, object instigator = null)
        {
            foreach (Player player in Obj.PlayersInRange)
            {
                if (player.BuffInterface.GetBuff(5968, player) != null)
                {
                    player.BuffInterface.RemoveBuffByEntry(5968);
                }
            }

            return true;
        }
    }

    [GeneralScript(false, "", 2000810, 0)]
    internal class NPC0 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000811, 0)]
    internal class NPC1 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000812, 0)]
    internal class NPC2 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000813, 0)]
    internal class NPC3 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000814, 0)]
    internal class NPC4 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000815, 0)]
    internal class NPC5 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000816, 0)]
    internal class NPC6 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000817, 0)]
    internal class NPC7 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000818, 0)]
    internal class NPC8 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000819, 0)]
    internal class NPC9 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000820, 0)]
    internal class NPC10 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000821, 0)]
    internal class NPC11 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000822, 0)]
    internal class NPC12 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000823, 0)]
    internal class NPC13 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000824, 0)]
    internal class NPC14 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000825, 0)]
    internal class NPC15 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000826, 0)]
    internal class NPC16 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000827, 0)]
    internal class NPC17 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000828, 0)]
    internal class NPC18 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000829, 0)]
    internal class NPC19 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000830, 0)]
    internal class NPC20 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000831, 0)]
    internal class NPC21 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000832, 0)]
    internal class NPC22 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000833, 0)]
    internal class NPC23 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000834, 0)]
    internal class NPC24 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000835, 0)]
    internal class NPC25 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }

    [GeneralScript(false, "", 2000836, 0)]
    internal class NPC26 : BasicHalloween
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            // Terror
            //Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, ApplyTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, RemoveTerror);
            //Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveTerror);
        }
    }
}