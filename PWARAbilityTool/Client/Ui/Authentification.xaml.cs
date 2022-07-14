using PWARAbilityTool.Client;
using apoc_api.security;
using NLog;
using System.Configuration;
using System.Windows;

namespace PWARAbilityTool
{
    /// <summary>
    /// Interaktionslogik für Authentification.xaml
    /// </summary>
    public partial class Authentification : Window
    {
        private string baseUrl = ConfigurationManager.AppSettings["base-url"];
        private string authPath = ConfigurationManager.AppSettings["authentication-path"];
        public string AuthenticationToken { get; set; }
        public string UserCode { get; set; }
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Authentification()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void connect(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.passwordBox.Password) || string.IsNullOrEmpty(this.userTb.Text))
            {
                this.labelDisplayMessage.Content = "Username or Password is empty. You can not login.";
                return;
            }

            Authenticator authenticationEngine = new Authenticator (Logger)
            {
                BaseUrl = baseUrl,
                AuthenticationPath = authPath,
                HeartbeatPath = ConfigurationManager.AppSettings["HeartbeatPath"]
            };

            if (authenticationEngine.CanConnect())
            {
                Logger.Info($"Trying to connect to {baseUrl}");
                AuthenticationToken = authenticationEngine.Connect(this.userTb.Text, this.passwordBox.Password);
                if (AuthenticationToken != "")
                {
                    Logger.Info($"Connected to server! Waiting for incoming requests...");
                    TokenHolder tokenHolder = TokenHolder.GetInstance();
                    tokenHolder.token = AuthenticationToken;
                    DialogResult = true;
                }
                else
                {
                    this.labelDisplayMessage.Content = "Username/password is invalid";
                    Logger.Info("Auth Token is empty. Username/password is invalid and client is not authorized!");
                }
            }
            else
            {
                this.labelDisplayMessage.Content = "unable to connect";
                Logger.Info($"unable to connect to {baseUrl}{authPath}!");
            }
        }
    }
}
