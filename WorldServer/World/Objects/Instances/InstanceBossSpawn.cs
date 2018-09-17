using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer
{
    public class InstanceBossSpawn : Creature
    {
        uint InstanceGroupSpawnID;
        public uint BossID;
        public ushort InstanceID;
        Instance Instance;
        
        public InstanceBossSpawn(Creature_spawn spawn, uint instancegroupspawnid, uint bossid, ushort Instanceid, Instance instance) : base(spawn)
        {
            InstanceGroupSpawnID = instancegroupspawnid;
            BossID = bossid;
            Instance = instance;
            InstanceID = Instanceid;
            EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
        }
		
        public bool OnEnterCombat(Object mob, object args)
        {
            Instance.Encounterinprogress = true;
            Unit Attacker = mob.GetCreature().CbtInterface.GetTarget(GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY);

            if(InstanceGroupSpawnID > 0)
            {
                Instance.BossAttackTarget(InstanceGroupSpawnID, Attacker);
            }
            return false;
        }

        public bool OnLeaveCombat(Object mob, object args)
        {
            
            if (!mob.GetInstanceBossSpawn().IsDead && InstanceGroupSpawnID > 0)
            {
                Instance.BossRespawnInstanceGroup(InstanceGroupSpawnID);
            }
            return false;
        }

        protected override void SetRespawnTimer()
        {
            
        }

        protected override void SetDeath(Unit killer)
        {
            Instance.OnBossDeath(InstanceGroupSpawnID, this);
            base.SetDeath(killer);
        }

        public InstanceBossSpawn RezInstanceSpawn()
        {
            InstanceBossSpawn newCreature = new InstanceBossSpawn(Spawn, InstanceGroupSpawnID, BossID, InstanceID,Instance);
            Region.AddObject(newCreature, Spawn.ZoneId);
            Destroy();
            return newCreature;
        }
		
        public override string ToString()
        {
            return "SpawnId=" + Spawn.Guid + ",Entry=" + Spawn.Entry + ",Spawngroup=" + InstanceGroupSpawnID + ",BossID=" + BossID + ",Name=" + Name + ",Level=" + Level + ",Rank=" + Rank + ",Max Health=" + MaxHealth + ",Faction=" + Faction + ",Emote=" + Spawn.Emote + "AI:" + AiInterface.State + ",Position :" + base.ToString();
        }
    }
}
