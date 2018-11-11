using System;
using FrameWork;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "creature_abilities", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Creature_abilities : DataObject
    {
        [DataElement]
        public uint ProtoEntry { get; set; }

        [DataElement]
        public ushort AbilityId { get; set; }

        [DataElement]
        public ushort Cooldown { get; set; }

        [DataElement]
        public string Text { get; set; }

        [DataElement]
        public uint TimeStart { get; set; }

        [DataElement]
        public byte ActivateAtHealthPercent { get; set; }

        [DataElement]
        public byte DisableAtHealthPercent { get; set; }

        [DataElement]
        public byte AbilityCycle { get; set; }

        [DataElement]
        public byte Active { get; set; }

        [DataElement]
        public byte ActivateOnCombatStart { get; set; }

        [DataElement]
        public byte RandomTarget { get; set; }

		[DataElement]
		public byte TargetFocus { get; set; }

		[DataElement]
        public byte MinRange { get; set; }
    }
}