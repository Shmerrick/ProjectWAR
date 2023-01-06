using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenAbilityModifierChecksMessage : IOpenAbilityMessage
    {
        #region properties
        public string title { get; set; }
        public AbilityModifierChecksModel model { get; set; }
        public ObservableCollection<string> cmdNames { get; set; }
        public ObservableCollection<string> failCodes { get; set; }
        public bool isInsertType { get; set; }
        #endregion

        public OpenAbilityModifierChecksMessage(string title, AbilityModifierChecksModel model,
            ObservableCollection<string> commandNames, ObservableCollection<string> failCodes, bool isInsertType)
        {
            this.title = title;
            this.model = model;
            this.cmdNames = commandNames;
            this.failCodes = failCodes;
            this.isInsertType = isInsertType;
        }
    }
}
