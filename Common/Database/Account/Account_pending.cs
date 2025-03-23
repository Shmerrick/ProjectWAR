using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "accounts_pending", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AccountPending : DataObject
    {
        private string _username;
        private string _code;
        private DateTime _expires;
        private int _id;

        [PrimaryKey(AutoIncrement = true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; Dirty = true; }
        }

        [DataElement(Unique = true, Varchar = 255)]
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                Dirty = true;
            }
        }

        [DataElement(Varchar = 255)]
        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;
                Dirty = true;
            }
        }

        [DataElement]
        public DateTime Expires
        {
            get { return _expires; }
            set
            {
                _expires = value;
                Dirty = true;
            }
        }
    }
}