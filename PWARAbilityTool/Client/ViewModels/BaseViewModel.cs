using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System.ComponentModel;

namespace PWARAbilityTool.Client.ViewModels
{
    public abstract class BaseViewModel : ViewModelBase, INotifyPropertyChanged
    {
        #region properties

        /// <summary>
        /// The caption of the window.
        /// </summary>
        public string WindowTitle { get; set; }

        public string Name { get; set; }
        protected ClientBackEndService clientBackEndService = new ClientBackEndService();

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

        public event PropertyChangedEventHandler PropertyChanged;

#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        private ViewModelType type;

        public ViewModelType Type { get => type; set => type = value; }
        public ViewModelMode Mode { get; set; }

        #endregion properties

        #region enums

        public enum ViewModelType
        {
            AbilitySingle,
            AbilityCommand,
            AbilityKnockBack,
            AbilityDmgHeal,
            AbilityModifiers,
            AbilityModifierChecks,
            AbilityBuffInfos,
            AbilityBuffCommands
        }

        public enum ViewModelMode
        {
            Update,
            Insert
        }

        #endregion enums

        #region constructors and destructors

        public BaseViewModel()
        {
            if (!IsInDesignModeStatic && !IsInDesignMode)
            {
                DispatcherHelper.Initialize();
            }
        }

        #endregion constructors and destructors

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}