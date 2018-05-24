using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "realms", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Realm : DataObject
    {
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
        private string _Region = "STR_REGION_EUROPE";
        private string _Retired = "0";
        private string _Waiting_destruction = "0";
        private string _Waiting_order = "0";
        private string _Density_destruction = "0";
        private string _Density_order = "0";
        private string _Openrvr = "1" ;
        private string _Rp = "1";
        private string _Status = "0";

        [PrimaryKey]
        public byte RealmId { get; set; }

        [DataElement(Varchar = 255)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Language
        {
            get { return _Language; }
            set { _Language = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Adresse
        {
            get { return _Adresse; }
            set { _Adresse = value; Dirty = true; }
        }

        [DataElement(AllowDbNull=false)]
        public int Port
        {
            get { return _Port; }
            set { _Port = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string AllowTrials
        {
            get { return _Allow_trials; }
            set { _Allow_trials = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string CharfxerAvailable
        {
            get { return _Charfxeravailable; }
            set { _Charfxeravailable = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Legacy
        {
            get { return _Legacy; }
            set { _Legacy = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string BonusDestruction
        {
            get { return _Bonus_destruction; }
            set { _Bonus_destruction = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string BonusOrder
        {
            get { return _Bonus_order; }
            set { _Bonus_order = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Redirect
        {
            get { return _Redirect; }
            set { _Redirect = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Region
        {
            get { return _Region; }
            set { _Region = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Retired
        {
            get { return _Retired; }
            set { _Retired = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string WaitingDestruction
        {
            get { return _Waiting_destruction; }
            set { _Waiting_destruction = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string WaitingOrder
        {
            get { return _Waiting_order; }
            set { _Waiting_order = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string DensityDestruction
        {
            get { return _Density_destruction; }
            set { _Density_destruction = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string DensityOrder
        {
            get { return _Density_order; }
            set { _Density_order = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string OpenRvr
        {
            get { return _Openrvr; }
            set { _Openrvr = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Rp
        {
            get { return _Rp; }
            set { _Rp = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Status
        {
            get { return _Status; }
            set { _Status = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Online { get; set; }

        [DataElement]
        public DateTime OnlineDate { get; set; }

        [DataElement]
        public uint OnlinePlayers { get; set; }

        [DataElement]
        public uint OrderCount { get; set; }

        [DataElement]
        public uint DestructionCount { get; set; }

        [DataElement]
        public uint MaxPlayers { get; set; }

        [DataElement]
        public uint OrderCharacters { get; set; }

        [DataElement]
        public uint DestruCharacters { get; set; }

        private long _nextRotationTime;

        [DataElement]
        public long NextRotationTime { get { return _nextRotationTime; } set { _nextRotationTime = value; Dirty = true; } }

        [DataElement]
        public string MasterPassword { get; set; }

        [DataElement]
        public int BootTime { get; set; }
    }
}
