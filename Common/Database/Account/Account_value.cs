using FrameWork;
using System;

namespace Common
{
    // This tells the computer that this class represents a table in the database that stores information about a player's computer.
    // This is used to help keep accounts secure.
    [DataTable(PreCache = false, TableName = "account_value", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Account_value : DataObject
    {
        // These are the different pieces of information about a player's computer.
        private int _id;
        private int _accountId;
        private string _installId;
        private string _ip;
        private string _mac;
        private string _hdSerialHash;
        private string _cpuidHash;
        private DateTime _modifyDate;

        // This is the unique number for each record.
        [PrimaryKey(AutoIncrement = true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; Dirty = true; }
        }

        // This is the ID of the account that this computer information belongs to.
        [DataElement]
        public int AccountId
        {
            get { return _accountId; }
            set { _accountId = value; Dirty = true; }
        }

        // This is the ID of the game installation on the player's computer.
        [DataElement]
        public string InstallId
        {
            get { return _installId; }
            set { _installId = value; Dirty = true; }
        }

        // This is the player's IP address.
        [DataElement]
        public string IP
        {
            get { return _ip; }
            set { _ip = value; Dirty = true; }
        }

        // This is the MAC address of the player's computer, which is a unique hardware identifier.
        [DataElement]
        public string MAC
        {
            get { return _mac; }
            set { _mac = value; Dirty = true; }
        }

        // This is a unique number from the player's hard drive.
        [DataElement]
        public string HDSerialHash
        {
            get { return _hdSerialHash; }
            set { _hdSerialHash = value; Dirty = true; }
        }

        // This is a unique number from the player's computer processor.
        [DataElement]
        public string CPUIDHash
        {
            get { return _cpuidHash; }
            set { _cpuidHash = value; Dirty = true; }
        }

        // This is the last time this information was updated.
        [DataElement]
        public DateTime ModifyDate
        {
            get { return _modifyDate; }
            set { _modifyDate = value; Dirty = true; }
        }
    }
}