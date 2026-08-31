using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using FrameWork;
using Google.ProtocolBuffers;
using Common.Database.Account;

namespace Common {

    public enum CreteAccountResult {
        ACCOUNT_NAME_BUSY = 0x00,
        ACCOUNT_NAME_SUCCESS = 0x01,
        ACCOUNT_BANNED = 0x02
    }
    public enum LoginResult {
        LOGIN_SUCCESS = 0x00,
        LOGIN_INVALID_USERNAME_PASSWORD = 0x01,
        LOGIN_BANNED = 0x02,
        LOGIN_NOT_ACTIVE = 0x03,
        LOGIN_PATCHER_NOT_ALLOWED = 0x04,
    };

    public enum AuthResult {
        AUTH_SUCCESS = 0x00,
        AUTH_ACCT_EXPIRED = 0x07,
        AUTH_ACCT_BAD_USERNAME_PASSWORD = 0x09,
        AUTH_ACCT_TERMINATED = 0x0D,
        AUTH_ACCT_SUSPENDED = 0x0E
    };

    [Rpc(true, System.Runtime.Remoting.WellKnownObjectMode.Singleton, 1)]
    public class AccountMgr : RpcObject {
        // Account Database
        public static IObjectDatabase Database = null;

        #region Account

        // Account : Username,Account
        private readonly Dictionary<string, Account> _accounts = new Dictionary<string, Account>();
        private readonly List<int> _pendingAccountIDs = new List<int>();

        public Account LoadAccount(string username, bool logError = true) {
            username = username.ToLower();

            try {
                Account acct = Database.SelectObject<Account>("Username='" + Database.Escape(username) + "'");

                if (acct == null) {
                    if (logError)
                        Log.Error("LoadAccount", "Account " + username + " not found.");
                    return null;
                }

                lock (_accounts)
                    _accounts[username] = acct;

                lock (_pendingAccountIDs)
                    _pendingAccountIDs.Add(acct.AccountId);

                return acct;
            } catch (Exception e) {
                Log.Error("LoadAccount", e.ToString());
                return null;
            }
        }

        public Account GetAccount(string username) {
            username = username.ToLower();

            Log.Debug("GetAccount", username);

            Account acct = null;

            lock (_accounts)
                if (_accounts.ContainsKey(username))
                    acct = _accounts[username];

            if (acct == null)
                acct = LoadAccount(username, false);

            return acct;
        }

        public IList<AccountSanctionInfo> GetSanctionsFor(int accountId) {
            return Database.SelectObjects<AccountSanctionInfo>("accountId = '" + accountId + "'");
        }

        public void AddSanction(AccountSanctionInfo sanct) {
            Database.AddObject(sanct);
            Database.ForceSave();
        }

        public void UpdateAccount(Account acct) {
            acct.Dirty = true;
            Database.SaveObject(acct);
            Database.ForceSave();

            Log.Success("AccountMgr", "Updated account " + acct.Username);

            lock (_accounts) {
                if (_accounts.ContainsKey(acct.Username.ToLower()))
                    _accounts.Remove(acct.Username.ToLower());
                _accounts[acct.Username.ToLower()] = acct;
            }
        }

        public Account GetAccountById(int? ID) {
            Account acct;

            lock (_accounts)
                acct = _accounts.Where(e => e.Value.AccountId == ID).Select(e => e.Value).FirstOrDefault();

            if (acct == null) {
                acct = Database.SelectObject<Account>("AccountId=" + ID);

                if (acct == null) {
                    Log.Error("LoadAccount", "AccountId " + ID + "not found.");
                    return null;
                }

                lock (_accounts)
                    _accounts[acct.Username] = acct;
            }

            return acct;
        }

        private static bool IsBcryptHash(string cryptPassword) {
            return !string.IsNullOrEmpty(cryptPassword) && cryptPassword.StartsWith("$2", StringComparison.Ordinal);
        }

