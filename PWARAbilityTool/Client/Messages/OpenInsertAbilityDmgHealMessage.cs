using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenInsertAbilityDmgHealMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityDamageHealsModel model { get; private set; }
        #endregion

        public OpenInsertAbilityDmgHealMessage(string title, AbilityDamageHealsModel model)
        {
            this.title = title;
            this.model = model;
        }
    }
}
