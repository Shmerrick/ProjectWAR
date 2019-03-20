using System.Collections.Generic;
using System.Linq;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 36612, 0)]
    class ElderKizzig : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override bool OnEnterCombat(Object npc = null, object instigator = null)
        {
            Creature c = Obj as Creature;
            c.IsInvulnerable = false;
            Stage = -1;

            var prms = new List<object>() { 36598, (Obj.WorldPosition.X + 250 - (50 * random.Next(1, 11))), (Obj.WorldPosition.Y + 250 - (50 * random.Next(1, 11))), Obj.WorldPosition.Z, Obj.Heading }; // Spawn snotling
            c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

            c.EvtInterface.AddEvent(SpawnSnotlings, 100, 1);

            c.EvtInterface.AddEvent(SpawnSnotlings, 60 * 1000, 0);

            c.Say("Com 'er Chipfang, we 'l show 'em!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            c.BuffInterface.RemoveBuffByEntry(13155); // Removing rage
            NewBuff newBuff = c.BuffInterface.GetBuff(13155, null);
            if (newBuff != null)
                newBuff.RemoveBuff(true);

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

            c.BuffInterface.RemoveBuffByEntry(13155); // Removing rage
            NewBuff newBuff = c.BuffInterface.GetBuff(13155, null);
            if (newBuff != null)
                newBuff.RemoveBuff(true);

            c.EvtInterface.RemoveEvent(SpawnSnotlings);
            c.EvtInterface.RemoveEvent(SpawnSnotlings);
            c.EvtInterface.RemoveEvent(SpawnSnotlings);
            c.EvtInterface.RemoveEvent(SpawnSnotlings);
            c.EvtInterface.RemoveEvent(SpawnSnotlings);
            c.EvtInterface.RemoveEvent(SpawnSnotlings);

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

            crea.BuffInterface.RemoveBuffByEntry(13155); // Removing rage

            addList = new List<Creature>();
            goList = new List<GameObject>();

            crea.EvtInterface.RemoveEvent(SpawnSnotlings);
            crea.EvtInterface.RemoveEvent(SpawnSnotlings);
            crea.EvtInterface.RemoveEvent(SpawnSnotlings);
            crea.EvtInterface.RemoveEvent(SpawnSnotlings);
            crea.EvtInterface.RemoveEvent(SpawnSnotlings);
            crea.EvtInterface.RemoveEvent(SpawnSnotlings);
        }

        public void SpawnSnotlings()
        {
            var prms = new List<object>() { 2000903, (Obj.WorldPosition.X + 250 - (50 * random.Next(1, 11))), (Obj.WorldPosition.Y + 250 - (50 * random.Next(1, 11))), Obj.WorldPosition.Z, Obj.Heading }; // Snotling
            Obj.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
            prms = new List<object>() { 2000903, (Obj.WorldPosition.X + 250 - (50 * random.Next(1, 11))), (Obj.WorldPosition.Y + 250 - (50 * random.Next(1, 11))), Obj.WorldPosition.Z, Obj.Heading }; // Snotling
            Obj.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
            prms = new List<object>() { 2000903, (Obj.WorldPosition.X + 250 - (50 * random.Next(1, 11))), (Obj.WorldPosition.Y + 250 - (50 * random.Next(1, 11))), Obj.WorldPosition.Z, Obj.Heading }; // Snotling
            Obj.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
            prms = new List<object>() { 2000903, (Obj.WorldPosition.X + 250 - (50 * random.Next(1, 11))), (Obj.WorldPosition.Y + 250 - (50 * random.Next(1, 11))), Obj.WorldPosition.Z, Obj.Heading }; // Snotling
            Obj.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
            prms = new List<object>() { 2000903, (Obj.WorldPosition.X + 250 - (50 * random.Next(1, 11))), (Obj.WorldPosition.Y + 250 - (50 * random.Next(1, 11))), Obj.WorldPosition.Z, Obj.Heading }; // Snotling
            Obj.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
            prms = new List<object>() { 2000903, (Obj.WorldPosition.X + 250 - (50 * random.Next(1, 11))), (Obj.WorldPosition.Y + 250 - (50 * random.Next(1, 11))), Obj.WorldPosition.Z, Obj.Heading }; // Snotling
            Obj.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);
        }
    }

    [GeneralScript(false, "", 36598, 0)]
    class ChipfangKizzig : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);

            Obj.PlayEffect(2185);

            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            foreach (Object obj in Obj.ObjectsInRange.ToList())
            {
                Creature creature = obj as Creature;
                if (creature != null && creature.Entry == 36612)
                {
                    //creature.AbtInterface.StartCast(creature, 13155, 1);
                    //BuffInfo b = AbilityMgr.GetBuffInfo(13155, creature, creature); // Rage
                    //creature.BuffInterface.QueueBuff(new BuffQueueInfo(creature, creature.Level, b));

                    var prms = new List<object>() {creature, (ushort)13155, "No, it cannot be... Nau DIE!" }; // Rage
                    creature.EvtInterface.AddEvent(DelayedBuff, 100, 1, prms);
                }
            }
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

    [GeneralScript(false, "", 2000903, 0)]
    class FalseSnotlingKizzig : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);

            Obj.PlayEffect(2185);

            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
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
