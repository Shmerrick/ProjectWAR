//#define DIST_DEBUG

using System;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.NetWork;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Abilities.Components;
using WorldServer.World.AI;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public class Pet : Creature
    {
        public enum PetUpdateType
        {
            Remove,
            Update,
            Initialize,
            AddAbility,
            RemoveAbility,
            Unk1,
            Unk2
        }

        public readonly ushort PetId;
        public Player Owner { get; }

        public bool IsStationary { get; }

        private bool _ownerUILinked;

        public byte AIMode;
        public byte FollowMode;

        public bool IsHeeling;

        public float SpeedMult = 1f;
        public static long COMMAND_ATTACK_REUSE = 500;
        public long AttackReuseTimer = TCPManager.GetTimeStampMS();

        private WeaponDamageContribution _weaponDamageContribution;
        private float _weaponDamageFactor = 1f;

        private bool _ignoreZ;

        public bool IsVanity = false;

        public Pet(ushort petId, Creature_spawn spawn, Player owner, byte aiMode, bool isStationary, bool isCombative) : base (spawn)
        {
            PetId = petId;
            Owner = owner;
            IsStationary = isStationary;
            if (!isStationary)
                FollowMode = 2;
            AIMode = aiMode;
            _ownerUILinked = isCombative;

            if (!isCombative)
                IsInvulnerable = true;

            else
                SpeedMult = 1.2f;

            switch (aiMode)
            {
                case 3: AiInterface.SetBrain(new PassiveBrain(this)); break;
                case 4: AiInterface.SetBrain(new GuardBrain(this)); break;
                case 5: AiInterface.SetBrain(new AggressiveBrain(this)); break; 
            }
            Realm = owner.Realm;
            Faction = (byte)(owner.Realm == Realms.REALMS_REALM_DESTRUCTION ? 8 : 6);

            Owner.SendStats();

            Health = 1;

            EvtInterface.AddEvent(SendPetInitial, 500, 1);
        }

        public void RemoveVanityPet()
        {
            Owner.Companion.Dismiss(null, null);
            Owner.Companion = null;

            NewBuff vanityPet = Owner.BuffInterface.GetBuff(15188,null);
            if (vanityPet != null)
                Owner.BuffInterface.RemoveBuffByEntry(15188);
            vanityPet = Owner.BuffInterface.GetBuff(15190, null);
            if (vanityPet != null)
                Owner.BuffInterface.RemoveBuffByEntry(15188);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            if (!IsStationary)
            {
                Recall();
                Owner.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, Recall);
            }

            Owner.EvtInterface.AddEventNotify(EventName.OnDealDamage, Attack);
            Owner.EvtInterface.AddEventNotify(EventName.OnDie, Dismiss);
            Owner.EvtInterface.AddEventNotify(EventName.OnRemoveFromWorld, Dismiss);

			#if DIST_DEBUG && DEBUG
            Owner.EvtInterface.AddEvent(DistanceDebug, 1000, 0);
			#endif
        }
        protected override void SetCreatureStats()
        {
            if (Owner.Info.CareerLine == (int)CareerLine.CAREERLINE_MAGUS)
                StsInterface.Load(CharMgr.GetCharacterInfoStats((byte) CareerLine.CAREERLINE_MAGUS, Math.Min(Level, (byte) 40)));
            float armorMod = 1.0f, resistMod = 1.0f, woundsMod = 1f, itemWoundsMod = 0f;

            switch (Spawn.Proto.Model1)
            {
                // White Lion
                // War Lions
                case 132:
                case 133:
                case 134:
                case 135:
                    StsInterface.Load(CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_WAR_LION, Math.Min(Level, (byte)40)));
                    woundsMod = 1f;
                    itemWoundsMod = 1f;
                    _weaponDamageContribution = WeaponDamageContribution.MainHand;
                    break;

                // Beastmaster
                case 1156: // War Manticore
                case 1086: // Harpy
                case 1142: // Giant Scorpion
                case 1272: // Hydra
                    StsInterface.Load(CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_WAR_MANTICORE, Math.Min(Level, (byte)40)));
                    woundsMod = 1f;
                    itemWoundsMod = 1f;
                    _weaponDamageContribution = WeaponDamageContribution.MainHand;
                    break;

                // Squig
                case 136:
                    StsInterface.Load(CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_SQUIG, Math.Min(Level, (byte)40)));
                    armorMod = 2.0f;
                    woundsMod = 1f;
                    itemWoundsMod = 0.5f;
                    _weaponDamageContribution = WeaponDamageContribution.MainHand;
                    break; 
                // Horned Squig
                case 137:
                    StsInterface.Load(CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_HORNED_SQUIG, Math.Min(Level, (byte)40)));
                    armorMod = 2.0f;
                    resistMod = 0.5f;
                    woundsMod = 1f;
                    itemWoundsMod = 1f;
                    _weaponDamageContribution = WeaponDamageContribution.MainAndRanged;
                    break;
                // Spiked Squig
                case 138:
                    StsInterface.Load(CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_SPIKED_SQUIG, Math.Min(Level, (byte)40)));
                    woundsMod = 0.85f;
                    itemWoundsMod = 0.85f;
                    _weaponDamageContribution = WeaponDamageContribution.Ranged;
                    _ignoreZ = true;
                    break;
                // Gas Squig
                case 139:
                    StsInterface.Load(CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_GAS_SQUIG, Math.Min(Level, (byte)40)));
                    woundsMod = 0.65f;
                    itemWoundsMod = 0.65f;
                    _weaponDamageContribution = WeaponDamageContribution.Ranged;
                    _ignoreZ = true;
                    break;

                // Magus
                // Pink Horror
                case 141:
                    woundsMod = 0.5f;
                    _weaponDamageContribution = WeaponDamageContribution.MainHand;
                    _weaponDamageFactor = 0.7f;
                    break;
                // Blue Horror 
                case 142:
                    armorMod = 2f;
                    woundsMod = 1f;
                    _weaponDamageContribution = WeaponDamageContribution.MainHand;
                    _weaponDamageFactor = 0.85f;
                    break;
                // Flamer
                case 143:
                    armorMod = 1.5f;
                    woundsMod = 0.75f;
                    _weaponDamageContribution = WeaponDamageContribution.MainHand;
                    break;

                // Engineer
                // Gun Turret
                case 145:
                    woundsMod = 0.5f;
                    _weaponDamageContribution = WeaponDamageContribution.MainAndRanged;
                    _weaponDamageFactor = 0.7f;
                    break;
                // Bombardment Turret
                case 146:
                    armorMod = 1.5f;
                    woundsMod = 0.75f;
                    _weaponDamageContribution = WeaponDamageContribution.MainAndRanged;
                    _weaponDamageFactor = 0.85f;
                    break;
                // Flame Turret
                case 147:
                    armorMod = 2f;
                    woundsMod = 1f;
                    _weaponDamageContribution = WeaponDamageContribution.MainAndRanged;
                    break;

                // This is to handle .setpet command, it sets the stats of Lion / Manticore
                default:
                    StsInterface.Load(CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_WAR_MANTICORE, Math.Min(Level, (byte)40)));
                    woundsMod = 1f;
                    itemWoundsMod = 1f;
                    _weaponDamageContribution = WeaponDamageContribution.MainHand;
                    break;
            }

            StsInterface.SetBaseStat(Stats.Wounds, (ushort)(Owner.StsInterface.GetCoreStat(Stats.Wounds) * woundsMod));

            StsInterface.AddItemBonusStat(Stats.Willpower, Owner.StsInterface.GetItemStat(Stats.Willpower));

            if (itemWoundsMod > 0f)
                StsInterface.AddItemBonusStat(Stats.Wounds, (ushort)(Owner.StsInterface.GetItemStat(Stats.Wounds) * itemWoundsMod));

            StsInterface.AddItemBonusStat(Stats.Initiative, Owner.StsInterface.GetItemStat(Stats.Initiative));

            StsInterface.AddItemBonusStat(Stats.Armor, (ushort)(Owner.StsInterface.GetItemStat(Stats.Armor) * armorMod));
            StsInterface.AddItemBonusStat(Stats.SpiritResistance, (ushort)(Owner.StsInterface.GetItemStat(Stats.SpiritResistance) * resistMod));
            StsInterface.AddItemBonusStat(Stats.ElementalResistance, (ushort)(Owner.StsInterface.GetItemStat(Stats.ElementalResistance) * resistMod));
            StsInterface.AddItemBonusStat(Stats.CorporealResistance, (ushort)(Owner.StsInterface.GetItemStat(Stats.CorporealResistance) * resistMod));

            StsInterface.ApplyStats();
        }

        public override ushort GetStrikeDamage()
        {
            if (Owner != null)
                /*if(Owner.Info.CareerLine == (int)CareerLine.CAREERLINE_WHITELION)
                    return (ushort)(Owner.ItmInterface.GetWeaponDamage(_weaponDamageContribution) * 2.5f * _weaponDamageFactor);
                else*/
                    return (ushort)(Owner.ItmInterface.GetWeaponDamage(_weaponDamageContribution) * 10f * _weaponDamageFactor);

            if (Spawn.Proto.Ranged <= 30)
                return (ushort)(19f * Level); // 76 DPS at rank 40
            return (ushort)(12f * Level); // 48 DPS at rank 40
        }

        public void DistanceDebug()
        {
            Owner?.SendClientMessage("Your pet is now " + Owner.GetDistanceToObject(this) + " feet away from you.");
        }

