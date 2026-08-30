using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ServerLauncher
{
    public partial class Form1 : Form
    {
        public Process AccountCacher;
        public Process LauncherServer;
        public Process LobbyServer;
        public Process WorldServer;

        private CancellationTokenSource _startupCancellation;

        private static readonly TimeSpan DependencyReadyTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan ServiceReadyTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan WorldServerStableTimeout = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan AccountCacherWarmupWindow = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan StabilityWindow = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(250);

        public Form1()
        {
            InitializeComponent();
        }

        private async void B_start_Click(object sender, EventArgs e)
        {
            if (_startupCancellation != null)
                return;

            _startupCancellation = new CancellationTokenSource();
            SetControlsEnabled(false);
            UpdateStatus("Starting selected services...");

            try
            {
                await StartSelectedServersAsync(_startupCancellation.Token);
                UpdateStatus("Startup sequence completed.");
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Startup canceled.");
            }
            catch (Exception ex)
            {
                UpdateStatus(ex.Message);
            }
            finally
            {
                _startupCancellation.Dispose();
                _startupCancellation = null;
                SetControlsEnabled(true);
            }
        }

        private async void B_stop_Click(object sender, EventArgs e)
        {
            if (_startupCancellation != null)
                _startupCancellation.Cancel();

            // Stopping is now a graceful close with a timeout per service, so it can take a few seconds.
            // Run it off the UI thread and hold the buttons so the window stays responsive meanwhile.
            B_stop.Enabled = false;
            SetControlsEnabled(false);
            UpdateStatus("Stopping services...");

            try
            {
                // Order matters and must stay sequential: WorldServer's shutdown writes a realm population
                // snapshot over RPC, so AccountCacher has to still be running when WorldServer goes down.
                await Task.Run(() =>
                {
                    StopTrackedProcess(WorldServer);
                    StopTrackedProcess(LobbyServer);
                    StopTrackedProcess(LauncherServer);
                    StopTrackedProcess(AccountCacher);
                });

                UpdateStatus("All tracked services stopped.");
            }
            finally
            {
                B_stop.Enabled = true;
                SetControlsEnabled(true);
            }
        }

        private async Task StartSelectedServersAsync(CancellationToken cancellationToken)
        {
            List<string> failures = new List<string>();
            bool anyDependentSelected = StartLauncherCheckBox.Checked || StartLobbyCheckBox.Checked || StartWorldCheckBox.Checked;
            bool accountReady = true;

            if (StartAccountCheckBox.Checked)
                accountReady = await EnsureAccountCacherReadyAsync(true, cancellationToken);
            else if (anyDependentSelected)
                accountReady = await EnsureAccountCacherReadyAsync(false, cancellationToken);

            if (!accountReady)
                throw new InvalidOperationException("AccountCacher RPC is not ready. Dependent services were not started.");

            if (StartLauncherCheckBox.Checked)
            {
                string launcherFailure = await TryStartLauncherServerAsync(cancellationToken);
                if (!string.IsNullOrEmpty(launcherFailure))
                    failures.Add(launcherFailure);
            }

            if (StartLobbyCheckBox.Checked)
            {
                string lobbyFailure = await TryStartLobbyServerAsync(cancellationToken);
                if (!string.IsNullOrEmpty(lobbyFailure))
                    failures.Add(lobbyFailure);
            }

            if (StartWorldCheckBox.Checked)
            {
                string worldFailure = await TryStartWorldServerAsync(cancellationToken);
                if (!string.IsNullOrEmpty(worldFailure))
                    failures.Add(worldFailure);
            }

            if (failures.Count > 0)
                throw new InvalidOperationException(string.Join(" ", failures));
        }

        private async Task<bool> EnsureAccountCacherReadyAsync(bool shouldStart, CancellationToken cancellationToken)
        {
            string accountConfigPath = GetConfigPath("Account.xml");
            string host = NormalizeHost(GetConfigValue(accountConfigPath, "RpcInfo", "RpcIp"));
            int port = GetConfigIntValue(accountConfigPath, "RpcInfo", "RpcPort");

            if (!shouldStart)
            {
                UpdateStatus($"Waiting for existing AccountCacher RPC on {host}:{port}...");
                return await WaitForPortReadyAsync(host, port, null, DependencyReadyTimeout, cancellationToken);
            }

            return await StartServiceAndWaitForPortAsync(
                "AccountCacher",
                "AccountCacher.exe",
                host,
                port,
                process => AccountCacher = process,
                () => AccountCacher,
                ServiceReadyTimeout,
                AccountCacherWarmupWindow,
                cancellationToken);
        }

        private async Task<string> TryStartLauncherServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                string configPath = GetConfigPath("Launcher.xml");
                string host = NormalizeHost(GetConfigValue(configPath, "RpcInfo", "RpcServerIp"));
                int dependencyPort = GetConfigIntValue(configPath, "RpcInfo", "RpcServerPort");
                int launcherPort = GetConfigIntValue(configPath, "LauncherServerPort");

                UpdateStatus($"Waiting for AccountCacher RPC on {host}:{dependencyPort} before LauncherServer...");
                if (!await WaitForPortReadyAsync(host, dependencyPort, AccountCacher, DependencyReadyTimeout, cancellationToken))
                    return $"LauncherServer skipped because AccountCacher RPC {host}:{dependencyPort} is unavailable.";

                bool ready = await StartServiceAndWaitForPortAsync(
                    "LauncherServer",
                    "LauncherServer.exe",
                    "127.0.0.1",
                    launcherPort,
                    process => LauncherServer = process,
                    () => LauncherServer,
                    ServiceReadyTimeout,
                    StabilityWindow,
                    cancellationToken);

                return ready ? null : $"LauncherServer failed to become ready on 127.0.0.1:{launcherPort}.";
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return "LauncherServer failed: " + ex.Message;
            }
        }

        private async Task<string> TryStartLobbyServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                string configPath = GetConfigPath("Lobby.xml");
                string host = NormalizeHost(GetConfigValue(configPath, "RpcInfo", "RpcServerIp"));
                int dependencyPort = GetConfigIntValue(configPath, "RpcInfo", "RpcServerPort");
                int lobbyPort = GetConfigIntValue(configPath, "ClientPort");

                UpdateStatus($"Waiting for AccountCacher RPC on {host}:{dependencyPort} before LobbyServer...");
                if (!await WaitForPortReadyAsync(host, dependencyPort, AccountCacher, DependencyReadyTimeout, cancellationToken))
                    return $"LobbyServer skipped because AccountCacher RPC {host}:{dependencyPort} is unavailable.";

                bool ready = await StartServiceAndWaitForPortAsync(
                    "LobbyServer",
                    "LobbyServer.exe",
                    "127.0.0.1",
                    lobbyPort,
                    process => LobbyServer = process,
                    () => LobbyServer,
                    ServiceReadyTimeout,
                    StabilityWindow,
                    cancellationToken);

                return ready ? null : $"LobbyServer failed to become ready on 127.0.0.1:{lobbyPort}.";
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return "LobbyServer failed: " + ex.Message;
            }
        }

        private async Task<string> TryStartWorldServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                string configPath = GetConfigPath("World.xml");
                string host = NormalizeHost(GetConfigValue(configPath, "AccountCacherInfo", "RpcServerIp"));
                int dependencyPort = GetConfigIntValue(configPath, "AccountCacherInfo", "RpcServerPort");

                UpdateStatus($"Waiting for AccountCacher RPC on {host}:{dependencyPort} before WorldServer...");
                if (!await WaitForPortReadyAsync(host, dependencyPort, AccountCacher, DependencyReadyTimeout, cancellationToken))
                    return $"WorldServer skipped because AccountCacher RPC {host}:{dependencyPort} is unavailable.";

                Process worldProcess = EnsureTrackedProcess("WorldServer", "WorldServer.exe", () => WorldServer, process => WorldServer = process);

                UpdateStatus($"Waiting for WorldServer to stabilize ({WorldServerStableTimeout.TotalSeconds:0}s)...");
                bool stable = await WaitForProcessStabilityAsync(worldProcess, WorldServerStableTimeout, cancellationToken);
                if (stable)
                    UpdateStatus("WorldServer is running.");

                return stable ? null : "WorldServer exited before completing its startup stabilization window.";
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return "WorldServer failed: " + ex.Message;
            }
        }

        private async Task<bool> StartServiceAndWaitForPortAsync(string serviceName, string executableName, string host, int port, Action<Process> processSetter, Func<Process> processGetter, TimeSpan timeout, TimeSpan stabilityWindow, CancellationToken cancellationToken)
        {
            EnsureTrackedProcess(serviceName, executableName, processGetter, processSetter);

            UpdateStatus($"Waiting for {serviceName} on {host}:{port}...");
            bool ready = await WaitForPortReadyAsync(host, port, processGetter(), timeout, cancellationToken);
            if (!ready)
                return false;

            bool stable = await WaitForProcessStabilityAsync(processGetter(), stabilityWindow, cancellationToken);
            if (!stable)
                return false;

            UpdateStatus($"{serviceName} is ready on {host}:{port}.");
            return true;
        }

        private Process EnsureTrackedProcess(string serviceName, string executableName, Func<Process> processGetter, Action<Process> processSetter)
        {
            Process trackedProcess = processGetter();
            if (IsProcessRunning(trackedProcess))
                return trackedProcess;

            Process existingProcess = FindExistingProcess(executableName);
            if (existingProcess != null)
            {
                processSetter(existingProcess);
                UpdateStatus($"{serviceName} is already running.");
                return existingProcess;
            }

            string executablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, executableName);
            if (!File.Exists(executablePath))
                throw new FileNotFoundException("Missing executable: " + executablePath);

            Process process = new Process();
            process.StartInfo.FileName = executablePath;
            process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.Start();

            processSetter(process);
            UpdateStatus($"{serviceName} started.");
            return process;
        }

        private async Task<bool> WaitForPortReadyAsync(string host, int port, Process process, TimeSpan timeout, CancellationToken cancellationToken)
        {
            DateTime deadline = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (process != null && !IsProcessRunning(process))
                    return false;

                if (await CanConnectAsync(host, port, cancellationToken))
                    return true;

                await Task.Delay(PollInterval, cancellationToken);
            }

            return false;
        }

        private async Task<bool> WaitForProcessStabilityAsync(Process process, TimeSpan duration, CancellationToken cancellationToken)
        {
            if (process == null)
                return false;

            DateTime deadline = DateTime.UtcNow.Add(duration);
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsProcessRunning(process))
                    return false;

                await Task.Delay(PollInterval, cancellationToken);
            }

            return true;
        }

        private static async Task<bool> CanConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            using (TcpClient client = new TcpClient())
            {
                Task connectTask = client.ConnectAsync(host, port);
                Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                Task completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask != connectTask)
                    return false;

                try
                {
                    await connectTask;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static Process FindExistingProcess(string executableName)
        {
            string processName = Path.GetFileNameWithoutExtension(executableName);
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }

        private static bool IsProcessRunning(Process process)
        {
            if (process == null)
                return false;

            try
            {
                return !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// How long a service gets to shut itself down before we terminate it. WorldServer uses this
        /// window to flush campaign progression and every dirty character to the database.
        /// </summary>
        private static readonly TimeSpan GracefulStopTimeout = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Stops a service by closing it rather than killing it. Process.Kill is TerminateProcess: it
        /// delivers no signal, so WorldServer's shutdown handler never ran and campaign state plus up to
        /// a save-pump interval of character writes were dropped on every stop. CloseMainWindow posts
        /// WM_CLOSE to the console window, which the service receives as CTRL_CLOSE_EVENT. Kill remains
        /// the fallback for a service that is wedged or has no window to close.
        /// </summary>
        private static void StopTrackedProcess(Process process)
        {
            try
            {
                if (process == null || process.HasExited)
                    return;

                bool closeRequested = false;
                try
                {
                    closeRequested = process.CloseMainWindow();
                }
                catch (InvalidOperationException)
                {
                    // Process exited between the HasExited check and here; nothing left to do.
                    return;
                }

                if (closeRequested && process.WaitForExit((int)GracefulStopTimeout.TotalMilliseconds))
                    return;

                if (!process.HasExited)
                    process.Kill();
            }
            catch
            {
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            B_start.Enabled = enabled;
            StartAccountCheckBox.Enabled = enabled;
            StartLauncherCheckBox.Enabled = enabled;
            StartLobbyCheckBox.Enabled = enabled;
            StartWorldCheckBox.Enabled = enabled;
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdateStatus), message);
                return;
            }

            StatusLabel.Text = message;
        }

        private static string GetConfigPath(string fileName)
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", fileName);
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Config file missing: " + configPath);

            return configPath;
        }

        private static string GetConfigValue(string configPath, params string[] elementPath)
        {
            XElement current = XDocument.Load(configPath).Root;
            foreach (string elementName in elementPath)
            {
                if (current == null)
                    break;

                current = current.Element(elementName);
            }

            if (current == null || string.IsNullOrWhiteSpace(current.Value))
                throw new InvalidOperationException("Missing config value " + string.Join("/", elementPath) + " in " + Path.GetFileName(configPath));

            return current.Value.Trim();
        }

        private static int GetConfigIntValue(string configPath, params string[] elementPath)
        {
            int parsedValue;
            if (!int.TryParse(GetConfigValue(configPath, elementPath), out parsedValue))
                throw new InvalidOperationException("Invalid numeric config value " + string.Join("/", elementPath) + " in " + Path.GetFileName(configPath));

            return parsedValue;
        }

        private static string NormalizeHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host) || host == "0.0.0.0" || host == "*" || host == "+")
                return "127.0.0.1";

            return host.Trim();
        }
    }
}