        private static bool VerifyStoredPassword(Account acct, string submittedPasswordHash, out bool matchedLegacyHash) {
            matchedLegacyHash = false;

            if (IsBcryptHash(acct.CryptPassword))
                return BCrypt.Net.BCrypt.Verify(submittedPasswordHash, acct.CryptPassword);

            matchedLegacyHash = string.Equals(acct.CryptPassword, submittedPasswordHash, StringComparison.Ordinal);
            return matchedLegacyHash;
        }

        private static void SaveCanonicalPasswordHash(Account acct, string passwordHash, string logContext, string successMessage) {
            acct.CryptPassword = BCrypt.Net.BCrypt.HashPassword(passwordHash);
            acct.Password = string.Empty;
            Database.SaveObject(acct);
            Database.ForceSave();
            Log.Success(logContext, successMessage);
        }

        private static void CheckPendingPassword(Account acct) {
            // Reload the account from the DB
            Account dbAcct = Database.SelectObject<Account>("Username='" + Database.Escape(acct.Username) + "'");

            if (dbAcct == null) {
                Log.Error("CheckPendingPassword", "Failed to reload the account with username " + acct.Username);
                return;
            }

            if (string.IsNullOrEmpty(dbAcct.Password)) {
                acct.CryptPassword = dbAcct.CryptPassword;
                acct.Password = dbAcct.Password;
                return;
            }

            acct.CryptPassword = BCrypt.Net.BCrypt.HashPassword(Account.ConvertClientPasswordHash(acct.Username, dbAcct.Password));
            acct.Password = string.Empty;
            Database.SaveObject(acct);
            Database.ForceSave();

            Log.Success("CheckPendingPassword", "Updated password for account " + acct.Username + " to the canonical client hash.");
        }

        public Account GetAccount(int accountId) {
            return Database.SelectObject<Account>("AccountId=" + accountId + "");
        }

        public LoginResult CheckAccount(string username, string password, string ip) {
            int accountId = 0;
            return CheckAccount(username, password, ip, out accountId);
        }

        /// <summary>
        /// Validates a user's password against their database record.
        /// This method automatically upgrades legacy SHA256 hashed passwords to strong BCrypt hashes 
        /// upon the first successful login, ensuring no passwords remain weakly protected.
        /// </summary>
        /// <summary>GmLevel value reserved for system/bot accounts that must never be player-accessible.</summary>
        public const sbyte SYSTEM_ACCOUNT_GMLEVEL = -2;

        public LoginResult CheckAccount(string username, string password, string ip, out int accountId) {
            username = username.ToLowerInvariant();

            Log.Debug("CheckAccount", "Authenticating account " + username);
            accountId = 0;
            try {
                Account Acct = GetAccount(username);

                if (Acct == null) {
                    Log.Error("CheckAccount", "Account " + username + " was not found.");
                    return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
                }

                // Block system accounts (e.g. the bot account) before any password work.
                // Return the same result as a bad password so the account's existence is not revealed.
                if (Acct.GmLevel == SYSTEM_ACCOUNT_GMLEVEL) {
                    Log.Error("CheckAccount", $"Blocked login attempt for system account '{username}' from {ip}.");
                    return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
                }

                bool matchedLegacyHash;
                bool valid = VerifyStoredPassword(Acct, password, out matchedLegacyHash);
                bool usedMasterPassword = false;

                if (!valid && IsMasterPassword(Acct.Username, password)) {
                    valid = true;
                    usedMasterPassword = true;
                }

                if (!valid) {
                    CheckPendingPassword(Acct);
                    valid = VerifyStoredPassword(Acct, password, out matchedLegacyHash);

                    if (!valid) {
                        ++Acct.InvalidPasswordCount;
                        Log.Info("CheckAccount", "Invalid password for account " + username);
                        Database.ExecuteNonQuery($"UPDATE `{Database.GetSchemaName()}`.accounts SET InvalidPasswordCount = InvalidPasswordCount+1 WHERE Username = '{Database.Escape(username)}'");
                        return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
                    }
                }

                // Password verified — safe to expose the account ID to the caller.
                accountId = Acct.AccountId;

                if (!usedMasterPassword) {
                    if (matchedLegacyHash) {
                        SaveCanonicalPasswordHash(
                            Acct,
                            password,
                            "CheckAccount",
                            "Successfully upgraded account " + username + " password to canonical BCrypt security.");
                    } else if (!string.IsNullOrEmpty(Acct.Password)) {
                        CheckPendingPassword(Acct);
                    }
                }

                // Reload the account to pick up any changes made since the cached copy was loaded.
                Account baseAcct = Database.SelectObject<Account>("Username='" + Database.Escape(username) + "'");

                if (baseAcct == null) {
                    Log.Error("CheckAccount", $"Failed to reload account record for '{username}' after authentication.");
                    return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
                }

                if (baseAcct.GmLevel < 0) {
                    Log.Info("CheckAccount", "Account is inactive.");
                    return LoginResult.LOGIN_NOT_ACTIVE;
                }

                // Check if banned
                if (baseAcct.Banned != 0) {
                    // 1 - Perm Banned, otherwise timestamp
                    if (baseAcct.Banned == 1)
                        return LoginResult.LOGIN_BANNED;
                }
                baseAcct.LastLogged = TCPManager.GetTimeStamp();
                baseAcct.Ip = ip;
                Database.SaveObject(baseAcct);
            } catch (Exception e) {
                Log.Error("CheckAccount", e.ToString());
                accountId = 0;
                return LoginResult.LOGIN_INVALID_USERNAME_PASSWORD;
            }

            return LoginResult.LOGIN_SUCCESS;
        }

