using System;
using System.Collections.Generic;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class StatsInterface : BaseInterface
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private class UnitStat
        {
            private List<ushort>[] _bonusStats;
            private List<ushort>[] _reducedStats;
            private List<float>[] _bonusPctStats;
            private List<float>[] _reducedPctStats;

            private readonly StatsInterface _stsInterface;

            public UnitStat(StatsInterface ownerInterface) { _stsInterface = ownerInterface; }

            #region Item and Talisman Bonuses

            private ushort _itemBonusStat;
            public byte ItemAddFromStat;
            public int ItemStatMultiplier = 1;
            public bool ItemBonusBlock;

            public ushort AvailableItemBonus => (ushort)(ItemBonusBlock ? 0 : (_itemBonusStat * _stsInterface.BolsterFactor));

            public ushort InternalItemBonus => (ushort)(_itemBonusStat * _stsInterface.BolsterFactor);

            public void AddItemBonusStat(ushort value)
            {
                _itemBonusStat += value;
            }

            // Adds a multiplicitive item bonus to a stat - be aware of the rounding. This will have an impact if you add/remove items as the rounded
            // value will be used when removing items from a player
            public void AddItemBonusStat(ushort value, ushort percentage)
            {
                

                var oldValue = _itemBonusStat;
                _itemBonusStat += (ushort)(1+Math.Round(value/100f));
                (_stsInterface._Owner as Player)?.SendClientMessage($"DEBUG : {oldValue}=>{oldValue*(1+(value/100f))}");
            }

            public void RemoveItemBonusStat(ushort value, ushort percentage)
            {
                var oldValue = _itemBonusStat;
                _itemBonusStat -= (ushort)(1+Math.Round(value/100f));
                (_stsInterface._Owner as Player)?.SendClientMessage($"DEBUG : {oldValue}=>{oldValue/(1+(value/100f))}");
            }

            public void RemoveItemBonusStat(ushort value)
            {
                _itemBonusStat -= value;
            }

            #endregion

            #region Additive

            private ushort _totalBonusStat, _totalReducedStat;

            public int TotalBonusStat => _totalBonusStat + AvailableItemBonus * ItemStatMultiplier;
            public int TotalReducedStat => _totalReducedStat;

            public int TotalStat => _totalBonusStat + AvailableItemBonus * ItemStatMultiplier - _totalReducedStat;

            public void AddBonusStat(ushort value, byte index)
            {
                // Tactics and Career Mechanics stack with everything
                if (index > 1)
                {
                    _totalBonusStat += value;
                    return;
                }

                if (_bonusStats == null)
                    _bonusStats = new List<ushort>[(byte)BuffClass.MaxBuffClasses];

                if (_bonusStats[index] == null)
                    _bonusStats[index] = new List<ushort>();

                if (_bonusStats[index].Count == 0)
                {
                    _bonusStats[index].Add(value);

                    _totalBonusStat += value;
                }
                else
                {
                    ushort curFirst = _bonusStats[index][0];

                    for (int i = 0; i <= _bonusStats[index].Count; ++i)
                    {
                        if (i == _bonusStats[index].Count || value >= _bonusStats[index][i])
                        {
                            _bonusStats[index].Insert(i, value);
                            break;
                        }
                    }

                    if (value > curFirst)
                    {
                        _totalBonusStat += (ushort)(value - curFirst);
                    }
                }
            }

            public void RemoveBonusStat(ushort value, byte index)
            {
                // Tactics and Career Mechanics stack with everything
                if (index > 1)
                {
                    _totalBonusStat -= value;
                    return;
                }

                for (int i = 0; i < _bonusStats[index].Count; ++i)
                {
                    if (value == _bonusStats[index][i])
                    {
                        if (i == 0)
                        {
                            if (_bonusStats[index].Count > 1)
                            {
                                _totalBonusStat -= (ushort)(value - _bonusStats[index][1]);
                                //if (_myPlayer != null)
                                //    _myPlayer.DebugMessage("[RemoveBonusStat]\nUnstacking " + Value + " (resultant " + (Value - _bonusStats[Index][1]) + ") from " + ((statAffil <= 16) ? Enum.GetName(typeof(GameData.Stats), statAffil) : Enum.GetName(typeof(GameData.BonusTypes), statAffil)) + "\nType:" + Enum.GetName(typeof(StatStackGroup), Index) + "\nTotal bonus is now " + _totalBonusStat);
                            }
                            else
                            {
                                _totalBonusStat -= value;
                                //if (_myPlayer != null)
                                //    _myPlayer.DebugMessage("[RemoveBonusStat]\nRemoving " + Value + " from " + ((statAffil <= 16) ? Enum.GetName(typeof(GameData.Stats), statAffil) : Enum.GetName(typeof(GameData.BonusTypes), statAffil)) + "\nType:" + Enum.GetName(typeof(StatStackGroup), Index) + "\nTotal bonus is now " + _totalBonusStat);
                            }
                        }

                        _bonusStats[index].RemoveAt(i);

                        break;
                    }
                }
            }

            public void AddReducedStat(ushort value, byte index)
            {
                // Tactics and Career Mechanics stack with everything

                if (index > 1)
                {
                    _totalReducedStat += value;
                    return;
                }

                if (_reducedStats == null)
                    _reducedStats = new List<ushort>[(byte)BuffClass.MaxBuffClasses];

                if (_reducedStats[index] == null)
                    _reducedStats[index] = new List<ushort>();

                if (_reducedStats[index].Count == 0)
                {
                    _reducedStats[index].Add(value);

                    _totalReducedStat += value;

                    //if (_myPlayer != null)
                    //    _myPlayer.Say("[AddReducedStat]\nRemoving " + Value + " from " + ((statAffil <= 16) ? Enum.GetName(typeof(GameData.Stats), statAffil) : Enum.GetName(typeof(GameData.BonusTypes), statAffil)) + "\nType:" + Enum.GetName(typeof(StatStackGroup), Index) + "\nTotal reduction is now " + _totalReducedStat, SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
                }
                else
                {
                    ushort curFirst = _reducedStats[index][0];

                    for (int i = 0; i <= _reducedStats[index].Count; ++i)
                    {
                        if (i == _reducedStats[index].Count || value >= _reducedStats[index][i])
                        {
                            _reducedStats[index].Insert(i, value);
                            break;
                        }
                    }

                    if (value > curFirst)
                    {
                        _totalReducedStat += (ushort)(value - curFirst);
                        //_myPlayer.Say("[AddReducedStat]\nStacking " + Value + " (resultant " + (Value - curFirst) + ") on " + ((statAffil <= 16) ? Enum.GetName(typeof(GameData.Stats), statAffil) : Enum.GetName(typeof(GameData.BonusTypes), statAffil)) + "\nType:" + Enum.GetName(typeof(StatStackGroup), Index) + "\nTotal reduction is now " + _totalReducedStat, SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
                    }
                }
            }

            public void RemoveReducedStat(ushort value, byte index)
            {
                // Tactics and Career Mechanics stack with everything
                if (index > 1)
                {
                    _totalReducedStat -= value;
                    return;
                }

                for (int i = 0; i < _reducedStats[index].Count; ++i)
                {
                    if (value == _reducedStats[index][i])
                    {
                        if (i == 0)
                        {
                            if (_reducedStats[index].Count > 1)
                                _totalReducedStat -= (ushort)(value - _reducedStats[index][1]);
                            else
                            {
                                _totalReducedStat -= value;
                                //if (_myPlayer != null)
                                //_myPlayer.Say("[RemoveReducedStat]\nRestoring " + Value + " to " + ((statAffil <= 16) ? Enum.GetName(typeof(GameData.Stats), statAffil) : Enum.GetName(typeof(GameData.BonusTypes), statAffil)) + "\nType:" + Enum.GetName(typeof(StatStackGroup), Index) + "\nTotal reduction is now " + _totalReducedStat, SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
                            }
                        }

                        _reducedStats[index].RemoveAt(i);

                        break;
                    }
                }
            }

            #endregion

            #region Multiplicative

            private float _totalBonusMult = 1, _totalReductionMult = 1;
            public float TotalBonusMult => _totalBonusMult;
            public float TotalReductionMult => _reductionBlocks == 0 ? _totalReductionMult : 0;

            public float TotalMultiplier => _totalBonusMult * TotalReductionMult;

            private byte _reductionBlocks;

            public void AddBonusMultiplier(float value, byte index)
            {
                if (_bonusPctStats == null)
                    _bonusPctStats = new List<float>[(byte)BuffClass.MaxBuffClasses];

                if (_bonusPctStats[index] == null)
                    _bonusPctStats[index] = new List<float>();

                if (_bonusPctStats[index].Count == 0)
                {
                    _bonusPctStats[index].Add(value);

                    _totalBonusMult += value;
                }

                // Tactics and Career Mechanics stack with everything
                else if (index > 1)
                    _totalBonusMult += value;

                else
                {
                    float curFirst = _bonusPctStats[index][0];

                    for (int i = 0; i <= _bonusPctStats[index].Count; ++i)
                    {
                        if (i == _bonusPctStats[index].Count || value >= _bonusPctStats[index][i])
                        {
                            _bonusPctStats[index].Insert(i, value);
                            break;
                        }
                    }

                    if (value > curFirst)
                        _totalBonusMult += value - curFirst;
                }
            }

            public void RemoveBonusMultiplier(float value, byte index)
            {
                // Tactics and Career Mechanics stack with everything
                if (index > 1)
                {
                    _totalBonusMult -= value;
                    return;
                }

                for (int i = 0; i < _bonusPctStats[index].Count; ++i)
                {
                    if (value == _bonusPctStats[index][i])
                    {
                        if (i == 0)
                        {
                            if (_bonusPctStats[index].Count > 1)
                                _totalBonusMult -= value - _bonusPctStats[index][1];
                            else _totalBonusMult -= value;
                        }

                        _bonusPctStats[index].RemoveAt(i);

                        break;
                    }
                }
            }

            public void AddReductionMultiplier(float value, byte index)
            {
                if (_reducedPctStats == null)
                    _reducedPctStats = new List<float>[(byte)BuffClass.MaxBuffClasses];

                if (value == 0)
                {
                    _reductionBlocks++;
                    return;
                }

                // Tactics and Career Mechanics stack with everything
                if (index > 1)
                {
                    _totalReductionMult *= value;
                    return;
                }

                if (_reducedPctStats[index] == null)
                    _reducedPctStats[index] = new List<float>();

                if (_reducedPctStats[index].Count == 0)
                {
                    _reducedPctStats[index].Add(value);

                    _totalReductionMult *= value;
                }
                else
                {
                    float curFirst = _reducedPctStats[index][0];

                    for (int i = 0; i <= _reducedPctStats[index].Count; ++i)
                    {
                        if (i == _reducedPctStats[index].Count || value <= _reducedPctStats[index][i])
                        {
                            _reducedPctStats[index].Insert(i, value);
                            break;
                        }
                    }

                    if (value < curFirst)
                        _totalReductionMult *= value / curFirst;
                }
            }

            public void RemoveReductionMultiplier(float value, byte index)
            {
                if (value == 0)
                    _reductionBlocks--;
                // Tactics and Career Mechanics stack with everything
                else if (index > 1)
                    _totalReductionMult *= 1f / value;
                else
                {
                    for (int i = 0; i < _reducedPctStats[index].Count; ++i)
                    {
                        if (value == _reducedPctStats[index][i])
                        {
                            if (i == 0)
                            {
                                _totalReductionMult *= 1f / value;

                                if (_reducedPctStats[index].Count > 1)
                                    _totalReductionMult *= _reducedPctStats[index][1];
                            }

                            _reducedPctStats[index].RemoveAt(i);

                            break;
                        }
                    }
                }
            }

            #endregion

           
        }
        internal void LoadPetOverrides(List<PetStatOverride> list, object v)
        {
            throw new NotImplementedException();
        }

        private readonly ushort[] _baseStats = new ushort[(int)Stats.BaseStatCount];
        private readonly Dictionary<ushort, ushort> _renownStats = new Dictionary<ushort, ushort>();
        private readonly UnitStat[] _statModifiers = new UnitStat[(int)Stats.MaxStatCount];

        public ushort Speed = 100;

        public ushort ItemStatTotal;

        public byte BolsterLevel;
        public float BolsterFactor = 1f;

        public byte HTLStacks;

        /*public void Load(Dictionary<byte, ushort> stats)
        {
            if (IsLoad)
                return;

            foreach (KeyValuePair<byte, ushort> values in stats)
                if (values.Key < _baseStats.Length)
                    _baseStats[values.Key] = values.Value;

            base.Load();
        }*/
        public void Load(List<CharacterInfo_stats> stats)
        { 
            if (IsLoad)
                return;

            foreach (CharacterInfo_stats stat in stats)
            {
                if (stat.StatId < _baseStats.Length)
                    _baseStats[stat.StatId] = stat.StatValue;
            }

            foreach (CharacterInfo_stats stat in stats)
            {
                if (_Owner != null && _Owner is Pet && ((Pet)_Owner).Owner != null && WorldMgr.WorldSettingsMgr.GetGenericSetting(19) == 0)
                {
                    List<PetStatOverride> overrides = CharMgr.GetPetStatOverride(stat.CareerLine);
                    List<PetMasteryModifiers> modifiers = CharMgr.GetPetMasteryModifiers(stat.CareerLine);

                    foreach (PetStatOverride ovr in overrides)
                    {
                        if (stat.StatId == ovr.PrimaryValue)
                        {
                            AddBonusMultiplier((Stats)ovr.PrimaryValue, ovr.SecondaryValue * .01f, BuffClass.Career);
                        }
                    }
                    foreach (PetMasteryModifiers mod in modifiers)
                    {
                        if (stat.StatId == mod.PrimaryValue)
                        {
                            byte[] points = new byte[((Pet)_Owner).Owner.AbtInterface._pointsInTree[mod.MasteryTree]];
                            int modPoints = Math.Abs(mod.PointEnd - mod.PointStart) + 1;
                            int modPtPdct = modPoints - (Math.Abs(mod.PointEnd - points.Length));

                            if (points.Length >= mod.PointEnd)
                            {
                                for (int i = 0; i < modPoints; i++)
                                {
                                    AddBonusMultiplier((Stats)mod.PrimaryValue, mod.MasteryModifierPercent * .01f, BuffClass.Career);
                                    AddBonusStat((Stats)mod.PrimaryValue, mod.MasteryModifierAddition, BuffClass.Career);
                                }
                            }
                            else if (points.Length < mod.PointEnd && points.Length >= mod.PointStart)
                            {
                                for (int i = 0; i < modPtPdct; i++)
                                {
                                    AddBonusMultiplier((Stats)mod.PrimaryValue, mod.MasteryModifierPercent * .01f, BuffClass.Career);
                                    AddBonusStat((Stats)mod.PrimaryValue, mod.MasteryModifierAddition, BuffClass.Career);
                                }
                            }
                        }
                    }
                }
            }
            if (_Owner is Player)
            {
                for (byte i = 0; i < _statModifiers.Length; ++i)
                {
                    if (_statModifiers[i] == null)
                        _statModifiers[i] = new UnitStat(this);
                }
            }

            base.Load();
        }

        #region Interface

        public ushort[] GetStats()
        {
            return (ushort[])_baseStats.Clone();
        }

        public ushort GetBaseStat(byte type)
        {
            return _baseStats[type];
        }

        public void SetBaseStat(Stats type, ushort stat)
        {
            if (type >= Stats.BaseStatCount)
                return;

            _baseStats[(int)type] = stat;
        }

        public void SetRenownStat(Stats bonusType, ushort value)
        {
            byte type = (byte) bonusType;
            if (!_renownStats.ContainsKey(type))
                _renownStats.Add(type, value);
            else
                _renownStats[type] = value;
        }

        public ushort GetCoreStat(Stats type)
        {
            ushort stat;

            //TODO: Consider scaling RenownStats component by bolstering factor (Debolstering fix)
            _renownStats.TryGetValue((ushort)type, out stat);

            if (type < Stats.BaseStatCount)
                stat += _baseStats[(int)type];

            return stat;
        }

        public ushort GetItemStat(Stats statType)
        {
            return GetUnitStat(statType).InternalItemBonus;
        }

        public ushort GetCombinationItemStat(Stats statType)
        {
            UnitStat mainStat = GetUnitStat(statType);

            if (mainStat.ItemAddFromStat == 0)
                return mainStat.InternalItemBonus;
            return (ushort)(mainStat.InternalItemBonus + _statModifiers[mainStat.ItemAddFromStat].InternalItemBonus);
        }

        private UnitStat GetUnitStat(Stats statType)
        {
            byte type = (byte) statType;

            if (_statModifiers[type] == null)
                _statModifiers[type] = new UnitStat(this);
            return _statModifiers[type];
        }

