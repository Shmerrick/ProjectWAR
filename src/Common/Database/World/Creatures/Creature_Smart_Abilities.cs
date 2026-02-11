using FrameWork;
using System;

namespace Common.Database.World.Creatures
{
    [DataTable(PreCache = false, TableName = "creature_smart_abilities", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CreatureSmartAbilities : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int Guid { get; set; }

        [PrimaryKey]
        public int CreatureTypeId { get; set; }

        [PrimaryKey]
        public int CreatureSubTypeId { get; set; }

        [DataElement(AllowDbNull = false)]  // Description of this type/subtype - for ease of understanding
        public string CreatureTypeDescription { get; set; }

        [DataElement(AllowDbNull = false)]  // Name of this ability
        public string SpellCastName { get; set; }

        [DataElement(AllowDbNull = true)] // Speech to emote on execution
        public string SpellCastSpeech { get; set; }

        [DataElement(AllowDbNull = false)]  // Function that must be true for this to activate
        public string SpellCondition { get; set; }

        [DataElement(AllowDbNull = false)]  // Function to be called on execution
        public string SpellCastExecution { get; set; }

        [DataElement(AllowDbNull = false)]  // Chance of execution if Condition is true
        public int SpellExecuteChance { get; set; }

        [DataElement(AllowDbNull = false)]  // How long to cool down this ability (0 = none)
        public int SpellCastCoolDown { get; set; }

        [DataElement(AllowDbNull = true)]  // Sounds to play
        public string SpellCastSound { get; set; }
    }
}