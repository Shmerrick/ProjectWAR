using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances
{
	public class SimpleAhzranok : InstanceBossSpawn
	{
		#region Constructors

		public SimpleAhzranok(Creature_spawn spawn, uint instancegroupspawnid, uint bossid, ushort Instanceid, Instance instance) : base (spawn, instancegroupspawnid, bossid, Instanceid, instance)
		{

		}

		#endregion Constructors

		#region Attributes

		public static uint TIMER_RAGE_MAX = 1200; // 20 min = 1200 sec

		#endregion Attributes

		#region Overrides
		
		public override void OnLoad()
		{
			base.OnLoad();

			AiInterface.SetBrain(new InstanceBossBrain(this));
		}

		#endregion Overrides

		#region Methods

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
