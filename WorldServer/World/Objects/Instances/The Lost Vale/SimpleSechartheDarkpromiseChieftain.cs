using System;
using System.Collections.Generic;
using Common;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances.The_Lost_Vale
{
    public class SimpleSechartheDarkpromiseChieftain : InstanceBossSpawn
    {
        #region Constructors
        public SimpleSechartheDarkpromiseChieftain(Creature_spawn spawn, uint bossId, ushort Instanceid, Instance instance) : base(spawn, bossId, Instanceid, instance)
        {
            //EvtInterface.AddEvent(CheckBossRageTimer, 1000, 0);
        }

        #endregion Constructors

        #region Attributes

        private List<List<object>> _AddSpawns = new List<List<object>>()
        {
            // spawns
            new List<object> { new List<uint> { }},
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
            //EvtInterface.AddEvent(SpawnAdds, 1000, 0);
            return res;
        }

        public override bool OnLeaveCombat(Object mob, object args)
        {
            bool res = base.OnLeaveCombat(mob, args);
            //EvtInterface.RemoveEvent(SpawnAdds);
            return res;
        }

        #endregion Overrides

        #region Methods

        private void SpawnAdds()
        {
            try
            {
                // first check if boss health is under 20%
                if (Health > MaxHealth * 0.2)
                    return;

                SpawnAdds(_AddSpawns);

                EvtInterface.RemoveEvent(SpawnAdds);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                EvtInterface.RemoveEvent(SpawnAdds);
                return;
            }
        }
        //private void CheckBossRageTimer()
        //{
        //	// check rage timer
        //	if (BossTimer != null && BossTimer.ElapsedMilliseconds / 1000 >= TIMER_RAGE_MAX)
        //	{
        //		// rage timer maximum reached
        //		// nuke all players
        //		GetPlayersInRange(300, false).ForEach(plr => plr.Terminate());
        //	}
        //}

        //private void ApplyIncomingDmgIncreaseOnPlayers()
        //{
        //	// apply incoming dmg increase on players
        //	if (BossTimer != null && (BossTimer.ElapsedMilliseconds / 1000) % 60 == 0)
        //	{
        //		foreach (Player plr in GetPlayersInRange(300, false))
        //		{
        //			//plr.BuffInterface.QueueBuff(new BuffQueueInfo(this, Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Chicken), AssignChickenBuff));
        //		}
        //	}
        //}

        #endregion Methods
    }
}