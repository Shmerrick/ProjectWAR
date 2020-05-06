using BehaviourTree;
using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using SystemData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.AI;
using WorldServer.World.Interfaces;
using CreatureSubTypes = GameData.CreatureSubTypes;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public class Creature : Unit, IClock
    {
        public Creature_spawn Spawn;
        public SiegeInterface SiegeInterface;
        public uint Entry => Spawn?.Entry ?? 0;
        protected byte Scale;
        public string PQSpawnId { get; set; }
        public ushort Ranged { get; set; }
        public ushort Model1 { get; set; }
        public ushort Model2 { get; set; }


        public Creature()
        {
        }

        public Creature(Creature_spawn spawn) : this()
        {
            if (spawn == null)
                throw new ArgumentNullException("NULL spawn passed to Creature.");
            Spawn = spawn;
            Name = spawn.Proto.Name;
            Ranged = spawn.Proto.Ranged;
            Model1 = spawn.Proto.Model1;
            Model2 = spawn.Proto.Model2;
            if (spawn.Proto.Invulnerable == 1)
                IsInvulnerable = true;

            Scale = (byte)StaticRandom.Instance.Next(Spawn.Proto.MinScale, Spawn.Proto.MaxScale);

            if (spawn.Proto.BaseRadiusUnits > 0)
                BaseRadius = spawn.Proto.BaseRadiusUnits * (Scale / 50f) / UNITS_TO_FEET;
            else
                BaseRadius *= (Scale / 50f);

            SiegeInterface = AddInterface<SiegeInterface>();
        }

        #region CrowdControl

        public override void ApplyKnockback(Unit caster, AbilityKnockbackInfo kbInfo)
        {
            BuffInterface.QueueBuff(new BuffQueueInfo(caster, caster.EffectiveLevel, AbilityMgr.GetBuffInfo(237)));
        }

        public long GetTimeStampInMilliseconds()
        {
            return DateTime.Now.Millisecond;
        }

        #endregion

        protected virtual void SetCreatureStats()
        {
            List<CharacterInfo_stats> baseStats;

            float statBonusMult = 1.0f;

            if (Rank > 0)
            {
                switch (Rank)
                {
                    case 1: statBonusMult = 2.25f; break;
                    case 2: statBonusMult = 6f; break;
                    case 3: statBonusMult = 12f; break;
                }
            }
            // NPC Career:
            // 0 or default - what it was before
            // 1 - Tank
            // 2 - MDPS
            // 3 - Magic RDPS
            // 4 - Phys RDPS
            // 5 - Healer

            int Career = Spawn.Proto.Career;
#if !DEBUG
            Career = 0;
#endif

            switch (Career)
            {
                default:
                    if (Ranged > 15)
                    {
                        baseStats = CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_SHADOW_WARRIOR, Math.Min(Level, (byte)80));
                        StsInterface.Load(baseStats);

                        StsInterface.AddItemBonusStat(Stats.BallisticSkill, (ushort)(5 * statBonusMult * Level * Spawn.Proto.PowerModifier));
                    }
                    else
                    {
                        baseStats = CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_SHADOW_WARRIOR, Math.Min(Level, (byte)80));
                        StsInterface.Load(baseStats);

                        StsInterface.AddItemBonusStat(Stats.Strength, (ushort)(5 * statBonusMult * Level * Spawn.Proto.PowerModifier));
                    }
                    break;
                case 1:
                    baseStats = CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_IRON_BREAKER, Math.Min(Level, (byte)80));
                    StsInterface.Load(baseStats);
                    ApplyCareer(baseStats, Spawn, statBonusMult);
                    break;

                case 2:
                    baseStats = CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_SLAYER, Math.Min(Level, (byte)80));
                    StsInterface.Load(baseStats);
                    ApplyCareer(baseStats, Spawn, statBonusMult);
                    break;

                case 3:
                    baseStats = CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_SORCERER, Math.Min(Level, (byte)80));
                    StsInterface.Load(baseStats);
                    ApplyCareer(baseStats, Spawn, statBonusMult);
                    break;

                case 4:
                    baseStats = CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_ENGINEER, Math.Min(Level, (byte)80));
                    StsInterface.Load(baseStats);
                    ApplyCareer(baseStats, Spawn, statBonusMult);
                    break;

                case 5:
                    baseStats = CharMgr.GetCharacterInfoStats((byte)CareerLine.CAREERLINE_RUNE_PRIEST, Math.Min(Level, (byte)80));
                    StsInterface.Load(baseStats);
                    ApplyCareer(baseStats, Spawn, statBonusMult);
                    break;
            }

            if (Career == 0)
            {
                foreach (var stat in baseStats)
                {
                    Stats bonusType = (Stats)stat.StatId;

                    if (bonusType < Stats.BlockSkill && bonusType != Stats.Wounds)
                        StsInterface.AddItemBonusStat(bonusType, (ushort)(stat.StatValue * (statBonusMult - 1f) * Spawn.Proto.PowerModifier));
                }
            }
            ushort armor = (ushort)((36 + 13 * Rank * Spawn.Proto.PowerModifier) * Level);

            StsInterface.AddItemBonusStat(Stats.Armor, armor);
            StsInterface.AddItemBonusStat(Stats.SpiritResistance, (ushort)((7.5f + 2.5f * Rank) * Level * Spawn.Proto.PowerModifier));
            StsInterface.AddItemBonusStat(Stats.ElementalResistance, (ushort)((7.5f + 2.5f * Rank) * Level * Spawn.Proto.PowerModifier));
            StsInterface.AddItemBonusStat(Stats.CorporealResistance, (ushort)((7.5f + 2.5f * Rank) * Level * Spawn.Proto.PowerModifier));

            StsInterface.SetBaseStat(Stats.Wounds, GenerateWounds(Level, Rank));

            //List<Creature_stats>

            foreach (Creature_stats stat in CreatureService.GetCreatureStats(Entry))
            {
                if (stat != null)
                {
                    if (stat.StatValue < 0)
                    {
                        int s = stat.StatValue * -1;
                        StsInterface.RemoveItemBonusStat((Stats)stat.StatId, (ushort)s);
                    }
                    else
                        StsInterface.AddItemBonusStat((Stats)stat.StatId, (ushort)stat.StatValue);
                }
            }

            StsInterface.ApplyStats();
        }

        public int GetMultiplierForCareerType(Stats stat, byte career)
        {
            switch (stat)
            {
                case Stats.Strength:
                    {
                        if (career == 1) // tank
                        {
                            return 3;
                        }
                        if (career == 2) // mdps
                        {
                            return 6;
                        }
                    }
                    break;
                case Stats.BallisticSkill:
                    {
                        if (career == 4) //rdps
                        {
                            return 6;
                        }
                    }
                    break;

                case Stats.Intelligence:
                    {
                        if (career == 3) //magical rdps
                        {
                            return 6;
                        }
                        if (career == 5) //healer
                        {
                            return 6;
                        }
                    }
                    break;
                case Stats.Toughness:
                    {
                        if (career == 1) //tank
                        {
                            return 2;
                        }
                    }
                    break;
                default:
                    {
                        return 1;
                    }
            }
            return 1;
        }

        public void ApplyCareer(List<CharacterInfo_stats> baseStats, Creature_spawn Spawn, float statBonusMult)
        {
            foreach (var stat in baseStats)
            {
                if ((Stats)stat.StatId < Stats.BlockSkill && (Stats)stat.StatId != Stats.Wounds && (Stats)stat.StatId != Stats.Agility)
                {
                    uint softcap = (uint)(50 + 25 * Level);

                    int totalStat, toAddStat, toAddPower = 0;

                    totalStat = (StsInterface.GetTotalStat((Stats)stat.StatId)) + (int)(GetMultiplierForCareerType((Stats)stat.StatId, Spawn.Proto.Career) * statBonusMult * Level * Spawn.Proto.PowerModifier);
                    toAddStat = (int)(GetMultiplierForCareerType((Stats)stat.StatId, Spawn.Proto.Career) * statBonusMult * Level * Spawn.Proto.PowerModifier);
                    if (totalStat > softcap && StatsExtensions.GetStatPowerType((Stats)stat.StatId) != Stats.None) //if we can add it as power, do so
                    {
                        toAddPower = totalStat - (int)softcap;
                        StsInterface.SetBaseStat((Stats)stat.StatId, (ushort)softcap);
                        StsInterface.AddItemBonusStat(StatsExtensions.GetStatPowerType((Stats)stat.StatId), (ushort)(toAddPower));
                    }
                    else
                    {
                        StsInterface.AddItemBonusStat((Stats)stat.StatId, (ushort)toAddStat);
                    }
                }
            }
        }

        public virtual ushort GetStrikeDamage()
        {
            ushort strikeDamage = (ushort)(50f * Level); // Normal NPC
            switch (Rank)
            {
                case 1: strikeDamage = (ushort)(120f * Level); break; // Champ
                case 2: strikeDamage = (ushort)(300f * Level); break; // Hero
                case 3: strikeDamage = (ushort)(700f * Level); break; // Lord
            }

            if (Spawn.Proto.WeaponDPS != 0)
                strikeDamage = (ushort)(Spawn.Proto.WeaponDPS * Level); // Override

            return strikeDamage;
        }

        protected ushort GenerateWounds(byte level, byte rank)
        {
            float wounds = 0;
            wounds += 70 * (level + level / 2);

            switch (rank)
            {
                case 1: wounds *= 2; break;
                case 2: wounds *= 8; break;
                case 3: wounds *= 16; break;
            }

            return (ushort)(wounds / 10 * Spawn.Proto.WoundsModifier);
        }

        public override void OnLoad()
        {
            InteractType = Spawn.Proto.InteractType;

            SetFaction(Spawn.Faction != 0 ? Spawn.Faction : Spawn.Proto.Faction);

            ItmInterface.Load(CreatureService.GetCreatureItems(Spawn.Entry));

            if (Spawn.Proto.CreatureType == (byte)GameData.CreatureTypes.SIEGE)
                Level = 40;
            else if (Spawn.Level != 0)
                Level = Spawn.Level;
            else if (Spawn.Proto.LairBoss)
                Level = Spawn.Proto.MaxLevel;
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

            // Ensure siege cannons won't go walkabout
            if (Spawn.Proto.CreatureType == (byte)GameData.CreatureTypes.SIEGE)
                AiInterface.SetBrain(new DummyBrain(this));

            //if (AiInterface.Waypoints.Count == 0)
                //if (Scale == 110)
                //{
                //    var x = AiInterface.Waypoints;
                //}
                //else
                //{
                //    AiInterface.Waypoints = WaypointService.GetNpcWaypoints(Spawn.Guid);
                //}


            if (StsInterface.Speed != 0)
            {
                StsInterface.Speed = 235;
                MvtInterface.SetBaseSpeed(Speed);
            }

            IsActive = true;

            if (Spawn.Proto.ImmuneToCC == 1)
                AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }

        public override void SendMeTo(Player plr)
        {
            //Log.Success("Creature", "SendMe " + Name);

            SendCreateMonster(plr);

            base.SendMeTo(plr);
        }

        protected virtual void SendCreateMonster(Player plr)
        {
            if (Spawn == null)
            {
                Log.Error("ERROR", "NO SPAWN FOR " + Name);
                return;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_MONSTER, 110 + States.Count + (Name?.Length ?? Spawn.Proto.Name.Length));
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);
            Out.WriteUInt16(0); // Speed Z
            if (Model2 != 0)
                Out.WriteUInt16(StaticRandom.Instance.Next(0, 100) < 50 ? Model1 : Model2);
            else
                Out.WriteUInt16(Model1);
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

            // 44 bytes
            byte statesLength = (byte)(States.Count + Spawn.Proto.States.Length);
            CreatureState questState = CreatureState.Merchant;

            if (QtsInterface.CreatureHasQuestToComplete(plr))
                questState = CreatureState.QuestFinishable;
            else if (QtsInterface.CreatureHasStartRepeatingQuest(plr))
                questState = CreatureState.RepeatableQuestAvailable;
            else if (QtsInterface.CreatureHasStartQuest(plr))
                questState = CreatureState.QuestAvailable;
            else if (QtsInterface.CreatureHasQuestToAchieve(plr))
                questState = CreatureState.QuestInProgress;

            if (questState != CreatureState.Merchant)
                ++statesLength;

            Out.WriteByte(statesLength);
            if (States.Count > 0)
                Out.Write(States.ToArray(), 0, States.Count);
            Out.Write(Spawn.Proto.States, 0, Spawn.Proto.States.Length);

            if (questState != CreatureState.Merchant)
                Out.WriteByte((byte)questState);

            Out.WriteByte(0);

            Out.WriteCString(Spawn.Proto.GenderedName);

            Out.Write(Spawn.Proto.FigLeafData, 0, Spawn.Proto.FigLeafData.Length);
            Out.WriteByte(Spawn.Proto.InteractTrainerType);

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

            // Send packet to prevent targeting of invulnerables
            if (IsInvulnerable)
            {
                Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
                Out.WriteUInt16(Oid);
                Out.WriteByte(1);
                Out.WriteByte(7);
                Out.Fill(0, 6);
                plr.SendPacket(Out);
            }
        }

        private byte _gatherType;
        private LootContainer _gatheringLoot;

        public override void SendInteract(Player player, InteractMenu menu)
        {
            player.QtsInterface.HandleEvent(Objective_Type.QUEST_SPEAK_TO, Spawn.Entry, 1);

            if (!IsDead)
            {
                if (!string.IsNullOrEmpty(Spawn.Proto.TokUnlock))
                {
                    player.TokInterface.AddToks(Spawn.Proto.TokUnlock);
                }

                // perhaps do some checks?
                switch (menu.Menu)
                {
                    case 7:
                        {
                            switch (Spawn.Proto.TitleId)
                            {
                                case CreatureTitle.Apothecary:
                                    if (player._Value.CraftingSkill != 4)
                                    {
                                        player.SendTradeSkill(player._Value.CraftingSkill, 0);
                                        player._Value.CraftingSkill = 0;
                                        player._Value.CraftingSkillLevel = 0;
                                        player._Value.CraftingSkill = 4;
                                        player._Value.CraftingSkillLevel = 1;
                                        player.CraftApoInterface.Load();
                                    }

                                    break;
                                case CreatureTitle.Butcher:
                                    if (player._Value.GatheringSkill != 1)
                                    {
                                        player.SendTradeSkill(player._Value.GatheringSkill, 0);
                                        player._Value.GatheringSkill = 0;
                                        player._Value.GatheringSkillLevel = 0;
                                        player._Value.GatheringSkill = 1;
                                        player._Value.GatheringSkillLevel = 1;
                                        player.GatherInterface.Load();
                                    }
                                    break;
                                case CreatureTitle.Cultivator:

                                    if (player._Value.GatheringSkill != 3)
                                    {
                                        player.SendTradeSkill(player._Value.GatheringSkill, 0);
                                        player._Value.GatheringSkill = 0;
                                        player._Value.GatheringSkillLevel = 0;
                                        player._Value.GatheringSkill = 3;
                                        player._Value.GatheringSkillLevel = 1;
                                        player.CultivInterface.Load();
                                    }
                                    break;
                                case CreatureTitle.Salvager:
                                    if (player._Value.GatheringSkill != 6)
                                    {
                                        player.SendTradeSkill(player._Value.GatheringSkill, 0);
                                        player._Value.GatheringSkill = 0;
                                        player._Value.GatheringSkillLevel = 0;
                                        player._Value.GatheringSkill = 6;
                                        player._Value.GatheringSkillLevel = 1;
                                        player.GatherInterface.Load();
                                    }
                                    break;
                                case CreatureTitle.Scavenger:
                                    if (player._Value.GatheringSkill != 2)
                                    {
                                        player.SendTradeSkill(player._Value.GatheringSkill, 0);
                                        player._Value.GatheringSkill = 0;
                                        player._Value.GatheringSkillLevel = 0;
                                        player._Value.GatheringSkill = 2;
                                        player._Value.GatheringSkillLevel = 1;
                                        player.GatherInterface.Load();
                                    }
                                    break;
                                case CreatureTitle.HedgeWizard: //talsiman
                                    if (player._Value.CraftingSkill != 5)
                                    {
                                        player.SendTradeSkill(player._Value.CraftingSkill, 0);
                                        player._Value.CraftingSkill = 0;
                                        player._Value.CraftingSkillLevel = 0;
                                        player._Value.CraftingSkill = 5;
                                        player._Value.CraftingSkillLevel = 1;
                                        player.CraftTalInterface.Load();
                                    }
                                    break;
                                default:
                                    PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 5);
                                    Out.WriteByte(5);
                                    Out.WriteByte(0x0F);
                                    Out.WriteByte(6);
                                    Out.WriteUInt16(0);
                                    player.SendPacket(Out);
                                    break;
                            }
                        }
                        break;
                    case 9:
                        // Dynamic Vendor
                        switch (Spawn.Proto.VendorID)
                        {
                            case 428:  // BlackMarket Vendor
                            {
                                var blackMarketVendor = new BlackMarketVendorItem(player);
                                WorldMgr.SendDynamicVendorItems(player,blackMarketVendor.GetBlackMarketItems(player));
                                break;
                            }
                            case 10002:  // Honor Vendor
                            {
                                WorldMgr.SendDynamicVendorItems(player,
                                    new HonorVendorItem(player).items);
                                break;
                            }
                            case 10001:  // Realm Captain Vendor
                            {
                                WorldMgr.SendDynamicVendorItems(player,
                                    new RealmCaptainVendorItem(player).items);
                                break;
                            }
                            case 10000:
                            {
                                WorldMgr.SendDynamicVendorItems(player,
                                    new RenownLevelVendorItem(player._Value.RenownRank, player._Value.Level).items);
                                break;
                            }
                            default:
                            {
                                if ((Spawn.Proto.VendorID > 0) && (Spawn.Proto.VendorID <= 9999))
                                    WorldMgr.SendVendor(player, Spawn.Proto.VendorID);
                                break;
                            }
                        }

                    
                        break;
                    case 10:
                        TakeInfluenceItem(player, menu);
                        break;
                    case 11:
                        // Black market vendor
                        if (Spawn.Proto.VendorID == 428)
                        {
                            var blackMarketVendor = new BlackMarketVendorItem(player);
                            var items = blackMarketVendor.GetBlackMarketItems(player);
                            WorldMgr.BuyItemBlackMarketVendor(player, menu, items);
                            break;
                        }

                        // Dynamic Vendor -- Honor vendor
                        if (Spawn.Proto.VendorID == 10002)
                        {
                            var items = new HonorVendorItem(player).items;
                            WorldMgr.BuyItemHonorDynamicVendor(player, menu, items);
                            // Refresh the available item list.
                            WorldMgr.SendDynamicVendorItems(player,
                                new HonorVendorItem(player).items);
                        }
                        else
                        {
                            // realm captain vendor
                            if (Spawn.Proto.VendorID == 10001)
                            {
                                var items = new RealmCaptainVendorItem(player).items;
                                WorldMgr.BuyItemRealmCaptainDynamicVendor(player, menu, items);
                            }
                            else
                            {
                                if (Spawn.Proto.VendorID == 10000)
                                {
                                    //WorldMgr.BuyItemDynamicVendor(player, menu,
                                    //    new RenownLevelVendorItem(player._Value.RenownRank, player._Value.Level).items);
                                }
                                else
                                {
                                    if (Spawn.Proto.VendorID > 0)
                                        WorldMgr.BuyItemVendor(player, menu, Spawn.Proto.VendorID);
                                }
                            }                            
                        }

                        break;
                    case 14:
                        player.ItmInterface.SellItem(menu);
                        break;
                    case 24:
                        // unk?
                        break;
                    case 25:
                        SetRallyPoint(player, menu);
                        break;
                    case 29:
                        player.ItmInterface.RepairItem(menu);
                        break;
                    case 32:
                        Repair();
                        break;
                    case 36:
                        player.ItmInterface.BuyBackItem(menu);
                        break;
                    case 37:
                        SendDyeList(player);
                        break;
                    case 38:
                        DyeItem(player, menu);
                        break;
                    case 39:
                        DyeAllItems(player, menu);
                        break;
                    case 42:
                        if (menu.Num == 3)
                            player.AbtInterface.RespecializeMastery(false);
                        if (menu.Num == 6)
                            player.RenInterface.Respec();
                        break;
                    default:
                        GenericInteract(player, menu);
                        break;
                }
            }
            base.SendInteract(player, menu);
        }

        private void GenericInteract(Player player, InteractMenu menu)
        {
            if (Spawn.Proto.CreatureType == (byte)GameData.CreatureTypes.SIEGE)
            {
                SiegeInterface.Interact(player, (byte)menu.Menu);
                return;
            }

            switch (InteractType)
            {
                case InteractType.INTERACTTYPE_FLIGHT_MASTER:
                    {
                        SendFlightInfo(player);
                        break;
                    }

                case InteractType.INTERACTTYPE_STORE:
                    {

                    }
                    break;
                case InteractType.INTERACTTYPE_BANKER:
                    {
                        PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                        Out.WriteByte(0x1D);
                        Out.WriteByte(0);
                        player.SendPacket(Out);
                    }
                    break;
                case InteractType.INTERACTTYPE_GUILD_VAULT:
                    player.GldInterface.Guild?.SendGuildVault(player);
                    break;
                case InteractType.INTERACTTYPE_HEALER:
                    HealerInteract(player, menu);
                    break;
                case InteractType.INTERACTTYPE_BARBER_SURGEON:
                    {
                        PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                        Out.WriteByte(16); // BarberShop
                        Out.WriteByte(0);
                        player.SendPacket(Out);
                    }
                    break;
                case InteractType.INTERACTTYPE_AUCTIONEER:
                    {
                        PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                        Out.WriteByte(0x1A);
                        Out.WriteByte(0);
                        player.SendPacket(Out);
                    }
                    break;
                case InteractType.INTERACTTYPE_LASTNAMESHOP:
                    {
                        const int dialogId = 0x1C;
                        PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                        Out.WriteByte(dialogId); // Last name change dialog
                        Out.WriteByte(0);
                        Out.WriteUInt32(Constants.LastNameChangeCost);
                        player.SendPacket(Out);
                    }
                    break;
                default:
                    {
                        ushort menuItems = 0;
                        string text = CreatureService.GetCreatureText(Spawn.Entry);

                        if (InteractType == InteractType.INTERACTTYPE_DYEMERCHANT)
                        {
                            if (!VendorValid())
                            {
                                PacketOut nope = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                                nope.WriteByte(0);
                                nope.WriteUInt16(menu.Oid);
                                nope.WriteUInt16(0);
                                nope.WriteUInt16(32); // hasText
                                nope.WriteShortString("There exists a time and a place for my services, " + player.Name + ", and this is neither the time nor the place.");
                                player.SendPacket(nope);
                                return;
                            }

                            menuItems += 2; // Shop
                            menuItems += 16384; // Dyes
                                                // You need Text to see the 'dyes' option
                            if (text == string.Empty)
                                text = "Come and see what I have.";
                        }

                        if (InteractType == InteractType.INTERACTTYPE_HEALER)
                        {
                            menuItems += 512; // Heal
                                              // You need Text to see the 'dyes' option
                            if (text == string.Empty)
                                text = "Come and see what I have.";
                        }

                        bool influence = false;
                        bool hasQuests = QtsInterface.HasQuestInteract(player, this);
                        if (hasQuests)
                            menuItems += 64; // Quests


                        if (InteractType == InteractType.INTERACTTYPE_GUILD_REGISTRAR)
                        {
                            menuItems += 128; // Guild Register
                                              // Guild Registrar needs text
                            if (text == string.Empty)
                                text = "Let's get started. To form a guild, you'll need to have a full group of six adventurers with you. None of you can belong to another guild. For a modest fee of only fifty silver I can create your guild.";
                        }


                        if (InteractType == InteractType.INTERACTTYPE_TRAINER)
                        {
                            if (Spawn.Proto.TitleId == CreatureTitle.CareerTrainer || Spawn.Proto.TitleId == CreatureTitle.RenownTrainer || Spawn.Proto.TitleId == CreatureTitle.Trainer || player.Level <= Constants.MaxTierLevel[Region.GetTier() - 1])
                                menuItems += 1; // Trainer

                            // Theese were previously in there, nice to keep them unless theres info in creature_texts
                            if (text == string.Empty)
                                if (player.Realm == Realms.REALMS_REALM_ORDER)
                                    text = "Hail defender of the Empire!  Your performance in battle is the only thing that keeps the hordes of Chaos at bay. Let's begin your training at once!";
                                else
                                    text = "Learn these lessons well, for gaining the favor of the Raven god should be of utmost importance to you. Otherwise... There is always room for more Spawn within our ranks.";
                        }

                        if (InteractType == InteractType.INTERACTTYPE_BINDER)
                        {
                            menuItems += 256; // Rally Point
                            menuItems += 4;
                            influence = true;
                        }
                        if (!string.IsNullOrEmpty(text))
                            menuItems += 32; // Text


                        PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                        Out.WriteByte(0);
                        Out.WriteUInt16(menu.Oid);
                        Out.WriteUInt16(0);
                        Out.WriteUInt16(menuItems);
                        if (hasQuests)
                            QtsInterface.BuildInteract(player, this, Out);
                        if (!string.IsNullOrEmpty(text))
                            Out.WriteShortString(text);

                        if (influence)
                            Out.WriteUInt16(ChapterService.GetChapterByNPCID(Entry));
                        if ((menuItems & 1) > 0)
                            Out.WriteByte(Spawn.Proto.InteractTrainerType);
                        player.SendPacket(Out);
                    }

                    break;
            }

        }

        private bool VendorValid()
        {
            if (Zone == null)
                return true;

            return !(Zone.Info.Illegal || Zone.ZoneId == 193 || Zone.ZoneId == 175);
        }

        public override void TryLoot(Player player, InteractMenu menu)
        {
            if (lootContainer != null && lootContainer.IsLootable())
            {
                player.PriorityGroup?.GroupLoot(player, lootContainer);

                lootContainer.SendInteract(player, menu);

                if (!lootContainer.IsLootable())
                {
                    SetLootable(false, player);

                    byte subType = Spawn.Proto.CreatureSubType;
                    if (CreatureService.IsButcherable(subType))
                    {
                        _gatherType = (byte)TradeSkills.TRADESKILLS_BUTCHERING;
                        GatherLootable(true, player, _gatherType);
                    }

                    else if (CreatureService.IsScavengeable(subType))
                    {
                        _gatherType = (byte)TradeSkills.TRADESKILLS_SCAVENGING;
                        GatherLootable(true, player, _gatherType);
                    }
                }
            }

            else if (_gatherType > 0 && player._Value.GatheringSkill == _gatherType)
            {
                if (_gatheringLoot == null)
                    _gatheringLoot = _gatherType == 1 ?
                        CreatureService.GenerateButchery(Spawn.Proto.CreatureSubType, (byte)Math.Min(Level * 5, player._Value.GatheringSkillLevel))
                         : CreatureService.GenerateScavenge(Level, Rank, (byte)Math.Min(Level * 5, player._Value.GatheringSkillLevel));

                _gatheringLoot.SendInteract(player, menu, this);

                if (!_gatheringLoot.IsLootable())
                {
                    player.GatherInterface.Gather(Level);
                    GatherLootable(false, player, _gatherType);
                    _gatherType = 0;
                    _gatheringLoot = null;
                }
            }

        }

        protected override void HandleDeathRewards(Player killer)
        {
            if (Rank < 2)
            {
                Player credited;

                CombatInterface_Npc npc = CbtInterface as CombatInterface_Npc;

                if (npc != null)
                    credited = npc.FirstStriker ?? killer;
                else
                    credited = killer;

                WorldMgr.GenerateXP(credited, this, 1f);

                GenerateLoot(credited.PriorityGroup != null ? credited.PriorityGroup.GetGroupLooter(credited) : GetLooter(credited), 1f);

                CreditQuestKill(credited);
            }

            else
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

                    float damageFactor = (float)kvpair.Value / TotalDamageTaken;

                    uint xpShare = (uint)(totalXP * damageFactor);

                    // Solo player, add their rewards directly.
                    if (curPlayer.PriorityGroup == null)
                    {
                        if (curPlayer.Level != curPlayer.AdjustedLevel)
                            xpShare = 0;

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
                            groupXPRenown.Add(curPlayer.PriorityGroup, new XpRenown(xpShare, 0, 0, 0, TCPManager.GetTimeStamp()));

                        groupXPRenown[curPlayer.PriorityGroup].Damage += kvpair.Value;

                        if (groupXPRenown[curPlayer.PriorityGroup].Damage > bestDamage)
                        {
                            looter = curPlayer.PriorityGroup.GetGroupLooter(curPlayer);
                            bestDamage = groupXPRenown[curPlayer.PriorityGroup].Damage;
                        }
                    }
                }

                if (groupXPRenown.Count > 0)
                {
                    foreach (KeyValuePair<Group, XpRenown> kvpair in groupXPRenown)
                        kvpair.Key.AddXpCount(killer, kvpair.Value.XP);
                }

                if (looter != null)
                {
                    GenerateLoot(looter, 1f);
                    CreditQuestKill(looter);
                }
            }
        }

        protected void CreditQuestKill(Player killer)
        {
            byte subtype = Spawn.Proto.CreatureSubType;

            if (subtype == 0)
                return;

            if (killer.PriorityGroup != null)
            {
                List<Player> curMembers = killer.PriorityGroup.GetPlayersCloseTo(killer, 150);

                foreach (Player subPlayer in curMembers)
                    subPlayer.TokInterface.AddKill(subtype);
            }
            else
                killer.TokInterface.AddKill(subtype);

            if (!string.IsNullOrEmpty(Spawn.Proto.TokUnlock))
            {
                killer.TokInterface.AddToks(Spawn.Proto.TokUnlock);

                if (killer.WorldGroup != null)
                {
                    List<Player> members = killer.WorldGroup.GetPlayerListCopy();

                    foreach (var member in members)
                        if (member != killer)
                            member.TokInterface.AddToks(Spawn.Proto.TokUnlock);
                }
            }

            killer.QtsInterface.HandleEvent(Objective_Type.QUEST_KILL_MOB, Spawn.Entry, 1);
        }

        public void CheckDamageCaster(Unit caster, AbilityDamageInfo damageInfo)
        {
            int LevelDiff = Spawn.Level - caster.Level;

            if ((Spawn.Level - caster.Level) >= 10)
            {
                damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                damageInfo.Mitigation = damageInfo.Damage;
                damageInfo.Damage = 0;
                damageInfo.PrecalcMitigation = damageInfo.Damage;
                damageInfo.PrecalcDamage = 0;
            }
        }

        public override void ModifyDamageIn(AbilityDamageInfo damage)
        {
            /*if (damage.SubDamageType == SubDamageTypes.Cannon || damage.SubDamageType == SubDamageTypes.Artillery)
                damage.DamageEvent = (byte) CombatEvent.COMBATEVENT_EVADE;*/
        }

        public override void ModifyDamageOut(AbilityDamageInfo outDamage)
        {
            switch ((CreatureSubTypes)Spawn.Proto.CreatureSubType)
            {
                case CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    outDamage.SubDamageType = SubDamageTypes.Cannon;
                    outDamage.ContributoryFactor = 0.35f;
                    break;
                case CreatureSubTypes.SIEGE_RAM:
                    outDamage.SubDamageType = SubDamageTypes.Ram;
                    break;
                case CreatureSubTypes.SIEGE_GTAOE:
                    outDamage.SubDamageType = SubDamageTypes.Artillery;
                    outDamage.ContributoryFactor = 0.1f;
                    break;
                case CreatureSubTypes.SIEGE_OIL:
                    outDamage.SubDamageType = SubDamageTypes.Oil;
                    outDamage.ContributoryFactor = 0.1f;
                    break;
            }
        }

        protected override void SetDeath(Unit killer)
        {
            base.SetDeath(killer);

            AiInterface.ProcessCombatEnd();

            SetRespawnTimer();
        }
        protected void Repair()
        {
            if (SiegeInterface != null)
                SiegeInterface.Repair();

        }

        protected virtual void SetRespawnTimer()
        {
            if (Spawn.RespawnMinutes > 0)
                EvtInterface.AddEvent(RezUnit, Spawn.RespawnMinutes * 60 * 1000, 1);
            else if (Spawn.Proto.LairBoss)     // Lair Bosses 2 hour spawn time
                EvtInterface.AddEvent(RezUnit, (7200000 + StaticRandom.Instance.Next(0, 3600000)), 1); // 2 hours seconde Rez
            else
            {
                int baseRespawn = 50000 + Level * 1000;

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

        public override void RezUnit()
        {
            Region.CreateCreature(Spawn);
            Detaunters.Clear();
            Destroy();
        }

        public override string ToString()
        {
            return "SpawnId=" + Spawn.Guid + ",Entry=" + Spawn.Entry + ",Name=" + Name + ",Level=" + Level + ",Rank=" + Rank + ",Max Health=" + MaxHealth + ",Faction=" + Faction + ",Emote=" + Spawn.Emote + "AI:" + AiInterface.State + ",Position :" + base.ToString();
        }

        #region Interactions

        private void HealerInteract(Player player, InteractMenu menu)
        {
            NewBuff healDebuff = player.BuffInterface.GetBuff(14025, null);

            if (healDebuff == null)
                return;

            byte stacks = healDebuff.StackLevel;
            uint costPerHeal = (uint)(50 * player.Level);

            if (menu.Menu == 0)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                Out.WriteByte(19); // healer
                Out.WriteByte(stacks); // number of penalties
                Out.WriteUInt32(costPerHeal); // heal cost per penalty
                player.SendPacket(Out);
            }

            else
            {
                if (player.RemoveMoney(costPerHeal * stacks))
                    healDebuff.RemoveStacks(menu.Num);
            }
        }

#warning FIXME. Looks like there's support for taking more than one item at a time.
        private void TakeInfluenceItem(Player player, InteractMenu menu)
        {
            ushort slot = menu.Packet.GetUint16();

            if (slot == 4)
                slot = 3;
            else if (slot == 8)
                slot = 4;

            slot -= 1;

            Chapter_Info Info = ChapterService.GetChapterEntry(ChapterService.GetChapterByNPCID(Entry));

            foreach (Characters_influence Obj in player.Info.Influences)
            {

                if (Obj.InfluenceId == ChapterService.GetChapterByNPCID(Entry))
                {
                    uint itemId = 0;
                    switch (menu.Num)
                    {
                        case 0:
                            if (Info.Tier1InfluenceCount <= Obj.InfluenceCount && !Obj.Tier_1_Itemtaken && player.ItmInterface.GetFreeMainBagInventorySlot() > 0)
                                itemId = GetCharpterRewardItemId(player, 1, Info, slot);
                            break;
                        case 1:
                            if (Info.Tier2InfluenceCount <= Obj.InfluenceCount && !Obj.Tier_2_Itemtaken && player.ItmInterface.GetFreeMainBagInventorySlot() > 0)
                                itemId = GetCharpterRewardItemId(player, 2, Info, slot);
                            break;
                        case 2:
                            if (Info.Tier3InfluenceCount <= Obj.InfluenceCount && !Obj.Tier_3_Itemtaken && player.ItmInterface.GetFreeMainBagInventorySlot() > 0)
                                itemId = GetCharpterRewardItemId(player, 3, Info, slot);
                            break;
                    }

                    if (itemId > 0)
                    {
                        player.ItmInterface.CreateItem(itemId, 1);
                        switch (menu.Num)
                        {
                            case 0: Obj.Tier_1_Itemtaken = true; break;
                            case 1: Obj.Tier_2_Itemtaken = true; break;
                            case 2: Obj.Tier_3_Itemtaken = true; break;
                        }
                    }
                }
            }

            player.SendInfluenceItems((byte)ChapterService.GetChapterByNPCID(Entry));
        }

        private uint GetCharpterRewardItemId(Player player, byte tier, Chapter_Info Info, ushort slot)
        {
            List<Chapter_Reward> rewards = player.ItmInterface.GetChapterRewards(tier, Info);
            if (slot >= rewards.Count)
            {
                Log.Error("GetCharpterRewardItemId", string.Concat("Item in slot ", slot, " was not found for chapter ", Info.Name));
                return 0;
            }
            uint itemId = rewards[slot].ItemId;
            return itemId;
        }

        private void SetRallyPoint(Player player, InteractMenu menu)
        {
            RallyPoint rally = RallyPointService.GetRallyPointFromNPC(Entry);
            if (rally != null)
            {
                player._Value.RallyPoint = rally.Id;

                PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 5);
                Out.WriteByte(0x12);
                Out.WriteUInt16(menu.Oid);
                Out.WriteUInt16(player._Value.RallyPoint);
                player.SendPacket(Out);
            }
            else
                player.SendLocalizeString("ERROR: Unknown Rally Point NPC (" + Entry + ").", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.CHAT_TAG_DEFAULT);
        }

        private void SendDyeList(Player player)
        {
            byte MAX_DYES = 30;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 64);
            Out.WriteByte(0x1B);

            List<Dye_Info> dyes = DyeService.GetDyes();
            byte count = (byte)Math.Min(dyes.Count, MAX_DYES);

            Out.WriteByte(count);
            for (byte i = 0; i < count; i++)
            {
                Out.WriteByte(i);
                Out.WriteUInt16(dyes[i].Entry);
                Out.WriteUInt32(dyes[i].Price);
            }

            player.SendPacket(Out);
        }

        private void DyeItem(Player player, InteractMenu menu)
        {
            Item item = player.ItmInterface.GetItemInSlot(menu.Num);

            if (item == null)
                return;

            byte primaryDye = menu.Packet.GetUint8();
            byte secondaryDye = menu.Packet.GetUint8();
            ushort primaryDyeId = 0;
            ushort secondaryDyeId = 0;

            uint cost = 0;
            // 255 = no dye selected
            if (primaryDye != 255)
                cost += DyeService.GetDyes()[primaryDye].Price;

            // 255 = no dye selected
            if (secondaryDye != 255)
                cost += DyeService.GetDyes()[secondaryDye].Price;

            if (!player.RemoveMoney(cost))
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_AUCTION_NOT_ENOUGH_MONEY);
                return;
            }

            // 255 = no dye selected
            if (primaryDye != 255)
                primaryDyeId = DyeService.GetDyes()[primaryDye].Entry;

            // 255 = no dye selected
            if (secondaryDye != 255)
                secondaryDyeId = DyeService.GetDyes()[secondaryDye].Entry;

            player.ItmInterface.DyeItem(item, primaryDyeId, secondaryDyeId);

            if (player.IsActive && player.IsInWorld() && player.Loaded)
            {
                foreach (Player P in PlayersInRange)
                {
                    if (P.HasInRange(player))
                        player.ItmInterface.SendEquipped(P);
                }
            }
        }

        private void DyeAllItems(Player player, InteractMenu menu)
        {
            byte count = 0;
            for (ushort i = 0; i < ItemsInterface.MAX_EQUIPMENT_SLOT; ++i)
                if (player.ItmInterface.Items[i] != null && player.ItmInterface.Items[i].Info.DyeAble) // && is dyable
                    ++count;

            byte primaryDye = menu.Packet.GetUint8();
            byte secondaryDye = menu.Packet.GetUint8();
            ushort primaryDyeId = 0;
            ushort secondaryDyeId = 0;

            uint cost = 0;
            // 255 = no dye selected
            if (primaryDye != 255)
                cost += DyeService.GetDyes()[primaryDye].Price * count;

            // 255 = no dye selected
            if (secondaryDye != 255)
                cost += DyeService.GetDyes()[secondaryDye].Price * count;

            if (!player.RemoveMoney(cost))
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_AUCTION_NOT_ENOUGH_MONEY);
                return;
            }

            // 255 = no dye selected
            if (primaryDye != 255)
                primaryDyeId = DyeService.GetDyes()[primaryDye].Entry;

            // 255 = no dye selected
            if (secondaryDye != 255)
                secondaryDyeId = DyeService.GetDyes()[secondaryDye].Entry;

            for (ushort i = 0; i < ItemsInterface.MAX_EQUIPMENT_SLOT; ++i)
                if (player.ItmInterface.Items[i] != null && player.ItmInterface.Items[i].Info.DyeAble) // && is dyable
                    player.ItmInterface.DyeItem(player.ItmInterface.Items[i], primaryDyeId, secondaryDyeId);

            if (player.IsActive && player.IsInWorld() && player.Loaded)
            {
                foreach (Player P in PlayersInRange)
                {
                    if (P.HasInRange(player))
                        player.ItmInterface.SendEquipped(P);
                }
            }
        }

        private void SendFlightInfo(Player player)
        {
            byte[] data =
            {
                    0x01, 0xF4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0x42, 0x39, 0x00, 0x00, 0x00, 0xC0, 0xE3,
                    0x03, 0x39, 0xA0, 0xD1, 0x6F, 0x00, 0xC8, 0xA8, 0x1D, 0x37, 0x28, 0x94, 0x79, 0x33, 0xB2, 0x24,
                    0x32, 0x44, 0xDB, 0xD7, 0x1C, 0x5D, 0x18, 0x5D, 0xDD, 0x1C, 0xA4, 0x0D, 0x00, 0x00, 0xA8, 0x6B,
                    0x21, 0x36, 0x11, 0x00, 0x00, 0x00, 0xC8, 0xD0, 0xAF, 0x3A, 0x78, 0xD1, 0x6F, 0x00
                };

            ushort counts = 1;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 64);
            Out.WriteByte(0x0A);
            List<Zone_Taxi> taxis = WorldMgr.GetTaxis(player);
            Out.WriteByte((byte)taxis.Count);
            foreach (Zone_Taxi taxi in taxis)
            {
                Out.WriteUInt16(counts);
                Out.WriteByte((byte)taxi.Info.Pairing);
                Out.WriteUInt16(taxi.Info.Price);
                Out.WriteUInt16(taxi.Info.ZoneId);
                Out.WriteByte(1);
                ++counts;
            }
            Out.Write(data);
            player.SendPacket(Out);
        }

        #endregion
        /// <summary>
        /// Determine whether this merchant is a siege merchant
        /// </summary>
        /// <returns></returns>
        public bool IsSiegeMerchant()
        {
            if (InteractType != InteractType.INTERACTTYPE_DYEMERCHANT)
                return false;
            var proto = CreatureService.GetCreatureProto(Entry);
            var vendorId = proto.VendorID;

            var items = VendorService.GetVendorItems((ushort)vendorId);
            foreach (var item in items)
            {
                if (item.Info.IsSiege)
                    return true;
            }

            return false;
        }

    }

  
}
