using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Buffs.SpecialBuffs
{
    class OYGAuraBuff : AuraBuff
    {
        private readonly Dictionary<ushort, long> _nextTargetHitTimes = new Dictionary<ushort, long>();
        private const ushort OYG_INTERVAL = 2000;

        public bool CanHitTarget(Unit Target)
        {
            long tick = TCPManager.GetTimeStampMS();

            lock(_nextTargetHitTimes)
            {
                if (!_nextTargetHitTimes.ContainsKey(Target.Oid))
                {
                    _nextTargetHitTimes.Add(Target.Oid, tick + OYG_INTERVAL);
                    return true;
                }
            }
                
            if (tick < _nextTargetHitTimes[Target.Oid])
                return false;

            _nextTargetHitTimes[Target.Oid] = tick + OYG_INTERVAL;

            return true;
        }

        protected override void PropagateGroup()
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
                foreach (var member in myGroup.GetUnitList(_myPlayer))
                {
                    if (member == Caster)
                        continue;

                    if (member.IsInCastRange(Caster, MAX_GROUP_RADIUS))
                    {
                        if (_groupTargetList.ContainsKey(member))
                        {
                            if (_groupTargetList[member].BuffHasExpired) // Group member already has this buff and it's still active, so sustain it
                                _groupTargetList.Remove(member);
                            continue;
                        }

                        if (Duration == 0)
                            member.BuffInterface.QueueBuff(new BuffQueueInfo(Caster, BuffLevel, AbilityMgr.GetBuffInfo(_buffInfo.Entry), BuffEffectInvoker.CreateOygBuff, RegisterOYG));
                        else
                        {
                            BuffInfo BI = AbilityMgr.GetBuffInfo(_buffInfo.Entry);
                            BI.Duration = (ushort)(RemainingTimeMs * 0.001f);
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

            List<Unit> gtlKeys = _groupTargetList.Keys.ToList();

            foreach (var guy in gtlKeys)
            {
                if (myGroup == null || !myGroup.HasUnit(guy))
                {
                    _groupTargetList[guy].BuffHasExpired = true;
                    _groupTargetList.Remove(guy);
                }
            }
        }

        // cross thread
        public void RegisterOYG(NewBuff B)
        {
            if (B != null)
            {
                lock (_groupPendingTargetList)
                    _groupPendingTargetList.Add(B);
                (B as OYGBuff).MasterAuraBuff = this;
            }
        }

        public override void InvokeDamageEvent(byte eventId, AbilityDamageInfo damageInfo, Unit eventInstigator)
        {
            if (BuffState != (byte)EBuffState.Running)
                return;
            if (!string.IsNullOrEmpty(EventCommands[0].Item2.EventCheck) && !BuffEffectInvoker.PerformCheck(this, damageInfo, EventCommands[0].Item2, eventInstigator))
                return;
            if (!CanHitTarget(eventInstigator))
                return;
            BuffEffectInvoker.InvokeDamageEventCommand(this, EventCommands[0].Item2, damageInfo, Target, eventInstigator);
        }
    }
}
