using FrameWork;
using System;

namespace Common.Database.World.Items
{
    [DataTable(PreCache = false, TableName = "black_market_vendor_items", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BlackMarketItem : DataObject
    {
        [PrimaryKey]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte RealmId { get; set; }
    }
}