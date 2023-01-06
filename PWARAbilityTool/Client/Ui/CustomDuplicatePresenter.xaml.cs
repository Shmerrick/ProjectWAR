using PWARAbilityTool.Dtos;
using System.Collections.Generic;
using System.Windows;

namespace PWARAbilityTool.Client.Ui
{
    /// <summary>
    /// Interaktionslogik für CustomDuplicatePresenter.xaml
    /// </summary>
    public partial class CustomDuplicatePresenter : Window
    {
        private object selectedItem;
        public object SelectedItem
        {
            get => selectedItem;
            set => selectedItem = value;
        }

        public CustomDuplicatePresenter(List<AbilitySingle> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        public CustomDuplicatePresenter(List<AbilityCommand> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        public CustomDuplicatePresenter(List<AbilityDamageHeals> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        public CustomDuplicatePresenter(List<AbilityKnockBackInfo> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        public CustomDuplicatePresenter(List<AbilityModifierChecks> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        public CustomDuplicatePresenter(List<AbilityModifiers> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        public CustomDuplicatePresenter(List<AbilityBuffInfos> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        public CustomDuplicatePresenter(List<AbilityBuffCommands> abilities)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            source.ItemsSource = abilities;
            source.DisplayMemberPath = "Display";
            source.SelectedValuePath = "Entry";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (source.SelectedItem != null)
            {
                SelectedItem = source.SelectedItem;
                Close();
            }
        }
    }
}
