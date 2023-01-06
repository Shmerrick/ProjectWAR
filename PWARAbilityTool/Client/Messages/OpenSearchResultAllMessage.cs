using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenSearchResultAllMessage
    {
        #region properties
        public ObservableCollection<AbilitySingleModel> abilitiesFound { get; private set; }
        public string title { get; private set; }
        #endregion

        public OpenSearchResultAllMessage(ObservableCollection<AbilitySingleModel> searchResult, string title)
        {
            this.abilitiesFound = searchResult;
            this.title = title;
        }
    }
}
