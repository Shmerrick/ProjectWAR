using FrameWork;
using System;

namespace Common
{
    // This tells the computer that this class represents a table in the database for game servers (realms).
    // A realm is like a separate world where players can play.
    [DataTable(PreCache = false, TableName = "realms", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Realm : DataObject
    {
        // These are all the different pieces of information about a realm.
        private string _Name;
        private string _Language;
        private string _Adresse;
        private int _Port;
        public RpcClientInfo Info;

        private string _Allow_trials = "0";
        private string _Charfxeravailable;
        private string _Legacy;
        private string _Bonus_destruction = "0";
        private string _Bonus_order = "0";
        private string _Redirect = "0";
        private string _Region = "STR_REGION_NORTHAMERICA";
        private string _Retired = "0";
        private string _Waiting_destruction = "0";
        private string _Waiting_order = "0";
        private string _Density_destruction = "0";
        private string _Density_order = "0";
        private string _Openrvr = "1";
        private string _Rp = "1";
        private string _Status = "0";

        // This is the unique number for each realm.
        [PrimaryKey]
        public byte RealmId { get; set; }

        // This is the name of the realm, like "Averland" or "Badlands".
        [DataElement(Varchar = 255)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        // This is the language of the realm, like "English" or "German".
        [DataElement(Varchar = 255)]
        public string Language
        {
            get { return _Language; }
            set { _Language = value; Dirty = true; }
        }

        // This is the address of the realm on the internet.
        [DataElement(Varchar = 255)]
        public string Adresse
        {
            get { return _Adresse; }
            set { _Adresse = value; Dirty = true; }
        }

        // This is the port number that players connect to.
        [DataElement(AllowDbNull = false)]
        public int Port
        {
            get { return _Port; }
            set { _Port = value; Dirty = true; }
        }

        // This says if trial accounts are allowed on this realm.
        [DataElement(Varchar = 255)]
        public string AllowTrials
        {
            get { return _Allow_trials; }
            set { _Allow_trials = value; Dirty = true; }
        }

        // This says if character transfers are available on this realm.
        [DataElement(Varchar = 255)]
        public string CharfxerAvailable
        {
            get { return _Charfxeravailable; }
            set { _Charfxeravailable = value; Dirty = true; }
        }

        // This says if this is a legacy realm.
        [DataElement(Varchar = 255)]
        public string Legacy
        {
            get { return _Legacy; }
            set { _Legacy = value; Dirty = true; }
        }

        // This is the bonus that Destruction players get on this realm.
        [DataElement(Varchar = 255)]
        public string BonusDestruction
        {
            get { return _Bonus_destruction; }
            set { _Bonus_destruction = value; Dirty = true; }
        }

        // This is the bonus that Order players get on this realm.
        [DataElement(Varchar = 255)]
        public string BonusOrder
        {
            get { return _Bonus_order; }
            set { _Bonus_order = value; Dirty = true; }
        }

        // This says if players should be redirected to another realm.
        [DataElement(Varchar = 255)]
        public string Redirect
        {
            get { return _Redirect; }
            set { _Redirect = value; Dirty = true; }
        }

        // This is the region of the realm, like "North America" or "Europe".
        [DataElement(Varchar = 255)]
        public string Region
        {
            get { return _Region; }
            set { _Region = value; Dirty = true; }
        }

        // This says if the realm is retired and no longer in use.
        [DataElement(Varchar = 255)]
        public string Retired
        {
            get { return _Retired; }
            set { _Retired = value; Dirty = true; }
        }

        // This is the number of Destruction players waiting to get into the realm.
        [DataElement(Varchar = 255)]
        public string WaitingDestruction
        {
            get { return _Waiting_destruction; }
            set { _Waiting_destruction = value; Dirty = true; }
        }

        // This is the number of Order players waiting to get into the realm.
        [DataElement(Varchar = 255)]
        public string WaitingOrder
        {
            get { return _Waiting_order; }
            set { _Waiting_order = value; Dirty = true; }
        }

        // This is how crowded the realm is with Destruction players.
        [DataElement(Varchar = 255)]
        public string DensityDestruction
        {
            get { return _Density_destruction; }
            set { _Density_destruction = value; Dirty = true; }
        }

        // This is how crowded the realm is with Order players.
        [DataElement(Varchar = 255)]
        public string DensityOrder
        {
            get { return _Density_order; }
            set { _Density_order = value; Dirty = true; }
        }

        // This says if Open Realm vs. Realm is enabled on this realm.
        [DataElement(Varchar = 255)]
        public string OpenRvr
        {
            get { return _Openrvr; }
            set { _Openrvr = value; Dirty = true; }
        }

        // This says if this is a role-playing realm.
        [DataElement(Varchar = 255)]
        public string Rp
        {
            get { return _Rp; }
            set { _Rp = value; Dirty = true; }
        }

        // This is the status of the realm, like "Online" or "Offline".
        [DataElement(Varchar = 255)]
        public string Status
        {
            get { return _Status; }
            set { _Status = value; Dirty = true; }
        }

        // This says if the realm is online (1) or offline (0).
        [DataElement(AllowDbNull = false)]
        public byte Online { get; set; }

        // This is the last time the realm was online.
        [DataElement]
        public DateTime OnlineDate { get; set; }

        // This is the number of players currently online on the realm.
        [DataElement]
        public uint OnlinePlayers { get; set; }

        // This is the number of Order players online.
        [DataElement]
        public uint OrderCount { get; set; }

        // This is the number of Destruction players online.
        [DataElement]
        public uint DestructionCount { get; set; }

        // This is the maximum number of players allowed on the realm.
        [DataElement]
        public uint MaxPlayers { get; set; }

        // This is the total number of Order characters on the realm.
        [DataElement]
        public uint OrderCharacters { get; set; }

        // This is the total number of Destruction characters on the realm.
        [DataElement]
        public uint DestruCharacters { get; set; }

        private long _nextRotationTime;

        // This is the time when the scenarios (mini-games) will change.
        [DataElement]
        public long NextRotationTime { get { return _nextRotationTime; } set { _nextRotationTime = value; Dirty = true; } }

        // This is the special password that Game Masters can use to log in.
        [DataElement]
        public string MasterPassword { get; set; }

        // This is the time when the realm was last started.
        [DataElement]
        public int BootTime { get; set; }
    }
}