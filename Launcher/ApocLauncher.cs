

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using Common;
using Launcher.Properties;
using NLog;

namespace Launcher {
    public partial class ApocLauncher : Form {
        public static ApocLauncher Acc;
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public ApocLauncher() {
            InitializeComponent();
            Acc = this;
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        private void Form1_Load(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(Settings.Default.LastLogin))
                T_username.Text = Settings.Default.LastLogin;


        }

        private void Disconnect(object sender, FormClosedEventArgs e) {
            Client.Close();
        }

        private void B_start_Click(object sender, EventArgs e) {
            Client.Connect(Client.LocalServerIP, Client.LocalServerPort);

            lblConnection.Text = $@"Connecting to : {Client.LocalServerIP}:{Client.LocalServerPort}";

            string userCode = T_username.Text.ToLower();
            string userPassword = T_password.Text.ToLower();

            Settings.Default.LastLogin = userCode;
            Settings.Default.Save();

            Client.User = userCode;

            string encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

            _logger.Info($@"Connecting to : {Client.LocalServerIP}:{Client.LocalServerPort} as {userCode}/{userPassword} [{encryptedPassword}]");
            _logger.Info($"Sending CL_START to {Client.LocalServerIP}:{Client.LocalServerPort}");

            PacketOut Out = new PacketOut((byte)Opcodes.CL_START);
            Out.WriteString(userCode);
            Out.WriteString(encryptedPassword);

            Client.SendTCP(Out);
            //B_start.Enabled = false;
        }

        public static string ConvertSHA256(string value) {
            SHA256 sha = SHA256.Create();
            byte[] data = sha.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }


        public void ReceiveStart() {
            //B_start.Enabled = true;
        }

        public void Print(string Message) {

        }

        private void bnTestServer_Click(object sender, EventArgs e) {
            var serverConfig = new ServerConfig();
            var zoneDirectoryLocation = serverConfig.ZoneDirectoryLocation;
            var IpAddress = serverConfig.GetTestIpAddress();

            Client.Connect(IpAddress, Client.TestServerPort);
            lblConnection.Text = $@"Connecting to : {IpAddress}:{Client.TestServerPort}";

            string userCode = T_username.Text.ToLower();
            string userPassword = T_password.Text.ToLower();

            Settings.Default.LastLogin = userCode;
            Settings.Default.Save();

            Client.User = userCode;

            string encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

            _logger.Info($@"Connecting to : {IpAddress}:{Client.TestServerPort} as {userCode}/{userPassword} [{encryptedPassword}]");

            _logger.Info($"Sending CL_START to {IpAddress}:{Client.TestServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_START);
            Out.WriteString(userCode);
            Out.WriteString(encryptedPassword);

            Client.SendTCP(Out);
        }

        private void bnClose_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void buttonPanelCreateAccount_Click(object sender, EventArgs e) {
            panelCreateAccount.Visible = true;
        }

        private void buttonCreate_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(textBoxUsername.Text) || string.IsNullOrEmpty(textBoxPassword.Text)) return;
            var serverConfig = new ServerConfig();
            var zoneDirectoryLocation = serverConfig.ZoneDirectoryLocation;
            var IpAddress = serverConfig.GetTestIpAddress();

            Client.Connect(IpAddress, Client.TestServerPort);
            lblConnection.Text = $@"Connecting to : {IpAddress}:{Client.TestServerPort}";

            string userCode = textBoxUsername.Text.ToLower();
            string userPassword = textBoxPassword.Text.ToLower();

            Client.User = userCode;



            _logger.Info($@"Create account : {IpAddress}:{Client.TestServerPort} as {userCode}/{userPassword}");

            _logger.Info($"Sending CL_START to {IpAddress}:{Client.TestServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_CREATE);
            Out.WriteString(userCode);
            Out.WriteString(userPassword);

            Client.SendTCP(Out);
        }

        private void buttonAccountClose_Click(object sender, EventArgs e) {
            panelCreateAccount.Visible = false;
        }
    }
}
