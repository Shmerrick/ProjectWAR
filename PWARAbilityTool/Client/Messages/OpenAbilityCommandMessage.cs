using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenAbilityCommandMessage : IOpenAbilityMessage
    {
        #region properties
        public string title { get; set; }
        public bool isInsertType { get; set; }
        public AbilityCommandModel model { get; set; }
        public ObservableCollection<string> targets { get; set; }
        public ObservableCollection<string> effectSources { get; set; }
        public ObservableCollection<string> cmdNames { get; set; }
        #endregion

        public OpenAbilityCommandMessage(string title, AbilityCommandModel model, ObservableCollection<string> targets, 
            ObservableCollection<string> effectSources, ObservableCollection<string> cmdNames, bool isInsertType)
        {
            this.title = title;
            this.model = model;
            this.targets = targets;
            this.effectSources = effectSources;
            this.cmdNames = cmdNames;
            this.isInsertType = isInsertType;
        }
    }
}
