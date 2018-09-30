using Common;
using System;
using System.Linq;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances
{
	public class SimpleMalghorGreathorn : InstanceBossSpawn
	{
		#region Constructors

		public SimpleMalghorGreathorn(Creature_spawn spawn, uint instancegroupspawnid, uint bossid, ushort Instanceid, Instance instance) : base(spawn, instancegroupspawnid, bossid, Instanceid, instance)
		{
			EvtInterface.AddEvent(ChargeRandomNonTankPlayer, 15000, 0);
		}

		#endregion Constructors

		#region Attributes
		
		#endregion Attributes

		#region Overrides

		public override void OnLoad()
		{
			base.OnLoad();

			AiInterface.SetBrain(new InstanceBossBrain(this));
		}

		#endregion Overrides

		#region Methods

		private void ChargeRandomNonTankPlayer()
		{
			var subset = GetPlayersInRange(300, false).Where(x => x.CrrInterface.GetArchetype() != EArchetype.ARCHETYPE_Tank).ToList();
			if (subset == null || subset.Count == 0)
				return;
			Random rnd = new Random();
			int idx = (int)Math.Round(rnd.NextDouble() * subset.Count, 0);

			Player plr = subset[idx];
			if (plr != null && !plr.IsDead && !plr.IsInvulnerable)
			{
				Say(plr.Name + ": Die!");
				MvtInterface.TurnTo(plr);
				MvtInterface.Follow(plr, 5, 10);

				if (plr.BuffInterface.HasGuard())
				{
					var knockback = AbtInterface.NPCAbilities.Where(x => x.Entry == 0).FirstOrDefault();
					if (knockback != null)
					{

					}
				}
				else
				{
					var oneshot = AbtInterface.NPCAbilities.Where(x => x.Entry == 1).FirstOrDefault();
					if (oneshot != null)
					{

					}
				}
			}
		}

		#endregion Methods
	}
}
