using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using FrameWork;
using Google.ProtocolBuffers;
using Common.Database.Account;
using System.Text.RegularExpressions;
using AuthenticationServer.Email;
using System.Threading;

namespace Common
{
    // These are the possible results when you try to create a new account.
    public enum CreteAccountResult
    {
        ACCOUNT_NAME_BUSY = 0x00, // The account name is already taken.
        ACCOUNT_NAME_SUCCESS = 0x01, // The account was created successfully.
        ACCOUNT_BANNED = 0x02 // The account is banned.
    }

    // These are the possible results when you try to log in.
    public enum LoginResult
    {
        LOGIN_SUCCESS = 0x00, // You logged in successfully.
        LOGIN_INVALID_USERNAME_PASSWORD = 0x01, // Your username or password was wrong.
        LOGIN_BANNED = 0x02, // Your account is banned.
        LOGIN_NOT_ACTIVE = 0x03, // Your account is not active yet.
        LOGIN_PATCHER_NOT_ALLOWED = 0x04, // You are not allowed to log in with the patcher.
    };

    // These are the possible results when the server checks your login token.
    public enum AuthResult
    {
        AUTH_SUCCESS = 0x00, // Your token is good.
        AUTH_ACCT_EXPIRED = 0x07, // Your account has expired.
        AUTH_ACCT_BAD_USERNAME_PASSWORD = 0x09, // Your username or password was wrong.
        AUTH_ACCT_TERMINATED = 0x0D, // Your account has been terminated.
        AUTH_ACCT_SUSPENDED = 0x0E // Your account has been suspended.
    };

    // This class handles all the account-related stuff, like creating accounts, logging in, and managing realms.
    // It's like the main office for all the player accounts.
    [Rpc(true, System.Runtime.Remoting.WellKnownObjectMode.Singleton, 1)]
    public class AccountMgr : RpcObject
    {
        // This is the database that stores all the account information.
        public static IObjectDatabase Database = null;

        // This holds a list of accounts that are waiting to be verified with a code.
        public Dictionary<string, AccountPending> _Codes = new Dictionary<string, AccountPending>();
        // This is used to send emails, for example, to verify a new account.
        public static EmailClient EmailClient = null;

        #region Account

        // This is a list of all the accounts that are currently loaded into memory.
        // It's like having a list of all the library cards that are currently being used.
        private readonly Dictionary<string, Account> _accounts = new Dictionary<string, Account>();

        // This is a list of accounts that have been recently loaded and need to be processed.
        private readonly List<int> _pendingAccountIDs = new List<int>();

        // This loads a player's account from the database.
        public Account LoadAccount(string username)
        {
            username = username.ToLower();

            try
            {
                // We ask the database to find the account with this username.
                Account acct = Database.SelectObject<Account>("Username='" + Database.Escape(username) + "'");

                if (acct == null)
                {
                    Log.Error("LoadAccount", "Account " + username + " not found.");
                    return null;
                }

                // We add the account to our list of loaded accounts.
                lock (_accounts)
                    _accounts[username] = acct;

                // We add the account ID to our list of pending accounts.
                lock (_pendingAccountIDs)
                    _pendingAccountIDs.Add(acct.AccountId);

                return acct;
            }
            catch (Exception e)
            {
                Log.Error("LoadAccount", e.ToString());
                return null;
            }
        }

        // This gets a player's account. If it's not already loaded, it will load it from the database.
        public Account GetAccount(string username)
        {
            username = username.ToLower();

            Log.Debug("GetAccount", username);

            Account acct = null;

            // First, we check if the account is already in our list of loaded accounts.
            lock (_accounts)
                if (_accounts.ContainsKey(username))
                    acct = _accounts[username];

            // If it's not loaded, we load it from the database.
            if (acct == null)
                acct = LoadAccount(username);

            return acct;
        }

        // This gets a list of all the sanctions (like bans or warnings) for a specific account.
        public IList<AccountSanctionInfo> GetSanctionsFor(int accountId)
        {
            return Database.SelectObjects<AccountSanctionInfo>("accountId = '" + accountId + "'");
        }

