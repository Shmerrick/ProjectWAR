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
        public CreateAccount()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            button.Background = Brushes.DarkRed;
            Content = null;
        }

        private void button_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            button.Background = Brushes.DarkRed;
        }
    }
}