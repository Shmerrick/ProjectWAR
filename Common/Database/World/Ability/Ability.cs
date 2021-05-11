using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Ability : DataObject
    {
        [PrimaryKey]
        public long ID { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public string Description { get; set; }

        [DataElement]
        public long Castime { get; set; }

        [DataElement]
        public long Cooldown { get; set; }

        [DataElement]
        public long TacticType { get; set; }

        [DataElement]
        public long AbilityType { get; set; }

        [DataElement]
        public long A20 { get; set; }

        [DataElement]
        public long A24 { get; set; }

        [DataElement]
        public long CareerLine { get; set; }

        [DataElement]
        public long A32 { get; set; }

        [DataElement]
        public long Flags { get; set; }

        [DataElement]
        public int EffectID { get; set; }

        [DataElement]
        public int A44 { get; set; }

        [DataElement]
        public int Range { get; set; }

        [DataElement]
        public int Angle { get; set; }

        [DataElement]
        public int MoraleCost { get; set; }

        [DataElement]
        public int ChannelInterval { get; set; }

        [DataElement]
        public int A54 { get; set; }

        [DataElement]
        public int ScaleStatMult { get; set; }

        [DataElement]
        public byte NumTacticSlots { get; set; }

        [DataElement]
        public byte AP { get; set; }

        [DataElement]
        public byte A61 { get; set; }

        [DataElement]
        public byte A62 { get; set; }

        [DataElement]
        public byte A63 { get; set; }

        [DataElement]
        public byte AbilityImprovementThreshold { get; set; }

        [DataElement]
        public byte Specialization { get; set; }

        [DataElement]
        public byte StanceOrder { get; set; }

        [DataElement]
        public byte A68 { get; set; }

        [DataElement]
        public byte MinLevel { get; set; }

        [DataElement]
        public byte A70 { get; set; }

        [DataElement]
        public byte A71 { get; set; }

        [DataElement]
        public string A132 { get; set; }

        [DataElement]
        public long A136 { get; set; }

        [DataElement]
        public int A140 { get; set; }

        [DataElement]
        public long A142C { get; set; }

        [DataElement]
        public byte Disabled { get; set; }

        [DataElement]
        public byte CanClickOff { get; set; }

        [DataElement]
        public long ScaleStat { get; set; }

        [DataElement]
        public byte CantCrit { get; set; }

        [DataElement]
        public int MoraleLevel { get; set; }

        [DataElement]
        public long AttackType { get; set; }

        [DataElement]
        public long CounterAmount { get; set; }

        [DataElement]
        public int RangeMin { get; set; }

        [DataElement]
        public byte EnemyTargetIgnoreLOS { get; set; }

        [DataElement]
        public byte FriendlyTargetIgnoreLOS { get; set; }

        [DataElement]
        public int RangeMax { get; set; }

        [DataElement]
        public int UniqueGroup { get; set; }

        [DataElement]
        public long Faction { get; set; }

        [DataElement]
        public string UsableWithBuff { get; set; }

        [DataElement]
        public int AnimationDelay { get; set; }

        [DataElement]
        public int BinIndex { get; set; }

        [DataElement]
        public long SpellDamageType { get; set; }

        [DataElement]
        public byte Toggle { get; set; }

        [DataElement]
        public long ToggleGroup { get; set; }

        [DataElement]
        public long PatcherFileID { get; set; }

        [DataElement]
        public byte CreateBinData { get; set; }
    }
}