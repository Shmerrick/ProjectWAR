using FrameWork;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios.Objects
{
    public class Bomb: HoldObject
    {
        public Bomb(uint identifier, string name, Point3D homeLoc, ushort buffId, ushort groundResetTime, InteractAction onPickupAction, BallAction onDropAction, BallAction onResetAction, 
            BuffQueueInfo.BuffCallbackDelegate onBuffCallback, ushort groundModelId, ushort homeModelId):base(identifier, name, homeLoc, buffId, groundResetTime, onPickupAction, 
                onDropAction, onResetAction, onBuffCallback, groundModelId, homeModelId)
        {
            ObjectType = 1;
        }

        public override void HoldObjectCallback(NewBuff b)
        {
            if (b == null)
                HolderDied();

            else if (Holder != null)
            {
                MyBuff = (HoldObjectBuff)b;
                MyBuff.DefaultLine = 2;
                MyBuff.DefaultValue = 5;
                MyBuff.FlagEffect = GameData.FLAG_EFFECTS.Bomb;
                MyBuff.HeldObject = this;
                OnBuffCallback?.Invoke(b);
            }
        }

        public override void HolderDied()
        {
            base.HolderDied();

            if (CapturingPlayer != null)
                CapturingPlayer.CanMount = true;
        }

        public override void ResetFromGround()
        {
            base.ResetFromGround();

            if (CapturingPlayer != null)
                CapturingPlayer.CanMount = true;
        }
        public override void NotifyInteractionComplete(NewBuff b)
        {
            if (CapturingPlayer == null || HeldState == EHeldState.Inactive)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);
            DispatchPacket(Out, true);

            SetHeldState(EHeldState.Carried);

            Holder = CapturingPlayer;
            CapturingPlayer = null;

            BuffInfo buffInfo = AbilityMgr.GetBuffInfo(BuffId);

            Holder.BuffInterface.QueueBuff(new BuffQueueInfo(Holder, 1, buffInfo, HoldObjectBuff.GetNew, HoldObjectCallback));

            OnPickupAction?.Invoke(this, Holder);
        }
    }
}
