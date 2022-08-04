using PWARAbilityTool.Client.Messages;
using PWARAbilityTool.Client.Ui;
using PWARAbilityTool.Client.ViewModels;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Owin.Hosting;
using NLog;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using static PWARAbilityTool.Client.ViewModels.BaseViewModel;

namespace PWARAbilityTool
{
    /// <summary>
    /// Logic for Interacting with MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly bool alreadyStarted = false;
        private readonly string baseUrl = ConfigurationManager.AppSettings["base-url"];
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            //TODO: remove when not used locally ;)
            if (!alreadyStarted)
            {
                WebApp.Start<Startup>(baseUrl);
                alreadyStarted = true;
            }

            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;

            Authentification authentification = new Authentification();
            if (authentification.ShowDialog() != true)
            {
                Close();
            }

            RegisterMessages();
        }

        #region methods

        private void RegisterMessages()
        {
            #region search abilities messages

            Messenger.Default.Register<OpenSearchResultAllMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                SearchResultPresenter window = new SearchResultPresenter();
                if (window.DataContext is SearchAbilityViewModel model)
                {
                    model.abilitiesSingle = msg.abilitiesFound;
                    model.WindowTitle = msg.title;
                    Logger.Info("======================Model changed!=========================");
                    Logger.Info("=====================I have now " + model.abilitiesSingle.Count + " items!============================");
                }

                window.ShowDialog();
            });

            Messenger.Default.Register<OpenSearchResultEntryMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                SearchResultPresenter window = new SearchResultPresenter();
                if (window.DataContext is SearchAbilityViewModel model)
                {
                    model.abilitiesSingle = msg.abilitiesFound;
                    model.WindowTitle = msg.title;
                    Logger.Info("======================Model changed!=========================");
                    Logger.Info("=====================I have now " + model.abilitiesSingle.Count + " items!============================");
                }

                window.ShowDialog();
            });

            Messenger.Default.Register<OpenSearchResultPlayerMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                SearchResultPresenter window = new SearchResultPresenter();
                if (window.DataContext is SearchAbilityViewModel model)
                {
                    model.abilitiesSingle = msg.abilitiesFound;
                    model.WindowTitle = msg.title;
                    Logger.Info("======================Model changed!=========================");
                    Logger.Info("=====================I have now " + model.abilitiesSingle.Count + " items!============================");
                }

                window.ShowDialog();
            });

            Messenger.Default.Register<OpenSearchResultNpcMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                SearchResultPresenter window = new SearchResultPresenter();
                if (window.DataContext is SearchAbilityViewModel model)
                {
                    model.abilitiesSingle = msg.abilitiesFound;
                    model.WindowTitle = msg.title;
                    Logger.Info("======================Model changed!=========================");
                    Logger.Info("=====================I have now " + model.abilitiesSingle.Count + " items!============================");
                }

                window.ShowDialog();
            });

            #endregion search abilities messages

            #region update/insert abilities messages

            Messenger.Default.Register<OpenAbilityMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesPage window = createNewAbsWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            Messenger.Default.Register<OpenAbilityCommandMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesCommandPage window = createNewAbsCmdWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            Messenger.Default.Register<OpenAbilityDmgHealMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesDmgHealsPage window = createNewAbsDmgHealWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            Messenger.Default.Register<OpenAbilityKnockbackInfoMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesKnockBackInfoPage window = createNewAbsKnockBackWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            Messenger.Default.Register<OpenAbilityModifierMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesModifiersPage window = createNewAbsModsWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            Messenger.Default.Register<OpenAbilityModifierChecksMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesModifierChecksPage window = createNewAbsModChecksWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            Messenger.Default.Register<OpenAbilityBuffInfoMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesBuffInfoPage window = createNewBuffInfoWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            Messenger.Default.Register<OpenAbilityBuffCmdMessage>(this, msg =>
            {
                Logger.Info("=====================Message received!=================================");
                ViewModelMode viewModelMode = ViewModelMode.Insert;
                if (!msg.isInsertType)
                {
                    viewModelMode = ViewModelMode.Update;
                }

                WarAbilitiesBuffCommandsPage window = createNewBuffCmdWindow(msg, viewModelMode);
                window.ShowDialog();
            });

            #endregion update/insert abilities messages
        }

        #region update/insert abs single window creation

        private WarAbilitiesPage createNewAbsWindow(OpenAbilityMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesPage window = new WarAbilitiesPage();
            if (window.DataContext is AbilitiesSingleViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.abilityTypes = msg.abilityTypes;
                viewModel.careerLines = msg.careerLines;
                viewModel.masteryTrees = msg.masteryTrees;
                viewModel.speclines = msg.speclines;
                viewModel.AbilitySingleModel = msg.model;
                viewModel.Type = ViewModelType.AbilitySingle;
                viewModel.Mode = viewModelMode;
                viewModel.AbilitySingleModel.ToUpdateMembers = new List<string>();
            }
            return window;
        }

        #endregion update/insert abs single window creation

        #region update/insert abs command window creation

        private WarAbilitiesCommandPage createNewAbsCmdWindow(OpenAbilityCommandMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesCommandPage window = new WarAbilitiesCommandPage();
            if (window.DataContext is AbilitiesCommandViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.targets = msg.targets;
                viewModel.effectSources = msg.effectSources;
                viewModel.cmdNames = msg.cmdNames;
                viewModel.AbilityCommandModel = msg.model;
                viewModel.Type = ViewModelType.AbilityCommand;
                viewModel.Mode = viewModelMode;
                viewModel.AbilityCommandModel.ToUpdateMembers = new List<string>();
            }
            return window;
        }

        #endregion update/insert abs command window creation

        #region update/insert abs dmg heal window creation

        private WarAbilitiesDmgHealsPage createNewAbsDmgHealWindow(OpenAbilityDmgHealMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesDmgHealsPage window = new WarAbilitiesDmgHealsPage();
            if (window.DataContext is AbilitiesDmgHealViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.AbilityDamageHealsModel = msg.model;
                viewModel.Type = ViewModelType.AbilityDmgHeal;
                viewModel.Mode = viewModelMode;
                viewModel.AbilityDamageHealsModel.ToUpdateMembers = new List<string>();
            }
            return window;
        }

        #endregion update/insert abs dmg heal window creation

        #region update/insert abs knockbackinfo window creation

        private WarAbilitiesKnockBackInfoPage createNewAbsKnockBackWindow(OpenAbilityKnockbackInfoMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesKnockBackInfoPage window = new WarAbilitiesKnockBackInfoPage();
            if (window.DataContext is AbilitiesKnockBackInfoViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.AbilityKnockBackInfoModel = msg.model;
                viewModel.Type = ViewModelType.AbilityKnockBack;
                viewModel.Mode = viewModelMode;
                viewModel.AbilityKnockBackInfoModel.ToUpdateMembers = new List<string>();
            }

            return window;
        }

        #endregion update/insert abs knockbackinfo window creation

        #region update/insert abs modifier window creation

        private WarAbilitiesModifiersPage createNewAbsModsWindow(OpenAbilityModifierMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesModifiersPage window = new WarAbilitiesModifiersPage();
            if (window.DataContext is AbilitiesModifiersViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.AbilityModifiersModel = msg.model;
                viewModel.commandNames = msg.commandNames;
                viewModel.Type = ViewModelType.AbilityModifiers;
                viewModel.Mode = viewModelMode;
                viewModel.AbilityModifiersModel.ToUpdateMembers = new List<string>();
            }

            return window;
        }

        #endregion update/insert abs modifier window creation

        #region update/insert abs modifier checks window creation

        private WarAbilitiesModifierChecksPage createNewAbsModChecksWindow(OpenAbilityModifierChecksMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesModifierChecksPage window = new WarAbilitiesModifierChecksPage();
            if (window.DataContext is AbilitiesModifierChecksViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.AbilityModifierChecksModel = msg.model;
                viewModel.commandNames = msg.cmdNames;
                viewModel.failCodes = msg.failCodes;
                viewModel.Type = ViewModelType.AbilityModifierChecks;
                viewModel.Mode = viewModelMode;
                viewModel.AbilityModifierChecksModel.ToUpdateMembers = new List<string>();
            }

            return window;
        }

        #endregion update/insert abs modifier checks window creation

        #region update/insert abs buff info window creation

        private WarAbilitiesBuffInfoPage createNewBuffInfoWindow(OpenAbilityBuffInfoMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesBuffInfoPage window = new WarAbilitiesBuffInfoPage();
            if (window.DataContext is AbilitiesBuffInfoViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.AbilityBuffInfosModel = msg.model;
                viewModel.Type = ViewModelType.AbilityBuffInfos;
                viewModel.Mode = viewModelMode;
                viewModel.AbilityBuffInfosModel.ToUpdateMembers = new List<string>();
            }

            return window;
        }

        #endregion update/insert abs buff info window creation

        #region update/insert abs buff cmd window creation

        private WarAbilitiesBuffCommandsPage createNewBuffCmdWindow(OpenAbilityBuffCmdMessage msg, ViewModelMode viewModelMode)
        {
            WarAbilitiesBuffCommandsPage window = new WarAbilitiesBuffCommandsPage();
            if (window.DataContext is AbilitiesBuffCommandViewModel viewModel)
            {
                viewModel.WindowTitle = msg.title;
                viewModel.AbilityBuffCommandsModel = msg.model;
                viewModel.commandNames = msg.cmdNames;
                viewModel.Type = ViewModelType.AbilityBuffCommands;
                viewModel.Mode = viewModelMode;
                viewModel.AbilityBuffCommandsModel.ToUpdateMembers = new List<string>();
            }

            return window;
        }

        #endregion update/insert abs buff cmd window creation

        #endregion methods
    }
}