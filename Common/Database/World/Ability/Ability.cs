using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Ability : DataObject
    {
        [PrimaryKey]
        public ushort ID { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public string Description { get; set; }

        [DataElement]
        public ushort Castime { get; set; }

        [DataElement]
        public ushort Cooldown { get; set; }

        [DataElement]
        public ushort TacticType { get; set; }

        [DataElement]
        public ushort AbilityType { get; set; }

        [DataElement]
        public ushort A20 { get; set; }

        [DataElement]
        public ushort A24 { get; set; }

        [DataElement]
        public ushort CareerLine { get; set; }

        [DataElement]
        public ushort A32 { get; set; }

        [DataElement]
        public ushort Flags { get; set; }

        [DataElement]
        public ushort EffectID { get; set; }

        [DataElement]
        public ushort A44 { get; set; }

        [DataElement]
        public ushort Range { get; set; }

        [DataElement]
        public ushort Angle { get; set; }

        [DataElement]
        public ushort MoraleCost { get; set; }

        [DataElement]
        public ushort ChannelInterval { get; set; }

        [DataElement]
        public ushort A54 { get; set; }

        [DataElement]
        public ushort ScaleStatMult { get; set; }

        [DataElement]
        public ushort NumTacticSlots { get; set; }

        [DataElement]
        public ushort AP { get; set; }

        [DataElement]
        public ushort A61 { get; set; }

        [DataElement]
        public ushort A62 { get; set; }

        [DataElement]
        public ushort A63 { get; set; }

        [DataElement]
        public ushort AbilityImprovementThreshold { get; set; }

        [DataElement]
        public ushort Specialization { get; set; }

        [DataElement]
        public ushort StanceOrder { get; set; }

        [DataElement]
        public ushort A68 { get; set; }

        [DataElement]
        public ushort MinLevel { get; set; }

        [DataElement]
        public ushort A70 { get; set; }

        [DataElement]
        public ushort A71 { get; set; }

        [DataElement]
        public ushort A132 { get; set; }

        [DataElement]
        public ushort A136 { get; set; }

        [DataElement]
        public ushort A140 { get; set; }

        [DataElement]
        public ushort A142C { get; set; }

        [DataElement]
        public ushort Disabled { get; set; }

        [DataElement]
        public ushort CanClickOff { get; set; }

        [DataElement]
        public ushort ScaleStat { get; set; }

        [DataElement]
        public ushort CantCrit { get; set; }

        [DataElement]
        public ushort MoraleLevel { get; set; }

        [DataElement]
        public ushort AttackType { get; set; }

        [DataElement]
        public ushort CounterAmount { get; set; }

        [DataElement]
        public ushort RangeMin { get; set; }

        [DataElement]
        public ushort EnemyTargetIgnoreLOS { get; set; }

        [DataElement]
        public ushort FriendlyTargetIgnoreLOS { get; set; }

        [DataElement]
        public ushort RangeMax { get; set; }

        [DataElement]
        public ushort UniqueGroup { get; set; }

        [DataElement]
        public ushort Faction { get; set; }

        [DataElement]
        public ushort UsableWithBuff { get; set; }

        [DataElement]
        public ushort AnimationDelay { get; set; }

        [DataElement]
        public ushort BinIndex { get; set; }

        [DataElement]
        public ushort SpellDamageType { get; set; }

        [DataElement]
        public ushort Toggle { get; set; }

        [DataElement]
        public ushort ToggleGroup { get; set; }

        [DataElement]
        public ushort PatcherFileID { get; set; }

        [DataElement]
        public ushort CreateBinData { get; set; }
    }
}