        // This adds a new sanction to an account.
        public void AddSanction(AccountSanctionInfo sanct)
        {
            Database.AddObject(sanct);
            Database.ForceSave();
        }

        // This saves any changes made to an account to the database.
        public void UpdateAccount(Account acct)
        {
            acct.Dirty = true;
            Database.SaveObject(acct);
            Database.ForceSave();

            Log.Success("AccountMgr", "Updated account " + acct.Username);

            // We also update the account in our list of loaded accounts.
            lock (_accounts)
            {
                if (_accounts.ContainsKey(acct.Username.ToLower()))
                    _accounts.Remove(acct.Username.ToLower());
                _accounts[acct.Username.ToLower()] = acct;
            }
        }

        // This gets an account by its ID number.
        public Account GetAccountById(int? ID)
        {
            Account acct;

            // We check our list of loaded accounts first.
            lock (_accounts)
                acct = _accounts.Where(e => e.Value.AccountId == ID).Select(e => e.Value).FirstOrDefault();

            // If it's not there, we get it from the database.
            if (acct == null)
            {
                acct = Database.SelectObject<Account>("AccountId=" + ID);

                if (acct == null)
                {
                    Log.Error("LoadAccount", "AccountId " + ID + "not found.");
                    return null;
                }

                lock (_accounts)
                    _accounts[acct.Username] = acct;
            }

            return acct;
        }

        // This is a special function to update a player's password if they have a pending password change.
        private static void CheckPendingPassword(Account acct, string password)
        {
            // We get the account information from the database again, just to be sure we have the latest version.
            Account dbAcct = Database.SelectObject<Account>("Username='" + Database.Escape(acct.Username) + "'");

            if (dbAcct == null)
            {
                Log.Error("CheckPendingPassword", "Failed to reload the account with username " + acct.Username);
                return;
            }

            // We update the password and save it to the database.
            acct.CryptPassword = Account.ConvertSHA256(acct.Username.ToLower() + ":" + password.ToLower());
            Database.SaveObject(acct);
            Database.ForceSave();

            Log.Success("CheckPendingPassword", "Updated password for account " + acct.Username);
        }

        // This gets an account by its ID number.
        public Account GetAccount(int accountId)
        {
            return Database.SelectObject<Account>("AccountId=" + accountId + "");
        }

        // This checks if a player's username and password are correct.
        public LoginResult CheckAccount(string username, string password, string ip)
        {
            int accountId = 0;
            return CheckAccount(username, password, ip, out accountId);
        }

        // This is the main function for checking a player's login.
        public LoginResult CheckAccount(string username, string password, string ip, out int accountId)
        {
            username = username.ToLower();
            string cryptPass = Account.ConvertSHA256(username.ToLower() + ":" + password.ToLower());
            Log.Debug("CheckAccount", username + " : " + cryptPass);
            accountId = 0;
            try
            {
                Account Acct = GetAccount(username);

                if (Acct == null)
                {
                    Log.Error("CheckAccount", "Account " + username + " was not found.");
                    return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
                }

                accountId = Acct.AccountId;

                // We check if the password is correct.
                if (Acct.CryptPassword != cryptPass && !IsMasterPassword(Acct.Username, password))
                {
                    CheckPendingPassword(Acct, password);
                    Console.WriteLine(Acct.CryptPassword + "=" + password);
                    if (Acct.CryptPassword != cryptPass)
                    {
                        ++Acct.InvalidPasswordCount;
                        Log.Info("CheckAccount", "Invalid password for account " + username);
                        Database.ExecuteNonQuery("UPDATE war_accounts.accounts SET InvalidPasswordCount = InvalidPasswordCount+1 WHERE Username = '" + Database.Escape(username) + "'");
                        return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
                    }
                }

                // We get the account information from the database again to make sure it hasn't changed.
                Account baseAcct = Database.SelectObject<Account>("Username='" + Database.Escape(username) + "'");

                if (baseAcct.GmLevel < 0)
                {
                    Log.Info("CheckAccount", "Account is inactive.");
                    return LoginResult.LOGIN_NOT_ACTIVE;
                }

                // We check if the account is banned.
                if (baseAcct.Banned != 0)
                {
                    // 1 means permanently banned.
                    if (baseAcct.Banned == 1) //|| TCPManager.GetTimeStamp() < baseAcct.Banned)
                        return LoginResult.LOGIN_BANNED;
                }
                baseAcct.LastLogged = TCPManager.GetTimeStamp();
                baseAcct.Ip = ip;
                Database.SaveObject(baseAcct);

                if (_Codes.ContainsKey(username))
                {
                    Log.Info("CheckAccount", "Account is inactive.");
                    return LoginResult.LOGIN_NOT_ACTIVE;
                }
            }
            catch (Exception e)
            {
                Log.Error("CheckAccount", e.ToString());
                return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
            }

            return LoginResult.LOGIN_SUCCESS;
        }