#region Additive

        public int GetStatLinearModifier(Stats statType)
        {
            UnitStat modifier = _statModifiers[(int) statType];

            if (modifier == null)
            {
                _logger.Trace($"Modifier : null");
                return 0;
            }
            else
            {
                _logger.Trace($"Modifier : {modifier.TotalStat} {modifier.TotalMultiplier} {modifier.AvailableItemBonus} {modifier.InternalItemBonus} {modifier.ItemAddFromStat}");
            }

            int linearStat = modifier.TotalStat;

            if (modifier.ItemAddFromStat != 0)
                linearStat += _statModifiers[modifier.ItemAddFromStat].InternalItemBonus;

            _logger.Trace($"HTStacks : {HTLStacks} linearstat {linearStat}" );

            if (HTLStacks == 0 || (statType != Stats.Disrupt && statType != Stats.Evade))
                return linearStat;

            _logger.Trace($"return is {linearStat + Math.Min((byte)3, HTLStacks) * 15}");

            return linearStat + Math.Min((byte)3, HTLStacks) * 15;
        }

        public int GetBonusStat(Stats statType)
        {
            UnitStat modifier = _statModifiers[(int)statType];

            if (modifier == null)
                return 0;

            int bonusStat = modifier.TotalBonusStat;

            if (modifier.ItemAddFromStat != 0)
                bonusStat += _statModifiers[modifier.ItemAddFromStat].InternalItemBonus;

            if (HTLStacks == 0 || (statType != Stats.Disrupt && statType != Stats.Evade))
                return bonusStat;

            return bonusStat + Math.Min((byte)3, HTLStacks) * 15;
        }

        public int GetReducedStat(Stats statType) 
        {
            if (_statModifiers[(int)statType] == null)
                return 0;
            return _statModifiers[(int)statType].TotalReducedStat;
        }

