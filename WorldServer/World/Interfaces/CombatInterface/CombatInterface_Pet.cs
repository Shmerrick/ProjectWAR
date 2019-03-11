using FrameWork;
using GameData;
using WorldServer.World.Objects;

namespace WorldServer.World.Interfaces
{
    public class CombatInterface_Pet : CombatInterface_Npc
    {
        private Pet _petOwner;

        public override bool Load()
        {
            _petOwner = (Pet) _Owner;
            CreditedPlayer = _petOwner.Owner;
            return base.Load();
        }

        public override void TryAttack()
        {
            if (!_petOwner.CanAutoAttack || CurrentTarget.IsStaggered)
                return;

            long tick = TCPManager.GetTimeStampMS();
            if (NextAttackTime >= tick)
                return;

            if (_petOwner.IsStationary)
                _petOwner.MvtInterface.TurnTo(CurrentTarget.WorldPosition);

            if (_petOwner.Spawn.Proto.Ranged > 0)
            {
                if (_petOwner.AbtInterface.IsCasting() || !_petOwner.IsInCastRange(CurrentTarget, (uint) (_petOwner.Spawn.Proto.Ranged*_petOwner.StsInterface.GetStatPercentageModifier(Stats.Range))))
                {
                    if (_petOwner.AiInterface.Debugger != null)
                        _petOwner.AiInterface.Debugger.SendClientMessage("[MR] Unable to auto attack target, casting ability or too far away.");
                    NextAttackTime = tick + 200;
                    return;
                }
                if (_petOwner.LOSHit(CurrentTarget))
                {
                    if (_petOwner.Owner.Info.CareerLine == (byte)CareerLine.CAREERLINE_MAGUS)
                        _petOwner.Strike(CurrentTarget, EquipSlot.NONE);
                    else
                        _petOwner.Strike(CurrentTarget, EquipSlot.RANGED_WEAPON);
                }
                else if (_petOwner.AiInterface.Debugger != null)
                    _petOwner.AiInterface.Debugger.SendClientMessage("[MR] Unable to auto attack target due to lack of LOS.");
                NextAttackTime = tick + 2000;
            }
            else
            {
                if (!_petOwner.IsInCastRange(CurrentTarget, 5))
                    return;
                _petOwner.Strike(CurrentTarget);
                NextAttackTime = tick + 2000;
            }
        }

        #region Events

        public bool IgnoreDamageEvents;

        public override void OnAttacked(Unit attacker)
        {
            if (_petOwner.AIMode != (byte)PetCommand.Passive && !_petOwner.IsHeeling && !IgnoreDamageEvents && CurrentTarget == null)
            {
                switch (AIInterface.State)
                {
                    case AiState.STANDING:
                        AIInterface.ProcessCombatStart(attacker);
                        break;
                    case AiState.MOVING:
                        AIInterface.ProcessCombatStart(attacker);
                        break;
                }
            }

            RefreshCombatTimer();

            _petOwner.Owner.CbtInterface.RefreshCombatTimer();
        }

        public override void OnTakeDamage(Unit fighter, uint damage, float hatredMod, uint mitigation = 0)
        {
            if (_petOwner.AIMode != (byte) PetCommand.Passive && !_petOwner.IsHeeling && !IgnoreDamageEvents && CurrentTarget == null)
            {
                switch (AIInterface.State)
                {
                    case AiState.STANDING:
                        AIInterface.ProcessCombatStart(fighter);
                        break;
                    case AiState.MOVING:
                        AIInterface.ProcessCombatStart(fighter);
                        break;
                }

                _Owner.EvtInterface.Notify(EventName.OnReceiveDamage, fighter, null);
            }

            RefreshCombatTimer();

            _petOwner.Owner.CbtInterface.RefreshCombatTimer();
        }

        public override void OnDealDamage(Unit victim, uint damageCount)
        {
            if (_petOwner.AIMode != (byte) PetCommand.Passive && !_petOwner.IsHeeling && !IgnoreDamageEvents && CurrentTarget == null)
            {
                switch (AIInterface.State)
                {
                    case AiState.STANDING:
                        AIInterface.ProcessCombatStart(victim);
                        break;
                    case AiState.MOVING:
                        AIInterface.ProcessCombatStart(victim);
                        break;
                }

                _Owner.EvtInterface.Notify(EventName.OnDealDamage, victim, damageCount);
            }

            RefreshCombatTimer();

            _petOwner.Owner.CbtInterface.RefreshCombatTimer();
        }

        public override void OnDealHeal(Unit target, uint damageCount)
        {
        }

        public override void OnTakeHeal(Unit caster)
        {
            _Owner.EvtInterface.Notify(EventName.OnReceiveHeal, caster, null);
        }

        public override void OnTargetDie(Unit victim)
        {
        }

        #endregion

        public override bool CanAttackPlayer(Player plr)
        {
            if (!plr.CbtInterface.IsPvp)
                return false;
            
            if (_petOwner.Owner == null)
                return false;
            
            return _petOwner.Owner.CbtInterface.IsPvp;
        }
    }
}