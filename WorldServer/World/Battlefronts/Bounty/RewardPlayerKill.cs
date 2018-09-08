using System;
using FrameWork;

namespace WorldServer.World.Battlefronts.Bounty
{
    /// <summary>
    /// Represents the rewards that are added per renown band (allows higher RR players to get slightly better and more appropriate rewards).
    /// </summary>

    [DataTable(PreCache = false, TableName = "rvr_reward_player_kill", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RewardPlayerKill : DataObject
    {
        [PrimaryKey]
        public int RenownBand { get; set; }
        [DataElement(AllowDbNull = false)]
        public int CrestId { get; set; }
        [DataElement(AllowDbNull = false)]
        public int Money { get; set; }
        [DataElement(AllowDbNull = false)]
        public int CrestCount { get; set; }
        [DataElement(AllowDbNull = false)]
        public int BaseRP { get; set; }
        [DataElement(AllowDbNull = false)]
        public int BaseXP { get; set; }
        [DataElement(AllowDbNull = false)]
        public int BaseInf { get; set; }

        public override string ToString()
        {
            return $"RewardPlayerKill {RenownBand}. {CrestCount}x{CrestId}, {Money}, {BaseRP}, {BaseXP}, {BaseInf} ";
        }
    }
}