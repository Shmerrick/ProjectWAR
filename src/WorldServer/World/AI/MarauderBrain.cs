using FrameWork;
using GameData;
using System.Linq;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

//36172 <-- Ravenclaw Marauder

namespace WorldServer.World.AI
{
    public class MarauderBrain : ABrain
    {
        public bool potionUsed { get; set; }
        public int nextDetauntAvailable { get; set; }

        public MarauderBrain(Unit myOwner)
            : base(myOwner)
        {
            potionUsed = false;
            nextDetauntAvailable = 0;
        }

        public override void Think(long tick)
        {
            if (_unit.IsDead)
                return;

            base.Think(tick);

            if (Combat.IsFighting)
            {
                PerformCombat();
            }
            else
            {
                PerformPatrolling();
            }
        }

        public void PerformPatrolling()
        {
            if (_unit is Creature creature)
            {
                if (creature.Spawn.Proto.IsWandering == 1 && creature.NextMove <= Core.TickCount)
                {
                    Point2D point = WorldUtils.CalculatePoint(new System.Random(), 150, creature.Spawn.WorldX, creature.Spawn.WorldY);
                    int Z = ZoneService.OcclusionProvider.GetTerrainZ((int)creature.ZoneId, (int)point.X, (int)point.Y);
                    creature.MvtInterface.Move(point.X, point.Y, Z);
                    creature.NextMove = Core.TickCount + new System.Random().Next(2000, 10000);
                }
            }
        }

