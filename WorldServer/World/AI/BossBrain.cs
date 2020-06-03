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
using WorldServer.World.AI.Abilities;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

//test with .spawnmobinstance 2000681
namespace WorldServer.World.AI
{
    public class BossBrain : ABrain
    {

        // Cooldown between special attacks 
        public static int NEXT_ATTACK_COOLDOWN = 2000;
        public Conditions ConditionManager { get; set; }
        public Executions ExecutionManager { get; set; }


        private static new readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BossBrain(Unit myOwner)
            : base(myOwner)
        {
            AbilityTracker = new Dictionary<BossSpawnAbilities, long>();
            SpawnList = new List<Creature>();
            CurrentPhase = 0;
            ConditionManager = new Conditions(_unit, Combat);
            ExecutionManager = new Executions(_unit, Combat, this);
        }

        public List<BossSpawnAbilities> Abilities { get; set; }

        public Dictionary<BossSpawnAbilities, long> AbilityTracker { get; set; }


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
                tick > NextTryCastTime)
            {
                var phaseAbilities = GetPhaseAbilities();

                // Get abilities that can fire now.
                    FilterAbilities(tick, phaseAbilities);

                // Sort dictionary in value (time) order.
                var myList = AbilityTracker.ToList();
                myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                foreach (var keyValuePair in myList)
                    _logger.Debug($"***{keyValuePair.Key.Name} => {keyValuePair.Value}");

                ExecuteNextAbilityFromList(tick, myList);
            }
        }

        private void FilterAbilities(long tick, List<BossSpawnAbilities> phaseAbilities)
        {
            foreach (var ability in phaseAbilities)
            {
                var t = ConditionManager.GetType();
                var method = t.GetMethod(ability.Condition);
                _logger.Debug($"Checking condition: {ability.Condition} ");
                var conditionTrue = (bool)method.Invoke(ConditionManager, null);
                if (conditionTrue)
                {
                    // If the ability is not in the ability tracker, add it
                    if (!AbilityTracker.ContainsKey(ability))
                    {
                        lock (AbilityTracker)
                        {
                            AbilityTracker.Add(ability, TCPManager.GetTimeStamp() + NEXT_ATTACK_COOLDOWN);
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
        }

        private void ExecuteNextAbilityFromList(long tick, List<KeyValuePair<BossSpawnAbilities, long>> myList)
        {
            // This contains the list of abilities that can possibly be executed.
            var rand = StaticRandom.Instance.Next(1, 100);
            lock (myList)
            {
                foreach (var keyValuePair in myList)
                {
                    if (keyValuePair.Value < tick)
                    {
                        if (keyValuePair.Key.ExecuteChance >= rand)
                        {
                            var method = ExecutionManager.GetType().GetMethod(keyValuePair.Key.Execution);

                            _logger.Trace($"Executing  : {keyValuePair.Key.Name} => {keyValuePair.Value} ");

                            PerformSpeech(keyValuePair.Key);
                           
                            PerformSound(keyValuePair.Key);
                            
                            _logger.Debug($"Executing  : {keyValuePair.Key.Name} => {keyValuePair.Value} ");

                            NextTryCastTime = TCPManager.GetTimeStampMS() + NEXT_ATTACK_COOLDOWN;

                            lock (AbilityTracker)
                            {
                                // TODO : See if this is required, or can use ability cool down instead
                                AbilityTracker[keyValuePair.Key] = tick + keyValuePair.Key.CoolDown * 1000;
                            }

                            try
                            {
                                method.Invoke(ExecutionManager, null);
                            }
                            catch (Exception e)
                            {
                                _logger.Error($"{e.Message} {e.StackTrace}");
                                throw;
                            }

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

        public void PerformSound(BossSpawnAbilities key)
        {
            if (!string.IsNullOrEmpty(key.Sound))
                foreach (var plr in GetClosePlayers(300))
                    plr.PlaySound(Convert.ToUInt16(key.Sound));
        }

        public void PerformSpeech(BossSpawnAbilities key)
        {
            if (!string.IsNullOrEmpty(key.Speech))
                _unit.Say(key.Speech, ChatLogFilters.CHATLOGFILTERS_SHOUT);

        }

        public List<BossSpawnAbilities> GetStartCombatAbilities()
        {
            var result = new List<BossSpawnAbilities>();
            foreach (var ability in Abilities)
            {
                if (ability.Phase == "!")
                {
                    result.Add(ability);
                    continue;
                }
            }
            return result;
        }


        private List<BossSpawnAbilities> GetPhaseAbilities()
        {
            var result = new List<BossSpawnAbilities>();
            foreach (var ability in Abilities)
            {
                // Any phase ability
                if (ability.Phase == "*")
                {
                    result.Add(ability);
                    continue;
                }

                // Start up ability
                if (ability.Phase == "!")
                    continue;

                if (Convert.ToInt32(ability.Phase) == CurrentPhase) result.Add(ability);
            }

            return result;
        }


        public void ExecuteStartUpAbilities()
        {
            var abilities = GetStartCombatAbilities();
            foreach (var startUpAbility in abilities)
            {
                _logger.Trace($"Executing Start Up : {startUpAbility.Name} ");
                var method = GetType().GetMethod(startUpAbility.Execution);
                method.Invoke(this, null);

                PerformSpeech(startUpAbility);

                    PerformSound(startUpAbility);
            }
        }


        public override void OnTaunt(Unit taunter, byte lvl)
        {
            if (_unit is Boss)
            {
                if ((_unit as Boss).CanBeTaunted)
                    base.OnTaunt(taunter, lvl);
            }

            
        }
    }
}