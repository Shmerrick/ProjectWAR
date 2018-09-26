using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WorldServer
{
    public class InstanceBossSpawn : Creature
    {
        uint InstanceGroupSpawnID;
        public uint BossID;
        public ushort InstanceID;
		public Instance Instance { get; set; } = null;
		public Stopwatch BossTimer { get; set; } = null;

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

			BossTimer = new Stopwatch();
			BossTimer.Start();

			return false;
        }

        public bool OnLeaveCombat(Object mob, object args)
        {
            
            if (!mob.GetInstanceBossSpawn().IsDead && InstanceGroupSpawnID > 0)
            {
                Instance.BossRespawnInstanceGroup(InstanceGroupSpawnID);
            }
			
			if (BossTimer != null)
			{
				BossTimer.Reset();
				BossTimer = null;
			}

			return false;
        }

        protected override void SetRespawnTimer()
        {
            
        }

        protected override void SetDeath(Unit killer)
        {
            base.SetDeath(killer);
			Instance.OnBossDeath(InstanceGroupSpawnID, this);

			// remove barriages from this instance
			Instance.RemoveInstanceObjectOnBossDeath(BossID);
		}

		public override void TryLoot(Player player, InteractMenu menu)
		{
			if (lootContainer != null && lootContainer.IsLootable())
			{
				List<Player> subGroup = new List<Player>();

				if (player.PriorityGroup != null)
				{
					foreach (Player member in player.PriorityGroup.Members)
					{
						if (!member.HasLockout((ushort)ZoneId, BossID))
							subGroup.Add(member);
					}
					player.PriorityGroup.SubGroupLoot(player, lootContainer, subGroup);
				}
				
				if (player.HasLockout((ushort)ZoneId, BossID))
				{
					if (player.PriorityGroup != null)
					{
						// find a player without lockout
						Player eligablePlayer = player.PriorityGroup.Members.Where(x => !x.HasLockout((ushort)ZoneId, BossID)).FirstOrDefault();
						if (eligablePlayer != null)
							lootContainer.SendInteract(eligablePlayer, menu);
					}
				}
				else
				{
					lootContainer.SendInteract(player, menu);
				}

				if (!lootContainer.IsLootable())
				{
					SetLootable(false, player);

					Instance.ApplyLockout(subGroup, InstanceGroupSpawnID, this);
				}
			}
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
