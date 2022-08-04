using System.Collections.Generic;

namespace PWARAbilityTool.Dtos
{
    public class AbilityBuffInfos
    {
        public int Entry { get; set; }
        public string Name { get; set; }
        public string BuffClassString { get; set; }
        public string TypeString { get; set; }
        public int Group { get; set; }
        public string AuraPropagation { get; set; }
        public int MaxCopies { get; set; }
        public int MaxStack { get; set; }
        public int UserMaxStackAsInitial { get; set; }
        public int StackLine { get; set; }
        public int StacksFromCaster { get; set; }
        public int Duration { get; set; }
        public int LeadInDelay { get; set; }
        public int Interval { get; set; }
        public int PersistsOnDeath { get; set; }
        public int CanRefresh { get; set; }
        public int FriendlyEffectID { get; set; }
        public int EnemyEffectID { get; set; }
        public int Silent { get; set; }

        public List<string> toUpdateMembers { get; set; }
        public string Display => Name + ", " + Entry.ToString();
    }
}
