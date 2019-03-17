using FrameWork;
using GameData;
using System.Linq;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.AI
{
    public class AggressiveBrain : ABrain
    {
        public AggressiveBrain(Unit myOwner)
            : base(myOwner)
        {

        }

        public override void Think(long tick)
        {
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

            if (_unit.IsCreature())
            {
                var target = Combat.GetCurrentTarget();
                if (target == null)
                    return;

                var proto = (_unit as Creature).Spawn.Proto;
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE) &&
                    ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_HUMANS)))
                {
                    
                    var rand = StaticRandom.Instance.Next(14);
                    switch (rand)
                    {
                        case 0:
                            {
                                // Switch targets
                                _logger.Debug($"{_unit} using Changing Targets {(target as Player).Name}");
                                var randomTarget = SetRandomTarget();
                                _logger.Debug($"{_unit} => {(randomTarget as Player).Name}");
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                // Crippling Blow
                                SimpleCast(_unit, target, "Crippling Blow", 5132);
                                break;
                            }
                        case 4:
                            {

                                // Now's Our Chance
                                SimpleCast(_unit, target, "Punish the False!", 8112);
                                break;
                            }
                        case 5:
                            {
                                // Confusing Movements
                                SimpleCast(_unit, target, "Confusing Movements", 631);
                                break;
                            }
                        case 6:
                            {
                                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                                if ((buff == null) &&
                                    (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                                {
                                    SimpleCast(_unit, target, "Smashing Counter", 8018);
                                }

                                break;
                            }
                        case 7:
                        case 8:
                            {
                                // Go for Low Health target
                                var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm)
                                    .ToList();
                                if (enemyPlayers.Count() > 0)
                                {
                                    foreach (var enemyPlayer in enemyPlayers)
                                    {
                                        if (enemyPlayer.PctHealth < 50)
                                        {
                                            _logger.Debug($"{_unit} changing target to  {(enemyPlayer as Player).Name}");
                                            _unit.CbtInterface.SetTarget(enemyPlayer.Oid,
                                                TargetTypes.TARGETTYPES_TARGET_ENEMY);
                                            break;
                                        }
                                    }
                                }

                                break;
                            }
                    }
                }
            }

        }
    }
}
