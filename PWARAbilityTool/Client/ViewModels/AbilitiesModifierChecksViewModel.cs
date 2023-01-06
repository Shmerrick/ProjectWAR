using PWARAbilityTool.Client.Models;
using PWARAbilityTool.Client.Services;
using PWARAbilityTool.Dtos;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Windows.Input;

namespace PWARAbilityTool.Client.ViewModels
{
    public class AbilitiesModifierChecksViewModel : BaseViewModel
    {
        #region observable combobox datasources
        public ObservableCollection<string> commandNames { get; set; }
        public ObservableCollection<string> failCodes { get; set; }
        #endregion

        #region class properties
        private AbilityModifierChecksModel abilityModifierChecksModel;
        public AbilityModifierChecksModel AbilityModifierChecksModel
        {
            get => abilityModifierChecksModel;
            set
            {
                if (abilityModifierChecksModel != value)
                {
                    abilityModifierChecksModel = value;
                }
            }
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesModifierChecksViewModel()
        {
            WindowTitle = "abilityModifierChecks";
            Name = "AbilitiesModifierChecksViewModel";
            AbilityModifierChecksModel = new AbilityModifierChecksModel();
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
            AbilityModifierChecks absDto = BuildService.buildAbilityModChecksDtoFromModel(this.AbilityModifierChecksModel);
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

        private void HandleUpdateMode(AbilityModifierChecks absDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilityModifierChecks(this.AbilityModifierChecksModel.Entry, absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilityModifierChecksModel.Entry;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilityModifierChecks absDto)
        {
            if (!ValidationService.ValidateAbsModifierChecks(this.AbilityModifierChecksModel) && !ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            absDto.ability_modifier_checks_ID = Guid.NewGuid().ToString();
            HttpResponseMessage response = clientBackEndService.insertAbilityModifierChecks(absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilityModifierChecksModel.Entry;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
