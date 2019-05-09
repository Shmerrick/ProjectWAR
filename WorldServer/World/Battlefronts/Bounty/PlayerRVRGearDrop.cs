using System;
using FrameWork;

namespace WorldServer.World.Battlefronts.Bounty
{
    /// <summary>
    /// Represents the rewards a zone lock will supply for a given Zone Lock.
    /// </summary>

    [DataTable(PreCache = false, TableName = "rvr_player_gear_drop", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PlayerRVRGearDrop : DataObject
    {
        [PrimaryKey]
        public uint ItemId { get; set; }
        [PrimaryKey]
        public int Realm { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint MinimumRenownRank { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint MaximumRenownRank { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint Money { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint DropChance { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint Career { get; set; }

        public override string ToString()
        {
            return $"RVR Player Gear Drop {ItemId}:{MinimumRenownRank}-{MaximumRenownRank}. {Money}";
        }
    }
}