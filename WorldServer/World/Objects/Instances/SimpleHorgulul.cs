using Common;
using System;
using System.Collections.Generic;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances
{
    public class SimpleHorgulul : InstanceBossSpawn
    {
        #region Constructors

        public SimpleHorgulul(Creature_spawn spawn, uint instancegroupspawnid, uint bossid, ushort Instanceid, Instance instance) : base(spawn, instancegroupspawnid, bossid, Instanceid, instance)
        {
            
        }

        #endregion Constructors

        #region Attributes

        private List<List<object>> _AddSpawns = new List<List<object>>()
        {
            // maggot: 6838
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1426357, 1584952, 6430, 3433 },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1425006, 1585658, 6490, 764 },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1425067, 1587121, 7226, 783  },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1425643, 1585308, 6415, 3317 },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1425351, 1587525, 7472, 3844 },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1424609, 1586294, 6860, 1524 },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1424589, 1587231, 7372, 3734 },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1425851, 1587941, 7850, 2241 },
            new List<object> { new List<uint> { 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838, 6838 }, 1427169, 1588944, 8030, 75 }
        };

        #endregion Attributes

        #region Overrides

        public override void OnLoad()
        {
            base.OnLoad();

            AiInterface.SetBrain(new InstanceBossBrain(this));
        }

        public override bool OnEnterCombat(Object mob, object args)
        {
            bool res = base.OnEnterCombat(mob, args);
            EvtInterface.AddEvent(SpawnAdds, 100, 0);
            return res;
        }

        public override bool OnLeaveCombat(Object mob, object args)
        {
            bool res = base.OnLeaveCombat(mob, args);
            EvtInterface.RemoveEvent(SpawnAdds);
            return res;
        }

        #endregion Overrides

        #region Methods

        private void SpawnAdds()
        {
            try
            {
                // first check if boss health is in following states: 75%, 50%, 25%
                if (!(IsNear(Health, MaxHealth * 0.75, 5e-2) || IsNear(Health, MaxHealth * 0.5, 5e-2) || IsNear(Health, MaxHealth * 0.25, 5e-2)))
                    return;

                SpawnAdds(_AddSpawns);

                if (Health <= MaxHealth * 0.25)
                    EvtInterface.RemoveEvent(SpawnAdds);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                EvtInterface.RemoveEvent(SpawnAdds);
                return;
            }
        }

        private bool IsNear(uint dis, double dat, double eps)
        {
            double value = Math.Abs(dis - dat);
            if (value <= dat * eps)
                return true;
            else
                return false;
        }

        #endregion Methods
    }
}
