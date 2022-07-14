using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenUpdateAbilityModifierMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityModifiersModel model { get; private set; }
        public ObservableCollection<string> commandNames { get; private set; }
        #endregion

        public OpenUpdateAbilityModifierMessage(string title, AbilityModifiersModel model, ObservableCollection<string> cmdNames)
        {
            this.title = title;
            this.model = model;
            this.commandNames = cmdNames;
        }
    }
}