#endregion

#region Percent

        public float GetStatPercentageModifier(Stats statType)
        {
            return GetUnitStat(statType).TotalMultiplier;
        }

        public float GetStatBonusModifier(Stats statType)
        {
            return GetUnitStat(statType).TotalBonusMult - 1;
        }

        public float GetStatReductionModifier(Stats statType)
        {
            return GetUnitStat(statType).TotalReductionMult;
        }

#endregion

        public short GetTotalStat(Stats bonusType)
        {

            if (GetUnit() == null)
            {
                _logger.Trace($" GetUnit is null, returning 0");
                return 0;
            }

            _logger.Trace($" Bonus Type {bonusType}");
            int value = GetCoreStat(bonusType);
            _logger.Trace($" Bonus Type Core Stat value = {value}");

            UnitStat statModifier = _statModifiers[(int)bonusType];
            

            if (statModifier == null)
            {
                _logger.Trace($" statModifier = null {value}");
                return (short) value;
            }
            else
            {
                _logger.Trace($" statModifier = {statModifier.TotalStat}");
            }

            if (bonusType < Stats.BaseStatCount && value + GetStatLinearModifier(bonusType) <= 0)
            {
                _logger.Trace($" base returning 0 ");
                return 0;
            }

            _logger.Trace($"GetStatLinearModifier {GetStatLinearModifier(bonusType)} ");
            _logger.Trace($"statModifier.TotalMultiplier {statModifier.TotalMultiplier} ");

            value = (int)((value + GetStatLinearModifier(bonusType)) * statModifier.TotalMultiplier);
            _logger.Trace($"value {value} ");
            return (short)value;
        }

        public short GiftItemStatTo(Stats statTo, Stats statFrom)
        {
            GetUnitStat(statTo).ItemAddFromStat = (byte)statFrom;
            return (short)GetUnitStat(statFrom).AvailableItemBonus;
        }

        public void RemoveItemStatGift(Stats bonusType)
        {
            _statModifiers[(byte)bonusType].ItemAddFromStat = 0;
        }

        public void DisableItemBonus(Stats bonusType, bool bDisabled)
        {
            GetUnitStat(bonusType).ItemBonusBlock = bDisabled;
        }

        public short SetItemStatMultiplier(Stats bonusType, int mult)
        {
            UnitStat stat = GetUnitStat(bonusType);
            stat.ItemStatMultiplier = mult;
            return (short)(stat.AvailableItemBonus * (stat.ItemStatMultiplier - 1));
        }

