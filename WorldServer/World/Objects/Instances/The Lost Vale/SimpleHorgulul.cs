using System;
using System.Collections.Generic;
using Common;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances.The_Lost_Vale
{
    public class SimpleHorgulul : InstanceBossSpawn
    {
        #region Constructors

        public SimpleHorgulul(Creature_spawn spawn, uint bossId, ushort Instanceid, Instance instance) : base(spawn, bossId, Instanceid, instance)
        {
            
        }

        #endregion Constructors

        #region Attributes

        private List<List<object>> _AddSpawns = new List<List<object>>()
        {
            // Painling: 6831
            new List<object> { new List<uint> { 6831, 6831, 6831, 6831, 6831, 6831, 6831, 6831, 6831 }, 1395956, 1570861, 6799, 2957 }
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
            //EvtInterface.AddEvent(SpawnAdds, 1000, 0, 0.75);
            EvtInterface.AddEvent(RemoveAllCCImmunitiesFromPlayers, 1000, 0);
            return res;
        }

        public override bool OnLeaveCombat(Object mob, object args)
        {
            bool res = base.OnLeaveCombat(mob, args);
            //EvtInterface.RemoveEvent(SpawnAdds);
            EvtInterface.RemoveEvent(RemoveAllCCImmunitiesFromPlayers);
            return res;
        }
        
        #endregion Overrides

        #region Methods

        private void SpawnAdds(object threshold)
        {
            try
            {
                // first check if boss health is under 'threshold'
                if (Health > MaxHealth * (double)threshold)
                    return;

                SpawnAdds(_AddSpawns);

                EvtInterface.RemoveEvent(SpawnAdds);

                switch((double)threshold)
                {
                    case 0.75:
                        EvtInterface.AddEvent(SpawnAdds, 1000, 0, 0.5);
                        break;

                    case 0.5:
                        EvtInterface.AddEvent(SpawnAdds, 1000, 0, 0.25);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                EvtInterface.RemoveEvent(SpawnAdds);
                return;
            }
        }

        /// <summary>
        /// 402, 403 = Unstoppable
        /// 408 = Immovable
        /// </summary>
        private void RemoveAllCCImmunitiesFromPlayers()
        {
            foreach (var plr in PlayersInRange)
            {
                plr.BuffInterface.RemoveBuffByEntry(408); // Removing Immunity
                NewBuff newBuff = plr.BuffInterface.GetBuff(408, null);
                if (newBuff != null)
                    newBuff.RemoveBuff(true);
            }
        }

        #endregion Methods
    }
}
