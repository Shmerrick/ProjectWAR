using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using FrameWork;
using Grpc.Core;
using Microsoft.Extensions.Hosting;

namespace AccountCacher.Services;

public class AccountMgrService : AccountMgr.AccountMgrBase, IHostedService
{
    // Account Database
    private IObjectDatabase _database = null;
    
    // Account : Username,Account
    //
    // This class uses a simple in-memory cache to store account information. The cache is implemented as a
    // ConcurrentDictionary for thread-safe access. A ConcurrentQueue is used to implement a simple FIFO
    // (First-In, First-Out) eviction policy, which is an approximation of LRU (Least Recently Used).
    // This is not a perfect LRU implementation, but it prevents the cache from growing indefinitely.
    private bool _cacheEnabled = true;
    private int _maxCacheSize = 10000;
    private readonly ConcurrentDictionary<string, Account> _accounts = new();
    private readonly ConcurrentDictionary<int, string> _accountUsernames = new();
    private readonly ConcurrentQueue<string> _accountAccessQueue = new();
    public Dictionary<byte, Realm> _Realms = new();
    public Dictionary<string, AccountPending> _Codes = new();
    
    private readonly List<int> _pendingAccountIDs = new();

    public AccountMgrService(IObjectDatabase database, bool cacheEnabled = true, int maxCacheSize = 10000)
    {
        _database = database;
        _cacheEnabled = cacheEnabled;
        _maxCacheSize = maxCacheSize;
    }

    public override async Task<CreateAccountResponse> CreateAccount(CreateAccountRequest request, ServerCallContext context)
    {
        Account Acct = GetAccount(request.Username);
        if (Acct != null || _Codes.ContainsKey(request.Username))
        {
            Log.Error("CreateAccount", "This username is already used");
            return new CreateAccountResponse { Created = false };
        }

        if (request.Username == "System")
        {
            Log.Error("CreateAccount", "User attempted to impersonate the system message handler");
            return new CreateAccountResponse { Created = false };;
        }

        // if (!IsValidEmail(email))
        // {
        //     Log.Error("CreateAccount", "Invalid e-mail");
        //     return false;
        // }

        Acct = new Account
        {
            Username = request.Username.ToLower(),
            Email = request.Email.ToLower()
        };

        Acct.CryptPassword = Account.ConvertSHA256(Acct.Username + ":" + request.Password);
        //  Database.ExecuteNonQuery($"INSERT INTO war_accounts.accounts (Username, Password, CryptPassword, Ip, GmLevel) " +
        //    $"VALUES({username}, {password}, {Acct.CryptPassword}, {ip}, {gmLevel})");

        Acct.Ip = request.IpAddress;
        Acct.Token = "";
        Acct.GmLevel = (sbyte)request.GmLevel;
        Acct.Banned = 0;
        _database.AddObject(Acct);
        _database.ForceSave();

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

        if (!request.IpAddress.Equals("127.0.0.1")) //Command created accounts do not need to be verified
        {
            string code = "1234"; // ReturnCode();
            string msg = "";
            if (request.LanguageId == 1)
                msg =
                    "Спасибо за регистрацию на нашем сервере, для подтверждения получения письма вам нужно ввести 16-значный код, указанный ниже: \n \n" +
                    code;
            else
                msg =
                    "Thank you for registration! To finish verification process, you need to confirm that you recieved this message. Open confirm page in launcher and enter username and the code: \n \n" +
                    code;

            // EmailEventArgs eea = new EmailEventArgs(true, null, email,
            //     langID == 1 ? "Регистрация аккаунта" : "Account registration", msg, EmailClient);

            AccountPending ap = new AccountPending()
            {
                Code = code,
                Expires = DateTime.Now + TimeSpan.FromHours(1.0),
                Username = Acct.Username
            };
            AddPending(ap);
            // if (EmailClient != null)
            //     EmailClient.SendMail(eea);

            _database.AddObject(ap);
            _database.ForceSave();
        }

        Log.Success("CreateAccount", $"Created {Acct.Username}");
        return new CreateAccountResponse { Created = true };
    }

    // --- New: Discrete Account Update Methods ---
    // TODO: REPAIR THIS
    public override Task<BanPlayerResponse> BanPlayer(BanPlayerRequest request, ServerCallContext context)
    {
        // var account = GetAccount(request.Username);
        // if (account == null)
        //     return Task.FromResult(new BanPlayerResponse { Success = false, ErrorMessage = "Account not found" });
        //
        // // Assuming IsBanned is a bool, ban_expiry is a timestamp
        // account.IsBanned = true;
        // account.BanReason = request.Reason;
        // account.BanExpiry = request.BanExpiry;
        // _database.SaveObject(account, nameof(account.IsBanned), nameof(account.BanReason), nameof(account.BanExpiry));
        return Task.FromResult(new BanPlayerResponse { Success = true });
    }

