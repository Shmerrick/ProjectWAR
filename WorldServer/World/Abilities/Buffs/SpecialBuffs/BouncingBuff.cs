using System;
using System.Collections.Generic;
using System.Threading;
using FrameWork;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    public class BouncingBuff : NewBuff
    {
        private const byte MAX_BOUNCES = 5;
        public List<Unit> previousPlayers = new List<Unit>();

        public override void Initialize(Unit caster, Unit target, ushort buffId, byte buffLevel, byte stackLevel, BuffInfo myBuffInfo, BuffInterface parentInterface)
        {
            base.Initialize(caster, target, buffId, buffLevel, stackLevel, myBuffInfo, parentInterface);
            previousPlayers.Add(target);
        }

        public override void StartBuff()
        {
            if (_buffInfo.CommandInfo != null)
            {
                for (byte i = 0; i < _buffInfo.CommandInfo.Count; ++i)
                {
                    if ((_buffInfo.CommandInfo[i].InvokeOn & BuffState) > 0)
                        BuffEffectInvoker.InvokeCommand(this, _buffInfo.CommandInfo[i], Target);
                }
            }

            BuffState = (byte)EBuffState.Running;

            NextTickTime = TCPManager.GetTimeStampMS() + _buffInfo.Interval;

            if (BuffLines.Count != 0)
                SendBounceStart();
            else
                BuffHasExpired = true;
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
                foreach (BuffCommandInfo Command in _buffInfo.CommandInfo)
                    if ((Command.InvokeOn & (byte)EBuffState.Ended) > 0)
                        BuffEffectInvoker.InvokeCommand(this, Command, Target);

            SendBounceEnd();

            if (_linkedBuff != null && !_linkedBuff.BuffHasExpired)
                _linkedBuff.BuffHasExpired = true;
        }

        public void SendBounceStart(Player Plr = null)
        {
            #region Init Effects packet
            // Actual buff
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS);
            Out.WriteByte(1);
            Out.WriteByte(1);
            switch (Entry)
            {
                case 9005: Out.WriteUInt16(0xF4FF); break;
                case 8090: Out.WriteUInt16(0xA5FF); break;
                default:
                    Out.WriteUInt16(0); break;
            }
            Out.WriteUInt16(Target.Oid);
            Out.WriteUInt16(BuffId);
            Out.WriteUInt16R(_buffInfo.Entry);
            Out.WriteZigZag(EndTime != 0 ? (int)((EndTime - TCPManager.GetTimeStampMS()) / 1000) : 0);
            Out.WriteUInt16R(Caster.Oid);


            Out.WriteByte((byte)BuffLines.Count);
            if (BuffLines.Count > 0)
            {
                foreach (Tuple<byte, int> lineInfo in BuffLines)
                {
                    Out.WriteByte(lineInfo.Item1);
                    Out.WriteZigZag(lineInfo.Item2);
                }
                Out.WriteByte(0);
            }

            if (Plr == null)
            {
                if (Target.IsPlayer())
                    ((Player)Target).DispatchPacket(Out, true);
                else if (Caster.IsPlayer())
                    ((Player)Caster).DispatchPacket(Out, true);
            }

            else
                Plr.SendPacket(Out);

            #endregion

            #region Appearance/Animation
            // Buff appearance / animation
            if (Plr == null)
            {
                Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT);
                Out.WriteUInt16(Caster.Oid);
                Out.WriteUInt16(Target.Oid);
                Out.WriteUInt16(_buffInfo.Entry == 3016 ? (ushort)1601 : (ushort)8557); // 00 00 07 D D

                Out.WriteByte((byte)previousPlayers.Count);
                Out.WriteByte(0);
                Out.WriteByte(1);   //7

                Out.WriteByte(0);
                Target.DispatchPacketUnreliable(Out, true, Target);
            }

            #endregion
        }

        protected void SendBounceEnd()
        {
            #region Init Effects packet
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS);
            Out.WriteByte(1);
            Out.WriteByte(2);
            Out.WriteUInt16(0);
            Out.WriteUInt16(Target.Oid);
            Out.WriteUInt16(BuffId);
            Out.WriteUInt16R(_buffInfo.Entry);
            Out.WriteByte(0);

            if (Target.IsPlayer())
                ((Player)Target).DispatchPacket(Out, true);
            else if (Caster.IsPlayer())
                ((Player)Caster).DispatchPacket(Out, true);

            #endregion

            if (HasSentEnd)
                return;

            #region Appearance / Animation

            // Buff appearance / animation
            Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT);
            Out.WriteUInt16(Caster.Oid);
            Out.WriteUInt16(Target.Oid);
            Out.WriteUInt16(_buffInfo.Entry == 3016 ? (ushort)1601 : (ushort)8557); // 00 00 07 D D

            Out.WriteByte((byte)previousPlayers.Count);
            Out.WriteByte(0); // Combat Event not used here
            Out.WriteByte(0); // Start/End

            Out.WriteByte(0); // Packet end

            Target.DispatchPacketUnreliable(Out, true, Target);

            #endregion
        }

        public bool CanBounce()
        {
            return MAX_BOUNCES > previousPlayers.Count;
        }

        public void PassPreviousBounces(NewBuff B)
        {
            if (B == null)
            {
                Log.Error("Bouncing Buff", "Received NULL buff!");
                return;
            }
            BouncingBuff bBuff = B as BouncingBuff;

            if (bBuff == null)
            {
                Log.Error("Bouncing Buff", "Received buff was not a BouncingBuff!");
                return;
            }

            bBuff.previousPlayers.AddRange(previousPlayers);
        }
    }
}
