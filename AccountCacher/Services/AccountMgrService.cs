using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using FrameWork;
using Google.Protobuf;
using Grpc.Core;

namespace AccountCacher.Services;

public class AccountMgrService : AccountMgr.AccountMgrBase
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

    public AccountMgrService(IObjectDatabase database)
    {
        _database = database;
    }
    
    public override Task<LoadAccountResponse> LoadAccount(LoadAccountRequest request, ServerCallContext context)
    {
        LoadAccountInternal(request.Username);
        return Task.FromResult(new LoadAccountResponse());
    }
    
    private Account LoadAccountInternal(string username)
    {
        username = username.ToLower();

        try
        {
            Account acct = _database.SelectObject<Account>("Username='" + _database.Escape(username) + "'");

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
                Name = x.Name,
                OnlinePlayers = x.OnlinePlayers,
                DestructionCount = x.DestructionCount,
                OrderCount = x.OrderCount
            }) }
        });
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
                Id = baseAcct.AccountId.ToString()
            },
            Token = baseAcct.Token
        });
    }

    public override Task<GetClusterListResponse> GetClusterList(GetClusterListRequest request, ServerCallContext context)
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
            return Task.FromResult(new GetClusterListResponse
            {
                Clusters = ByteString.CopyFrom(ClusterListReplay.Build().ToByteArray())
            });
    }
    
    private ClusterProp setProp(string name, string value)
    {
        return ClusterProp.CreateBuilder().SetPropName(name)
            .SetPropValue(value)
            .Build();
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
}