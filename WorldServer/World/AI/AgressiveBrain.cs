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

                var rand = StaticRandom.Instance.Next(15);

                var proto = (_unit as Creature).Spawn.Proto;
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_EMPIRE) &&
                    ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_HUMANS)))
                {
                    
                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Crippling Blow", 5132);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Punish the False!", 8112);
                                break;
                            }
                        case 5:
                            {
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
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }
                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_HUMANS_CHAOS) &&
                            ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_HUMANS)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Seeping Wound", 8320);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Thunderous Blow", 8424);
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
                                    SimpleCast(_unit, target, "Downfall", 8346);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_DWARFS_DWARF) &&
                           ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_DWARFS)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Vengeful Strike", 1357);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Heavy Blow", 1354);
                                break;
                            }
                        case 5:
                            {
                                // Confusing Movements
                                SimpleCast(_unit, target, "Skin Of Iron", 1419);
                                break;
                            }
                        case 6:
                            {
                                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                                if ((buff == null) &&
                                    (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                                {
                                    SimpleCast(_unit, target, "Shield Of Reprisal", 1369);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_GREENSKINS_ORC) &&
                           ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_GREENSKINS)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Big Slash", 1680);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Wot Armor", 1666);
                                break;
                            }
                        case 5:
                            {
                                // Confusing Movements
                                SimpleCast(_unit, target, "Tuffa N Nail", 1669);
                                break;
                            }
                        case 6:
                            {
                                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                                if ((buff == null) &&
                                    (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                                {
                                    SimpleCast(_unit, target, "Down Ya Go", 1688);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_ELVES_HIGH_ELF) &&
                           ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_ELVES)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Quick Incision", 9004);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Phantoms Blade", 9020);
                                break;
                            }
                        case 5:
                            {
                                // Confusing Movements
                                SimpleCast(_unit, target, "Raze", 607);
                                break;
                            }
                        case 6:
                            {
                                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                                if ((buff == null) &&
                                    (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                                {
                                    SimpleCast(_unit, target, "Crashing Wave", 9028);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_DARK_ELVES_DARK_ELF) &&
                           ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_DARK_ELVES)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Murderous Wrath", 9320);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Hateful Strike", 9315);
                                break;
                            }
                        case 5:
                            {
                                // Confusing Movements
                                SimpleCast(_unit, target, "Away Cretins", 9381);
                                break;
                            }
                        case 6:
                            {
                                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                                if ((buff == null) &&
                                    (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                                {
                                    SimpleCast(_unit, target, "Spiteful Slam", 9321);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.HUMANOIDS_BEASTMEN_GOR) &&
            ((proto.CreatureType == (int)GameData.CreatureTypes.HUMANOIDS_BEASTMEN)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Bloody Claw", 4606);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Charge", 13307);
                                break;
                            }
                        case 5:
                            {
                                // This 1 needs looking into Basicly want mob to have slow aura
                                SimpleCast(_unit, target, "Broad Swings", 629);
                                break;
                            }
                        case 6:
                            {
                                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                                if ((buff == null) &&
                                    (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                                {
                                    SimpleCast(_unit, target, "Downfall", 8346);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.MONSTERS_CHAOS_BREEDS_CENTIGOR) &&
            ((proto.CreatureType == (int)GameData.CreatureTypes.MONSTERS_CHAOS_BREEDS)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Charge", 13307);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Disabling Strike", 5806);
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
                                    SimpleCast(_unit, target, "Tentacle Knock up", 5467);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.DAEMONS_KHORNE_BLOODLETTER) &&
            ((proto.CreatureType == (int)GameData.CreatureTypes.DAEMONS_KHORNE)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Slice", 9398);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Envenomed Blade", 9403);
                                break;
                            }
                        case 5:
                            {
                                SimpleCast(_unit, target, "Crippling Bellow", 13573);
                                break;
                            }
                        case 6:
                            {
                                var buff = target.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, target);

                                if ((buff == null) &&
                                    (_unit.GetDistanceToObject(_unit.CbtInterface.GetCurrentTarget()) < 5))
                                {
                                    SimpleCast(_unit, target, "Sever Blessing", 8101);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }

                }
                if ((proto.CreatureSubType == (int)GameData.CreatureSubTypes.DAEMONS_UNMARKED_DAEMONS_CHAOS_HOUND) &&
            ((proto.CreatureType == (int)GameData.CreatureTypes.DAEMONS_UNMARKED)))
                {

                    switch (rand)
                    {
                        case 0:
                            {
                                SwitchTarget(target);
                                break;
                            }
                        case 1:
                        case 2:
                        case 3:
                            {
                                SimpleCast(_unit, target, "Feral Bite", 13076);
                                break;
                            }
                        case 4:
                            {
                                SimpleCast(_unit, target, "Slice", 9398);
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
                                    SimpleCast(_unit, target, "Downfall", 8346);
                                }

                                break;
                            }
                        case 7:
                            {
                                SwitchToLowHealthTarget();

                                break;
                            }
                    }
                }
            }
        }

        private void SwitchToLowHealthTarget()
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
        }

        private void SwitchTarget(Unit target)
        {
            // Switch targets
            if (target is Player)
            {
                _logger.Debug($"{_unit} using Changing Targets {(target as Player).Name}");
                var randomTarget = SetRandomTarget();
                if (randomTarget != null)
                    _logger.Debug($"{_unit} => {(randomTarget as Player).Name}");
            }
        }
    }
}
