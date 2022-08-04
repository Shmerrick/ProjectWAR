using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenUpdateAbilityModifierChecksMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityModifierChecksModel model { get; private set; }
        public ObservableCollection<string> commandNames { get; private set; }
        public ObservableCollection<int> failCodes { get; private set; }
        #endregion

        public OpenUpdateAbilityModifierChecksMessage(string title, AbilityModifierChecksModel model,
            ObservableCollection<string> commandNames, ObservableCollection<int> failCodes)
        {
            this.title = title;
            this.model = model;
            this.commandNames = commandNames;
            this.failCodes = failCodes;
        }
    }
}
