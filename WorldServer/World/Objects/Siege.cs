﻿using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Components;
using WorldServer.World.AI;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using CreatureSubTypes = GameData.CreatureSubTypes;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public class Siege : Creature
    {
//        public BattleFrontKeep Keep { get; }
        private readonly SiegeType _type;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public BattleFrontKeep AssignedKeep { get; set; }
        public const int MAX_SHOTS = 15;
        public int ShotCount = MAX_SHOTS;
        public long SiegeLifeSpan = (int) TimeSpan.FromMinutes(5).TotalMilliseconds;

        public Siege(Creature_spawn spawn, Player owner, SiegeType type, BattleFrontKeep keep=null) : base(spawn)
        {
            _type = type;
            SiegeInterface = AddInterface<SiegeInterface>();
            SiegeInterface.Creator = owner;
            AssignedKeep = keep;  // Only need to assign keep for oil
            SiegeInterface.DeathTime = TCPManager.GetTimeStampMS() + SiegeLifeSpan;
        }

        

        public static Siege SpawnSiegeWeapon(Player plr, ushort zoneId, uint entry, bool defender)
        {
            Creature_proto proto = CreatureService.GetCreatureProto(entry);

            Creature_spawn spawn = null;
            spawn = new Creature_spawn
            {
                Guid = (uint)CreatureService.GenerateCreatureSpawnGUID(),
                WorldO = plr.Heading,
                WorldY = plr.WorldPosition.Y,
                WorldZ = plr.Z,
                WorldX = plr.WorldPosition.X,
                ZoneId = zoneId,
                Level = 40
            };
            
            spawn.BuildFromProto(proto);

            return new Siege(spawn, plr, GetSiegeType(entry).Value);
        }

        protected override void SetCreatureStats()
        {
            Speed = 320;
            StsInterface.Speed = 320;
            MvtInterface.SetBaseSpeed(Speed);

            // Normal siege weapons will tank 5 cannon hits (4500, cannons hit for 1000)
            // Rams will tank 300 cannon hits (60000, cannons hit for 200)
            StsInterface.SetBaseStat(Stats.Wounds, _type == SiegeType.RAM ? (ushort)6000 : (ushort)900);
            StsInterface.ApplyStats();
        }

        public override void OnLoad()
        {
            InteractType = Spawn.Proto.InteractType;

            SetFaction(Spawn.Faction != 0 ? Spawn.Faction : Spawn.Proto.Faction);

            ItmInterface.Load(CreatureService.GetCreatureItems(Spawn.Entry));
            if (Spawn.Proto.MinLevel > Spawn.Proto.MaxLevel)
                Spawn.Proto.MinLevel = Spawn.Proto.MaxLevel;

            if (Spawn.Proto.MaxLevel <= Spawn.Proto.MinLevel)
                Spawn.Proto.MaxLevel = Spawn.Proto.MinLevel;

            if (Spawn.Proto.MaxLevel == 0) Spawn.Proto.MaxLevel = 1;
            if (Spawn.Proto.MinLevel == 0) Spawn.Proto.MinLevel = 1;

            if (Spawn.Level != 0)
            {
                if (Spawn.Level > 2)
                    Level = (byte)StaticRandom.Instance.Next(Spawn.Level - 1, Spawn.Level + 1);
                else
                    Level = (byte)StaticRandom.Instance.Next(Spawn.Level, Spawn.Level + 1);
            }
            else
                Level = (byte)StaticRandom.Instance.Next(Spawn.Proto.MinLevel, Spawn.Proto.MaxLevel + 1);

            SetCreatureStats();

            Health = TotalHealth;

            X = Zone.CalculPin((uint)Spawn.WorldX, true);
            Y = Zone.CalculPin((uint)Spawn.WorldY, false);
            Z = (ushort)Spawn.WorldZ;

            // TODO : Bad Height Formula
            /*int HeightMap = HeightMapMgr.GetHeight(Zone.ZoneId, X, Y);
            if (Z < HeightMap)
            {
                Log.Error("Creature", "["+Spawn.Entry+"] Invalid Height : Min=" + HeightMap + ",Z=" + Z);
                return;
            }*/

            Heading = (ushort)Spawn.WorldO;
            WorldPosition.X = Spawn.WorldX;
            WorldPosition.Y = Spawn.WorldY;
            WorldPosition.Z = Spawn.WorldZ;

            SetOffset((ushort)(Spawn.WorldX >> 12), (ushort)(Spawn.WorldY >> 12));
            ScrInterface.AddScript(Spawn.Proto.ScriptName);

            SaveSpawnData();
            LoadInterfaces();

            AiInterface.SetBrain(new DummyBrain(this));

            States.Add(0x12);
            States.Add((byte)CreatureState.UnkOmnipresent);

            Speed = 350;
            StsInterface.Speed = 350;
            MvtInterface.SetBaseSpeed(Speed);
            MvtInterface.FollowReacquisitionInterval = 100;

            IsActive = true;
        }

        protected override void SendCreateMonster(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_MONSTER, 110 + States.Count + (Name?.Length ?? Spawn.Proto.Name.Length));
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);
            Out.WriteUInt16(0); // Speed Z
            Out.WriteUInt16(Spawn.Proto.Model1);
            Out.WriteByte(Scale);

            Out.WriteByte(Level);
            Out.WriteByte(Faction);

            Out.WriteByte(0);
            Out.WriteByte(SiegeInterface != null && SiegeInterface.IsDeployed ? (byte)1 : (byte)0);
            Out.Fill(0, 2);
            Out.WriteByte(Spawn.Emote);
            Out.WriteByte(0); // ?
            Out.WriteUInt16(Spawn.Proto._Unks[1]);
            Out.WriteByte(0);
            Out.WriteUInt16(Spawn.Proto._Unks[2]);
            Out.WriteUInt16(Spawn.Proto._Unks[3]);
            Out.WriteUInt16(Spawn.Proto._Unks[4]);
            Out.WriteUInt16(Spawn.Proto._Unks[5]);
            Out.WriteUInt16(Spawn.Proto._Unks[6]);
            Out.WriteUInt16(Spawn.Proto.Title);

            Out.WriteByte((byte)States.Count);
            Out.Write(States.ToArray(), 0, States.Count);

            Out.WriteByte(0);

            Out.WriteCString(Name ?? Spawn.Proto.Name);

            // Fig leaf data

            if (Spawn.Proto.CreatureType == (byte)GameData.CreatureTypes.SIEGE)
            {
                Out.Fill(0, 3);
                Out.WriteByte(0x8); // Player-placed Siege
                Out.WriteByte(0x1);
                Out.WriteByte(0x0A);
            }
            // End fig leaf data

            Out.WriteByte(0);

            Out.WriteUInt16(0); // Owner OID

            // Need to write OBJECT_STATE length here
            long objStateLenPos = Out.Position;
            Out.WriteByte(0);
            Out.WriteByte(5); // F_PLAYER_INVENTORY length
            Out.WriteByte(0); // unk
            MvtInterface.WriteMovementState(Out);

            long latestPos = Out.Position;

            Out.Position = objStateLenPos;
            Out.WriteByte((byte)(latestPos - (objStateLenPos + 3)));
            Out.Position = latestPos;

            // F_PLAYER_INVENTORY
            Out.WriteUInt16(Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);

            plr.SendPacket(Out);
        }

        private long _nextAreaCheckTime;
        private long _nextDamageTime;

        public override void Update(long msTick)
        {
            base.Update(msTick);

            if (_nextAreaCheckTime < msTick)
            {
                _nextAreaCheckTime = msTick + 2000;

                Zone_Area newArea = Zone.ClientInfo.GetZoneAreaFor((ushort)X, (ushort)Y, Zone.ZoneId, (ushort)Z);

                if (newArea == null || !newArea.IsRvR)
                    OutOfAreaDecay();
                else if (_outofAreaCount > 0)
                    _outofAreaCount--;
            }

            // RB   6/25/2016   Kill siege weapons that have decayed and are not being interacted with.
            if (SiegeInterface.IsAbandoned && msTick > _nextDamageTime)
            {
                ReceiveDamage(this, MaxHealth / 10);
                _nextDamageTime = msTick + 20 * 1000;
            }

            if (msTick > SiegeInterface.DeathTime && msTick > _nextDamageTime)
            {
                ReceiveDamage(this, MaxHealth / 10);
                _nextDamageTime = msTick + 20 * 1000;
            }


        }

        private int _outofAreaCount;
        private void OutOfAreaDecay()
        {
            _outofAreaCount++;
            if (_outofAreaCount > 5)
            {
                SiegeInterface.RemoveAllPlayers();
                Destroy();
            }
        }

        // Azarael - Siege weapons don't resurrect
        protected override void SetRespawnTimer()
        {

        }

        #region Health/Damage

        // RB   5/20/2016   Siege weapons should not regenerate naturally.
        public override void UpdateHealth(long tick)
        {
        }

        // RB   5/21/2016   Damage Resistance for siege
        /// <summary>Inflicts damage upon this unit.</summary>
        public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1f, uint mitigation = 0)
        {
            bool wasKilled = false;
            Player creditedPlayer = null;

            if (IsDead || PendingDisposal || IsInvulnerable)
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

            CbtInterface.OnTakeDamage(caster, damage, 1f);
            Siege siege = caster as Siege;
            if (siege != null)
            {
                Siege s = siege;
                foreach (KeyValuePair<Player, byte> p in s.SiegeInterface.Players)
                    p.Key.CbtInterface.OnDealDamage(this, damage);
            }
            else
                caster.CbtInterface.OnDealDamage(this, damage);

            LastHitOid = caster.Oid;

            if (wasKilled)
                SetDeath(caster);

            return wasKilled;
        }

        protected override void SetDeath(Unit killer)
        {
            base.SetDeath(killer);

            Pet pet = killer as Pet;
            Player credited = (pet != null) ? pet.Owner : (killer as Player);

            if (killer is Player)
                (killer as Player).SendClientMessage($"{(killer as Player).Name} has killed a siege item!");


            if (this.SiegeInterface.Creator != null)
            {
                this.SiegeInterface.Creator.SendClientMessage($"Your siege has been destroyed!");
            }

            if (credited != null)
            {
                // Contribution for Siege kill
                credited.UpdatePlayerBountyEvent((byte)ContributionDefinitions.DESTROY_SIEGE);
                HandleXPRenown(credited);
            }

            SiegeInterface.RemoveAllPlayers();

            Health = 0;

            States.Add((byte)CreatureState.Dead); // Death State

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(killer.IsPet() ? killer.GetPet().Owner.Oid : killer.Oid);
            Out.Fill(0, 6);
            DispatchPacket(Out, true);

            switch (_type)
            {
                case SiegeType.OIL:
                    EvtInterface.AddEvent(Destroy, 180000, 1); // Oil is replacable after 180 seconds
                    break;
                case SiegeType.GTAOE:
                case SiegeType.SNIPER:
                    EvtInterface.AddEvent(Destroy, 30000, 1);
                    break;
                case SiegeType.RAM:
                    EvtInterface.AddEvent(Destroy, 30000, 1);
                    break;
            }

        }

        public override bool ShouldDefend(Unit attacker, AbilityDamageInfo incDamage)
        {
            if (attacker is Pet)
                return true;

            CreatureSubTypes subType = (CreatureSubTypes)Spawn.Proto.CreatureSubType;

            switch (incDamage.SubDamageType)
            {
                case SubDamageTypes.Oil:
                    return true;
                case SubDamageTypes.None:
                    if (subType == CreatureSubTypes.SIEGE_RAM)
                        return incDamage.StatUsed != 1;
                    return incDamage.DamageType == DamageTypes.Physical && incDamage.StatUsed != 1;
            }

            return false;
        }

        /// <summary>
        /// Reduces the damage a siege weapon takes from regular types of attack.
        /// </summary>
        /// <param name="incDamage"></param>
        public override void ModifyDamageIn(AbilityDamageInfo incDamage)
        {
            CreatureSubTypes subType = (CreatureSubTypes)Spawn.Proto.CreatureSubType;

            if (subType == CreatureSubTypes.SIEGE_RAM)
            {
                switch (incDamage.SubDamageType)
                {
                    case SubDamageTypes.Cleave:
                        incDamage.DamageReduction = 0.15f;
                        break;
                    case SubDamageTypes.Artillery:
                        incDamage.Mitigation = incDamage.Damage * 0.975f;
                        incDamage.Damage *= 0.025f;
                        break;
                    case SubDamageTypes.Oil:
                        incDamage.Damage = 0;
                        incDamage.PrecalcDamage = 0;
                        incDamage.DamageReduction = 0f;
                        incDamage.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                        break;
                    case SubDamageTypes.Cannon:
                        if (SiegeInterface.IsDeployed)
                        {
                            incDamage.Mitigation = incDamage.Damage * 0.95f;
                            incDamage.Damage *= 0.05f;
                        }
                        else
                        {
                            incDamage.Mitigation = incDamage.Damage * 0.75f;
                            incDamage.Damage *= 0.25f;
                        }
                        break;
                    case SubDamageTypes.None:
                        switch (incDamage.DamageType)
                        {
                            case DamageTypes.Physical:
                                if (incDamage.StatUsed != 1)
                                {
                                    incDamage.Damage = 0;
                                    incDamage.PrecalcDamage = 0;
                                    incDamage.DamageReduction = 0f;
                                    incDamage.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                                }
                                else
                                    incDamage.DamageReduction *= 0.066f;
                                break;
                            case DamageTypes.RawDamage:
                                incDamage.Mitigation = incDamage.Damage * 0.99f;
                                incDamage.Damage *= 0.01f;
                                incDamage.PrecalcMitigation = incDamage.PrecalcDamage * 0.99f;
                                incDamage.PrecalcDamage *= 0.01f;
                                break;
                            default:
                                incDamage.DamageReduction *= 0.005f;
                                break;
                        }
                        break;
                }
            }

            else switch (incDamage.SubDamageType)
                {
                    case SubDamageTypes.Cleave:
                        incDamage.DamageReduction *= 0.8f;
                        break;
                    case SubDamageTypes.Artillery:
                        incDamage.Mitigation = incDamage.Damage * 0.95f;
                        incDamage.Damage *= 0.05f;
                        break;
                    case SubDamageTypes.Oil:
                        incDamage.Damage = 0;
                        incDamage.PrecalcDamage = 0;
                        incDamage.DamageReduction = 0f;
                        incDamage.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                        break;
                    case SubDamageTypes.Cannon:
                        incDamage.Mitigation = incDamage.Damage * 0.5f;
                        incDamage.Damage *= 0.5f;
                        break;
                    case SubDamageTypes.None:
                        switch (incDamage.DamageType)
                        {
                            case DamageTypes.Physical:
                                if (incDamage.StatUsed == 8)
                                {
                                    incDamage.Damage = 0;
                                    incDamage.PrecalcDamage = 0;
                                    incDamage.DamageReduction = 0f;
                                    incDamage.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                                }
                                else
                                    incDamage.DamageReduction *= 0.4f;
                                break;
                            case DamageTypes.RawDamage:
                                incDamage.Mitigation = incDamage.Damage * 0.5f;
                                incDamage.Damage *= 0.5f;
                                break;
                            default:
                                incDamage.DamageReduction *= 0.05f;
                                break;
                        }
                        break;
                }
        }

        private uint _damageOut;

        /// <summary>
        /// Save the damage a siege weapon inflicts as a measure of contribution.
        /// </summary>
        public override void ModifyDamageOut(AbilityDamageInfo outDamage)
        {
            _damageOut += outDamage.GetDamageForLevel(10) / 100;
            base.ModifyDamageOut(outDamage);
        }

        #endregion

        public void AddShots(int shots)
        {
            ShotCount = Math.Min(ShotCount + shots, MAX_SHOTS);
            SiegeInterface.Creator?.SendClientMessage($"Your siege weapon has been resupplied with {shots} {(ShotCount == 1 ? "shot" : "shots")}", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
        }

        public bool CanFire(Player player)
        {
            if (ShotCount == 0)
            {
                player.SendClientMessage("No ammunition remains in this siege weapon - resupply your keep", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return false;
            }

            --ShotCount;

            player.SendClientMessage($"{ShotCount} {(ShotCount == 1 ? "shot remains" : "shots remain")} for this siege weapon", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
            return true;
        }

        /// <summary>
        /// Grants XP, Renown, Influence, ToK kill incrementation and kill contribution credit to all players inflicting damage.
        /// </summary>
        private void HandleXPRenown(Player killer)
        {
            Dictionary<Group, XpRenown> groupXPRenown = new Dictionary<Group, XpRenown>();

            #region Initialize reward values

            uint totalXP = (uint)(500 * Region.GetTier() * Math.Min(6f, _damageOut / (float)100) * (1f + killer.AAOBonus));
            uint totalRenown = (uint)(100 * Region.GetTier() * Math.Min(6f, _damageOut / (float)100) * (1f + killer.AAOBonus));

            ushort influenceId = 0;
            uint totalInfluence = 0;

            if (killer.CurrentArea != null && killer.CurrentArea.IsRvR)
            {
                influenceId = (killer.Realm == Realms.REALMS_REALM_DESTRUCTION) ? (ushort)killer.CurrentArea.DestroInfluenceId : (ushort)killer.CurrentArea.OrderInfluenceId;
                totalInfluence = (uint)(100 * (1f + killer.AAOBonus));
            }

            #endregion

            #region Remove players irrelevant to the kill

            RemoveDistantDamageSources();

            if (DamageSources.Count == 0 || TotalDamageTaken == 0)
                return;

            #endregion

            foreach (KeyValuePair<Player, uint> kvpair in DamageSources)
            {
                #region Get reward values for this player
                Player curPlayer = kvpair.Key;

                float damageFactor = (float)kvpair.Value / TotalDamageTaken;

                uint xpShare = (uint)(totalXP * damageFactor);
                uint renownShare = (uint)(totalRenown * damageFactor);
                ushort influenceShare = (ushort)(totalInfluence * damageFactor);

                if (damageFactor >= 2f)
                {
                    curPlayer.SendClientMessage("You dealt " + kvpair.Value + " damage to " + Name + ", but they only took " + TotalDamageTaken + " total damage. No rewards have been given.");
                    continue;
                }

                #endregion

                if (curPlayer.PriorityGroup == null)
                {
                    #region Handle solo rewards
                    if (curPlayer.ScnInterface.Scenario == null || !curPlayer.ScnInterface.Scenario.DeferKillReward(curPlayer, xpShare, renownShare))
                    {
                        curPlayer.AddXp(xpShare, true, true);
                        curPlayer.AddRenown(renownShare, true);
                        if (influenceId != 0)
                            curPlayer.AddInfluence(influenceId, influenceShare);

                    }

                    curPlayer.EvtInterface.Notify(EventName.OnKill, killer, null);
                    curPlayer._Value.RVRKills++;
                    curPlayer.SendRVRStats();
                    #endregion
                }

                else
                {
                    #region Collate group rewards
                    if (groupXPRenown.ContainsKey(kvpair.Key.PriorityGroup))
                        groupXPRenown[kvpair.Key.PriorityGroup].Add(xpShare, renownShare, influenceShare);
                    else
                        groupXPRenown.Add(kvpair.Key.PriorityGroup, new XpRenown(xpShare, renownShare, influenceId, influenceShare, TCPManager.GetTimeStamp()));
                    #endregion
                }
            }

            #region Apply group rewards
            if (groupXPRenown.Count > 0)
            {
                foreach (KeyValuePair<Group, XpRenown> kvpair in groupXPRenown)
                    kvpair.Key.HandleKillRewards(this, killer, 1, kvpair.Value.XP, kvpair.Value.Renown, influenceId, kvpair.Value.Influence, 1, null);
            }
            #endregion
        }

        public override void Destroy()
        {
            //_logger.Debug($"Destroying Siege {this.Name}. SiegeManager : {this.Region.Campaign.SiegeManager.ToString()}");
            try
            {
                if (this.SiegeInterface == null)
                    _logger.Debug($"SiegeIntf null");

                if (this.SiegeInterface.Creator == null)
                    _logger.Debug($"SiegeIntf.Creator null");

                if (this.Region == null)
                    _logger.Debug($"this.Region null");

                if (this.Region.Campaign == null)
                    _logger.Debug($"this.Region.Campaign null");

                if (this.Region.Campaign.SiegeManager == null)
                    _logger.Debug($"this.Region.Campaign.SiegeManager null");

                if (this.AssignedKeep != null)
                {
                    foreach (var h in this.AssignedKeep.HardPoints)
                    {
                        if (h.CurrentWeapon == this)
                        {
                            h.CurrentWeapon = null;
                        }
                    }
                }

                this.Region.Campaign?.SiegeManager?.Remove(this, this.SiegeInterface.Creator.Realm);
            }
            catch (Exception e)
            {
                _logger.Debug($"{e.Message}{e.StackTrace}");
            }
            
            PendingDisposal = true;
        }

        public override void Dispose()
        {
            SiegeInterface.RemoveAllPlayers();

            base.Dispose();
        }

        public static SiegeType? GetSiegeType(uint primaryValue)
        {
           var siegeProto =  CreatureService.GetCreatureProto(primaryValue);
            if (siegeProto == null)
                return null;

            SiegeType siegeType;


            switch ((GameData.CreatureSubTypes)siegeProto.CreatureSubType)
            {
                case GameData.CreatureSubTypes.SIEGE_GTAOE:
                    siegeType = SiegeType.GTAOE;
                    break;
                case GameData.CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    siegeType = SiegeType.SNIPER;
                    break;
                case GameData.CreatureSubTypes.SIEGE_RAM:
                    siegeType = SiegeType.RAM;
                    break;
                default:
                    siegeType = SiegeType.RAM;
                    break;
            }

            return siegeType;
        }
    }
}
