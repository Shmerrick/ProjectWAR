using System;
using FrameWork;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities
{
    public class NewChannelHandler
    {
        private readonly Unit _host;
        private Unit _target;
        private readonly Player _playerHost;
        private Unit _caster;
        private byte _castSequence;
        private long _channelStartTime;
        private long _nextTickTime;
        private readonly AbilityProcessor _parent;
        private AbilityInfo _channelInfo;
        private NewBuff _channelBuff;
        private readonly bool _checkVisibility;

        private ushort _baseEntry;

        public NewChannelHandler(AbilityProcessor parent, Unit host)
        {
            _parent = parent;
            _host = host;
            _playerHost = host as Player;
            if (_playerHost == null && !(host is Siege))
                _checkVisibility = true;
        }

        #region Interface
        /*
        public void Initialize(Unit caster, Unit target, ushort baseEntry, ushort channelId, byte castSequence)
        {
            _target = target;
            _caster = caster;
            _castSequence = castSequence;
            _baseEntry = baseEntry;
            _channelInfo = AbilityMgr.GetAbilityInfo(channelId);
            if (_channelInfo == null)
                Log.Error("NewChannelHandler", "Couldn't find the channel info for ID: " + channelId);
            else StartChannel();
        }
        */
        public void Initialize(AbilityInfo abInfo, byte castSequence)
        {
            _target = abInfo.Target;
            _caster = abInfo.Instigator;
            _castSequence = castSequence;
            _baseEntry = abInfo.Entry;
            _channelInfo = abInfo;
            StartChannel();
        }

        public bool HasInfo()
        {
            return _channelInfo != null;
        }

        #endregion

        #region Init

        private void StartChannel()
        {
            if (_channelInfo.CastTime == 0)
                throw new InvalidOperationException("A channel's cast time must never be zero.");
            _channelStartTime = TCPManager.GetTimeStampMS();
            _nextTickTime = _channelStartTime + 1000;
            InvokeChannelBuff();
            _host.AbtInterface.SetCooldown(_baseEntry, _channelInfo.Cooldown * 1000);
            if (_playerHost != null && _channelInfo.SpecialCost > 5)
                _playerHost.CrrInterface.ConsumeResource((byte)_channelInfo.SpecialCost, false);
            else if (_channelInfo.SpecialCost < 0)
            {
                Player playerCaster = _host as Player;

                if (playerCaster != null)
                {
                    playerCaster.ResetMorale();

                    PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                    Out.WriteHexStringBytes("000002000000EA6000000000");
                    playerCaster.SendPacket(Out);
                }
            }
        }

        private void SendChannelStart()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);
            Out.WriteUInt16(0);
            Out.WriteUInt16(_baseEntry);
            Out.WriteUInt16(_host.Oid);
            Out.WriteUInt16(_channelInfo.ConstantInfo.EffectID);

            if (_target != null && !(_target is GroundTarget))
                Out.WriteUInt16(_target.Oid);
            else if (_channelInfo.Range == 0)
                Out.WriteUInt16(_host.Oid);
            else Out.WriteUInt16(0);

            Out.WriteByte(3); // channel
            Out.WriteByte(1);

            Out.WriteUInt32(_channelInfo.CastTime);

            Out.WriteByte(_castSequence);
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            _host.DispatchPacket(Out, true);

            Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 200);

            Out.WriteUInt16(_host.Oid);
            Out.WriteUInt16(_host.Oid);
            Out.WriteUInt16(_baseEntry);
            Out.WriteByte((byte)_channelInfo.ConstantInfo.EffectID);
            Out.WriteByte((byte)0); //parried, crit, dodged, etc
            Out.WriteByte((byte)1);
            Out.WriteByte((byte)0);
            _host.DispatchPacket(Out, true);

        }

        #endregion

        #region Tick

        public void Update(long tick)
        {
            // Target is dead.
            if (_target.IsDead)
            {
                SendChannelEnd();
                _channelInfo = null;
                _channelBuff = null;
                _parent.NotifyChannelEnded();
            }
          
            // Cast finishes
            if (_channelInfo != null)
            {
                if (TCPManager.GetTimeStampMS() >= _channelStartTime + _channelInfo.CastTime)
                {
                    SendChannelEnd();
                    _parent.NotifyChannelEnded();
                    _channelInfo = null;
                    _channelBuff = null;
                }

                else if (TCPManager.GetTimeStampMS() >= _nextTickTime)
                {
                    if (_channelInfo.ApCost > 0 && !_host.ConsumeActionPoints(_channelInfo.ApCost))
                        _parent.CancelCast((byte) GameData.AbilityResult.ABILITYRESULT_AP);
                    else if (_playerHost != null && _channelInfo.SpecialCost > 3 && !_playerHost.CrrInterface.ConsumeResource((byte) _channelInfo.SpecialCost, true))
                        _parent.CancelCast(1);
                    else if (_target != _host && !_host.IsInCastRange(_target, Math.Max((uint) 25, _channelInfo.Range)))
                        _parent.CancelCast((byte) GameData.AbilityResult.ABILITYRESULT_OUTOFRANGE);
                    else if (_checkVisibility && !_host.LOSHit(_target))
                        _parent.CancelCast((byte) GameData.AbilityResult.ABILITYRESULT_NOT_VISIBLE);
                    else
                        _nextTickTime += 1000;
                }
            }
        }

        #endregion

        #region Interruption

        public void Interrupt()
        {
            _channelInfo = null;
            if (_channelBuff != null && !_channelBuff.BuffHasExpired)
                _channelBuff.RemoveBuff(true);
            _channelBuff = null;
        }
        #endregion

        #region Casting

        private void InvokeChannelBuff()
        {
            byte desiredLevel = _channelInfo.Level;

            if (_channelInfo.BoostLevel > 0 && _caster != _target)
			{
                desiredLevel = _channelInfo.BoostLevel;

	            #if DEBUG
	            ((Player)_caster).SendClientMessage("Boost debug: Casting with level "+desiredLevel+" on target "+_target.Name);
	            #endif
			}

            BuffInfo buffInfo = AbilityMgr.GetBuffInfo(_channelInfo.Entry, _host, _target);
            if (!string.IsNullOrEmpty(buffInfo.AuraPropagation))
                _target.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, desiredLevel, buffInfo, BuffEffectInvoker.CreateAura, ChannelInitialization));
            else _target.BuffInterface.QueueBuff(new BuffQueueInfo(_caster, desiredLevel, buffInfo, ChannelInitialization));
        }

        /// <summary>
        /// Callback to start the channel proper
        /// </summary>
        public void ChannelInitialization(NewBuff channelBuff)
        {
			// Was cancelled before the channel callback
            if (_channelInfo == null || _host == null || _host.IsDead)
            {
                if (channelBuff != null)
                {
                    channelBuff.BuffHasExpired = true;
                    _channelBuff = null;
                }
            }
			// Couldn't create a channel buff, so cancel
            else if (channelBuff == null)
            {
                _parent.CancelCast(0);
                _channelInfo = null;
            }
			// Successful link
            else
            {
                _channelBuff = channelBuff;
                _channelBuff.ChannelHandler = this;
                // Oil
                if (_host is Siege)
                    _channelBuff.OptionalObject = _host;
                _host.BuffInterface.NotifyAbilityCasted(_channelInfo);
            }
        }

        public void NotifyBuffStarted()
        {
            if (_channelInfo != null)  
                SendChannelStart();
        }

        private void SendChannelEnd()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);
            Out.WriteUInt16(0);
            Out.WriteUInt16(_channelInfo.Entry);
            Out.WriteUInt16(_host.Oid);
            Out.WriteUInt16(_channelInfo.ConstantInfo.EffectID);

            if (_target != null && !(_target is GroundTarget))
                Out.WriteUInt16(_target.Oid);
            else if (_channelInfo?.Range == 0)
                Out.WriteUInt16(_host.Oid);
            else Out.WriteUInt16(0);

            Out.WriteByte(4); // channel end
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.WriteUInt16(_target != null ? (ushort)(_host.GetDistanceToObject(_target, true) * 12) : (ushort)0);
            Out.WriteByte(_castSequence);
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            _host.DispatchPacket(Out, true);

            Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 200);
            Out.WriteUInt16(_host.Oid);
            Out.WriteUInt16(_host.Oid);
            Out.WriteUInt16(_baseEntry);
            Out.WriteByte((byte)_channelInfo.ConstantInfo.EffectID);
            Out.WriteByte((byte)0); //parried, crit, dodged, etc
            Out.WriteByte((byte)0);
            Out.WriteByte((byte)0);
            _host.DispatchPacket(Out, true);
        }
#endregion
    }
}