#region Orders

        public bool Attack(Object obj, object args)
        {
            CombatInterface_Npc cb = CbtInterface as CombatInterface_Npc;
            if (cb == null)
            {
                Log.Error("Pet", "Missing CombatInterface in Attack()");
                return false;
            }

            Unit attacker = obj as Unit;

            if (attacker == null)
            {
                Log.Error("Pet", "Object is NULL in Attack()");
                return false;
            }

            if (AIMode != 3 && cb.CurrentTarget == null)
                AiInterface.ProcessCombatStart(attacker);

            return false;
        }

        public bool Recall(Object obj, object args)
        {
            if (!IsDead && !IsStationary && !PendingDisposal)
            {
                if (Zone == null)
                {
                    Log.Error("Pet", $"{Name} owned by {Owner.Name}: Not in a zone - IsDisposed: {IsDisposed}\n{Environment.StackTrace}");
                    return true;
                }
                MvtInterface.ScaleSpeed(Owner.MountID != 0 ? 1.5f : SpeedMult);
                MvtInterface.Recall(Owner);

                FollowMode = 2;
                IsHeeling = true;
                AiInterface.Debugger?.SendClientMessage("[MR]: Ignoring enemies during heel.");
                SendPetUpdate();
            }

            return false;
        }

        public bool Recall()
        {
            if (!IsDead && !IsStationary && !PendingDisposal)
            {
                MvtInterface.ScaleSpeed(Owner.MountID != 0 ? 1.5f : SpeedMult);
                MvtInterface.Recall(Owner);

                FollowMode = 2;
                IsHeeling = true;
                AiInterface.Debugger?.SendClientMessage("[MR]: Ignoring enemies during heel.");
                SendPetUpdate();
            }

            return false;
        }

        public bool Dismiss(Object obj, object args)
        {
            Destroy();
            return false;
        }

