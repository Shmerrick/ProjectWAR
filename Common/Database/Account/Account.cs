using FrameWork;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    // This tells the computer that this class represents a table in the database.
    // It's like a blueprint for a player's account information.
    [DataTable(PreCache = false, TableName = "accounts", DatabaseName = "Accounts", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Account : DataObject
    {
        // These are all the different pieces of information that make up a player's account.
        // It's like the different fields on a library card.
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

        // This is the unique number for each account. It's like a library card number.
        [PrimaryKey(AutoIncrement = true)]
        public int AccountId
        {
            get { return _accountId; }
            set { _accountId = value; Dirty = true; }
        }

        // This is a switch to turn on or off logging of the data packets sent by this player.
        // It's useful for debugging.
        [DataElement()]
        public bool PacketLog
        {
            get { return _packetLog; }
            set { _packetLog = value; Dirty = true; }
        }

        // This is the player's username. It has to be unique.
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

        // This is the player's password.
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

        // This is the player's password after it has been encrypted, so it's safe.
        [DataElement(Varchar = 255)]
        public string CryptPassword { get; set; }

        // This is the player's IP address, which is like their address on the internet.
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

        // This is a special, temporary token that the player uses to log in.
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

        // This is the player's Game Master level. A higher level gives them more powers in the game.
        [DataElement(AllowDbNull = false)]
        public sbyte GmLevel
        {
            get { return _gmLevel; }
            set
            {
                _gmLevel = value;
                Dirty = true;
            }
        }

        // This is a number that says if the account is banned. If it's not 0, the player can't log in.
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

        // This is the reason why the account was banned.
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

        // These are quick checks to see if the player is banned, muted, or blocked from giving advice.
        public bool IsBanned => _banned > TCPManager.GetTimeStamp();
        public bool IsStealthMuted => _stealthMuteEnd > TCPManager.GetTimeStamp();
        public bool IsAdviceBlocked => _adviceBlockEnd > TCPManager.GetTimeStamp();

        // This is when the player's advice block ends.
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

        // This is when the player's stealth mute ends.
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

        // This is the player's core level.
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

        // This is the last time the player logged in.
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

        // This is the last time the player changed their name.
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

        // This is the log from the last time the player used the patcher.
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

        // This counts how many times the player has entered the wrong password.
        [DataElement(AllowDbNull = false)]
        public uint InvalidPasswordCount { get; set; } = 0;

        // This is a special function to encrypt the password, so it's stored safely.
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

        // This is a switch to say if the player has a surname or not.
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

        // This is the player's email address.
        [DataElement(AllowDbNull = true)]
        public string Email { get; set; }
    }
}