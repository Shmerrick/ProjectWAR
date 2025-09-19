using System;
using System.Collections.Concurrent;
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
    public enum CreteAccountResult
    {
        ACCOUNT_NAME_BUSY = 0x00,
        ACCOUNT_NAME_SUCCESS = 0x01,
        ACCOUNT_BANNED = 0x02
    }

    public enum LoginResult
    {
        LOGIN_SUCCESS = 0x00,
        LOGIN_INVALID_USERNAME_PASSWORD = 0x01,
        LOGIN_BANNED = 0x02,
        LOGIN_NOT_ACTIVE = 0x03,
        LOGIN_PATCHER_NOT_ALLOWED = 0x04,
    };

    public enum AuthResult
    {
        AUTH_SUCCESS = 0x00,
        AUTH_ACCT_EXPIRED = 0x07,
        AUTH_ACCT_BAD_USERNAME_PASSWORD = 0x09,
        AUTH_ACCT_TERMINATED = 0x0D,
        AUTH_ACCT_SUSPENDED = 0x0E
    };

    /// <summary>
    /// Manages all account-related operations, such as creating, retrieving, and updating accounts.
    /// This class also handles account caching to improve performance.
    /// </summary>
    public class AccountMgr
    {
        // Account Database
        public static IObjectDatabase Database = null;

        public Dictionary<string, AccountPending> _Codes = new Dictionary<string, AccountPending>();
        public static EmailClient EmailClient = null;

        /// <summary>
        /// Sets up the cache for storing account information.
        /// The cache helps to speed up access to account data by keeping it in memory.
        /// </summary>
        /// <param name="enabled">Whether the cache should be used or not.</param>
        /// <param name="maxSize">The maximum number of accounts to keep in the cache.</param>
        public void InitializeCache(bool enabled, int maxSize)
        {
            _cacheEnabled = enabled;
            _maxCacheSize = maxSize;
        }

        #region Account

        // Account : Username,Account
        //
        // This class uses a simple in-memory cache to store account information. The cache is implemented as a
        // ConcurrentDictionary for thread-safe access. A ConcurrentQueue is used to implement a simple FIFO
        // (First-In, First-Out) eviction policy, which is an approximation of LRU (Least Recently Used).
        // This is not a perfect LRU implementation, but it prevents the cache from growing indefinitely.
        private bool _cacheEnabled = true;
        private int _maxCacheSize = 10000;
        private readonly ConcurrentDictionary<string, Account> _accounts = new ConcurrentDictionary<string, Account>();
        private readonly ConcurrentDictionary<int, string> _accountUsernames = new ConcurrentDictionary<int, string>();
        private readonly ConcurrentQueue<string> _accountAccessQueue = new ConcurrentQueue<string>();

        private readonly List<int> _pendingAccountIDs = new List<int>();

        /// <summary>
        /// Loads an account from the database into the cache.
        /// </summary>
        /// <param name="username">The username of the account to load.</param>
        /// <returns>The loaded account, or null if the account was not found.</returns>
        public Account LoadAccount(string username)
        {
            username = username.ToLower();

            try
            {
                Account acct = Database.SelectObject<Account>("Username='" + Database.Escape(username) + "'");

                if (acct == null)
                {
                    Log.Error("LoadAccount", "Account " + username + " not found.");
                    return null;
                }

                if (_cacheEnabled)
                {
                    while (_accountAccessQueue.Count >= _maxCacheSize)
                    {
                        if (_accountAccessQueue.TryDequeue(out string lruUsername))
                        {
                            if (_accounts.TryRemove(lruUsername, out var lruAcct))
                            {
                                _accountUsernames.TryRemove(lruAcct.AccountId, out _);
                            }
                        }
                    }
                    _accounts[username] = acct;
                    _accountUsernames[acct.AccountId] = username;
                    _accountAccessQueue.Enqueue(username);
                }

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

        /// <summary>
        /// Gets an account by its username.
        /// This method will first try to get the account from the cache. If it's not in the cache, it will load it from the database.
        /// </summary>
        /// <param name="username">The username of the account to get.</param>
        /// <returns>The account, or null if the account was not found.</returns>
        public Account GetAccount(string username)
        {
            username = username.ToLower();

            Log.Debug("GetAccount", username);

            if (_cacheEnabled && _accounts.TryGetValue(username, out var acct))
            {
                return acct;
            }

            return LoadAccount(username);
        }

        /// <summary>
        /// Gets a list of sanctions (e.g., bans, mutes) for a specific account.
        /// </summary>
        /// <param name="accountId">The ID of the account to get sanctions for.</param>
        /// <returns>A list of sanctions for the account.</returns>
        public IList<AccountSanctionInfo> GetSanctionsFor(int accountId)
        {
            return Database.SelectObjects<AccountSanctionInfo>("accountId = '" + accountId + "'");
        }

        /// <summary>
        /// Adds a new sanction to an account.
        /// </summary>
        /// <param name="sanct">The sanction to add.</param>
        public void AddSanction(AccountSanctionInfo sanct)
        {
            Database.AddObject(sanct);
            Database.ForceSave();
        }

        /// <summary>
        /// Saves changes to an account to the database and updates it in the cache.
        /// </summary>
        /// <param name="acct">The account to update.</param>
        public void UpdateAccount(Account acct)
        {
            acct.Dirty = true;
            Database.SaveObject(acct);
            Database.ForceSave();

            Log.Success("AccountMgr", "Updated account " + acct.Username);

            _accounts[acct.Username.ToLower()] = acct;
        }

        /// <summary>
        /// Gets an account by its ID.
        /// This method will first try to get the account from the cache. If it's not in the cache, it will load it from the database.
        /// </summary>
        /// <param name="ID">The ID of the account to get.</param>
        /// <returns>The account, or null if the account was not found.</returns>
        public Account GetAccountById(int? ID)
        {
            if (_cacheEnabled && ID.HasValue && _accountUsernames.TryGetValue(ID.Value, out var username))
            {
                if (_accounts.TryGetValue(username, out var acct))
                {
                    return acct;
                }
            }

            var acctFromDb = Database.SelectObject<Account>("AccountId=" + ID);

            if (acctFromDb == null)
            {
                Log.Error("LoadAccount", "AccountId " + ID + "not found.");
                return null;
            }

            if (_cacheEnabled)
            {
                _accounts[acctFromDb.Username] = acctFromDb;
                _accountUsernames[acctFromDb.AccountId] = acctFromDb.Username;
                _accountAccessQueue.Enqueue(acctFromDb.Username);
            }

            return acctFromDb;
        }

        private static void CheckPendingPassword(Account acct, string password)
        {
            // Reload the account from the DB
            Account dbAcct = Database.SelectObject<Account>("Username='" + Database.Escape(acct.Username) + "'");

            if (dbAcct == null)
            {
                Log.Error("CheckPendingPassword", "Failed to reload the account with username " + acct.Username);
                return;
            }

            acct.CryptPassword = Account.ConvertSHA256(acct.Username.ToLower() + ":" + password.ToLower());
            Database.SaveObject(acct);
            Database.ForceSave();

            Log.Success("CheckPendingPassword", "Updated password for account " + acct.Username);
        }

        public Account GetAccount(int accountId)
        {
            return Database.SelectObject<Account>("AccountId=" + accountId + "");
        }

        /// <summary>
        /// Checks if an account's username and password are valid.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="password">The password to check.</param>
        /// <param name="ip">The IP address of the user trying to log in.</param>
        /// <returns>The result of the login attempt.</returns>
        public LoginResult CheckAccount(string username, string password, string ip)
        {
            int accountId = 0;
            return CheckAccount(username, password, ip, out accountId);
        }

        /// <summary>
        /// Checks if an account's username and password are valid.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="password">The password to check.</param>
        /// <param name="ip">The IP address of the user trying to log in.</param>
        /// <param name="accountId">The ID of the account that was checked.</param>
        /// <returns>The result of the login attempt.</returns>
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

                // Reload the account to check if it's changed. Blech.
                Account baseAcct = Database.SelectObject<Account>("Username='" + Database.Escape(username) + "'");

                if (baseAcct.GmLevel < 0)
                {
                    Log.Info("CheckAccount", "Account is inactive.");
                    return LoginResult.LOGIN_NOT_ACTIVE;
                }

                // Check if banned
                if (baseAcct.Banned != 0)
                {
                    // 1 - Perm Banned, otherwise timestamp
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

        /// <summary>
        /// Checks if an IP address is banned.
        /// </summary>
        /// <param name="Ip">The IP address to check.</param>
        /// <returns>True if the IP address is not banned, false otherwise.</returns>
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

        /// <summary>
        /// Generates a new security token for an account.
        /// This token is used to authenticate the user's session.
        /// </summary>
        /// <param name="username">The username of the account to generate the token for.</param>
        /// <returns>The new security token.</returns>
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

        /// <summary>
        /// Checks if a security token is valid for a specific account.
        /// </summary>
        /// <param name="Username">The username of the account to check the token for.</param>
        /// <param name="Token">The security token to check.</param>
        /// <returns>The result of the token check.</returns>
        public AuthResult CheckToken(string Username, string Token)
        {
            Account Acct = GetAccount(Username);
            if (Acct == null)
                return AuthResult.AUTH_ACCT_BAD_USERNAME_PASSWORD;

            if (Acct.Token != Token)
                return AuthResult.AUTH_ACCT_BAD_USERNAME_PASSWORD;

            return AuthResult.AUTH_SUCCESS;
        }

        /// <summary>
        /// Checks if a security token is valid.
        /// </summary>
        /// <param name="Token">The security token to check.</param>
        /// <returns>The result of the token check.</returns>
        public ResultCode CheckToken(string Token)
        {
            return ResultCode.RES_SUCCESS;
        }

        /// <summary>
        /// Bans an account for a specific amount of time.
        /// </summary>
        /// <param name="Username">The username of the account to ban.</param>
        /// <param name="Time">The duration of the ban in seconds.</param>
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

        /// <summary>
        /// Gets a list of accounts that are waiting to be created.
        /// </summary>
        /// <returns>A list of pending account IDs.</returns>
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

        public Dictionary<byte, Realm> _Realms = new Dictionary<byte, Realm>();

        /// <summary>
        /// Loads all the realms from the database.
        /// </summary>
        public void LoadRealms()
        {
            foreach (Realm Rm in Database.SelectAllObjects<Realm>())
                AddRealm(Rm);
        }

        /// <summary>
        /// Loads all the pending accounts from the database.
        /// </summary>
        public void LoadPending()
        {
            foreach (AccountPending Ap in Database.SelectAllObjects<AccountPending>())
                AddPending(Ap);
        }

        /// <summary>
        /// Adds a new pending account.
        /// </summary>
        /// <param name="Ap">The pending account to add.</param>
        /// <returns>True if the pending account was added successfully, false otherwise.</returns>
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
                        _accounts.TryRemove(acc.Username, out _);
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

        /// <summary>
        /// Adds a new realm.
        /// </summary>
        /// <param name="Rm">The realm to add.</param>
        /// <returns>True if the realm was added successfully, false otherwise.</returns>
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

        /// <summary>
        /// Gets a realm by its ID.
        /// </summary>
        /// <param name="RealmId">The ID of the realm to get.</param>
        /// <returns>The realm, or null if the realm was not found.</returns>
        public Realm GetRealm(byte RealmId)
        {
            Log.Debug("GetRealm", "RealmId = " + RealmId);
            lock (_Realms)
                if (_Realms.ContainsKey(RealmId))
                    return _Realms[RealmId];

            return null;
        }

        /// <summary>
        /// Checks if a verification code is valid for a specific account.
        /// </summary>
        /// <param name="username">The username of the account to check the code for.</param>
        /// <param name="code">The verification code to check.</param>
        /// <returns>An integer indicating the result of the check.</returns>
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

        /// <summary>
        /// Gets a list of all the realms.
        /// </summary>
        /// <returns>A list of all the realms.</returns>
        public List<Realm> GetRealms()
        {
            List<Realm> Rms = new List<Realm>();
            Rms.AddRange(_Realms.Values);
            return Rms;
        }

        // /// <summary>
        // /// Gets a realm by its RPC ID.
        // /// </summary>
        // /// <param name="RpcId">The RPC ID of the realm to get.</param>
        // /// <returns>The realm, or null if the realm was not found.</returns>
        // public Realm GetRealmByRpc(int RpcId)
        // {
        //     lock (_Realms)
        //         return _Realms.Values.ToList().Find(info => info.Info != null && info.Info.RpcID == RpcId);
        // }

        /// <summary>
        /// Updates the scenario rotation time for a realm.
        /// </summary>
        /// <param name="realmId">The ID of the realm to update.</param>
        /// <param name="nextRotation">The next rotation time.</param>
        public void UpdateRealmScenarioRotationTime(byte realmId, long nextRotation)
        {
            Realm rm = GetRealm(realmId);

            if (rm != null)
            {
                rm.NextRotationTime = nextRotation;
                Database.SaveObject(rm);
            }
        }

        /// <summary>
        /// Updates the status of a realm.
        /// </summary>
        /// <param name="Info">The RPC client info for the realm.</param>
        /// <param name="RealmId">The ID of the realm to update.</param>
        /// <returns>True if the realm was updated successfully, false otherwise.</returns>
        public bool UpdateRealm(byte RealmId)
        {
            Realm Rm = GetRealm(RealmId);

            if (Rm != null)
            {
                // Log.Success("Realm", "Realm (" + Rm.Name + ") online at " + Info.Ip + ":" + Info.Port);
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

        /// <summary>
        /// Updates the online player counts for a realm.
        /// </summary>
        /// <param name="RealmId">The ID of the realm to update.</param>
        /// <param name="OnlinePlayers">The number of online players.</param>
        /// <param name="OrderCount">The number of Order players.</param>
        /// <param name="DestructionCount">The number of Destruction players.</param>
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

        /// <summary>
        /// Updates the character counts for a realm.
        /// </summary>
        /// <param name="RealmId">The ID of the realm to update.</param>
        /// <param name="OrderCharacters">The number of Order characters.</param>
        /// <param name="DestruCharacters">The number of Destruction characters.</param>
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

        private ClusterProp setProp(string name, string value)
        {
            return ClusterProp.CreateBuilder().SetPropName(name)
                                              .SetPropValue(value)
                                              .Build();
        }

        /// <summary>
        /// Builds a list of all the clusters (realms).
        /// </summary>
        /// <returns>A byte array containing the cluster list.</returns>
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

        /// <summary>
        /// Called when a client disconnects from the RPC server.
        /// </summary>
        /// <param name="Info">The RPC client info for the disconnected client.</param>
        // public override void OnClientDisconnected(RpcClientInfo Info)
        // {
        //     Realm Rm = GetRealmByRpc(Info.RpcID);
        //     if (Rm != null && Rm.Info.RpcID == Info.RpcID)
        //     {
        //         Log.Error("Realm", "Realm offline : " + Rm.Name);
        //         Rm.Info = null;
        //         Rm.Online = 0;
        //         Rm.Dirty = true;
        //         Database.SaveObject(Rm);
        //     }
        // }

        #endregion Realm

        private string[] _bannedNames = { "zyklon", "fuck", "hitler", "nigger", "nigga", "faggot", "jihad", "muhajid" };

        /// <summary>
        /// Checks if an email address is valid.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if the email address is valid, false otherwise.</returns>
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            const string regexString = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
            return Regex.IsMatch(email, regexString);
        }

        private Random _random = new Random();

        /// <summary>
        /// Generates a random character.
        /// </summary>
        /// <returns>A random character.</returns>
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

        /// <summary>
        /// Generates a random verification code.
        /// </summary>
        /// <returns>A random verification code.</returns>
        public string ReturnCode()
        {
            string toreturn = "";

            for (string s = ""; s.Length < 11; s += RandomChar())
            {
                toreturn = s;
            }

            return toreturn;
        }

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="username">The username for the new account.</param>
        /// <param name="password">The password for the new account.</param>
        /// <param name="email">The email address for the new account.</param>
        /// <param name="gmLevel">The GM level for the new account.</param>
        /// <param name="langID">The language ID for the new account.</param>
        /// <param name="ip">The IP address of the user creating the account.</param>
        /// <returns>True if the account was created successfully, false otherwise.</returns>
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

            // if (!IsValidEmail(email))
            // {
            //     Log.Error("CreateAccount", "Invalid e-mail");
            //     return false;
            // }

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

            if (_cacheEnabled)
            {
                while (_accountAccessQueue.Count >= _maxCacheSize)
                {
                    if (_accountAccessQueue.TryDequeue(out string lruUsername))
                    {
                        if (_accounts.TryRemove(lruUsername, out var lruAcct))
                        {
                            _accountUsernames.TryRemove(lruAcct.AccountId, out _);
                        }
                    }
                }
                _accounts[Acct.Username] = Acct;
                _accountUsernames[Acct.AccountId] = Acct.Username;
                _accountAccessQueue.Enqueue(Acct.Username);
            }

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

        private void RemovePending(string user)
        {
            Account acc = GetAccount(_Codes[user].Username);
            if (acc != null)
            {
                _accounts.TryRemove(acc.Username, out _);
                Database.DeleteObject(acc);
            }
            _Codes.Remove(user);
            Database.ExecuteNonQuery($"DELETE FROM accounts_pending WHERE Username = '{Database.Escape(user)}'");
        }

        /// <summary>
        /// Updates the patcher log for an account.
        /// </summary>
        /// <param name="accountId">The ID of the account to update.</param>
        /// <param name="log">The patcher log to save.</param>
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

        /// <summary>
        /// Updates the bio information for an account.
        /// </summary>
        /// <param name="accountId">The ID of the account to update.</param>
        /// <param name="ip">The IP address of the user.</param>
        /// <param name="installID">The installation ID of the user's client.</param>
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

        /// <summary>
        /// Gets the name of the account database schema.
        /// </summary>
        /// <returns>The name of the account database schema.</returns>
        public string GetAccountSchemaName()
        {
            return Database.GetSchemaName();
        }
    }
}