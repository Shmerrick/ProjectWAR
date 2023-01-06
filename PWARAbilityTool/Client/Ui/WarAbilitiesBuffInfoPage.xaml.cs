using PWARAbilityTool.Client.ViewModels;
using System.Windows;

namespace PWARAbilityTool.Client.Ui
{
    /// <summary>
    /// Interaktionslogik für WarAbilitiesBuffInfoPage.xaml
    /// </summary>
    public partial class WarAbilitiesBuffInfoPage : Window
    {
        public WarAbilitiesBuffInfoPage()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            DataContext = new AbilitiesBuffInfoViewModel();
        }
    }
}
