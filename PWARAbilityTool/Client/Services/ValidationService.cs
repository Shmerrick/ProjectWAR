using PWARAbilityTool.Client.Models;
using PWARAbilityTool.Client.ViewModels;
using PWARAbilityTool.Dtos;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;

namespace PWARAbilityTool.Client.Services
{
    public class ValidationService
    {
        #region validate models
        public static bool ValidateToUpdateMembers(List<string> toUpdateMembers)
        {
            bool returnVal = false;
            if (toUpdateMembers.Count > 0)
                returnVal = true;
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("Nothing to Update or Insert. Please changes values."));

            return returnVal;
        }

        public static bool ValidateAbsSingle(AbilitySingleModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityByEntry(model.Entry).Count == 0) ? true : false;
        }

        public static bool ValidateAbsCommand(AbilityCommandModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityCommandByEntry(model.Entry).Count == 0) ? true : false;
        }

        public static bool ValidateAbsDmgHeals(AbilityDamageHealsModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityDmgHealByEntry(model.Entry).Count == 0) ? true : false;
        }

        public static bool ValidateAbsKnockBackInfo(AbilityKnockBackInfoModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityKnockBackByEntry(model.Entry).Count == 0) ? true : false;
        }

        public static bool ValidateAbsModifierChecks(AbilityModifierChecksModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityModifierChecksByEntry(model.Entry).Count == 0) ? true : false;
        }

        public static bool ValidateAbsModifiers(AbilityModifiersModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityModifiersByEntry(model.Entry).Count == 0) ? true : false;
        }

        public static bool ValidateAbsBuffInfo(AbilityBuffInfosModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityBuffInfosByEntry(model.Entry).Count == 0) ? true : false;
        }

        public static bool ValidateAbsBuffCommands(AbilityBuffCommandsModel model)
        {
            ClientBackEndService clientBackEndService = new ClientBackEndService();
            return (clientBackEndService.searchAbilityBuffCommandsByEntry(model.Entry).Count == 0) ? true : false;
        }
        #endregion

        #region validate dto´s
        public static bool ValidateSearchResultAbilitySingle(List<AbilitySingle> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in Abilities table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }

        public static bool ValidateSearchResultAbilityCommand(List<AbilityCommand> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in Command table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }

        public static bool ValidateSearchResultAbilityDmgHeal(List<AbilityDamageHeals> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in DamageHeals table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }

        public static bool ValidateSearchResultAbilityKnockBackInfo(List<AbilityKnockBackInfo> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in Knockbackinfo table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }

        public static bool ValidateSearchResultAbilityModifierChecks(List<AbilityModifierChecks> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in Modifier-Checks table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }

        public static bool ValidateSearchResultAbilityModifiers(List<AbilityModifiers> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in Modifier table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }

        public static bool ValidateSearchResultAbilityBuffCommands(List<AbilityBuffCommands> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in Buff Commands table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }

        public static bool ValidateSearchResultAbilityBuffInfos(List<AbilityBuffInfos> result, string entry)
        {
            bool isNull = false;
            if (result.Count < 1)
            {
                ViewModelLocator.NotifyUserMethod(new NotificationMessage($"No Ability in Buff Infos table found with Entry: {entry}"));
                isNull = true;
            }

            return isNull;
        }
        #endregion
    }
}
