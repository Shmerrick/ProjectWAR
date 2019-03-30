using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Components;
using WorldServer.World.AI;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects.Instances;
using WorldServer.World.Objects.Instances.The_Lost_Vale;

namespace WorldServer.World.Objects
{
    public class Boss : Creature
    {
        // Creature Proto Id
        public uint BossProtoId;
        public Timer BossCombatTimer { get; set; } = null;
        public static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        // List of Adds that the Boss can spawn
        public List<BossSpawn> AddDictionary;
        public List<BossSpawn> SpawnDictionary;
        public int PlayerDeathsCount { get; set; } = 0;
        // List of CC that the Boss is immune to.
        public List<GameData.CrowdControlTypes> CrowdControlImmunities { get; set; }
        // Whether the boss can be knockedback/down
        public bool CanBeKnockedBack { get; set; }
        public bool CanBeTaunted { get; set; }
        public int BossCombatTimerInterval { get; set; } = 30000;
        

        public override string Name => Spawn.Proto.Name;

        public Boss(Creature_spawn spawn, uint protoId) : base(spawn)
        {
            BossProtoId = protoId;
            EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            CrowdControlImmunities = new List<CrowdControlTypes>();
            BossCombatTimer = new Timer();
            CanBeKnockedBack = false;  // default : no KD - can be overriden
            CanBeTaunted = true;
            AddDictionary = new List<BossSpawn>();
            SpawnDictionary = new List<BossSpawn>();
        }

        public override void OnLoad()
        {
            base.OnLoad();

            foreach (var crowdControlImmunity in CrowdControlImmunities)
            {
                AddCrowdControlImmunity((int) crowdControlImmunity);
            }

            BossCombatTimer.Enabled = false;
        }


        public override void Update(long msTick)
        {
            base.Update(msTick);
        }

        public override void ApplyKnockback(Unit caster, AbilityKnockbackInfo kbInfo)
        {
            if (CanBeKnockedBack)
                base.ApplyKnockback(caster, kbInfo);
            else
            {
                // no knockdown Mob on bosses ...
            }
        }

        public virtual bool OnEnterCombat(Object mob, object args)
        {
            BossCombatTimer.Interval = BossCombatTimerInterval;

                if (this.AiInterface.CurrentBrain is BossBrain)
            {
                (this.AiInterface.CurrentBrain as BossBrain).ExecuteStartUpAbilities();

               
            }

            return false;
        }

        public virtual bool OnLeaveCombat(Object mob, object args)
        {
            
            //// reset all Modify Scalers
            //ModifyDmgHealScaler = 1f;
            //List<Player> plrs = GetPlayersInRange(300, false);
            //foreach (Player plr in plrs)
            //{
            //    plr.ModifyDmgHealScaler = 1f;
            //}
            //// reset the outgoing damage of the boss
            //try
            //{
            //    StsInterface.RemoveBonusMultiplier(GameData.Stats.OutgoingDamagePercent, 1.0f, BuffClass.Standard);
            //}
            //catch (Exception e)
            //{
            //    Log.Error("Exception", e.Message + "\r\n" + e.StackTrace);
            //}

            if (mob is Boss)
            {
                (mob as Boss).BuffInterface.RemoveBuffsOnDeath();
            }

            if (BossCombatTimer != null)
            {
                BossCombatTimer.Stop();
            }

            return false;
        }

        public override bool ReceiveDamage(Unit caster, AbilityDamageInfo damageInfo)
        {
            return base.ReceiveDamage(caster, damageInfo);
        }

        public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1, uint mitigation = 0)
        {
            return base.ReceiveDamage(caster, damage, hatredScale, mitigation);
        }

        public override int ReceiveHeal(Unit caster, uint healAmount, float healHatredScale = 1)
        {
            return base.ReceiveHeal(caster, healAmount, healHatredScale);
        }

        protected override void SetRespawnTimer()
        {

        }

        protected override void SetDeath(Unit killer)
        {
            base.SetDeath(killer);

            // Clean up spawns.
            if (SpawnDictionary.Count > 0)
            {
                foreach (var entry in SpawnDictionary)
                {
                    if ((!entry.Creature.IsDisposed) && (entry.Creature.IsInWorld()))
                    {
                        entry.Creature.Destroy();
                    }
                }
            }
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
                        if (!member.HasLockout((ushort)ZoneId, BossProtoId))
                            subGroup.Add(member);
                    }
                    // used to only have items of careers in group in the lootcontainer
                    //player.PriorityGroup.SubGroupLoot(player, lootContainer, subGroup);
                    player.PriorityGroup.GroupLoot(player, lootContainer);
                }

                if (menu.Menu == 12 || menu.Menu == 13) // on looting
                {
                    if (player.PriorityGroup == null &&
                        player.Zone != null &&
                        player.Zone.Info != null &&
                        (player.Zone.Info.Type == 4 || player.Zone.Info.Type == 5 || player.Zone.Info.Type == 6))
                    {
                        if (!player.HasLockout((ushort)ZoneId, BossProtoId))
                        {
                            lootContainer.SendInteract(player, menu);
                            
                        }
                    }
                }
                else
                    lootContainer.SendInteract(player, menu);

                if (!lootContainer.IsLootable())
                {
                    SetLootable(false, player);
                }
            }
        }



        public override string ToString()
        {
            return $"BOSS : {this.BossProtoId} ({Name})";
        }
    }

    public class BossPhase
    {
        public int PhaseId { get; set; }
        public string PhaseName { get; set; }
    }
}
