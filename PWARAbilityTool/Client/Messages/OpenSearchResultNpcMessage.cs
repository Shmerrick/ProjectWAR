using PWARAbilityTool.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenSearchResultNpcMessage
    {
        #region properties
        public ObservableCollection<AbilitySingleModel> abilitiesFound { get; private set; }
        public string title { get; private set; }
        #endregion

        public OpenSearchResultNpcMessage(ObservableCollection<AbilitySingleModel> searchResult, string title)
        {
            this.abilitiesFound = searchResult;
            this.title = title;
        }
    }
}
