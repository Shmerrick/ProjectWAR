using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    [DataTable(PreCache = false, TableName = "rvr_player_contribution", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PlayerContribution : DataObject
    {
        [PrimaryKey]
        public int BattleFrontId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint CharacterId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ContributionTypeId { get; set; }

    }
}
