using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.World.Battlefronts.Keeps;

namespace WorldServer.World.Battlefronts
{
    internal class RationTracker
    {
        /// <summary>
        /// The number of players which a keep is always capable of supporting.
        /// </summary>
        const int SUPPLY_BASE_SUPPORT = 24;

        private RegionMgr _region;
        public float[] RationFactor { get; } = { 1f, 1f };
        private readonly int[] _supplyCaps = new int[2];
        private readonly HashSet<Player>[] _withinKeepRange = { new HashSet<Player>(), new HashSet<Player>() };
        private readonly List<RationBuff>[] _rationDebuffs = { new List<RationBuff>(), new List<RationBuff>() };

        public RationTracker(RegionMgr region)
        {
            _region = region;
        }

        /// <summary>
        /// Gets the ration factor that should be applied to given unit.
        /// </summary>
        /// <param name="unit">To applie factor to, not null</param>
        /// <returns>Factor less or equal 1f</returns>
        public float GetRationFactor(Unit unit)
        {
            return RationFactor[(int)unit.Realm - 1];
        }

        internal void AddRationed(Player player, int index)
        {
            lock (_withinKeepRange)
                _withinKeepRange[index].Add(player);

            if (RationFactor[index] > 1f)
                player.BuffInterface.QueueBuff(new BuffQueueInfo(player, 1, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Rationing), RationBuff.CreateRationBuff, AssignRationDebuff));
        }

        internal void RemoveRationed(Player player)
        {
            int realmIndex = (int)player.Realm - 1;
            lock (_withinKeepRange)
            {
                if (!_withinKeepRange[realmIndex].Contains(player))
                    return;
            }

            lock (_withinKeepRange)
                _withinKeepRange[realmIndex].Remove(player);

            lock (_rationDebuffs[realmIndex])
                for (int i = 0; i < _rationDebuffs[realmIndex].Count; ++i)
                {
                    if (_rationDebuffs[realmIndex][i].Target == player)
                    {
                        _rationDebuffs[realmIndex][i].BuffHasExpired = true;
                        _rationDebuffs[realmIndex].RemoveAt(i);
                        break;
                    }
                }
        }

        private void RemoveAllRationed()
        {
            for (int index = 0; index < 2; ++index)
            {
                lock (_withinKeepRange)
                    _withinKeepRange[index].Clear();

                lock (_rationDebuffs[index])
                    foreach (RationBuff buff in _rationDebuffs[index])
                        buff.BuffHasExpired = true;

                _rationDebuffs[index].Clear();
            }
        }

        public void UpdateRationing(IList<Player> players, IList<Keep> keeps, IList<int>[] popHistory)
        {
            int[] popHigh = { popHistory[0].Max(), popHistory[1].Max() };

            for (int i = 0; i < 2; ++i)
            {
                Keep currentKeep = null;
                foreach (Keep keep in keeps)
                {
                    if ((int)keep.Realm == i + 1 && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && keep.KeepStatus != KeepStatus.KEEPSTATUS_SEIZED)
                    {
                        currentKeep = keep;
                        break;
                    }
                }

                if (currentKeep == null)
                {
                    RemoveAllRationed();
                    return;
                }

                Realms realm = (i == 0 ? Realms.REALMS_REALM_ORDER : Realms.REALMS_REALM_DESTRUCTION);

                foreach (Player plr in players)
                {
                    if (plr.Realm != realm)
                        continue;

                    bool isInKeepRange = plr.IsWithinRadiusFeet(currentKeep, 600);
                    if (_withinKeepRange[i].Contains(plr))
                    {
                        if (!isInKeepRange)
                            RemoveRationed(plr);
                    }
                    else if (isInKeepRange)
                    {
                        AddRationed(plr, i);
                    }
                }

                int rationedCount = _withinKeepRange[i].Count;

                // The keep's supply cap is the population high of the enemy realm over the last 15 minutes.
                // TODO - NEWDAWN
                // _supplyCaps[i] = (int)Math.Max(SUPPLY_BASE_SUPPORT, popHigh[1 - i] * currentKeep.GetRationFactor());

                if (rationedCount <= _supplyCaps[i])
                {
                    // The realm is within ration tolerance.
                    // Return if not penalized.
                    if (RationFactor[i] == 1f)
                        continue;

                    //Remove the penalty.
                    RationFactor[i] = 1f;

                    lock (_rationDebuffs[i])
                    {
                        foreach (RationBuff b in _rationDebuffs[i])
                            b.BuffHasExpired = true;

                        _rationDebuffs[i].Clear();
                    }
                }

                else
                {
                    // There are more members within the keep than its supplies can allow for.
                    float newRationFactor = (int)((rationedCount * 5) / (float)_supplyCaps[i]);
                    newRationFactor *= 0.2f;

                    if (newRationFactor > 1f)
                    {
                        newRationFactor -= 1f;
                        newRationFactor *= 0.35f;
                        newRationFactor += 1f;
                    }

                    if (newRationFactor == RationFactor[i])
                        continue;

                    if (RationFactor[i] == 1f)
                    {
                        lock (_withinKeepRange)
                            foreach (Player player in _withinKeepRange[i])
                                player.BuffInterface.QueueBuff(new BuffQueueInfo(player, 1, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Rationing), RationBuff.CreateRationBuff, AssignRationDebuff));
                    }

                    else
                    {
                        // Update existing ration debuffs
                        lock (_rationDebuffs[i])
                        {
                            foreach (RationBuff debuff in _rationDebuffs[i])
                                debuff.PendingDebuffFactor = newRationFactor;
                        }
                    }

                    RationFactor[i] = newRationFactor;
                }
            }
        }

        public void AssignRationDebuff(NewBuff rationDebuff)
        {
            if (rationDebuff == null)
                return;

            int realmIndex = (int)rationDebuff.Caster.Realm - 1;

            if (RationFactor[realmIndex] == 1f || rationDebuff.Target.Region != _region)
                rationDebuff.BuffHasExpired = true;
            else
            {
                lock (_rationDebuffs[realmIndex])
                    _rationDebuffs[realmIndex].Add((RationBuff)rationDebuff);
            }
        }

    }
}
