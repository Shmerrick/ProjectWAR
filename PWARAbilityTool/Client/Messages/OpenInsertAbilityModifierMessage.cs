using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenInsertAbilityModifierMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityModifiersModel model { get; private set; }
        public ObservableCollection<string> commandNames { get; private set; }
        #endregion

        public OpenInsertAbilityModifierMessage(string title, AbilityModifiersModel model, ObservableCollection<string> commandNames)
        {
            this.title = title;
            this.model = model;
            this.commandNames = commandNames;
        }
    }
}
