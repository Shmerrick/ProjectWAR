using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "dye_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Dye_Info : DataObject
    {
        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint Price { get; set; }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public string Name { get; set; }
    }
}