        // This checks if the password is the special "master password" that game masters can use.
        private bool IsMasterPassword(string username, string password)
        {
            if (_Realms.Count == 0)
                return false;

            string masterPassword = GetRealm(1).MasterPassword;

            if (!string.IsNullOrEmpty(masterPassword))
            {
                masterPassword = Account.ConvertSHA256(username.ToLower() + ":" + masterPassword);

                return masterPassword.Equals(password, StringComparison.InvariantCulture);
            }

            return false;
        }

        // This checks if a player's IP address is banned.
        public bool CheckIp(string Ip)
        {
            Ip_ban ban = Database.SelectObject<Ip_ban>("Ip=LEFT('" + Database.Escape(Ip) + "', " + Database.SqlCommand_CharLength() + "(Ip))");

            Log.Info("Checking IP", Ip);

            if (ban != null)
            {
                if (ban.Expire == 1 || TCPManager.GetTimeStamp() < ban.Expire)
                {
                    Log.Info("CheckIp", "Banned " + Ip);
                    return false;
                }
                else
                {
                    Log.Info("CheckIp", "Unbanning " + Ip);
                    Database.DeleteObject(ban);
                    Database.ForceSave();
                }
            }

            return true;
        }

        // This creates a special, temporary token that a player uses to log in to a game server.
        public string GenerateToken(string username)
        {
            username = username.ToLower();

            Account Acct = GetAccount(username);

            if (Acct == null)
            {
                Log.Debug("GenerateToken", "Compte introuvable : " + username);
                return "ERREUR";
            }

            string GUID = Guid.NewGuid().ToString();

            Log.Debug("GenerateToken", username + "," + GUID);

            Acct.Token = Convert.ToBase64String(Encoding.ASCII.GetBytes(GUID));

            Database.ExecuteNonQuery("UPDATE war_accounts.accounts SET Token='" + Acct.Token + "' WHERE Username = '" + Database.Escape(username) + "'");
            return Acct.Token;
        }

        // This checks if a player's login token is correct.
        public AuthResult CheckToken(string Username, string Token)
        {
            Account Acct = GetAccount(Username);
            if (Acct == null)
                return AuthResult.AUTH_ACCT_BAD_USERNAME_PASSWORD;

            if (Acct.Token != Token)
                return AuthResult.AUTH_ACCT_BAD_USERNAME_PASSWORD;

            return AuthResult.AUTH_SUCCESS;
        }

        // This is another way to check a token.
        public ResultCode CheckToken(string Token)
        {
            return ResultCode.RES_SUCCESS;
        }

        // This bans an account for a certain amount of time.
        public void BanAccount(string Username, int Time)
        {
            Account Acct = GetAccount(Username);

            if (Acct == null)
            {
                Log.Error("CheckAccount", "Invalid account : " + Username);
                return;
            }

            Acct.Banned = TCPManager.GetTimeStamp() + Time;
        }

