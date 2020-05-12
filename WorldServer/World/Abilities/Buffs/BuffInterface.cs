//#define ABILITY_DEVELOPMENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.AI;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Buffs
{
    public class BuffQueueInfo
    {
        public delegate NewBuff BuffCreationDelegate();
        public delegate void BuffCallbackDelegate(NewBuff buff);

        public BuffCreationDelegate BuffCreator;
        private readonly BuffCallbackDelegate _buffCallback;

        public Unit Caster { get; }

        public byte DesiredLevel { get; set; }

        public BuffInfo BuffInfo { get; }

        public void BuffCallback(NewBuff buff)
        {
            if (_buffCallback != null)
            {
                if (buff == null && BuffInfo.BuffClass == BuffClass.Tactic)
                    Log.Info(BuffInfo.Name, "Tactic buff unsuccessfully applied!");
                _buffCallback(buff);
            }
        }

        public BuffQueueInfo()
        {
        }

        public BuffQueueInfo(Unit caster, byte desiredLevel, BuffInfo buffInfo)
        {
            Caster = caster;
            this.DesiredLevel = desiredLevel;
            this.BuffInfo = buffInfo;
        }

        public BuffQueueInfo(Unit caster, byte desiredLevel, BuffInfo buffInfo, BuffCreationDelegate creator)
        {
            Caster = caster;
            this.DesiredLevel = desiredLevel;
            this.BuffInfo = buffInfo;
            BuffCreator = creator;
        }

        public BuffQueueInfo(Unit caster, byte desiredLevel, BuffInfo buffInfo, BuffCallbackDelegate callback)
        {
            Caster = caster;
            this.DesiredLevel = desiredLevel;
            this.BuffInfo = buffInfo;
            _buffCallback = callback;
        }

        public BuffQueueInfo(Unit caster, byte desiredLevel, BuffInfo buffInfo, BuffCreationDelegate creator, BuffCallbackDelegate callback)
        {
            Caster = caster;
            this.DesiredLevel = desiredLevel;
            this.BuffInfo = buffInfo;
            BuffCreator = creator;
            _buffCallback = callback;
        }
    }

    ///<summary>
    ///<para>This interface manages the player's buffs and any events they must handle.</para>
    ///<para>Buffs are added to a queue, which is updated when the interface is.</para>
    ///</summary>

    public class BuffInterface : BaseInterface
    {
        #region Define

        private const byte MAX_EVENTS = (byte) BuffCombatEvents.MaxEvents;
        private const ushort MAX_BUFFS = 200;

        private const byte BUFF_UPDATE_INTERVAL = 250;

        #endregion

        private Unit _unitOwner;
        private Player _playerOwner;

        /// <summary>The list of the player's current buffs, as visible to other parts of the system.</summary>
        private readonly List<NewBuff> _buffs = new List<NewBuff>();
        /// <summary>
        /// <para>Buffs which are pending application.</para>
        /// <para>This is used within Update() calls and is copied to the buffs array at the end of the call.</para>
        /// </summary>
        private readonly List<NewBuff> _pendingBuffs = new List<NewBuff>();
        /// <summary>A queue of information about buffs which are yet to be applied.</summary>
        private readonly List<BuffQueueInfo> _queuedInfo = new List<BuffQueueInfo>();
        private readonly bool[] _filledSlots = new bool[MAX_BUFFS];

        public bool DetauntWard { get; set; }

        private List<GuardBuff> _guardBuffs;
        private readonly List<GuardBuff> _backingGuardBuffs = new List<GuardBuff>();
        private bool _guardsChanged;
        private readonly NewBuff[] _careerBuffs = new NewBuff[2];
        private byte _auraCount;

        private long _nextBuffUpdateTime;

        private readonly ReaderWriterLockSlim _buffRWLock = new ReaderWriterLockSlim();

        #region Init/Add

        public BuffInterface()
        {
            for (byte i = 0; i < MAX_EVENTS; ++i)
                _buffCombatSubs[i] = new List<NewBuff>();
        }

        public override bool Load()
        {
            if (_unitOwner == null)
            {
                _unitOwner = (Unit) _Owner;
                _playerOwner = _Owner as Player;

                if (_playerOwner != null)
                {
                    if (_playerOwner.Info.Buffs == null)
                        _playerOwner.Info.Buffs = new List<CharacterSavedBuff>();

                    else
                        LoadSavedBuffs();
                }
            }

            if (_playerOwner != null)
                SendBuffList(_playerOwner, true);

            return base.Load();
        }

        public void QueueBuff(BuffQueueInfo queueInfo)
        {
            if (Stopping || (DetauntWard && queueInfo.BuffInfo.Group == BuffGroups.Detaunt))
                return;
            if (queueInfo.BuffInfo == null)
                Log.Error("BuffInterface", "Attempt to queue a null Buff Info: " + queueInfo.BuffInfo); //Environment.StackTrace);
            else
            {
                lock (_queuedInfo)
                _queuedInfo.Add(queueInfo);
            }

            if (_unitOwner != null && _unitOwner.CbtInterface.IsInCombat)
                queueInfo.Caster.CbtInterface.RefreshCombatTimer();
        }

        public override void Update(long tick)
        {
            bool bBuffsModified = false;
            List<BuffQueueInfo> queuedList = null;

            // Pull in the buffinfo for buffs that are pending creation
            lock (_queuedInfo)
            {
                if (_queuedInfo.Count > 0)
                {
                    queuedList = new List<BuffQueueInfo>();
                    queuedList.AddRange(_queuedInfo);
                    _queuedInfo.Clear();
                }
            }

            int pendingLen = _pendingBuffs.Count;

            // Clear out any expired buffs from the pendingBuffs array
            for (int i = 0; i < pendingLen; ++i)
            {
                if (!_pendingBuffs[i].BuffHasExpired)
                    continue;

                NewBuff curBuff = _pendingBuffs[i];

                bBuffsModified = true;

                curBuff.RemoveBuff(false);

                if (curBuff.PersistsOnLogout && curBuff.BuffState == (byte) EBuffState.Ended)
                    RemoveSavedBuff(curBuff);

                _filledSlots[curBuff.BuffId] = false;

                if (curBuff.BuffGroup == BuffGroups.Guard && _Owner != curBuff.Caster)
                {
                    _backingGuardBuffs.Remove((GuardBuff)curBuff);
                    _guardsChanged = true;
                }
                else if (curBuff is AuraBuff && curBuff.Caster == _Owner)
                    _auraCount--;
                else if (curBuff.BuffGroup == BuffGroups.SelfClassBuff)
                    _careerBuffs[0] = null;
                else if (curBuff.BuffGroup == BuffGroups.SelfClassSecondaryBuff)
                    _careerBuffs[1] = null;

                #if DEBUG && ABILITY_DEVELOPMENT
                _Owner.Say("[X] " + curBuff.BuffName);
                #endif

                _pendingBuffs.RemoveAt(i);

                --i;
                pendingLen = _pendingBuffs.Count;
            }

            // Then add any queued buffs
            if (queuedList != null)
            {
                foreach (BuffQueueInfo bqi in queuedList)
                {
                    if (_unitOwner.IsDead && bqi.BuffInfo.RequiresTargetAlive)
                        continue;

                    if (!_unitOwner.IsDead && bqi.BuffInfo.RequiresTargetDead)
                        continue;

                    NewBuff buff = TryCreateBuff(bqi);
                    if (buff != null)
                    {
                        bBuffsModified = true;
                        _pendingBuffs.Add(buff);

                        _filledSlots[buff.BuffId] = true;
                    }

                    // Let the invoker of the buff know the result.
                    bqi.BuffCallback(buff);

                    if (buff != null)
                    {
                        buff.StartBuff();

                        if (buff.PersistsOnLogout)
                            RegisterSavedBuff(buff);
                    }
                }
            }

            if (_unitOwner != null && _unitOwner.IsDead && bBuffsModified)
            {
                pendingLen = _pendingBuffs.Count;

                for(int i=0; i < pendingLen; ++i)
                {
                    if (_pendingBuffs[i].RequiresTargetAlive)
                    {
                        RemoveFromPending(_pendingBuffs[i]);
                        --pendingLen;
                        --i;
                    }
                }
            }

            // Manage Guard
            if (_guardsChanged)
            {
                _guardBuffs = new List<GuardBuff>(_backingGuardBuffs);
                _guardsChanged = false;
            }

            // Copy the pending buffs into the used buff array
            if (bBuffsModified)
            {
                _buffRWLock.EnterWriteLock();
                try
                {
                    _buffs.Clear();
                    _buffs.AddRange(_pendingBuffs);
                }
                finally
                {
                    _buffRWLock.ExitWriteLock();
                }
            }

            // Update all the buffs
            if (_buffs.Count > 0 && TCPManager.GetTimeStampMS() > _nextBuffUpdateTime)
            {
                for (int index = 0; index < _buffs.Count; index++)
                    _buffs[index].Update(tick);

                _nextBuffUpdateTime = TCPManager.GetTimeStampMS() + BUFF_UPDATE_INTERVAL;
            }
        }

        public void InsertUnstoppable(BuffQueueInfo bqi)
        {
            NewBuff buff = TryCreateBuff(bqi);

            if (buff != null)
            {
                _pendingBuffs.Add(buff);
                _filledSlots[buff.BuffId] = true;
            }

            buff?.StartBuff();
        }

        public void Hotswap(NewBuff oldBuff, ushort newBuffEntry)
        {
            BuffInfo newInfo = AbilityMgr.GetBuffInfo(newBuffEntry);

            NewBuff buff = new NewBuff();

            buff.Initialize(oldBuff.Caster, (Unit)_Owner, oldBuff.BuffId, 40, newInfo.MaxStack, newInfo, this);

            _pendingBuffs[_pendingBuffs.IndexOf(oldBuff)] = buff;
            _buffs[_buffs.IndexOf(oldBuff)] = buff;

            buff.StartBuff();
        }

        public void RemoveFromPending(NewBuff buff, bool suppressNotify = false)
        {
            // Testing for Soul Willpower I proc
            if (suppressNotify && buff.Entry == 18139)
                buff.SuppressEndNotification = true;
            _pendingBuffs.Remove(buff);
            buff.RemoveBuff(true);

            _filledSlots[buff.BuffId] = false;
        }

        public void RemoveCCFromPending(int ccFlags)
        {
            List<NewBuff> removedBuffs = _pendingBuffs.FindAll(buff => (buff.CrowdControl & ccFlags) > 0);
            foreach (var buff in removedBuffs)
            {
                if ((buff.CrowdControl & ccFlags) > 0)
                {
                    _pendingBuffs.Remove(buff);
                    buff.RemoveBuff(true);

                    _filledSlots[buff.BuffId] = false;
                }
            }
        }

        /// <summary>
        /// <para>Creates and returns a new buff applied to the owner of this AbilityInterface.</para>
        /// </summary>
        public NewBuff TryCreateBuff(BuffQueueInfo buffQueueInfo)
        {
            if (buffQueueInfo.BuffInfo == null)
            {
                Log.Error("TryCreateBuff","NULL BUFFINFO");
                return null;
            }

            #if DEBUG && ABILITY_DEVELOPMENT
            _Owner.Say("+++ "+buffQueueInfo.BuffInfo.Name + " (" + buffQueueInfo.BuffInfo.Type + ")");
            #endif

            // Cap the level of same-faction buffs
            if (buffQueueInfo.Caster.Realm == _unitOwner.Realm && (float)buffQueueInfo.Caster.EffectiveLevel / _unitOwner.EffectiveLevel > 1.3f)
                buffQueueInfo.DesiredLevel = _unitOwner.EffectiveLevel;

            Tuple<ushort, byte> buffSlotInfo = CanAcceptBuff(buffQueueInfo);

            if (buffSlotInfo == null || buffSlotInfo.Item1 == 32000)
                return null;

            NewBuff myBuff = buffQueueInfo.BuffCreator == null ? new NewBuff() : buffQueueInfo.BuffCreator();

            if (myBuff != null)
            {
                myBuff.Initialize(buffQueueInfo.Caster, (Unit) _Owner, buffSlotInfo.Item1,
                    buffQueueInfo.DesiredLevel, Math.Min(buffQueueInfo.BuffInfo.MaxStack, buffSlotInfo.Item2),
                    buffQueueInfo.BuffInfo, this);

                if (myBuff.BuffGroup == BuffGroups.SelfClassBuff)
                    _careerBuffs[0] = myBuff;
                else if (myBuff.BuffGroup == BuffGroups.SelfClassSecondaryBuff)
                    _careerBuffs[1] = myBuff;
                else if (myBuff.BuffGroup == BuffGroups.Guard && _Owner != myBuff.Caster)
                {
                    myBuff.IsGroupBuff = true;
                    _backingGuardBuffs?.Add((GuardBuff) myBuff);
                    _guardsChanged = true;
                }
                else if (myBuff is AuraBuff && myBuff.Caster == _Owner)
                    _auraCount++;
            }

            return myBuff;
        }

        /// <summary>
        /// <para>Checks whether a buff can be added to the list.</para>
        /// <para>Handles buffs which are grouped (Blade Enchantments, etc) and multiple copies of buffs.</para>
        /// </summary>
        /// <returns>The slot into which a new buff could be inserted, or 255 if no such slot exists.</returns>
        private Tuple<ushort, byte> CanAcceptBuff(BuffQueueInfo newInfo)
        {
            NewBuff existingBuff;
            List<NewBuff> existingBuffs;
            byte desiredStackLevel = (byte)newInfo.BuffInfo.InitialStacks;

            switch (newInfo.BuffInfo.Group)
            {
                #region Guard
                case BuffGroups.Guard:
                    existingBuffs = _pendingBuffs.FindAll(buff => buff.BuffGroup == newInfo.BuffInfo.Group);

                    if (existingBuffs.Any(buff => buff.Caster == newInfo.Caster))
                        return null;

                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                #endregion
                #region Hold The Line
                case BuffGroups.HoldTheLine:
                    existingBuffs = _pendingBuffs.FindAll(buff => buff.BuffGroup == newInfo.BuffInfo.Group);
                    if (existingBuffs.Count >= 3)
                    {
                        if (newInfo.Caster != _Owner)
                            return null;

                        existingBuffs[0].BuffHasExpired = true;
                        ushort newSlot = existingBuffs[0].BuffId;
                        RemoveFromPending(existingBuffs[0]);
                        return new Tuple<ushort, byte>(newSlot, desiredStackLevel);
                    }

                    if (existingBuffs.Any(buff => buff.Caster == newInfo.Caster))
                        return null;

                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                #endregion
                #region Oath Friend
                case BuffGroups.OathFriend:
                    existingBuffs = _pendingBuffs.FindAll(buff => buff.BuffGroup == newInfo.BuffInfo.Group);
                    if (existingBuffs.Count > 0)
                    {
                        // Self component of Oath Friend. Remove any existing version
                        if (newInfo.Caster == _Owner)
                        {
                            foreach (NewBuff buff in existingBuffs)
                            {
                                if (buff.Caster != _Owner)
                                    continue;

                                buff.BuffHasExpired = true;
                                ushort newSlot2 = buff.BuffId;
                                RemoveFromPending(buff);
                                return new Tuple<ushort, byte>(newSlot2, desiredStackLevel);
                            }
                        }

                        else
                        {
                            foreach (NewBuff buff in existingBuffs)
                            {
                                // Look for and replace existing OF/DP from the caster
                                if (buff.Caster != newInfo.Caster)
                                    continue;

                                buff.BuffHasExpired = true;
                                ushort newSlot2 = buff.BuffId;
                                RemoveFromPending(buff);
                                return new Tuple<ushort, byte>(newSlot2, desiredStackLevel);
                            }
                        }
                    }

                    // Oath friend on enemy. Always apply.
                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                 #endregion
                #region Class self-buffs
                case BuffGroups.SelfClassBuff:
                    existingBuff = _pendingBuffs.Find(buff => buff.BuffGroup == newInfo.BuffInfo.Group);
                    if (existingBuff != null)
                    {
                        if (existingBuff.Entry == newInfo.BuffInfo.Entry && !newInfo.BuffInfo.CanRefresh)
                            return null;
                        ushort newSlot= existingBuff.BuffId;
                        RemoveFromPending(existingBuff);
                        return new Tuple<ushort, byte>(newSlot, desiredStackLevel);
                    }
                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                #endregion
                #region Class buffs for others
                case BuffGroups.OtherClassBuff:
                    existingBuffs = _pendingBuffs.FindAll(buff => buff.BuffGroup == newInfo.BuffInfo.Group);
                    foreach(var buff in existingBuffs)
                    {
                        if (buff.Entry == newInfo.BuffInfo.Entry)
                        {
                            if (buff.Caster != newInfo.Caster)
                                return null;
                            //if (!newInfo.BuffInfo.CanRefresh)
                            //    return null;
                        }
                        if (buff.Caster == newInfo.Caster)
                        {
                            ushort newSlot = buff.BuffId;
                            RemoveFromPending(buff);
                            return new Tuple<ushort, byte>(newSlot, desiredStackLevel);
                        }
                    }
                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                #endregion
                #region Auras
                    
                case BuffGroups.Aura:
                    existingBuff = _pendingBuffs.Find(buff => buff.Entry == newInfo.BuffInfo.Entry);
                    if (existingBuff != null)
                    {
                        // The owner's version of an aura must always persist over all others
                        if (existingBuff.Caster == _Owner)
                            return null;

                        if (newInfo.Caster == _Owner)
                        {
                            ushort newSlot = existingBuff.BuffId;
                            RemoveFromPending(existingBuff);
                            return new Tuple<ushort, byte>(newSlot, desiredStackLevel);
                        }

                        return null;

                    }
                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                #endregion
                #region Potion Stat Buffs and other exclusives
                case BuffGroups.HealPotion:
                case BuffGroups.StatPotion:
                case BuffGroups.DefensePotion:
                case BuffGroups.SharedCooldown1:
                case BuffGroups.Vanity:
                    existingBuff = _pendingBuffs.Find(buff => buff.BuffGroup == newInfo.BuffInfo.Group);
                    if (existingBuff != null)
                        RemoveFromPending(existingBuff);
                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                #endregion
                #region Resurrection
                case BuffGroups.Resurrection:
                    existingBuff = _pendingBuffs.Find(buff => buff.BuffGroup == newInfo.BuffInfo.Group || buff.Entry == 1608 || buff.Entry == 8567);
                    if (existingBuff == null)
                        return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                    if (newInfo.DesiredLevel > existingBuff.BuffLevel)
                    {
                        RemoveFromPending(existingBuff);
                        return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                    }
                    return null;
                #endregion
                #region Other buffs
                default:
                    if (newInfo.BuffInfo.MaxCopies == 0)
                    {
                        existingBuff = _pendingBuffs.Find(buff => buff.Entry == newInfo.BuffInfo.Entry && buff.Caster == newInfo.Caster);
                        if (existingBuff == null || newInfo.BuffInfo.StacksFromCaster)
                            return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);
                        if (newInfo.BuffInfo.CanRefresh)
                        {
                            desiredStackLevel += existingBuff.StackLevel;
                            ushort newSlot = existingBuff.BuffId;
                            RemoveFromPending(existingBuff, true);
                            return new Tuple<ushort, byte>(newSlot, desiredStackLevel);
                        }

                        return null;
                    }

                    existingBuffs = _pendingBuffs.FindAll(buff => buff.Entry == newInfo.BuffInfo.Entry);

                    if (existingBuffs.Count > 0)
                    {
                        foreach (var buff in existingBuffs)
                        {
                            if (buff.Caster == newInfo.Caster || buff.BuffLevel < newInfo.DesiredLevel)
                            {
                                if (newInfo.BuffInfo.CanRefresh)
                                {
                                    desiredStackLevel += buff.StackLevel;
                                    ushort newSlot = buff.BuffId;
                                    RemoveFromPending(buff, true);
                                    return new Tuple<ushort, byte>(newSlot, desiredStackLevel);
                                }
                            }
                        }

                        if (existingBuffs.Count >= newInfo.BuffInfo.MaxCopies) // B.MaxCopies)
                            return null;
                    }

                    return new Tuple<ushort, byte>(GetFreeSlot(), desiredStackLevel);

                    #endregion
            }
        }

        /// <summary>
        /// Finds the next open slot in the buff list.
        /// FIXME: Iterates. Might be slow with a lot of buffs.
        /// </summary>
        private ushort GetFreeSlot()
        {

            if (_pendingBuffs.Count() >= MAX_BUFFS)
                return 32000;

            for (ushort i = 0; i < MAX_BUFFS; ++i)
            {
                if (!_filledSlots[i])
                    return i;
            }

            return 32000;
        }

        #endregion

        #region Access

        public NewBuff GetBuff(ushort entry, Unit caster)
        {
            bool needsLock = !_buffRWLock.IsReadLockHeld;
            if (needsLock)
                _buffRWLock.EnterReadLock();

            try {
                foreach (NewBuff buff in _buffs)
                {
                    if (buff.Entry == entry && (caster == null || buff.Caster == caster))
                        return buff;
                }
                return null;
            }
            finally {
                if (needsLock)
                    _buffRWLock.ExitReadLock();
            }
        }

        // Mostly so that we can reliably poke the Witch Hunter Bullet buffs when we Execute
        public NewBuff GetCareerBuff(int index)
        {
            return _careerBuffs[index];
        }

        public void SendBuffList(Player plr, bool forceFX)
        {
            _buffRWLock.EnterReadLock();
            try
            {
                foreach (NewBuff buff in _buffs)
                    buff.SendStart(plr, forceFX);
            }
            finally { _buffRWLock.ExitReadLock(); }
        }

        public bool CleanseCC(byte flags)
        {
            bool bRemoved = false;

            _buffRWLock.EnterReadLock();

            try
            {
                foreach (var buff in _buffs)
                {
                    if ((buff.CrowdControl & flags) != 0)
                    {
                        if(buff.BuffClass != BuffClass.Morale)
                        {
                            bRemoved = true;
                            buff.BuffHasExpired = true;
                        }
                    }
                }
            }

            finally { _buffRWLock.ExitReadLock(); }

            return bRemoved;
        }

        public bool HasBuffOfType(byte flags)
        {
            _buffRWLock.EnterReadLock();

            try { return _buffs.Find(buff => ((int)buff.Type & flags) > 0) != null; }

            finally { _buffRWLock.ExitReadLock(); }
        }

        public short CleanseDebuffType(byte flags, byte count)
        {
            byte removeCount = 0;

            _buffRWLock.EnterReadLock();
            try
            {
                foreach (var buff in _buffs)
                {
                    if (!buff.BuffHasExpired && ((int)buff.Type & flags) != 0)
                    {
                        buff.BuffHasExpired = true;
                        removeCount++;
                        if (count > 0 && removeCount == count)
                            break;
                    }
                }
            }
            finally { _buffRWLock.ExitReadLock(); }

            return removeCount;
        }

        public bool CanAcceptAura()
        {
            return _auraCount < 3;
        }

        #endregion

        #region Removals

        public void RemoveBuffByEntry(ushort buffEntry)
        {
            if (Stopping)
                return;

            _buffRWLock.EnterReadLock();

            try
            {
                NewBuff toRemove = _buffs.Find(buff => buff.Entry == buffEntry);
                if (toRemove != null)
                    toRemove.BuffHasExpired = true;
            }

            finally { _buffRWLock.ExitReadLock(); }
        }

        public void RemoveBuffByID(byte buffID)
        {
            _buffRWLock.EnterReadLock();

            try
            {
                NewBuff toRemove = _buffs.Find(buff => buff.BuffId == buffID);
                if (toRemove == null)
                    return;
                if (toRemove.Caster == _Owner && (toRemove.Type == BuffTypes.Blessing || toRemove.Type == BuffTypes.Enchantment))
                    toRemove.BuffHasExpired = true;
            }

            finally { _buffRWLock.ExitReadLock(); }
        }

        public void RemoveGroupBuffsNotFrom(HashSet<Player> playerSet)
        {
            _buffRWLock.EnterReadLock();
            try
            {
                foreach (NewBuff buff in _buffs)
                {
                    if (buff.IsGroupBuff && buff.Caster != _unitOwner && (playerSet == null || !playerSet.Contains(buff.Caster)))
                        buff.BuffHasExpired = true;
                }
            }
            finally { _buffRWLock.ExitReadLock(); }
        }

        public void RemoveGroupBuffsFrom(Player player)
        {
            _buffRWLock.EnterReadLock();
            try
            {
                foreach (NewBuff buff in _buffs)
                {
                    if (buff.IsGroupBuff && buff.Caster == player)
                        buff.BuffHasExpired = true;
                }
            }
            finally { _buffRWLock.ExitReadLock(); }
        }

        public void RemoveBuffsAboveLevel(int level)
        {
            _buffRWLock.EnterReadLock();
            try
            {
                foreach (NewBuff buff in _buffs)
                {
                    if (buff.BuffLevel <= level)
                        continue;

                    if (buff.BuffClass == BuffClass.Standard || (buff.BuffClass == BuffClass.Career && buff.Type > 0))
                        buff.BuffHasExpired = true;
                }
            }
            finally { _buffRWLock.ExitReadLock(); }

        }

        public void RemoveBuffsOnDeath()
        {
            _buffRWLock.EnterReadLock();
            try
            {
                foreach (NewBuff buff in _buffs)
                {
                    if (buff.RequiresTargetAlive)
                        buff.BuffHasExpired = true;
                }
            }
            finally { _buffRWLock.ExitReadLock(); }
        }

        public void RemoveCasterBuffs()
        {
            _buffRWLock.EnterReadLock();
            try
            {
                foreach (NewBuff buff in _buffs)
                {
                    if (buff.Caster == _unitOwner)
                    {
                        if (buff.BuffClass == 0 && (buff.BuffGroup < BuffGroups.HealPotion || buff.BuffGroup > BuffGroups.SharedCooldown1))
                            buff.BuffHasExpired = true;
                    }
                }
            }
            finally { _buffRWLock.ExitReadLock(); }
        }

        #endregion

        #region Guard

        public bool HasGuard()
        {
            if (_guardBuffs == null)
                return false;

            foreach (GuardBuff g in _guardBuffs)
                if (g.Caster.GetDistanceSquare(_unitOwner) <= 900)
                    return true;

            return false;
        }

        public void CheckGuard(AbilityDamageInfo damageInfo, Unit attacker)
        {
            if (_guardBuffs == null)
                return;

            foreach(GuardBuff g in _guardBuffs)
                if (g.SplitDamage(attacker, damageInfo))
                    return;
        }

        public void CheckGuardHate(ABrain monsterBrain, ref uint hateCaused)
        {
            if (_guardBuffs == null)
                return;

            foreach (GuardBuff g in _guardBuffs)
                if (g.SplitHate(monsterBrain, ref hateCaused))
                    return;
        }

        #endregion

        #region Buff Saving

        private void LoadSavedBuffs()
        {
            List<CharacterSavedBuff> buffList = _playerOwner.Info.Buffs;

            if (buffList.Count == 0)
                return;

            uint curTime = (uint) TCPManager.GetTimeStamp();

            for (int i=0; i < buffList.Count; ++i)
            {
                if (buffList[i].EndTimeSeconds < curTime + 1)
                {
                    CharMgr.Database.DeleteObject(buffList[i]);
                    buffList.RemoveAt(i);
                    --i;
                    continue;
                }

                BuffInfo newInfo = AbilityMgr.GetBuffInfo(buffList[i].BuffId);

                newInfo.Duration = (uint)(buffList[i].EndTimeSeconds - curTime);
                if (buffList[i].StackLevel > 1)
                    newInfo.InitialStacks = buffList[i].StackLevel;

                QueueBuff(new BuffQueueInfo(_playerOwner, buffList[i].Level, newInfo));
            }
        }

        public void RegisterSavedBuff(NewBuff b)
        {
            if (_playerOwner == null)
                return;

            foreach (CharacterSavedBuff buffSave in _playerOwner.Info.Buffs)
            {
                if (buffSave.BuffId == b.Entry)
                {
                    buffSave.EndTimeSeconds = (uint)TCPManager.GetTimeStamp() + b.Duration;
                    buffSave.Level = b.BuffLevel;
                    buffSave.Dirty = true;
                    CharMgr.Database.SaveObject(buffSave);
                    return;
                }
            }

            CharacterSavedBuff save = new CharacterSavedBuff {CharacterId = _playerOwner.Info.CharacterId, BuffId = b.Entry, Level = b.BuffLevel, StackLevel = b.StackLevel, EndTimeSeconds = (uint)TCPManager.GetTimeStamp() + b.Duration};

            _playerOwner.Info.Buffs.Add(save);
            CharMgr.Database.AddObject(save);
        }



        public void RemoveSavedBuff(NewBuff b)
        {
            if (_playerOwner == null)
                return;

            List<CharacterSavedBuff> buffList = _playerOwner.Info.Buffs;

            if (buffList.Count == 0)
                return;

            for (int i = 0; i < buffList.Count; ++i)
            {
                if (buffList[i].BuffId == b.Entry)
                {
                    CharMgr.Database.DeleteObject(buffList[i]);
                    buffList.RemoveAt(i);
                    return;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Contains the buffs which want to receive notifications about a given event.
        /// </summary>
        private readonly List<NewBuff>[] _buffCombatSubs = new List<NewBuff>[MAX_EVENTS];

        public void AddEventSubscription(NewBuff buff, byte eventID)
        {
            lock(_buffCombatSubs[eventID - 1])
            {
                _buffCombatSubs[eventID - 1].Add(buff);
            }
        }

        public void NotifyAbilityStarted(AbilityInfo abInfo)
        {
            if (_buffCombatSubs[(byte)BuffCombatEvents.AbilityStarted - 1].Count == 0)
                return;

            foreach (var buff in _buffCombatSubs[(byte)BuffCombatEvents.AbilityStarted - 1])
                buff.InvokeCastEvent((byte)BuffCombatEvents.AbilityStarted, abInfo);
        }

        public void NotifyAbilityCasted(AbilityInfo abInfo)
        {
            if (_buffCombatSubs[(byte)BuffCombatEvents.AbilityCasted - 1].Count == 0)
                return;

            foreach (var buff in _buffCombatSubs[(byte)BuffCombatEvents.AbilityCasted - 1])
                buff.InvokeCastEvent((byte)BuffCombatEvents.AbilityCasted, abInfo);
        }

        public void NotifyPetEvent(Pet myPet)
        {
            if (_buffCombatSubs[(byte)BuffCombatEvents.PetEvent - 1].Count == 0)
                return;

            foreach (var buff in _buffCombatSubs[(byte)BuffCombatEvents.PetEvent - 1])
                buff.InvokePetEvent((byte)BuffCombatEvents.PetEvent, myPet);
        }

        public void NotifyResourceEvent(byte eventID, byte oldVal, ref byte change)
        {
            if (_buffCombatSubs[eventID - 1].Count == 0)
                return;

            foreach (var buff in _buffCombatSubs[eventID - 1])
                buff.InvokeResourceEvent(eventID, oldVal, ref change);
        }

        public void NotifyItemEvent(byte eventID, Item_Info myItemInfo)
        {
            if (_buffCombatSubs[eventID - 1].Count == 0)
                return;

            foreach (var buff in _buffCombatSubs[eventID - 1])
                buff.InvokeItemEvent(eventID, myItemInfo);
        }

        public void NotifyCombatEvent(byte eventID, AbilityDamageInfo damageInfo, Unit eventInstigator)
        {
            if (_buffCombatSubs[eventID - 1].Count == 0)
                return;

            List<NewBuff> localSubs;

            lock (_buffCombatSubs[eventID - 1])
            {
                localSubs = new List<NewBuff>(_buffCombatSubs[eventID - 1]);
            }

            foreach (var buff in localSubs)
                buff.InvokeDamageEvent(eventID, damageInfo, eventInstigator);
        }

        public void RemoveEventSubscription(NewBuff buff, byte eventID)
        {
            lock (_buffCombatSubs[eventID - 1])
            {
                _buffCombatSubs[eventID - 1].Remove(buff);
            }
        }

        #endregion

        #region Stop

        public bool Stopping { get; private set; }

        public override void Stop()
        {
            Stopping = true;

            lock (_queuedInfo)
                _queuedInfo.Clear();

            _buffRWLock.EnterReadLock();

            try
            {
                if (_buffs.Count > 0)
                {
                    foreach (NewBuff buff in _buffs)
                    {
                        buff.BuffHasExpired = true;


#pragma warning disable CS1030 // Директива #warning
#warning Danger of lock recursion exception.
                        buff.RemoveBuff(true);
#pragma warning restore CS1030 // Директива #warning
                    }
                }
            }

            finally { _buffRWLock.ExitReadLock(); }

            Update(TCPManager.GetTimeStampMS());

            base.Stop();
        }

        public void RemoveAllBuffs()
        {
            _buffRWLock.EnterReadLock();
            try
            {
                if (_buffs.Count > 0)
                {
                    foreach (var buff in _buffs)
                    {
                        if (!buff.AlwaysOn)
                            buff.BuffHasExpired = true;
                    }
                }
            }
            finally { _buffRWLock.ExitReadLock(); }
        }
        #endregion
    }
}
