using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ip_bans", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Ip_ban : DataObject
    {
        private string _ip;
        private int _expire;

        [PrimaryKey]
        public string Ip
        {
            get { return _ip; }
            set { _ip = value; Dirty = true; }
        }

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