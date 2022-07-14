/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:PWARAbilityTool"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace PWARAbilityTool.Client.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        #region properties => all ViewModels

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public SearchAbilityViewModel Search => ServiceLocator.Current.GetInstance<SearchAbilityViewModel>();
        public AbilitiesSingleViewModel absSingle => ServiceLocator.Current.GetInstance<AbilitiesSingleViewModel>();
        public AbilitiesCommandViewModel absCommand => ServiceLocator.Current.GetInstance<AbilitiesCommandViewModel>();
        public AbilitiesDmgHealViewModel absDmgHeal => ServiceLocator.Current.GetInstance<AbilitiesDmgHealViewModel>();
        public AbilitiesModifierChecksViewModel absModChecks => ServiceLocator.Current.GetInstance<AbilitiesModifierChecksViewModel>();
        public AbilitiesModifiersViewModel absMods => ServiceLocator.Current.GetInstance<AbilitiesModifiersViewModel>();
        public AbilitiesBuffInfoViewModel absBuffInfo => ServiceLocator.Current.GetInstance<AbilitiesBuffInfoViewModel>();
        public AbilitiesBuffCommandViewModel absBuffCmd => ServiceLocator.Current.GetInstance<AbilitiesBuffCommandViewModel>();
        #endregion

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SearchAbilityViewModel>();
            SimpleIoc.Default.Register<AbilitiesSingleViewModel>();
            SimpleIoc.Default.Register<AbilitiesCommandViewModel>();
            SimpleIoc.Default.Register<AbilitiesDmgHealViewModel>();
            SimpleIoc.Default.Register<AbilitiesModifierChecksViewModel>();
            SimpleIoc.Default.Register<AbilitiesModifiersViewModel>();
            SimpleIoc.Default.Register<AbilitiesBuffInfoViewModel>();
            SimpleIoc.Default.Register<AbilitiesBuffCommandViewModel>();

            Messenger.Default.Register<NotificationMessage>(this, NotifyUserMethod);
        }

        public static void NotifyUserMethod(NotificationMessage message)
        {
            MessageBox.Show(message.Notification);
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}