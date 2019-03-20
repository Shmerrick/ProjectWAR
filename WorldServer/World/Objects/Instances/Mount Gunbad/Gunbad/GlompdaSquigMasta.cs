using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 38829, 0)]
    class GlompdaSquigMasta : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            SquigForm(1);

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
            if (c != null)
            {
                c.IsInvulnerable = false;
                Stage = -1;
            }

            c.Ranged = c.Spawn.Proto.Ranged;

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
                if (creature != null && creature.Entry == 2000864)
                    creature.Destroy();

                if (creature != null && creature.Entry == 2000865)
                    creature.Destroy();

                if (creature != null && creature.Entry == 2000866)
                    creature.Destroy();
            }

            addList = new List<Creature>();
            goList = new List<GameObject>();

            c.Ranged = c.Spawn.Proto.Ranged;

            SquigForm(0);

            DismountNPC();

            Obj.PlayEffect(2185); // Mount puff effect

            RemoveStatChange(c);

            return false;
        }

        public override void OnDie(Object Obj)
        {
            Stage = -1;
            stuffInRange.Clear();

            Creature creature = Obj as Creature;
            ChangeModel(creature, 0);

            foreach (Creature c in addList)
                c.Destroy();
            foreach (GameObject go in goList)
                go.Destroy();

            addList = new List<Creature>();
            goList = new List<GameObject>();

            // Gobo form
            creature.Ranged = creature.Spawn.Proto.Ranged;

            SquigForm(0);

            DismountNPC();

            //Obj.PlayEffect(2185); // Mount puff effect
            Obj.PlayEffect(2215); // Squig Explosion effect

            RemoveStatChange(creature);

            CreateExitPortal(spawnWorldX, spawnWorldY, spawnWorldZ, Obj.Heading);
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }

            if (c.Health < c.TotalHealth * 0.3  && Stage < 4 && !c.IsDead) // At 20% HP he fails to summon anything
            {
                ApplyIronSkin();

                DismountNPC();

                SquigForm(1);

                c.Say("I almost 'ad ya!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY); 

                c.PlayEffect(2185); // Mount puff effect

                Stage = 4;
            }
            else if (c.Health < c.TotalHealth * 0.4 && Stage < 3 && !c.IsDead)
            {
                var prms = new List<object>() { 2000866, 929787, 930312, 27020, Obj.Heading }; // Spikestabbin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000866, 930761, 931907, 27026, Obj.Heading }; // Spikestabbin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000866, 929366, 932515, 27062, Obj.Heading }; // Spikestabbin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000866, 928632, 931686, 26987, Obj.Heading }; // Spikestabbin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000866, 928715, 930710, 27000, Obj.Heading }; // Spikestabbin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                ApplyIronSkin();

                MountNPC(c, 136);
                c.EvtInterface.AddEvent(DismountNPC, 30 * 10000, 1);

                c.Say("Spikestabba' Squigs, get out 'ere!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.PlayEffect(1359); // Skull Effect
                c.PlayEffect(2185); // Mount puff effect

                Stage = 3;
            }
            else if (c.Health < c.TotalHealth * 0.6 && Stage < 2 && !c.IsDead)
            {
                var prms = new List<object>() { 2000865, 929787, 930312, 27020, Obj.Heading }; // Stinkspewin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000865, 930761, 931907, 27026, Obj.Heading }; // Stinkspewin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000865, 929366, 932515, 27062, Obj.Heading }; // Stinkspewin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000865, 928632, 931686, 26987, Obj.Heading }; // Stinkspewin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000865, 928715, 930710, 27000, Obj.Heading }; // Stinkspewin'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                ApplyIronSkin();

                MountNPC(c, 136);
                c.EvtInterface.AddEvent(DismountNPC, 30 * 10000, 1);

                c.Say("Stinkspewin' Squigs, get out 'ere!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.PlayEffect(1359); // Skull Effect
                c.PlayEffect(2185); // Mount puff effect

                Stage = 2;
            }
            else if (c.Health < c.TotalHealth * 0.90 && Stage < 1 && !c.IsDead)
            {
                var prms = new List<object>() { 2000864, 929787, 930312, 27020, Obj.Heading }; // Skewering'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000864, 930761, 931907, 27026, Obj.Heading }; // Skewering'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000864, 929366, 932515, 27062, Obj.Heading }; // Skewering'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000864, 928632, 931686, 26987, Obj.Heading }; // Skewering'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                prms = new List<object>() { 2000864, 928715, 930710, 27000, Obj.Heading }; // Skewering'Squigs
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                ApplyIronSkin();
                MountNPC(c, 136); // Squig Mount
                c.EvtInterface.AddEvent(DismountNPC, 30 * 10000, 1);

                c.Say("Skewerin' Squigs, get out 'ere!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.PlayEffect(1359); // Skull Effect
                c.PlayEffect(2185); // Mount puff effect

                Stage = 1;
            }

            return false;
        }

        public void ChangeModel(Creature c, byte enable)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE, 6);
            Out.WriteUInt16(c.Oid);
            Out.WriteByte(1);
            Out.WriteByte(6); // This is probably Squig Armor
            Out.WriteByte(enable);   // active
            Out.WriteByte(0);
            foreach (Player plr in c.PlayersInRange.ToList())
            {
                c.SendMeTo(plr);
                plr.SendPacket(Out);
            }
        }

        public void ApplyStatChange(Creature c)
        {
            c.StsInterface.AddItemBonusStat(GameData.Stats.Armor, 1500);
            //c.StsInterface.AddItemBonusStat(GameData.Stats.MeleeCritRate, 20);
            c.StsInterface.AddItemBonusStat(GameData.Stats.Toughness, 300);
            //c.StsInterface.AddItemBonusStat(GameData.Stats.Strength, 1300);
        }

        public void RemoveStatChange(Creature c)
        {
            c.StsInterface.RemoveItemBonusStat(GameData.Stats.Armor, 1500);
            //c.StsInterface.RemoveItemBonusStat(GameData.Stats.MeleeCritRate, 20);
            c.StsInterface.RemoveItemBonusStat(GameData.Stats.Toughness, 300);
            //c.StsInterface.RemoveItemBonusStat(GameData.Stats.Strength, 1300);
        }

        public void SquigForm(byte enable)
        {
            Creature c = Obj as Creature;
            c.Ranged = 15;

            ChangeModel(c, enable);

            ApplyStatChange(c);

            // Size increase
            if (enable == 1)
                c.OSInterface.AddEffect(1); // State 1 Champ, State 7 Super Champ
            else
                c.OSInterface.RemoveEffect(1); // State 1 Champ, State 7 Super Champ

            AggroInfo aggro = c.AiInterface.CurrentBrain.GetMaxAggroHate();
            
            if (aggro != null)
            { 
                foreach (Player player in c.PlayersInRange)
                {
                    if (c.PlayersInRange.ToList().Count() == 1 || (player != null && aggro.Oid != player.Oid))
                    {
                        c.MvtInterface.StopMove();
                        c.AiInterface.CurrentBrain.Chase(player, true);
                        break;
                    }
                }
            }
        }
    }

    [GeneralScript(false, "", 2000864, 0)]
    class SkeweringGlompSquig : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }

    [GeneralScript(false, "", 2000865, 0)]
    class StinkspewinGlompSquig : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }

    [GeneralScript(false, "", 2000866, 0)]
    class SpikestabbinGlompSquig : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }
}
