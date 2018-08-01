using System;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "rvr_objects", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RvRObjectInfo : DataObject
    {
        [PrimaryKey] public ushort ModelId { get; set; }

        [DataElement(AllowDbNull = false)] public string Name { get; set; }

        [DataElement(AllowDbNull = false)] public byte Race { get; set; }

        [DataElement(AllowDbNull = false)] public uint MaxInteractionDist { get; set; }

        [DataElement(AllowDbNull = false)] public uint MaxHealth { get; set; }

        [DataElement(AllowDbNull = false)] public uint BuildTime { get; set; }

        [DataElement(AllowDbNull = false)] public float ExclusionRadius { get; set; }
    }
}
