using System.Collections.Generic;

namespace PWARAbilityTool.Dtos
{
    public class AbilityDamageHeals
    {
        public int Entry { get; set; }
        public int DisplayEntry { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int DamageVariance { get; set; }
        public string DamageType { get; set; }
        public int ParentCommandID { get; set; }
        public int ParentCommandSequence { get; set; }
        public float CastTimeDamageMult { get; set; }
        public string WeaponDamageFrom { get; set; }
        public float WeaponDamageScale { get; set; }
        public int NoCrits { get; set; }
        public int Undefendable { get; set; }
        public int OverrideDefenseEvent { get; set; }
        public int StatUsed { get; set; }
        public float StatDamageScale { get; set; }
        public int RessourceBuild { get; set; }
        public int CastPlayerSubID { get; set; }
        public float ArmorResistPenFactor { get; set; }
        public float HatredScale { get; set; }
        public float HealHatredScale { get; set; }
        public float PriStatMultiplier { get; set; }

        public List<string> ToUpdateMembers { get; set; }
        public string Display => Name + ", " + Entry.ToString() + ", " + ParentCommandID.ToString() + ", " + ParentCommandSequence.ToString();
    }
}