using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog.Internal;

namespace Launcher
{
    public partial class ApocLauncher : Form
    {
        public static ApocLauncher Acc;

        public static string LocalServerIP = "127.0.0.1";
        public static string TestServerIP = "63.209.33.116";
        public static int LocalServerPort = 8000;
        public static int TestServerPort = 8000;
        static HttpClient client = new HttpClient();
        private Patcher patcher;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ApocLauncher(bool allowLocal)
        {
            InitializeComponent();
            Acc = this;

            if (allowLocal)
            {
                this.bnConnectLocal.Visible = true;
                this.bnCreateLocal.Visible = true;
            }
            else
            {
                this.bnConnectLocal.Visible = false;
                this.bnCreateLocal.Visible = false;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        private void Form1_Load(object sender, EventArgs e)
        {

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var attrs = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            this.lblVersion.Text = fvi.FileVersion;
            //this.lblVersion.Text = $"{fvi.FileVersion} ({attrs.Single(x => x.Key == "GitHash").Value})";
            _logger.Debug($"Calling Patcher Server on { System.Configuration.ConfigurationManager.AppSettings["ServerPatchIPAddress"]}:{ System.Configuration.ConfigurationManager.AppSettings["ServerPatchPort"]}");
            patcher = new Patcher(_logger, $"{System.Configuration.ConfigurationManager.AppSettings["ServerPatchIPAddress"]}:{System.Configuration.ConfigurationManager.AppSettings["ServerPatchPort"]}");

            Thread thread = new Thread(() => patcher.Patch().Wait());
            thread.IsBackground = true;
            thread.Start();
        }

        private void Disconnect(object sender, FormClosedEventArgs e)
        {
            Client.Close();
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

        public static string ConvertSHA256(string value)
        {
            SHA256 sha = SHA256.Create();
            byte[] data = sha.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
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

        private void bnConnectToServer_Click(object sender, EventArgs e)
        {
            Client.Connect(TestServerIP, TestServerPort);
            lblConnection.Text = $@"Connecting to : {TestServerIP}:{TestServerPort}";

            string userCode = T_username.Text.ToLower();
            string userPassword = T_password.Text.ToLower();


            Client.User = userCode;

            string encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

            _logger.Info($@"Connecting to : {TestServerIP}:{TestServerPort} as {userCode} [{encryptedPassword}]");

            _logger.Info($"Sending CL_START to {TestServerIP}:{TestServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_START);
            Out.WriteString(userCode);
            Out.WriteString(encryptedPassword);

            Client.SendTCP(Out);
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
        private void buttonCreate_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxUsername.Text) || String.IsNullOrEmpty(textBoxPassword.Text)) return;

            Client.Connect(TestServerIP, TestServerPort);
            lblConnection.Text = $@"Connecting to : {TestServerIP}:{TestServerPort}";

            string userCode = textBoxUsername.Text.ToLower();
            string userPassword = textBoxPassword.Text.ToLower();

            Client.User = userCode;

            _logger.Info($@"Create account : {TestServerIP}:{TestServerPort} as {userCode}");

            _logger.Info($"Sending CL_CREATE to {TestServerIP}:{TestServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_CREATE);
            Out.WriteString(userCode);
            Out.WriteString(userPassword);

            Client.SendTCP(Out);
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
        private void bnCreateLocal_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxUsername.Text) || String.IsNullOrEmpty(textBoxPassword.Text)) return;

            Client.Connect(LocalServerIP, LocalServerPort);
            lblConnection.Text = $@"Connecting to : {LocalServerIP}:{LocalServerPort}";

            string userCode = textBoxUsername.Text.ToLower();
            string userPassword = textBoxPassword.Text.ToLower();

            Client.User = userCode;

            _logger.Info($@"Create account : {LocalServerIP}:{LocalServerPort} as {userCode}");

            _logger.Info($"Sending CL_CREATE to {LocalServerIP}:{LocalServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_CREATE);
            Out.WriteString(userCode);
            Out.WriteString(userPassword);

            Client.SendTCP(Out);
        }



        private void bnConnectToLocal_Click(object sender, EventArgs e)
        {
            Client.Connect(LocalServerIP, LocalServerPort);
            lblConnection.Text = $@"Connecting to : {LocalServerIP}:{LocalServerPort}";

            string userCode = T_username.Text.ToLower();
            string userPassword = T_password.Text.ToLower();


            Client.User = userCode;

            string encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

            _logger.Info($@"Connecting to : {LocalServerIP}:{LocalServerPort} as {userCode} [{encryptedPassword}]");

            _logger.Info($"Sending CL_START to {LocalServerIP}:{LocalServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_START);
            Out.WriteString(userCode);
            Out.WriteString(encryptedPassword);

            Client.SendTCP(Out);
        }

        private void timer1_Tick(object sender, EventArgs e)
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
                lblDownloading.Text = $"Requesting manifest...";
            }
            else if (patcher.CurrentState == Patcher.State.ProcessManifest)
            {
                bnConnectToServer.Enabled = false;
                lblDownloading.Text = $"Processing manifest...";
            }
            else if (patcher.CurrentState == Patcher.State.Done || patcher.CurrentState == Patcher.State.Error)
            {
                bnConnectToServer.Enabled = true;
                lblDownloading.Text = "";
            }
        }
    }
}