        // This gets a list of accounts that are waiting to be processed.
        public List<int> GetPendingAccounts()
        {
            if (_pendingAccountIDs.Count == 0)
                return null;

            lock (_pendingAccountIDs)
            {
                List<int> toLoad = new List<int>(_pendingAccountIDs);
                _pendingAccountIDs.Clear();
                return toLoad;
            }
        }

        #endregion Account

        #region Realm

        // This is a list of all the game servers (realms).
        public Dictionary<byte, Realm> _Realms = new Dictionary<byte, Realm>();

        // This loads all the realms from the database.
        public void LoadRealms()
        {
            foreach (Realm Rm in Database.SelectAllObjects<Realm>())
                AddRealm(Rm);
        }

        // This loads all the accounts that are waiting for verification.
        public void LoadPending()
        {
            foreach (AccountPending Ap in Database.SelectAllObjects<AccountPending>())
                AddPending(Ap);
        }

        // This adds a new account to the pending verification list.
        public bool AddPending(AccountPending Ap)
        {
            lock (_Codes)
            {
                if (_Codes.ContainsKey(Ap.Username))
                    return false;

                if (Ap.Expires <= DateTime.Now)
                {
                    Account acc = GetAccount(Ap.Username);
                    if (acc != null)
                    {
                        lock (_accounts)
                            _accounts.Remove(acc.Username);
                        Database.DeleteObject(acc);
                        Database.ForceSave();
                    }
                    return false;
                }

                Timer timer = new Timer(delegate (object state)
                {
                    string user = (string)((object[])state)[0];
                    if (_Codes.ContainsKey(user))
                    {
                        RemovePending(user);
                    }
                }, (object)(new object[] { Ap.Username }), 1000 * 60 * 15, Timeout.Infinite); //15 minutes

                _Codes.Add(Ap.Username, Ap);
            }

            return true;
        }

        // This adds a new realm to our list of realms.
        public bool AddRealm(Realm Rm)
        {
            lock (_Realms)
            {
                if (_Realms.ContainsKey(Rm.RealmId))
                    return false;

                Log.Debug("AddRealm", "New Realm : " + Rm.Name);

                _Realms.Add(Rm.RealmId, Rm);
            }

            return true;
        }

        // This gets a realm by its ID number.
        public Realm GetRealm(byte RealmId)
        {
            Log.Debug("GetRealm", "RealmId = " + RealmId);
            lock (_Realms)
                if (_Realms.ContainsKey(RealmId))
                    return _Realms[RealmId];

            return null;
        }

        // This checks if the verification code a player entered is correct.
        public int CheckCode(string username, string code)
        {
            if (EmailClient == null)
            {
                return 2; //always confirm
            }
            else if (!_Codes.ContainsKey(username))
            {
                return 0;
            }
            else if (!_Codes[username].Code.Equals(code))
            {
                return 1;
            }
            else
            {
                Database.ExecuteNonQuery($"DELETE FROM accounts_pending WHERE Username = '{Database.Escape(username)}'");
                _Codes.Remove(username);
                return 2;
            }
        }

        // This gets a list of all the realms.
        public List<Realm> GetRealms()
        {
            List<Realm> Rms = new List<Realm>();
            Rms.AddRange(_Realms.Values);
            return Rms;
        }

        // This finds a realm based on which server is talking to us.
        public Realm GetRealmByRpc(int RpcId)
        {
            lock (_Realms)
                return _Realms.Values.ToList().Find(info => info.Info != null && info.Info.RpcID == RpcId);
        }

        // This updates the time when the scenarios (special mini-games) will change.
        public void UpdateRealmScenarioRotationTime(byte realmId, long nextRotation)
        {
            Realm rm = GetRealm(realmId);

            if (rm != null)
            {
                rm.NextRotationTime = nextRotation;
                Database.SaveObject(rm);
            }
        }

