using NLog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Infrastructure.Network;
using Launcher.NetWork;
using Launcher.Resources;
using Launcher.Services;
using LauncherServer.Dtos;

namespace Launcher;

public partial class ApocLauncher : Form
{
    public bool LaunchLocalServer { get; }
    public bool AllowMYPPatch { get; }
    public bool AllowServerPatch { get; }
    public bool AllowWarClientLaunch { get; }
    public static ApocLauncher Acc;

    public static string LocalServerIP = "127.0.0.1";
    public static string TestServerIP = "127.0.0.1";
    public static int LocalServerPort = 8000;
    public static int TestServerPort = 8000;
    private Patcher patcher;

    private readonly LauncherService _launcherService;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public ApocLauncher()
    {
        // Read optional app settings (they may not exist in the app.config file)
        AllowWarClientLaunch = SafeReadAppSettings("AutoLaunch", true);
        AllowMYPPatch = SafeReadAppSettings("PatchMYP", true);
        AllowServerPatch = SafeReadAppSettings("PatchExe", true);
        LaunchLocalServer = SafeReadAppSettings("LaunchLocal", false);

        // Initialize launcher services
        var serializerContext = new LauncherSerializerContext();
        var serializerFactory = new BinaryPacketSerializerFactory(serializerContext);
            
        _launcherService = new LauncherService(TestServerIP, TestServerPort, serializerFactory);
        _launcherService.Disconnected += OnDisconnected;

        InitializeComponent();
        Acc = this;

        if (LaunchLocalServer)
        {
            bnConnectLocal.Visible = true;
            bnCreateLocal.Visible = true;
        }
        else
        {
            bnConnectLocal.Visible = false;
            bnCreateLocal.Visible = false;
        }
    }