#endregion

#region Death/Rez

        public override void SendMeTo(Player plr)
        {
            base.SendMeTo(plr);

            if (plr == Owner && _ownerUILinked && !IsDead)
                EvtInterface.AddEvent(SendPetInitial, 500, 1);
        }

        public override void SendRemove(Player plr)
        {
            if (_ownerUILinked && (plr == null || plr == Owner))
                SendPetRemove(false);

            PacketOut Out = new PacketOut((byte)Opcodes.F_REMOVE_PLAYER, 4);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            if (plr != null)
                plr.SendPacket(Out);
            else
                DispatchPacket(Out, false);
        }

        public override void RezUnit()
        {
            Destroy();
        }

        protected override void SetDeath(Unit killer)
        {
            Health = 0;

            States.Add((byte)CreatureState.Dead); // Death State

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(killer.IsPet() ? killer.GetPet().Owner.Oid : killer.Oid);
            Out.Fill(0, 6);
            DispatchPacket(Out, true);

            AbtInterface.Cancel(true);
            ScrInterface.OnDie(this);

            BuffInterface.RemoveBuffsOnDeath();

            EvtInterface.Notify(EventName.OnDie, this, killer);

            AiInterface.ProcessCombatEnd();

            EvtInterface.AddEvent(RezUnit, 10000, 1); // Clear the object in 10 seconds.

            if (_ownerUILinked)
            {
                SendPetRemove(true);

                IPetCareerInterface petInterface = Owner.CrrInterface as IPetCareerInterface;

                petInterface?.Notify_PetDown();

                _ownerUILinked = false;
            }
        }

        public override void Destroy()
        {
            if (!PendingDisposal)
            {
                PendingDisposal = true;

                if (_ownerUILinked)
                {
                    IPetCareerInterface petInterface = Owner.CrrInterface as IPetCareerInterface;

                    petInterface?.Notify_PetDown();

                    _ownerUILinked = false;
                }
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            AbtInterface.NPCAbilities.Clear();

            // Remove any owner events before disposing
            Owner.EvtInterface.RemoveEventNotify(EventName.OnDealDamage, Attack);
            Owner.EvtInterface.RemoveEventNotify(EventName.OnLeaveCombat, Recall);
            Owner.EvtInterface.RemoveEventNotify(EventName.OnDie, Dismiss);
            Owner.EvtInterface.RemoveEventNotify(EventName.OnRemoveFromWorld, Dismiss);
#if DIST_DEBUG && DEBUG
            Owner.EvtInterface.RemoveEvent(DistanceDebug);
#endif

            if (_ownerUILinked)
            {
                IPetCareerInterface petInterface = Owner.CrrInterface as IPetCareerInterface;

                petInterface?.Notify_PetDown();

                _ownerUILinked = false;
            }

            base.Dispose();
        }

#endregion

#region Send/Update

        protected override void SendCreateMonster(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_MONSTER, 100 + Name.Length);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);
            Out.WriteUInt16(0); // Speed Z
            // 18
            if (Spawn.Proto.Model2 != 0)
                Out.WriteUInt16(StaticRandom.Instance.Next(0, 100) < 50 ? Spawn.Proto.Model1 : Spawn.Proto.Model2);
            else
                Out.WriteUInt16(Spawn.Proto.Model1);

            Out.WriteByte(Scale);

            Out.WriteByte(Level);
            Out.WriteByte(Faction);

            Out.Fill(0, 4);
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

            // States region
            Out.WriteByte(1);
            Out.WriteByte(0x19);
            Out.WriteByte(0);

            // 47

            Out.WriteCString(Name);

            Out.WriteUInt16(0x010A); // Fig leaf data
            Out.WriteByte(0);
            Out.WriteUInt16(Owner.Oid);
            
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

        public void SendPetInitial()
        {
            if (!_ownerUILinked || Owner == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PET_INFO, 12);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY)?.Oid ?? 0); // Pet target?
            Out.WriteUInt16((ushort)AbtInterface.NPCAbilities.Count);
            Out.WriteByte(FollowMode);
            Out.WriteByte(AIMode);
            Out.WriteByte((byte)PetUpdateType.Initialize);
            foreach (NPCAbility ab in AbtInterface.NPCAbilities)
            {
                Out.WriteUInt16(ab.Entry);
                Out.WriteByte(ab.AutoUse ? (byte)1 : (byte)0);
            }
            Out.WriteByte(0);
            Out.WriteByte(0);
            Owner.SendPacket(Out);
        }

        public void SendPetUpdate()
        {
            if (!_ownerUILinked || Owner == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PET_INFO, 12);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY)?.Oid ?? 0);
            Out.WriteUInt16(0);
            Out.WriteByte(FollowMode);
            Out.WriteByte(AIMode);
            Out.WriteByte((byte)PetUpdateType.Update);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Owner.SendPacket(Out);
        }

        /// <summary>
        /// Sends pet's death to client.
        /// </summary>
        /// <param name="death">True if removal was caused by death</param>
        public void SendPetRemove(bool death)
        {
            if (!CbtInterface.IsAttacking && Owner != null
                && _ownerUILinked && !IsStationary && !death)
            {
                IPetCareerInterface petCareer = Owner.CrrInterface as IPetCareerInterface;
                if (petCareer != null)
                    petCareer.SummonPet(PetId);
            }
            if (!_ownerUILinked || Owner == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_PET_INFO, 12);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte((byte)PetUpdateType.Remove);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Owner.SendPacket(Out);
        }

        public void AddAbilities(ushort minBound, ushort maxBound)
        {
            if (maxBound == 0)
            {
                AbilityInfo abInfo = AbilityMgr.GetAbilityInfo(minBound);
                if (abInfo != null && abInfo.ConstantInfo.MinimumRank <= Level)
                {
                    NPCAbility npcAbility = new NPCAbility(abInfo.Entry, abInfo.ConstantInfo.AIRange, (byte) abInfo.Cooldown, true, "");
                    AbtInterface.NPCAbilities.Add(npcAbility);
                    SendPetAbility(npcAbility);
                }
            }

            else
            {
                for (ushort i = minBound; i <= maxBound; ++i)
                {
                    AbilityInfo abInfo = AbilityMgr.GetAbilityInfo(i);
                    if (abInfo == null || abInfo.ConstantInfo.MinimumRank > Level)
                        continue;
                    NPCAbility npcAbility = new NPCAbility(abInfo.Entry, abInfo.ConstantInfo.AIRange, (byte) abInfo.Cooldown, true, "");
                    AbtInterface.NPCAbilities.Add(npcAbility);
                    SendPetAbility(npcAbility);
                }
            }
        }

        public void RemoveAbilities()
        {
            AttemptClear();
            if (AbtInterface.NPCAbilities.Count != 0)
                AbtInterface.NPCAbilities.Clear();
        }

        public void AttemptClear()
        {
            foreach (NPCAbility ab in AbtInterface.NPCAbilities)
            {
                PacketOut Out = new PacketOut((byte) Opcodes.F_PET_INFO, 12);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY)?.Oid ?? 0); // Pet target?
                Out.WriteUInt16(ab.Entry);
                Out.WriteByte(FollowMode); // 1 stay 2= heel
                Out.WriteByte(AIMode); // 3 passive // 4 guard // 5 Aggressive
                Out.WriteByte((byte)PetUpdateType.RemoveAbility);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Owner.SendPacket(Out);
            }
        }

        public void SendPetAbility(NPCAbility ability)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PET_INFO, 14);
            Out.WriteUInt16(Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(1);
            Out.WriteByte(FollowMode);
            Out.WriteByte(AIMode);
            Out.WriteByte((byte)PetUpdateType.AddAbility);
            Out.WriteUInt16(ability.Entry);
            Out.WriteByte((byte)(ability.AutoUse ? 1 : 0));
            Out.WriteByte(0);
            Out.WriteByte(0);
            Owner.SendPacket(Out);
        }

