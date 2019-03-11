using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemData;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Interfaces;

namespace WorldServer.World.Objects.PublicQuests
{
    public class PQuestCreature : Creature
    {
        private readonly PQuestObjective _objective;
        private readonly PublicQuest _publicQuest;

        public PQuestCreature(Creature_spawn spawn, PQuestObjective objective, PublicQuest publicQuest) : base(spawn)
        {
            _objective = objective;
            _publicQuest = publicQuest;

            if (objective.Objective.Type == (byte)Objective_Type.QUEST_PROTECT_UNIT)
                EvtInterface.AddEvent(Protected, objective.Objective.Time * 1000, 1);

            EvtInterface.AddEventNotify(EventName.OnLeaveCombat, _publicQuest.MobLeavingCombat);
        }

        public override void RezUnit()
        {
            if (_objective.Objective.NoRespawn != 1 && this.Spawn.NoRespawn != 1)
            {
                lock (_objective.ActiveCreatures) {
                    _objective.ActiveCreatures.Remove(this);
                    PQuestCreature newCreature = new PQuestCreature(Spawn, _objective, _publicQuest);
                    newCreature.PQSpawnId = this.PQSpawnId;
                    _objective.ActiveCreatures.Add(newCreature);
                    Region.AddObject(newCreature, Spawn.ZoneId);
                }
                Destroy();
            }

        }

        protected override void SetRespawnTimer()
        {
            int baseRespawn = 0;
            if (_publicQuest.IsDungeon())
            {
                baseRespawn = 60 * 10 * 1000; // 10 minutes
                // + StaticRandom.Instance.Next(0, 180001);
                EvtInterface.AddEvent(RezUnit, baseRespawn, 1);
            }
            else
            {
                baseRespawn = 50000 + Level * 1000;

                switch (Rank)
                {
                    case 1:
                        baseRespawn *= 2; break;
                    case 2:
                        baseRespawn *= 15 + StaticRandom.Instance.Next(15); break;
                }

                EvtInterface.AddEvent(RezUnit, baseRespawn, 1); // 30 seconde Rez
            }
        }

        /// <summary>
        /// Distributes the loot and contribution rewards for this particular NPC.
        /// </summary>
        /// <param name="killer"></param>
        protected override void HandleDeathRewards(Player killer)
        {
            Dictionary<Group, XpRenown> groupXPRenown = new Dictionary<Group, XpRenown>();

            uint totalXP = WorldMgr.GenerateXPCount(killer, this);

            RemoveDistantDamageSources();

            if (DamageSources.Count == 0 || TotalDamageTaken == 0)
                return;

            Player looter = null;
            uint bestDamage = 0;

            foreach (KeyValuePair<Player, uint> kvpair in DamageSources)
            {
                Player curPlayer = kvpair.Key;

                if (curPlayer == null)
                    continue;

                float damageFactor = (float)kvpair.Value / TotalDamageTaken;

                uint xpShare = (uint)(totalXP * damageFactor);

                // Handle contribution to the PQ.
                int rankMod;

                switch (Rank)
                {
                    case 1:
                        rankMod = 4;
                        break;
                    case 2:
                        rankMod = 20; break;
                    default:
                        rankMod = 1; break;
                }
                
                _publicQuest.HandleEvent(curPlayer, Objective_Type.QUEST_KILL_MOB, Spawn.Entry, bestDamage == 0 ? 1 : 0, (ushort)(100 * damageFactor * rankMod));
                curPlayer.SendClientMessage("Received " + 100 * damageFactor * rankMod + " contribution for dealing damage.", ChatLogFilters.CHATLOGFILTERS_QUEST, true);

                // Solo player, add their rewards directly.
                if (curPlayer.PriorityGroup == null)
                {
                    curPlayer.AddXp(xpShare, true, true);
                    if (kvpair.Value > bestDamage)
                    {
                        looter = curPlayer;
                        bestDamage = kvpair.Value;
                    }
                }

                else
                {
                    if (groupXPRenown.ContainsKey(curPlayer.PriorityGroup))
                        groupXPRenown[curPlayer.PriorityGroup].XP += xpShare;
                    else
                        groupXPRenown.Add(curPlayer.PriorityGroup, new XpRenown(xpShare, 0, 0, 0, TCPManager.GetTimeStampMS()));

                    groupXPRenown[curPlayer.PriorityGroup].Damage += kvpair.Value;

                    if (groupXPRenown[curPlayer.PriorityGroup].Damage > bestDamage)
                    {
                        looter = curPlayer;
                        bestDamage = kvpair.Value;
                    }
                }
            }

            if (groupXPRenown.Count > 0)
            {
                foreach (KeyValuePair<Group, XpRenown> kvpair in groupXPRenown)
                    kvpair.Key.AddXpCount(killer, kvpair.Value.XP);
            }

            if (looter != null)
                GenerateLoot(looter, 1f);

            CreditQuestKill(looter);
            _publicQuest.NotifyKilled(this);
        }

        public void Protected()
        {
            _objective.Quest.HandleEvent(null, Objective_Type.QUEST_PROTECT_UNIT, Spawn.Entry, _objective.Objective.Count, (ushort)(_objective.Objective.Time * 40));
        }
        
        /// <summary>
        /// Objective property necessary for scripts.
        /// </summary>
        public PQuestObjective Objective { get { return _objective; } }
    }
}
