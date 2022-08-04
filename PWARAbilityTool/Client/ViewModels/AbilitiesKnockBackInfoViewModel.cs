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
    public class AbilitiesKnockBackInfoViewModel : BaseViewModel
    {
        #region class properties
        private AbilityKnockBackInfoModel abilityKnockBackInfoModel;
        public AbilityKnockBackInfoModel AbilityKnockBackInfoModel
        {
            get => abilityKnockBackInfoModel;
            set
            {
                if (abilityKnockBackInfoModel != value)
                {
                    abilityKnockBackInfoModel = value;
                }
            }
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesKnockBackInfoViewModel()
        {
            WindowTitle = "abilityKnockbackInfo";
            Name = "AbilitiesKnockBackInfoViewModel";
            AbilityKnockBackInfoModel = new AbilityKnockBackInfoModel();
            SaveCommand = new RelayCommand(fireUpdateInsert);
        }

        #region methods
        private void fireUpdateInsert()
        {
            AbilityKnockBackInfo absDto = BuildService.buildAbilityKnockBackInfoDtoFromModel(this.AbilityKnockBackInfoModel);
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

        private void HandleUpdateMode(AbilityKnockBackInfo absDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absDto.ToUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilityKnockBackInfo(this.AbilityKnockBackInfoModel.Entry, absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilityKnockBackInfoModel.Id;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilityKnockBackInfo absDto)
        {
            if (!ValidationService.ValidateAbsKnockBackInfo(this.AbilityKnockBackInfoModel) && !ValidationService.ValidateToUpdateMembers(absDto.ToUpdateMembers))
                return;
            HttpResponseMessage response = clientBackEndService.insertAbilityKnockBackInfo(absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilityKnockBackInfoModel.Id;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
