using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using NLog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Launcher;
using System.Windows.Input;

namespace PWARClientLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool LaunchLocalServer { get; }
        public bool AllowMYPPatch { get; }
        public bool AllowServerPatch { get; }
        public bool AllowWarClientLaunch { get; }
        public object T_username { get; private set; }

        public static MainWindow Acc;

        public static string LocalServerIP = "127.0.0.1";
        public static string TestServerIP = "127.0.0.1";
        public static int LocalServerPort = 8000;
        public static int TestServerPort = 8000;
        private static HttpClient client = new HttpClient();
        //private Patcher patcher;

        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CreateAccount createAccount = new CreateAccount();

        public MainWindow()
        {
            // Read optional app settings (they may not exist in the app.config file)
            AllowWarClientLaunch = SafeReadAppSettings("AutoLaunch", true);
            AllowMYPPatch = SafeReadAppSettings("PatchMYP", true);
            AllowServerPatch = SafeReadAppSettings("PatchExe", true);
            LaunchLocalServer = SafeReadAppSettings("LaunchLocal", false);

            InitializeComponent();
            Acc = this;
        }

        private bool SafeReadAppSettings(string keyName, bool defaultValue)
        {
            var s = System.Configuration.ConfigurationManager.AppSettings[keyName];
            if (!String.IsNullOrEmpty(s))
            {
                // Key exists
                if (s == "false")
                    return false;
                if (s == "true")
                    return true;
            }
            else
            {
                // Key doesn't exist
                return defaultValue;
            }
            return defaultValue;
        }

        public void ReceiveStart()
        {
        }

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        //    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        //    var attrs = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

        //    //this.lblVersion.Text = $"{fvi.FileVersion} ({attrs.Single(x => x.Key == "GitHash").Value})";

        //    if (this.AllowMYPPatch)
        //    {
        //        _logger.Debug($"Calling Patcher Server on {System.Configuration.ConfigurationManager.AppSettings["ServerPatchIPAddress"]}:{System.Configuration.ConfigurationManager.AppSettings["ServerPatchPort"]}");
        //        patcher = new Patcher(_logger,
        //            $"{System.Configuration.ConfigurationManager.AppSettings["ServerPatchIPAddress"]}:{System.Configuration.ConfigurationManager.AppSettings["ServerPatchPort"]}");

        //        var patchDirectory = System.Configuration.ConfigurationManager.AppSettings["PatchDirectory"];

        //        Thread thread = new Thread(() => patcher.Patch(patchDirectory).Wait()) { IsBackground = true };
        //        thread.Start();
        //    }
        //}

        //private void Disconnect(object sender, FormClosedEventArgs e)
        //{
        //    Client.Close();
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

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            Client.Connect(TestServerIP, TestServerPort);

            string userCode = TextBox_login.Text.ToLower();
            string userPassword = TextBox_password.Text.ToLower();

            Client.User = userCode;

            string encryptedPassword = ConvertSHA256(userCode + ":" + userPassword);

            _logger.Info($@"Connecting to : {TestServerIP}:{TestServerPort} as {userCode} [{encryptedPassword}]");

            _logger.Info($"Sending CL_START to {TestServerIP}:{TestServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_START);
            Out.WriteString(userCode);
            Out.WriteString(encryptedPassword);

            Client.SendTCP(Out);

            Configuration configuration = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (configuration.AppSettings.Settings["LastUSerCode"] == null)
            {
                configuration.AppSettings.Settings.Add("LastUserCode", TextBox_login.Text);
            }
            else
            {
                configuration.AppSettings.Settings["LastUserCode"].Value = TextBox_login.Text;
            }
            configuration.Save();

            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Button_Start_Click(this, new RoutedEventArgs());
        }

        private void TextBox_PasswordChanged(object sender, TextChangedEventArgs e)
        {
            this.Button_Start_Click(this, new RoutedEventArgs());
        }

        private void Button_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new CreateAccount();
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
        }
    }
}