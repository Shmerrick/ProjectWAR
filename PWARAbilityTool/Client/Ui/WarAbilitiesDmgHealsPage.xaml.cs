using PWARAbilityTool.Client.ViewModels;
using System.Windows;

namespace PWARAbilityTool.Client.Ui
{
    /// <summary>
    /// Interaktionslogik für WarAbilitiesDmgHealsPage.xaml
    /// </summary>
    public partial class WarAbilitiesDmgHealsPage : Window
    {
        public WarAbilitiesDmgHealsPage()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            DataContext = new AbilitiesDmgHealViewModel();
        }
    }
}
