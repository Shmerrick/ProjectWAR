using GameData;
using System.Threading;
using WorldServer.World.Battlefronts;

namespace WorldServer
{
    /// <summary>
    /// Ration wounds debuff applied to RvR players,
    /// depending on battlegroup 
    /// </summary>
    public class RationBuff : NewBuff
    {
        private IBattlefront _battlefront;
        private float _rationDebuffFactor;
        public float PendingDebuffFactor;

        public override void StartBuff()
        {
            _battlefront = Target.Region?.Bttlfront;

            if (_battlefront == null)
            {
                BuffHasExpired = true;
                return;
            }

            _rationDebuffFactor = _battlefront.GetRationFactor(Target);

            if (_rationDebuffFactor == 1f)
            {
                BuffHasExpired = true;
                return;
            }

            AddBuffParameter(1, -1);

            BuffState = (byte)EBuffState.Running;

            Target.StsInterface.AddReducedMultiplier(Stats.Wounds, 1f/_rationDebuffFactor, BuffClass.Career);

            SendStart(null);
        }

        public override void Update(long tick)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;

            if (PendingDebuffFactor > 0f)
            {
                Target.StsInterface.RemoveReducedMultiplier(Stats.Wounds, 1f / _rationDebuffFactor, BuffClass.Career);

                _rationDebuffFactor = PendingDebuffFactor;
                PendingDebuffFactor = 0f;

                Target.StsInterface.AddReducedMultiplier(Stats.Wounds, 1f / _rationDebuffFactor, BuffClass.Career);
            }
        }

        protected override void BuffEnded(bool wasRemoved, bool wasManual)
        {
            if (Interlocked.CompareExchange(ref BuffEndLock, 1, 0) != 0)
                return;

            BuffHasExpired = true;
            WasManuallyRemoved = wasManual;

            if (wasRemoved)
                BuffState = (byte)EBuffState.Removed;
            else
                BuffState = (byte)EBuffState.Ended;

            Interlocked.Exchange(ref BuffEndLock, 0);

            Target.StsInterface.RemoveReducedMultiplier(Stats.Wounds, 1f / _rationDebuffFactor, BuffClass.Career);

            SendEnded();
        }

        public static NewBuff CreateRationBuff()
        {
            return new RationBuff();
        }
    }
}
