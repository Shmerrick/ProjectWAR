using FrameWork;
using System;

namespace Common
{
    // This tells the computer that this class represents a table in the database that keeps track of punishments.
    // It's like a record of when a player gets in trouble.
    [DataTable(PreCache = false, TableName = "account_sanction_logs", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AccountSanctionInfo : DataObject
    {
        // This is the ID of the account that got the punishment.
        [DataElement]
        public int AccountId { get; set; }

        // This is the name of the Game Master who gave the punishment.
        [DataElement(Varchar = 24)]
        public string IssuedBy { get; set; }

        // This is the type of punishment, like a ban or a mute.
        [DataElement(Varchar = 24)]
        public string ActionType { get; set; }

        // This is the Game Master level of the person who gave the punishment.
        [DataElement]
        public int IssuerGmLevel { get; set; }

        // This is how long the punishment lasts.
        [DataElement]
        public string ActionDuration { get; set; }

        // This is a note about why the punishment was given.
        [DataElement(Varchar = 255)]
        public string ActionLog { get; set; }

        // This is when the punishment was given.
        [DataElement]
        public int ActionTime { get; set; }
    }
}