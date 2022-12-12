using NLog;
using PWARClientLauncher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PWARClientLauncher
{
    /// <summary>
    /// Interaction logic for CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Page
    {
        public string TestServerIP = "127.0.0.1"; // IP for your server
        public int TestServerPort = 8000; // Port for your server
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public CreateAccount()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close window button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            button.Background = Brushes.DarkRed;
            Content = null;
        }

        /// <summary>
        /// Create account button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCA_Create_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxCA_login.Text) || String.IsNullOrEmpty(PasswordBoxCA_password.Text)) return;

            Client.Connect(TestServerIP, TestServerPort);
            //MessageBox.Show($@"Connecting to : {TestServerIP}:{TestServerPort}");

            string userCode = TextBoxCA_login.Text.ToLower();
            string userPassword = PasswordBoxCA_password.Text.ToLower();

            Client.User = userCode;

            _logger.Info($@"Create account : {TestServerIP}:{TestServerPort} as {userCode}");

            _logger.Info($"Sending CL_CREATE to {TestServerIP}:{TestServerPort}");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_CREATE);
            Out.WriteString(userCode);
            Out.WriteString(userPassword);

            Client.SendTCP(Out);
        }
    }
}