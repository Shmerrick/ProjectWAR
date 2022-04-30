using GameData;
using System.Collections.Generic;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.AI
{
    public class GuardBrain : ABrain
    {
        public GuardBrain(Unit myOwner)
            : base(myOwner)
        {
        }
        public override void Think(long tick)
        {
            if (_unit.IsDead)
                return;

            base.Think(tick);
            //we are pet
            if (_pet != null)
            {
                if (_pet.Owner == null)
                {
                    //owner is null?
                    return;
                }
                uint dist = (uint)(_pet.Spawn.Proto.Ranged * _pet.StsInterface.GetStatPercentageModifier(Stats.Range));
                if (dist == 0)
                {
                    dist = 5;
                }
                if (!_pet.IsInCastRange(_pet.Owner, 250))
                {
                    //far away from owner
                    return;
                }
                Unit curTarget = _pet.CbtInterface.GetCurrentTarget();
                //We're already attacking aggressive enemy
                if (curTarget != null && curTarget.AiInterface.GetAttackableUnit() == _pet.Owner)
                {
                    return;
                }

                //Getting creatures in pet range
                List<Creature> creatures = _pet.GetInRange<Creature>(250);
                foreach (Creature creature in creatures)
                {
                    Unit creaTarget = creature.AiInterface.GetAttackableUnit();
                    //If selected creature has targeted our owner
                    if (creaTarget != null && creaTarget == _pet.Owner && _pet.LOSHit(creature))
                    {
                        //Attack it!
                        _pet.AiInterface.ProcessCombatStart(creature);
                    }
                }
            }
        }
    }
}