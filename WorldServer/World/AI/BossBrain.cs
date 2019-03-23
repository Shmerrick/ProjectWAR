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
using Common;
using WorldServer.Services.World;
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

                lock (myList)
                {
                    foreach (var keyValuePair in myList)
                    {
                        if (keyValuePair.Value < tick)
                        {
                            if (keyValuePair.Key.ExecuteChance >= rand)
                            {
                                Type t = GetType();
                                MethodInfo method = t.GetMethod(keyValuePair.Key.Execution);

                                _logger.Trace($"Executing  : {keyValuePair.Key.Name} => {keyValuePair.Value} ");


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

                                method.Invoke(this, null);

                                _logger.Trace(
                                    $"Updating the tracker : {keyValuePair.Key.Name} => {tick + keyValuePair.Key.CoolDown * 1000} ");
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
                }
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

        public void SpawnAdds()
        {
            for (int i = 0; i < 5; i++)
            {
                ushort facing = 2093;

                var X = _unit.WorldPosition.X;
                var Y = _unit.WorldPosition.Y;
                var Z = _unit.WorldPosition.Z;


                Creature_spawn spawn = new Creature_spawn {Guid = (uint) CreatureService.GenerateCreatureSpawnGUID()};
                var proto = CreatureService.GetCreatureProto(6986);
                if (proto == null)
                    return;
                spawn.BuildFromProto(proto);

                spawn.WorldO = facing;
                spawn.WorldX = X + StaticRandom.Instance.Next(500);
                spawn.WorldY = Y + StaticRandom.Instance.Next(500);
                spawn.WorldZ = Z;
                spawn.ZoneId = (ushort)_unit.ZoneId;
                spawn.Level = 35;

                var c = _unit.Region.CreateCreature(spawn);
                c.AiInterface.SetBrain(new AggressiveBrain(c));
                
            }
            // Force zones to update
            _unit.Region.Update();
        }
        public class BossAbilityTrack
        {
            public string Execution { get; set; }
            public int NextInvocation { get; set; }

        }
    }
}
