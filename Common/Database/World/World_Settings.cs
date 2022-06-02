using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "world_settings", DatabaseName = "World")]
    [Serializable]
    public class World_Settings : DataObject
    {
        private int _SettingId, _Setting;

        [DataElement]
        public string Description
        { get; set; }

        [DataElement]
        public int Setting
        {
            get { return _Setting; }
            set { _Setting = value; Dirty = true; }
        }

        [PrimaryKey(AutoIncrement = true)]
        public int SettingId
        {
            get { return _SettingId; }
            set { _SettingId = value; Dirty = true; }
        }
    }
}