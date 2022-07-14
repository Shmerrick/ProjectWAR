using System.Collections.Generic;

namespace PWARAbilityTool.Dtos
{
    public class AbilityBuffCommands
    {
        public int Entry { get; set; }
        public string Name { get; set; }
        public int CommandID { get; set; }
        public int CommandSequence { get; set; }
        public string CommandName { get; set; }
        public int PrimaryValue { get; set; }
        public int SecondaryValue { get; set; }
        public int TertiaryValue { get; set; }
        public int InvokeOn { get; set; }
        public string Target { get; set; }
        public string EffectSource { get; set; }
        public int EffectRadius { get; set; }
        public int EffectAngle { get; set; }
        public int MaxTargets { get; set; }
        public string EventIDString { get; set; }
        public string EventCheck { get; set; }
        public int EventCheckParam { get; set; }
        public int EventChance { get; set; }
        public int ConsumesStack { get; set; }
        public int RetriggerInterval { get; set; }
        public int BuffLine { get; set; }
        public int NoAutoUse { get; set; }
        public string BuffClassString { get; set; }

        public List<string> toUpdateMembers { get; set; }
        public string Display => Name + ", " + Entry.ToString() + ", " + CommandID;
    }
}