#endregion

        public void BuildStats(ref PacketOut Out)
        {
            Out.WriteByte(BolsterLevel); // effective level
            Out.WriteByte(GetPlayer().Level); // level
            for (byte i = 1; i <= _baseStats.Length; ++i)
            {
                Out.WriteByte(i);
                if (i == 10 && (_Owner.GetPlayer().ItmInterface.GetItemInSlot(11) != null && _Owner.GetPlayer().ItmInterface.GetItemInSlot(11).Info.Type == 5))
                { 
                    Out.WriteUInt16((ushort)((_Owner.GetPlayer().ItmInterface.GetItemInSlot(11).Info.Armor / ((7.5 * (BolsterLevel > 0 ? BolsterLevel : GetPlayer().Level) + 50) * 0.2)) * 100));
                    // My spaghetti I will need to dig in that weird bolster armor bug. One day... One day I say...
                    /*int armor = _Owner.GetPlayer().ItmInterface.GetItemInSlot(11).Info.Armor;
                    int bolster = BolsterLevel;
                    int level = GetPlayer().Level;
                    int result = (ushort)((_Owner.GetPlayer().ItmInterface.GetItemInSlot(11).Info.Armor / ((7.5 * (BolsterLevel > 0 ? BolsterLevel : GetPlayer().Level) + 50) * 0.2)) * 100);
                    int ii = 1;
                    ushort simpleresult = (ushort)((armor / ((7.5 * level + 50) * 0.2)) * 100);
                    ushort simpleresult2 = (ushort)((armor / ((7.5 * bolster + 50) * 0.2)) * 100);
                    int iii = 1;*/
                    
                }
                else if (i == 11)
                    Out.WriteUInt16((ushort)((GetTotalStat(Stats.WeaponSkill) / ((7.5 * (BolsterLevel > 0 ? BolsterLevel : GetPlayer().Level) + 50) * 0.075)) * 100));
                else if (i == 12)
                    Out.WriteUInt16((ushort)((GetTotalStat(Stats.Initiative) / ((7.5 * (BolsterLevel > 0 ? BolsterLevel : GetPlayer().Level) + 50) * 0.075)) * 100));
                else if (i == 13)
                    Out.WriteUInt16((ushort)((GetTotalStat(Stats.Willpower) / ((7.5 * (BolsterLevel > 0 ? BolsterLevel : GetPlayer().Level) + 50) * 0.075)) * 100));
                else if (i == _baseStats.Length)
                    Out.WriteUInt16(1);
                else
                    Out.WriteUInt16(_baseStats[i]);
            }
        }

