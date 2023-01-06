using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenInsertAbilityBuffInfoMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityBuffInfosModel model { get; private set; }
        #endregion

        public OpenInsertAbilityBuffInfoMessage(string title, AbilityBuffInfosModel model)
        {
            this.title = title;
            this.model = model;
        }
    }
}
