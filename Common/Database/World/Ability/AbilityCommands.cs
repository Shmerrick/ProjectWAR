using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ability_commands", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class DBAbilityCommandInfo : DataObject
    {
        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement(Varchar = 255)]
        public string AbilityName { get; set; }

        [PrimaryKey]
        public byte CommandID { get; set; }

        [PrimaryKey]
        public byte CommandSequence { get; set; }

        [DataElement(Varchar = 255)]
        public string CommandName { get; set; }

        [DataElement]
        public int PrimaryValue { get; set; }

        [DataElement]
        public int SecondaryValue { get; set; }

        [DataElement]
        public byte EffectRadius { get; set; }

        [DataElement]
        public byte EffectAngle { get; set; }

        [DataElement(Varchar=24)]
        public string Target { get; set; }

        [DataElement(Varchar = 24)]
        public string EffectSource { get; set; }

        [DataElement]
        public byte MaxTargets { get; set; }

        [DataElement]
        public byte AttackingStat { get; set; }

        [DataElement]
        public bool IsDelayedEffect { get; set; }

        [DataElement]
        public bool FromAllTargets { get; set; }

        [DataElement]
        public bool NoAutoUse { get; set; }
    }
}
