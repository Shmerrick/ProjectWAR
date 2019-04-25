using FrameWork;
using System;

namespace Common.Database.World.Creatures
{
    [DataTable(PreCache = false, TableName = "boss_spawn", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BossSpawn : DataObject
    {
        [PrimaryKey]
        public int BossSpawnId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ProtoId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int SpawnGuid { get; set; }
        
        [DataElement(AllowDbNull = true)]
        public string Name { get; set; }

        [DataElement (AllowDbNull = false)]
        public int Enabled{ get; set; }
    }
}
