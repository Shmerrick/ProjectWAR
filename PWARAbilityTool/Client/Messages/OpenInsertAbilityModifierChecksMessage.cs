using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenInsertAbilityModifierChecksMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityModifierChecksModel model { get; private set; }
        public ObservableCollection<string> cmdNames { get; private set; }
        public ObservableCollection<int> failCodes { get; private set; }
        #endregion

        public OpenInsertAbilityModifierChecksMessage(string title, AbilityModifierChecksModel model,
            ObservableCollection<string> cmdNames, ObservableCollection<int> failCodes)
        {
            this.title = title;
            this.model = model;
            this.cmdNames = cmdNames;
            this.failCodes = failCodes;
        }
    }
}
