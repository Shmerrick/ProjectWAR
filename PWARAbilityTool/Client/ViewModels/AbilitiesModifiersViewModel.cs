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
    public class AbilitiesModifiersViewModel : BaseViewModel
    {
        #region observable combobox datasources
        public ObservableCollection<string> commandNames { get; set; }
        #endregion

        #region class properties
        private AbilityModifiersModel abilityModifiersModel;
        public AbilityModifiersModel AbilityModifiersModel
        {
            get => abilityModifiersModel;
            set
            {
                if (abilityModifiersModel != value)
                {
                    abilityModifiersModel = value;
                }
            }
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesModifiersViewModel()
        {
            WindowTitle = "abilityModifiers";
            Name = "AbilitiesModifiersViewModel";
            AbilityModifiersModel = new AbilityModifiersModel();
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
            AbilityModifiers absDto = BuildService.buildAbilityModDtoFromModel(this.AbilityModifiersModel);
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

        private void HandleUpdateMode(AbilityModifiers absDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilityModifiers(this.AbilityModifiersModel.Entry, absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilityModifiersModel.Entry;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilityModifiers absDto)
        {
            if (!ValidationService.ValidateAbsModifiers(this.AbilityModifiersModel) && !ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            absDto.ability_modifiers_ID = Guid.NewGuid().ToString();
            HttpResponseMessage response = clientBackEndService.insertAbilityModifiers(absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilityModifiersModel.Entry;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
