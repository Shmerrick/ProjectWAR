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
    public class AbilitiesBuffCommandViewModel : BaseViewModel
    {
        #region class properties
        private AbilityBuffCommandsModel abilityBuffCommandsModel;
        public AbilityBuffCommandsModel AbilityBuffCommandsModel
        {
            get => abilityBuffCommandsModel;
            set
            {
                if (abilityBuffCommandsModel != value)
                {
                    abilityBuffCommandsModel = value;
                }
            }
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region observable combobox datasources
        public ObservableCollection<string> commandNames { get; set; }
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesBuffCommandViewModel()
        {
            WindowTitle = "abilitiesBuffCommandViewModel";
            Name = "AbilitiesBuffCommandViewModel";
            AbilityBuffCommandsModel = new AbilityBuffCommandsModel();
            SaveCommand = new RelayCommand(fireUpdateInsert);
            InitializeComboProperties();
        }

        #region methods
        private void InitializeComboProperties()
        {
            commandNames = new ObservableCollection<string>();
        }

        private void fireUpdateInsert()
        {
            AbilityBuffCommands absDto = BuildService.buildAbilityBuffCommandsDtoFromModel(this.AbilityBuffCommandsModel);
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

        private void HandleUpdateMode(AbilityBuffCommands absDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilityBuffCommands(this.AbilityBuffCommandsModel.Entry, absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilityBuffCommandsModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilityBuffCommands absDto)
        {
            if (!ValidationService.ValidateAbsBuffCommands(this.AbilityBuffCommandsModel) && !ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.insertAbilityBuffCommands(absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilityBuffCommandsModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
