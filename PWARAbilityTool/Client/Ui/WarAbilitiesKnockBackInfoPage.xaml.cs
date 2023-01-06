using PWARAbilityTool.Client.ViewModels;
using System.Windows;

namespace PWARAbilityTool.Client.Ui
{
    /// <summary>
    /// Interaktionslogik für WarAbilitiesKnockBackInfoPage.xaml
    /// </summary>
    public partial class WarAbilitiesKnockBackInfoPage : Window
    {
        public WarAbilitiesKnockBackInfoPage()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            DataContext = new AbilitiesKnockBackInfoViewModel();
        }
    }
}
