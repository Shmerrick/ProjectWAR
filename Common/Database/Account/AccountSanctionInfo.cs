using System;
using System.Data.SqlTypes;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "account_sanction_logs", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AccountSanctionInfo : DataObject
    {
        [DataElement]
        public int AccountId { get; set; }

        [DataElement(Varchar = 24)]
        public string IssuedBy { get; set; }

        [DataElement(Varchar = 24)]
        public string ActionType { get; set; }

        [DataElement]
        public int IssuerGmLevel { get; set; }

        [DataElement]
        public string ActionDuration { get; set; }

        [DataElement(Varchar = 255)]
        public string ActionLog { get; set; }

        [DataElement]
        public int ActionTime { get; set; }
    }
}
