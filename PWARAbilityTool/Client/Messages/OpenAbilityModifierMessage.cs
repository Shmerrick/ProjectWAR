using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenAbilityModifierMessage : IOpenAbilityMessage
    {
        #region properties
        public string title { get; set; }
        public AbilityModifiersModel model { get; set; }
        public ObservableCollection<string> commandNames { get; set; }
        public bool isInsertType { get; set; }
        #endregion

        public OpenAbilityModifierMessage(string title, AbilityModifiersModel model, ObservableCollection<string> cmdNames, bool isInsertType)
        {
            this.title = title;
            this.model = model;
            this.commandNames = cmdNames;
            this.isInsertType = isInsertType;
        }
    }
}
