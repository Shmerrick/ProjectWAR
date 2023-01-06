using System.Collections.Generic;

namespace PWARAbilityTool.Dtos
{
    public class AbilityCommand
    {
        public int Entry { get; set; }
        public string AbilityName { get; set; }
        public int CommandID { get; set; }
        public int CommandSequence { get; set; }
        public string CommandName { get; set; }
        public int PrimaryValue { get; set; }
        public int SecondaryValue { get; set; }
        public string Target { get; set; }
        public int EffectRadius { get; set; }
        public int EffectAngle { get; set; }
        public string EffectSource { get; set; }
        public int FromAllTargets { get; set; }
        public int MaxTargets { get; set; }
        public int AttackingStat { get; set; }
        public int isDelayedEffect { get; set; }
        public int NoAutoUse { get; set; }

        public List<string> toUpdateMembers { get; set; }
        public string Display => AbilityName + ", " + Entry.ToString() + ", " + CommandName;
    }
}
