/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WarShared.Readers
{
    public enum RequirmentType
    {
        HaveItem = 70,
    }

    public class AbilityBin
    {
        public ushort A40_AbilityID { get; internal set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint A00_Castime { get; set; }
        public uint A04_Cooldown { get; set; }
        public TacticType A08_TacticType { get; set; }
        public TargetType A12_TargetType { get; set; }
        public AbilityType A16_AbilityType { get; set; }
        public AbilityAttackType AttackType { get; set; }
        public uint A24 { get; set; }
        public CareerLine A28_CareerID { get; set; }
        public uint CounterAmount { get; set; }
        public uint A36_Flags { get; set; }
        public string Flags = "";
        public bool RequiresPet;
        public bool IsBuffDebuff;
        public bool IsBuff;
        public bool IsDebuff;
        public bool IsDamaging;
        public bool IsHealing;
        public bool IsDefensive;
        public bool IsOffensive;
        public bool IsStatsBuff;
        public bool IsGranted;
        public bool IsPassive;

        public ushort A42_EffectID { get; set; }
        public ushort A44 { get; set; }
        public ushort A46_Range { get; set; }
        public ushort A48_Angle { get; set; }
        public ushort A50_MoraleCost { get; set; }
        public ushort A52_ChannelInterval { get; set; }
        public ushort A54 { get; set; }
        public ushort A56_ScaleStatMult { get; set; }
        public byte A58_NumTacticSlots { get; set; }
        public byte A59_MoraleLevel;
        public byte A60_AP { get; set; }
        public byte A61 { get; set; }
        public byte A62 { get; set; }
        public byte Faction { get; set; }
        public byte A64_AbilityImprovementThreshold { get; set; }
        public byte A65_AbilityImprovementCap { get; set; }
        public byte A66_Specialization { get; set; }
        public byte A67_StanceOrder { get; set; }
        public byte A68 { get; set; }
        public byte A69_MinLevel { get; set; }
        public byte A70 { get; set; }
        public byte A71 { get; set; }
        public ushort[] ComponentIDs { get; set; } = new ushort[10];
        public uint[] Triggers { get; set; } = new uint[10];
        public ushort[] UsableWithBuff { get; set; } = new ushort[0x4];
        public uint A136 { get; set; }
        public ushort A140 { get; set; }
        public ushort[] Groups { get; set; } = new ushort[04]; //bytes 0-3 (curse, hex, ail, etc)
        public uint A142C { get; set; }
        public string DamageType = "";
        public ushort[] Labels { get; set; } = new ushort[5];
        public byte[] ComponentVfx { get; set; } = new byte[0xA];
        public ushort A170;

        public Dictionary<int, ExtData> Data { get; set; } = new Dictionary<int, ExtData>();


        public override string ToString()
        {
            return Name.ToString();
        }



        public List<ExtData> DataList
        {
            get
            {
                return Data.Values.Where(e => e != null).ToList();
            }
        }
        public Dictionary<int, AbilityComponent> Components = new Dictionary<int, AbilityComponent>();

        public AbilityBin()
        {
        }

        public AbilityBin(ushort ID, AbilityBin cloneFrom, MYPManager manager, MythicPackage package, AbilityComponentExport compExpSource, AbilityComponentExport compExpDest)
        {
            A40_AbilityID = ID;
            if (cloneFrom != null)
            {
                Name = cloneFrom.Name;
                Description = cloneFrom.Description;
                A00_Castime = cloneFrom.A00_Castime;
                A04_Cooldown = cloneFrom.A04_Cooldown;
                A08_TacticType = cloneFrom.A08_TacticType;
                A12_TargetType = cloneFrom.A12_TargetType;
                A16_AbilityType = cloneFrom.A16_AbilityType;
                AttackType = cloneFrom.AttackType;
                A24 = cloneFrom.A24;
                A28_CareerID = cloneFrom.A28_CareerID;
                CounterAmount = cloneFrom.CounterAmount;
                A36_Flags = cloneFrom.A36_Flags;
                A42_EffectID = cloneFrom.A42_EffectID;
                A44 = cloneFrom.A44;
                A46_Range = cloneFrom.A46_Range;
                A48_Angle = cloneFrom.A48_Angle;
                A50_MoraleCost = cloneFrom.A50_MoraleCost;
                A52_ChannelInterval = cloneFrom.A52_ChannelInterval;
                A54 = cloneFrom.A54;
                A56_ScaleStatMult = cloneFrom.A56_ScaleStatMult;
                A58_NumTacticSlots = cloneFrom.A58_NumTacticSlots;
                A59_MoraleLevel = cloneFrom.A59_MoraleLevel;
                A60_AP = cloneFrom.A60_AP;
                A61 = cloneFrom.A61;
                A62 = cloneFrom.A62;
                Faction = cloneFrom.Faction;
                A64_AbilityImprovementThreshold = cloneFrom.A64_AbilityImprovementThreshold;
                A65_AbilityImprovementCap = cloneFrom.A65_AbilityImprovementCap;
                A66_Specialization = cloneFrom.A66_Specialization;
                A67_StanceOrder = cloneFrom.A67_StanceOrder;
                A68 = cloneFrom.A68;
                A69_MinLevel = cloneFrom.A69_MinLevel;
                A70 = cloneFrom.A70;
                A71 = cloneFrom.A71;

                for (int i = 0; i < 10; i++)
                {
                    if (cloneFrom.ComponentIDs[i] != 0 && compExpSource.CompHash.ContainsKey(cloneFrom.ComponentIDs[i]))
                    {
                        var comp = compExpDest.NewComponent(manager, package, this, compExpSource.CompHash[cloneFrom.ComponentIDs[i]]);
                        ComponentIDs[i] = comp.A11_ComponentID;
                    }
                }

                for (int i = 0; i < 10; i++)
                    Triggers[i] = cloneFrom.Triggers[i];

                for (int i = 0; i < 8; i++)
                    UsableWithBuff[i] = cloneFrom.UsableWithBuff[i];

                A136 = cloneFrom.A136;
                A140 = cloneFrom.A140;

                for (int i = 0; i < 4; i++)
                    Groups[i] = cloneFrom.Groups[i];

                A142C = cloneFrom.A142C;

                for (int i = 0; i < 5; i++)
                    Labels[i] = cloneFrom.Labels[i];

                for (int i = 0; i < 10; i++)
                    ComponentVfx[i] = cloneFrom.ComponentVfx[i];

                A170 = cloneFrom.A170;

                foreach (var data in cloneFrom.Data)
                    Data[data.Key] = new ExtData(data.Value);

                foreach (Language language in Enum.GetValues(typeof(Language)))
                {
                    string pathNames = "data/strings/" + language + "/abilitynames.txt";
                    string pathDesc = "data/strings/" + language + "/abilitydesc.txt";

                    var str = manager.GetString(language, pathNames, cloneFrom.A40_AbilityID, package);

                    //update name
                    if (str != null && str.Length > 0)
                    {
                        manager.SetString(language, pathNames, A40_AbilityID, str, package);
                        var doc = manager.GetString(language, pathNames);
                        manager.UpdateAsset(package, pathNames, System.Text.UnicodeEncoding.Unicode.GetPreamble().Concat(System.Text.UnicodeEncoding.Unicode.GetBytes(doc)).ToArray());
                    }

                    //update description
                    str = manager.GetString(language, pathDesc, cloneFrom.A40_AbilityID, package);

                    if (str != null && str.Length > 0)
                    {
                        manager.SetString(language, pathDesc, A40_AbilityID, str, package);
                        var doc = manager.GetString(language, pathDesc);
                        manager.UpdateAsset(package, pathDesc,  System.Text.UnicodeEncoding.Unicode.GetPreamble().Concat(System.Text.UnicodeEncoding.Unicode.GetBytes(doc)).ToArray());
                    }
                }
            }
        }

        public static string ListToString(List<object> arr)
        {
            string result = "";
            foreach (var val in arr)
            {
                if (val is uint)
                {
                    var v = (uint)val;
                    if (v != 0XAAAAAAAA)
                        result += v.ToString() + " ";
                    else
                        result += "| ";
                }
                else if (val is ushort)
                {
                    var v = (ushort)val;

                    result += v.ToString() + " ";

                }
                else if (val is byte)
                {
                    var v = (byte)val;

                    result += v.ToString() + " ";

                }
            }
            return result;
        }
        public static string ListToString(byte[] arr)
        {
            string result = "";
            foreach (var val in arr)
            {

                var v = (byte)val;

                result += v.ToString() + " ";


            }
            return result;
        }
        public static string ListToString(ushort[] arr)
        {
            string result = "";
            foreach (var val in arr)
            {

                var v = (ushort)val;

                result += v.ToString() + " ";


            }
            return result;
        }
        public static string ListToString(short[] arr)
        {
            string result = "";
            foreach (var val in arr)
            {

                var v = (short)val;

                result += v.ToString() + " ";


            }
            return result;
        }
        public static string ListToString(uint[] arr)
        {
            string result = "";
            foreach (var val in arr)
            {

                var v = (uint)val;

                result += v.ToString() + " ";


            }
            return result;
        }

        public AbilityBin(Stream stream)
        {
            var pos1 = stream.Position;

            A00_Castime = PacketUtil.GetUint32R(stream);
            A04_Cooldown = PacketUtil.GetUint32R(stream);
            A08_TacticType = (TacticType)PacketUtil.GetUint32R(stream); //career, renown tome
            A12_TargetType = (TargetType)PacketUtil.GetUint32R(stream);
            A16_AbilityType = (AbilityType)PacketUtil.GetUint32R(stream);
            AttackType = (AbilityAttackType)PacketUtil.GetUint32R(stream);
            A24 = PacketUtil.GetUint32R(stream);
            A28_CareerID = (CareerLine)PacketUtil.GetUint32R(stream);
            CounterAmount = PacketUtil.GetUint32R(stream);
            A36_Flags = PacketUtil.GetUint32R(stream);
            Flags = Convert.ToString(A36_Flags, 2).PadLeft(32, '0');

            IsGranted = (A36_Flags & 1) == 1;
            IsPassive = (A36_Flags & 2) == 2;
            IsBuff = (A36_Flags & 4) == 4;
            IsDebuff = (A36_Flags & 8) == 8;
            IsDamaging = (A36_Flags & 0x10) == 0x10;
            IsHealing = (A36_Flags & 0x20) == 0x20;
            RequiresPet = (A36_Flags & 0x100) == 0x100;
            IsDefensive = (A36_Flags & 0x40) == 0x40;
            IsOffensive = (A36_Flags & 0x80) == 0x80;
            IsStatsBuff = (A36_Flags & 0x40000) == 0x40000;
            IsBuffDebuff = ((A36_Flags >> 21) & 0x1) == 1;


            A40_AbilityID = PacketUtil.GetUint16R(stream);
            A42_EffectID = PacketUtil.GetUint16R(stream);
            A44 = PacketUtil.GetUint16R(stream);
            A46_Range = PacketUtil.GetUint16R(stream);
            A48_Angle = PacketUtil.GetUint16R(stream);
            A50_MoraleCost = PacketUtil.GetUint16R(stream);
            A52_ChannelInterval = PacketUtil.GetUint16R(stream);
            A54 = PacketUtil.GetUint16R(stream);
            A56_ScaleStatMult = PacketUtil.GetUint16R(stream);
            A58_NumTacticSlots = PacketUtil.GetUint8(stream);//tactic slots
            A59_MoraleLevel = PacketUtil.GetUint8(stream);//morale level
            A60_AP = PacketUtil.GetUint8(stream);
            A61 = PacketUtil.GetUint8(stream);
            A62 = PacketUtil.GetUint8(stream);
            Faction = PacketUtil.GetUint8(stream);
            A64_AbilityImprovementThreshold = PacketUtil.GetUint8(stream);
            A65_AbilityImprovementCap = PacketUtil.GetUint8(stream);
            A66_Specialization = PacketUtil.GetUint8(stream);
            A67_StanceOrder = PacketUtil.GetUint8(stream);
            A68 = PacketUtil.GetUint8(stream);
            A69_MinLevel = PacketUtil.GetUint8(stream);

            for (int i = 0; i < 10; i++)
                ComponentIDs[i] = PacketUtil.GetUint16R(stream);


            for (int i = 0; i < 10; i++)
                Triggers[i] = PacketUtil.GetUint32R(stream);



            for (int i = 0; i < 4; i++)
                UsableWithBuff[i] = PacketUtil.GetUint16R(stream);

            A136 = PacketUtil.GetUint32R(stream);
            A140 = PacketUtil.GetUint16R(stream);


            for (int i = 0; i < 4; i++)
                Groups[i] = PacketUtil.GetUint16R(stream);
            //   A142C = BitConverter.ToUInt32(A142, 0);


            for (int i = 0; i < 5; i++)
                Labels[i] = PacketUtil.GetUint16R(stream);

            ComponentVfx = PacketUtil.GetByteArray(stream, 0xA);

            A170 = PacketUtil.GetUint16R(stream);

            for (int i = 0; i < 5; i++)
            {
                var d = PacketUtil.GetUint32R(stream);

                if (d == 0XAAAAAAAA)
                {
                    Data[i] = new ExtData()
                    {
                        Type = (AbilitySourceType)PacketUtil.GetUint32R(stream),
                        Operation = (AbilityOperation)PacketUtil.GetUint32R(stream),
                        Condition = (AbilityCondition )PacketUtil.GetUint32R(stream),
                        LogicOperator = (AbilityLogicOperator)PacketUtil.GetUint32R(stream),
                        Val5 = (int)PacketUtil.GetUint32R(stream),
                        Val6 = (int)PacketUtil.GetUint32R(stream),
                        Val7 = (int)PacketUtil.GetUint32R(stream),
                        Val8 = (int)PacketUtil.GetUint32R(stream),
                        Val9 = PacketUtil.GetUint8(stream),
                    };
                }

            }

            var pos2 = stream.Position;

            var total = pos2 - pos1;

        }

        public void Save(Stream stream)
        {

            PacketUtil.WriteUInt32R(stream, A00_Castime);
            PacketUtil.WriteUInt32R(stream, A04_Cooldown);
            PacketUtil.WriteUInt32R(stream, (uint)A08_TacticType);
            PacketUtil.WriteUInt32R(stream, (uint)A12_TargetType);
            PacketUtil.WriteUInt32R(stream, (uint)A16_AbilityType);
            PacketUtil.WriteUInt32R(stream, (uint)AttackType);
            PacketUtil.WriteUInt32R(stream, A24);
            PacketUtil.WriteUInt32R(stream, (uint)A28_CareerID);
            PacketUtil.WriteUInt32R(stream, CounterAmount);
            PacketUtil.WriteUInt32R(stream, A36_Flags);
            PacketUtil.WriteUInt16R(stream, A40_AbilityID);
            PacketUtil.WriteUInt16R(stream, A42_EffectID);
            PacketUtil.WriteUInt16R(stream, A44);
            PacketUtil.WriteUInt16R(stream, A46_Range);
            PacketUtil.WriteUInt16R(stream, A48_Angle);
            PacketUtil.WriteUInt16R(stream, A50_MoraleCost);
            PacketUtil.WriteUInt16R(stream, A52_ChannelInterval);
            PacketUtil.WriteUInt16R(stream, A54);
            PacketUtil.WriteUInt16R(stream, A56_ScaleStatMult);
            PacketUtil.WriteByte(stream, A58_NumTacticSlots);
            PacketUtil.WriteByte(stream, A59_MoraleLevel);
            PacketUtil.WriteByte(stream, A60_AP);
            PacketUtil.WriteByte(stream, A61);
            PacketUtil.WriteByte(stream, A62);
            PacketUtil.WriteByte(stream, Faction);
            PacketUtil.WriteByte(stream, A64_AbilityImprovementThreshold);
            PacketUtil.WriteByte(stream, A65_AbilityImprovementCap);
            PacketUtil.WriteByte(stream, A66_Specialization);
            PacketUtil.WriteByte(stream, A67_StanceOrder);
            PacketUtil.WriteByte(stream, A68);
            PacketUtil.WriteByte(stream, A69_MinLevel);

            for (int i = 0; i < 10; i++)
                PacketUtil.WriteUInt16R(stream, ComponentIDs[i]);

            for (int i = 0; i < 10; i++)
                PacketUtil.WriteUInt32R(stream, Triggers[i]);

            for(int i=0; i<4; i++)
                PacketUtil.WriteUInt16R(stream, UsableWithBuff[i]);

            PacketUtil.WriteUInt32R(stream, A136);
            PacketUtil.WriteUInt16R(stream, A140);
            for (int i = 0; i < 4; i++)
                PacketUtil.WriteUInt16R(stream, Groups[i]);

            for (int i = 0; i < 5; i++)
                PacketUtil.WriteUInt16R(stream, Labels[i]);

            PacketUtil.WriteBytes(stream, ComponentVfx);
            PacketUtil.WriteUInt16R(stream, A170);

            for (int i = 0; i < 5; i++)
            {

                if (Data.ContainsKey(i))
                {
                    PacketUtil.WriteUInt32R(stream, 0XAAAAAAAA);
                    Data[i].Save(stream);
                }
                else
                    PacketUtil.WriteUInt32R(stream, 0);
            }

            //PacketUtil.WriteUInt32R(stream, 5);
            //PacketUtil.WriteUInt32R(stream, 6);
            //PacketUtil.WriteUInt32R(stream, 7);
        }


    }

    public class AbilityExport
    {
        public uint Header;
        public uint Size;
        public List<AbilityBin> Abilities = new List<AbilityBin>();
        public Dictionary<uint, AbilityBin> AbilitiesByID = new Dictionary<uint, AbilityBin>();
        public byte[] Original = null;
        public void Load(MYPManager manager, Stream stream)
        {
            Original = new byte[stream.Length];
            stream.Read(Original, 0, (int)stream.Length);
            stream.Position = 0;
            var pos = stream.Position;
            var data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            stream.Position = pos;
            Header = PacketUtil.GetUint32R(stream);
            Size = PacketUtil.GetUint32R(stream);

            while (stream.Position < stream.Length)
            {
                var a = new AbilityBin(stream);
                Abilities.Add(a);
                if (a.A40_AbilityID != 0 && manager != null)
                {
                    a.Name = manager.GetString(Language.english, "abilitynames.txt", a.A40_AbilityID).Replace("^n", "");
                    a.Description = manager.GetString(Language.english, "abilitydesc.txt", a.A40_AbilityID).Replace("^n", "");
                }
                AbilitiesByID[a.A40_AbilityID] = a;
            }
            if (stream.Position < stream.Length)
                throw new Exception($"Unexpected {nameof(AbilityExport)} size");
        }

        public void Save(Stream stream)
        {
            PacketUtil.WriteUInt32R(stream, 0x24);
            PacketUtil.WriteUInt32R(stream, (uint)Abilities.Count);
            for (int i = 0; i < Abilities.Count; i++)
            {
                var ability = Abilities[i];
                ability.Save(stream);
            }
        }

        public AbilityBin NewAbility(MYPManager manager, MythicPackage package, AbilityBin cloneFrom, AbilityComponentExport compExpSource, AbilityComponentExport compExpDest)
        {
            ushort id = 1;
            if (Abilities.Count > 0)
                id = (ushort)(Abilities.Select(e => e.A40_AbilityID).Max() + 1);
            AbilityBin ability = new AbilityBin(id, cloneFrom, manager, package, compExpSource, compExpDest);
            Abilities.Add(ability);

            return ability;
        }
    }

    public class ExtData
    {
        public AbilitySourceType Type { get; set; }
        public AbilityOperation Operation { get; set; }
        public AbilityCondition Condition { get; set; }
        public AbilityLogicOperator LogicOperator { get; set; }
        public int Val5 { get; set; }
        public int Val6 { get; set; }
        public int Val7 { get; set; }
        public int Val8 { get; set; }
        public byte Val9 { get; set; }

        public void Save(Stream stream)
        {
            PacketUtil.WriteUInt32R(stream, (uint)Type);
            PacketUtil.WriteUInt32R(stream, (uint)Operation);
            PacketUtil.WriteUInt32R(stream, (uint)Condition);

            PacketUtil.WriteUInt32R(stream, (uint)LogicOperator);
            PacketUtil.WriteUInt32R(stream, (uint)Val5);
            PacketUtil.WriteUInt32R(stream, (uint)Val6);

            PacketUtil.WriteUInt32R(stream, (uint)Val7);
            PacketUtil.WriteUInt32R(stream, (uint)Val8);
            PacketUtil.WriteByte(stream, Val9);
        }

        public ExtData()
        {
        }

        public ExtData(ExtData loadFrom)
        {
            Type = loadFrom.Type;
            Operation = loadFrom.Operation;
            Condition = loadFrom.Condition;
            LogicOperator = loadFrom.LogicOperator;
            Val5 = loadFrom.Val5;
            Val6 = loadFrom.Val6;
            Val7 = loadFrom.Val7;
            Val8 = loadFrom.Val8;
            Val9 = loadFrom.Val9;
        }
        public override string ToString()
        {
            return $"{Type.ToString()} {Operation.ToString()} {Condition.ToString()} {LogicOperator.ToString()} {Val5.ToString().PadLeft(3, '0')} {Val6.ToString().PadLeft(3, '0')} {Val7.ToString().PadLeft(3, '0')} {Val8.ToString().PadLeft(3, '0')} {Val9.ToString().PadLeft(3, '0')}";
        }
    }

    public class AbilityComponent
    {
        public AbilityBin Ability;
        public byte Index { get; set; }
        public ushort A11_ComponentID { get; internal set; }
        public string Description;
        public ushort A00 { get; set; }
        public Dictionary<int, ExtData> A02_Data { get; set; } = new Dictionary<int, ExtData>();
        public List<ExtData> Data
        {
            get
            {
                return A02_Data.Values.Where(e => e != null).ToList();
            }
        }
        public int[] Values { get; set; } = new int[8];
        public int[] Multipliers { get; set; } = new int[8];
        // public uint[] A04 { get; set; } = new uint[8];
        public uint ActivationDelay { get; set; }//A05
        public uint A06_Duration { get; set; }
        public uint Flags { get; set; }
        public uint A08 { get; set; }
        public ComponentOperationType A09_Operation { get; set; }
        public uint A10_Interval { get; set; }

        public ushort A12_Radius { get; set; }
        public ushort ConeAngle { get; set; }//A13
        public ushort FlightSpeed { get; set; }//A14
        public ushort A15 { get; set; }
        public byte MaxTargets { get; set; }//A16

        public BonusType BonusType { get; internal set; }

        public string LinkedAbility { get; internal set; }

        public override string ToString()
        {
            return A09_Operation.ToString();
        }
        public AbilityComponent(ushort ID, AbilityComponent cloneFrom = null)
        {
            A11_ComponentID = ID;
            if (cloneFrom != null)
            {
                A00 = cloneFrom.A00;

                foreach (var data in cloneFrom.A02_Data)
                {
                    A02_Data.Add(data.Key, new ExtData()
                    {
                        Type = data.Value.Type,
                        Operation = data.Value.Operation,
                        Condition = data.Value.Condition,
                        LogicOperator = data.Value.LogicOperator,
                        Val5 = data.Value.Val5,
                        Val6 = data.Value.Val6,
                        Val7 = data.Value.Val7,
                        Val8 = data.Value.Val8,
                        Val9 = data.Value.Val9,
                    });
                }

                for (int i = 0; i < 8; i++)
                    Values[i] = cloneFrom.Values[i];


                for (int i = 0; i < 8; i++)
                    Multipliers[i] = cloneFrom.Multipliers[i];

                ActivationDelay = cloneFrom.ActivationDelay;
                A06_Duration = cloneFrom.A06_Duration;
                Flags = cloneFrom.Flags;
                A08 = cloneFrom.A08;
                A09_Operation = cloneFrom.A09_Operation;
                A10_Interval = cloneFrom.A10_Interval;
                A12_Radius = cloneFrom.A12_Radius;
                ConeAngle = cloneFrom.ConeAngle;
                FlightSpeed = cloneFrom.FlightSpeed;
                A15 = cloneFrom.A15;
                MaxTargets = cloneFrom.MaxTargets;
                Description = cloneFrom.Description;
                BonusType = cloneFrom.BonusType;
                LinkedAbility = cloneFrom.LinkedAbility;
            }
        }


        public AbilityComponent(Stream stream)
        {
            A00 = PacketUtil.GetUint16R(stream);

            for (int i = 0; i < 8; i++)
            {
                
                var d = PacketUtil.GetUint32R(stream);

                if (d == 0XAAAAAAAA)
                {
                    var data = new ExtData()
                    {
                        Type = (AbilitySourceType)PacketUtil.GetUint32R(stream),
                        Operation = (AbilityOperation)PacketUtil.GetUint32R(stream),
                        Condition = (AbilityCondition)PacketUtil.GetUint32R(stream),
                        LogicOperator = (AbilityLogicOperator)PacketUtil.GetUint32R(stream),
                        Val5 = (int)PacketUtil.GetUint32R(stream),
                        Val6 = (int)PacketUtil.GetUint32R(stream),
                        Val7 = (int)PacketUtil.GetUint32R(stream),
                        Val8 = (int)PacketUtil.GetUint32R(stream),
                        Val9 = PacketUtil.GetUint8(stream),
                    };
                    A02_Data[i] = data;
                }
            }
            var pos = stream.Position;
            var datda = new byte[8 * 4];
            stream.Read(datda, 0, datda.Length);
            stream.Position = pos;

            for (int i = 0; i < 8; i++)
                Values[i] = (int)PacketUtil.GetUint32R(stream);

            for (int i = 0; i < 8; i++)
                Multipliers[i] = (int)PacketUtil.GetUint32R(stream);

            ActivationDelay = PacketUtil.GetUint32R(stream);
            A06_Duration = PacketUtil.GetUint32R(stream);
            Flags = PacketUtil.GetUint32R(stream);
            A08 = PacketUtil.GetUint32R(stream);
            A09_Operation = (ComponentOperationType)PacketUtil.GetUint32R(stream);
            A10_Interval = PacketUtil.GetUint32R(stream);

            A11_ComponentID = PacketUtil.GetUint16R(stream);

            A12_Radius = PacketUtil.GetUint16R(stream);
            ConeAngle = PacketUtil.GetUint16R(stream);
            FlightSpeed = PacketUtil.GetUint16R(stream);
            A15 = PacketUtil.GetUint16R(stream);

            MaxTargets = PacketUtil.GetUint8(stream);
        }
        public void Save(Stream stream)
        {
            //if (stream.Position > 6692)
            //    throw new Exception("sdf");
            PacketUtil.WriteUInt16R(stream, A00);

            for (int i = 0; i < 8; i++)
            {
                if (A02_Data.ContainsKey(i))
                {
                    PacketUtil.WriteUInt32R(stream, 0XAAAAAAAA);
                    A02_Data[i].Save(stream);
                }
                else
                    PacketUtil.WriteUInt32R(stream, 0);
            }

            for (int i = 0; i < 8; i++)
                PacketUtil.WriteUInt32R(stream, (uint)Values[i]);

            for (int i = 0; i < 8; i++)
                PacketUtil.WriteUInt32R(stream, (uint)Multipliers[i]);

            PacketUtil.WriteUInt32R(stream, ActivationDelay);
            PacketUtil.WriteUInt32R(stream, A06_Duration);
            PacketUtil.WriteUInt32R(stream, Flags);
            PacketUtil.WriteUInt32R(stream, A08);
            PacketUtil.WriteUInt32R(stream, (uint)A09_Operation);
            PacketUtil.WriteUInt32R(stream, A10_Interval);

            PacketUtil.WriteUInt16R(stream, A11_ComponentID);
            PacketUtil.WriteUInt16R(stream, A12_Radius);
            PacketUtil.WriteUInt16R(stream, ConeAngle);
            PacketUtil.WriteUInt16R(stream, FlightSpeed);
            PacketUtil.WriteUInt16R(stream, A15);

            PacketUtil.WriteByte(stream, MaxTargets);
        }


    }

    public class AbilityComponentExport
    {
        public uint Header;
        public uint Size;
        public List<AbilityComponent> Components = new List<AbilityComponent>();
        public Dictionary<ushort, AbilityComponent> CompHash = new Dictionary<ushort, AbilityComponent>();
        public byte[] Original = null;
   

        public AbilityComponent NewComponent(MYPManager manager, MythicPackage package, AbilityBin ability, AbilityComponent cloneFrom = null)
        {

            if (ability.Components.Count < 10)
            {
                var max = 1;
                if (Components.Count > 0)
                    max = Components.Select(e => e.A11_ComponentID).Max() + 1;

                var component = new AbilityComponent((ushort)(max), cloneFrom);
                component.Index = (byte)Components.Count;
                Components.Add(component);
                ability.Components.Add(ability.Components.Count, component);
                if (cloneFrom != null)
                {
                    foreach (Language language in Enum.GetValues(typeof(Language)))
                    {
                        string path = "data/strings/" + language + "/componenteffects.txt";
                        var str = manager.GetString(language, path, cloneFrom.A11_ComponentID, package);

                        if (str != null && str.Length > 0)
                        {
                            manager.SetString(language, path, component.A11_ComponentID, str, package);
                            var doc = manager.GetString(language, path);
                            manager.UpdateAsset(package, path, System.Text.UnicodeEncoding.Unicode.GetPreamble().Concat(System.Text.UnicodeEncoding.Unicode.GetBytes(doc)).ToArray());
                        }
                    }
                    component.Description = manager.GetString(Language.english, "data/strings/english/componenteffects.txt", component.A11_ComponentID, package);
                }
                for (int i = 0; i < 10; i++)
                {
                    if (ability.ComponentIDs[i] == 0)
                    {
                        ability.ComponentIDs[i] = component.A11_ComponentID;
                        break;
                    }
                }
                CompHash[component.A11_ComponentID] = component;
                return component;
            }
            return null;
        }

        public void Load(MYPManager manager, Stream stream)
        {
            Original = new byte[stream.Length];
            stream.Read(Original, 0, (int)stream.Length);
            stream.Position = 0;
            Header = PacketUtil.GetUint32R(stream);
            Size = PacketUtil.GetUint32R(stream);

           for(int i=0; i<Size; i++)
            {
                if (stream.Position >= stream.Length)
                {
                    break;
                }
                var a = new AbilityComponent(stream);

                Components.Add(a);
                a.Description = manager.GetString(Language.english, "componenteffects.txt", a.A11_ComponentID);

                if (a.A09_Operation == ComponentOperationType.BONUS_TYPE_ADJUST)
                    a.BonusType = ((BonusType)a.Values[0]);
                else if (a.A09_Operation == ComponentOperationType.STAT_CHANGE)
                    a.BonusType = ((BonusType)a.Flags);
                else
                    a.BonusType = BonusType.NONE;

                if (a.A09_Operation == ComponentOperationType.APPLY_ABILITY)
                    a.LinkedAbility = manager.GetString(Language.english, "abilitynames.txt", (ushort)a.Values[0]).Replace("^n", "") + " (" + a.Values[0] + ")";

                //foreach 
            }

            if (stream.Position < stream.Length)
                throw new Exception($"Unexpected {nameof(AbilityComponentExport)} size");
            foreach (var comp in Components)
            {
                if (CompHash.ContainsKey(comp.A11_ComponentID))
                    Console.WriteLine("Already contains component " + comp.A11_ComponentID);
                CompHash[comp.A11_ComponentID] = comp;
            }
        }

        public void Save(Stream stream)
        {
            PacketUtil.WriteUInt32R(stream, 0x0C);
            PacketUtil.WriteUInt32R(stream, (uint)Components.Count);


            for (int i = 0; i < Components.Count; i++)
            {
                var ability = Components[i];
                if (i == Components.Count - 1)
                {
                    Console.Write("dsf");
                }
                ability.Save(stream);
            }
        }



    }

    public class AbilityRequirmentBin
    {
        public ushort ID { get; set; }
        public Dictionary<int, ExtData> A02Data = new Dictionary<int, ExtData>();
        public List<ExtData> A02 { get { return A02Data.Values.ToList(); } }

        public ExtData D1
        {
            get
            {
                if (A02.Count > 0)
                    return A02[0];
                return new ExtData();
            }
        }

        public ExtData D2
        {
            get
            {
                if (A02.Count > 1)
                    return A02[1];
                return new ExtData();
            }
        }
        public ExtData D3
        {
            get
            {
                if (A02.Count > 2)
                    return A02[2];
                return new ExtData();
            }
        }
        public ExtData D4
        {
            get
            {
                if (A02.Count > 3)
                    return A02[3];
                return new ExtData();
            }
        }
        public ExtData D5
        {
            get
            {
                if (A02.Count > 4)
                    return A02[4];
                return new ExtData();
            }
        }
        public ExtData D6
        {
            get
            {
                if (A02.Count > 5)
                    return A02[5];
                return new ExtData();
            }
        }
        public ExtData D7
        {
            get
            {
                if (A02.Count > 6)
                    return A02[6];
                return new ExtData();
            }
        }
        public ExtData D8
        {
            get
            {
                if (A02.Count > 7)
                    return A02[7];
                return new ExtData();
            }
        }
        public ExtData D9
        {
            get
            {
                if (A02.Count > 8)
                    return A02[8];
                return new ExtData();
            }
        }

        public AbilityRequirmentBin(Stream stream)
        {
            ID = PacketUtil.GetUint16R(stream);
            for (int i = 0; i < 6; i++)
            {
                var d = PacketUtil.GetUint32R(stream);

                if (d == 0XAAAAAAAA)
                {
                    var data = new ExtData()
                    {
                        Type = (AbilitySourceType)PacketUtil.GetUint32R(stream),
                        Operation = (AbilityOperation)PacketUtil.GetUint32R(stream),
                        Condition = (AbilityCondition)PacketUtil.GetUint32R(stream),
                        LogicOperator = (AbilityLogicOperator)PacketUtil.GetUint32R(stream),
                        Val5 = (int)PacketUtil.GetUint32R(stream),
                        Val6 = (int)PacketUtil.GetUint32R(stream),
                        Val7 = (int)PacketUtil.GetUint32R(stream),
                        Val8 = (int)PacketUtil.GetUint32R(stream),
                        Val9 = PacketUtil.GetUint8(stream),
                    };
                    A02Data[i] = data;

                }
            }
        }

        public void Save(Stream stream)
        {
            PacketUtil.WriteUInt16R(stream, ID);
            for (int i = 0; i < 6; i++)
            {

                if (A02Data.ContainsKey(i))
                {
                    PacketUtil.WriteUInt32R(stream, 0XAAAAAAAA);
                    A02Data[i].Save(stream);
                }
                else
                    PacketUtil.WriteUInt32R(stream, 0);
            }

        }
    }

    public class AbilityRequirmentExport
    {

        public uint Header;
        public uint Size;
        public List<AbilityRequirmentBin> Requirments = new List<AbilityRequirmentBin>();
        public void Load(Stream stream)
        {
            Header = PacketUtil.GetUint32R(stream);
            Size = PacketUtil.GetUint32R(stream);

            while (stream.Position < stream.Length)
            {

                var a = new AbilityRequirmentBin(stream);
                Requirments.Add(a);
            }
            if (stream.Position < stream.Length)
                throw new Exception($"Unexpected {nameof(AbilityRequirmentExport)} size");
        }

        public void Save(Stream stream)
        {
            PacketUtil.WriteUInt32R(stream, 0x01);
            PacketUtil.WriteUInt32R(stream, (uint)Requirments.Count);


            for (int i = 0; i < Requirments.Count; i++)
            {
                var ability = Requirments[i];
                ability.Save(stream);
            }

        }
    }

    public class UpgradeTable
    {
        public ushort ID { get; set; }
        public List<UpgradeItem> Items = new List<UpgradeItem>();

        public override string ToString()
        {
            return ID.ToString();
        }
        public class UpgradeItem
        {
            public ushort A00A { get; set; }
            public ushort A00B { get; set; }

            public ushort A01A { get; set; }
            public ushort A01B { get; set; }

            public ushort A02A { get; set; }
            public ushort A02B { get; set; }

            public ushort A03A { get; set; }
            public ushort A03B { get; set; }

            //public byte A04{ get; set; }
            //public byte A05{ get; set; }
            //public byte A06{ get; set; }

            public override string ToString()
            {
                return $"{A00A.ToString("X2")} {A00B.ToString("X2")} {A01A.ToString("X2")} {A01B.ToString("X2")} {A02A.ToString("X2")} {A02B.ToString("X2")} {A03A.ToString("X2")} {A03B.ToString("X2")}";
            }

            public UpgradeItem(Stream stream)
            {
                A00A = (ushort)PacketUtil.GetUint16R(stream);
                A00B = (ushort)PacketUtil.GetUint16R(stream);

                A01A = (ushort)PacketUtil.GetUint16R(stream);
                A01B = (ushort)PacketUtil.GetUint16R(stream);

                A02A = (ushort)PacketUtil.GetUint16R(stream);
                A02B = (ushort)PacketUtil.GetUint16R(stream);

                A03A = (ushort)PacketUtil.GetUint16R(stream);
                A03B = (ushort)PacketUtil.GetUint16R(stream);
                //A03 = PacketUtil.GetUint8(stream);
                //A04 = PacketUtil.GetUint8(stream);
                //A05 = PacketUtil.GetUint8(stream);
                //A06 = PacketUtil.GetUint8(stream);

            }
            public void Save(Stream stream)
            {
                //PacketUtil.WriteUInt32R(stream, (uint)A00);
                //PacketUtil.WriteUInt32R(stream, (uint)A01);
                //PacketUtil.WriteUInt32R(stream, (uint)A02);

                //PacketUtil.WriteUInt32R(stream, (uint)A03);
                //PacketUtil.WriteByte(stream, A04);
                //PacketUtil.WriteByte(stream, A05);
                //PacketUtil.WriteByte(stream, A06);
            }
        }

        public UpgradeItem I1
        {
            get
            {
                return Items[0];
            }
        }
        public UpgradeItem I2
        {
            get
            {
                return Items[1];
            }
        }
        public UpgradeItem I3
        {
            get
            {
                return Items[2];
            }
        }
        public UpgradeItem I4
        {
            get
            {
                return Items[3];
            }
        }
        public UpgradeTable(Stream stream)
        {
            ID = PacketUtil.GetUint16R(stream);
            for (int i = 0; i < 0x14; i++)
            {
                var item = new UpgradeItem(stream);
                Items.Add(item);
            }

        }
        public void Save(Stream stream)
        {
            PacketUtil.WriteUInt16R(stream, ID);
            for (int i = 0; i < 0x14; i++)
            {
                Items[i].Save(stream);
            }
        }
    }

    public class UpgradeTableExport
    {
        public uint Header;
        public uint Size;
        public List<UpgradeTable> Upgrades = new List<UpgradeTable>();
        public void Load(Stream stream)
        {
            Header = PacketUtil.GetUint32R(stream);
            Size = PacketUtil.GetUint32R(stream);

            while (stream.Position < stream.Length)
            {

                var a = new UpgradeTable(stream);
                Upgrades.Add(a);
            }

            if (stream.Position < stream.Length)
                throw new Exception($"Unexpected {nameof(UpgradeTableExport)} size");
        }

        public void Save(Stream stream)
        {
            PacketUtil.WriteUInt32R(stream, 0x01);
            PacketUtil.WriteUInt32R(stream, (uint)Upgrades.Count);


            for (int i = 0; i < Upgrades.Count; i++)
            {
                var ability = Upgrades[i];
                ability.Save(stream);
            }
        }
    }

    public class PacketUtil
    {
        public static uint ConvertToUInt32(byte v1, byte v2, byte v3, byte v4)
        {
            return (uint)((v1 << 24) | (v2 << 16) | (v3 << 8) | v4);
        }

        public static int IndexOfSequence(byte[] buff, int offset, params byte[] sequence)
        {
            int index = -1;

            for (int i = offset; i < buff.Length - sequence.Length; i++)
            {
                bool allMatch = true;
                index = i;
                for (int c = 0; c < sequence.Length; c++)
                {

                    if (buff[i + c] == sequence[c])
                    {
                        allMatch = true;
                    }
                    else
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public static uint GetUint32(Stream stream)
        {
            byte v1 = (byte)stream.ReadByte();
            byte v2 = (byte)stream.ReadByte();
            byte v3 = (byte)stream.ReadByte();
            byte v4 = (byte)stream.ReadByte();

            return ConvertToUInt32(v1, v2, v3, v4);
        }

        public static float GetFloat(Stream stream)
        {
            byte[] data = new byte[4];
            stream.Read(data, 0, 4);
            return BitConverter.ToSingle(data, 0);
        }


        public static UInt64 GetUint64(Stream stream)
        {
            UInt64 value = (GetUint32(stream) << 24) + (GetUint32(stream));
            return value;
        }

        public static byte GetUint8(Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        public static ushort GetUint16R(Stream stream)
        {
            byte v1 = GetUint8(stream);
            byte v2 = GetUint8(stream);

            return ConvertToUInt16(v2, v1);
        }

        public static short GetInt16R(Stream stream)
        {
            byte v1 = GetUint8(stream);
            byte v2 = GetUint8(stream);

            return ConvertToInt16(v1, v2);
        }
        public static short ConvertToInt16(byte v1, byte v2)
        {
            return (short)(v1 | (v2 << 8));
        }

        public static void WriteUInt16R(Stream stream, ushort val)
        {
            WriteByte(stream, (byte)(val & 0xff));
            WriteByte(stream, (byte)(val >> 8));
        }
        public static uint GetUint32R(Stream stream)
        {
            byte v1 = GetUint8(stream);
            byte v2 = GetUint8(stream);
            byte v3 = GetUint8(stream);
            byte v4 = GetUint8(stream);
            return ConvertToUInt32(v4, v3, v2, v1);
        }

        private static string ConvertToString(byte[] cstyle)
        {
            if (cstyle == null)
                return null;

            for (int i = 0; i < cstyle.Length; i++)
            {
                if (cstyle[i] == 0)
                    return Encoding.GetEncoding("iso-8859-1").GetString(cstyle, 0, i);
            }
            return Encoding.GetEncoding("iso-8859-1").GetString(cstyle);
        }


        public static string GetString(Stream stream)
        {
            int length = (int)GetUint32(stream);
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return ConvertToString(buffer);
        }


        public static string GetPascalString(Stream stream)
        {
            int length = (int)ReadVarUInt(stream);
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return ConvertToString(buffer);
        }
        public static string GetCString(Stream stream)
        {
            string result = "";
            byte c;
            while (stream.Position < stream.Length)
            {
                c = (byte)stream.ReadByte();
                if (c != 0)
                    result += (char)c;
                else
                    break;

            }
            return result;
        }
        public static void WritePascalString(Stream stream, string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(stream, 0);
                return;
            }

            byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(str);
            WriteByte(stream, (byte)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WritePascalString32(Stream stream, string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteUInt32R(stream, 0);
                return;
            }

            byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(str);
            WriteUInt32R(stream, (uint)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteCString(Stream stream, string str)
        {
            if (str == null || str.Length <= 0)
            {
                WriteByte(stream, 0);
                return;
            }

            byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
            WriteByte(stream, 0);
        }

        public static string GetString(Stream stream, int maxlen)
        {
            var buf = new byte[maxlen];
            stream.Read(buf, 0, maxlen);

            return ConvertToString(buf);
        }

        public static byte[] GetByteArray(Stream stream, int size)
        {
            byte[] buf = new byte[size];
            stream.Read(buf, 0, size);

            return buf;
        }

        public static int DecodeMythicSize(MemoryStream stream)
        {
            int mSize = 0;
            int mByteCount = 0;
            int mByte = 0;

            mByte = stream.ReadByte();
            while ((mByte & 0x80) == 0x80)
            {
                //Log.Debug("readSize", "mByte = " + mByte);
                mByte ^= 0x80;
                mSize = (mSize | (mByte << (7 * mByteCount)));

                if (stream.Length == stream.Capacity)
                    return 0;


                mByte = stream.ReadByte();
                mByteCount++;
            }

            mSize = (mSize | (mByte << (7 * mByteCount)));
            return mSize;
        }
        public static ushort ConvertToUInt16(byte v1, byte v2)
        {
            return (ushort)(v2 | (v1 << 8));
        }

        public static UInt16 GetUint16(Stream stream)
        {
            return ConvertToUInt16(GetUint8(stream), GetUint8(stream));
        }

        public static void WriteHexStringBytes(Stream stream, string hexString)
        {
            int length = hexString.Length / 2;

            if ((hexString.Length % 2) == 0)
            {
                for (int i = 0; i < length; i++)
                    WriteByte(stream, Convert.ToByte(hexString.Substring(i * 2, 2), 16));
            }
            else
            {
                WriteByte(stream, 0);
            }
        }

        public static void WritePacketString(Stream stream, string packet)
        {
            packet = packet.Replace(" ", string.Empty);

            using (StringReader Reader = new StringReader(packet))
            {
                string Line;
                while ((Line = Reader.ReadLine()) != null)
                {
                    WriteHexStringBytes(stream, Line.Substring(1, Line.IndexOf("|", 2) - 1));
                }
            }
        }


        public static string Bin(byte[] dump, int start, int len)
        {
            string result = "";
            for (int i = start; i < dump.Length; i++)
            {
                string b = Convert.ToString(dump[i], 2).PadLeft(8, '0');
                result += b;
            }
            return result;

        }
        public static string Hex(byte[] dump, int start, int len, int? current = null)
        {
            var hexDump = new StringBuilder();

            hexDump.AppendLine("|------------------------------------------------|----------------|");
            hexDump.AppendLine("|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|");
            hexDump.AppendLine("|------------------------------------------------|----------------|");
            try
            {
                int end = start + len;
                for (int i = start; i < end; i += 16)
                {
                    StringBuilder text = new StringBuilder();
                    StringBuilder hex = new StringBuilder();


                    for (int j = 0; j < 16; j++)
                    {
                        if (j + i < end)
                        {
                            byte val = dump[j + i];

                            if (current.HasValue && current.Value == j + i)
                                hex.Append("[" + dump[j + i].ToString("X2") + "]");
                            else
                                hex.Append(dump[j + i].ToString("X2"));

                            hex.Append(" ");
                            if (val >= 32 && val < 127)
                            {
                                if (current.HasValue && current.Value == j + i)
                                    text.Append("[" + (char)val + "]");
                                else
                                    text.Append((char)val);
                            }
                            else
                            {
                                if (current.HasValue && current.Value == j + i)
                                    text.Append("[.]");
                                else
                                    text.Append(".");
                            }
                        }
                    }

                    hexDump.AppendLine("|" + hex.ToString().PadRight(48) + "|" + text.ToString().PadRight(16) + "|");
                }
            }
            catch
            {
                // Log.Error("HexDump", e.ToString());
            }

            hexDump.Append("-------------------------------------------------------------------");
            return hexDump.ToString();
        }

        public static void Skip(Stream stream, long num)
        {
            stream.Seek(num, SeekOrigin.Current);
        }

        public static void WriteBool(Stream stream, bool data)
        {
            stream.WriteByte(data ? (byte)1 : (byte)0);
        }


        public static void WriteInt16R(Stream stream, short val)
        {
            WriteByte(stream, (byte)(val & 0xff));
            WriteByte(stream, (byte)(val >> 8));
        }
        public static void WriteByte(Stream stream, byte data)
        {
            stream.WriteByte(data);
        }

        public static void WriteParam(Stream stream, byte paramIndex, int paramValue)
        {
            PacketUtil.WriteByte(stream, paramIndex); //param 
            foreach (byte packed in PacketUtil.Pack(new List<int>() { paramValue }))
            {
                PacketUtil.WriteByte(stream, packed);
            }

        }

        public static void WriteBytes(Stream stream, byte[] data)
        {
            if (data != null)
                stream.Write(data, 0, data.Length);
        }
        public static void WriteString(Stream stream, string str)
        {
            WriteUInt32(stream, (UInt32)str.Length);
            WriteStringBytes(stream, str);
        }

        public static int GetTimeStamp()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static void WriteString(Stream stream, string str, int maxlen)
        {
            if (str.Length <= 0)
                return;

            byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(str);
            stream.Write(bytes, 0, bytes.Length < maxlen ? bytes.Length : maxlen);
        }

        public static void WriteStringBytes(Stream stream, string str)
        {
            if (str.Length <= 0)
                return;

            byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteUInt32(Stream stream, uint val)
        {
            WriteByte(stream, (byte)(val >> 24));
            WriteByte(stream, (byte)((val >> 16) & 0xff));
            WriteByte(stream, (byte)((val & 0xffff) >> 8));
            WriteByte(stream, (byte)((val & 0xffff) & 0xff));
        }

        public static void WriteUInt64(Stream stream, long val)
        {
            WriteByte(stream, (byte)(val >> 56));
            WriteByte(stream, (byte)((val >> 48) & 0xff));
            WriteByte(stream, (byte)((val >> 40) & 0xff));
            WriteByte(stream, (byte)((val >> 32) & 0xff));
            WriteByte(stream, (byte)((val >> 24) & 0xff));
            WriteByte(stream, (byte)((val >> 16) & 0xff));
            WriteByte(stream, (byte)((val >> 8) & 0xff));
            WriteByte(stream, (byte)(val & 0xff));
        }

        public static void WriteUInt32R(Stream stream, uint val)
        {
            WriteByte(stream, (byte)((val & 0xffff) & 0xff));
            WriteByte(stream, (byte)((val & 0xffff) >> 8));
            WriteByte(stream, (byte)((val >> 16) & 0xff));
            WriteByte(stream, (byte)(val >> 24));
        }

        public static void WriteUInt16(Stream stream, ushort val)
        {
            WriteByte(stream, (byte)(val >> 8));
            WriteByte(stream, (byte)(val & 0xff));
        }

        public static void Fill(Stream stream, byte val, int num)
        {
            for (int i = 0; i < num; ++i)
                WriteByte(stream, val);
        }


        public static List<int> Unpack(Stream stream, int count)
        {
            if (stream.Position == stream.Length)
                return new List<int>() { 0 };

            List<int> values = new List<int>();

            int index = 0;
            int curVal = 0;
            int intPos = 0;
            bool signed = false;
            bool readSign = true;
            while (values.Count < count && stream.Position < stream.Length)
            {
                byte curByte = GetUint8(stream);


                bool more = (curByte & 0x80) >> 7 == 1;
                int curByteVal = 0;
                if (readSign)
                {
                    curByteVal = ((curByte & 0x7F) >> 1);

                }
                else
                {
                    curByteVal = ((curByte & 0x7F));
                }

                int d = curByteVal << intPos;

                if (readSign)
                {
                    intPos += 6;
                    signed = (curByte & 0x01) == 1;
                    readSign = false;
                }
                else
                    intPos += 7;

                curVal = d | curVal;

                if (!more)
                {
                    if (signed)
                    {
                        if (curVal == 0)
                            curVal = -1;
                        else
                            curVal += 1;
                    }
                    else if (curVal > 1)
                        curVal = -curVal;
                    values.Add(curVal);

                    curVal = 0;
                    signed = false;
                    intPos = 0;
                    readSign = true;
                }

                index++;
            }
            if (values.Count == 0)
                return new List<int>() { 0 };
            return values;
        }

        public static string GetPascalString32(Stream stream)
        {
            int length = (int)GetUint32R(stream);
            if (length > 0 && length < 100)
            {
                byte[] buffer = new byte[length];
                stream.Read(buffer, 0, length);
                return ConvertToString(buffer);

            }
            return "";
        }

        public static int ReadVarInt(Stream stream)
        {
            var value = ReadVarUInt(stream);
            if ((value & 0x01) != 0)
                return (int)~(value >> 1);
            else
                return (int)(value >> 1);
        }

        public static uint ReadVarUInt(Stream stream)
        {
            int result = 0;
            for (int i = 0; i < 5 && stream.Position < stream.Length; i++)
            {
                var b = stream.ReadByte();
                result |= (b & 0x7F) << (i * 7);
                if ((b & 0x80) == 0)
                    break;
            }
            return (uint)result;
        }

        public static void WriteVarInt32(MemoryStream stream, int value)
        {


            if (value < 0)
            {
                value = (-value << 1) | 0x1;
            }
            else
                value = value << 1;

            WriteVarUInt32(stream, (uint)value);
        }

        public static void WriteVarUInt32(MemoryStream stream, uint val)
        {
            if (val == 0)
            {
                stream.WriteByte(0);
                return;
            }

            while (val > 0)
            {
                stream.WriteByte((byte)((val & 0x7F) ^ ((val > 0x7F) ? 0x80 : 0x00)));
                val = val >> 7;
            }
        }

        public static byte[] ToVarUInt32(uint val)
        {


            if (val == 0)
            {
                return new byte[0];
            }
            int size = 0;
            int pos = 0;
            while (val > 0)
            {
                val = val >> 7;
                size++;
            }
            var result = new byte[size];
            while (val > 0)
            {
                result[pos] = ((byte)((val & 0x7F) ^ ((val > 0x7F) ? 0x80 : 0x00)));
                val = val >> 7;
                pos++;
            }
            return result;
        }



        public static void WriteZigZag(MemoryStream stream, int val)
        {
            byte sign = (byte)((val < 0) ? 1 : 0);
            if (sign == 1)
                val++;
            val = Math.Abs(val);
            stream.WriteByte((byte)(((val << 1) & 0x7F) ^ ((val > 0x3F) ? 0x80 : 0x00) + sign));
            val = val >> 6;

            while (val > 0)
            {
                stream.WriteByte((byte)((val & 0x7F) ^ ((val > 0x7F) ? 0x80 : 0x00)));
                val = val >> 7;
            }
        }

        public static List<uint> UnpackUnsigned(Stream stream, int count)
        {
            if (stream.Position == stream.Length)
                return new List<uint>() { 0 };

            List<uint> values = new List<uint>();

            uint index = 0;
            uint curVal = 0;
            uint intPos = 0;
            bool signed = false;
            bool readSign = true;
            while (values.Count < count && stream.Position < stream.Length)
            {
                byte curByte = GetUint8(stream);


                bool more = (curByte & 0x80) >> 7 == 1;
                uint curByteVal = 0;
                if (readSign)
                {
                    curByteVal = (uint)((curByte & 0x7F) >> 1);

                }
                else
                {
                    curByteVal = (uint)((curByte & 0x7F));
                }

                uint d = (uint)((int)curByteVal << (int)intPos);

                if (readSign)
                {
                    intPos += 6;
                    signed = (curByte & 0x01) == 1;
                    readSign = false;
                }
                else
                    intPos += 7;

                curVal = d | curVal;

                if (!more)
                {
                    if (signed)
                    {
                        if (curVal == 0)
                            curVal = 0;
                        else
                            curVal += 1;
                    }

                    values.Add(curVal);

                    curVal = 0;
                    signed = false;
                    intPos = 0;
                    readSign = true;
                }

                index++;
            }

            return values;
        }

        public static List<byte> Pack(List<int> values)
        {
            List<byte> bytes = new List<byte>();
            foreach (int val in values)
            {
                if (val == 0)
                {
                    bytes.Add(0);
                    continue;
                }
                else if (val == 1)
                {
                    bytes.Add(2);
                    continue;
                }
                else if (val == -1)
                {
                    bytes.Add(1);
                    continue;
                }

                int signedValue = val;
                if (val != 1)
                {
                    if (val > 0)
                        signedValue = signedValue - 1;
                    else
                        signedValue = -signedValue;
                }

                int bitCount = BitCount(signedValue);
                int bitCountRemaning = bitCount;
                int octetCount = 0;
                int c = 0;
                while (bitCountRemaning > 0)
                {
                    if (c == 0)
                        bitCountRemaning -= 6;
                    else
                        bitCountRemaning -= 7;
                    octetCount++;
                }

                int offset = 0;
                int shift = 6;
                for (int i = 0; i < octetCount; i++)
                {
                    int mask = CreateMask(offset, offset + shift);
                    int octet = (mask & signedValue) >> offset;

                    if (i == 0)
                    {
                        octet = octet << 1; //LSB is the sign bit on first octet

                        if (val > 0)//if value was position, we need to set sign flag
                            octet |= 1 << 0; //1 for position
                        else
                            octet &= ~(1 << 0); //0 for negative
                    }

                    if (i != (octetCount - 1)) //if not on last octet, set expansion bit
                        octet |= 0x80;

                    bytes.Add((byte)octet);
                    offset += shift;

                    if (i == 0) //first octet we write 6 bits, then 7 bits on rest
                        shift = 7;
                }
            }
            return bytes;
        }

        private static int BitCount(int value)
        {
            int bitCount = 0;
            int r = value;
            while (r > 0)
            {
                r /= 2;
                bitCount++;
            }
            return bitCount;
        }

        private static int CreateMask(int a, int b)
        {
            int r = 0;
            for (int i = a; i <= b; i++)
                r |= 1 << i;

            return r;
        }


        public static void FillString(Stream stream, string str, int len)
        {
            long pos = stream.Position;

            Fill(stream, 0x0, len);

            if (str == null)
                return;

            stream.Position = pos;

            if (str.Length <= 0)
            {
                stream.Position = pos + len;
                return;
            }

            byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(str);
            stream.Write(bytes, 0, len > bytes.Length ? bytes.Length : len);
            stream.Position = pos + len;
        }
    }
}
*/