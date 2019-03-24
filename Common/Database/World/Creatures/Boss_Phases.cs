using FrameWork;
using System;

namespace Common.Database.World.Creatures
{
    [DataTable(PreCache = false, TableName = "boss_spawn_phases", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BossSpawnPhase : DataObject
    {
        [PrimaryKey]
        public int BossSpawnId { get; set; }

        [PrimaryKey]  
        public int PhaseId { get; set; }
        
        [DataElement(AllowDbNull = false)]  // Name of this ability
        public string Name { get; set; }


    }



}