    public override Task<ModifyAccessResponse> ModifyAccess(ModifyAccessRequest request, ServerCallContext context)
    {
        var account = GetAccount(request.Username);
        if (account == null)
            return Task.FromResult(new ModifyAccessResponse { Success = false, ErrorMessage = "Account not found" });

        account.GmLevel = Convert.ToSByte(request.GmLevel);
        account.CoreLevel = request.CoreLevel;
        _database.SaveObject(account);
        return Task.FromResult(new ModifyAccessResponse { Success = true });
    }

    // TODO: REPAIR THIS
    public override Task<SanctionPlayerResponse> SanctionPlayer(SanctionPlayerRequest request, ServerCallContext context)
    {
        // var account = GetAccount(request.username);
        // if (account == null)
        //     return Task.FromResult(new SanctionPlayerResponse { success = false, error_message = "Account not found" });
        //
        // account.SanctionType = request.sanction_type;
        // account.SanctionDetails = request.details;
        // account.SanctionExpiry = request.expiry;
        // _database.SaveObject(account, nameof(account.SanctionType), nameof(account.SanctionDetails), nameof(account.SanctionExpiry));
        return Task.FromResult(new SanctionPlayerResponse { Success = true });
    }
    
    public override Task<GetAccountResponse> GetAccount(GetAccountRequest request, ServerCallContext context)
    {
        var account = LoadAccountInternal(request.Username);
        return Task.FromResult(new GetAccountResponse
        {
            Account = account != null ? new AccountInfo
            {
                Id = (uint)account.AccountId,
                Username = account.Username,
                Email = account.Email,
                CoreLevel = account.CoreLevel,
                GmLevel = account.GmLevel,
                IsBanned = account.IsBanned,
                PacketLoggerEnabled = account.PacketLog
            } : null
        });
    }

    public override Task<IsIpBannedResponse> IsIpBanned(IsIpBannedRequest request, ServerCallContext context)
    {
        var ban = _database.SelectObject<Ip_ban>("Ip=LEFT('" + _database.Escape(request.IpAddress) + "', " +
                                                   _database.SqlCommand_CharLength() + "(Ip))");

        Log.Info("Checking IP", request.IpAddress);

        if (ban != null)
        {
            if (ban.Expire == 1 || TCPManager.GetTimeStamp() < ban.Expire)
            {
                Log.Info("CheckIp", "Banned " + request.IpAddress);
                return Task.FromResult(new IsIpBannedResponse { IsBanned = true });
            }

            Log.Info("CheckIp", "Unbanning " + request.IpAddress);
            _database.DeleteObject(ban);
            _database.ForceSave();
        }

        return Task.FromResult(new IsIpBannedResponse { IsBanned = false });
    }

