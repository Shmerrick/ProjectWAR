using Common.Database.World.Creatures;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SystemData;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

//test with .spawnmobinstance 2000681
namespace WorldServer.World.AI
{
    public class BossBrain : ABrain
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public List<BossSpawnAbilities> Abilities { get; set; }
        public Dictionary<BossSpawnAbilities, long> AbilityTracker { get; set; }
        public static int BOSS_MELEE_RANGE = 25;
        public static int NEXT_ATTACK_COOLDOWN = 2500;

        public BossBrain(Unit myOwner)
            : base(myOwner)
        {
            AbilityTracker = new Dictionary<BossSpawnAbilities, long>();
        }

        public override void Think(long tick)
        {
            if (_unit.IsDead)
                return;


            base.Think(tick);

            // Only bother to seek targets if we're actually being observed by a player
            if (Combat.CurrentTarget == null && _unit.PlayersInRange.Count > 0)
            {
                if (_pet != null && (_pet.IsHeeling || ((CombatInterface_Pet)_pet.CbtInterface).IgnoreDamageEvents))
                    return;

                Unit target = _unit.AiInterface.GetAttackableUnit();
                if (target != null)
                    _unit.AiInterface.ProcessCombatStart(target);
            }

            if (Combat.IsFighting && Combat.CurrentTarget != null &&
                _unit.AbtInterface.CanCastCooldown(0) &&
                TCPManager.GetTimeStampMS() > NextTryCastTime)
            {

                var target = Combat.GetCurrentTarget();

                // Get abilities that can fire now.
                foreach (var ability in Abilities)
                {
                    Type t = GetType();
                    MethodInfo method = t.GetMethod(ability.Condition);
                    _logger.Trace($"Checking condition: {ability.Condition} ");
                    bool conditionTrue = (bool)method.Invoke(this, null);
                    if (conditionTrue)
                    {
                        // If the ability is not in the ability tracker, add it
                        if (!AbilityTracker.ContainsKey(ability))
                        {
                            lock (AbilityTracker)
                            {
                                AbilityTracker.Add(ability, 0); // 0 to ensure its possible to execute on check
                            }
                            _logger.Debug($"Adding ability to the tracker : {AbilityTracker.Count} {ability.Name} 0");
                        }
                        else // If this ability is already in the abilitytracker  -- can probably remove this as it should be removed on execution.
                        {
                            long nextInvocation = 0;

                            // If it's next invocation > now, dont add.
                            AbilityTracker.TryGetValue(ability, out nextInvocation);
                            if (nextInvocation > tick)
                            {
                                // Do nothing
                            }
                            else
                            {
                                // If it's next invocation < now, leave it.
                            }
                        }
                    }
                }

                // This contains the list of abilities that can possibly be executed.
                var rand = StaticRandom.Instance.Next(1, 100);
                // Sort dictionary in value (time) order.
                var myList = AbilityTracker.ToList();
                myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                foreach (var keyValuePair in myList)
                {
                    _logger.Debug($"***{keyValuePair.Key.Name} => {keyValuePair.Value}");
                }


                foreach (var keyValuePair in myList)
                {
                    if (keyValuePair.Value < tick)
                    {
                        if (keyValuePair.Key.ExecuteChance >= rand)
                        {
                            Type t = GetType();
                            MethodInfo method = t.GetMethod(keyValuePair.Key.Execution);

                            _logger.Trace($"Executing  : {keyValuePair.Key.Name} => {keyValuePair.Value} ");

                            method.Invoke(this, null);
                            if (!String.IsNullOrEmpty(keyValuePair.Key.Speech))
                            {
                                _unit.Say(keyValuePair.Key.Speech, ChatLogFilters.CHATLOGFILTERS_SHOUT);
                            }
                            if (!String.IsNullOrEmpty(keyValuePair.Key.Sound))
                            {
                                foreach (var plr in GetClosePlayers())
                                {
                                    plr.PlaySound(Convert.ToUInt16(keyValuePair.Key.Sound));
                                }
                            }

                            _logger.Trace($"Executing  : {keyValuePair.Key.Name} => {keyValuePair.Value} ");
                            NextTryCastTime = FrameWork.TCPManager.GetTimeStamp() + NEXT_ATTACK_COOLDOWN;
                            lock (AbilityTracker)
                            {
                                AbilityTracker[keyValuePair.Key] = tick + keyValuePair.Key.CoolDown * 1000;
                            }
                            _logger.Trace($"Updating the tracker : {keyValuePair.Key.Name} => {tick + keyValuePair.Key.CoolDown * 1000} ");
                            _logger.Debug($"CoolDowns : {_unit.AbtInterface.Cooldowns.Count}");
                            break; // Leave the loop, come back on next tick
                        }
                        else
                        {
                            _logger.Debug($"Skipping : {keyValuePair.Key.Name} => {keyValuePair.Value} (random)");
                        }
                    }
                    else
                    {

                       // _logger.Debug($"Skipping : {keyValuePair.Key.Name} => {keyValuePair.Value} (time not valid) {keyValuePair.Value} >= {tick} ({ new DateTime(tick).ToString("HHmmss")})");
                    }
                }


                //var rand = StaticRandom.Instance.Next(20);
                //switch (rand)
                //{
                //    case 0:
                //        {
                //            // Switch targets
                //            _logger.Debug($"{_unit} using Changing Targets {(target as Player).Name}");
                //            var randomTarget = SetRandomTarget();
                //            _logger.Debug($"{_unit} => {(randomTarget as Player).Name}");
                //            break;
                //        }
                //    case 1:
                //    case 2:
                //        {
                //            // 8035 - Shining Blade
                //            SpeakYourMind($"{_unit} using Shining Blade vs {(target as Player).Name}");
                //            _unit.AbtInterface.StartCast(_unit, 8035, 1);
                //            break;
                //        }
                //    case 3:
                //    case 4:
                //        {
                //            // 8036 - Now's Our Chance!
                //            _logger.Debug($"{_unit} using Now's Our Chance! vs {(target as Player).Name}");
                //            _unit.AbtInterface.StartCast(_unit, 8036, 1);
                //            break;
                //        }
                //    case 5:
                //    case 6:
                //    case 7:
                //    case 8:
                //    case 9:
                //        {
                //            // 8005 - Precision Strike
                //            _logger.Debug($"{_unit} using Precision Strike vs {(target as Player).Name}");
                //            _unit.AbtInterface.StartCast(_unit, 8005, 1);
                //            break;
                //        }
                //    case 10:
                //    case 11:
                //        {
                //            var tauntTarget = SetRandomTarget();
                //            // 8010 - Taunt
                //            _logger.Debug($"{_unit} using Taunt vs {(tauntTarget as Player).Name}");
                //            _unit.AbtInterface.StartCast(_unit, 8010, 1);
                //            break;
                //        }
                //    case 12:
                //        {
                //            // 608 - Champion's Challenge
                //            _logger.Debug($"{_unit} using Champion's Challenge vs {(target as Player).Name}");
                //            _unit.AbtInterface.StartCast(_unit, 608, 1);
                //            break;
                //        }
                //    case 13:
                //    case 14:
                //        {
                //            var blessing = target.BuffInterface.HasBuffOfType((byte)BuffTypes.Blessing);
                //            if (blessing && (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                //            {
                //                // 8023 - Shatter Confidence
                //                _logger.Debug($"{_unit} using Shatter Confidence vs {(target as Player).Name}");
                //                _unit.AbtInterface.StartCast(_unit, 8023, 1);

                //            }
                //            break;
                //        }
                //}


            }

        }

