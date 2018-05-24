using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ability_modifier_checks", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AbilityModifierCheck : DataObject
    {
        public AbilityModifierCheck nextCheck;

        public void AddCheck(AbilityModifierCheck newCheck)
        {
            if (nextCheck != null)
                nextCheck.AddCheck(newCheck);
            else nextCheck = newCheck;
        }

        [DataElement]
        public ushort Entry { get; set; }

        [DataElement(Varchar = 48)]
        public string SourceAbility { get; set; }

        [DataElement]
        public ushort Affecting { get; set; }

        [DataElement(Varchar = 48)]
        public string AffectedAbility { get; set; }

        [DataElement]
        public byte PreOrPost { get; set; }

        [DataElement]
        public byte ID { get; set; }

        [DataElement]
        public byte Sequence { get; set; }

        [DataElement(Varchar = 255)]
        public string CommandName { get; set; }

        [DataElement]
        public byte FailCode { get; set; }

        [DataElement]
        public int PrimaryValue { get; set; }

        [DataElement]
        public int SecondaryValue { get; set; }
    }
}
