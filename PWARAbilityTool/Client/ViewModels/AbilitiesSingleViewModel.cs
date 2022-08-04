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
    public class AbilitiesSingleViewModel : BaseViewModel
    {
        #region binding properties
        #region selected items of cb´s 
        private string selectedSpecline;
        public string SelectedSpecline
        {
            get => selectedSpecline;
            set
            {
                selectedSpecline = value;
                OnPropertyChanged("SelectedSpecline");
            }
        }

        private string selectedCareerLine;
        public string SelectedCareerLine
        {
            get => selectedCareerLine;
            set
            {
                selectedCareerLine = value;
                OnPropertyChanged("SelectedCareerLine");
            }
        }

        private string selectedAbilityType;
        public string SelectedAbilityType
        {
            get => selectedAbilityType;
            set
            {
                selectedAbilityType = value;
                OnPropertyChanged("SelectedAbilityType");
            }
        }

        private string selectedMasteryTree;
        public string SelectedMasteryTree
        {
            get => selectedMasteryTree;
            set
            {
                selectedMasteryTree = value;
                OnPropertyChanged("SelectedMasteryTree");
            }
        }
        #endregion

        #region observable combobox datasources
        public ObservableCollection<string> speclines { get; set; }
        public ObservableCollection<string> careerLines { get; set; }
        public ObservableCollection<string> abilityTypes { get; set; }
        public ObservableCollection<string> masteryTrees { get; set; }
        #endregion
        #endregion

        #region class properties
        private AbilitySingleModel abilitySingleModel;
        public AbilitySingleModel AbilitySingleModel
        {
            get => abilitySingleModel;
            set
            {
                if (abilitySingleModel != value)
                {
                    abilitySingleModel = value;
                }
            }
        }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region commands
        public ICommand SaveCommand { get; private set; }
        #endregion

        public AbilitiesSingleViewModel()
        {
            WindowTitle = "abilities";
            Name = "AbilitiesSingleViewModel";
            AbilitySingleModel = new AbilitySingleModel();
            SaveCommand = new RelayCommand(fireUpdateInsert);
            InitializeComboProperties();
        }

        #region methods
        private void InitializeComboProperties()
        {
            speclines = new ObservableCollection<string>();
            careerLines = new ObservableCollection<string>();
            abilityTypes = new ObservableCollection<string>();
            masteryTrees = new ObservableCollection<string>();
        }

        private void fireUpdateInsert()
        {
            AbilitySingle absSingleDto = BuildService.buildAbilitySingleDtoFromModel(this.AbilitySingleModel);
            switch (Mode)
            {
                case ViewModelMode.Update:
                    HandleUpdateMode(absSingleDto);
                    break;
                case ViewModelMode.Insert:
                    HandleInsertMode(absSingleDto);
                    break;
                default:
                    break;
            }
        }

        private void HandleUpdateMode(AbilitySingle absSingleDto)
        {
            if (!ValidationService.ValidateToUpdateMembers(absSingleDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.updateAbilitySingle(this.AbilitySingleModel.Entry, absSingleDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to update ability: " + this.AbilitySingleModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("update successful!"));
        }

        private void HandleInsertMode(AbilitySingle absSingleDto)
        {
            if (!ValidationService.ValidateAbsSingle(this.AbilitySingleModel) && !ValidationService.ValidateToUpdateMembers(absSingleDto.toUpdateMembers))
                return;

            HttpResponseMessage response = clientBackEndService.insertAbilitySingle(absSingleDto);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = "failed to insert new ability: " + this.AbilitySingleModel.Name;
                Logger.Error(msg);
                ViewModelLocator.NotifyUserMethod(new NotificationMessage(msg));
            }
            else
                ViewModelLocator.NotifyUserMethod(new NotificationMessage("insert successful!"));
        }
        #endregion
    }
}
