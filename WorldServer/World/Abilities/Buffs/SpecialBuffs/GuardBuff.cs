using Common;

namespace WorldServer
{
    public class GuardBuff : NewBuff
    {
        private float _damageSplitFactor = 0.5f;
        private float _hateSplitFactor = 0.35f;

        public override void StartBuff()
        {
            if (Entry == 1674)
                _hateSplitFactor = 0.5f;

            BuffState = (byte)EBuffState.Running;

            if (Caster != Target)
            {
                AddBuffParameter(1, (int)(_damageSplitFactor * 100));
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

            damageInfo.Damage *= _damageSplitFactor;
            damageInfo.Mitigation *= _damageSplitFactor;

            CombatManager.InflictGuardDamage(attacker, (Player)Caster, Entry, damageInfo);

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
