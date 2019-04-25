using System.Collections.Generic;
using System.Linq;
using Common;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    class BasicGunbad : BasicScript
    {
        public virtual void SpawnAdds(object crea)
        {
            var Params = (List<object>)crea;

            int Entry = (int)Params[0];
            int X = (int)Params[1];
            int Y = (int)Params[2];
            int Z = (int)Params[3];
            ushort O = (ushort)Params[4];

            Creature_proto Proto = CreatureService.GetCreatureProto((uint)Entry);

            Creature_spawn Spawn = new Creature_spawn();
            Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = (int)O;
            Spawn.WorldX = X;
            Spawn.WorldY = Y;
            Spawn.WorldZ = Z;
            Spawn.ZoneId = (ushort)Obj.ZoneId;
            Creature c = Obj.Region.CreateCreature(Spawn);
            c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds); // We are removing spawns from server when adds die
            addList.Add(c); // Adding adds to the list for easy removal
        }

        public virtual void SetRandomTarget()
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
                            haveTarget = true;
                            c.MvtInterface.TurnTo(player);
                            c.MvtInterface.SetBaseSpeed(400);
                            c.MvtInterface.Follow(player, 5, 10);
                            break;
                        }
                    }
                }
            }
        }

        public virtual void GoToMommy(object mommy)
        {
            var Params = (List<object>)mommy;
            uint Entry = (uint)Params[0];

            Creature c = Obj as Creature;

            foreach (Object obj in c.ObjectsInRange)
            {
                Creature crea = obj as Creature;
                if (crea != null && crea.Entry == Entry)
                {
                    c.MvtInterface.TurnTo(crea);
                    //c.MvtInterface.SetBaseSpeed(400);
                    c.MvtInterface.Follow(crea, 5, 10);
                    break;
                }
            }
        }

        public void SpawnGO(object list)
        {
            var Params = (List<object>)list;

            int Entry = (int)Params[0];
            int X = (int)Params[1];
            int Y = (int)Params[2];
            int Z = (int)Params[3];
            int O = (int)Params[4];
            uint AllowVfxUpdate = (uint)Params[5];

            GameObject_proto Proto = GameObjectService.GetGameObjectProto((uint)Entry);

            GameObject_spawn Spawn = new GameObject_spawn();
            Spawn.Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID();
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = O;
            Spawn.WorldX = X;
            Spawn.WorldY = Y;
            Spawn.WorldZ = Z;
            Spawn.ZoneId = (ushort)Obj.ZoneId;
            Spawn.AllowVfxUpdate = AllowVfxUpdate;
            GameObject go = Obj.Region.CreateGameObject(Spawn);
            go.Respawn = 0;
            go.EvtInterface.AddEventNotify(EventName.OnDie, RemoveGOs); // We are removing spawns from server when adds die

            goList.Add(go); // Adding adds to the list for easy removal
        }

        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 10 * 1000, 1);
            return false;
        }

        public bool RemoveGOs(Object obj = null, object instigator = null)
        {
            GameObject go = obj as GameObject;
            go.EvtInterface.AddEvent(go.Destroy, 10 * 1000, 1);
            return false;
        }

        public virtual void RemoveBuffs()
        {
            Creature c = this.Obj as Creature;
            c.IsInvulnerable = false;
            foreach (Player plr in c.PlayersInRange.ToList())
                c.SendMeTo(plr);
        }

        public bool OnKilledPlayer(Object pkilled, object instigator)
        {
            Player plr = pkilled as Player; // Casting object pkilled to Player type

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();
        }

        public void CreateExitPortal(int X, int Y, int Z, int O)
        {
            GameObject_proto Proto = GameObjectService.GetGameObjectProto(98878); // This is portal

            GameObject_spawn Spawn = new GameObject_spawn();
            Spawn.Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID();
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = O;
            Spawn.WorldX = X;
            Spawn.WorldY = Y;
            Spawn.WorldZ = Z;
            Spawn.ZoneId = (ushort)Obj.ZoneId;
            GameObject go = Obj.Region.CreateGameObject(Spawn);
            go.Respawn = 0;
            go.EvtInterface.AddEvent(go.Destroy, 30 * 10 * 1000, 1);

            //goList.Add(go); // Adding adds to the list for easy removal
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

        public void ApplyTerrorToEveryoneInRadius()
        {
            Unit u = Obj as Unit;
            foreach (Player player in Obj.PlayersInRange)
            {
                BuffInfo b = AbilityMgr.GetBuffInfo(5968, u, player); // This is Terror buff
                player.BuffInterface.QueueBuff(new BuffQueueInfo(u, 40, b));
            }
        }

        public bool ApplyIronSkin(Object npc = null, object instigator = null)
        {
            Unit u = Obj as Unit;
            BuffInfo b = AbilityMgr.GetBuffInfo(5262); // This is Iron Skin buff - Squig Commando
            b.Duration = 30;
            u.BuffInterface.QueueBuff(new BuffQueueInfo(u, u.Level, b));

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

        public void ClearImmunities()
        {
            Creature c = Obj as Creature;
            if (c != null && !c.IsDead)
            {
                foreach (Player player in c.PlayersInRange.ToList())
                {
                    player.BuffInterface.RemoveBuffByEntry(402); // Removing Immunity
                    NewBuff newBuff = player.BuffInterface.GetBuff(402, null);
                    if (newBuff != null)
                        newBuff.RemoveBuff(true);

                    player.BuffInterface.RemoveBuffByEntry(403); // Removing Immunity
                    newBuff = player.BuffInterface.GetBuff(403, null);
                    if (newBuff != null)
                        newBuff.RemoveBuff(true);
                }

            }
        }

    }
}
