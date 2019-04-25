using System;
using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    public class AuraBuff : NewBuff
    {
        protected class AuraInfo
        {
            public NewBuff Buff;
            public int PassNum;

            public AuraInfo(NewBuff buff, int passNum)
            {
                Buff = buff;
                PassNum = passNum;
            }
        }

        private long _nextAuraPropagationTime;
        private int _propInterval;
        protected Player _myPlayer;

        protected Dictionary<Unit, NewBuff>    _groupTargetList;
        protected Dictionary<Unit, AuraInfo>    _otherTargetList;
        protected List<NewBuff> _groupPendingTargetList, _otherPendingTargetList;

        protected const int MAX_GROUP_RADIUS = 100;
        protected int MaxFoeRadius = 30;
        protected const uint MAX_HTL_RADIUS = 50;

        public override void Initialize(Unit caster, Unit target, ushort buffId, byte buffLevel, byte stackLevel, BuffInfo myBuffInfo, BuffInterface parentInterface)
        {
            base.Initialize(caster, target, buffId, buffLevel, stackLevel, myBuffInfo, parentInterface);

            _myPlayer = caster as Player;

            switch (_buffInfo.AuraPropagation)
            {
                case "Group":
                    _groupTargetList = new Dictionary<Unit, NewBuff>();
                    _groupPendingTargetList = new List<NewBuff>();
                    break;
                case "Foe":
                case "Foe20":
                case "Foe30":
                case "Foe40":
                case "Foe250":
                    _otherTargetList = new Dictionary<Unit, AuraInfo>();
                    _otherPendingTargetList = new List<NewBuff>();
                    break;
                case "All":
                    _groupTargetList = new Dictionary<Unit, NewBuff>();
                    _groupPendingTargetList = new List<NewBuff>();
                    _otherTargetList = new Dictionary<Unit, AuraInfo>();
                    _otherPendingTargetList = new List<NewBuff>();
                    break;
                case "HTL":
                    _otherTargetList = new Dictionary<Unit, AuraInfo>();
                    _otherPendingTargetList = new List<NewBuff>();
                    break;
            }

            _propInterval = Entry == 9251 || Entry == 1927 ? 250 : 1000;
        }

        public override void Update(long tick)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;

            long curTime = TCPManager.GetTimeStampMS();

            if (EndTime > 0 && curTime >= EndTime)
                BuffEnded(false, false);
            else
            {
                if (NextTickTime > 0 && curTime >= NextTickTime)
                {
                    NextTickTime += _buffInfo.Interval;

                    foreach (BuffCommandInfo command in _buffInfo.CommandInfo)
                        if ((command.InvokeOn & (byte)EBuffState.Running) > 0)
                            BuffEffectInvoker.InvokeCommand(this, command, Target);
                }

                if (_nextAuraPropagationTime <= tick)
                {
                    _nextAuraPropagationTime = tick + _propInterval;
                    SpreadAura();
                }
            }
        }

        private void SpreadAura()
        {
            try
            {
                switch (_buffInfo.AuraPropagation)
                {
                    case "Group":
                        PropagateGroup();
                        break;
                    case "Foe":
                        PropagateFoe();
                        break;
                    case "Foe20":
                        MaxFoeRadius = 20;
                        PropagateFoe();
                        break;
                    case "Foe30":
                        MaxFoeRadius = 30;
                        PropagateFoe();
                        break;
                    case "Foe40":
                        MaxFoeRadius = 40;
                        PropagateFoe();
                        break;
                    case "Foe250":
                        MaxFoeRadius = 250;
                        PropagateFoe();
                        break;
                    case "All":
                        PropagateGroup();
                        PropagateFoe();
                        break;
                    case "HTL":
                        PropagateBehind();
                        break;
                }
            }
            catch
            {
                (Caster as Player)?.SendClientMessage("Aura spread failure - one of your auras was removed.");
                BuffHasExpired = true;
            }

            ++_passNum;
        }

        // within master thread
        public override void RemoveBuff(bool wasManual)
        {
            if (BuffState == (byte) EBuffState.Running)
            {
                BuffEnded(true, wasManual);

                if (_groupTargetList != null && _groupTargetList.Count > 0)
                {
                    foreach (NewBuff buff in _groupTargetList.Values)
                        buff.BuffHasExpired = true;
                }

                if (_groupPendingTargetList != null && _groupPendingTargetList.Count > 0)
                {
                    lock (_groupPendingTargetList)
                    {
                        foreach (NewBuff buff in _groupPendingTargetList)
                            buff.BuffHasExpired = true;
                        _groupPendingTargetList.Clear();
                    }
                }

                if (_otherTargetList != null && _otherTargetList.Count > 0)
                {
                    foreach (AuraInfo info in _otherTargetList.Values)
                        info.Buff.BuffHasExpired = true;
                }


                if (_otherPendingTargetList != null && _otherPendingTargetList.Count > 0)
                {
                    lock (_otherPendingTargetList)
                    {
                        foreach (var buff in _otherPendingTargetList)
                            buff.BuffHasExpired = true;
                        _otherPendingTargetList.Clear();
                    }
                }
            }
        }

        #region Propagation

        private int _passNum;

        protected virtual void PropagateGroup() 
        {
            if (_groupPendingTargetList.Count > 0)
            {
                List<NewBuff> locList = new List<NewBuff>();
                lock (_groupPendingTargetList)
                {
                    locList.AddRange(_groupPendingTargetList);
                    _groupPendingTargetList.Clear();
                }

                foreach (var buff in locList)
                    _groupTargetList.Add(buff.Target, buff);
            }

            Group myGroup = _myPlayer.PriorityGroup;

            if (myGroup != null)
            {
                foreach (var member in myGroup.GetUnitList((Player)Caster))
                {
                    if (member == Caster && Target == Caster)
                        continue;

                    if (!Target.IsDead && member.ObjectWithinRadiusFeet(Target, MAX_GROUP_RADIUS - 10))
                    {
                        if (_groupTargetList.ContainsKey(member))
                        {
                            if (_groupTargetList[member].BuffHasExpired)
                                _groupTargetList.Remove(member);
                            continue;
                        }

                        if (Duration == 0)
                            member.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, member), RegisterGroupBuff));
                        else
                        {
                            BuffInfo BI = AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, member);
                            BI.Duration = (ushort)(Math.Max(1, RemainingTimeMs*0.001f));
                            member.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, BI, RegisterGroupBuff));
                        }
                    }

                    else
                    {
                        if (_groupTargetList.ContainsKey(member))
                        {
                            // Group member out of range - finish the buff and remove them
                            if (!_groupTargetList[member].BuffHasExpired)
                                _groupTargetList[member].BuffHasExpired = true;
                            _groupTargetList.Remove(member);
                        }
                    }
                }
            }

            else
            {
                if (_myPlayer != Target)
                {
                    if (_myPlayer.IsInCastRange(Target, MAX_GROUP_RADIUS))
                    {
                        if (_groupTargetList.ContainsKey(_myPlayer))
                        {
                            if (_groupTargetList[_myPlayer].BuffHasExpired)
                                _groupTargetList.Remove(_myPlayer);
                        }

                        else if (Duration == 0)
                            _myPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, _myPlayer), RegisterGroupBuff));
                        else
                        {
                            BuffInfo BI = AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, _myPlayer);
                            BI.Duration = (ushort)(Math.Max(1, RemainingTimeMs * 0.001f));
                            _myPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, BI, RegisterGroupBuff));
                        }
                    }

                    else
                    {
                        if (_groupTargetList.ContainsKey(_myPlayer))
                        {
                            // Group member out of range - finish the buff and remove them
                            if (!_groupTargetList[_myPlayer].BuffHasExpired)
                                _groupTargetList[_myPlayer].BuffHasExpired = true;
                            _groupTargetList.Remove(_myPlayer);
                        }
                    }

                }

            }


            List<Unit> gtlKeys = _groupTargetList.Keys.ToList();

            foreach (var guy in gtlKeys)
            {
                if (guy == _myPlayer && _myPlayer != Target)
                    continue;

                if (Target.IsDead || myGroup == null || !myGroup.HasUnit(guy))
                {
                    _groupTargetList[guy].BuffHasExpired = true;
                    _groupTargetList.Remove(guy);
                }
            }
        }

        private void PropagateFoe() 
        {
            int addedThisTick = 0;

            if (_otherPendingTargetList.Count > 0)
            {
                lock (_otherPendingTargetList)
                {
                    foreach (NewBuff buff in _otherPendingTargetList)
                        _otherTargetList.Add(buff.Target, new AuraInfo(buff, _passNum));

                    _otherPendingTargetList.Clear();
                }
            }

            foreach (Object obj in Target.ObjectsInRange)
            {
                Unit foe = obj as Unit;

                if (foe == null || foe.IsInvulnerable)
                    continue;

                if (!Target.IsDead && foe.ObjectWithinRadiusFeet(Target, MaxFoeRadius - 10) && CombatInterface.CanAttack(Caster, foe) && ((Caster is Creature) || (!(foe is Player) || foe.CbtInterface.IsPvp)) && Target.LOSHit(foe))
                {
                    if (_otherTargetList.ContainsKey(foe))
                    {
                        if (_otherTargetList[foe].Buff.BuffHasExpired)
                            _otherTargetList.Remove(foe);
                        else
                            _otherTargetList[foe].PassNum = _passNum;
                        continue;
                    }

                    if (addedThisTick + _otherTargetList.Count == 9)
                        continue;

                    if (Duration == 0)
                        foe.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, foe), RegisterOtherBuff));
                    else
                    {
                        BuffInfo BI = AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, foe);
                        BI.IsAoE = true;
                        BI.Duration = Math.Max((ushort)1, (ushort)(RemainingTimeMs * 0.001f));
                        foe.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, BI, RegisterOtherBuff));
                    }

                    ++addedThisTick;
                }

                else
                {
                    if (!_otherTargetList.ContainsKey(foe))
                        continue;

                    // Group member out of range - finish the buff and remove them
                    if (!_otherTargetList[foe].Buff.BuffHasExpired)
                        _otherTargetList[foe].Buff.BuffHasExpired = true;

                    _otherTargetList.Remove(foe);
                }
            }


            List<Unit> oldUnits = _otherTargetList.Keys.ToList();

            // Remove any units not refreshed on this tick
            foreach (Unit oldfoe in oldUnits)
            {
                if (_otherTargetList[oldfoe].PassNum == _passNum)
                    continue;

                _otherTargetList[oldfoe].Buff.BuffHasExpired = true;
                _otherTargetList.Remove(oldfoe);
            }
        }

        private void PropagateBehind() 
        {
            if (_otherPendingTargetList.Count > 0)
            {
                List<NewBuff> locList = new List<NewBuff>();
                lock (_otherPendingTargetList)
                {
                    locList.AddRange(_otherPendingTargetList);
                    _otherPendingTargetList.Clear();
                }

                foreach (var buff in locList)
                    _otherTargetList.Add(buff.Target, new AuraInfo(buff, _passNum));
            }

            foreach (Object obj in Target.ObjectsInRange)
            {
                Unit ally = obj as Player;

                if (ally == null || ally.Realm != Caster.Realm)
                    continue;

                if (Caster.CanHitWithAoE(ally, 360, MAX_HTL_RADIUS - 10) && !Caster.IsObjectInFront(ally, 270))
                {
                    if (_otherTargetList.ContainsKey(ally))
                    {
                        if (_otherTargetList[ally].Buff.BuffHasExpired) // Group member already has this buff and it's still active, so sustain it
                            _otherTargetList.Remove(ally);
                        else
                            _otherTargetList[ally].PassNum = _passNum;
                        continue;
                    }

                    if (Duration == 0)
                        ally.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, ally), RegisterOtherBuff));

                    else if (RemainingTimeMs * 0.001f > 0)
                    {
                        BuffInfo bi = AbilityMgr.GetBuffInfo(_buffInfo.Entry, Caster, ally);
                        bi.Duration = Math.Max((ushort)1, (ushort)(RemainingTimeMs * 0.001f));

                        ally.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, bi, RegisterOtherBuff));
                    }
                }

                else
                {
                    if (!_otherTargetList.ContainsKey(ally))
                        continue;

                    // Group member out of range - finish the buff and remove them
                    if (!_otherTargetList[ally].Buff.BuffHasExpired)
                        _otherTargetList[ally].Buff.BuffHasExpired = true;

                    _otherTargetList.Remove(ally);
                }
            }

            List<Unit> oldUnits = _otherTargetList.Keys.ToList();

            // Remove any units not refreshed on this tick
            foreach (Unit oldAlly in oldUnits)
            {
                if (_otherTargetList[oldAlly].PassNum == _passNum)
                    continue;

                _otherTargetList[oldAlly].Buff.BuffHasExpired = true;
                _otherTargetList.Remove(oldAlly);
            }
        }

        #endregion

        // cross thread
        public void RegisterGroupBuff(NewBuff buff)
        {
            if (buff == null)
                return;

            if (BuffHasExpired)
                buff.BuffHasExpired = true;
            else
            {
                // Oil
                buff.OptionalObject = OptionalObject;
                lock (_groupPendingTargetList)
                    _groupPendingTargetList.Add(buff);
            }
        }

        // cross thread
        public void RegisterOtherBuff(NewBuff buff)
        {
            if (buff == null)
                return;

            if (BuffHasExpired)
                buff.BuffHasExpired = true;
            else
            {
                // Oil
                buff.OptionalObject = OptionalObject;
                lock (_otherPendingTargetList)
                    _otherPendingTargetList.Add(buff);

            }
        }
    }
}