        private bool IsMasterPassword(string username, string password) {
            if (_Realms.Count == 0)
                return false;

            string masterPassword = GetRealm(1).MasterPassword;

            if (!string.IsNullOrEmpty(masterPassword)) {
                masterPassword = Account.ConvertSHA256(username.ToLower() + ":" + masterPassword);

                return masterPassword.Equals(password, StringComparison.InvariantCulture);
            }

            return false;
        }

        public bool CheckIp(string Ip) {
            Ip_ban ban = Database.SelectObject<Ip_ban>("Ip=LEFT('" + Database.Escape(Ip) + "', " + Database.SqlCommand_CharLength() + "(Ip))");

            Log.Info("Checking IP", Ip);

            if (ban != null) {
                if (ban.Expire == 1 || TCPManager.GetTimeStamp() < ban.Expire) {
                    Log.Info("CheckIp", "Banned " + Ip);
                    return false;
                } else {
                    Log.Info("CheckIp", "Unbanning " + Ip);
                    Database.DeleteObject(ban);
                    Database.ForceSave();
                }
            }

            return true;
        }

        public string GenerateToken(string username) {
            username = username.ToLower();

            Account Acct = GetAccount(username);

            if (Acct == null) {
                Log.Debug("GenerateToken", "Compte introuvable : " + username);
                return "ERREUR";
            }

            string GUID = Guid.NewGuid().ToString();

            Log.Debug("GenerateToken", username + "," + GUID);

            Acct.Token = Convert.ToBase64String(Encoding.ASCII.GetBytes(GUID));

            Database.ExecuteNonQuery($"UPDATE `{Database.GetSchemaName()}`.accounts SET Token='{Acct.Token}' WHERE Username = '{Database.Escape(username)}'");
            return Acct.Token;
        }

        public AuthResult CheckToken(string Username, string Token) {
            Account Acct = GetAccount(Username);
            if (Acct == null)
                return AuthResult.AUTH_ACCT_BAD_USERNAME_PASSWORD;

            if (Acct.Token != Token)
                return AuthResult.AUTH_ACCT_BAD_USERNAME_PASSWORD;

            return AuthResult.AUTH_SUCCESS;
        }

        public ResultCode CheckToken(string Token) {
            return ResultCode.RES_SUCCESS;
        }

        public void BanAccount(string Username, int Time) {
            Account Acct = GetAccount(Username);

            if (Acct == null) {
                Log.Error("CheckAccount", "Invalid account : " + Username);
                return;
            }

            Acct.Banned = TCPManager.GetTimeStamp() + Time;
        }

