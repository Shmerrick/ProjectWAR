using System.Collections.Generic;

namespace PWARAbilityTool.Dtos
{
    public class AbilityKnockBackInfo
    {
        public int Entry { get; set; }
        public int Id { get; set; }
        public int Angle { get; set; }
        public int Power { get; set; }
        public int RangeExtension { get; set; }
        public int GravMultiplier { get; set; }
        public int Unk { get; set; }

        public List<string> ToUpdateMembers { get; set; }
        public string Display => Entry.ToString() + ", " + Id.ToString();
    }
}