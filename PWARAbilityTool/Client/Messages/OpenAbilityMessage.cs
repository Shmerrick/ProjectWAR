using PWARAbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace PWARAbilityTool.Client.Messages
{
    public class OpenAbilityMessage : IOpenAbilityMessage
    {
        #region properties
        public string title { get; set; }
        public AbilitySingleModel model { get; set; }
        public ObservableCollection<string> speclines { get; set; }
        public ObservableCollection<string> careerLines { get; set; }
        public ObservableCollection<string> abilityTypes { get; set; }
        public ObservableCollection<string> masteryTrees { get; set; }
        public bool isInsertType { get; set; }
        #endregion

        public OpenAbilityMessage(string title, AbilitySingleModel model, ObservableCollection<string> speclines, ObservableCollection<string> careerLines,
            ObservableCollection<string> abilityTypes, ObservableCollection<string> masteryTrees, bool isInsertType)
        {
            this.title = title;
            this.model = model;
            this.speclines = speclines;
            this.careerLines = careerLines;
            this.abilityTypes = abilityTypes;
            this.masteryTrees = masteryTrees;
            this.isInsertType = isInsertType;
        }
    }
}
