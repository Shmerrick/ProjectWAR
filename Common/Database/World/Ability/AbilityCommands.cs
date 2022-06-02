using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ability_commands", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class DBAbilityCommandInfo : DataObject
    {
        [DataElement(Varchar = 255)]
        public string AbilityName { get; set; }

        [DataElement]
        public byte AttackingStat { get; set; }

        [PrimaryKey]
        public byte CommandID { get; set; }

        [DataElement(Varchar = 255)]
        public string CommandName { get; set; }

        [PrimaryKey]
        public byte CommandSequence { get; set; }

        [DataElement]
        public byte EffectAngle { get; set; }

        [DataElement]
        public byte EffectRadius { get; set; }

        [DataElement(Varchar = 24)]
        public string EffectSource { get; set; }

        [PrimaryKey]
        public ushort Entry { get; set; }
        [DataElement]
        public bool FromAllTargets { get; set; }

        [DataElement]
        public bool IsDelayedEffect { get; set; }

        [DataElement]
        public byte MaxTargets { get; set; }

        [DataElement]
        public bool NoAutoUse { get; set; }

        [DataElement]
        public int PrimaryValue { get; set; }

        [DataElement]
        public int SecondaryValue { get; set; }
        [DataElement(Varchar = 24)]
        public string Target { get; set; }
    }
}