        private List<Player> GetClosePlayers()
        {
            return _unit.GetPlayersInRange(300, true);
        }

        public bool TargetInMeleeRange()
        {
            if (Combat.HasTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY))
            {
                if ((_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < BOSS_MELEE_RANGE))  // In melee range
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        public bool HasBlessing()
        {
            if (Combat.HasTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY))
            {
                if ((_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < BOSS_MELEE_RANGE))  // In melee range
                {
                    var blessing = Combat.CurrentTarget.BuffInterface.HasBuffOfType((byte)BuffTypes.Blessing);
                    return blessing;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        public bool CanKnockDownTarget()
        {
            if (!TargetInMeleeRange())
                return false;
            if (TargetIsUnstoppable())
                return false;

            return true;
        }

        public bool CanPuntTarget()
        {
            if (!TargetInMeleeRange())
                return false;
            if (TargetIsUnstoppable())
                return false;

            return true;
        }

        public bool TwentyPercentHealth()
        {
            if (_unit.PctHealth <= 24)
                return true;
            else
            {
                return false;
            }
        }

        public bool FourtyNinePercentHealth()
        {
            if (_unit.PctHealth <= 49)
                return true;
            else
            {
                return false;
            }
        }

        public bool SeventyFivePercentHealth()
        {
            if (_unit.PctHealth <= 74)
                return true;
            else
            {
                return false;
            }
        }

        public bool NinetyNinePercentHealth()
        {
            if (_unit.PctHealth <= 99)
                return true;
            else
            {
                return false;
            }
        }

        public void ShatterBlessing()
        {
            if (Combat.CurrentTarget != null)
            {
                SpeakYourMind($" using Shatter Confidence vs {(Combat.CurrentTarget as Player).Name}");
                SimpleCast(_unit, Combat.CurrentTarget, "Shatter Confidence", 8023);
            }
        }

        public void PrecisionStrike()
        {
            if (Combat.CurrentTarget != null)
            {
                SpeakYourMind($" using PrecisionStrike vs {(Combat.CurrentTarget as Player).Name}");
                SimpleCast(_unit, Combat.CurrentTarget, "PrecisionStrike", 8005);
            }
        }

        public void SeepingWound()
        {
            if (Combat.CurrentTarget != null)
            {
                SpeakYourMind($" using Seeping Wound vs {(Combat.CurrentTarget as Player).Name}");
                SimpleCast(_unit, Combat.CurrentTarget, "Seeping Wound", 8346);
            }
        }

        public bool TargetIsUnstoppable()
        {
            var buff = Combat.CurrentTarget.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, Combat.CurrentTarget);
            if (buff != null)
                SpeakYourMind($" {Combat.CurrentTarget.Name} is unstoppable!");
            return buff != null;
        }

        public void KnockDownTarget()
        {
            SpeakYourMind($" using Downfall vs {(Combat.CurrentTarget as Player).Name}");
            SimpleCast(_unit, Combat.CurrentTarget, "Downfall", 8346);
        }

        public void PuntTarget()
        {

            if (Combat.CurrentTarget != null)
            {
                SpeakYourMind($" using Repel vs {(Combat.CurrentTarget as Player).Name}");
                Combat.CurrentTarget.ApplyKnockback(_unit, AbilityMgr.GetKnockbackInfo(8329, 0));
            }
        }

        public void Corruption()
        {
            if (Combat.CurrentTarget != null)
            {
                SpeakYourMind($" using Corruption vs {(Combat.CurrentTarget as Player).Name}");
                SimpleCast(_unit, Combat.CurrentTarget, "Corruption", 8400);
            }
        }


        public void Stagger()
        {
            if (Combat.CurrentTarget != null)
            {
                SpeakYourMind($" using Quake vs {(Combat.CurrentTarget as Player).Name}");
                SimpleCast(_unit, Combat.CurrentTarget, "Quake", 8349);
            }
        }
        public class BossAbilityTrack
        {
            public string Execution { get; set; }
            public int NextInvocation { get; set; }

        }
    }
}