        public void SelectTarget()
        {
            if (Combat.CurrentTarget == null && _unit.PlayersInRange.Count > 0)
            {
                if (_pet != null && (_pet.IsHeeling || ((CombatInterface_Pet)_pet.CbtInterface).IgnoreDamageEvents))
                    return;

                Unit target = _unit.AiInterface.GetAttackableUnit();
                if (target != null)
                    _unit.AiInterface.ProcessCombatStart(target);
            }

            if (StaticRandom.Instance.Next(20) == 0)
            {
                _logger.Debug($"{_unit} using Changing Targets");
                var randomTarget = SetRandomTarget();
                if (randomTarget != null)
                    _logger.Debug($"{_unit} => {randomTarget.Name}");
                return;
            }

            var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm).ToList();
            if (enemyPlayers.Count > 0)
            {
                var lowHealthTarget = enemyPlayers.FirstOrDefault(e => e.PctHealth < 50);
                if (lowHealthTarget != null)
                {
                    _logger.Debug($"{_unit} changing target to low health target {lowHealthTarget.Name}");
                    _unit.CbtInterface.SetTarget(lowHealthTarget.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                    return;
                }
            }

            if (enemyPlayers.Count > 0)
            {
                var softTarget = enemyPlayers.FirstOrDefault(e =>
                    e.Info.CareerLine == (int)CareerLine.CAREERLINE_BRIGHT_WIZARD ||
                    e.Info.CareerLine == (int)CareerLine.CAREERLINE_ARCHMAGE ||
                    e.Info.CareerLine == (int)CareerLine.CAREERLINE_RUNE_PRIEST ||
                    e.Info.CareerLine == (int)CareerLine.CAREERLINE_WARRIOR_PRIEST);

                if (softTarget != null)
                {
                     _logger.Debug($"{_unit} changing target to soft target {softTarget.Name}");
                    _unit.CbtInterface.SetTarget(softTarget.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                }
            }
        }

        public void PerformHealthCheck()
        {
            var percentHealth = (_unit.Health * 100) / _unit.MaxHealth;
            var target = Combat.GetCurrentTarget();

            if (percentHealth < 20f)
            {
                var items = CreatureService.GetCreatureItems((_unit as Creature).Entry)
                    .Where(x => x.ModelId == 695);

                if (items.Any())
                {
                    if (!potionUsed)
                    {
                        SimpleCast(_unit, _unit, "Potion of Healing", 7872);
                        potionUsed = true;
                    }
                }
                else
                {
                    if (nextDetauntAvailable < TCPManager.GetTimeStamp())
                    {
                        SimpleCast(_unit, target, "Wave of Horror (detaunt)", 8402);
                        nextDetauntAvailable = TCPManager.GetTimeStamp() + 30;
                    }
                }
            }
        }

        public void PerformCombat()
        {
            if (Combat.CurrentTarget == null)
            {
                SelectTarget();
                if (Combat.CurrentTarget == null)
                    return;
            }

            if (!Combat.IsFighting || Combat.CurrentTarget == null || !_unit.AbtInterface.CanCastCooldown(0) ||
                TCPManager.GetTimeStampMS() <= NextTryCastTime) return;

            PerformHealthCheck();

            var target = Combat.GetCurrentTarget();

            if (target.CbtInterface.WasDefendedAgainst((int)CombatEvent.COMBATEVENT_PARRY))
            {
                _logger.Debug($"{target} has parried - enabling parry skills");
                if ((_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                {
                    var randParry = StaticRandom.Instance.Next(100);

                    if (randParry < 50)
                    {
                        SimpleCast(_unit, target, "Gut Ripper", 8414);
                    }
                    else
                    {
                        SimpleCast(_unit, target, "Death Grip", 8405);
                    }
                }
            }

            if (StaticRandom.Instance.Next(3) == 0)
            {
                var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm).ToList();
                if (enemyPlayers.Count > 0)
                {
                    var oldTarget = target;
                    var casterTarget = enemyPlayers.FirstOrDefault(e =>
                        e.Info.CareerLine == (int)CareerLine.CAREERLINE_BRIGHT_WIZARD ||
                        e.Info.CareerLine == (int)CareerLine.CAREERLINE_ARCHMAGE ||
                        e.Info.CareerLine == (int)CareerLine.CAREERLINE_RUNE_PRIEST ||
                        e.Info.CareerLine == (int)CareerLine.CAREERLINE_WARRIOR_PRIEST);

                    if (casterTarget != null)
                    {
                         _unit.CbtInterface.SetTarget(casterTarget.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                         SimpleCast(_unit, casterTarget, "Mouth of Tzeetch", 8397);
                         _unit.CbtInterface.SetTarget(oldTarget.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                    }
                }
            }

            var rand = StaticRandom.Instance.Next(15);
            switch (rand)
            {
                case 0:
                case 1:
                    {
                        SimpleCast(_unit, target, "Thunderous Blow", 8424);
                        break;
                    }
                case 2:
                case 3:
                    {
                        SimpleCast(_unit, target, "Cutting Claw", 8418);
                        break;
                    }
                case 4:
                case 5:
                    {
                        SimpleCast(_unit, target, "Corruption", 8400);
                        break;
                    }
                case 6:
                case 7:
                    {
                        SimpleCast(_unit, target, "Rend", 8395);
                        break;
                    }
                case 8:
                    {
                        SimpleCast(_unit, target, "Tainted Claw", 8401);
                        break;
                    }
                case 9:
                case 10:
                    {
                        if (target is Player player && (player.Info.CareerLine == (int)CareerLine.CAREERLINE_BRIGHT_WIZARD ||
                            player.Info.CareerLine == (int)CareerLine.CAREERLINE_ARCHMAGE ||
                            player.Info.CareerLine == (int)CareerLine.CAREERLINE_RUNE_PRIEST ||
                            player.Info.CareerLine == (int)CareerLine.CAREERLINE_WARRIOR_PRIEST))
                        {
                            SimpleCast(_unit, target, "Touch of Instability", 8407);
                        }
                        break;
                    }
                case 11:
                    {
                        SimpleCast(_unit, target, "Confusing Movements", 631);
                        break;
                    }

                case 12:
                    {
                        SimpleCast(_unit, target, "Debilitate", 8396);
                        break;
                    }
                case 13:
                    SelectTarget();
                    break;
            }
        }
    }
}