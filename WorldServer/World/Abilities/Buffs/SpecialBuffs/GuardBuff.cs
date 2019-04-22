using WorldServer.World.Abilities.Components;
using WorldServer.World.AI;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    public class GuardBuff : NewBuff
    {
        private float _casterDamageSplitFactor = 0.5f;
        private float _targetDamageSplitFactor = 0.5f;
        private float _hateSplitFactor = 0.35f;

        public override void StartBuff()
        {
            
            if (Caster is Player)
            {
                // Reduce Guard damage by 30% (Stoic)
                if (Caster.BuffInterface.GetBuff(10365, (Unit)Caster) != null)
                {
                    _casterDamageSplitFactor *= 0.7f;
                }
                // Reduce Guard damage by 30% (Solid)
                if (Caster.BuffInterface.GetBuff(10379, (Unit)Caster) != null)
                {
                    _casterDamageSplitFactor *= 0.7f;
                }
            }

            BuffState = (byte)EBuffState.Running;

            if (Caster != Target)
            {
                AddBuffParameter(1, (int)(_casterDamageSplitFactor * 100));
                AddBuffParameter(2, (int)(_hateSplitFactor * -100));
                AddBuffParameter(3, -1);
                AddBuffParameter(4, -1);
                AddBuffParameter(8, -1);
                AddBuffParameter(9, -1);
            }
            else
                AddBuffParameter(7, 1);

            SendStart(null);
        }

        public bool SplitDamage(Unit attacker, AbilityDamageInfo damageInfo)
        {
            if (damageInfo.ExclusiveReductionApplied[0])
                return true;
            if (Caster.IsDead || !Caster.ObjectWithinRadiusFeet(Target, 30))
                return false;

           // (Caster as Player).SendClientMessage($"Caster Split:{_casterDamageSplitFactor} Original Damage:({damageInfo.Damage}) Caster Damage (post Guard):({damageInfo.Damage*_casterDamageSplitFactor})");

            damageInfo.Damage *= _targetDamageSplitFactor;
            damageInfo.Mitigation *= _targetDamageSplitFactor;

          //  (Caster as Player).SendClientMessage($"Target Split:{_targetDamageSplitFactor} Target Damage (post Guard):({damageInfo.Damage})");

            CombatManager.InflictGuardDamage(attacker, (Player)Caster, Entry, damageInfo, _casterDamageSplitFactor);

            return true;
        }

        public bool SplitHate(ABrain monsterBrain, ref uint hateCaused)
        {
            if (Caster.IsDead || !Caster.ObjectWithinRadiusFeet(Target, 30))
                return false;

            uint myHate = (uint)(hateCaused * _hateSplitFactor);
            hateCaused -= myHate;

            monsterBrain.AddHatred(Caster, true, myHate);

            return true;
        }
    }
}
