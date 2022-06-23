using NLog;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PWARClientLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool AllowMYPPatch { get; }
        public bool AllowServerPatch { get; }
        public bool AllowWarClientLaunch { get; }

        public static MainWindow Acc;

        public string TestServerIP = "127.0.0.1"; // IP for your server
        public int TestServerPort = 8000; // Port for your server
        private readonly HttpClient client = new HttpClient();
        //private Patcher patcher;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CreateAccount createAccount = new CreateAccount();

        /// <summary>
        /// Main Program Window here
        /// </summary>
        public MainWindow()
        {
            // Read optional app settings (they may not exist in the App.config file)
            AllowWarClientLaunch = SafeReadAppSettings("AutoLaunch", true);
            AllowMYPPatch = SafeReadAppSettings("PatchMYP", true);
            AllowServerPatch = SafeReadAppSettings("PatchExe", true);

            InitializeComponent();
            Acc = this;
            this.TextBox_login.Text = System.Configuration.ConfigurationManager.AppSettings["LastUserCode"];
            this.TextBox_password.Text = System.Configuration.ConfigurationManager.AppSettings["LastUSerPass"];
        }

        /// <summary>
        /// Settings for your all here
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private bool SafeReadAppSettings(string keyName, bool defaultValue)
        {
            var s = System.Configuration.ConfigurationManager.AppSettings[keyName];
            if (!string.IsNullOrEmpty(s))
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

        /// <summary>
        /// Start MainWindow program
        /// </summary>
        public void ReceiveStart()
        {
        }

        /// <summary>
        /// enscrypt for password Textbox ( I don't use it )
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        public void Print(string Message)
        {
        }

        /// <summary>
        /// Button Start functions here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (configuration.AppSettings.Settings["LastUSerPass"] == null)
            {
                configuration.AppSettings.Settings.Add("LastUSerPass", TextBox_password.Text);
            }
            else
            {
                configuration.AppSettings.Settings["LastUSerPass"].Value = TextBox_password.Text;
            }
            configuration.Save();

            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Login TextBox window functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Button_Start_Click(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Password TextBox window functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Button_Start_Click(this, new RoutedEventArgs());
        }

        /// <summary>
        /// We start Create account page here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new CreateAccount();
        }

        /// <summary>
        /// Create account page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
        }
    }
}