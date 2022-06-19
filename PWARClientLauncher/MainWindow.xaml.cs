using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PWARClientLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CreateAccount createAccount = new CreateAccount();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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