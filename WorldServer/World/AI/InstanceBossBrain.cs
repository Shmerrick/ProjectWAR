using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemData;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.AI
{
	public class InstanceBossBrain : ABrain
	{
		#region Constructors

		public InstanceBossBrain(Unit unit) : base(unit)
		{

		}

		#endregion Constructors

		#region Attributes

		#endregion Attributes

		#region Overrides
		
		public override void Think(long tick)
		{
			base.Think(tick);
		}

		public override bool Start(Dictionary<ushort, AggroInfo> aggros)
		{
			return base.Start(aggros);
		}

		public override bool Stop()
		{
			return base.Stop();
		}

		public override void OnTaunt(Unit taunter, byte lvl)
		{
			base.OnTaunt(taunter, lvl);
		}

		public override void Fight()
		{
			base.Fight();
		}

		public override bool StartCombat(Unit fighter)
		{
			return base.StartCombat(fighter);
		}

		public override bool EndCombat()
		{
			return base.EndCombat();
		}
		
		public override Unit GetNextTarget()
		{
			return base.GetNextTarget();
		}

		public override void AddHatred(Unit fighter, bool isPlayer, long hatred)
		{
			base.AddHatred(fighter, isPlayer, hatred);
		}

		public override void AddHealReceive(ushort oid, bool isPlayer, uint count)
		{
			base.AddHealReceive(oid, isPlayer, count);
		}

		public override void TryUseAbilities()
		{
			if (_unit.AbtInterface.NPCAbilities == null)
				return;

			if (Combat.IsFighting && Combat.CurrentTarget != null && _unit.AbtInterface.CanCastCooldown(0) && TCPManager.GetTimeStampMS() > NextTryCastTime)
			{
				long curTimeMs = TCPManager.GetTimeStampMS();

				float rangeFactor = _unit.StsInterface.GetStatPercentageModifier(Stats.Range);

				uint AllowPercentAbilityCycle = 0;

				if (CurTarget != null)
				{
					var prm = new List<object>() { CurTarget };
					_unit.EvtInterface.AddEvent(SetOldTarget, 2000, 1, prm);
					CurTarget = null;
				}

				foreach (NPCAbility ability in _unit.AbtInterface.NPCAbilities)
				{
					// If ability is set to Active = 0 it will not be played
					if (ability.Active == 0)
						continue;

					// If target is below MinRange that is set in creature_abilities we don't want to run this particular skill
					if (ability.MinRange != 0 && _unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < ability.MinRange)
						continue;
					
					if (ability.DisableAtHealthPercent != 0)
					{
						if (_unit.Health + 1 < (_unit.TotalHealth * ability.DisableAtHealthPercent) / 100)
							continue;
					}

					if (ability.ActivateAtHealthPercent != 0)
					{
						// This checks if we can add new ability to ability cycle
						if (ability.AbilityCycle == 1 && _unit.Health < (_unit.TotalHealth * ability.ActivateAtHealthPercent) / 100)
							AllowPercentAbilityCycle = 1;

						// This checks if we can reset the ability if NPC healed - if it's still on cooldwon, we do not refresh it
						if (ability.AbilityCycle == 0 && ability.AbilityUsed == 1 && (_unit.Health > (_unit.TotalHealth * ability.ActivateAtHealthPercent) / 100) && OneshotPercentCast < curTimeMs)
							ability.AbilityUsed = 0;

						// This will play ability after NPC is wounded below X %
						if (ability.AbilityCycle == 0 && ability.AbilityUsed == 0 && (_unit.Health < (_unit.TotalHealth * ability.ActivateAtHealthPercent) / 100) && OneshotPercentCast < curTimeMs)
						{
                            // This set random target if needed
                            if (ability.RandomTarget == 1)
                                SetRandomTarget();

                            // This list of parameters is passed to the function that delays the cast by 1000 ms
                            var prms = new List<object>() { _unit, ability.Entry, ability.RandomTarget };

                            if (ability.Text != "") _unit.Say(ability.Text.Replace("<character name>", _unit.CbtInterface.GetCurrentTarget().Name));
                            _unit.EvtInterface.AddEvent(StartDelayedCast, 1000, 1, prms);
                            OneshotPercentCast = TCPManager.GetTimeStampMS() + ability.Cooldown * 1000;
                            ability.AbilityUsed = 1;
                            continue;
                        }
					}

					if (ability.AutoUse && !_unit.AbtInterface.IsCasting() && ability.CooldownEnd < curTimeMs && _unit.AbtInterface.CanCastCooldown(ability.Entry) && curTimeMs > CombatStart + ability.TimeStart * 1000)
					{
						uint ExtraRange = 0;
						if (Combat != null && Combat.CurrentTarget != null && Combat.CurrentTarget.IsMoving)
							ExtraRange = 5;
						
						if ((ability.Range == 0 || _unit.IsInCastRange(Combat.CurrentTarget, Math.Max(5 + ExtraRange, (uint)(ability.Range * rangeFactor)))))
						{
							if (ability.ActivateAtHealthPercent == 0 || AllowPercentAbilityCycle == 1)
							{
								if (!_unit.LOSHit(Combat.CurrentTarget))
									NextTryCastTime = TCPManager.GetTimeStampMS() + 1000;
								else
                                {
                                    // This set random target if needed
                                    if (ability.RandomTarget == 1)
                                        SetRandomTarget();

                                    // This list of parameters is passed to the function that delays the cast by 1000 ms
                                    var prms = new List<object>() { _unit, ability.Entry, ability.RandomTarget };

									if (ability.Text != "") _unit.Say(ability.Text.Replace("<character name>", _unit.CbtInterface.GetCurrentTarget().Name), ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
									_unit.EvtInterface.AddEvent(StartDelayedCast, 1000, 1, prms);
									
									ability.CooldownEnd = curTimeMs + ability.Cooldown * 1000;
								}

								break;
							}
						}
						else
						{
							Chase(_unit.CbtInterface.GetCurrentTarget(), true, true, ability.Range);
						}
					}

				}
			}
		}

		#endregion Overrides

		#region Methods
        
		#endregion Methods
	}
}