#endregion

#region Teleport

        public void SafePinTeleport(ushort pinX, ushort pinY, ushort pinZ, ushort worldO)
        {
            if (pinX == 0 || pinY == 0)
                return;

            Point3D world = ZoneService.GetWorldPosition(Zone.Info, pinX, pinY, pinZ);
            SafeWorldTeleport((uint)world.X, (uint)world.Y, (ushort)world.Z, worldO);
        }

        public void SafeWorldTeleport(uint worldX, uint worldY, ushort worldZ, ushort worldO)
        {
            if (worldX == 0 || worldY == 0)
                return;
           
            X = Zone.CalculPin(worldX, true);
            Y = Zone.CalculPin(worldY, true);
            SetPosition((ushort)X, (ushort)Y, worldZ, worldO, Zone.ZoneId);
        }

#endregion

        /*
        0000
        0017
        211B
        0000
        211B
        01
        01
        0000000000000000*/
        public override int GetAbilityRangeTo(Unit caster)
        {
            if (_ignoreZ)
                return Get2DDistanceToObject(caster, true);

            return GetDistanceToObject(caster, true);
        }

        // Auto Attack Override
        public override void SendAttackMovement(Unit target)
        {
            switch (Spawn.Proto.Model1)
            {
                case 145:
                    break;
                case 146:
                    break;
                case 147:
                    break;
                case 141: // pink
                    break;
                case 142: // blue
                    break;
                case 143: // flamer
                    break;
                default:
                    base.SendAttackMovement(target); return;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);
            Out.WriteUInt16(0);
            switch (Spawn.Proto.Model1)
            {
                case 145:
                    Out.WriteUInt16(23); break;
                case 146:
                    Out.WriteUInt16(26); break;
                case 147:
                    Out.WriteUInt16(29); break;
                case 141: // pink
                    Out.WriteUInt16(380); break;
                case 142: // blue
                    Out.WriteUInt16(382); break;
                case 143: // flamer
                    Out.WriteUInt16(381); break;
                default:
                    Out.WriteUInt16(Oid); break;
            }
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(1);
            Out.WriteHexStringBytes("0000000000000000");
            DispatchPacket(Out, true);

            Out = new PacketOut((byte)Opcodes.F_USE_ABILITY, 20);
            Out.WriteUInt16(0);
            switch (Spawn.Proto.Model1)
            {
                case 145:
                    Out.WriteUInt16(23); break;
                case 146:
                    Out.WriteUInt16(26); break;
                case 147:
                    Out.WriteUInt16(29); break;
                case 141: // pink
                    Out.WriteUInt16(380); break;
                case 142: // blue
                    Out.WriteUInt16(382); break;
                case 143: // flamer
                    Out.WriteUInt16(381); break;
                default:
                    Out.WriteUInt16(Oid); break;
            }
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            Out.WriteUInt16(Oid);
            Out.WriteByte(2);
            Out.WriteByte(1);
            Out.WriteHexStringBytes("0000000000000000");
            DispatchPacket(Out, true);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_COMMAND_CONTROLLED, (int)eClientState.Playing, "F_COMMAND_CONTROLLED")]
        public static void F_COMMAND_CONTROLLED(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;

            if (cclient.Plr?.CrrInterface == null)
                return;

            IPetCareerInterface petInterface = cclient.Plr.CrrInterface as IPetCareerInterface;

            Pet myPet = petInterface?.myPet;

            if (myPet == null)
                return;

            ushort abilityid = packet.GetUint16();
            PetCommand command = (PetCommand)packet.GetUint8();

            switch (command)
            {
                case PetCommand.Stay:
                    if (cclient.Plr.IsMounted)
                        return;
                    myPet.MvtInterface.StopMove();
                    myPet.FollowMode = 1;
                    myPet.IsHeeling = false;
                    myPet.AiInterface.Debugger?.SendClientMessage("[MR]: Holding position by request.");
                    myPet.SendPetUpdate();
                    break; // stay
                case PetCommand.Follow:
                    if (cclient.Plr.IsMounted)
                        return;
                    myPet.AiInterface.ProcessCombatEnd();
                    if (myPet.StsInterface.Speed == 0)
                        break;
                    myPet.MvtInterface.ScaleSpeed(myPet.SpeedMult);
                    myPet.MvtInterface.Recall(cclient.Plr);
                    myPet.FollowMode = 2;
                    myPet.AiInterface.Debugger?.SendClientMessage("[MR]: Heeling by request.");
                    myPet.SendPetUpdate();
                    break; // heel
                case PetCommand.Passive:
                    myPet.AiInterface.SetBrain(new PassiveBrain(myPet));
                    petInterface.AIMode = 3;
                    myPet.AIMode = 3;
                    myPet.AiInterface.Debugger?.SendClientMessage("[MR]: Passive state.");
                    break; // mode Passive
                case PetCommand.Defensive:
                    myPet.AiInterface.SetBrain(new GuardBrain(myPet));
                    petInterface.AIMode = 4;
                    myPet.AIMode = 4;
                    myPet.AiInterface.Debugger?.SendClientMessage("[MR]: Defensive state.");
                    break; // mode Defensive
                case PetCommand.Aggressive:
                    myPet.AiInterface.SetBrain(new AggressiveBrain(myPet));
                    petInterface.AIMode = 5;
                    myPet.AIMode = 5;
                    myPet.AiInterface.Debugger?.SendClientMessage("[MR]: Aggressive state.");
                    break; // mode Aggressive
                case PetCommand.Attack:
                    long now = TCPManager.GetTimeStampMS();
                    if (cclient.Plr.IsMounted ||  now <= myPet.AttackReuseTimer + COMMAND_ATTACK_REUSE)
                        return;
                    myPet.AttackReuseTimer = now;
                    Unit target = cclient.Plr.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY);
                    if (target == null || !CombatInterface.CanAttack(myPet, target))
                        return;

                    if (!cclient.Plr.LOSHit(target))
                    {
                        return;
                    }
                    
                    myPet.AiInterface.Debugger?.SendClientMessage("[MR]: Attacking by request.");
                    myPet.IsHeeling = false;
                    myPet.FollowMode = 0;
                    myPet.Owner.CbtInterface.RefreshCombatTimer();
                    myPet.AiInterface.ProcessCombatStart(target);
                    myPet.SendPetUpdate();
                    break; //attack

                case PetCommand.Release:
                    myPet.Destroy();
                    break;
                case PetCommand.AbilityCast:
                    if (cclient.Plr.IsMounted)
                        return;
                    foreach (NPCAbility pa in myPet.AbtInterface.NPCAbilities)
                    {
                        if (pa.Entry == abilityid)
                        {
                            if (pa.Range > 0)
                            {
                                Unit abTarget = myPet.CbtInterface.GetCurrentTarget();
                                if (abTarget == null || !myPet.LOSHit(abTarget))
                                    return;
                            }
                            myPet.AbtInterface.StartCast(myPet, abilityid, 1);
                            break;
                        }
                    }
                    break;
                case PetCommand.Autocast:
                    if (cclient.Plr.IsMounted)
                        return;
                    foreach (NPCAbility pa in myPet.AbtInterface.NPCAbilities)
                    {
                        if (pa.Entry != abilityid)
                            continue;

                        pa.AutoUse = !pa.AutoUse;
                        myPet.SendPetUpdate();
                        break;
                    }
                    break;
           }
        }
    }
}
