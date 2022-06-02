using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "account_value", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Account_value : DataObject
    {
        private int _accountId;
        private string _cpuidHash;
        private string _hdSerialHash;
        private int _id;
        private string _installId;
        private string _ip;
        private string _mac;
        private DateTime _modifyDate;

        [DataElement]
        public int AccountId
        {
            get { return _accountId; }
            set { _accountId = value; Dirty = true; }
        }

        [DataElement]
        public string CPUIDHash
        {
            get { return _cpuidHash; }
            set { _cpuidHash = value; Dirty = true; }
        }

        [DataElement]
        public string HDSerialHash
        {
            get { return _hdSerialHash; }
            set { _hdSerialHash = value; Dirty = true; }
        }

        [PrimaryKey(AutoIncrement = true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; Dirty = true; }
        }
        [DataElement]
        public string InstallId
        {
            get { return _installId; }
            set { _installId = value; Dirty = true; }
        }

        [DataElement]
        public string IP
        {
            get { return _ip; }
            set { _ip = value; Dirty = true; }
        }

        [DataElement]
        public string MAC
        {
            get { return _mac; }
            set { _mac = value; Dirty = true; }
        }
        [DataElement]
        public DateTime ModifyDate
        {
            get { return _modifyDate; }
            set { _modifyDate = value; Dirty = true; }
        }
    }
}