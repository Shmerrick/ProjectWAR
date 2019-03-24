using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using Common.Database.World.Creatures;
using FrameWork;
using GameData;
using NLog;
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
        // Melee range for the boss - could use baseradius perhaps?
        public static int BOSS_MELEE_RANGE = 25;

        // Cooldown between special attacks 
        public static int NEXT_ATTACK_COOLDOWN = 2500;


        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BossBrain(Unit myOwner)
            : base(myOwner)
        {
            AbilityTracker = new Dictionary<BossSpawnAbilities, long>();
            SpawnList = new List<Creature>();
            CurrentPhase = 0;
        }

        public List<BossSpawnAbilities> Abilities { get; set; }

        public Dictionary<BossSpawnAbilities, long> AbilityTracker { get; set; }

        // List of Adds that the boss should spawn, if any.
        public List<uint> AddList { get; set; }

        // List of Adds that the boss has spawned, and their states.
        public List<Creature> SpawnList { get; set; }
        public List<BossSpawnPhase> Phases { get; set; }
        public int CurrentPhase { get; set; }

        public override void Think(long tick)
        {
            if (_unit.IsDead)
                return;

            base.Think(tick);

            // Only bother to seek targets if we're actually being observed by a player
            if (Combat.CurrentTarget == null && _unit.PlayersInRange.Count > 0)
            {
                if (_pet != null && (_pet.IsHeeling || ((CombatInterface_Pet) _pet.CbtInterface).IgnoreDamageEvents))
                    return;

                var target = _unit.AiInterface.GetAttackableUnit();
                if (target != null)
                    _unit.AiInterface.ProcessCombatStart(target);
            }

            if (Combat.IsFighting && Combat.CurrentTarget != null &&
                _unit.AbtInterface.CanCastCooldown(0) &&
                TCPManager.GetTimeStampMS() > NextTryCastTime)
            {
                var target = Combat.GetCurrentTarget();

                var phaseAbilities = GetPhaseAbilities(Abilities);

                // Get abilities that can fire now.
                foreach (var ability in phaseAbilities)
                {
                    var t = GetType();
                    var method = t.GetMethod(ability.Condition);
                    _logger.Trace($"Checking condition: {ability.Condition} ");
                    var conditionTrue = (bool) method.Invoke(this, null);
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
                        }
                    }
                }

                // This contains the list of abilities that can possibly be executed.
                var rand = StaticRandom.Instance.Next(1, 100);
                // Sort dictionary in value (time) order.
                var myList = AbilityTracker.ToList();
                myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                foreach (var keyValuePair in myList)
                    _logger.Debug($"***{keyValuePair.Key.Name} => {keyValuePair.Value}");

                lock (myList)
                {
                    foreach (var keyValuePair in myList)
                        if (keyValuePair.Value < tick)
                        {
                            if (keyValuePair.Key.ExecuteChance >= rand)
                            {
                                var t = GetType();
                                var method = t.GetMethod(keyValuePair.Key.Execution);

                                _logger.Trace($"Executing  : {keyValuePair.Key.Name} => {keyValuePair.Value} ");


                                if (!string.IsNullOrEmpty(keyValuePair.Key.Speech))
                                    _unit.Say(keyValuePair.Key.Speech, ChatLogFilters.CHATLOGFILTERS_SHOUT);

                                if (!string.IsNullOrEmpty(keyValuePair.Key.Sound))
                                    foreach (var plr in GetClosePlayers())
                                        plr.PlaySound(Convert.ToUInt16(keyValuePair.Key.Sound));

                                _logger.Trace($"Executing  : {keyValuePair.Key.Name} => {keyValuePair.Value} ");
                                NextTryCastTime = TCPManager.GetTimeStamp() + NEXT_ATTACK_COOLDOWN;
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

                            _logger.Debug($"Skipping : {keyValuePair.Key.Name} => {keyValuePair.Value} (random)");
                        }
                }
            }
        }

        private List<BossSpawnAbilities> GetPhaseAbilities(List<BossSpawnAbilities> abilities)
        {
            var result = new List<BossSpawnAbilities>();
            foreach (var ability in Abilities)
            {
                if (ability.Phase == "*")
                {
                    result.Add(ability);
                    continue;
                }

                if (Convert.ToInt32(ability.Phase) == CurrentPhase) result.Add(ability);
            }

            return result;
        }

        private List<Player> GetClosePlayers()
        {
            return _unit.GetPlayersInRange(300, false);
        }

        public bool PlayersWithinRange()
        {
            if (_unit != null)
            {
                var players = _unit.GetPlayersInRange(30, false);
                if (players == null)
                    return false;
                else
                {
                    return true;
                }
            }

            return false;
        }

        public bool TargetInMeleeRange()
        {
            if (Combat.HasTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY))
            {
                if (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < BOSS_MELEE_RANGE
                ) // In melee range
                    return true;
                return false;
            }

            return false;
        }

        public bool HasBlessing()
        {
            if (Combat.HasTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY))
            {
                if (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < BOSS_MELEE_RANGE
                ) // In melee range
                {
                    var blessing = Combat.CurrentTarget.BuffInterface.HasBuffOfType((byte) BuffTypes.Blessing);
                    return blessing;
                }

                return false;
            }

            return false;
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
            if (_unit.PctHealth <= 20)
                return true;
            return false;
        }

        public bool FourtyNinePercentHealth()
        {
            if (_unit.PctHealth <= 49)
                return true;
            return false;
        }

        public bool SeventyFivePercentHealth()
        {
            if (_unit.PctHealth <= 74)
                return true;
            return false;
        }

        public bool NinetyNinePercentHealth()
        {
            if (_unit.PctHealth <= 99)
                return true;
            return false;
        }

        public void IncrementPhase()
        {
            // Phases must be ints in ascending order.
            var currentPhase = CurrentPhase;
            if (Phases.Count == currentPhase)
                return;
            CurrentPhase = currentPhase + 1;

            SpeakYourMind($" using Increment Phase vs {currentPhase}=>{CurrentPhase}");
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
            var buff = Combat.CurrentTarget.BuffInterface.GetBuff((ushort) GameBuffs.Unstoppable, Combat.CurrentTarget);
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

        public void BestialFlurry()
        {
            if (Combat.CurrentTarget != null)
            {
                SpeakYourMind($" using BestialFlurry vs {(Combat.CurrentTarget as Player).Name}");
                SimpleCast(_unit, Combat.CurrentTarget, "BestialFlurry", 5347);
            }
        }

        public void Whirlwind()
        {
            SpeakYourMind(" using Whirlwind");
            SimpleCast(_unit, Combat.CurrentTarget, "Whirlwind", 5568);
        }

        public void EnfeeblingShout()
        {
            SpeakYourMind(" using Enfeebling Shout");
            SimpleCast(_unit, Combat.CurrentTarget, "Enfeebling Shout", 5575);
        }

        public void SpawnAdds()
        {
            foreach (var protoId in AddList)
            {
                ushort facing = 2093;

                var X = _unit.WorldPosition.X;
                var Y = _unit.WorldPosition.Y;
                var Z = _unit.WorldPosition.Z;


                var spawn = new Creature_spawn {Guid = (uint) CreatureService.GenerateCreatureSpawnGUID()};
                var proto = CreatureService.GetCreatureProto(protoId);
                if (proto == null)
                    return;
                spawn.BuildFromProto(proto);

                spawn.WorldO = facing;
                spawn.WorldX = X + StaticRandom.Instance.Next(500);
                spawn.WorldY = Y + StaticRandom.Instance.Next(500);
                spawn.WorldZ = Z;
                spawn.ZoneId = (ushort) _unit.ZoneId;


                var creature = _unit.Region.CreateCreature(spawn);
                SpawnList.Add(creature);
                creature.AiInterface.SetBrain(new AggressiveBrain(creature));
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