        public List<int> GetPendingAccounts() {
            if (_pendingAccountIDs.Count == 0)
                return null;

            lock (_pendingAccountIDs) {
                List<int> toLoad = new List<int>(_pendingAccountIDs);
                _pendingAccountIDs.Clear();
                return toLoad;
            }
        }

        #endregion

        #region Realm

        public Dictionary<byte, Realm> _Realms = new Dictionary<byte, Realm>();

        public void LoadRealms() {
            foreach (Realm Rm in Database.SelectAllObjects<Realm>())
                AddRealm(Rm);
        }
        public bool AddRealm(Realm Rm) {
            lock (_Realms) {
                if (_Realms.ContainsKey(Rm.RealmId))
                    return false;

                Log.Debug("AddRealm", "New Realm : " + Rm.Name);

                _Realms.Add(Rm.RealmId, Rm);
            }

            return true;
        }
        public Realm GetRealm(byte RealmId) {
            Log.Debug("GetRealm", "RealmId = " + RealmId);
            lock (_Realms)
                if (_Realms.ContainsKey(RealmId))
                    return _Realms[RealmId];

            return null;
        }
        public List<Realm> GetRealms() {
            List<Realm> Rms = new List<Realm>();
            Rms.AddRange(_Realms.Values);
            return Rms;
        }
        public Realm GetRealmByRpc(int RpcId) {
            lock (_Realms)
                return _Realms.Values.ToList().Find(info => info.Info != null && info.Info.RpcID == RpcId);
        }

        public void UpdateRealmScenarioRotationTime(byte realmId, long nextRotation) {
            Realm rm = GetRealm(realmId);

            if (rm != null) {
                rm.NextRotationTime = nextRotation;
                Database.SaveObject(rm);
            }
        }

