using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    class OYGBuff : NewBuff
    {
        private OYGAuraBuff _masterAuraBuff;
        public OYGAuraBuff MasterAuraBuff { set { _masterAuraBuff = value; } }

        public override void InvokeDamageEvent(byte eventId, AbilityDamageInfo damageInfo, Unit eventInstigator)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;
            if (!string.IsNullOrEmpty(EventCommands[0].Item2.EventCheck) && !BuffEffectInvoker.PerformCheck(this, damageInfo, EventCommands[0].Item2, eventInstigator))
                return;
            if (_masterAuraBuff == null || !_masterAuraBuff.CanHitTarget(eventInstigator))
                return;
            BuffEffectInvoker.InvokeDamageEventCommand(this, EventCommands[0].Item2, damageInfo, Target, eventInstigator);
        }
    }
}
