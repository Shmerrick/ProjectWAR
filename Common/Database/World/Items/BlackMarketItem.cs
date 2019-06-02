using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;

namespace Common.Database.World.Items
{
    [DataTable(PreCache = false, TableName = "black_market_vendor_items", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BlackMarketItem
        : DataObject
    {
        [PrimaryKey]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte RealmId { get; set; }

    }


}
