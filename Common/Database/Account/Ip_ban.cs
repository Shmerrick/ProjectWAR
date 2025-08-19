using FrameWork;
using System;

namespace Common
{
    // This tells the computer that this class represents a table in the database for banned IP addresses.
    // An IP address is like a house address on the internet. Banning an IP address is like blocking a house from getting mail.
    [DataTable(PreCache = false, TableName = "ip_bans", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Ip_ban : DataObject
    {
        // These are the different pieces of information for an IP ban.
        private string _ip;
        private int _expire;

        // This is the IP address that is banned.
        [PrimaryKey]
        public string Ip
        {
            get { return _ip; }
            set { _ip = value; Dirty = true; }
        }

        // This is when the ban will expire. If it's 1, the ban is permanent.
        [DataElement]
        public int Expire
        {
            get { return _expire; }
            set
            {
                _expire = value;
                Dirty = true;
            }
        }
    }
}