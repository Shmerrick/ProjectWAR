using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "pairing_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRPairing : DataObject
    {
        [PrimaryKey]
        public int PairingId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string PairingName { get; set; }

    }
}
