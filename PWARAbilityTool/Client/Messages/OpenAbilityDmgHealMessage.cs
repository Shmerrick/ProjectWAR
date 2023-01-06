using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenAbilityDmgHealMessage : IOpenAbilityMessage
    {
        #region properties
        public string title { get; set; }
        public AbilityDamageHealsModel model { get; set; }
        public bool isInsertType { get; set; }
        #endregion

        public OpenAbilityDmgHealMessage(string title, AbilityDamageHealsModel model, bool isInsertType)
        {
            this.title = title;
            this.model = model;
            this.isInsertType = isInsertType;
        }
    }
}
