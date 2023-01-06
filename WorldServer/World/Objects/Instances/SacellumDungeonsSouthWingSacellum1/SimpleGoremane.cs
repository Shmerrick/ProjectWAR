using Common;
using System.Linq;
using WorldServer.Services.World;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances.SacellumDungeonsWestWingSacellum1
{
    public class SimpleGoremane : InstanceBossSpawn
    {
        #region Constructors

        public SimpleGoremane(Creature_spawn spawn, uint bossId, ushort Instanceid, Instance instance) : base(spawn, bossId, Instanceid, instance)
        {
            //EvtInterface.AddEvent(CheckBossRageTimer, 1000, 0);
        }

        #endregion Constructors

        #region Overrides

        public override void OnLoad()
        {
            base.OnLoad();

            var brain = new BossBrain(this);
            brain.Abilities = CreatureService.BossSpawnAbilities.Where(x => x.BossSpawnId == this.BossId).ToList();
            AiInterface.SetBrain(brain);

            //AiInterface.SetBrain(new BossBrain(this));
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

        //private void CheckBossRageTimer()
        //{
        //	// check rage timer
        //	if (BossCombatTimer != null && BossCombatTimer.ElapsedMilliseconds / 1000 >= TIMER_RAGE_MAX)
        //	{
        //		// rage timer maximum reached
        //		// nuke all players
        //		GetPlayersInRange(300, false).ForEach(plr => plr.Terminate());
        //	}
        //}

        //private void ApplyIncomingDmgIncreaseOnPlayers()
        //{
        //	// apply incoming dmg increase on players
        //	if (BossCombatTimer != null && (BossCombatTimer.ElapsedMilliseconds / 1000) % 60 == 0)
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