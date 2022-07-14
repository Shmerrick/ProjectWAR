using PWARAbilityTool.Dtos;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PWARAbilityTool.Client
{
    public class ClientBackEndService
    {
        #region members
        public HttpClient client = new HttpClient();
        public string baseUrl = ConfigurationManager.AppSettings["base-url"];
        public string basePath = $"api/";
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        public ClientBackEndService()
        {
        }

        #region search
        public List<AbilitySingle> searchAllAbilities()
        {
            Logger.Info("Trying to search for all abilities");
            string path = basePath + "Ability/all";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilitySingle> abilityList = JsonConvert.DeserializeObject<List<AbilitySingle>>(response.Content.ReadAsStringAsync().Result);
            return abilityList;
        }

        public List<AbilitySingle> searchAbilityByEntry(int Entry)
        {
            Logger.Info($"Trying to search for ability with Entry {Entry}");
            string path = basePath + $"Ability/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilitySingle> abilitySingle = JsonConvert.DeserializeObject<List<AbilitySingle>>(response.Content.ReadAsStringAsync().Result);
            return abilitySingle;
        }

        public List<AbilityCommand> searchAbilityCommandByEntry(int Entry)
        {
            Logger.Info($"Trying to search for abilityCommand with Entry {Entry}");
            string path = basePath + $"AbilityCommand/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilityCommand> abilitySingle = JsonConvert.DeserializeObject<List<AbilityCommand>>(response.Content.ReadAsStringAsync().Result);
            return abilitySingle;
        }

        public List<AbilityDamageHeals> searchAbilityDmgHealByEntry(int Entry)
        {
            Logger.Info($"Trying to search for AbilityDmgHeal with Entry {Entry}");
            string path = basePath + $"AbilityDmgHeal/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilityDamageHeals> abilitySingle = JsonConvert.DeserializeObject<List<AbilityDamageHeals>>(response.Content.ReadAsStringAsync().Result);
            return abilitySingle;
        }

        public List<AbilityKnockBackInfo> searchAbilityKnockBackByEntry(int Entry)
        {
            Logger.Info($"Trying to search for AbilityKnockBack with Entry {Entry}");
            string path = basePath + $"AbilityKnockBack/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilityKnockBackInfo> abilityList = JsonConvert.DeserializeObject<List<AbilityKnockBackInfo>>(response.Content.ReadAsStringAsync().Result);
            return abilityList;
        }

        public List<AbilityModifiers> searchAbilityModifiersByEntry(int Entry)
        {
            Logger.Info($"Trying to search for AbilityMod with Entry {Entry}");
            string path = basePath + $"AbilityMod/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilityModifiers> abilitySingle = JsonConvert.DeserializeObject<List<AbilityModifiers>>(response.Content.ReadAsStringAsync().Result);
            return abilitySingle;
        }

        public List<AbilityModifierChecks> searchAbilityModifierChecksByEntry(int Entry)
        {
            Logger.Info($"Trying to search for AbilityModChecks with Entry {Entry}");
            string path = basePath + $"AbilityModChecks/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilityModifierChecks> abilitySingle = JsonConvert.DeserializeObject<List<AbilityModifierChecks>>(response.Content.ReadAsStringAsync().Result);
            return abilitySingle;
        }

        public List<AbilityBuffCommands> searchAbilityBuffCommandsByEntry(int Entry)
        {
            Logger.Info($"Trying to search for BuffCommand with Entry {Entry}");
            string path = basePath + $"BuffCommand/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilityBuffCommands> abilitySingle = JsonConvert.DeserializeObject<List<AbilityBuffCommands>>(response.Content.ReadAsStringAsync().Result);
            return abilitySingle;
        }

        public List<AbilityBuffInfos> searchAbilityBuffInfosByEntry(int Entry)
        {
            Logger.Info($"Trying to search for BuffInfo with Entry {Entry}");
            string path = basePath + $"BuffInfo/Entry/{Entry}";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilityBuffInfos> abilitySingle = JsonConvert.DeserializeObject<List<AbilityBuffInfos>>(response.Content.ReadAsStringAsync().Result);
            return abilitySingle;
        }

        public List<AbilitySingle> searchPlayerAbility()
        {
            Logger.Info($"Trying to search for ability with specline 'Player'");
            string path = basePath + "Ability/player";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilitySingle> abilitiesSingle = JsonConvert.DeserializeObject<List<AbilitySingle>>(response.Content.ReadAsStringAsync().Result);
            return abilitiesSingle;
        }

        public List<AbilitySingle> searchNPCAbility()
        {
            Logger.Info($"Trying to search for ability with specline 'NPC'");
            string path = basePath + "Ability/npc";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<AbilitySingle> abilitiesSingle = JsonConvert.DeserializeObject<List<AbilitySingle>>(response.Content.ReadAsStringAsync().Result);
            return abilitiesSingle;
        }
        #endregion

        #region update
        #region update AbilitySingle
        public HttpResponseMessage updateAbilitySingle(int Entry, AbilitySingle abilitySingle)
        {
            Logger.Info($"Trying to update ability with Entry {Entry}");
            string path = basePath + "Ability/update/abilitySingle";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilitySingle), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region update AbilityCommand
        public HttpResponseMessage updateAbilityCommand(int Entry, AbilityCommand abilityCommand)
        {
            Logger.Info($"Trying to update abilityCommand with Entry {Entry}");
            string path = basePath + "Ability/update/abilityCommand";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityCommand), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region update AbilityDamageHeals
        public HttpResponseMessage updateAbilityDamageHeals(int Entry, AbilityDamageHeals abilityDamageHeals)
        {
            Logger.Info($"Trying to update abilityDmgHeal with Entry {Entry}");
            string path = basePath + "Ability/update/abilityDamageHeals";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityDamageHeals), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region update AbilityKnockBackInfo
        public HttpResponseMessage updateAbilityKnockBackInfo(int Entry, AbilityKnockBackInfo abilityKnockBackInfo)
        {
            Logger.Info($"Trying to update abilityKnockDownInfo with Entry {Entry}");
            string path = basePath + "Ability/update/abilityKnockBackInfo";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityKnockBackInfo), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region update AbilityModifierChecks
        public HttpResponseMessage updateAbilityModifierChecks(int Entry, AbilityModifierChecks abilityModifierChecks)
        {
            Logger.Info($"Trying to update abilityModifierChecks with Entry {Entry}");
            string path = basePath + "Ability/update/abilityModifierChecks";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityModifierChecks), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region update AbilityModifiers
        public HttpResponseMessage updateAbilityModifiers(int Entry, AbilityModifiers abilityModifiers)
        {
            Logger.Info($"Trying to update abilityModifiers with Entry {Entry}");
            string path = basePath + "Ability/update/abilityModifiers";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityModifiers), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region update AbilityBuffInfos
        public HttpResponseMessage updateAbilityBuffInfos(int Entry, AbilityBuffInfos abilityBuffInfos)
        {
            Logger.Info($"Trying to update abilityBuffInfo with Entry {Entry}");
            string path = basePath + "Ability/update/abilityBuffInfos";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityBuffInfos), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region update AbilityBuffCommands
        public HttpResponseMessage updateAbilityBuffCommands(int Entry, AbilityBuffCommands abilityBuffCommands)
        {
            Logger.Info($"Trying to update abilityBuffCommand with Entry {Entry}");
            string path = basePath + "Ability/update/abilityBuffCommands";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityBuffCommands), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #endregion

        #region inserts
        #region insert ability single
        public HttpResponseMessage insertAbilitySingle(AbilitySingle abilitySingle)
        {
            string path = basePath + "Ability/insert/abilitySingle";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilitySingle), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region insert abilityCommand
        public HttpResponseMessage insertAbilityCommand(AbilityCommand abilityCommand)
        {
            string path = basePath + "Ability/insert/abilityCommand";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityCommand), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region insert abilityDmgHeals
        public HttpResponseMessage insertAbilityDmgHeal(AbilityDamageHeals abilityDmgHeals)
        {
            string path = basePath + "Ability/insert/abilityDmgHeals";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityDmgHeals), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region insert abilityKnockBack
        public HttpResponseMessage insertAbilityKnockBackInfo(AbilityKnockBackInfo abilityKnockBack)
        {
            string path = basePath + "Ability/insert/abilityKnockBack";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityKnockBack), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region insert abilityModifiers
        public HttpResponseMessage insertAbilityModifiers(AbilityModifiers abilityModifiers)
        {
            string path = basePath + "Ability/insert/abilityModifiers";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityModifiers), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region insert abilityModifierChecks
        public HttpResponseMessage insertAbilityModifierChecks(AbilityModifierChecks abilityModifierChecks)
        {
            string path = basePath + "Ability/insert/abilityModifierChecks";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityModifierChecks), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region insert abilityBuffInfos
        public HttpResponseMessage insertAbilityBuffInfos(AbilityBuffInfos abilityBuffInfos)
        {
            string path = basePath + "Ability/insert/abilityBuffInfos";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityBuffInfos), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #region insert abilityBuffCommands
        public HttpResponseMessage insertAbilityBuffCommands(AbilityBuffCommands abilityBuffCommands)
        {
            string path = basePath + "Ability/insert/abilityBuffCommands";
            prepareClient();
            HttpResponseMessage response = client.PostAsync(path, new StringContent(JsonConvert.SerializeObject(abilityBuffCommands), Encoding.UTF8, "application/json")).Result;
            return response;
        }
        #endregion

        #endregion

        #region get methods for viewmodels
        #region abilities table
        public List<string> GetAllAbilitySpeclines()
        {
            Logger.Info($"Trying to get all speclines");
            string path = basePath + "Ability/speclines";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> speclines = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return speclines;
        }

        public List<string> GetAllAbilityCareerLines()
        {
            Logger.Info($"Trying to get all careerLines");
            string path = basePath + "Ability/careerLines";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> careerLines = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return careerLines;
        }

        public List<string> GetAllAbilityTypes()
        {
            Logger.Info($"Trying to get all abilityTypes");
            string path = basePath + "Ability/abilityTypes";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> abilityTypes = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return abilityTypes;
        }

        public List<string> GetAllAbilityMasteryTrees()
        {
            Logger.Info($"Trying to get all masteryTrees");
            string path = basePath + "Ability/masteryTrees";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> masteryTrees = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return masteryTrees;
        }
        #endregion

        #region abs command table
        public List<string> GetAllAbsCommandTargets()
        {
            Logger.Info("Trying to get all Targets");
            string path = basePath + "Ability/targets";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> allTargets = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return allTargets;
        }

        public List<string> GetAllAbsCommandEffectSource()
        {
            Logger.Info("Trying to get all EffectSource");
            string path = basePath + "Ability/effectSource";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> allEffectSource = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return allEffectSource;
        }

        public List<string> GetAllAbsCommandNames()
        {
            Logger.Info($"Trying to get all commandNames");
            string path = basePath + "Ability/cmdNames";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> commandNames = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return commandNames;
        }
        #endregion

        #region abs modifier checks
        public List<string> GetAllFailCodes()
        {
            Logger.Info($"Trying to get all failcodes");
            string path = basePath + "Ability/failCodes";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> failCodes = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return failCodes;
        }

        public List<string> GetAllCommandNames()
        {
            Logger.Info($"Trying to get all commandNames");
            string path = basePath + "Ability/commandNames";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> commandNames = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return commandNames;
        }
        #endregion

        #region abs modifiers;
        public List<string> GetAllModifierCommandNames()
        {
            Logger.Info($"Trying to get all modifierCommandNames");
            string path = basePath + "Ability/modifierCommandNames";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> modifierCommandNames = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return modifierCommandNames;
        }
        #endregion

        #region buff command 
        public List<string> GetAllBuffCommandNames()
        {
            Logger.Info($"Trying to get all buffCommandNames");
            string path = basePath + "Ability/buffCommandNames";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> buffCommandNames = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return buffCommandNames;
        }

        public List<string> GetAllBuffCommandTargets()
        {
            Logger.Info("Trying to get all buffTargets");
            string path = basePath + "Ability/buffTargets";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> allTargets = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return allTargets;
        }

        public List<string> GetAllBuffCommandEffectSource()
        {
            Logger.Info("Trying to get all buffEffectSource");
            string path = basePath + "Ability/buffEffectSource";
            prepareClient();
            HttpResponseMessage response = client.GetAsync(path).Result;
            List<string> allEffectSource = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
            return allEffectSource;
        }
        #endregion

        #endregion
        #region helpers
        private void prepareClient()
        {
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri($"{baseUrl}");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("auth-token", TokenHolder.GetInstance().token);
            }
        }
        #endregion
    }
}
