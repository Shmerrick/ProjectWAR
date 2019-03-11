using System;
using FrameWork;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    public class CareerInterface_Ironbreaker: CareerInterface
    {
        private const ushort _resourceID = 252, _masterID = 251;
        private Player _oathFriend;
        private long _nextTimer = 0;
        private long _nextOTimer = 0;
        private long Now = 0;
        // I'm given to understand the lock statement is implemented as a hybrid mutex
        private object _resourceLock = new object();

        public CareerInterface_Ironbreaker(Player player) : base(player)
        {
            _maxResource = 100;
            _resourceTimeout = 15000;
    }

        public override bool Load()
        {
            // Queues Grudge Master, used for giving grudge when damaged.
            myPlayer?.BuffInterface.QueueBuff(new BuffQueueInfo(myPlayer, myPlayer.Level, AbilityMgr.GetBuffInfo(_masterID)));
            return true;
        }

        public override Unit GetTargetOfInterest()
        {
            return _oathFriend;
        }

        public void SetOathFriend(Player Plr)
        {
            _oathFriend = Plr;
        }

        public void Notify_OathFriendHit(Unit Attacker)
        {
            if (myPlayer.IsDead)
                return;

            byte cRes = _careerResource;
            if (cRes >= 60)
                AddResource(3, false);
            else if (cRes >= 30)
                AddResource(5, false);
            else AddResource(10, false);

            myPlayer.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.Manual, null, Attacker);
        }

        public override bool AddResource(byte amount, bool blockEvent)
        {
            long Now = TCPManager.GetTimeStampMS();
            if (Now < _nextTimer)
                return true;
            _nextTimer = Now + 2;

            _lastResourceTime = TCPManager.GetTimeStampMS();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte) BuffCombatEvents.ResourceGained, _careerResource, ref amount);

            lock (_resourceLock)
            {
                _lastResource = _careerResource;
                _careerResource = (byte)Math.Min(_maxResource, _careerResource + amount);
            }

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            return true;
        }

        public override bool AddResourceOverride(byte amount, bool blockEvent, bool noTimer)
        {
            Now = TCPManager.GetTimeStampMS();
            if (noTimer == false)
            {
                if (Now < _nextOTimer)
                    return true;

                _nextOTimer = Now + 2;
            }

            _lastResourceTime = TCPManager.GetTimeStampMS();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceGained, _careerResource, ref amount);

            lock (_resourceLock)
            {
                _lastResource = _careerResource;
                _careerResource = (byte)Math.Min(_maxResource, _careerResource + amount);
            }

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            return true;
        }
        
        public override bool ConsumeResource(byte amount, bool blockEvent)
        {
            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte) BuffCombatEvents.ResourceLost, _lastResource, ref amount);

            lock (_resourceLock)
            {
                _lastResource = _careerResource;
                if (_careerResource < amount)
                {
                    _careerResource = 0;
                    return false;
                }
                _lastResource = _careerResource;
                _careerResource = (byte)Math.Max(0, _careerResource - amount);
            }

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            return true;
        }

        public override byte GetCurrentResourceLevel(byte which)
        {
            return (byte)(CareerResource / 25);
        }

        public override byte GetResourceLevelMax(byte which)
        {
            return 4;
        }

        private long _lastTick;
        private int _decayTimer;

        public override void Update(long tick)
        {
            if (_careerResource > 0 && _lastResourceTime != 0 && tick - _lastResourceTime > _resourceTimeout)
            {
                _decayTimer += (int)(tick - _lastTick);

                if (_decayTimer >= 2000)
                {
                    ConsumeResource(20, false);
                    _decayTimer -= 2000;
                }
            }

            else
                _decayTimer = 0;

            if (_lastResource != _careerResource)
            {
                SendResource();
                _lastResource = _careerResource;
            }

            _lastTick = tick;
        }

        public override void SendResource()
        {
            PacketOut Out;

            if (_lastResource != 0)
            {
                Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 12);
                Out.WriteByte(1);
                Out.WriteByte(BUFF_REMOVE); // add
                Out.WriteUInt16(0x7C00);
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte(255); // buffID - some number I pulled out of the air
                Out.WriteByte(0);
                Out.WriteUInt16R(_resourceID); // Balance
                Out.WriteByte(00);

                myPlayer.SendPacket(Out);
            }
            if (_careerResource == 0)
                return; // zero resource means there's no buff left on the client

            Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 18);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_ADD); // add
            Out.WriteUInt16(0);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(255); // buffID - some number I pulled out of the air
            Out.WriteByte(0);
            Out.WriteUInt16R(_resourceID); // Balance
            Out.WriteByte(00); // Duration
            Out.WriteUInt16R(_Owner.Oid);

            Out.WriteByte(0x01);
            Out.WriteByte(00);
            Out.WriteZigZag(_careerResource);

            Out.WriteByte(00);
            myPlayer.SendPacket(Out);
        }

        public override EArchetype GetArchetype()
        {
            return EArchetype.ARCHETYPE_Tank;
        }
    }
}