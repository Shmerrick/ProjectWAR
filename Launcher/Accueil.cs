

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

namespace Launcher
{
    public partial class Accueil : Form
    {
        public static Accueil Acc;
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public Accueil()
        {
            InitializeComponent();
            Acc = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.Default.LastLogin))
                T_username.Text = Settings.Default.LastLogin;
        }

        private void Disconnect(object sender, FormClosedEventArgs e)
        {
            Client.Close();
        }

        private void B_start_Click(object sender, EventArgs e)
        {
            Client.Connect(Client.LocalServerIP, Client.LocalServerPort);

            Accueil.Acc.statusStrip1.Items[0].Text = $@"Connecting to : {Client.LocalServerIP}:{Client.LocalServerPort}";

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
    
        private void bnTestServer_Click(object sender, EventArgs e)
        {
            var serverConfig = new ServerConfig();

            serverConfig.ZoneDirectoryLocation = "abc";

            var zoneDirectoryLocation = serverConfig.ZoneDirectoryLocation;

            serverConfig.ZoneDirectoryLocation = "def";

            var IpAddress = serverConfig.GetTestIpAddress();

            Client.Connect(IpAddress, Client.TestServerPort);
            Accueil.Acc.statusStrip1.Items[0].Text = $@"Connecting to : {IpAddress}:{Client.TestServerPort}";

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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void T_username_TextChanged(object sender, EventArgs e)
        {

        }

        private void T_password_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }
    }
}