    private static bool SafeReadAppSettings(string keyName, bool defaultValue)
    {
        var s = ConfigurationManager.AppSettings[keyName];

        return s switch
        {
            // Key exists
            "false" => false,
            "true" => true,
            _ => defaultValue
        };
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == WM_NCHITTEST)
            m.Result = HT_CAPTION;
    }

    private const int WM_NCHITTEST = 0x84;
    private const int HT_CLIENT = 0x1;
    private const int HT_CAPTION = 0x2;

    private void Form1_Load(object sender, EventArgs e)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        var attrs = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        lblVersion.Text = fvi.FileVersion;
        //this.lblVersion.Text = $"{fvi.FileVersion} ({attrs.Single(x => x.Key == "GitHash").Value})";

        lblDownloading.Visible = false;
        if (AllowMYPPatch)
        {
            _logger.Debug($"Calling Patcher Server on { ConfigurationManager.AppSettings["ServerPatchIPAddress"]}:{ ConfigurationManager.AppSettings["ServerPatchPort"]}");
            patcher = new Patcher(_logger,
                $"{ConfigurationManager.AppSettings["ServerPatchIPAddress"]}:{ConfigurationManager.AppSettings["ServerPatchPort"]}");

            lblDownloading.Visible = true;

            var patchDirectory = ConfigurationManager.AppSettings["PatchDirectory"];

            var thread = new Thread(() => patcher.Patch(patchDirectory).Wait()) { IsBackground = true };
            thread.Start();
        }

        T_username.Text = ConfigurationManager.AppSettings["LastUserCode"];
    }

    private void Disconnect(object sender, FormClosedEventArgs e)
    {
        _launcherService?.Dispose();
    }

    private void OnDisconnected(DisconnectReason reason)
    {
        _logger.Warn($"Disconnected from launcher server: {reason}");
            
        if (InvokeRequired)
        {
            BeginInvoke(new Action<DisconnectReason>(OnDisconnected), reason);
            return;
        }
            
        lblConnection.Text = "Disconnected";
    }

    //private void B_start_Click(object sender, EventArgs e)
    //{
    //    Client.Connect(LocalServerIP, LocalServerPort);

    //    lblConnection.Text = $@"Connecting to : {LocalServerIP}:{LocalServerPort}";

    //    string userCode = T_username.Text.ToLower();
    //    string userPassword = T_password.Text.ToLower();

    //    Client.User = userCode;

    //    string encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

    //    _logger.Info($@"Connecting to : {LocalServerIP}:{LocalServerPort} as {userCode} [{encryptedPassword}]");
    //    _logger.Info($"Sending CL_START to {LocalServerIP}:{LocalServerPort}");

    //    PacketOut Out = new PacketOut((byte)Opcodes.CL_START);
    //    Out.WriteString(userCode);
    //    Out.WriteString(encryptedPassword);

    //    Client.SendTCP(Out);
    //    //B_start.Enabled = false;
    //}

    private static string ConvertSHA256(string value)
    {
        var data = SHA256.HashData(Encoding.Default.GetBytes(value));
        var sb = new StringBuilder();
        foreach (var t in data)
        {
            sb.Append(t.ToString("x2"));
        }
        return sb.ToString();
    }

    public void ReceiveStart()
    {
        //B_start.Enabled = true;
    }

    public void Print(string Message)
    {
    }

    private async void bnConnectToServer_Click(object sender, EventArgs e)
    {
        try
        {
            lblConnection.Text = $@"Connecting to : {TestServerIP}:{TestServerPort}";

            var userCode = T_username.Text.ToLower();
            var userPassword = T_password.Text.ToLower();
            var encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

            _logger.Info($@"Connecting to : {TestServerIP}:{TestServerPort} as {userCode}");

            try
            {
                // Get connected proxy and call the RPC method with full type safety
                var proxy = await _launcherService.OpenConnectionAsync();
                var response = await proxy.CL_START(new StartRequest
                {
                    Username = userCode,
                    PasswordHash = encryptedPassword
                });

                HandleStartResponse(response, userCode);

                // Save username to config
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (configuration.AppSettings.Settings["LastUserCode"] == null)
                {
                    configuration.AppSettings.Settings.Add("LastUserCode", T_username.Text);
                }
                else
                {
                    configuration.AppSettings.Settings["LastUserCode"].Value = T_username.Text;
                }
                configuration.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Login failed");
                lblConnection.Text = Strings.Message_ConnectionFailed;
            }
        }
        catch (Exception ex)
        {
            throw; // TODO handle exception
        }
    }

    private void HandleStartResponse(StartResponse response, string username)
    {
        switch (response.Result)
        {
            case LoginResult.Success:
                _logger.Info($"Authentication successful for {username}");
                lblConnection.Text = Strings.Message_AuthenticatedStartingGame;
                LaunchGame(username, response.AuthToken);
                break;

            case LoginResult.InvalidCredentials:
                _logger.Warn(Strings.Message_InvalidUsernamePassword);
                lblConnection.Text = Strings.Message_InvalidUsernamePassword;
                MessageBox.Show("Invalid username or password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;

            case LoginResult.AccountBanned:
                _logger.Warn("Account is banned");
                lblConnection.Text = "Account is banned";
                MessageBox.Show("Your account has been banned", "Account Banned", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                break;

            case LoginResult.NotActive:
                _logger.Warn("Account is not active");
                lblConnection.Text = "Account not active";
                MessageBox.Show("Your account is not active", "Account Inactive", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;

            default:
                _logger.Error($"Unknown login response: {response.Result}");
                lblConnection.Text = "Unknown error";
                MessageBox.Show("An unknown error occurred", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
        }
    }

    private void LaunchGame(string username, string authToken)
    {
        try
        {
            var warDirectory = Directory.GetParent(Application.StartupPath.TrimEnd('\\'));
            lblConnection.Text = "Patching..";
                
            // Patching operations would go here
            // patchExe();
            // UpdateWarData();
                
            lblConnection.Text = "Starting WAR.exe";

            if (!File.Exists(Application.StartupPath + "\\mythloginserviceconfig.xml"))
            {
                _logger.Warn("mythloginserviceconfig.xml does not exist");
                lblConnection.Text = "Cannot locate mythloginserviceconfig.xml";
                return;
            }

            if (!File.Exists(warDirectory.FullName + "\\world.myp"))
            {
                _logger.Warn("world.myp does not exist");
                lblConnection.Text = "Is your launcher in the Launcher folder?";
                return;
            }

            if (AllowWarClientLaunch)
            {
                var process = new Process()
                {
                    StartInfo =
                    {
                        WorkingDirectory = warDirectory.FullName,
                        FileName = "WAR.exe",
                        Arguments = $" --acctname={Convert.ToBase64String(Encoding.ASCII.GetBytes(username))} --sesstoken={Convert.ToBase64String(Encoding.ASCII.GetBytes(authToken))}",
                        UseShellExecute = true
                    }
                };
                _logger.Info($"Starting WAR.exe in {warDirectory}");
                process.Start();
                Directory.SetCurrentDirectory(warDirectory.FullName);
            }
            else
            {
                _logger.Info($"Not launching WAR.exe (disabled in config)");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to start game client");
            lblConnection.Text = "Failed to start client";
            MessageBox.Show($"Failed to start the game: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void bnClose_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void buttonPanelCreateAccount_Click(object sender, EventArgs e)
    {
        panelCreateAccount.Visible = true;
    }

    /// <summary>
    /// Create new user account.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void buttonCreate_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(textBoxUsername.Text) || string.IsNullOrEmpty(textBoxPassword.Text)) return;

            lblConnection.Text = $@"Connecting to : {TestServerIP}:{TestServerPort}";

            var userCode = textBoxUsername.Text.ToLower();
            var userPassword = textBoxPassword.Text.ToLower();

            _logger.Info($@"Creating account: {userCode}");

            try
            {
                using var proxy = await _launcherService.OpenConnectionAsync();
                var response = await proxy.CL_CREATE(new CreateAccountRequest
                {
                    Username = userCode,
                    Password = userPassword,
                    Email = "",
                    LangID = 1
                });

                HandleCreateAccountResponse(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Account creation failed");
                lblConnection.Text = "Account creation failed";
                MessageBox.Show($"Failed to create account: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            throw; // TODO handle exception
        }
    }

    private void HandleCreateAccountResponse(CreateAccountResponse response)
    {
        switch (response.Status)
        {
            case CreateAccountResult.ACCOUNT_NAME_SUCCESS:
                _logger.Info("Account created successfully");
                lblConnection.Text = "Account created!";
                MessageBox.Show("Account created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                panelCreateAccount.Visible = false;
                break;

            case CreateAccountResult.ACCOUNT_NAME_BUSY:
                _logger.Warn("Account name already in use");
                lblConnection.Text = "Account name busy";
                MessageBox.Show("Account name is already in use", "Account Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                break;

            case CreateAccountResult.ACCOUNT_BANNED:
                _logger.Warn("IP banned");
                lblConnection.Text = "Account banned";
                MessageBox.Show("Your IP address is banned", "Banned", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                break;

            default:
                _logger.Error($"Unknown create account response: {response.Status}");
                lblConnection.Text = "Unknown error";
                break;
        }
    }

    private void buttonAccountClose_Click(object sender, EventArgs e)
    {
        panelCreateAccount.Visible = false;
    }

    public void sendUI(string msg)
    {
        if (lblConnection.InvokeRequired)
        {
            lblConnection.BeginInvoke(new Action(() =>
            {
                sendUI(msg);
            }));
            return;
        }

        lblConnection.Text = msg;
    }

    private async void bnCreateLocal_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(textBoxUsername.Text) || string.IsNullOrEmpty(textBoxPassword.Text)) return;

            lblConnection.Text = $@"Connecting to : {LocalServerIP}:{LocalServerPort}";

            var userCode = textBoxUsername.Text.ToLower();
            var userPassword = textBoxPassword.Text.ToLower();

            _logger.Info($@"Creating local account: {userCode}");

            try
            {
                var proxy = await _launcherService.OpenConnectionAsync();
                var response = await proxy.CL_CREATE(new CreateAccountRequest
                {
                    Username = userCode,
                    Password = userPassword,
                    Email = "",
                    LangID = 1
                });

                HandleCreateAccountResponse(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Local account creation failed");
                lblConnection.Text = "Account creation failed";
                MessageBox.Show($"Failed to create account: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            throw; // TODO handle exception
        }
    }

    private async void bnConnectToLocal_Click(object sender, EventArgs e)
    {
        lblConnection.Text = $@"Connecting to : {LocalServerIP}:{LocalServerPort}";

        var userCode = T_username.Text.ToLower();
        var userPassword = T_password.Text.ToLower();
        var encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

        _logger.Info($@"Connecting to local server as {userCode}");

        try
        {
            var proxy = await _launcherService.OpenConnectionAsync();
            var response = await proxy.CL_START(new StartRequest
            {
                Username = userCode,
                PasswordHash = encryptedPassword
            });

            HandleStartResponse(response, userCode);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Local login failed");
            lblConnection.Text = Strings.Message_ConnectionFailed;
            MessageBox.Show($"Failed to connect: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (AllowMYPPatch)
        {
            if (patcher.CurrentState == Patcher.State.Downloading)
            {
                bnConnectToServer.Enabled = false;

                long percent = 0;
                if (patcher.TotalDownloadSize > 0)
                    percent = (patcher.Downloaded * 100) / patcher.TotalDownloadSize;

                lblDownloading.Text = $"Downloading {patcher.CurrentFile} ({percent}%)";
            }
            else if (patcher.CurrentState == Patcher.State.RequestManifest)
            {
                bnConnectToServer.Enabled = false;
                lblDownloading.Text = $"Looking for updates..";
            }
            else if (patcher.CurrentState == Patcher.State.ProcessManifest)
            {
                bnConnectToServer.Enabled = false;
                lblDownloading.Text = $"Processing updates..";
            }
            else if (patcher.CurrentState == Patcher.State.Done || patcher.CurrentState == Patcher.State.Error)
            {
                bnConnectToServer.Enabled = true;
                lblDownloading.Text = "";
            }
        }
    }

    private void T_username_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            bnConnectToServer_Click(this, EventArgs.Empty);
        }
    }

    private void T_password_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            bnConnectToServer_Click(this, EventArgs.Empty);
        }
    }

    private void bnMinimise_Click(object sender, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
    }

    private async void bnServerBrowser_Click(object sender, EventArgs e)
    {
        panelServerBrowser.Visible = !panelServerBrowser.Visible;
        
        if (panelServerBrowser.Visible)
        {
            await RefreshServerList();
            timerServerRefresh.Start();
        }
        else
        {
            timerServerRefresh.Stop();
        }
    }

    private void buttonServerBrowserClose_Click(object sender, EventArgs e)
    {
        panelServerBrowser.Visible = false;
        timerServerRefresh.Stop();
    }

    private async void timerServerRefresh_Tick(object sender, EventArgs e)
    {
        await RefreshServerList();
    }

    private async Task RefreshServerList()
    {
        try
        {
            var response = await _launcherService.ExecuteAsync(
                async proxy => await proxy.CL_INFO(new GetInfoRequest())
            );

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => PopulateServerList(response)));
            }
            else
            {
                PopulateServerList(response);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to refresh server list");
            
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => lblConnection.Text = "Failed to load server list"));
            }
            else
            {
                lblConnection.Text = "Failed to load server list";
            }
        }
    }

    private void PopulateServerList(GetInfoResponse response)
    {
        listViewServers.Items.Clear();

        if (response?.RealmInfo == null || response.RealmInfo.Count == 0)
        {
            var item = new ListViewItem("N/A");
            item.SubItems.Add("No servers available");
            item.SubItems.Add("0");
            item.SubItems.Add("0 / 0");
            listViewServers.Items.Add(item);
            return;
        }

        foreach (var realm in response.RealmInfo)
        {
            var item = new ListViewItem(realm.Online ? "Online" : "Offline");
            item.SubItems.Add(realm.Name);
            item.SubItems.Add(realm.OnlinePlayers.ToString());
            item.SubItems.Add($"{realm.OrderCount} / {realm.DestructionCount}");
            
            // Color code based on online status
            if (realm.Online)
            {
                item.ForeColor = Color.LimeGreen;
            }
            else
            {
                item.ForeColor = Color.Gray;
            }
            
            listViewServers.Items.Add(item);
        }
    }
}