using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using FrameWork;


namespace Common.Database.Account
{
    [DataTable(PreCache = false, TableName = "patch_myps", DatabaseName = "Patch", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Patch_MYP : DataObject
    {
        private int _Id;
        private string _name;
        private int _CRC32;
        private ulong _extractedSize;

        [PrimaryKey(AutoIncrement = true)]
        public int Id
        {
            get { return _Id; }
            set { _Id = value; Dirty = true; }
        }

        [DataElement(Varchar = 2000)]
        public string Name
        {
            get { return _name; }
            set { _name = value; Dirty = true; }
        }

        [DataElement]
        public int CRC32
        {
            get { return _CRC32; }
            set { _CRC32 = value; Dirty = true; }
        }

        public ulong ExtractedSize { get; set; }
        public uint AssetCount { get; set; }

    }
}