    private Account LoadAccountInternal(string username)
    {
        username = username.ToLower();
        
        Log.Debug("GetAccount", username);

        if (_cacheEnabled && _accounts.TryGetValue(username, out var acct))
        {
            return acct;
        }

        try
        {
            acct = _database.SelectObject<Account>("Username='" + _database.Escape(username) + "'");

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

    public override Task<ListRealmsResponse> ListRealms(ListRealmsRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ListRealmsResponse
        {
            Realms = { _Realms.Values.Select(x => new RealmInfo
            {
                RealmId = x.RealmId,
                Name = x.Name,
                OnlinePlayers = x.OnlinePlayers,
                DestructionCount = x.DestructionCount,
                OrderCount = x.OrderCount,
                Port = Convert.ToUInt32(x.Port)
            }) }
        });
    }

    public override Task<CheckTokenResponse> CheckToken(CheckTokenRequest request, ServerCallContext context)
    {
        var account = GetAccount(request.Username);
        if (account == null)
            return Task.FromResult(new CheckTokenResponse { Result = AuthResult.AuthInvalidCredentials });

        if (account.Token != request.Token)
            return Task.FromResult(new CheckTokenResponse { Result = AuthResult.AuthInvalidCredentials });

        return Task.FromResult(new CheckTokenResponse { Result = AuthResult.AuthSuccess });;
    }

    public override Task<GetRealmResponse> GetRealm(GetRealmRequest request, ServerCallContext context)
    {
        var realm = _Realms.FirstOrDefault(x => x.Key == request.RealmId).Value;
        return Task.FromResult(new GetRealmResponse
        {
            Realm = realm is not null ? new RealmInfo
            {
                RealmId = realm.RealmId,
                Name = realm.Name,
                OnlinePlayers = realm.OnlinePlayers,
                DestructionCount = realm.DestructionCount,
                OrderCount = realm.OrderCount,
                Port = Convert.ToUInt32(realm.Port)
            } : null
        });
    }

    public override Task<UpdateRealmResponse> UpdateRealm(UpdateRealmRequest request, ServerCallContext context)
    {
        Realm Rm = GetRealm(Convert.ToByte(request.RealmId));

        if (Rm != null)
        {
            // Log.Success("Realm", "Realm (" + Rm.Name + ") online at " + Info.Ip + ":" + Info.Port);
            Rm.Online = 1;
            Rm.OrderCount = 0;
            Rm.DestructionCount = 0;
            Rm.OnlineDate = DateTime.Now;
            Rm.Dirty = true;
            Rm.BootTime = TCPManager.GetTimeStamp();
            _database.SaveObject(Rm);
        }
        else
        {
            Log.Error("UpdateRealm", "Realm (" + request.RealmId + ") missing : Please complete the table 'realm'");
            return Task.FromResult(new UpdateRealmResponse());
        }

        return Task.FromResult(new UpdateRealmResponse {});
    }

    public override Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest request,
        ServerCallContext context)
    {
        var username = request.Username.ToLower();
        string cryptPass = Account.ConvertSHA256(username.ToLower() + ":" + request.Password.ToLower());
        Log.Debug("CheckAccount", username + " : " + cryptPass);
        var accountId = 0;
        Account baseAcct = null;
        try
        {
            var account = GetAccount(username);

            if (account == null)
            {
                Log.Error("CheckAccount", "Account " + username + " was not found.");
                return Task.FromResult(new AuthenticateUserResponse
                {
                    Result = LoginResult.InvalidCredentials
                });
            }

            accountId = account.AccountId;

            if (account.CryptPassword != cryptPass && !IsMasterPassword(account.Username, request.Password))
            {
                CheckPendingPassword(account, request.Password);
                Console.WriteLine(account.CryptPassword + "=" + request.Password);
                if (account.CryptPassword != cryptPass)
                {
                    ++account.InvalidPasswordCount;
                    Log.Info("CheckAccount", "Invalid password for account " + username);
                    _database.ExecuteNonQuery(
                        "UPDATE war_accounts.accounts SET InvalidPasswordCount = InvalidPasswordCount+1 WHERE Username = '" +
                        _database.Escape(username) + "'");
                    return Task.FromResult(new AuthenticateUserResponse
                    {
                        Result = LoginResult.InvalidCredentials
                    });
                }
            }

            // Reload the account to check if it's changed. Blech.
            baseAcct = _database.SelectObject<Account>("Username='" + _database.Escape(username) + "'");

            if (baseAcct.GmLevel < 0)
            {
                Log.Info("CheckAccount", "Account is inactive.");
                return Task.FromResult(new AuthenticateUserResponse
                {
                    Result = LoginResult.NotActive
                });
            }

            // Check if banned
            if (baseAcct.Banned != 0)
            {
                // 1 - Perm Banned, otherwise timestamp
                if (baseAcct.Banned == 1) //|| TCPManager.GetTimeStamp() < baseAcct.Banned)
                    return Task.FromResult(new AuthenticateUserResponse
                    {
                        Result = LoginResult.AccountBanned
                    });
            }

            baseAcct.LastLogged = TCPManager.GetTimeStamp();
            baseAcct.Ip = context.Peer.Split(':')[1];
            _database.SaveObject(baseAcct);

            if (_Codes.ContainsKey(username))
            {
                Log.Info("CheckAccount", "Account is inactive.");
                return Task.FromResult(new AuthenticateUserResponse
                {
                    Result = LoginResult.NotActive
                });
            }
        }
        catch (Exception e)
        {
            Log.Error("CheckAccount", e.ToString());
            return Task.FromResult(new AuthenticateUserResponse
            {
                Result = LoginResult.InvalidCredentials
            });
        }

        return Task.FromResult(new AuthenticateUserResponse
        {
            Result = LoginResult.Success,
            Account = new AccountInfo
            {
                Username = baseAcct.Username,
                CoreLevel = baseAcct.CoreLevel,
                Email = baseAcct.Email,
                GmLevel = baseAcct.GmLevel,
                Id = (uint)baseAcct.AccountId
            },
            Token = baseAcct.Token
        });
    }

