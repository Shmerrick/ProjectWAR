using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "accounts", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Account : DataObject
    {
        private int _accountId;
        private string _username;
        private string _password;
        private string _ip;
        private string _token;
        private sbyte _gmLevel;
        private int _banned;
        private bool _packetLog;
        private int _adviceBlockEnd;
        private int _stealthMuteEnd;
        private string _banReason;
        private int _lastLogged;
        private int _lastNameChange;
        private string _lastPatcherLog;
        private int _coreLevel;
        private sbyte _noSurname;

        [PrimaryKey(AutoIncrement = true)]
        public int AccountId
        {
            get { return _accountId; }
            set { _accountId = value; Dirty = true; }
        }

        [DataElement()]
        public bool PacketLog
        {
            get { return _packetLog; }
            set { _packetLog = value; Dirty = true; }
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
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                Dirty = true;
            }
        }

        [DataElement(Varchar = 255)]
        public string CryptPassword { get; set; }

        [DataElement(Varchar = 255)]
        public string Ip
        {
            get { return _ip; }
            set
            {
                _ip = value;
                Dirty = true;
            }
        }

        [DataElement(Varchar = 255)]
        public string Token
        {
            get { return _token; }
            set
            {
                _token = value;
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull=false)]
        public sbyte GmLevel
        {
            get { return _gmLevel; }
            set
            {
                _gmLevel = value;
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = false)]
        public int Banned
        {
            get { return _banned; }
            set
            {
                _banned = value;
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = true)]
        public string BanReason
        {
            get { return _banReason; }
            set
            {
                _banReason = value;
                Dirty = true;
            }
        }

        public bool IsBanned => _banned > TCPManager.GetTimeStamp();
        public bool IsStealthMuted => _stealthMuteEnd > TCPManager.GetTimeStamp();
        public bool IsAdviceBlocked => _adviceBlockEnd > TCPManager.GetTimeStamp();

        [DataElement]
        public int AdviceBlockEnd
        {
            get { return _adviceBlockEnd; }
            set
            {
                _adviceBlockEnd = value;
                Dirty = true;
            }
        }

        [DataElement]
        public int StealthMuteEnd
        {
            get { return _stealthMuteEnd; }
            set
            {
                _stealthMuteEnd = value;
                Dirty = true;
            }
        }

        [DataElement]
        public int CoreLevel
        {
            get { return _coreLevel; }
            set
            {
                _coreLevel = value;
                Dirty = true;
            }
        }


        [DataElement]
        public int LastLogged
        {
            get { return _lastLogged; }
            set
            {
                _lastLogged = value;
                Dirty = true;
            }
        }

        [DataElement]
        public int LastNameChanged
        {
            get { return _lastNameChange; }
            set
            {
                _lastNameChange = value;
                Dirty = true;
            }
        }

        [DataElement]
        public string LastPatcherLog
        {
            get { return _lastPatcherLog; }
            set
            {
                _lastPatcherLog = value;
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = false)]
        public uint InvalidPasswordCount { get; set; } = 0;


        public static string ConvertSHA256(string value)
        {
            SHA256 sha = SHA256.Create();
            byte[] data = sha.ComputeHash(Encoding.ASCII.GetBytes(value));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        [DataElement(AllowDbNull = false)]
        public sbyte noSurname
        {
            get { return _noSurname; }
            set
            {
                _noSurname = value;
                Dirty = true;
            }
        }


        [DataElement(AllowDbNull = true)]
        public string Email { get; set; }
    }
} 