#region Speed Handling

        internal class VelocityModifierComparer : Comparer<Tuple<NewBuff, float>>
        {
            public override int Compare(Tuple<NewBuff, float> first, Tuple<NewBuff, float> second)
            {
                return first.Item2.CompareTo(second.Item2) != 0 ? first.Item2.CompareTo(second.Item2) : first.Item1.BuffId.CompareTo(second.Item1.BuffId);
            }
        }

        private readonly SortedSet<Tuple<NewBuff, float>> _velocityModifiers = new SortedSet<Tuple<NewBuff, float>>(new VelocityModifierComparer());
        private float _velocityMod = 1;

        public float VelocityMod => _velocityMod;

        public virtual void AddVelocityModifier(NewBuff source, float amount)
        {
            _velocityModifiers.Add(new Tuple<NewBuff, float>(source, amount));
            RecalculateVelocityMod();
        }

        public virtual void RemoveVelocityModifier(NewBuff source)
        {
            _velocityModifiers.RemoveWhere(buff => buff.Item1 == source);
            RecalculateVelocityMod();
        }

        public void CheckVelocityModifiers(Player player)
        {
            if (_velocityModifiers.Count == 0)
            {   player.SendClientMessage("No modifiers.");
                return;
            }

            foreach (var item in _velocityModifiers)
                player.SendClientMessage("Entry: "+item.Item1.Entry+", Power: "+item.Item2);
        }

        private void RecalculateVelocityMod()
        {
            if (_velocityModifiers.Count == 0)
                _velocityMod = 1;
            else
            {
                float minMod = _velocityModifiers.Min.Item2;
                float maxMod = _velocityModifiers.Max.Item2;

                if (maxMod <= 1 || minMod == 0f)
                    _velocityMod = minMod;
                else if (minMod >= 1)
                    _velocityMod = maxMod;
                else
                    _velocityMod = maxMod - (1 - minMod);
            }

            if (_Owner is Unit)
                ((Unit) _Owner).UpdateSpeed();
        }

        public bool IsImpeded()
        {
            return _velocityModifiers.Count > 0 && _velocityModifiers.Min.Item2 < 1f;
        }

        public bool IsRooted()
        {
            return _velocityModifiers.Count > 0 && _velocityModifiers.Min.Item2 < 0.1f;
        }

