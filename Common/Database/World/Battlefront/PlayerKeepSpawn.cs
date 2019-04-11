using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "player_keep_spawn", DatabaseName = "World",
        BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PlayerKeepSpawn : DataObject
    {
        [PrimaryKey] public int KeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int X{ get; set; }

        [DataElement(AllowDbNull = false)]
        public int Y { get; set; }
        [DataElement(AllowDbNull = false)]
        public int Z { get; set; }

    }
}