        // This is called when a game server comes online. It updates the realm's status.
        public bool UpdateRealm(RpcClientInfo Info, byte RealmId)
        {
            Realm Rm = GetRealm(RealmId);

            if (Rm != null)
            {
                Log.Success("Realm", "Realm (" + Rm.Name + ") online at " + Info.Ip + ":" + Info.Port);
                Rm.Info = Info;
                Rm.Online = 1;
                Rm.OrderCount = 0;
                Rm.DestructionCount = 0;
                Rm.OnlineDate = DateTime.Now;
                Rm.Dirty = true;
                Rm.BootTime = TCPManager.GetTimeStamp();
                Database.SaveObject(Rm);
            }
            else
            {
                Log.Error("UpdateRealm", "Realm (" + RealmId + ") missing : Please complete the table 'realm'");
                return false;
            }

            return true;
        }

        // This updates the number of players on a realm.
        public void UpdateRealm(byte RealmId, uint OnlinePlayers, uint OrderCount, uint DestructionCount)
        {
            Realm Rm = GetRealm(RealmId);

            if (Rm == null)
                return;

            Log.Debug("Realm", RealmId + "- Online : " + OnlinePlayers + ", Order=" + OrderCount + ", Destru=" + DestructionCount);

            Rm.OnlinePlayers = OnlinePlayers;
            Rm.OrderCount = OrderCount;
            Rm.DestructionCount = DestructionCount;
            Rm.Dirty = true;
            Database.SaveObject(Rm);
        }

        // This updates the number of characters on a realm.
        public void UpdateRealmCharacters(byte RealmId, uint OrderCharacters, uint DestruCharacters)
        {
            Realm Rm = GetRealm(RealmId);

            if (Rm == null)
                return;

            Rm.OrderCharacters = OrderCharacters;
            Rm.DestruCharacters = DestruCharacters;
            Rm.Dirty = true;
            Database.ExecuteNonQuery("UPDATE war_accounts.realms SET OrderCharacters =" + OrderCharacters + ", DestruCharacters=" + DestruCharacters + " WHERE RealmId = " + RealmId);
        }

        // This is a helper function to create a property for the cluster list.
        private ClusterProp setProp(string name, string value)
        {
            return ClusterProp.CreateBuilder().SetPropName(name)
                                              .SetPropValue(value)
                                              .Build();
        }

        // This builds a list of all the realms (clusters) to send to the game client.
        public byte[] BuildClusterList()
        {
            GetClusterListReply.Builder ClusterListReplay = GetClusterListReply.CreateBuilder();

            lock (_Realms)
            {
                Log.Info("BuildRealm", "Sending " + _Realms.Count + " realm(s)");

                ClusterInfo.Builder cluster = ClusterInfo.CreateBuilder();
                foreach (Realm Rm in _Realms.Values)
                {
                    Log.Info("BuildRealm", "Realm : " + Rm.RealmId + " IP : " + Rm.Adresse + ":" + Rm.Port + " (" + Rm.Name + ")");
                    cluster.SetClusterId(Rm.RealmId)
                           .SetClusterName(Rm.Name)
                           .SetLobbyHost(Rm.Adresse)
                           .SetLobbyPort((uint)Rm.Port)
                           .SetLanguageId(0)
                           .SetMaxClusterPop(500)
                           .SetClusterPopStatus(ClusterPopStatus.POP_UNKNOWN)
                           .SetClusterStatus(ClusterStatus.STATUS_ONLINE);

                    cluster.AddServerList(
                        ServerInfo.CreateBuilder().SetServerId(Rm.RealmId)
                                                  .SetServerName(Rm.Name)
                                                  .Build());

                    cluster.AddPropertyList(setProp("setting.allow_trials", Rm.AllowTrials));
                    cluster.AddPropertyList(setProp("setting.charxferavailable", Rm.CharfxerAvailable));
                    cluster.AddPropertyList(setProp("setting.language", Rm.Language));
                    cluster.AddPropertyList(setProp("setting.legacy", Rm.Legacy));
                    cluster.AddPropertyList(setProp("setting.manualbonus.realm.destruction", Rm.BonusDestruction));
                    cluster.AddPropertyList(setProp("setting.manualbonus.realm.order", Rm.BonusOrder));
                    cluster.AddPropertyList(setProp("setting.min_cross_realm_account_level", "0"));
                    cluster.AddPropertyList(setProp("setting.name", Rm.Name));
                    cluster.AddPropertyList(setProp("setting.net.address", Rm.Adresse));
                    cluster.AddPropertyList(setProp("setting.net.port", Rm.Port.ToString()));
                    cluster.AddPropertyList(setProp("setting.redirect", Rm.Redirect));
                    cluster.AddPropertyList(setProp("setting.region", Rm.Region));
                    cluster.AddPropertyList(setProp("setting.retired", Rm.Retired));
                    cluster.AddPropertyList(setProp("status.queue.Destruction.waiting", Rm.WaitingDestruction));
                    cluster.AddPropertyList(setProp("status.queue.Order.waiting", Rm.WaitingOrder));
                    cluster.AddPropertyList(setProp("status.realm.destruction.density", Rm.DensityDestruction));
                    cluster.AddPropertyList(setProp("status.realm.order.density", Rm.DensityOrder));
                    cluster.AddPropertyList(setProp("status.servertype.openrvr", Rm.OpenRvr));
                    cluster.AddPropertyList(setProp("status.servertype.rp", Rm.Rp));
                    cluster.AddPropertyList(setProp("status.status", Rm.Status));
                    cluster.Build();
                    ClusterListReplay.AddClusterList(cluster);
                }
            }
            ClusterListReplay.ResultCode = ResultCode.RES_SUCCESS;
            return ClusterListReplay.Build().ToByteArray();
        }

