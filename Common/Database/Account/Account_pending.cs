using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    // This tells the computer that this class represents a table in the database for accounts that are waiting to be verified.
    // It's like a list of people who have signed up for a library card but haven't picked it up yet.
    [DataTable(PreCache = false, TableName = "accounts_pending", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AccountPending : DataObject
    {
        // These are the different pieces of information for a pending account.
        private string _username;
        private string _code;
        private DateTime _expires;
        private int _id;

        // This is the unique number for each pending account record.
        [PrimaryKey(AutoIncrement = true)]
        public int Id
        {
            get { return _id; }
            set { _id = value; Dirty = true; }
        }

        // This is the username of the account waiting for verification.
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

        // This is the special code that was sent to the player's email to verify their account.
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

        // This is when the verification code will expire and no longer work.
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