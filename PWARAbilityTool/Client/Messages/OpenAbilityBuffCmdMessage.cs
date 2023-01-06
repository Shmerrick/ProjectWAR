using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenAbilityBuffCmdMessage : IOpenAbilityMessage
    {
        #region properties
        public string title { get; set; }
        public AbilityBuffCommandsModel model { get; set; }
        public ObservableCollection<string> cmdNames { get; set; }
        public bool isInsertType { get; set; }
        #endregion

        public OpenAbilityBuffCmdMessage(string title, AbilityBuffCommandsModel model, ObservableCollection<string> cmdNames, bool isInsertType)
        {
            this.title = title;
            this.model = model;
            this.cmdNames = cmdNames;
            this.isInsertType = isInsertType;
        }
    }
}
