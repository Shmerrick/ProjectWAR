using AbilityTool.Client.Models;
using System.Collections.ObjectModel;

namespace AbilityTool.Client.Messages
{
    public class OpenUpdateAbilityMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilitySingleModel model { get; private set; }
        public ObservableCollection<string> speclines { get; set; }
        public ObservableCollection<string> careerLines { get; set; }
        public ObservableCollection<string> abilityTypes { get; set; }
        public ObservableCollection<string> masteryTrees { get; set; }
        #endregion

        public OpenUpdateAbilityMessage(string title, AbilitySingleModel model, ObservableCollection<string> speclines, ObservableCollection<string> careerLines,
            ObservableCollection<string> abilityTypes, ObservableCollection<string> masteryTrees)
        {
            this.title = title;
            this.model = model;
            this.speclines = speclines;
            this.careerLines = careerLines;
            this.abilityTypes = abilityTypes;
            this.masteryTrees = masteryTrees;
        }
    }
}
