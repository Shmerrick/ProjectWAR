using PWARAbilityTool.apoc_api_serverrunner.Services;
using PWARAbilityTool.Client.Commands;
using PWARAbilityTool.Client.Messages;
using PWARAbilityTool.Client.Models;
using PWARAbilityTool.Client.Services;
using PWARAbilityTool.Dtos;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PWARAbilityTool.Client.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        #region binding properties
        public ObservableCollection<AbilitySingleModel> abilitiesSingle { get; set; }
        private string entry;
        public string Entry
        {
            get => entry;
            set
            {
                entry = value;
                RaisePropertyChanged(() => isReady);
            }
        }

        private string selectedTableToUpdate;
        public string SelectedTableToUpdate
        {
            get => selectedTableToUpdate;
            set
            {
                if (selectedTableToUpdate != value)
                {
                    selectedTableToUpdate = value;
                    OnPropertyChanged("SelectedTableToUpdate");
                }
            }
        }

        private string selectedTableToInsert;
        public string SelectedTableToInsert
        {
            get => selectedTableToInsert;
            set
            {
                if (selectedTableToInsert != value)
                {
                    selectedTableToInsert = value;
                    OnPropertyChanged("SelectedTableToInsert");
                }
            }
        }

        public ObservableCollection<string> tableNames { get; set; }
        #endregion

        #region class properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string msgDefault = "Search successfull - displaying now Searchresult";
        private string msgError = "Unsuccessfull search! Could not find any Abilities!";
        #endregion

        #region commands
        public ICommand searchAllAbilitiesCommand { get; private set; }
        public ICommand searchAbilityByEntryCommand { get; private set; }
        public ICommand searchPlayerAbilitiesCommand { get; private set; }
        public ICommand searchNpcAbilitiesCommand { get; private set; }
        public ICommand openUpdateAbsCommand { get; private set; }
        public ICommand openInsertAbsCommand { get; private set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            WindowTitle = "WAR-Apoc PWARAbilityTool";
            Name = "MainViewModel";
            InitializeProperties();
            fillTableNames();
        }

        #region command methods

        #region update
        private void fireUpdateAbility(string entry, string selectedTable)
        {
            if (selectedTable.Equals("war_world.abilities"))
            {
                List<AbilitySingle> result = clientBackEndService.searchAbilityByEntry(int.Parse(entry));

                if (!ValidationService.ValidateSearchResultAbilitySingle(result, entry))
                {
                    AbilitySingle ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilitySingleModel model = BuildService.buildAbilitySingleModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityMessage("Update abilities", model, fillSpeclines(), fillCareerLines(), 
                        fillAbilityTypes(), fillMasteryTrees(), false));
                }
            }
            else if (selectedTable.Equals("war_world.ability_commands"))
            {
                List<AbilityCommand> result = clientBackEndService.searchAbilityCommandByEntry(int.Parse(entry));
                if (!ValidationService.ValidateSearchResultAbilityCommand(result, entry))
                {
                    AbilityCommand ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilityCommandModel model = BuildService.buildAbilityCommandModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityCommandMessage("Update ability_command", model, fillAllAbsCommandTargets(), 
                        fillAllAbsCommandEffectSource(), fillAllAbsCommandNames(), false));
                }
            }
            else if (selectedTable.Equals("war_world.ability_damage_heals"))
            {
                List<AbilityDamageHeals> result = clientBackEndService.searchAbilityDmgHealByEntry(int.Parse(entry));
                if (!ValidationService.ValidateSearchResultAbilityDmgHeal(result, entry))
                {
                    AbilityDamageHeals ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilityDamageHealsModel model = BuildService.buildAbilityDmgHealModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityDmgHealMessage("Update ability_dmg_heal", model, false));
                }
            }
            else if (selectedTable.Equals("war_world.ability_knockback_info"))
            {
                List<AbilityKnockBackInfo> result = clientBackEndService.searchAbilityKnockBackByEntry(int.Parse(entry));
                if (!ValidationService.ValidateSearchResultAbilityKnockBackInfo(result, entry))
                {
                    AbilityKnockBackInfo ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilityKnockBackInfoModel model = BuildService.buildAbilityKnockBackInfoModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityKnockbackInfoMessage("Update ability_knockbackInfo", model, false));
                }
            }
            else if (selectedTable.Equals("war_world.ability_modifiers"))
            {
                List<AbilityModifiers> result = clientBackEndService.searchAbilityModifiersByEntry(int.Parse(entry));
                if (!ValidationService.ValidateSearchResultAbilityModifiers(result, entry))
                {
                    AbilityModifiers ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilityModifiersModel model = BuildService.buildAbilityModModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityModifierMessage("Update ability_modifiers", model, fillAllAbsModifiersCmdNames(), false));
                }
            }
            else if (selectedTable.Equals("war_world.ability_modifier_checks"))
            {
                List<AbilityModifierChecks> result = clientBackEndService.searchAbilityModifierChecksByEntry(int.Parse(entry));
                if (!ValidationService.ValidateSearchResultAbilityModifierChecks(result, entry))
                {
                    AbilityModifierChecks ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilityModifierChecksModel model = BuildService.buildAbilityModChecksModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityModifierChecksMessage("Update ability_modifierchecks", model,
                        fillAllAbsModifierCheckCommandNames(), fillAllAbsModChecksFailCodes(), false));
                }
            }
            else if (selectedTable.Equals("war_world.buff_commands"))
            {
                List<AbilityBuffCommands> result = clientBackEndService.searchAbilityBuffCommandsByEntry(int.Parse(entry));
                if (!ValidationService.ValidateSearchResultAbilityBuffCommands(result, entry))
                {
                    AbilityBuffCommands ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilityBuffCommandsModel model = BuildService.buildAbilityBuffCommandsModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityBuffCmdMessage("Update ability_buffCommands", model, fillAbsBuffCmds(), false));
                }
            }
            else
            {
                List<AbilityBuffInfos> result = clientBackEndService.searchAbilityBuffInfosByEntry(int.Parse(entry));
                if (!ValidationService.ValidateSearchResultAbilityBuffInfos(result, entry))
                {
                    AbilityBuffInfos ab;
                    if (result.Count > 1)
                        ab = AbilityService.getSelectedAbility(result);
                    else
                        ab = result.Find(abs => abs != null);

                    AbilityBuffInfosModel model = BuildService.buildAbilityBuffInfosModelFromDto(ab);
                    MessengerInstance.Send(new OpenAbilityBuffInfoMessage("Update ability_buffInfos", model, false));
                }
            }
        }
        #endregion

        #region insert
        private void fireInsertAbility(string selectedTable)
        {
            if (selectedTable.Equals("war_world.abilities"))
            {
                AbilitySingleModel model = BuildService.buildAbilitySingleModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityMessage("Insert abilities", model, fillSpeclines(), fillCareerLines(), 
                    fillAbilityTypes(), fillMasteryTrees(), true));
            }
            else if (selectedTable.Equals("war_world.ability_commands"))
            {
                AbilityCommandModel model = BuildService.buildAbilityCommandModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityCommandMessage("Insert ability_command", model, fillAllAbsCommandTargets(), 
                    fillAllAbsCommandEffectSource(), fillAllAbsCommandNames(), true));
            }
            else if (selectedTable.Equals("war_world.ability_damage_heals"))
            {
                AbilityDamageHealsModel model = BuildService.buildAbilityDmgHealModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityDmgHealMessage("Insert ability_dmg_heal", model, true));
            }
            else if (selectedTable.Equals("war_world.ability_knockback_info"))
            {
                AbilityKnockBackInfoModel model = BuildService.buildAbilityKnockBackInfoModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityKnockbackInfoMessage("Insert ability_knockbackInfo", model, true));
            }
            else if (selectedTable.Equals("war_world.ability_modifiers"))
            {
                AbilityModifiersModel model = BuildService.buildAbilityModModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityModifierMessage("Insert ability_modifiers", model, fillAllAbsModifiersCmdNames(), true));
            }
            else if (selectedTable.Equals("war_world.ability_modifier_checks"))
            {
                AbilityModifierChecksModel model = BuildService.buildAbilityModChecksModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityModifierChecksMessage("Insert ability_modifierchecks", model,
                    fillAllAbsModifierCheckCommandNames(), fillAllAbsModChecksFailCodes(), true));
            }
            else if (selectedTable.Equals("war_world.buff_commands"))
            {
                AbilityBuffCommandsModel model = BuildService.buildAbilityBuffCommandsModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityBuffCmdMessage("Insert ability_buffCommands", model, fillAbsBuffCmds(), true));
            }
            else if (selectedTable.Equals("war_world.buff_infos"))
            {
                AbilityBuffInfosModel model = BuildService.buildAbilityBuffInfosModelFromDto(null);
                MessengerInstance.Send(new OpenAbilityBuffInfoMessage("Insert ability_buffInfos", model, true));

            }
        }
        #endregion

        #region update/insert abssingle
        private ObservableCollection<string> fillSpeclines()
        {
            List<string> result = clientBackEndService.GetAllAbilitySpeclines();
            ObservableCollection<string> speclines = new ObservableCollection<string>();
            result?.ForEach(res =>
            {
                if (res != null)
                    speclines.Add(res);
            });

            return speclines;
        }

        private ObservableCollection<string> fillCareerLines()
        {
            List<string> result = clientBackEndService.GetAllAbilityCareerLines();
            ObservableCollection<string> acl = new ObservableCollection<string>();
            result?.ForEach(res =>
            {
                if (res != null)
                {
                    switch (res)
                    {
                        case "0":
                            acl.Add("All");
                            break;
                        case "1":
                            acl.Add("Iron Breaker");
                            break;
                        case "2":
                            acl.Add("Slayer");
                            break;
                        case "4":
                            acl.Add("Rune Priest");
                            break;
                        case "8":
                            acl.Add("Engineer");
                            break;
                        case "16":
                            acl.Add("Black Orc");
                            break;
                        case "32":
                            acl.Add("Choppa");
                            break;
                        case "64":
                            acl.Add("Shaman");
                            break;
                        case "128":
                            acl.Add("Squig Herder");
                            break;
                        case "256":
                            acl.Add("Witch Hunter");
                            break;
                        case "512":
                            acl.Add("Knight of the Blazing Sun");
                            break;
                        case "1024":
                            acl.Add("Bright Wizard");
                            break;
                        case "2048":
                            acl.Add("Warrior Priest");
                            break;
                        case "4096":
                            acl.Add("Chosen");
                            break;
                        case "8192":
                            acl.Add("Marauder");
                            break;
                        case "16384":
                            acl.Add("Zealot");
                            break;
                        case "32768":
                            acl.Add("Magus");
                            break;
                        case "65536":
                            acl.Add("Sword Master");
                            break;
                        case "131072":
                            acl.Add("Shadow Warrior");
                            break;
                        case "262144":
                            acl.Add("White Lion");
                            break;
                        case "524288":
                            acl.Add("Archmage");
                            break;
                        case "1048576":
                            acl.Add("Black Guard");
                            break;
                        case "2097152":
                            acl.Add("Witch Elf");
                            break;
                        case "4194304":
                            acl.Add("Disciple Of Khaine");
                            break;
                        case "8388608":
                            acl.Add("Sorceress");
                            break;
                        case "25":
                            acl.Add("Pet - Lion");
                            break;
                        case "26":
                            acl.Add("Pet - Squig");
                            break;
                        case "27":
                            acl.Add("Pet - Horned Squig");
                            break;
                        case "28":
                            acl.Add("Pet - Gas Squig");
                            break;
                        case "29":
                            acl.Add("Pet - Spiked Squig");
                            break;
                        default:
                            acl.Add("undefined");
                            break;
                    }
                }
            });

            return acl;
        }

        private ObservableCollection<string> fillAbilityTypes()
        {
            List<string> result = clientBackEndService.GetAllAbilityTypes();
            ObservableCollection<string> at = new ObservableCollection<string>();
            result?.ForEach(res =>
            {
                if (res != null)
                {
                    switch (res)
                    {
                        case "0":
                            at.Add("None");
                            break;
                        case "1":
                            at.Add("Melee");
                            break;
                        case "2":
                            at.Add("Ranged");
                            break;
                        case "3":
                            at.Add("Verbal");
                            break;
                        case "255":
                            at.Add("Effect");
                            break;
                        default:
                            break;
                    }
                }
            });

            return at;
        }

        private ObservableCollection<string> fillMasteryTrees()
        {
            List<string> result = clientBackEndService.GetAllAbilityMasteryTrees();
            ObservableCollection<string> mt = new ObservableCollection<string>();
            result?.ForEach(res =>
            {
                if (mt != null)
                    mt.Add(res);
            });

            return mt;
        }
        #endregion

        #region update/insert abs command
        private ObservableCollection<string> fillAllAbsCommandTargets()
        {
            ObservableCollection<string> targets = new ObservableCollection<string>();
            List<string> result = clientBackEndService.GetAllAbsCommandTargets();
            result?.ForEach(target =>
            {
                if (target != null)
                    targets.Add(target);
            });
            return targets;
        }

        private ObservableCollection<string> fillAllAbsCommandEffectSource()
        {
            ObservableCollection<string> effectSources = new ObservableCollection<string>();
            List<string> result = clientBackEndService.GetAllAbsCommandEffectSource();
            result?.ForEach(effectSource =>
            {
                if (effectSource != null)
                    effectSources.Add(effectSource);
            });
            return effectSources;
        }

        private ObservableCollection<string> fillAllAbsCommandNames()
        {
            ObservableCollection<string> cmdNames = new ObservableCollection<string>();
            List<string> result = clientBackEndService.GetAllAbsCommandNames();
            result?.ForEach(cmdName =>
            {
                if (cmdName != null)
                    cmdNames.Add(cmdName);
            });
            return cmdNames;
        }
        #endregion

        #region update/insert abs modifierchecks
        private ObservableCollection<string> fillAllAbsModifierCheckCommandNames()
        {
            ObservableCollection<string> cmdNames = new ObservableCollection<string>();
            List<string> result = clientBackEndService.GetAllCommandNames();
            result?.ForEach(cmdName =>
            {
                if (!string.IsNullOrEmpty(cmdName))
                    cmdNames.Add(cmdName);
            });
            return cmdNames;
        }

        private ObservableCollection<string> fillAllAbsModChecksFailCodes()
        {
            ObservableCollection<string> failCodes = new ObservableCollection<string>();
            List<string> result = clientBackEndService.GetAllFailCodes();
            result?.ForEach(fc =>
            {
                failCodes.Add(fc);
            });

            return failCodes;
        }
        #endregion

        #region update/insert abs modifiers getting cmdNames
        private ObservableCollection<string> fillAllAbsModifiersCmdNames()
        {
            ObservableCollection<string> cmdNames = new ObservableCollection<string>();
            List<string> result = clientBackEndService.GetAllModifierCommandNames();
            result?.ForEach(cmdName =>
            {
                if (cmdName != null)
                    cmdNames.Add(cmdName);
            });
            return cmdNames;
        }
        #endregion

        #region update/insert abs buff command
        private ObservableCollection<string> fillAbsBuffCmds()
        {
            ObservableCollection<string> cmdNames = new ObservableCollection<string>();
            List<string> result = clientBackEndService.GetAllBuffCommandNames();
            result?.ForEach(res =>
            {
                cmdNames.Add(res);
            });

            return cmdNames;
        }
        #endregion

        private void fireSearch()
        {
            abilitiesSingle.Clear();
            List<AbilitySingle> searchResult = clientBackEndService.searchAllAbilities();
            handleResultSearch(searchResult);
            MessengerInstance.Send(new OpenSearchResultAllMessage(abilitiesSingle, "searchresult of all abilities"));
        }

        private void fireSearchByEntry(string entry)
        {
            if (string.IsNullOrEmpty(entry))
            {
                Logger.Error("Input Entry was null or empty!");
                return;
            }

            abilitiesSingle.Clear();
            List<AbilitySingle> searchResult = clientBackEndService.searchAbilityByEntry(int.Parse(entry));
            handleResultSearch(searchResult);
            MessengerInstance.Send(new OpenSearchResultEntryMessage(abilitiesSingle, "searchresult of abilities with given entry"));
        }

        private void fireSearchForNonNPCAbs()
        {
            abilitiesSingle.Clear();
            List<AbilitySingle> searchResult = clientBackEndService.searchPlayerAbility();
            handleResultSearch(searchResult);
            MessengerInstance.Send(new OpenSearchResultPlayerMessage(abilitiesSingle, "searchresult of player abilities"));
        }

        private void fireSearchForNPCAbs()
        {
            abilitiesSingle.Clear();
            List<AbilitySingle> searchResult = clientBackEndService.searchNPCAbility();
            handleResultSearch(searchResult);
            MessengerInstance.Send(new OpenSearchResultNpcMessage(abilitiesSingle, "searchresult of npc abilities"));
        }

        #endregion

        #region Property change
        public bool isReady => !string.IsNullOrEmpty(Entry);
        #endregion

        #region class methods
        private void InitializeProperties()
        {
            abilitiesSingle = new ObservableCollection<AbilitySingleModel>();
            tableNames = new ObservableCollection<string>();
            searchAllAbilitiesCommand = new RelayCommand(fireSearch);
            searchPlayerAbilitiesCommand = new RelayCommand(fireSearchForNonNPCAbs);
            searchNpcAbilitiesCommand = new RelayCommand(fireSearchForNPCAbs);
            searchAbilityByEntryCommand = new Command(
                () => fireSearchByEntry(this.Entry),
                () => !string.IsNullOrEmpty(this.Entry));
            openUpdateAbsCommand = new Command(
                () => fireUpdateAbility(this.Entry, this.selectedTableToUpdate),
                () => !string.IsNullOrEmpty(this.Entry) && this.selectedTableToUpdate != null);
            openInsertAbsCommand = new Command(
                () => fireInsertAbility(this.selectedTableToInsert),
                () => !string.IsNullOrEmpty(this.selectedTableToInsert));
        }

        private void handleResultSearch(List<AbilitySingle> searchResult)
        {
            if (searchResult == null)
            {
                Logger.Error(msgError);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msgError));
            }
            else
            {
                searchResult.ForEach(item =>
                {
                    abilitiesSingle.Add(BuildService.buildAbilitySingleModelFromDto(item));
                });
                Logger.Info(msgDefault);
            }
        }

        private void fillTableNames()

        {
            tableNames.Add("war_world.abilities");
            tableNames.Add("war_world.ability_commands");
            tableNames.Add("war_world.ability_damage_heals");
            tableNames.Add("war_world.ability_knockback_info");
            tableNames.Add("war_world.ability_modifiers");
            tableNames.Add("war_world.ability_modifier_checks");
            tableNames.Add("war_world.buff_commands");
            tableNames.Add("war_world.buff_infos");
        }
        #endregion
    }
}