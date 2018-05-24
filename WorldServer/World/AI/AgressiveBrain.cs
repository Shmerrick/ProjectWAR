using System;
using FrameWork;
using GameData;

namespace WorldServer
{
    public class AggressiveBrain : ABrain
    {
        public AggressiveBrain(Unit myOwner)
            : base(myOwner)
        {

        }

        public override void Think()
        {
            base.Think();

            // Only bother to seek targets if we're actually being observed by a player
            if (Combat.CurrentTarget == null && _unit.PlayersInRange.Count > 0)
            {
                if (_pet != null && (_pet.IsHeeling || ((CombatInterface_Pet)_pet.CbtInterface).IgnoreDamageEvents))
                    return;

                Unit target = _unit.AiInterface.GetAttackableUnit();
                if (target != null)
                    _unit.AiInterface.ProcessCombatStart(target);
            }
        }
    }
}
