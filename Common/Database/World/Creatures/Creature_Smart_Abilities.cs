using FrameWork;
using System;

namespace Common.Database.World.Creatures
{
    [DataTable(PreCache = false, TableName = "creature_smart_abilities", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CreatureSmartAbilities : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int UniqueId { get; set; }

        [PrimaryKey]
        public int CreatureTypeId { get; set; }

        [PrimaryKey]
        public int CreatureSubTypeId { get; set; }

        [DataElement(AllowDbNull = false)]  // Description of this type/subtype - for ease of understanding
        public string CreatureTypeDescription { get; set; }

        [DataElement(AllowDbNull = false)]  // Name of this ability
        public string Name { get; set; }

        [DataElement(AllowDbNull = true)] // Speech to emote on execution
        public string Speech { get; set; }

        [DataElement(AllowDbNull = false)]  // Function that must be true for this to activate
        public string Condition { get; set; }

        [DataElement(AllowDbNull = false)]  // Function to be called on execution
        public string Execution { get; set; }

        [DataElement(AllowDbNull = false)]  // Chance of execution if Condition is true
        public int ExecuteChance { get; set; }

        [DataElement(AllowDbNull = false)]  // How long to cool down this ability (0 = none)
        public int CoolDown { get; set; }
        
        [DataElement(AllowDbNull = true)]  // Sounds to play
        public string Sound { get; set; }

      

    }



}