#endregion

#region Additive

        public void AddBonusStat(Stats bonusType, ushort stat, BuffClass statGroup)
        {
            if (bonusType < Stats.MaxStatCount)
                GetUnitStat(bonusType).AddBonusStat(stat, (byte)statGroup);

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

            switch (bonusType)
            {
                case Stats.Wounds:
                    (_Owner as Pet)?.ReceiveHeal(null, (ushort)(stat*10));
                    break;
                case Stats.Mastery1Bonus:
                case Stats.Mastery2Bonus:
                case Stats.Mastery3Bonus:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                    {
                        Player p = (_Owner as Player);
                        if (p != null && p.AbtInterface.Loaded)
                        {
                            p.AbtInterface.RefreshBonusMasteryPoints();
                        }
                    }
                    break;
                // RB   4/17/2016   Added support for adjusting max AP pool
                case Stats.MaxActionPoints:
                        if (_Owner.Loaded && _Owner.IsPlayer())
                        {
                            Player p = (_Owner as Player);
                            p.MaxActionPoints += stat;
                        }
                    break;
            }
                    
        }

        public void RemoveBonusStat(Stats bonusType, ushort stat, BuffClass statGroup)
        {
            if (bonusType < Stats.MaxStatCount)
                _statModifiers[(byte)bonusType].RemoveBonusStat(stat, (byte)statGroup);

            if (bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

            switch (bonusType)
            {
                case Stats.Wounds:
                case Stats.Armor:
                    ApplyStats();
                    break;
                case Stats.Mastery1Bonus:
                case Stats.Mastery2Bonus:
                case Stats.Mastery3Bonus:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                    {
                        Player p = (_Owner as Player);
                        if (p != null && p.AbtInterface.Loaded)
                        {
                            p.AbtInterface.RefreshBonusMasteryPoints();
                        }
                    }
                    break;

                // RB   4/17/2016   Added support for adjusting max AP pool
                case Stats.MaxActionPoints:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                    {
                        Player p = (_Owner as Player);
                        p.MaxActionPoints -= stat;
                    }
                    break;
            }
        }

        public void AddReducedStat(Stats bonusType, ushort stat, BuffClass statGroup)
        {
                if (bonusType < Stats.MaxStatCount)
                GetUnitStat(bonusType).AddReducedStat(stat, (byte)statGroup);

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

            switch (bonusType)
            {
                case Stats.ArmorPenetration:
                    int ArmPen = (int)((Player)_Owner).StsInterface.GetTotalStat(Stats.ArmorPenetration);
                    if (_Owner.Loaded && _Owner.IsPlayer())
                        ArmPen -= stat;
                    break;
                case Stats.MaxActionPoints:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                        ((Player) _Owner).MaxActionPoints -= stat;
                    break;
            }
        }

        public void RemoveReducedStat(Stats bonusType, ushort stat, BuffClass statGroup)
        {
            if (bonusType < Stats.MaxStatCount)
                _statModifiers[(byte)bonusType].RemoveReducedStat(stat, (byte)statGroup);

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

            switch (bonusType)
            {
                case Stats.ArmorPenetration:
                    int ArmPen = (int)((Player)_Owner).StsInterface.GetTotalStat(Stats.ArmorPenetration);
                    if (_Owner.Loaded && _Owner.IsPlayer())
                        ArmPen += stat;
                    break;
                case Stats.MaxActionPoints:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                    {
                        Player p = (_Owner as Player);
                        p.MaxActionPoints += stat;
                    }
                    break;
            }
        }

        /// <summary>
        /// Add item bonuses to stats
        /// </summary>
        /// <param name="bonusType"></param>
        /// <param name="stat"></param>
        /// <param name="percentage">0:Additive, 1:Multiplicative</param>
        public void AddItemBonusStat(Stats bonusType, ushort stat, ushort percentage=0)
        {
            if (bonusType < Stats.MaxStatCount)
            {
                if (percentage == 0)
                {
                    GetUnitStat(bonusType).AddItemBonusStat(stat);
                }
                else
                {
                     AddBonusMultiplier(Stats.OutgoingDamagePercent, stat/100f, BuffClass.Career);
                    //GetUnitStat(bonusType).AddItemBonusStat(stat, percentage);
                }
            }

            if (bonusType <= Stats.Intelligence)
                ItemStatTotal += stat;

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

            switch (bonusType)
            {
                case Stats.Mastery1Bonus:
                case Stats.Mastery2Bonus:
                case Stats.Mastery3Bonus:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                    {
                        Player p = (_Owner as Player);
                        if (p != null && p.AbtInterface.Loaded)
                        {
                            p.AbtInterface.RefreshBonusMasteryPoints();
                        }
                    }
                    break;
                case Stats.MaxActionPoints:
                        if (_Owner.Loaded && _Owner.IsPlayer())
                        {
                            Player p = (_Owner as Player);
                            p.MaxActionPoints += stat;
                        }
                    break;
            }
        }

        public void RemoveItemBonusStat(Stats bonusType, ushort stat, ushort percentage=0)
        {
            byte type = (byte)bonusType;

            if (bonusType < Stats.MaxStatCount)
            {
                if (percentage == 0)
                {
                    _statModifiers[type].RemoveItemBonusStat(stat);
                }
                else
                {
                    RemoveBonusMultiplier(Stats.OutgoingDamagePercent, stat/100f, BuffClass.Career);
                    //_statModifiers[type].RemoveItemBonusStat(stat, percentage);
                }
            }


            if (bonusType <= Stats.Intelligence)
                ItemStatTotal -= stat;

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

            switch (bonusType)
            {
                case Stats.Mastery1Bonus:
                case Stats.Mastery2Bonus:
                case Stats.Mastery3Bonus:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                    {
                        Player p = (_Owner as Player);
                        if (p != null && p.AbtInterface.Loaded)
                        {
                            p.AbtInterface.RefreshBonusMasteryPoints();
                        }
                    }
                    break;
                case Stats.MaxActionPoints:
                    if (_Owner.Loaded && _Owner.IsPlayer())
                        {
                            Player p = (_Owner as Player);
                            p.MaxActionPoints -= stat;
                        }
                    break;
            }
        }

#endregion

#region Multiplicative

        public void AddBonusMultiplier(Stats bonusType, float mult, BuffClass statGroup)
        {
            if (bonusType < Stats.MaxStatCount)
                GetUnitStat(bonusType).AddBonusMultiplier(mult, (byte)statGroup);

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

        }
        public void RemoveBonusMultiplier(Stats bonusType, float mult, BuffClass statGroup)
        {
            byte type = (byte)bonusType;

            if (bonusType < Stats.MaxStatCount)
                if (_statModifiers[type] != null)
                    _statModifiers[type].RemoveBonusMultiplier(mult, (byte)statGroup);

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();
        }
        public void AddReducedMultiplier(Stats bonusType, float mult, BuffClass statGroup)
        {
            if (bonusType < Stats.MaxStatCount)
                GetUnitStat(bonusType).AddReductionMultiplier(mult, (byte)statGroup);

            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();

        }
        public void RemoveReducedMultiplier(Stats bonusType, float mult, BuffClass statGroup)
        {
            byte type = (byte)bonusType;

            if (bonusType < Stats.MaxStatCount)
                _statModifiers[type].RemoveReductionMultiplier(mult, (byte)statGroup);
            if (bonusType == Stats.Wounds || bonusType == Stats.Armor || bonusType == Stats.Willpower || bonusType == Stats.WeaponSkill || bonusType == Stats.Initiative || bonusType == Stats.Strength || bonusType == Stats.Toughness || bonusType == Stats.BallisticSkill)
                ApplyStats();
        }

#endregion

#region Ability

        /// <summary>
        /// Modifies the cast time, cooldown, range and AP cost of an ability according to stats.
        /// </summary>
        /// <param name="abInfo"></param>
        public void ModifyAbilityVolatiles(AbilityInfo abInfo)
        {
            if (abInfo.ConstantInfo.ChannelID == 0)
            {
                int castMod = GetStatLinearModifier(Stats.BuildTime);

                if (abInfo.CastTime + castMod <= 0)
                    abInfo.CastTime = 0;
                else
                    abInfo.CastTime = (ushort)((abInfo.CastTime + castMod) * GetStatPercentageModifier(Stats.BuildTime));

                if (abInfo.CastTime == 0)
                    abInfo.CanCastWhileMoving = true;
            }

            if (abInfo.Entry != 9553 && abInfo.Entry != 8239 && abInfo.Entry != 9035 && abInfo.Entry != 1734) // exempt divine mend, khaine's invigoration, sudden shift and changin' da plan
            {
                short cooldownMod = (short) (GetStatLinearModifier(Stats.Cooldown)/1000);
                if (abInfo.Cooldown + cooldownMod <= 0)
                    abInfo.Cooldown = 0;
                else
                    abInfo.Cooldown = (ushort) ((abInfo.Cooldown + cooldownMod)*(GetStatPercentageModifier(Stats.Cooldown)));
            }

            float rangeMod = GetStatPercentageModifier(Stats.Range);

            abInfo.FlightTimeMod = 1f/rangeMod;

            if (abInfo.Range != 0)
                abInfo.Range = (ushort) (abInfo.Range * rangeMod);
            if (abInfo.ApCost != 0)
                abInfo.ApCost = (byte) ((abInfo.ApCost + GetTotalStat(Stats.ActionPointCost))*GetStatPercentageModifier(Stats.ActionPointCost));

        }

#endregion

        public void ApplyStats()
        {
            if (_Owner.IsUnit() && !_Owner.IsGameObject())
            {
                Unit unit = GetUnit();
                unit.MaxHealth = (uint)(Math.Max(10, GetTotalStat(Stats.Wounds) * 10));
                if (unit.Health > unit.MaxHealth)
                    unit.Health = unit.MaxHealth;
            }

            if (_Owner.Loaded && _Owner.IsPlayer())
                _Owner.GetPlayer().SendStats();
        }

        public void SendRenownStats()
        {
            //Log.Info("RenownStats", "Sending from "+_Owner.Name);
            PacketOut Out = new PacketOut((byte)Opcodes.F_BAG_INFO, 800);
            Out.WriteByte(0x19);
            Out.WriteByte(0x62);

            for (ushort i = 0; i < 98; ++i)
            {
                if (_renownStats.ContainsKey(i))
                {
                    if (i == (int)Stats.Range)
                        Out.WriteUInt32R((uint)(_renownStats[i] * 12));
                    else
                        Out.WriteUInt32R(_renownStats[i]);
                }
                else
                    Out.WriteUInt32R(0);

                Out.WriteUInt16(0);
                Out.WriteUInt16(0x803F);
            }

            ((Player)_Owner).SendPacket(Out);
        }
    }
}
