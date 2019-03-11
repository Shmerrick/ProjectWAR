using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.Buffs
{
    public class NewBuff
    {
        #region Definitions

        #region BuffInfo Access
        protected BuffInfo _buffInfo;

        public ushort Entry => _buffInfo.Entry;

        public string BuffName => _buffInfo.Name;

        public bool WasCastFromAoE => _buffInfo.IsAoE;

        public bool IsGroupBuff
        {
            get { return _buffInfo.IsGroupBuff; }
            set { _buffInfo.IsGroupBuff = value; }
        }

        public BuffGroups BuffGroup => _buffInfo.Group;

        public bool RequiresTargetAlive => _buffInfo.PersistsOnDeath == 0;
        public bool AlwaysOn => (_buffInfo.PersistsOnDeath & 1) > 0;
        public bool RequiresTargetDead => _buffInfo.PersistsOnDeath == 2;
        public bool PersistsOnLogout => (_buffInfo.PersistsOnDeath & 4) > 0;

        public byte MaxStack => _buffInfo.MaxStack;
        public uint Duration => _buffInfo.Duration;
        public BuffTypes Type { get { return _buffInfo.Type; } set { _buffInfo.Type = value; } }
        public ushort Interval => _buffInfo.Interval;
        public BuffClass BuffClass => _buffInfo.BuffClass;
        /// <summary>
        /// Gets the buff class considering potential buff command override.
        /// </summary>
        /// <param name="command">Command to apply.</param>
        /// <returns>Buff class</returns>
        public BuffClass GetBuffClass(BuffCommandInfo command)
        {
            BuffClass clazz = command.BuffClass;
            if (clazz != BuffClass.Standard)
                return clazz;
            return _buffInfo.BuffClass;
        }

        public byte BuffIntervals => _buffInfo.BuffIntervals;

        #endregion

        protected NewBuff _linkedBuff; // used for buff pairs like guard and oath friend

        protected BuffInterface _buffInterface;

        public Unit Caster { get; protected set; }
        public Unit Target { get; protected set; }

        public ushort RemainingTimeMs 
        { 
            get 
            { 
                if (TCPManager.GetTimeStampMS() > EndTime) 
                    return 0; 
                return (ushort)(EndTime - TCPManager.GetTimeStampMS()); 
            } 
        }

        public byte StackLevel { get; protected set; }

        public ushort BuffId { get; protected set; }

        public byte BuffLevel { get; protected set; }

        /// <summary>
        /// Indicates the present state of a buff (starting, running, ending or being removed.)
        /// </summary>
        public byte BuffState { get; protected set; } = 1;

        /// <summary>
        /// The duration of this effect, in milliseconds.
        /// </summary>
        protected uint DurationMs;

        /// <summary>
        /// Indicates that this effect has expired and is pending removal.
        /// </summary>
        public bool BuffHasExpired { get; set; }

        /// <summary>
        ///  Type of crowd control represented by this buff
        /// </summary>
        public byte CrowdControl { get; set; }

        /// <summary>
        /// Can be set to false to cause a buff to be effectively hidden from view, while still remaining applied. Used by Diminishing Rations.
        /// </summary>
        protected bool IsEnabled = true;

        // Storage for scaling effect of AoE damage on buffs.
        public float AoEMod { get; set; }

        // Event list
        protected List<Tuple<byte, BuffCommandInfo>> EventCommands = new List<Tuple<byte, BuffCommandInfo>>();

        // Buff text lines on client
        protected List<Tuple<byte, int>> BuffLines = new List<Tuple<byte, int>>();

        private bool _noFX;

        // Used for command storage, since they're delegates
        public object OptionalObject;

        #endregion

        public void SetLinkedBuff(NewBuff linkedBuff)
        {
            _linkedBuff = linkedBuff;
        }

        /// <summary>
        /// Sends buff start/end packets only to the buff's target, any group mates and anyone who has the buff target targeted.
        /// </summary>
        private void DispatchBuffPacket(PacketOut Out)
        {
            (Target as Player)?.SendPacket(Out);

            if (!Target.IsActive)
                return;

            lock (Target.PlayersInRange)
            {
                foreach (Player player in Target.PlayersInRange)
                {
                    if (player.Realm == Target.Realm)
                    {
                        if (player.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY) == Target || (player.PriorityGroup != null && player.PriorityGroup.HasMember(Target)))
                            player.SendPacket(Out);
                    }
                    else if (player.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY) == Target)
                        player.SendPacket(Out);
                }
            }
        }

        protected void SetEnabled(bool newValue)
        {
            if (IsEnabled == newValue)
                return;

            if (newValue)
            {
                IsEnabled = true;
                SendStart(null);
            }
            else
            {
                if (BuffState == (byte)EBuffState.Running)
                    SendEnded();
                IsEnabled = false;
            }
        }

        #region Init

        public NewChannelHandler ChannelHandler { get; set; }

        public virtual void Initialize(Unit caster, Unit target, ushort buffId, byte buffLevel, byte stackLevel, BuffInfo myBuffInfo, BuffInterface parentInterface)
        {
            Caster = caster;
            Target = target;
            _buffInfo = myBuffInfo;
            StackLevel = stackLevel;
            BuffLevel = buffLevel;
            BuffId = buffId;
            _buffInterface = parentInterface;

            uint leadMs = 0;

            // Fixed lead-in (Oil channel)
            if (_buffInfo.LeadInDelay < 0)
                leadMs = (uint)-_buffInfo.LeadInDelay;
            // Variable lead-in with range (Rapid Fire channel)
            else if (_buffInfo.LeadInDelay > 0)
                leadMs = (uint) (_buffInfo.LeadInDelay*Caster.GetDistanceTo(Target)*0.01f);

            NextTickTime = TCPManager.GetTimeStampMS() + _buffInfo.Interval + leadMs;

            if (Duration > 0)
            {
                DurationMs = (uint) (_buffInfo.Duration*1000 + leadMs);
                EndTime = TCPManager.GetTimeStampMS() + DurationMs;
            }
        }

        public virtual void StartBuff()
        {
            if (_buffInfo.StackLine != 0)
                AddBuffParameter(_buffInfo.StackLine, StackLevel);

            // Invoke commands and register event subscriptions.
            if (_buffInfo.CommandInfo != null)
            {
                for (byte i = 0; i < _buffInfo.CommandInfo.Count; ++i)
                {
                    BuffCommandInfo command = _buffInfo.CommandInfo[i];

                    if (command.EventID != 0)
                    {
                        EventCommands.Add(new Tuple<byte, BuffCommandInfo>(command.EventID, command));
                        _buffInterface.AddEventSubscription(this, command.EventID);
                        //InvokeOn override - 8 == Invoke as permanent conditional effect while buff is active (for Channel)
                        if (command.InvokeOn == 8 && command.TargetType == CommandTargetTypes.Caster)
                        {
                            Caster.BuffInterface.AddEventSubscription(this, command.EventID);
                        }

                        if (command.InvokeOn == 0 && command.BuffLine > 0)
                            AddBuffParameter(command.BuffLine, command.PrimaryValue);
                    }
                    if ((command.InvokeOn & BuffState) > 0)
                        BuffEffectInvoker.InvokeCommand(this, command, Target);
                }
            }

            BuffState = (byte)EBuffState.Running;

            #region Check for CC block or no tooltip text
            // If a buff is crowd control and no tooltip text was added, a CC immunity blocked it.
            // In this case the buff is removed here.
            if (BuffLines.Count != 0)
            {
                if (Duration > 0)
                {
                    if (CrowdControl == 1)
                    {
                        DurationMs = (uint) (DurationMs*Target.StsInterface.GetStatReductionModifier(Stats.SnareDuration));
                        EndTime = TCPManager.GetTimeStampMS() + DurationMs;
                    }
                    if (CrowdControl == 16)
                    {
                        DurationMs = (uint) (DurationMs*Target.StsInterface.GetStatReductionModifier(Stats.KnockdownDuration));
                        EndTime = TCPManager.GetTimeStampMS() + DurationMs;
                    }
                }

                SendStart(null);

                ChannelHandler?.NotifyBuffStarted();
            }
            else
            {
                #if DEBUG
                Log.Info("Buff " + _buffInfo.Entry, "Couldn't find any buff lines.");
                if (CrowdControl == 0 && Caster is Player)
                    ((Player)Caster).SendClientMessage(Entry + " "+ AbilityMgr.GetAbilityNameFor(Entry)+": Couldn't find any buff lines.");
                #endif
                BuffHasExpired = true;
            }
            #endregion
        }

        public void SendStart(Player player, bool forceFX = false, bool suppressFx = false)
        {
            if (!IsEnabled || Entry == 8289) // suppress Cleansing Power
                return;

            PacketOut Out = BuildBuffStartPacket();

            if (player == null)
                DispatchBuffPacket(Out);
            else
                player.SendPacket(Out);

            if (_buffInfo.EffectType == 0 || suppressFx)
                return;

            #region Appearance/Animation

            bool forceSend = false;

            // Buff appearance / animation
            if (player == null || forceFX)
            {
                Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(Caster.Oid);
                if (_buffInfo.Group == BuffGroups.DefensePotion || _buffInfo.Group == BuffGroups.Caltrops)
                {
                    Out.WriteUInt16(Caster.Oid);
                }
                else
                {
                    Out.WriteUInt16(Target.Oid);
                }
                Out.WriteUInt16(_buffInfo.Entry); // 00 00 07 D D

                if (Target is BuffHostObject || Target is GroundTarget)
                {
                    forceSend = true;

                    if (Entry == 8177 || _buffInfo.Group == BuffGroups.Caltrops)
                        Out.WriteByte(1);
                    else
                        Out.WriteByte(0);
                }
                else
                    Out.WriteByte(Target.Realm == Caster.Realm ? _buffInfo.FriendlyEffectID : _buffInfo.EnemyEffectID);
                Out.WriteByte(0);
                Out.WriteByte(_buffInfo.EffectType);   //7
                Out.WriteByte(0);

				if (forceFX)
					player.SendPacket(Out);
				else
				{
	                if (Target.PlayersInRange.Count > 100 && !forceSend)
	                {
	                    _noFX = true;
	                    (Target as Player)?.SendPacket(Out);
	                    (Caster as Player)?.SendPacket(Out);
	                }
	                else
	                    Target.DispatchPacket(Out, true);
				}
            }

            #endregion
        }

        private PacketOut BuildBuffStartPacket()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 30);
            Out.WriteByte(1);
            Out.WriteByte(1);
            switch (Entry)
            {
                case 9005: Out.WriteUInt16(0xF4FF); break;
                case 8090: Out.WriteUInt16(0xA5FF); break;
                case 8551:
                case 8556:
                case 8560:
                    Out.WriteUInt16(0x1D10); break;
                default:
                    Out.WriteUInt16(0); break;
            }
            Out.WriteUInt16(Target.Oid);
            Out.WriteUInt16(BuffId);
            Out.WriteUInt16R(_buffInfo.Entry == 3320 ? (ushort)20619 : _buffInfo.Entry); // override for Inevitable Doom
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

            return Out;
        }

        public void UpdateDuration(int newDuration)
        {
            _buffInfo.Duration = (uint)newDuration;
            DurationMs = (uint)(_buffInfo.Duration * 1000);
        }

        public void SoftRefresh()
        {
            NextTickTime = TCPManager.GetTimeStampMS() + Interval;

            if (EndTime > 0)
            {
                EndTime = TCPManager.GetTimeStampMS() + DurationMs;
                Resend();
            }
        }

        public void Resend()
        {
            if (!IsEnabled)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 30);
            Out.WriteByte(1); // effect count
            Out.WriteByte(1); // update operation
            Out.WriteUInt16(0);
            Out.WriteUInt16(Target.Oid);
            Out.WriteUInt16(BuffId);
            Out.WriteUInt16R(_buffInfo.Entry);
            Out.WriteZigZag(EndTime != 0 ? (int)((EndTime - TCPManager.GetTimeStampMS()) / 1000) : 0);
            Out.WriteUInt16R(Caster.Oid);

            Out.WriteByte((byte)BuffLines.Count);
            foreach (Tuple<byte, int> lineInfo in BuffLines)
            {
                Out.WriteByte(lineInfo.Item1);
                Out.WriteZigZag(lineInfo.Item2);
            }

            Out.WriteByte(0);

            DispatchBuffPacket(Out);
        }

        // Ordering is apparently important here.
        public void AddBuffParameter(byte line, int value)
        {
            if (line == 0)
                return;
            --line;

            for (int i = 0; i < BuffLines.Count; ++i)
            {
                if (line == BuffLines[i].Item1)
                    return;

                if (line < BuffLines[i].Item1)
                {
                    BuffLines.Insert(i, new Tuple<byte, int>(line, value));
                    return;
                }
            }

            BuffLines.Add(new Tuple<byte, int>(line, value));
        }

        public void DeleteBuffParameter(byte line)
        {
            if (line == 0)
                return;
            --line;
            BuffLines.RemoveAll(x => x.Item1 == line);
        }

        #endregion

        #region Tick

        protected long NextTickTime;
        protected long EndTime;

        public virtual void Update (long tick)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;

            long curTime = TCPManager.GetTimeStampMS();

            if (EndTime > 0 && curTime >= EndTime)
                 BuffEnded(false, false);
            else if (NextTickTime > 0 && curTime >= NextTickTime)
            {
                NextTickTime += _buffInfo.Interval;

                if(_buffInfo.CommandInfo != null)
                    foreach (BuffCommandInfo command in _buffInfo.CommandInfo)
                        if ((command.InvokeOn & (byte)EBuffState.Running) > 0)
                            BuffEffectInvoker.InvokeCommand(this, command, Target);
            }                
        }

        #endregion

        #region End/Remove

        public bool HasSentEnd = false;
        public bool WasManuallyRemoved { get; set; }
        public long LastPassTime { get; set; }

        protected int BuffEndLock;

        public void RemoveStack()
        {
            if (StackLevel == 0)
                return;
            if (StackLevel == 1)
            {
                StackLevel = 0;
                BuffHasExpired = true;
            }
            else
            {
                --StackLevel;
                DeleteBuffParameter(_buffInfo.StackLine);
                AddBuffParameter(_buffInfo.StackLine, StackLevel);
                SendStart(null, false, true);
            }
        }

        public void RemoveStacks(int numStacks)
        {
            if (StackLevel == 0)
                return;
            if (StackLevel <= numStacks)
            {
                StackLevel = 0;
                BuffHasExpired = true;
            }
            else
            {
                StackLevel = (byte) (StackLevel - numStacks);
                DeleteBuffParameter(_buffInfo.StackLine);
                AddBuffParameter(_buffInfo.StackLine, StackLevel);
                SendStart(null, false, true);
            }
        }

        /// <summary>
        /// <para>Immediately removes a buff.</para>
        /// <para>This must be called from the same thread as the player.</para>
        /// </summary>
        public virtual void RemoveBuff(bool wasManual)
        {
            if (BuffState == (byte)EBuffState.Running)
                BuffEnded(true, wasManual);
        }

        protected virtual void BuffEnded(bool wasRemoved, bool wasManual)
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

            // Flag exhaustion needs to send end before performing the swap
            if (Entry == 14323)
                SendEnded();

            if (_buffInfo.CommandInfo != null)
                foreach (BuffCommandInfo command in _buffInfo.CommandInfo)
                    if ((command.InvokeOn & (byte)EBuffState.Ended) > 0)
                        BuffEffectInvoker.InvokeCommand(this, command, Target);

            if (EventCommands.Count > 0)
                foreach (var evtpair in EventCommands)
                    _buffInterface.RemoveEventSubscription(this, evtpair.Item1);

            if (_buffInfo.CommandInfo != null)
            {
                foreach (BuffCommandInfo command in _buffInfo.CommandInfo)
                {
                    if (command.InvokeOn == 8 && command.TargetType == CommandTargetTypes.Caster)
                    {
                            Caster.BuffInterface.RemoveEventSubscription(this, command.EventID);
                    }
                }
            }

            if (Entry != 14323)
                SendEnded();

            if (_linkedBuff != null && !_linkedBuff.BuffHasExpired)
                _linkedBuff.BuffHasExpired = true;
        }

        public bool SuppressEndNotification = false;

        protected void SendEnded()
        {
            if (!IsEnabled || Entry == 8289) // suppress cleansing power
                return;

            if (!SuppressEndNotification)
            { 
                PacketOut Out = new PacketOut((byte) Opcodes.F_INIT_EFFECTS, 12);
                Out.WriteByte(1);
                Out.WriteByte(2);
                Out.WriteUInt16(0);
                Out.WriteUInt16(Target.Oid);
                Out.WriteUInt16(BuffId);
                Out.WriteUInt16R(_buffInfo.Entry == 3320 ? (ushort) 20619 : _buffInfo.Entry);
                Out.WriteByte(0);

                DispatchBuffPacket(Out);
            }
            if (_buffInfo.EffectType != 1 || (HasSentEnd && _buffInfo.Group != BuffGroups.ItemProc))
                return;

            // Buff appearance / animation
            {
                PacketOut Out = new PacketOut((byte) Opcodes.F_CAST_PLAYER_EFFECT, 10);

                if (_buffInfo.Group != BuffGroups.Caltrops)
                {
                    Out.WriteUInt16(Caster.Oid);
                    Out.WriteUInt16(Target.Oid);
                }
                else
                {
                    Out.WriteUInt16(Caster.Oid);
                    Out.WriteUInt16(Caster.Oid);
                }
                
                Out.WriteUInt16(_buffInfo.Entry);

                if (Target is BuffHostObject || Target is GroundTarget)
                {
                    if (Entry == 8177 || _buffInfo.Group == BuffGroups.Caltrops)
                        Out.WriteByte(1);
                    else
                        Out.WriteByte(0);
                }
                else
                    Out.WriteByte(Target.Realm == Caster.Realm ? _buffInfo.FriendlyEffectID : _buffInfo.EnemyEffectID);

                Out.WriteByte(0); // Combat Event not used here
                Out.WriteByte(0); // Start/End
                Out.WriteByte(0); // Packet end

                if (_noFX)
                {
                    (Target as Player)?.SendPacket(Out);
                    (Caster as Player)?.SendPacket(Out);
                }
                else
                    Target.DispatchPacket(Out, true);
            }
        }
        #endregion

        #region Events

        protected int BuffTimerLock, BuffStackLock;

        public long AbilityThrottleReleaseTime;

        public virtual void InvokeDamageEvent(byte eventId, AbilityDamageInfo damageInfo, Unit eventInstigator)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;

            BuffCommandInfo myCommand = EventCommands.Find(evtpair => evtpair.Item1 == eventId).Item2;

            if (myCommand == null)
                return;

            if (!string.IsNullOrEmpty(myCommand.EventCheck) && !BuffEffectInvoker.PerformCheck(this, damageInfo, myCommand, eventInstigator))
                return;

            if (myCommand.EventChance > 0 && StaticRandom.Instance.Next(0, 100) > myCommand.EventChance)
                return;

            if (myCommand.RetriggerInterval != 0)
            {
                // If two threads clash here, we're guaranteed to be setting the next time anyway
                // so the thread which can't get the lock should just return
                if (Interlocked.CompareExchange(ref BuffTimerLock, 1, 0) != 0)
                    return;

                if (myCommand.NextTriggerTime != 0 && myCommand.NextTriggerTime > TCPManager.GetTimeStampMS())
                {
                    Interlocked.Exchange(ref BuffTimerLock, 0);
                    return;
                }

                myCommand.NextTriggerTime = TCPManager.GetTimeStampMS() + myCommand.RetriggerInterval;
                Interlocked.Exchange(ref BuffTimerLock, 0);
            }

            if (myCommand.ConsumesStack)
            {
                while (Interlocked.CompareExchange(ref BuffStackLock, 1, 0) != 0);

                if (StackLevel == 0)
                {
                    Interlocked.Exchange(ref BuffStackLock, 0);
                    return;
                }

                RemoveStack();

                if (Entry == 8090 || Entry == 9393)
                    ((Player) Caster).SendClientMessage((eventInstigator?.Name ?? "Something") + "'s " + AbilityMgr.GetAbilityNameFor(damageInfo.DisplayEntry) + " broke your stealth.");


                Interlocked.Exchange(ref BuffStackLock, 0);
            }

            if (myCommand.CommandName == "None")
                return;

            BuffEffectInvoker.InvokeDamageEventCommand(this, myCommand, damageInfo, Target, eventInstigator);
        }

        public virtual void InvokeCastEvent(byte eventID, AbilityInfo abInfo)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;
            BuffCommandInfo myCommand = EventCommands.Find(evtpair => evtpair.Item1 == eventID).Item2;
            if (myCommand == null)
                return;
            if (myCommand.EventChance > 0 && StaticRandom.Instance.Next(0, 100) > myCommand.EventChance)
                return;

            // Lazy checking for some tactics that don't warrant creating new delegates.
            switch (Entry)
            {
                // Scourged Warping (requires Scourge to be the ability casted)
                case 3765:
                    if (abInfo.Entry != 8548)
                        return;
                    break;
                // Flashfire (requires that the ability have a cast time)
                case 3422:
                    if (abInfo.ConstantInfo.BaseCastTime == 0 || abInfo.ConstantInfo.ChannelID > 0 || abInfo.ConstantInfo.Origin == AbilityOrigin.AO_ITEM)
                        return;
                    break;
                // Shadow Prowler, Incognito (ability must break)
                case 8090:
                case 9393:
                    if (abInfo.ConstantInfo.StealthInteraction == AbilityStealthType.Ignore)
                        return;
                    break;
            }

            if (myCommand.ConsumesStack)
            {
                while (Interlocked.CompareExchange(ref BuffStackLock, 1, 0) != 0) ;

                if (StackLevel == 0)
                {
                    Interlocked.Exchange(ref BuffStackLock, 0);
                    return;
                }

                RemoveStack();

                Interlocked.Exchange(ref BuffStackLock, 0);
            }

            if (myCommand.CommandName == "None")
                return;
            BuffEffectInvoker.InvokeAbilityUseCommand(this, myCommand, abInfo);
        }

        public void InvokePetEvent(byte eventID, Pet myPet)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;
            BuffCommandInfo myCommand = EventCommands.Find(evtpair => evtpair.Item1 == eventID).Item2;

            if (myCommand == null)
                return;

            if (myCommand.ConsumesStack)
            {
                while (Interlocked.CompareExchange(ref BuffStackLock, 1, 0) != 0) ;

                if (StackLevel == 0)
                {
                    Interlocked.Exchange(ref BuffStackLock, 0);
                    return;
                }

                RemoveStack();

                Interlocked.Exchange(ref BuffStackLock, 0);
            }

            if (myCommand.CommandName == "None")
                return;

            BuffEffectInvoker.InvokePetCommand(this, myCommand, myPet);
        }

        public void InvokeResourceEvent(byte eventID, byte oldVal, ref byte change)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;
            BuffCommandInfo myCommand = EventCommands.Find(evtpair => evtpair.Item1 == eventID).Item2;
            if (myCommand == null)
                return;
            if (myCommand.EventChance > 0 && StaticRandom.Instance.Next(0, 100) > myCommand.EventChance)
                return;

            if (myCommand.RetriggerInterval != 0)
            {
                // If two threads clash here, we're guaranteed to be setting the next time anyway
                // so the thread which can't get the lock should just return
                if (Interlocked.CompareExchange(ref BuffTimerLock, 1, 0) != 0)
                    return;

                if (myCommand.NextTriggerTime != 0 && myCommand.NextTriggerTime > TCPManager.GetTimeStampMS())
                {
                    Interlocked.Exchange(ref BuffTimerLock, 0);
                    return;
                }

                myCommand.NextTriggerTime = TCPManager.GetTimeStampMS() + myCommand.RetriggerInterval;
                Interlocked.Exchange(ref BuffTimerLock, 0);
            }


            if (myCommand.ConsumesStack)
            {
                while (Interlocked.CompareExchange(ref BuffStackLock, 1, 0) != 0);

                if (StackLevel == 0)
                {
                    Interlocked.Exchange(ref BuffStackLock, 0);
                    return;
                }

                RemoveStack();

                Interlocked.Exchange(ref BuffStackLock, 0);
            }

            if (myCommand.CommandName == "None")
                return;

            BuffEffectInvoker.InvokeResourceCommand(this, myCommand, oldVal, ref change);
        }

        public void InvokeItemEvent(byte eventID, Item_Info itmInfo)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;
            BuffCommandInfo myCommand = EventCommands.Find(evtpair => evtpair.Item1 == eventID).Item2;
            if (myCommand == null)
                return;

            BuffEffectInvoker.InvokeItemCommand(this, myCommand, itmInfo);
        }

        #endregion
    }
}
