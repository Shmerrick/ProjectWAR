using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenAbilityBuffInfoMessage : IOpenAbilityMessage
    {
        #region properties
        public string title { get; set; }
        public AbilityBuffInfosModel model { get; set; }
        public bool isInsertType { get; set; }
        #endregion

        public OpenAbilityBuffInfoMessage(string title, AbilityBuffInfosModel model, bool isInsertType)
        {
            this.title = title;
            this.model = model;
            this.isInsertType = isInsertType;
        }
    }
}
