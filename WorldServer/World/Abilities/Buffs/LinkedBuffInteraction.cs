using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Buffs
{
    /// <summary>
    /// A class which creates two buffs on two different players and links them together.
    /// </summary>
    public class LinkedBuffInteraction
    {
        private Unit _Caster, _Target;
        private NewBuff _targetBuff;
        private ushort _desiredBuff;
        private BuffQueueInfo.BuffCreationDelegate _creator;

        public LinkedBuffInteraction(ushort Entry, Unit Caster, Unit Target)
        {
            _Caster = Caster;
            _Target = Target;
            _desiredBuff = Entry;
        }

        public LinkedBuffInteraction(ushort Entry, Unit Caster, Unit Target, BuffQueueInfo.BuffCreationDelegate Creator)
        {
            _Caster = Caster;
            _Target = Target;
            _desiredBuff = Entry;
            _creator = Creator;
        }

        public void Initialize()
        {
            _Target.BuffInterface.QueueBuff(new BuffQueueInfo(_Caster, _Caster.Level, AbilityMgr.GetBuffInfo(_desiredBuff, _Caster, _Target), _creator, PostTargetCreate));
        }

        public void PostTargetCreate(NewBuff B)
        {
            if (B == null)
                return;
            _targetBuff = B;

            _Caster.BuffInterface.QueueBuff(new BuffQueueInfo(_Caster, _Caster.Level, AbilityMgr.GetBuffInfo(_desiredBuff, _Caster, _Caster), _creator, PostCasterCreate));
        }

        public void PostCasterCreate(NewBuff B)
        {
            //This shouldn't really happen
            if (B == null)
            {
                _targetBuff.BuffHasExpired = true;
                return;
            }

            B.SetLinkedBuff(_targetBuff);
            _targetBuff.SetLinkedBuff(B);
        }
    }
}
