using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;

namespace WorldServer.World.Battlefronts.NewDawn.Rewards
{
    /// <summary>
    /// Represents the rewards a zone lock will supply for a given Zone Lock.
    /// </summary>

    [DataTable(PreCache = false, TableName = "rvr_reward_zone_lock", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RenownBandRVRZoneLock : DataObject
    {
        [PrimaryKey]
        public int RenownBand { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint CrestId { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint Money { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint CrestCount { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint BaseRP { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint BaseXP { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint BaseInf { get; set; }

        public override string ToString()
        {
            return $"Renown Band Zone Lock {RenownBand}. {CrestCount}x{CrestId}, {Money}, {BaseRP}, {BaseXP}, {BaseInf} ";
        }
    }
}