using AbilityTool.Client.Models;

namespace AbilityTool.Client.Messages
{
    public class OpenInsertAbilityKnockbackInfoMessage
    {
        #region properties
        public string title { get; private set; }
        public AbilityKnockBackInfoModel model { get; private set; }
        #endregion

        public OpenInsertAbilityKnockbackInfoMessage(string title, AbilityKnockBackInfoModel model)
        {
            this.title = title;
            this.model = model;
        }
    }
}
