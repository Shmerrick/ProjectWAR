using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenUpdateAbilityCommandMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityCommandModel model { get; private set; }
        public ObservableCollection<string> targets { get; set; }
        public ObservableCollection<string> effectSources { get; set; }
        public ObservableCollection<string> cmdNames { get; set; }
        #endregion

        public OpenUpdateAbilityCommandMessage(string title, AbilityCommandModel model, ObservableCollection<string> targets, ObservableCollection<string> effectSources, ObservableCollection<string> cmdNames)
        {
            this.title = title;
            this.model = model;
            this.targets = targets;
            this.effectSources = effectSources;
            this.cmdNames = cmdNames;
        }
    }
}
