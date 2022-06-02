using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "mount_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Mount_Info : DataObject
    {
        [PrimaryKey]
        public uint Id { get; set; }

        [DataElement(Unique = true, AllowDbNull = false)]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort Speed { get; set; }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public string Name { get; set; }
    }
}