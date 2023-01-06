using PWARAbilityTool.Client.Models;
using PWARAbilityTool.Client.Services;
using PWARAbilityTool.Dtos;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using System.Net;
using System.Net.Http;
using System.Windows.Input;

namespace PWARAbilityTool.Client.ViewModels
{
    public class AbilitiesDmgHealViewModel : BaseViewModel
    {
        #region class properties
        private AbilityDamageHealsModel abilityDamageHealsModel;
        public AbilityDamageHealsModel AbilityDamageHealsModel
        {
            get => abilityDamageHealsModel;
            set
            {
                if (abilityDamageHealsModel != value)
                {
                    abilityDamageHealsModel = value;
                }
            }
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesDmgHealViewModel()
        {
            WindowTitle = "abilityDmgHeal";
            Name = "AbilitiesDmgHealViewModel";
            AbilityDamageHealsModel = new AbilityDamageHealsModel();
            SaveCommand = new RelayCommand(fireUpdateInsert);
        }

        #region methods
        private void fireUpdateInsert()
        {
            AbilityDamageHeals absDto = BuildService.buildAbilityDmgHealDtoFromModel(this.AbilityDamageHealsModel);
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

        private void HandleUpdateMode(AbilityDamageHeals absDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absDto.ToUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilityDamageHeals(this.AbilityDamageHealsModel.Entry, absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilityDamageHealsModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilityDamageHeals absDto)
        {
            if (!ValidationService.ValidateAbsDmgHeals(this.AbilityDamageHealsModel) && !ValidationService.ValidateToUpdateMembers(absDto.ToUpdateMembers))
                return;
            HttpResponseMessage response = clientBackEndService.insertAbilityDmgHeal(absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilityDamageHealsModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