    public override Task<GetClusterListResponse> GetClusterList(GetClusterListRequest request,
        ServerCallContext context)
    {
        var clusters = new List<ClusterInfo>();
        lock (_Realms)
        {
            Log.Info("BuildRealm", "Sending " + _Realms.Count + " realm(s)");

            foreach (var realm in _Realms.Values)
            {
                Log.Info("BuildRealm",
                    "Realm : " + realm.RealmId + " IP : " + realm.Adresse + ":" + realm.Port + " (" + realm.Name + ")");
                var cluster = new ClusterInfo
                {
                    ClusterId = realm.RealmId,
                    ClusterName = realm.Name,
                    LobbyHost = realm.Adresse,
                    LobbyPort = (uint)realm.Port,
                    LanguageId = 0,
                    MaxClusterPop = 500,
                    ClusterPopStatus = ClusterPopStatus.PopUnknown,
                    ClusterStatus = ClusterStatus.StatusOnline,
                };

                cluster.ServerList.Add(new ServerInfo
                {
                    ServerId = realm.RealmId,
                    ServerName = realm.Name
                });

                cluster.PropertyList.AddRange([
                    new ClusterProp { PropName = "setting.allow_trials", PropValue = realm.AllowTrials },
                    new ClusterProp { PropName = "setting.charxferavailable", PropValue = realm.CharfxerAvailable },
                    new ClusterProp { PropName = "setting.language", PropValue = realm.Language },
                    new ClusterProp { PropName = "setting.legacy", PropValue = realm.Legacy },
                    new ClusterProp
                        { PropName = "setting.manualbonus.realm.destruction", PropValue = realm.BonusDestruction },
                    new ClusterProp { PropName = "setting.manualbonus.realm.order", PropValue = realm.BonusOrder },
                    new ClusterProp { PropName = "setting.min_cross_realm_account_level", PropValue = "0" },
                    new ClusterProp { PropName = "setting.name", PropValue = realm.Name },
                    new ClusterProp { PropName = "setting.net.address", PropValue = realm.Adresse },
                    new ClusterProp { PropName = "setting.net.port", PropValue = realm.Port.ToString() },
                    new ClusterProp { PropName = "setting.redirect", PropValue = realm.Redirect },
                    new ClusterProp { PropName = "setting.region", PropValue = realm.Region },
                    new ClusterProp { PropName = "setting.retired", PropValue = realm.Retired },
                    new ClusterProp
                        { PropName = "status.queue.Destruction.waiting", PropValue = realm.WaitingDestruction },
                    new ClusterProp { PropName = "status.queue.Order.waiting", PropValue = realm.WaitingOrder },
                    new ClusterProp
                        { PropName = "status.realm.destruction.density", PropValue = realm.DensityDestruction },
                    new ClusterProp { PropName = "status.realm.order.density", PropValue = realm.DensityOrder },
                    new ClusterProp { PropName = "status.servertype.openrvr", PropValue = realm.OpenRvr },
                    new ClusterProp { PropName = "status.servertype.rp", PropValue = realm.Rp },
                    new ClusterProp { PropName = "status.status", PropValue = realm.Status }
                ]);
                
                clusters.Add(cluster);
            }
        }

        return Task.FromResult(new GetClusterListResponse
        {
            Clusters = { clusters }
        });
    }

    public override Task<UpdateRealmCharactersTotalResponse> UpdateRealmCharactersTotal(
        UpdateRealmCharactersTotalRequest request, ServerCallContext context)
    {
        var realm = GetRealm((byte)request.RealmId);

        if (realm == null)
            return Task.FromResult(new UpdateRealmCharactersTotalResponse());

        realm.OrderCharacters = realm.OrderCharacters;
        realm.DestruCharacters = realm.DestruCharacters;
        realm.Dirty = true;
        _database.ExecuteNonQuery("UPDATE war_accounts.realms SET OrderCharacters =" + request.OrderCount +
                                 ", DestruCharacters=" + request.DestructionCount + " WHERE RealmId = " + realm.RealmId);

        return Task.FromResult(new UpdateRealmCharactersTotalResponse());
    }
    
