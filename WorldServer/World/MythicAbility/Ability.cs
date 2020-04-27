using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FrameWork;

namespace Common.Database.World.MythicAbility.Ability
{
    [DataTable(PreCache = false, TableName = "ability", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]

    public class Ability : DataObject
    {
        [PrimaryKey]
        public ushort ID { get; set; }

        [PrimaryKey]
        public string Name { get; set; }

        [PrimaryKey]
        public string Description { get; set; }

        [PrimaryKey]
        public ushort Castime { get; set; }

        [PrimaryKey]
        public ushort Cooldown { get; set; }

        [PrimaryKey]
        public ushort TacticType { get; set; }

        [PrimaryKey]
        public ushort AbilityType { get; set; }

        [PrimaryKey]
        public ushort A20 { get; set; }

        [PrimaryKey]
        public ushort A24 { get; set; }

        [PrimaryKey]
        public ushort CareerLine { get; set; }

        [PrimaryKey]
        public ushort A32 { get; set; }

        [PrimaryKey]
        public ushort Flags { get; set; }

        [PrimaryKey]
        public ushort EffectID { get; set; }

        [PrimaryKey]
        public ushort A44 { get; set; }

        [PrimaryKey]
        public ushort Range { get; set; }

        [PrimaryKey]
        public ushort Angle { get; set; }

        [PrimaryKey]
        public ushort MoraleCost { get; set; }

        [PrimaryKey]
        public ushort ChannelInterval { get; set; }

        [PrimaryKey]
        public ushort A54 { get; set; }

        [PrimaryKey]
        public ushort ScaleStatMult { get; set; }

        [PrimaryKey]
        public ushort NumTacticSlots { get; set; }

        [PrimaryKey]
        public ushort AP { get; set; }

        [PrimaryKey]
        public ushort A61 { get; set; }

        [PrimaryKey]
        public ushort A62 { get; set; }

        [PrimaryKey]
        public ushort A63 { get; set; }
        [PrimaryKey]

        public ushort AbilityImprovementThreshold { get; set; }

        [PrimaryKey]
        public ushort Specialization { get; set; }

        [PrimaryKey]
        public ushort StanceOrder { get; set; }

        [PrimaryKey]
        public ushort A68 { get; set; }

        [PrimaryKey]
        public ushort MinLevel { get; set; }

        [PrimaryKey]
        public ushort A70 { get; set; }

        [PrimaryKey]
        public ushort A71 { get; set; }

        [PrimaryKey]
        public ushort A132 { get; set; }

        [PrimaryKey]
        public ushort A136 { get; set; }

        [PrimaryKey]
        public ushort A140 { get; set; }

        [PrimaryKey]
        public ushort A142C { get; set; }

        [PrimaryKey]
        public ushort Disabled { get; set; }

        [PrimaryKey]
        public ushort CanClickOff { get; set; }

        [PrimaryKey]
        public ushort ScaleStat { get; set; }

        [PrimaryKey]
        public ushort CantCrit { get; set; }

        [PrimaryKey]
        public ushort MoraleLevel { get; set; }

        [PrimaryKey]
        public ushort AttackType { get; set; }

        [PrimaryKey]
        public ushort CounterAmount { get; set; }

        [PrimaryKey]
        public ushort RangeMin { get; set; }

        [PrimaryKey]
        public ushort EnemyTargetIgnoreLOS { get; set; }

        [PrimaryKey]
        public ushort FriendlyTargetIgnoreLOS { get; set; }

        [PrimaryKey]
        public ushort RangeMax { get; set; }

        [PrimaryKey]
        public ushort UniqueGroup { get; set; }

        [PrimaryKey]
        public ushort Faction { get; set; }

        [PrimaryKey]
        public ushort UsableWithBuff { get; set; }

        [PrimaryKey]
        public ushort AnimationDelay { get; set; }

        [PrimaryKey]
        public ushort BinIndex { get; set; }

        [PrimaryKey]
        public ushort SpellDamageType { get; set; }

        [PrimaryKey]
        public ushort Toggle { get; set; }

        [PrimaryKey]
        public ushort ToggleGroup { get; set; }

        [PrimaryKey]
        public ushort PatcherFileID { get; set; }

        [PrimaryKey]
        public ushort CreateBinData { get; set; }    


    }
}