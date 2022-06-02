using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ability_modifiers", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AbilityModifierEffect : DataObject
    {
        public AbilityModifierEffect nextMod;

        [DataElement(Varchar = 48)]
        public string AffectedAbility { get; set; }

        [DataElement()]
        public ushort Affecting { get; set; }

        [DataElement()]
        public byte BuffLine { get; set; }

        [DataElement()]
        public ushort Entry { get; set; }

        [DataElement(Varchar = 255)]
        public string ModifierCommandName { get; set; }

        [DataElement()]
        public byte PreOrPost { get; set; }

        [DataElement()]
        public int PrimaryValue { get; set; }

        [DataElement()]
        public int SecondaryValue { get; set; }

        [DataElement()]
        public byte Sequence { get; set; }

        [DataElement(Varchar = 48)]
        public string SourceAbility { get; set; }

        [DataElement()]
        public byte TargetCommandID { get; set; }

        [DataElement()]
        public byte TargetCommandSequence { get; set; }

        public void AddModifier(AbilityModifierEffect newMod)
        {
            if (nextMod != null)
                nextMod.AddModifier(newMod);
            else nextMod = newMod;
        }
    }
}