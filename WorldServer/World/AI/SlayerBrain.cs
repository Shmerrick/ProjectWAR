﻿using FrameWork;
using GameData;
using System.Linq;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

//36172 <-- Ravenclaw Marauder

namespace WorldServer.World.AI
{
    public class SlayerBrain : ABrain
    {
        public bool potionUsed { get; set; }
        public int nextDetauntAvailable { get; set; }

        public SlayerBrain(Unit myOwner)
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

            // Only bother to seek targets if we're actually being observed by a player
            if (Combat.CurrentTarget == null && _unit.PlayersInRange.Count > 0)
            {
                if (_pet != null && (_pet.IsHeeling || ((CombatInterface_Pet)_pet.CbtInterface).IgnoreDamageEvents))
                    return;

                Unit target = _unit.AiInterface.GetAttackableUnit();
                if (target != null)
                    _unit.AiInterface.ProcessCombatStart(target);
            }

            if (Combat.IsFighting && Combat.CurrentTarget != null && _unit.AbtInterface.CanCastCooldown(0) &&
                TCPManager.GetTimeStampMS() > NextTryCastTime)
            {
                var percentHealth = (_unit.Health * 100) / _unit.MaxHealth;
                var target = Combat.GetCurrentTarget();

                if (percentHealth < 20f)
                {
                    // 695 is healing pot model - bit of hack
                    // This needs to be timed if we dont have a proper inventory to work with.
                    var items = CreatureService.GetCreatureItems((_unit as Creature).Entry)
                        .Where(x => x.ModelId == 695);
                    // Low health -- potion of healing
                    if (items.Count() > 0)
                    {
                        // 7872 - Potion of Healing ability
                        if (!potionUsed)
                        {
                            SimpleCast(_unit, _unit, "Potion of Healing", 7872);
                            potionUsed = true;
                        }
                    }
                    else
                    {
                        if (nextDetauntAvailable < FrameWork.TCPManager.GetTimeStamp())
                        {
                            SimpleCast(_unit, target, "Distracting Roar (detaunt)", 1441);
                            nextDetauntAvailable = FrameWork.TCPManager.GetTimeStamp() + 30; // available in another 30 seconds
                        }
                    }
                }

                if (target.CbtInterface.WasDefendedAgainst((int)CombatEvent.COMBATEVENT_PARRY))
                {
                    _logger.Debug($"{target} has parried - enabling partry skills");
                    if ((_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                    {
                        var randParry = StaticRandom.Instance.Next(100);

                        if (randParry >= 50)
                        {
                            SimpleCast(_unit, target, "Numbing Strike", 1444);
                        }
                    }
                }

                var randInterrupt = StaticRandom.Instance.Next(3);

                if (randInterrupt == 0)
                {
                    var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm).ToList();
                    if (enemyPlayers.Count() > 0)
                    {
                        var oldTarget = target;
                        foreach (var enemyPlayer in enemyPlayers)
                        {
                            if ((enemyPlayer.Info.CareerLine == (int)CareerLine.CAREERLINE_BRIGHT_WIZARD) ||
                                (enemyPlayer.Info.CareerLine == (int)CareerLine.CAREERLINE_ARCHMAGE) ||
                                (enemyPlayer.Info.CareerLine == (int)CareerLine.CAREERLINE_RUNE_PRIEST) ||
                                (enemyPlayer.Info.CareerLine == (int)CareerLine.CAREERLINE_WARRIOR_PRIEST))
                            {
                                _unit.CbtInterface.SetTarget(enemyPlayer.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);

                                // 1447 - Wild Swing
                                SimpleCast(_unit, target, "Wild Swing", 1447);
                                _unit.CbtInterface.SetTarget(oldTarget.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                                break;
                            }
                        }
                    }
                }

                var rand = StaticRandom.Instance.Next(20);
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
                        {
                            // 1459 - Rampage
                            SimpleCast(_unit, target, "Rampage", 1459);
                            break;
                        }
                    case 3:
                    case 4:
                        {
                            // 1434 - Deep Wound
                            SimpleCast(_unit, target, "Deep Wound", 1434);
                            break;
                        }
                    case 5:
                    case 6:
                        {
                            // 1438 - Enervating Blow
                            SimpleCast(_unit, target, "Enervating Blow", 1438);
                            break;
                        }
                    case 7:
                    case 8:
                        {
                            // 1431 - Relentless Strike
                            SimpleCast(_unit, target, "Relentless Strike", 1431);
                            break;
                        }
                    case 9:
                        {
                            // 1465 - Inevitable Doom
                            SimpleCast(_unit, target, "Inevitable Doom", 1465);
                            break;
                        }
                    case 10:
                    case 11:
                        {
                            if (((target as Player).Info.CareerLine == (int)CareerLine.CAREERLINE_BRIGHT_WIZARD) ||
                                ((target as Player).Info.CareerLine == (int)CareerLine.CAREERLINE_ARCHMAGE) ||
                                ((target as Player).Info.CareerLine == (int)CareerLine.CAREERLINE_RUNE_PRIEST) ||
                                ((target as Player).Info.CareerLine == (int)CareerLine.CAREERLINE_WARRIOR_PRIEST))
                            {
                                SimpleCast(_unit, target, "Cleft In Twain", 1460);
                            }
                            break;
                        }
                    case 12:
                        {
                            // Confusing Movements
                            SimpleCast(_unit, target, "Confusing Movements", 631);
                            break;
                        }

                    case 13:
                        {
                            // 1463 - No Escape
                            SimpleCast(_unit, target, "No Escape", 1463);
                            break;
                        }
                    case 14:
                        {
                            // Go for Low Health target
                            var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm).ToList();
                            if (enemyPlayers.Count() > 0)
                            {
                                foreach (var enemyPlayer in enemyPlayers)
                                {
                                    if (enemyPlayer.PctHealth < 50)
                                    {
                                        _logger.Debug($"{_unit} changing target to  {(enemyPlayer as Player).Name}");
                                        _unit.CbtInterface.SetTarget(enemyPlayer.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                                        break;
                                    }
                                }
                            }

                            break;
                        }
                    case 15:
                        {
                            // Go for Soft Target target
                            var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm).ToList();
                            if (enemyPlayers.Count() > 0)
                            {
                                foreach (var enemyPlayer in enemyPlayers)
                                {
                                    if (((enemyPlayer).Info.CareerLine == (int)CareerLine.CAREERLINE_BRIGHT_WIZARD) ||
                                        ((enemyPlayer).Info.CareerLine == (int)CareerLine.CAREERLINE_ARCHMAGE) ||
                                        ((enemyPlayer).Info.CareerLine == (int)CareerLine.CAREERLINE_RUNE_PRIEST) ||
                                        ((enemyPlayer).Info.CareerLine == (int)CareerLine.CAREERLINE_WARRIOR_PRIEST))
                                    {
                                        _logger.Debug($"{_unit} changing target to  {(enemyPlayer as Player).Name}");
                                        _unit.CbtInterface.SetTarget(enemyPlayer.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
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