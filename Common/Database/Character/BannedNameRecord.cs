using System;
using SystemData;
using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "banned_names", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BannedNameRecord : DataObject
    {
        [PrimaryKey]
        public string NameString { get; set; }

        [DataElement]
        public string FilterTypeString
        {
            get
            {
                return FilterType.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    FilterType = (NameFilterType)Enum.Parse(typeof(NameFilterType), value);
            }
        }

        public NameFilterType FilterType;
    }
}
