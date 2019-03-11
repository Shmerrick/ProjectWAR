using System.Threading;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using WorldServer.World.Scenarios.Objects;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    public class HoldObjectBuff : NewBuff
    {
        public HoldObject HeldObject { get; set; }

        public byte DefaultLine
        {
            get
            {
                return _defaultLine;
            }

            set
            {
                _defaultLine = value;
            }
        }

        public int DefaultValue
        {
            get
            {
                return _defaultValue;
            }

            set
            {
                _defaultValue = value;
            }
        }

        public FLAG_EFFECTS FlagEffect
        {
            get
            {
                return _flagEffect;
            }

            set
            {
                _flagEffect = value;
            }
        }

        public delegate void OnUpdateDelegate(NewBuff b, HoldObject h, long l);

        public OnUpdateDelegate OnUpdate;
        private byte _defaultLine = 1;
        private int _defaultValue = 1;
        private FLAG_EFFECTS _flagEffect = FLAG_EFFECTS.Mball1;

        public override void StartBuff()
        {
            AddBuffParameter(DefaultLine, DefaultValue);

            base.StartBuff();

            Caster.OSInterface.AddEffect(0xB, (byte)_flagEffect);

            ((Player)Caster).HeldObject = HeldObject;
        }

        public override void Update(long tick)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;

            long curTime = TCPManager.GetTimeStampMS();

            if (EndTime > 0 && curTime >= EndTime)
                BuffEnded(false, false);
            else if (NextTickTime > 0 && curTime >= NextTickTime)
            {
                NextTickTime += _buffInfo.Interval;

                OnUpdate?.Invoke(this, HeldObject, tick);

                if (_buffInfo.CommandInfo != null)
                    foreach (BuffCommandInfo command in _buffInfo.CommandInfo)
                        if ((command.InvokeOn & (byte)EBuffState.Running) > 0)
                            BuffEffectInvoker.InvokeCommand(this, command, Target);
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
            else BuffState = (byte)EBuffState.Ended;

            Interlocked.Exchange(ref BuffEndLock, 0);

            if (_buffInfo.CommandInfo != null)
                foreach (BuffCommandInfo command in _buffInfo.CommandInfo)
                    if ((command.InvokeOn & (byte)EBuffState.Ended) > 0)
                        BuffEffectInvoker.InvokeCommand(this, command, Target);

            if (EventCommands.Count > 0)
                foreach (var evtpair in EventCommands)
                    _buffInterface.RemoveEventSubscription(this, evtpair.Item1);

            BuffHasExpired = true;

            BuffState = (byte)EBuffState.Removed;

            Player player = (Player) Caster;

            if (player.HeldObject != HeldObject)
                Log.Error(player.Name, "Holding multiple objects!");
            else
                player.HeldObject = null;

            HeldObject.HolderDied();

            Caster.OSInterface.RemoveEffect(0xB);

            SendEnded();
        }

        public static NewBuff GetNew()
        {
            return new HoldObjectBuff();
        }
    }


}