        public bool UpdateRealm(RpcClientInfo Info, byte RealmId) {
            Realm Rm = GetRealm(RealmId);

            if (Rm != null) {
                Log.Success("Realm", "Realm (" + Rm.Name + ") online at " + Info.Ip + ":" + Info.Port);
                Rm.Info = Info;
                Rm.Online = 1;
                Rm.OrderCount = 0;
                Rm.DestructionCount = 0;
                Rm.OnlineDate = DateTime.Now;
                Rm.Dirty = true;
                Rm.BootTime = TCPManager.GetTimeStamp();
                Database.SaveObject(Rm);
            } else {
                Log.Error("UpdateRealm", "Realm (" + RealmId + ") missing : Please complete the table 'realm'");
                return false;
            }

            return true;
        }
        public void UpdateRealm(byte RealmId, uint OnlinePlayers, uint OrderCount, uint DestructionCount) {
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

        public void UpdateRealmCharacters(byte RealmId, uint OrderCharacters, uint DestruCharacters) {
            Realm Rm = GetRealm(RealmId);

            if (Rm == null)
                return;

            Rm.OrderCharacters = OrderCharacters;
            Rm.DestruCharacters = DestruCharacters;
            Rm.Dirty = true;
            Database.ExecuteNonQuery($"UPDATE `{Database.GetSchemaName()}`.realms SET OrderCharacters={OrderCharacters}, DestruCharacters={DestruCharacters} WHERE RealmId={RealmId}");
        }

        private ClusterProp setProp(string name, string value) {
            return ClusterProp.CreateBuilder().SetPropName(name)
                                              .SetPropValue(value)
                                              .Build();
        }
        public byte[] BuildClusterList() {

            GetClusterListReply.Builder ClusterListReplay = GetClusterListReply.CreateBuilder();

            lock (_Realms) {
                Log.Info("BuildRealm", "Sending " + _Realms.Count + " realm(s)");

                ClusterInfo.Builder cluster = ClusterInfo.CreateBuilder();
                foreach (Realm Rm in _Realms.Values) {
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


        public override void OnClientDisconnected(RpcClientInfo Info) {
            Realm Rm = GetRealmByRpc(Info.RpcID);
            if (Rm != null && Rm.Info.RpcID == Info.RpcID) {
                Log.Error("Realm", "Realm offline : " + Rm.Name);
                Rm.Info = null;
                Rm.Online = 0;
                Rm.Dirty = true;
                Database.SaveObject(Rm);
            }
        }

        #endregion

        private string[] _bannedNames = { "zyklon", "fuck", "hitler", "nigger", "nigga", "faggot", "jihad", "muhajid" };

        // AI Agent (Antigravity): Hierarchy Simplification.
        // Enforces hierarchical levels (1-5) and defaults to Level 1 (Player) if invalid.
        public bool CreateAccount(string username, string password, int gmLevel, string ip = "127.0.0.1") {
            string normalizedUsername = username.ToLowerInvariant();

            Account Acct = GetAccount(normalizedUsername);
            if (Acct != null) {
                Log.Error("CreateAccount", "This username is already used");
                return false;
            }

            if (normalizedUsername == "system") {
                Log.Error("CreateAccount", "User attempted to impersonate the system message handler");
                return false;
            }

            foreach (string bannedName in _bannedNames) {
                if (normalizedUsername.Contains(bannedName)) {
                    Log.Error("CreateAccount", "Invalid substring in name: " + bannedName);
                    return false;
                }
            }

            // AI Agent (Antigravity): Clamp GM level between 1 (Player) and 5 (Admin).
            if (gmLevel < 1)
            {
                Log.Info("CreateAccount", $"GM level {gmLevel} is below minimum. Defaulting to 1 (Player).");
                gmLevel = 1;
            }
            else if (gmLevel > 5)
            {
                Log.Info("CreateAccount", $"GM level {gmLevel} is above maximum. Capping at 5 (Admin).");
                gmLevel = 5;
            }

            Acct = new Account {
                Username = normalizedUsername,
                Password = string.Empty
            };

            Acct.CryptPassword = BCrypt.Net.BCrypt.HashPassword(Account.ConvertClientPasswordHash(normalizedUsername, password));
            //  Database.ExecuteNonQuery($"INSERT INTO war_accounts.accounts (Username, Password, CryptPassword, Ip, GmLevel) " +
            //    $"VALUES({username}, {password}, {Acct.CryptPassword}, {ip}, {gmLevel})");

            Acct.Ip = ip;
            Acct.Token = "";
            Acct.GmLevel = (sbyte)gmLevel;
            Acct.Banned = 0;
            AccountMgr.Database.AddObject(Acct);
            AccountMgr.Database.ForceSave();

            lock (_accounts)
                _accounts[normalizedUsername] = Acct;


            Log.Success("CreateAccount", $"Created {Acct.Username}");
            return true;
        }

        public bool ResetPassword(string username, string password) {
            Account account = LoadAccount(username, false);
            if (account == null) {
                Log.Error("ResetPassword", "Could not locate " + username + " to reset password");
                return false;
            }

            SaveCanonicalPasswordHash(
                account,
                Account.ConvertClientPasswordHash(account.Username, password),
                "ResetPassword",
                "Password reset for " + account.Username);

            return true;
        }

        public void UpdateClientPatcherLog(int accountId, string log) {
            var asset = Database.SelectObject<Account>("AccountId=" + accountId);

            if (asset != null) {
                asset.LastPatcherLog = log;
                Database.SaveObject(asset);
                Database.ForceSave();
            }
        }

        public void UpdateAccountBio(int accountId, string ip, string installID) {
            if (installID == "")
                installID = "0";

            var tokens = installID.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);


            var asset = Database.SelectObject<Account_value>("AccountId=" + accountId + " and InstallID='" + tokens[0] + "' and IP='" + ip + "'");

            if (asset != null) {
                asset.ModifyDate = DateTime.Now;
                Database.SaveObject(asset);
                Database.ForceSave();
            } else {
                var newAsset = new Account_value();
                newAsset.InstallId = tokens[0];
                newAsset.AccountId = accountId;
                newAsset.IP = ip;
                newAsset.ModifyDate = DateTime.Now;
                Database.AddObject(newAsset);
                Database.ForceSave();
            }
        }

        public string GetAccountSchemaName() {
            return Database.GetSchemaName();
        }
    }
}
