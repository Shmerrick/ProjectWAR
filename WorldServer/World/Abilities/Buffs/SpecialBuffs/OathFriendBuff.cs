using System.Threading;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    public class OathFriendBuff : NewBuff
    {
        private CareerInterface_Ironbreaker _masterInterface;

        public override void StartBuff()
        {
            BuffState = (byte)EBuffState.Running;
            if (Caster != Target)
            {
                _masterInterface = ((Player)Caster).CrrInterface as CareerInterface_Ironbreaker;
                _masterInterface?.SetOathFriend((Player)Target);
                AddBuffParameter(2, 1);
            }
            else
                AddBuffParameter(1, 200);

            SendStart(null);

            _buffInterface.AddEventSubscription(this, (byte)BuffCombatEvents.WasAttacked);
        }

        protected override void BuffEnded(bool wasRemoved, bool wasManual)
        {
            if (Interlocked.CompareExchange(ref BuffEndLock, 1, 0) != 0)
                return;

            BuffHasExpired = true;
            WasManuallyRemoved = wasManual;

            if (wasRemoved)
                BuffState = (byte)EBuffState.Removed;
            else BuffState = (byte)EBuffState.Ended;

            Interlocked.Exchange(ref BuffEndLock, 0);

            _buffInterface.RemoveEventSubscription(this, (byte)BuffCombatEvents.WasAttacked);

            SendEnded();

            if (_linkedBuff != null && !_linkedBuff.BuffHasExpired)
                _linkedBuff.BuffHasExpired = true;
        }

        public override void InvokeDamageEvent(byte eventId, AbilityDamageInfo damageInfo, Unit eventInstigator)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;

            if (Caster.Region == Target.Region && Caster.Get2DDistanceToObject(Target) <= 300)
                _masterInterface?.Notify_OathFriendHit(eventInstigator);
        }
    }
}
