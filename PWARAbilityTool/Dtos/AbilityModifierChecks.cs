using System.Collections.Generic;

namespace PWARAbilityTool.Dtos
{
    public class AbilityModifierChecks
    {
        public int Entry { get; set; }
        public string SourceAbility { get; set; }
        public int Affecting { get; set; }
        public string AffectedAbility { get; set; }
        public int PreOrPost { get; set; }
        public int ID { get; set; }
        public int Sequence { get; set; }
        public string CommandName { get; set; }
        public int FailCode { get; set; }
        public int PrimaryValue { get; set; }
        public int SecondaryValue { get; set; }
        public string ability_modifier_checks_ID { get; set; }

        public List<string> toUpdateMembers { get; set; }
        public string Display => Entry.ToString() + ", " + SourceAbility + ", " + PreOrPost.ToString() + ", " + ID.ToString() + ", " + Sequence.ToString();
    }
}
