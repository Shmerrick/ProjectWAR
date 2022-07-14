using System.Collections.Generic;

namespace PWARAbilityTool.Dtos
{
    public class AbilityModifiers
    {
        public int Entry { get; set; }
        public string SourceAbility { get; set; }
        public int Affecting { get; set; }
        public string AffectedAbility { get; set; }
        public int PreOrPost { get; set; }
        public int Sequence { get; set; }
        public string ModifierCommandName { get; set; }
        public int TargetCommandID { get; set; }
        public int TargetCommandSequence { get; set; }
        public int PrimaryValue { get; set; }
        public int SecondaryValue { get; set; }
        public int BuffLine { get; set; }
        public string ability_modifiers_ID { get; set; }

        public List<string> toUpdateMembers { get; set; }
        public string Display => Entry.ToString() + ", " + SourceAbility + ", " + Sequence.ToString();
    }
}
