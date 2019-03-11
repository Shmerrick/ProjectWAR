using System;
using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scripting
{
    class BasicScript : AGeneralScript
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
            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }
            return false;
        }

        public virtual bool OnLeaveCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }

            foreach (Creature creature in addList)
                creature.Destroy();
            foreach (Objects.GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<Objects.GameObject>();

            return false;
        }

        public override void OnRemoveObject(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            foreach (Creature c in addList)
                c.Destroy();
            foreach (Objects.GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<Objects.GameObject>();
        }

        public override void OnRemoveFromWorld(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            foreach (Creature c in addList)
                c.Destroy();
            foreach (Objects.GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<Objects.GameObject>();
        }

        public void MountNPC(Unit target, uint entry)
        {
            /*var Params = (List<object>)crea;

            Unit target = (Unit)Params[0];
            uint Entry = (uint)Params[1];*/

            Common.Mount_Info info = new Common.Mount_Info();

            info.Entry = entry;
            target.Mount((ushort)info.Entry);
        }

        public void DismountNPC()
        {
            Creature c = Obj as Creature;
            Common.Mount_Info info = new Common.Mount_Info();

            info.Entry = 0;
            c.Mount((ushort)info.Entry);
        }

        public void SetInvisible(string name = "")
        {
            Creature c = Obj as Creature;
            c.Model1 = 999; // Invisi-dude

            if (name != "")
                c.Name = name;

            foreach (Player player in c.PlayersInRange.ToList())
            {
                c.SendMeTo(player);
            }
        }

        public void SetVisible(string name = "")
        {
            Creature c = Obj as Creature;
            c.Model1 = c.Spawn.Proto.Model1; // Invisi-dude

            if (name != "")
                c.Name = name;

            c.PlayEffect(2185);

            foreach (Player player in c.PlayersInRange.ToList())
            {
                c.SendMeTo(player);
            }
        }

        public void CloseDoor(uint door)
        {
            foreach (Object o in Obj.ObjectsInRange)
            {
                Objects.GameObject go = o as Objects.GameObject;
                if (go != null && go.Spawn.Guid == door)
                {
                    go.Interactable = false;
                    go.CloseDoor();

                    foreach (Player player in go.PlayersInRange)
                        go.SendMeTo(player);

                    return;
                }
            }
        }

        public void OpenDoor(uint door)
        {
            foreach (Object o in Obj.ObjectsInRange)
            {
                Objects.GameObject go = o as Objects.GameObject;
                if (go != null && go.Spawn.Guid == door)
                {
                    go.Interactable = true;
                    go.OpenDoor(false);

                    foreach (Player player in go.PlayersInRange)
                        go.SendMeTo(player);

                    return;
                }
            }
        }


        public void LoopVfx(object parameters)
        {
            var Params = (List<object>)parameters;

            Object o = Params[0] as Object;
            ushort effectId = (ushort)((int)Params[1]);

            if (o != null)
            { 
                o.PlayEffect(effectId);
            }
        }

        public void PlayAnimation(object parameters)
        {
            var Params = (List<object>)parameters;

            Object o = Params[0] as Object;
            ushort animID = (ushort)((int)Params[1]);

            var Out = new PacketOut((byte)Opcodes.F_ANIMATION);

            Out.WriteUInt16(o.Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)animID);

            foreach (Player player in o.PlayersInRange)
            {
                player.DispatchPacket(Out, true);
            }
        }

        public void DelayedBuff(object prms)
        {
            var Params = (List<object>)prms;

            Creature c = (Creature)Params[0];
            ushort BuffId = (ushort)Params[1];
            string Text = (string)Params[2];

            BuffInfo b = AbilityMgr.GetBuffInfo(BuffId, c, c);
            c.BuffInterface.QueueBuff(new BuffQueueInfo(c, c.Level, b));

            if (!String.IsNullOrEmpty(Text))
                c.Say(Text, SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
        }

        public void DelayedCast(object prms)
        {
            var Params = (List<object>)prms;

            Creature c = (Creature)Params[0];
            ushort AbilityId = (ushort)Params[1];
            string Text = (string)Params[2];

            c.AbtInterface.StartCast(c, AbilityId, 1);

            if (!String.IsNullOrEmpty(Text))
                c.Say(Text, SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
        }

        public void SetRandomTargetToNPC(int entry)
        {
            Creature c = null;
            foreach (Object o in Obj.ObjectsInRange.ToList())
            {
                Creature crea = o as Creature;
                if (crea != null && crea.Entry == entry)
                {
                    c = crea;
                    break;
                }
            }

            if (c != null && !c.IsDead && c.CbtInterface.GetCurrentTarget() == null && !c.CbtInterface.IsInCombat && !c.MvtInterface.IsMoving)
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
                            haveTarget = true;
                            c.MvtInterface.TurnTo(player);
                            c.MvtInterface.Follow(player, 5, 10);
                            break;
                        }
                    }
                }
            }
        }
    }
}