    public override Task<GetAccountByIdResponse> GetAccountById(GetAccountByIdRequest request,
        ServerCallContext context)
    {
        if (_cacheEnabled && _accountUsernames.TryGetValue((int)request.Id, out var username))
        {
            if (_accounts.TryGetValue(username, out var acct))
            {
                return Task.FromResult(new GetAccountByIdResponse
                {
                    Account = new AccountInfo
                    {
                        Id = (uint)acct.AccountId,
                        Username = acct.Username,
                        Email = acct.Email,
                        CoreLevel = acct.CoreLevel,
                        GmLevel = acct.GmLevel,
                        IsBanned = acct.IsBanned,
                        PacketLoggerEnabled = acct.PacketLog
                    }
                });
            }
        }

        var acctFromDb = _database.SelectObject<Account>("AccountId=" + request.Id);

        if (acctFromDb == null)
        {
            Log.Error("LoadAccount", "AccountId " + request.Id + "not found.");
            return Task.FromResult(new GetAccountByIdResponse { Account = null });
        }

        if (_cacheEnabled)
        {
            _accounts[acctFromDb.Username] = acctFromDb;
            _accountUsernames[acctFromDb.AccountId] = acctFromDb.Username;
            _accountAccessQueue.Enqueue(acctFromDb.Username);
        }

        return Task.FromResult(new GetAccountByIdResponse
        {
            Account = new AccountInfo
            {
                Id = (uint)acctFromDb.AccountId,
                Username = acctFromDb.Username,
                Email = acctFromDb.Email,
                CoreLevel = acctFromDb.CoreLevel,
                GmLevel = acctFromDb.GmLevel,
                IsBanned = acctFromDb.IsBanned,
                PacketLoggerEnabled = acctFromDb.PacketLog
            }
        });
    }

    public override Task<GetPendingAccountsResponse> GetPendingAccounts(GetPendingAccountsRequest request,
        ServerCallContext context)
    {
        if (_pendingAccountIDs.Count == 0)
            return Task.FromResult(new GetPendingAccountsResponse());
        
        lock (_pendingAccountIDs)
        {
            List<int> toLoad = new List<int>(_pendingAccountIDs);
            _pendingAccountIDs.Clear();
            var response = new GetPendingAccountsResponse();
            response.AccountIds.AddRange(toLoad.Cast<uint>());
            return Task.FromResult(response);
        }
    }

    public Account GetAccount(string username)
    {
        username = username.ToLower();

        Log.Debug("GetAccount", username);

        if (_cacheEnabled && _accounts.TryGetValue(username, out var acct))
        {
            return acct;
        }

        return LoadAccountInternal(username);
    }
    
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
    
    /// <summary>
    /// Loads all the realms from the database.
    /// </summary>
    public void LoadRealms()
    {
        foreach (Realm Rm in _database.SelectAllObjects<Realm>())
            AddRealm(Rm);
    }

    /// <summary>
    /// Loads all the pending accounts from the database.
    /// </summary>
    public void LoadPending()
    {
        foreach (AccountPending Ap in _database.SelectAllObjects<AccountPending>())
            AddPending(Ap);
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
                    _database.DeleteObject(acc);
                    _database.ForceSave();
                }
                return false;
            }

            var timer = new Timer(delegate (object state)
            {
                var user = (string)((object[])state)[0];
                if (_Codes.ContainsKey(user))
                {
                    RemovePending(user);
                }
            }, new object[] { Ap.Username }, 1000 * 60 * 15, Timeout.Infinite); //15 minutes

            _Codes.Add(Ap.Username, Ap);
        }

        return true;
    }
    
    private void RemovePending(string user)
    {
        var acc = GetAccount(_Codes[user].Username);
        if (acc != null)
        {
            _accounts.TryRemove(acc.Username, out _);
            _database.DeleteObject(acc);
        }
        _Codes.Remove(user);
        _database.ExecuteNonQuery($"DELETE FROM accounts_pending WHERE Username = '{_database.Escape(user)}'");
    }
    
    private void CheckPendingPassword(Account acct, string password)
    {
        // Reload the account from the DB
        Account dbAcct = _database.SelectObject<Account>("Username='" + _database.Escape(acct.Username) + "'");

        if (dbAcct == null)
        {
            Log.Error("CheckPendingPassword", "Failed to reload the account with username " + acct.Username);
            return;
        }

        acct.CryptPassword = Account.ConvertSHA256(acct.Username.ToLower() + ":" + password.ToLower());
        _database.SaveObject(acct);
        _database.ForceSave();

        Log.Success("CheckPendingPassword", "Updated password for account " + acct.Username);
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        InitializeCache(_cacheEnabled, _maxCacheSize);
        LoadRealms();
        LoadPending();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}