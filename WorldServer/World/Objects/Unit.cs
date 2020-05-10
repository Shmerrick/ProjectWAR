using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.AI;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios;
using AbilityType = WorldServer.World.Abilities.Components.AbilityType;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public enum StateOpcode
    {
        None = 0,
        Wireframe = 1,
        Flight = 2,
        RvRFlag = 4,
        RenownTitle = 8,
        Lootable = 9,
        ToKTitle = 0xC,
        ZoneEntry = 0x11,
        Down = 19,
        Combat = 0x1A,
        CastCompletion = 0x1B,
        Butcherable = 0x1F,
        Scavengeable = 0x20,
        SiegeIdleTimer = 0x21,
        Stunned = 0x23,
        HelmCloak = 0x24
    };

    public class XpRenown
    {
        public uint XP { get; set; }
        public uint Renown { get; set; }
        public uint Damage { get; set; }
        public long LastUpdatedTime { get; set; }
        public ushort InfluenceId { get; }
        public ushort Influence { get; set; }


        public XpRenown(uint xp, uint renown, ushort infId, ushort inf, long lastUpdatedTime)
        {
            XP = xp;
            Renown = renown;
            InfluenceId = infId;
            Influence = inf;
            LastUpdatedTime = lastUpdatedTime;
        }

        public void Add(uint xp, uint renown, ushort inf)
        {
            XP += xp;
            Renown += renown;
            Influence += inf;
        }

        public override string ToString()
        {
            return $"XPR:XP:{XP} RP:{Renown} INF:{Influence} DMG:{Damage} TIME:{LastUpdatedTime}";
        }
    }

    public class Unit : Object
    {
        #region Static

        public static int HEALTH_REGEN_TIME = 4000; // 4secondes
        public static int ACTION_REGEN_TIME = 500; // .5 seconds
        public static int CR_REGEN_TIME = 1000;

        private static readonly Logger DeathLogger = LogManager.GetLogger("DeathLogger");
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");

        public static InteractType GenerateInteractType(Creature_proto proto)
        {
            InteractType type = InteractType.INTERACTTYPE_IDLE_CHAT;

            switch (proto.TitleId)
            {
                case CreatureTitle.RallyMaster:
                    type = InteractType.INTERACTTYPE_BINDER;
                    break;

                case CreatureTitle.Apothecary:
                case CreatureTitle.Butcher:
                case CreatureTitle.Cultivator:
                case CreatureTitle.Salvager:
                case CreatureTitle.Scavenger:
                case CreatureTitle.HedgeWizard:
                case CreatureTitle.Trainer:
                case CreatureTitle.CareerTrainer:
                case CreatureTitle.RenownTrainer:
                case CreatureTitle.ApprenticeCareerTrainer:
                case CreatureTitle.ApprenticeRenownTrainer:
                    type = InteractType.INTERACTTYPE_TRAINER;
                    break;

                case CreatureTitle.FlightMaster:
                case CreatureTitle.ExpeditionQuartermaster:
                    type = InteractType.INTERACTTYPE_FLIGHT_MASTER;
                    break;
                case CreatureTitle.GuildRegistrar:
                    type = InteractType.INTERACTTYPE_GUILD_REGISTRAR;
                    break;
                case CreatureTitle.Healer:
                    type = InteractType.INTERACTTYPE_HEALER;
                    break;
                case CreatureTitle.Banker:
                    type = InteractType.INTERACTTYPE_BANKER;
                    break;

                case CreatureTitle.Auctioneer:
                    type = InteractType.INTERACTTYPE_AUCTIONEER;
                    break;

                case CreatureTitle.NameRegistrar:
                    type = InteractType.INTERACTTYPE_LASTNAMESHOP;
                    break;

                case CreatureTitle.VaultKeeper:
                    type = InteractType.INTERACTTYPE_GUILD_VAULT;
                    break;

                case CreatureTitle.Merchant:
                case CreatureTitle.ArmorMerchant:
                case CreatureTitle.WeaponMerchant:
                case CreatureTitle.CampMerchant:
                case CreatureTitle.SiegeWeaponMerchant:
                case CreatureTitle.CraftSupplyMerchant:
                case CreatureTitle.RenownGearMerchant:
                case CreatureTitle.Quartermaster:
                case CreatureTitle.BasicRenownGearMerchant:
                case CreatureTitle.VeteranRenownGearMerchant:
                case CreatureTitle.AdvancedRenownGearMerchant:
                case CreatureTitle.RecruitMedallionQuartermaster:
                case CreatureTitle.ScoutMedallionQuartermaster:
                case CreatureTitle.SoldierMedallionQuartermaster:
                case CreatureTitle.OfficerMedallionQuartermaster:
                case CreatureTitle.RoyalQuartermaster:
                case CreatureTitle.RecruitEmblemQuartermaster:
                case CreatureTitle.ScoutEmblemQuartermaster:
                case CreatureTitle.SoldierEmblemQuartermaster:
                case CreatureTitle.OfficerEmblemQuartermaster:
                case CreatureTitle.VanquisherQuartermaster:
                case CreatureTitle.VerySpecialDyeVendor:
                case CreatureTitle.SpecializedArmorsmith:
                case CreatureTitle.RenownArmorQuartermaster:
                case CreatureTitle.RenownWeaponQuartermaster:
                case CreatureTitle.CommoditiesQuartermaster:
                case CreatureTitle.TomeTacticLibrarian:
                case CreatureTitle.TomeTrophyLibrarian:
                case CreatureTitle.EliteRenownGearMerchant:
                case CreatureTitle.UpgradeMerchant:
                case CreatureTitle.DoorRepairMerchant:
                case CreatureTitle.StandardMerchant:
                case CreatureTitle.TomeAccessoryLibrarian:
                case CreatureTitle.TomeTokenLibrarian:
                case CreatureTitle.TalismanMerchant:
                case CreatureTitle.MountVendor:
                case CreatureTitle.CompanionKeeper:
                case CreatureTitle.NoveltyVendor:
                case CreatureTitle.Librarian:
                case CreatureTitle.RecordsKeeper:
                    type = InteractType.INTERACTTYPE_DYEMERCHANT;
                    break;

                case CreatureTitle.LightMountVendor:
                case CreatureTitle.HeavyMountVendor:
                case CreatureTitle.SpecialtyMountWrangler:
                    type = InteractType.INTERACTTYPE_STORE;
                    break;

                case CreatureTitle.BarberSurgeon:
                    type = InteractType.INTERACTTYPE_BARBER_SURGEON;
                    break;
            }

            return type;
        }

        #endregion

        public List<byte> States = new List<byte>();
        public InteractType InteractType = InteractType.INTERACTTYPE_IDLE_CHAT;

        public Point3D SpawnPoint = new Point3D(0, 0, 0);
        public Point3D WorldSpawnPoint = new Point3D(0, 0, 0);
        public ushort SpawnHeading;
        public bool StateDirty;

        /// <summary>Indicates that a unit cannot suffer damage.</summary>
        public bool IsInvulnerable = false;

        /// <summary>The ID of a model to use as this player's mount.</summary>
        public ushort MountID { get; protected set; }
        /// <summary>Mount armor for medium mounts.</summary>
        public ushort MountArmor { get; protected set; }

        public bool CanMount { get; set; } = true;
        
        /// <summary>
        /// Scaler applied to damage/heal received or dealed in instance boss fights.
        /// </summary>
        public volatile float ModifyDmgHealScaler = 1f;

        public Unit()
        {
            ItmInterface = AddInterface<ItemsInterface>();
            CbtInterface = (CombatInterface) AddInterface(CombatInterface.GetInterfaceFor(this));
            StsInterface = AddInterface<StatsInterface>();
            QtsInterface = AddInterface<QuestsInterface>();
            MvtInterface = AddInterface<MovementInterface>();
            AbtInterface = AddInterface<AbilityInterface>();
            BuffInterface = AddInterface<BuffInterface>();
            AiInterface = AddInterface<AIInterface>();
            OSInterface = AddInterface<ObjectStateInterface>();
        }

        public override void OnLoad()
        {
            SaveSpawnData();

            LoadInterfaces();
        }
 
        protected void SaveSpawnData()
        {
            SpawnPoint.SetCoordsFrom(this);
            SpawnHeading = Heading;
            WorldSpawnPoint.SetCoordsFrom(WorldPosition);
        }

        /// <summary>
        /// Repeater interval to send F_OBJECT_STATE in order to maintain an object on the clients.
        /// The client holds an object for 1 minute, so we send a repeater close to that lastUpdatedTime.
        /// </summary>
        private readonly int _stateInterval = 40000 + StaticRandom.Instance.Next(10000);

        public override void Update(long msTick)
        {
            if (!IsDead)
            {
                UpdateHealth(msTick);
                SendCollatedHit();
            }

            if (_nextSend < msTick)
            {
                _nextSend = msTick + _stateInterval;
                StateDirty = true;
            }

            if (StateDirty)
            {
                _nextSend = msTick + _stateInterval;
                StateDirty = false;
                SendState();
            }

            base.Update(msTick);
        }

        #region Sender

        private long _nextSend;

        public override void SendMeTo(Player plr)
        {
            /*if (Plr.CrrInterface is IPetCareerInterface && Plr.CrrInterface.GetTargetOfInterest() != null)
                (Plr.CrrInterface.GetTargetOfInterest() as Pet).SendPet(2);*/
            ItmInterface.SendEquipped(plr);
            SendState(plr);
            OSInterface.SendObjectStates(plr);
            base.SendMeTo(plr);
            if (MountID != 0)
                SendMount(plr);
            SendGfxMods();
        }

        public virtual void SendMovementState(Player player, ushort targetX, ushort targetY, ushort targetZ, bool isRun)
        {
            PacketOut Out = new PacketOut((byte) Opcodes.F_OBJECT_STATE, 28);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16((ushort) X);
            Out.WriteUInt16((ushort) Y);
            Out.WriteUInt16((ushort) Z);
            Out.WriteByte(PctHealth);
            // Possibly flags:
            // 1: Destination position update (Rotation update if 0)
            // 2: Look-at target
            // 16: States
            // 32: No Gravity
            // 40: Recall state
            byte flags = 1;
            if (CbtInterface.GetCurrentTarget() != null)
                flags |= 2;
            Out.WriteByte(flags);
            Out.WriteByte((byte) Zone.ZoneId);
            Out.WriteByte(0); // Unk1
            Out.WriteUInt32(0); // Unk2

            /*if (isRun)
                Out.WriteUInt16R(0xEB);
            else
                Out.WriteUInt16R(0x55);*/
            Out.WriteUInt16R((ushort)(Speed * 2.35f));

            Out.WriteByte(0); // DestUnk
            Out.WriteUInt16R(targetX);
            Out.WriteUInt16R(targetY);
            Out.WriteUInt16R(targetZ);
            Out.WriteByte((byte) Zone.ZoneId);
            if ((flags & 2) == 2)
            Out.WriteUInt16R(CbtInterface.GetCurrentTarget()?.Oid ?? 0);

            if (player == null)
                DispatchPacket(Out, false);
            else
                player.SendPacket(Out);
        }

        public virtual void SendState(Player plr=null)
        {
            if (!IsInWorld())
                return;

            MvtInterface.UpdateMovementState(plr);

            if (MvtInterface.IsMoving)
                SendAnimation();
        }

        public virtual void SendAnimation()
        {
            PacketOut Out = new PacketOut((byte) Opcodes.F_ANIMATION, 6);
            Out.WriteUInt16(Oid);
            Out.WriteUInt32(0);
            DispatchPacket(Out, false);
        }

        #endregion

        #region Interfaces

        public ObjectStateInterface OSInterface;
        public MovementInterface MvtInterface;
        public ItemsInterface ItmInterface;
        public CombatInterface CbtInterface;
        public StatsInterface StsInterface;
        public QuestsInterface QtsInterface;
        public AbilityInterface AbtInterface;
        public BuffInterface BuffInterface;
        public AIInterface AiInterface;

        #endregion

        #region Health & Damage

        protected uint _health;
        public uint MaxHealth = 0;
        public uint BonusHealth = 0;

        /// <summary>This player is heavily wounded and moves more slowly than normal.</summary>
        protected bool _isCriticallyWounded;

        public uint Health
        {
            get { return _health; }
            set
            {
                _health = value;
                StateDirty = true;
                if (this is Player)
                    ((Player) this).SendHealth();
            }
        }

        public long NextHpRegen;
 
        public uint TotalHealth => MaxHealth + BonusHealth;
        public byte PctHealth => (byte)((Health * 100) / (TotalHealth == 0 ? 1 : TotalHealth));

        public bool IsDead => Health <= 0;

        /// <summary>Object to lock on when modifying a player's hit points.</summary>
        protected object DamageApplicationLock = new object();

        public virtual void UpdateHealth(long tick)
        {
            if (tick < NextHpRegen)
                return;

            NextHpRegen = tick + HEALTH_REGEN_TIME;

            if (Health == TotalHealth)
                return;

            int bonusHP = StsInterface.GetTotalStat(Stats.HealthRegen) * 4;

            if (!CbtInterface.IsInCombat && AiInterface.State != AiState.FIGHTING)
                bonusHP += (int)TotalHealth/8;

            if (bonusHP > 0)
                ReceiveHeal(null, (uint)bonusHP);
        }
    
        /// <summary>Heals this unit and returns the points healed, or -1 if the unit died before it could be healed.</summary>
        public virtual int ReceiveHeal(Unit caster, uint healAmount, float healHatredScale = 1.0f)
        {
            uint oldHealth;
            uint resultantHealth;

            if (IsDead || PendingDisposal)
                return -1;

            lock (DamageApplicationLock)
            {
                if (IsDead)
                    return -1;
                oldHealth = _health;
                _health = Math.Min(MaxHealth, _health + healAmount);
                resultantHealth = _health;

                if (_health == MaxHealth && oldHealth < MaxHealth)
                    DamageSources.Clear();
            }

            if (caster != null)
            {
                CbtInterface.OnTakeHeal(caster);
                caster.CbtInterface.OnDealHeal(this, resultantHealth - oldHealth);
            }

            LastHealOid = caster?.Oid ?? Oid;

            return (int)(resultantHealth - oldHealth);
        }

        protected uint TotalDamageTaken, PendingTotalDamageTaken;

        /// <summary>Inflicts damage upon this unit and returns whether lethal damage was dealt.</summary>
        public virtual bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1f, uint mitigation = 0)
        {
            bool wasKilled = false;
            Player creditedPlayer = null;

            if (IsDead || PendingDisposal || (IsInvulnerable && damage != int.MaxValue))
                return false;

            if (caster.Realm != Realm)
                creditedPlayer = caster.CbtInterface.CreditedPlayer;

            lock (DamageApplicationLock)
            {
                if (IsDead)
                    return false;
                if (damage >= Health)
                {
                    wasKilled = true;

                    damage = Health;
                    _health = 0;

                    if (creditedPlayer != null)
                    {
                        PendingTotalDamageTaken += damage;
                        AddTrackedDamage(creditedPlayer, damage);
                    }

                    TotalDamageTaken = PendingTotalDamageTaken;
                    PendingTotalDamageTaken = 0;
                }
                else
                {
                    _health = Math.Max(0, _health - damage);

                    if (creditedPlayer != null)
                    {
                        PendingTotalDamageTaken += damage;
                        AddTrackedDamage(creditedPlayer, damage);
                    }
                }
            }

            CbtInterface.OnTakeDamage(caster, damage, hatredScale, mitigation);
            caster.CbtInterface.OnDealDamage(this, damage);

            LastHitOid = caster.Oid;

            if (wasKilled)
                SetDeath(caster);

            return wasKilled;
        }

        /// <summary>Inflicts damage upon this unit and returns whether lethal damage was dealt.</summary>
        public virtual bool ReceiveDamage(Unit caster, AbilityDamageInfo damageInfo)
        {
            // It's sad I feel I need to do this. If the character is a GM, only do 25% damage.
            if (caster is Player)
            {
                if ((caster as Player).GmLevel == 8)
                    damageInfo.Damage = damageInfo.Damage * 0.25f;
            }
            return ReceiveDamage(caster, (uint)damageInfo.Damage, damageInfo.HatredScale, (uint)damageInfo.Mitigation);
        }

        protected ushort LastHitOid;
        protected ushort LastHealOid;
        private uint _lastHealth;

        protected void SendHit(ushort casterId)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_HIT_PLAYER, 12);
            Out.WriteUInt16(casterId);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            Out.WriteUInt16((ushort)_health);
            Out.WriteByte(PctHealth);
            Out.Fill(0, 3);

            DispatchPacket(Out, true);

            LastHitOid = 0;
            LastHealOid = 0;
            _lastHealth = _health;
        }

        protected void SendCollatedHit()
        {
            if (_lastHealth == 0)
                _lastHealth = TotalHealth;

            if (LastHitOid == 0 && LastHealOid == 0)
                return;

            SendHit(_health >= _lastHealth ? LastHealOid : LastHitOid);
        }

        protected readonly Dictionary<Player, uint> DamageSources = new Dictionary<Player, uint>();

        protected void AddTrackedDamage(Player caster, uint damage)
        {
            if (caster == this)
                return;
            if (DamageSources.ContainsKey(caster))
                DamageSources[caster] += damage;
            else DamageSources.Add(caster, damage);

            if (this is Player player)
            {
                DeathLogger.Trace($"Looking for instanceManagerInstance");
                ImpactMatrixManager impactManagerInstance;
                if (player.ScnInterface.Scenario == null)
                {
                    //find the battlefrontstatus that this player is in.
                    impactManagerInstance =
                        player.GetBattlefrontManager(player.Region.RegionId).ImpactMatrixManagerInstance;
                }
                else
                {
                    impactManagerInstance = ScenarioMgr.ImpactMatrixManagerInstance;

                }

                var modificationValue = (float) impactManagerInstance.CalculateModificationValue((float) player.BaseBountyValue, (float) caster.BaseBountyValue);

                // Added impact to ImpactMatrix
                if (this.CbtInterface.IsPvp)
                {

                    if (Region == null)
                        DeathLogger.Debug($"Region is null for caster {caster.Name}");
                    if (Region?.Campaign == null)
                        DeathLogger.Debug($"Region.Campaign is null for caster {caster.Name}");

                    impactManagerInstance.UpdateMatrix(player.CharacterId,
                        new PlayerImpact
                        {
                            CharacterId = caster.Info.CharacterId,
                            ExpiryTimestamp = FrameWork.TCPManager.GetTimeStamp() + ImpactMatrixManager.IMPACT_EXPIRY_TIME,
                            ImpactValue = (int) damage,
                            ModificationValue = modificationValue
                        });
                }
            }
        }

        public void ClearTrackedDamage()
        {
            DamageSources.Clear();
            PendingTotalDamageTaken = 0;
            TotalDamageTaken = 0;
        }

        protected void RemoveDistantDamageSources()
        {
            List<Player> damageSourceRemovals = new List<Player>();
            foreach (Player plr in DamageSources.Keys)
            {
                if (plr.GetDistanceTo(this) > 300)
                {
                    TotalDamageTaken -= DamageSources[plr];
                    damageSourceRemovals.Add(plr);
                }
            }

            if (damageSourceRemovals.Count > 0)
                foreach (var plr in damageSourceRemovals)
                    DamageSources.Remove(plr);
        }

        /// <summary>Flags this unit as dead and generates XP and loot.</summary>
        protected virtual void SetDeath(Unit killer)
        {
            

            DeathLogger.Trace($"Base.SetDeath {killer.Name}");
            try
            {
                Health = 0;

                States.Add((byte)CreatureState.Dead); // Death State
                DeathLogger.Trace($"SendCollatedHit {killer.Name}");
                SendCollatedHit();

                Pet pet = (killer as Pet);
                Player credited = (pet != null) ? pet.Owner : (killer as Player);

                if (credited == null)
                    DeathLogger.Trace($"Credited is empty {killer.Name}");
                else
                {
                    DeathLogger.Trace($"Credited : {killer.Name}");
                }

                DeathLogger.Trace($"SendPacket 0x0A {killer.Name}");
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
                Out.WriteUInt16(Oid);
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WriteUInt16(credited?.Oid ?? killer.Oid);
                Out.Fill(0, 6);
                DispatchPacket(Out, true);

                DeathLogger.Trace($"Interfaces {killer.Name}");

                AbtInterface.Cancel(true);
                ScrInterface.OnDie(this);
                BuffInterface.RemoveBuffsOnDeath();
                AiInterface.OnOwnerDied();

                DeathLogger.Trace($"Notify OnDie {killer.Name}");

                EvtInterface.Notify(EventName.OnDie, this, credited ?? killer);

                DeathLogger.Trace($"Base HandleDeath {killer.Name} {credited?.Name}" );

                if (credited != null && credited.Zone != null && !credited.PendingDisposal)
                    HandleDeathRewards(credited);
                

                DeathLogger.Trace($"Clearing {killer.Name}");

                ClearTrackedDamage();
            }
            catch (Exception e)
            {
                DeathLogger.Warn($"Exception : {e.Message} {e.StackTrace}");
                throw;
            }
            
        }

        protected virtual void HandleDeathRewards(Player killer)
        {
            DeathLogger.Warn($"Unit.HandleDeathRewards : {killer.Name}");
            if (killer == this)
                return;

            WorldMgr.GenerateXP(killer, this, 1f);

            GenerateLoot(killer.PriorityGroup != null ? killer.PriorityGroup.GetGroupLooter(killer) : GetLooter(killer), 1f);
        }

        /// <summary>
        /// Resurrects this unit.
        /// </summary>
        public virtual void RezUnit()
        {
            AiInterface.ProcessCombatEnd();
            States.Remove((byte)CreatureState.Dead); // Death State
            Health = TotalHealth;
            Region.UpdateRange(this, true);

            //EvtInterface.Notify(EventName.ON_REZURECT, this, null);
        }

        /// <summary>
        /// Returns true if this unit should block the particular type of incoming damage.
        /// </summary>
        public virtual bool ShouldDefend(Unit attacker, AbilityDamageInfo incDamage)
        {
            return false;
        }

        /// <summary>
        /// Provides an opportunity for this unit to check the caster of the incoming ability damage from enemies.
        /// </summary>
        public virtual void CheckDamageCaster(Unit caster, AbilityDamageInfo incDamage)
        {

        }

        /// <summary>
        /// Provides an opportunity for this unit to modify incoming ability damage from enemies.
        /// </summary>
        public virtual void ModifyDamageIn(AbilityDamageInfo incDamage)
        {
            
        }

        /// <summary>
        /// Provides an opportunity for this unit to modify outgoing ability damage it deals.
        /// </summary>
        public virtual void ModifyDamageOut(AbilityDamageInfo outDamage)
        {
            
        }

        /// <summary>
        /// Provides an opportunity for this unit to modify incoming ability heal from enemies.
        /// </summary>
        public virtual void ModifyHealIn(AbilityDamageInfo incHeal)
        {
            if (!incHeal.IsHeal)
                return;

            if (incHeal.Damage > 0) // direct heal
            {
                incHeal.Damage *= ModifyDmgHealScaler;
            }
            else // hot
            {
                incHeal.DamageBonus *= ModifyDmgHealScaler;
            }
        }

        /// <summary>
        /// Provides an opportunity for this unit to modify outgoing ability heal it deals.
        /// </summary>
        public virtual void ModifyHealOut(AbilityDamageInfo outHeal)
        {
            if (!outHeal.IsHeal)
                return;

            if (outHeal.Damage > 0) // direct heal
            {
                outHeal.Damage *= ModifyDmgHealScaler;
            }
            else // hot
            {
                outHeal.DamageBonus *= ModifyDmgHealScaler;
            }
        }

        /// <summary>
        /// Attempt at detaunts redo
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Dictionary<ushort, float> Detaunters = new Dictionary<ushort, float>();

        #endregion

        #region Action Points

        public virtual bool HasActionPoints(int amount)
        {
            return true;
        }

        /// <summary><para>Adds the amount specified to the player's action points.</para>
        /// <para>Returns the change in action points.</para></summary>
        public virtual int ModifyActionPoints(int amount)
        {
            return 0;
        }

        /// <summary><para>Removes the amount specified from the player's action points.</para>
        /// <para>Returns false if player doesn't have enough AP.</para></summary>
        public virtual bool ConsumeActionPoints(ushort amount)
        {
            return true;
        }

        #endregion

        #region Combat

        public bool CanHitWithAoE(Unit target, double hitAngle, uint maxRadius)
        {
            if (target?.Zone == null)
                return false;

            Pet pet = null;
            if (target is Pet)
                pet = target as Pet;

            if (pet != null && pet.Owner != null && (pet.Owner.Info.CareerLine == (int)CareerLine.CAREERLINE_WHITELION) && WorldMgr.WorldSettingsMgr.GetGenericSetting(18) == 0)
                return false;

            if (Math.Abs(target.Z - Z) > 360) // 20ft by ability range (30ft in reality)
                return false;

            float angle = GetAngle(new Point2D(target.WorldPosition.X, target.WorldPosition.Y));
            if (angle >= 360 - hitAngle / 2 || angle < hitAngle / 2)
            {
                if (maxRadius == 0)
                    return true;

                // Check for players in cover
                Player plr = target as Player;

                if (plr != null && plr.Palisade != null && (plr.Palisade.IsObjectInFront(plr, 180) ^ plr.Palisade.IsObjectInFront(this, 180)))
                    return false;

                return IsInCastRange(target, maxRadius - 10) && LOSHit(target);
            }

            return false;
        }

        public static float CHARACTER_HEIGHT = 72.0f;

        public bool LOSHit(Unit target)
        {
            if (Zone == null)
            {
                Log.Error("LOSHit", "No Zone");
                return false;
            }

            if (target == null)
            {
                Log.Error("LOSHit", "No target");
                return false;
            }

            if (target.Zone == null)
            {
                Log.Error("LOSHit", "Target not in a Zone");
                return false;
            }

            if (!World.Map.Occlusion.Initialized)
            {
                Log.Error("LOSHit", "Occlusion library not initialized");
                return false;
            }

            if (Zone.ZoneId != target.Zone.ZoneId)
            {
                Log.Info("LOSHit", "Cross zone LOS not implemented");
                return false;
            }

            var playnice = new World.Map.OcclusionInfo();

            int fromRegionX = X + (Zone.Info.OffX << 12);
            int fromRegionY = Y + (Zone.Info.OffY << 12);
            int toRegionX = target.X + (Zone.Info.OffX << 12);
            int toRegionY = target.Y + (Zone.Info.OffY << 12);

            World.Map.Occlusion.SegmentIntersect(Zone.ZoneId, Zone.ZoneId, fromRegionX, fromRegionY, Z + CHARACTER_HEIGHT, toRegionX, toRegionY, target.Z + CHARACTER_HEIGHT, true, true, 190, ref playnice);



            return playnice.Result == World.Map.OcclusionResult.NotOccluded;
        }

        public bool LOSHit(ushort zoneId, Point3D pinPos)
        {
            if (Zone == null)
            {
                Log.Error("LOSHit", "No Zone");
                return false;
            }

            if (!World.Map.Occlusion.Initialized)
            {
                Log.Error("LOSHit", "Occlusion library not initialized");
                return false;
            }

            var playnice = new World.Map.OcclusionInfo();

            int fromRegionX = X + (Zone.Info.OffX << 12);
            int fromRegionY = Y + (Zone.Info.OffY << 12);
            int toRegionX = pinPos.X + (Zone.Info.OffX << 12);
            int toRegionY = pinPos.Y + (Zone.Info.OffY << 12);

            World.Map.Occlusion.SegmentIntersect(Zone.ZoneId, Zone.ZoneId, fromRegionX, fromRegionY, Z + CHARACTER_HEIGHT, toRegionX, toRegionY, pinPos.Z + CHARACTER_HEIGHT, true, true, 190, ref playnice);
          
//            #if DEBUG
//            //if (IsPlayer())
//               // GetPlayer().SendLocalizeString(playnice.ToString(), SystemData.ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.CHAT_TAG_DEFAULT);
//#endif

            return playnice.Result == World.Map.OcclusionResult.NotOccluded;
        }

        /// <summary>Performs an auto-attack against the target using the specified hand.</summary>
        public virtual void Strike(Unit target, EquipSlot slot = EquipSlot.MAIN_HAND)
        {
            if (target.IsDead || !CanAutoAttack)
                return;

            CombatManager.InflictAutoAttackDamage(slot, this, target);
        }

        /// <summary>Displays the auto-attack animation on clients.</summary>
        public virtual void SendAttackMovement(Unit target)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);
            Out.WriteUInt16(0);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            Out.WriteUInt16(target.Oid);
            Out.WriteByte(2);
            Out.Fill(0, 9);
            DispatchPacket(Out, true);
        }

        #endregion

        #region Loot

        public LootContainer lootContainer;

        /// <summary><para>Determines who should receive the loot for this kill.</para></summary>
        public virtual Player GetLooter(Player killer)
        {
            Player firstStriker = ((CombatInterface_Npc)CbtInterface).FirstStriker;
            if (firstStriker != null && Rank < 2)
                return firstStriker;
            return killer;
        }

        public virtual void GenerateLoot(Player looter, float dropMod)
        {
            RewardLogger.Debug($"Looter : {looter.Name}");
            
            lootContainer = LootsMgr.GenerateLoot(this, looter, dropMod);
            
            if (lootContainer != null)
                SetLootable(true, looter);          
        }
		
		public void SetLootable(bool value, Player looter)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(Oid);
            Out.WriteByte(9);
            Out.WriteByte((byte)(value ? 1 : 0));
            Out.Fill(0, 6);
            if (looter != null)
                looter.SendPacket(Out);
            else
                DispatchPacket(Out, false);
        }

        public void GatherLootable(bool value, Player looter, byte gatherProfession)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(Oid);
            Out.WriteByte((byte)(30 + gatherProfession));
            Out.WriteByte((byte)(value ? 1 : 0));
            Out.Fill(0, 6);


            foreach (Player player in PlayersInRange)
            {
                if (player._Value.GatheringSkill == gatherProfession)
                    player.SendCopy(Out);
            }
        }

        #endregion

        #region Interact

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (IsDead)
                TryLoot(player, menu);

            base.SendInteract(player, menu);
        }

        public virtual void TryLoot(Player player, InteractMenu menu)
        {
            if (lootContainer != null && lootContainer.IsLootable())
            {
                player.PriorityGroup?.GroupLoot(player, lootContainer);

                lootContainer.SendInteract(player, menu);

                if (!lootContainer.IsLootable())
                    SetLootable(false, player);
            }
        }

        #endregion

        #region Values

        public ushort Speed
        {
            get
            {
                if (IsDisabled)
                    return 0;
                return (ushort)(StsInterface.Speed * StsInterface.VelocityMod);
            }
            set
            {
                StsInterface.Speed = value;
            }
        }

        public virtual void UpdateSpeed()
        {
            MvtInterface.SetBaseSpeed((ushort)(StsInterface.Speed * StsInterface.VelocityMod));
        }

        private byte _level = 1;
        public byte Level
        {
            get {
                return IsPlayer() ? GetPlayer()._Value.Level : _level;
            }
            set
            {
                if (IsPlayer())
                    GetPlayer().SetLevel(value);
                else
                    _level = value;
            }
        }

        /// <summary>
        /// Represents the level the player has for the purpose of combat calculations.
        /// </summary>
        public byte EffectiveLevel => StsInterface.BolsterLevel == 0 ? Level : StsInterface.BolsterLevel;

        /// <summary>
        /// Represents the current base level that the player has. This is equal to the player's level unless they are debolstered.
        /// </summary>
        public byte AdjustedLevel => StsInterface.BolsterLevel == 0 ? Level : Math.Min(Level, StsInterface.BolsterLevel);

        private byte _renown = 1;
        public byte RenownRank
        {
            get
            {
                if (IsPlayer())
                    return GetPlayer()._Value.RenownRank;
                return _renown;
            }
            set
            {
                if (IsPlayer())
                    GetPlayer().SetRenownLevel(value);
                else
                    _renown = value;
            }
        }

        protected byte _adjustedRenown;

        /// <summary>
        /// Represents the current base renown rank that the player has. This is equal to the player's renown rank unless they are debolstered.
        /// </summary>
        public byte AdjustedRenown => _adjustedRenown > 0 ? _adjustedRenown : RenownRank;

        private ushort _model;
        public ushort Model
        {
            get
            {
                if (IsPlayer())
                    return GetPlayer().Info.ModelId;
                else
                    return _model;
            }
            set
            {
                if (IsPlayer())
                    GetPlayer().Info.ModelId = (byte)value;
                else
                    _model = value;
            }
        }

        public byte Rank; // Normal,Champion,Hero,Lord
        public byte Faction; // Faction Flag
        public byte FactionId; // FactionFlag/8
        public bool Aggressive;
        public Realms Realm { get; set; } = Realms.REALMS_REALM_NEUTRAL;

        public void SetFaction(byte newFaction)
        {
            Faction = newFaction;

            FactionId = (byte)(newFaction / 8);
            Faction = (byte)(newFaction % 8);
            Aggressive = Convert.ToBoolean(Faction % 2);
            Rank = (byte)(Faction / 2);

            if (FactionId >= 8 && FactionId <= 15)
                Realm = Realms.REALMS_REALM_ORDER;
            else if (FactionId >= 16 && FactionId <= 23)
                Realm = Realms.REALMS_REALM_DESTRUCTION;
            else
                Realm = Realms.REALMS_REALM_NEUTRAL;

            Faction = newFaction;

            if (AiInterface.CurrentBrain == null || (!(this is GameObject) && AiInterface.CurrentBrain is DummyBrain))
            {
                if (Aggressive)
                {
                    AiInterface.SetBrain(new AggressiveBrain(this));
                }
                else
                    AiInterface.SetBrain(new PassiveBrain(this));
            }
        }

        #endregion

        #region Range

        public override void AddInRange(Object obj)
        {
           // Log.Info("InRange", "AddInRange : For=" + Name + " To " + obj.Name + " id=" + obj.Oid + " distance=" + GetAdjustedDistanceTo(obj));
            if (obj.IsUnit())
                AiInterface.AddRange(obj.GetUnit());

            base.AddInRange(obj);
        }

        public override void RemoveInRange(Object obj)
        {
            if (obj.IsUnit())
                AiInterface.RemoveRange(obj.GetUnit());

            base.RemoveInRange(obj);
        }

        public override void ClearRange(bool fromNewRegion = false)
        {
            AiInterface.ClearRange();
            base.ClearRange(fromNewRegion);
        }

        #endregion

        #region UpdateState

        public void SendUpdateState(byte stateID, byte val1, byte val2 = 0, byte val3 = 0)
        {
            if (!(this is Player))
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(Oid);
            Out.WriteByte(stateID);
            Out.WriteByte(val1);
            Out.WriteByte(val2);
            Out.WriteByte(val3);    // guild heraldy
            Out.Fill(0, 5);
            ((Player) this).SendPacket(Out);
        }

        public void DispatchUpdateState(byte stateID, byte val1, byte val2 = 0, byte val3 = 0)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(stateID);
            Out.WriteByte(val1);
            Out.WriteByte(val2);
            Out.WriteByte(val3);    // guild heraldry
            Out.Fill(0, 5);
            DispatchPacket(Out, true);
        }

        public void DispatchUpdateState(byte stateID, ushort val1)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(stateID);
            Out.Fill(0, 3);
            Out.WriteUInt16(val1);
            Out.Fill(0, 2);
            DispatchPacket(Out, true);
        }

        #endregion

        #region Mounting

        public virtual void Mount(ushort mountID)
        {
            MountID = mountID;
            SendMount();
        }

        protected void SendMount(Player Plr = null)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_MOUNT_UPDATE, 20);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(MountID);
            Out.WriteUInt16(MountArmor);
            Out.Fill(0, 14);

            if (Plr == null)
                DispatchPacket(Out, true);
            else
                Plr.SendPacket(Out);
        }

        public virtual void Dismount()
        {
            MountID = 0;

            PacketOut Out = new PacketOut((byte)Opcodes.F_MOUNT_UPDATE, 20);
            Out.WriteUInt16(Oid);
            Out.Fill(0, 18);

            DispatchPacket(Out, true);
        }

        #endregion

        #region CrowdControl

        /// <summary>Indicates that this unit cannot be knocked back or rooted.</summary>
        public bool IsImmovable
        {
            get
            {
                return _immovableCount > 0;
            }
            set
            {
                if (value) ++_immovableCount;
                else --_immovableCount;
            }
        }

        private byte _immovableCount;

        public bool NoKnockbacks { get; set; }

        /// <summary>Object to lock on when performing an action which might trigger Immovable.</summary>
        protected object MovementCCLock = new object();

        /// <summary>Active crowd control effects.</summary>
        public byte CrowdControlType { get; set; }
        public int CrowdControlBlock { get; protected set; }

        private readonly byte[] _crowdControlBlocks = new byte[5];

        public bool IsPolymorphed;

        public bool IsKeepLord = false;

		public bool IsPatrol = false;

		public uint WaypointGUID { get; set; } = 0;

        public void AddCrowdControlImmunity(int flags)
        {
            byte count = 0;

            while (flags > 0 && count < 5)
            {
                if ((flags & 1) > 0)
                {
                    if (_crowdControlBlocks[count] == 0)
                        CrowdControlBlock |= 1 << count;
                    ++_crowdControlBlocks[count];
                }
                ++count;
                flags = flags >> 1;
            }

            /*#if DEBUG
            Say("Add: CC Immunity: " + Convert.ToString(CrowdControlBlock, 2), SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
            #endif*/
        }

        public void RemoveCrowdControlImmunity(int flags)
        {
            byte count = 0;

            while (flags > 0 && count < 5)
            {
                if ((flags & 1) > 0)
                {
                    --_crowdControlBlocks[count];
                    if (_crowdControlBlocks[count] == 0)
                        CrowdControlBlock &= ~(1 << count);
                }
                ++count;
                flags = flags >> 1;
            }

            /*
            #if DEBUG
            Say("Remove: CC Immunity: " + Convert.ToString(CrowdControlBlock, 2), SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
            #endif
            */
        }

        public bool ImmuneToCC(int ccFlag, Unit caster, ushort abilityEntry)
        {
            if ((ccFlag & CrowdControlBlock) == 0)
                return false;

            if (caster == null)
                return true;

            NotifyImmune(caster, abilityEntry);

            return true;
        }

        protected void NotifyImmune(Unit caster, ushort abilityEntry)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

            Out.WriteUInt16(caster.Oid);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(abilityEntry);

            Out.WriteByte(0);
            Out.WriteByte((byte)CombatEvent.COMBATEVENT_IMMUNE);
            Out.WriteByte(5);

            Out.WriteByte(0);

            DispatchPacketUnreliable(Out, true, this);
        }

        /// <summary>Determines whether active crowd control effects would prevent this ability from casting.</summary>
        public bool BlockedByCC(AbilityConstants abCstInfo)
        {
            byte ccState = CrowdControlType;

            if (IsPolymorphed)
                return true;

            if (ccState == 0)
                return false;

            // Knockdown or Stagger
            if ((ccState & 48) > 0)
                return true;

            // Disarm
            if ((abCstInfo.AbilityType == AbilityType.Melee || abCstInfo.AbilityType == AbilityType.Ranged) && (ccState & 4) > 0)
                return true;

            // Silence
            if (abCstInfo.AbilityType == AbilityType.Verbal && (ccState & 8) > 0)
                return true;

            return false;
        }

        public virtual void ApplyKnockback(Unit caster, AbilityKnockbackInfo kbInfo)
        {

        }

        /// <summary>Indicates whether a unit is incapable of moving or acting.</summary>
        public bool IsDisabled => CrowdControlType > 0 && (Utils.HasFlag(CrowdControlType, (int)CrowdControlTypes.Disabled) || Utils.HasFlag(CrowdControlType, (int)CrowdControlTypes.Knockdown));

        /// <summary>Indicates whether a unit is capable of auto-attacking.</summary>
        public bool CanAutoAttack => CrowdControlType == 0 || !Utils.HasFlag(CrowdControlType, (int)CrowdControlTypes.NoAutoAttack);

        /// <summary>Indicates whether a unit is staggered.</summary>
        public bool IsStaggered => CrowdControlType > 0 && Utils.HasFlag(CrowdControlType, (int)CrowdControlTypes.Stagger);

        /// <summary>Regular root command. Will fail if the target is Immovable.</summary>
        public virtual bool TryRoot(NewBuff hostBuff)
        {
            if (IsImmovable)
                return false;
            lock (MovementCCLock)
            {
                if (IsImmovable)
                    return false;
                IsImmovable = true;
            }

            BuffInterface.QueueBuff(new BuffQueueInfo(this, EffectiveLevel, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Immovable)));
            StsInterface.AddVelocityModifier(hostBuff, 0);
            return true;
        }

        /// <summary>
        /// <para>Root command used for Champion's Challenge.</para>
        /// <para>Prevents either target from being punted or punting themselves.</para>
        /// </summary>
        public void EnterGrapple(NewBuff hostBuff, bool triggersImmunity)
        {
            lock (MovementCCLock)
            {
                if (!IsImmovable && !NoKnockbacks)
                {
                    if (triggersImmunity)
                    {
                        IsImmovable = true;
                        BuffInterface.QueueBuff(new BuffQueueInfo(this, EffectiveLevel, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Immovable)));
                    }
                }
                NoKnockbacks = true;
                StsInterface.AddVelocityModifier(hostBuff, 0);
            }
        }


        public void SetImmovable(bool state)
        {
            lock (MovementCCLock)
            {
                if (state)
                    ++_immovableCount;
                else
                    --_immovableCount;
            }
        }

        #endregion

        #region GFX

        private readonly List<Tuple<ushort, ushort>> _gfxModList = new List<Tuple<ushort, ushort>>();

        public void SendGfxMods()
        {
            SendGfxMods(null);
        }

        public void SendGfxMods(Player player)
        {
            if (_gfxModList.Count == 0)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_GFX_MOD);
            Out.WriteByte((byte)_gfxModList.Count);
            Out.WriteByte(0);
            Out.WriteUInt16(Oid);
            foreach (Tuple<ushort, ushort> mod in _gfxModList)
            {
                Out.WriteUInt16(mod.Item1);
                Out.WriteUInt16(mod.Item2);
            }

            Out.WriteUInt16(0);

            if (player != null)
                player.SendPacket(Out);
            else
                DispatchPacket(Out, false);
        }

        public void AddGfxMod(ushort oldMod, ushort newMod)
        {
            _gfxModList.Add(new Tuple<ushort, ushort>(oldMod, newMod));
        }

        #endregion
    }
}
