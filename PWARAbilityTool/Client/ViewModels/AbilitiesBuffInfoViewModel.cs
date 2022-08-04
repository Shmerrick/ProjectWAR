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
    public class AbilitiesBuffInfoViewModel : BaseViewModel
    {
        #region class properties
        private AbilityBuffInfosModel abilityBuffInfosModel;
        public AbilityBuffInfosModel AbilityBuffInfosModel
        {
            get => abilityBuffInfosModel;
            set
            {
                if (abilityBuffInfosModel != value)
                {
                    abilityBuffInfosModel = value;
                }
            }
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesBuffInfoViewModel()
        {
            WindowTitle = "abilitiesBuffInfoViewModel";
            Name = "AbilitiesBuffInfoViewModel";
            AbilityBuffInfosModel = new AbilityBuffInfosModel();
            SaveCommand = new RelayCommand(fireUpdateInsert);
        }

        #region methods
        private void fireUpdateInsert()
        {
            AbilityBuffInfos absDto = BuildService.buildAbilityBuffInfosDtoFromModel(this.AbilityBuffInfosModel);
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

        private void HandleUpdateMode(AbilityBuffInfos absDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilityBuffInfos(this.AbilityBuffInfosModel.Entry, absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilityBuffInfosModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilityBuffInfos absDto)
        {
            if (!ValidationService.ValidateAbsBuffInfo(this.AbilityBuffInfosModel) && !ValidationService.ValidateToUpdateMembers(absDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.insertAbilityBuffInfos(absDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilityBuffInfosModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
