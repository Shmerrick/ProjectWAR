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

		public override void Update(long tick)
		{
			base.Update(tick);

			// check rage timer
			if (BossTimer != null && BossTimer.ElapsedMilliseconds * 1000 >= TIMER_RAGE_MAX)
			{
				// rage timer maximum reached
				// nuke all players
				PlayersInRange.ForEach(plr => plr.Terminate());
			}
		}

		public override void OnLoad()
		{
			base.OnLoad();

			AiInterface.SetBrain(new InstanceBossBrain(this));
		}

		public override void ModifyDamageIn(AbilityDamageInfo damage)
		{
			base.ModifyDamageIn(damage);
		}

		public override void ModifyDamageOut(AbilityDamageInfo outDamage)
		{
			base.ModifyDamageOut(outDamage);
		}

		public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1, uint mitigation = 0)
		{
			return base.ReceiveDamage(caster, damage, hatredScale, mitigation);
		}

		public override bool ReceiveDamage(Unit caster, AbilityDamageInfo damageInfo)
		{
			return base.ReceiveDamage(caster, damageInfo);
		}

		public override bool ShouldDefend(Unit attacker, AbilityDamageInfo incDamage)
		{
			return base.ShouldDefend(attacker, incDamage);
		}

		public override void TryLoot(Player player, InteractMenu menu)
		{
			base.TryLoot(player, menu);
		}

		protected override void HandleDeathRewards(Player killer)
		{
			base.HandleDeathRewards(killer);
		}

		protected override void SetDeath(Unit killer)
		{
			base.SetDeath(killer);
		}

		#endregion Overrides

		#region Methods

		#endregion Methods
	}
}
