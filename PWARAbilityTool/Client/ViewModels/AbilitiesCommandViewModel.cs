using PWARAbilityTool.Client.Models;
using PWARAbilityTool.Client.Services;
using PWARAbilityTool.Dtos;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Windows.Input;

namespace PWARAbilityTool.Client.ViewModels
{
    public class AbilitiesCommandViewModel : BaseViewModel
    {
        #region binding properties
        #region selected items of cb´s 
        private string selectedTarget;
        public string SelectedTarget
        {
            get => selectedTarget;
            set
            {
                selectedTarget = value;
                OnPropertyChanged("SelectedTarget");
            }
        }

        private string selectedEffectSource;
        public string SelectedEffectSource
        {
            get => selectedEffectSource;
            set
            {
                selectedEffectSource = value;
                OnPropertyChanged("SelectedEffectSource");
            }
        }
        #endregion

        #region observable combobox datasources
        public ObservableCollection<string> targets { get; set; }
        public ObservableCollection<string> effectSources { get; set; }
        public ObservableCollection<string> cmdNames { get; set; }
        #endregion
        #endregion

        #region class properties
        private AbilityCommandModel abilityCommandModel;
        public AbilityCommandModel AbilityCommandModel
        {
            get => abilityCommandModel;
            set
            {
                if (abilityCommandModel != value)
                {
                    abilityCommandModel = value;
                }
            }
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesCommandViewModel()
        {
            WindowTitle = "abilityCommand";
            Name = "AbilitiesCommandViewModel";
            AbilityCommandModel = new AbilityCommandModel();
            SaveCommand = new RelayCommand(fireUpdateInsert);
            InitializeComboProperties();
        }

        #region methods
        private void InitializeComboProperties()
        {
            targets = new ObservableCollection<string>();
            effectSources = new ObservableCollection<string>();
        }

        private void fireUpdateInsert()
        {
            AbilityCommand absDto = BuildService.buildAbilityCommandDtoFromModel(this.AbilityCommandModel);
            switch (Mode)
            {
                case ViewModelMode.Update:
                    HandleUpdateMode(absDto);
                    break;
                case ViewModelMode.Insert:
                    HandleInsertMode(absDto);
                    break;
                default:
                    break;
            }
        }

        private void HandleUpdateMode(AbilityCommand absDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilityCommand(this.AbilityCommandModel.Entry, absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilityCommandModel.AbilityName;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilityCommand absDto)
        {
            if (!ValidationService.ValidateAbsCommand(this.AbilityCommandModel) && !ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.insertAbilityCommand(absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilityCommandModel.AbilityName;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
