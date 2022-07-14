using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenUpdateAbilityBuffInfoMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityBuffInfosModel model { get; private set; }
        #endregion

        public OpenUpdateAbilityBuffInfoMessage(string title, AbilityBuffInfosModel model)
        {
            this.title = title;
            this.model = model;
        }
    }
}