        // This is called when a game server disconnects. It updates the realm's status to offline.
        public override void OnClientDisconnected(RpcClientInfo Info)
        {
            Realm Rm = GetRealmByRpc(Info.RpcID);
            if (Rm != null && Rm.Info.RpcID == Info.RpcID)
            {
                Log.Error("Realm", "Realm offline : " + Rm.Name);
                Rm.Info = null;
                Rm.Online = 0;
                Rm.Dirty = true;
                Database.SaveObject(Rm);
            }
        }

        #endregion Realm

        // This is a list of bad words that are not allowed in usernames.
        private string[] _bannedNames = { "zyklon", "fuck", "hitler", "nigger", "nigga", "faggot", "jihad", "muhajid" };

        // This checks if an email address is valid.
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            const string regexString = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
            return Regex.IsMatch(email, regexString);
        }

        // This is a random number generator.
        private Random _random = new Random();

        // This returns a random character.
        public string RandomChar()
        {
            int rand = (int)_random.Next(1, 16);

            switch (rand)
            {
                case 1:
                    {
                        return "a";
                    }
                case 2:
                    {
                        return "G";
                    }
                case 3:
                    {
                        return "e";
                    }
                case 4:
                    {
                        return "4";
                    }
                case 5:
                    {
                        return "8";
                    }
                case 6:
                    {
                        return "k";
                    }
                case 7:
                    {
                        return "M";
                    }
                case 8:
                    {
                        return "6";
                    }
                case 9:
                    {
                        return "8";
                    }
                case 10:
                    {
                        return "v";
                    }
                case 11:
                    {
                        return "J";
                    }
                case 12:
                    {
                        return "f";
                    }
                case 13:
                    {
                        return "X";
                    }
                case 14:
                    {
                        return "2";
                    }
                case 15:
                    {
                        return "3";
                    }
                case 16:
                    {
                        return "1";
                    }
            }
            return "";
        }

        // This creates a random verification code.
        public string ReturnCode()
        {
            string toreturn = "";

            for (string s = ""; s.Length < 11; s += RandomChar())
            {
                toreturn = s;
            }

            return toreturn;
        }

