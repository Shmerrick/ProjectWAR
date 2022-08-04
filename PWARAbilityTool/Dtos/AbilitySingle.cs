using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWARAbilityTool.Dtos
{
    public class AbilitySingle
    {
        public int Entry { get; set; }
        public int CareerLine { get; set; }
        public string Name { get; set; }
        public int MinRange { get; set; }
        public int Range { get; set; }
        public int CastTime { get; set; }
        public int Cooldown { get; set; }
        public int ApCost { get; set; }
        public int SpecialCost { get; set; }
        public int MoveCast { get; set; }
        public int InvokeDelay { get; set; }
        public int EffectDelay { get; set; }
        public int EffectID { get; set; }
        public int ChannelID { get; set; }
        public int CooldownEntry { get; set; }
        public int ToggleEntry { get; set; }
        public int CastAngle { get; set; }
        public int AbilityType { get; set; }
        public int MasteryTree { get; set; }
        public string Specline { get; set; }
        public int WeaponNeeded {get; set; }
        public int AffectsDead { get; set; }
        public int IgnoreGlobalCooldown { get; set; }
        public int IgnoreOwnModifiers { get; set; }
        public int Fragile { get; set; }
        public int MinimumRank { get; set; }
        public int MinimumRenown { get; set; }
        public int IconId { get; set; }
        public int Category { get; set; }
        public int Flags { get; set; }
        public int PointCost { get; set; }
        public int CashCost { get; set; }
        public int StealthInteraction { get; set; }
        public int AIRange { get; set; }
        public int IgnoreCooldownReduction { get; set; }
        public int CDcap { get; set; }
        public string VFXTarget { get; set; }
        public int abilityID { get; set; }
        public int effectID2 { get; set; }
        public int Time { get; set; }

        public List<string> toUpdateMembers { get; set; }
        public string Display => Name + ", " + Entry.ToString();
    }
}
