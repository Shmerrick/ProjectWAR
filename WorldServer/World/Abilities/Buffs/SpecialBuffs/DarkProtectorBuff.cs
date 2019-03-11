using System.Threading;
using FrameWork;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    public class DarkProtectorBuff : NewBuff
    {
        private CareerInterface_Blackguard _masterInterface;

        public override void StartBuff()
        {
            BuffState = (byte)EBuffState.Running;
            if (Caster != Target)
            {
                _masterInterface = ((Player)Caster).CrrInterface as CareerInterface_Blackguard;
                _masterInterface?.SetDarkProtector((Player)Target);
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

        private long _nextResTime;

        public override void InvokeDamageEvent(byte eventId, AbilityDamageInfo damageInfo, Unit eventInstigator)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;

            if (Interlocked.CompareExchange(ref BuffTimerLock, 1, 0) != 0)
                return;

            if (_nextResTime > TCPManager.GetTimeStampMS())
            {
                Interlocked.Exchange(ref BuffTimerLock, 0);
                return;
            }

            _nextResTime = TCPManager.GetTimeStampMS() + 1000;
            Interlocked.Exchange(ref BuffTimerLock, 0);

            if (Caster.Region == Target.Region && Caster.Get2DDistanceToObject(Target) <= 300)
                _masterInterface?.Notify_DarkProtectorHit();
        }
    }
}