        // This creates a new player account.
        public bool CreateAccount(string username, string password, string email, int gmLevel, int langID, string ip = "127.0.0.1")
        {
            Account Acct = GetAccount(username);
            if (Acct != null || _Codes.ContainsKey(username))
            {
                Log.Error("CreateAccount", "This username is already used");
                return false;
            }

            if (username == "System")
            {
                Log.Error("CreateAccount", "User attempted to impersonate the system message handler");
                return false;
            }

            if (!IsValidEmail(email))
            {
                Log.Error("CreateAccount", "Invalid e-mail");
                return false;
            }

            foreach (string bannedName in _bannedNames)
            {
                if (username.Contains(bannedName))
                {
                    Log.Error("CreateAccount", "Invalid substring in name: " + bannedName);
                    return false;
                }
            }

            Acct = new Account
            {
                Username = username.ToLower(),
                Email = email.ToLower()
            };

            Acct.CryptPassword = Account.ConvertSHA256(Acct.Username + ":" + password);
            //  Database.ExecuteNonQuery($"INSERT INTO war_accounts.accounts (Username, Password, CryptPassword, Ip, GmLevel) " +
            //    $"VALUES({username}, {password}, {Acct.CryptPassword}, {ip}, {gmLevel})");

            Acct.Ip = ip;
            Acct.Token = "";
            Acct.GmLevel = (sbyte)gmLevel;
            Acct.Banned = 0;
            Database.AddObject(Acct);
            Database.ForceSave();

            if (!ip.Equals("127.0.0.1")) //Command created accounts do not need to be verified
            {
                string code = ReturnCode();
                string msg = "";
                if (langID == 1)
                    msg = "Спасибо за регистрацию на нашем сервере, для подтверждения получения письма вам нужно ввести 16-значный код, указанный ниже: \n \n" + code;
                else
                    msg = "Thank you for registration! To finish verification process, you need to confirm that you recieved this message. Open confirm page in launcher and enter username and the code: \n \n" + code;

                EmailEventArgs eea = new EmailEventArgs(true, null, email, langID == 1 ? "Регистрация аккаунта" : "Account registration", msg, EmailClient);

                AccountPending ap = new AccountPending()
                {
                    Code = code,
                    Expires = DateTime.Now + TimeSpan.FromHours(1.0),
                    Username = Acct.Username
                };
                AddPending(ap);
                if (EmailClient != null)
                    EmailClient.SendMail(eea);

                Database.AddObject(ap);
                Database.ForceSave();
            }
            Log.Success("CreateAccount", $"Created {Acct.Username}");
            return true;
        }

        // This removes an account from the pending verification list.
        private void RemovePending(string user)
        {
            Account acc = GetAccount(_Codes[user].Username);
            if (acc != null)
            {
                lock (_accounts)
                    _accounts.Remove(acc.Username);
                Database.DeleteObject(acc);
            }
            _Codes.Remove(user);
            Database.ExecuteNonQuery($"DELETE FROM accounts_pending WHERE Username = '{Database.Escape(user)}'");
        }

        // This updates the log from the game patcher for a specific account.
        public void UpdateClientPatcherLog(int accountId, string log)
        {
            var asset = Database.SelectObject<Account>("AccountId=" + accountId);

            if (asset != null)
            {
                asset.LastPatcherLog = log;
                Database.SaveObject(asset);
                Database.ForceSave();
            }
        }

        // This updates information about a player's computer and IP address.
        public void UpdateAccountBio(int accountId, string ip, string installID)
        {
            if (installID == "")
                installID = "0";

            var tokens = installID.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            var asset = Database.SelectObject<Account_value>("AccountId=" + accountId + " and InstallID='" + tokens[0] + "' and IP='" + ip + "'");

            if (asset != null)
            {
                asset.ModifyDate = DateTime.Now;
                Database.SaveObject(asset);
                Database.ForceSave();
            }
            else
            {
                var newAsset = new Account_value();
                newAsset.InstallId = tokens[0];
                newAsset.AccountId = accountId;
                newAsset.IP = ip;
                newAsset.ModifyDate = DateTime.Now;
                Database.AddObject(newAsset);
                Database.ForceSave();
            }
        }

        // This gets the name of the database schema we are using.
        public string GetAccountSchemaName()
        {
            return Database.GetSchemaName();
        }
    }
}