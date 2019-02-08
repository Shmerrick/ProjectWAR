using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer
{
    public class InstanceSpawn : Creature
    {
        uint InstanceGroupSpawnID;
        uint BossID;
        Instance Instance;
        
        public InstanceSpawn(Creature_spawn spawn, uint instancegroupspawnid, uint bossid,Instance instance):base(spawn)
        {
            InstanceGroupSpawnID = instancegroupspawnid;
            BossID = bossid;
            Instance = instance;
            EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
        }


        public bool OnEnterCombat(Object mob, object args)
        {
            Unit Attacker = mob.GetCreature().CbtInterface.GetTarget(GameData.TargetTypes.TARGETTYPES_TARGET_ENEMY);
            if (Attacker == null)
                return false;
            if(InstanceGroupSpawnID > 0)
            {
                Instance.AttackTarget(InstanceGroupSpawnID, Attacker);
            }
            return false;
        }

        public bool OnLeaveCombat(Object mob, object args)
        {
            if (!mob.GetInstanceSpawn().IsDead && InstanceGroupSpawnID > 0)
            {
                Instance.RespawnInstanceGroup(InstanceGroupSpawnID);
            }
            return false;
        }

        protected override void SetRespawnTimer()
        {
            
        }

        public InstanceSpawn RezInstanceSpawn()
        {
            InstanceSpawn newCreature = new InstanceSpawn(Spawn, InstanceGroupSpawnID, BossID, Instance);
            Region.AddObject(newCreature, Spawn.ZoneId);
            Destroy();
            return newCreature;
        }



        public override string ToString()
        {
            return "SpawnId=" + Spawn.Guid + ",Entry=" + Spawn.Entry + ",Spawngroup=" + InstanceGroupSpawnID + ",LinkedBoss=" + BossID + ",Name=" + Name + ",Level=" + Level + ",Rank=" + Rank + ",Max Health=" + MaxHealth + ",Faction=" + Faction + ",Emote=" + Spawn.Emote + "AI:" + AiInterface.State + ",Position :" + base.ToString();
        }
    }
}
