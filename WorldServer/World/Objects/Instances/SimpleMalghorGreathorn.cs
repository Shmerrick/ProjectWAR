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

        public override bool OnEnterCombat(Object mob, object args)
        {
            bool res = base.OnEnterCombat(mob, args);
            EvtInterface.AddEvent(ChargeRandomNonTankPlayer, 15000, 0);
            return res;
        }

        public override bool OnLeaveCombat(Object mob, object args)
        {
            bool res = base.OnLeaveCombat(mob, args);
            EvtInterface.RemoveEvent(ChargeRandomNonTankPlayer);
            return res;
        }

        #endregion Overrides

        #region Methods

        private void ChargeRandomNonTankPlayer()
		{
            try
            {
                var subset = GetPlayersInRange(300, false).Where(x => x.CrrInterface.GetArchetype() != EArchetype.ARCHETYPE_Tank).ToList();
                if (subset == null || subset.Count == 0)
                    return;
                Random rnd = new Random();
                int idx = (int)Math.Round(rnd.NextDouble() * subset.Count, 0);

                Player plr = subset[idx];
                if (plr != null && !plr.IsDead && !plr.IsInvulnerable)
                {
                    Say(plr.Name + ": Die by the hand of chaos!");
                    MvtInterface.TurnTo(plr);
                    MvtInterface.Follow(plr, 5, 10);

                    NPCAbility ability = null;

                    if (plr.BuffInterface.HasGuard())
                    {
                        //ability = AbtInterface.NPCAbilities.Where(x => x.Entry == 0).FirstOrDefault();
                    }
                    else
                    {
                        //ability = AbtInterface.NPCAbilities.Where(x => x.Entry == 1).FirstOrDefault();
                    }
                    if (ability != null)
                    {
                        //EvtInterface.AddEvent(StartDelayedCast, 1000, 1, prms);
                        //OneshotPercentCast = TCPManager.GetTimeStampMS() + ability.Cooldown * 1000;
                        //ability.AbilityUsed = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                EvtInterface.RemoveEvent(ChargeRandomNonTankPlayer);
                return;
            }
		}

		#endregion Methods
	}
}
