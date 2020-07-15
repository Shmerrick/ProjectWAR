﻿using System.Linq;
using FrameWork;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

//test with .spawnmobinstance 2000681
namespace WorldServer.World.AI
{
    public class IronBreakerBrain : ABrain
    {
        private static new readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public IronBreakerBrain(Unit myOwner)
            : base(myOwner)
        {
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

            var friendlyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm == _unit.Realm).ToList();
            if (friendlyPlayers.Count() > 0)
            {
                lock (friendlyPlayers)
                {
                    var randomFriend = StaticRandom.Instance.Next(friendlyPlayers.Count());
                    LinkedBuffInteraction lbi = new LinkedBuffInteraction((ushort)8325, _unit, friendlyPlayers[randomFriend],
                        BuffEffectInvoker.CreateGuardBuff);
                    lbi.Initialize();
                }
            }

            if (Combat.IsFighting && Combat.CurrentTarget != null && _unit.AbtInterface.CanCastCooldown(0) && TCPManager.GetTimeStampMS() > NextTryCastTime)
            {
                var percentHealth = (_unit.Health * 100) / _unit.MaxHealth;
                var target = Combat.GetCurrentTarget();

                if (percentHealth < 20f)
                {
                    // 695 is healing pot model - bit of hack
                    // This needs to be timed if we dont have a proper inventory to work with.
                    var items = CreatureService.GetCreatureItems((_unit as Creature).Entry).Where(x => x.ModelId == 695);
                    // Low health -- potion of healing
                    if (items.Count() > 0)
                    {
                        // 7872 - Potion of Healing ability
                        _logger.Debug($"{_unit} using Potion of Healing");
                        _unit.AbtInterface.StartCast(_unit, (ushort)7872, 1);

                    }
                }

                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                if ((buff == null) && (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                {

                    //// If the target is guarding someone - punt them
                    //var guardBuff = target.BuffInterface.GetBuff((ushort)8325, target);

                    var randCC = StaticRandom.Instance.Next(100);

                    if (randCC < 80)
                    {
                        // 1369 - Shield of Reprisal
                        _logger.Debug($"{_unit} using Shield of Reprisal vs {(target as Player).Name}");
                        _unit.AbtInterface.StartCast(_unit, 1369, 1);
                    }

                    if (randCC >= 80)
                    {
                        // 1365 - Away With Ye
                        _logger.Debug($"{_unit} using Away With Ye vs {(target as Player).Name}");
                        //                        _unit.AbtInterface.StartCast(_unit, 8356, 1);
                        target.ApplyKnockback(_unit, AbilityMgr.GetKnockbackInfo(1365, 0));

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
                            // 1356 - Guarded Attack
                            _logger.Debug($"{_unit} using Guarded Attack vs {(target as Player).Name}");
                            _unit.AbtInterface.StartCast(_unit, 1356, 1);
                            break;
                        }
                    case 3:
                    case 4:
                        {
                            // 1357 - Vengeful Strike
                            _logger.Debug($"{_unit} using Vengeful Strike vs {(target as Player).Name}");
                            _unit.AbtInterface.StartCast(_unit, 1357, 1);
                            break;
                        }
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        {
                            // 1364 - Inspiring Attack
                            _logger.Debug($"{_unit} using Inspiring Attack vs {(target as Player).Name}");
                            _unit.AbtInterface.StartCast(_unit, 1364, 1);
                            break;
                        }
                    case 10:
                    case 11:
                        {
                            var tauntTarget = SetRandomTarget();
                            // 1360 - Taunt
                            _logger.Debug($"{_unit} using Taunt vs {(tauntTarget as Player).Name}");
                            _unit.AbtInterface.StartCast(_unit, 1360, 1);
                            break;
                        }
                    case 12:
                        {
                            // 608 - Champion's Challenge
                            _logger.Debug($"{_unit} using Champion's Challenge vs {(target as Player).Name}");
                            _unit.AbtInterface.StartCast(_unit, 608, 1);
                            break;
                        }
                    case 13:
                    case 14:
                        {
                            var blessing = target.BuffInterface.HasBuffOfType((byte)BuffTypes.Blessing);
                            if (blessing && (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                            {
                                // 1374 - Sever blessing
                                _logger.Debug($"{_unit} using Sever Blessing vs {(target as Player).Name}");
                                _unit.AbtInterface.StartCast(_unit, 1374, 1);

                            }
                            break;
                        }
                }


            }

        }

    }
}
