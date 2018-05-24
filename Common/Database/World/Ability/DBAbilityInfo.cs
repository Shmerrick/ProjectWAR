using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "abilities", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class DBAbilityInfo : DataObject
    {
        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement]
        public uint CareerLine { get; set; }

        [DataElement(Varchar = 255)]
        public string Name { get; set; }

        [DataElement]
        public byte MinRange { get; set; }

        [DataElement]
        public ushort Range { get; set; }

        [DataElement]
        public ushort CastTime { get; set; }

        [DataElement]
        public ushort Cooldown { get; set; }

        [DataElement]
        public byte ApCost { get; set; }

        /// <summary>
        /// Used for morale and career resource costs
        /// </summary>
        [DataElement]
        public short SpecialCost { get; set; }

        [DataElement]
        public bool MoveCast { get; set; } = false;

        [DataElement]
        public byte MinimumRank { get; set; }

        [DataElement]
        public byte MinimumRenown { get; set; }

        [DataElement]
        public ushort IconId { get; set; }

        [DataElement(Varchar = 255)]
        public string Specline { get; set; }

        [DataElement]
        public byte MasteryTree { get; set; }

        [DataElement]
        public byte Category { get; set; }

        [DataElement]
        public ushort Flags { get; set; }

        [DataElement]
        public byte PointCost { get; set; }

        [DataElement]
        public uint CashCost { get; set; }

        /// <summary>
        ///  None - Melee - Ranged - Magical
        /// </summary>
        [DataElement]
        public byte AbilityType { get; set; }

        [DataElement]
        public ushort ChannelID { get; set; }

        [DataElement]
        public ushort CastAngle { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort EffectID { get; set; }

        [DataElement]
        public byte WeaponNeeded { get; set; }

        [DataElement]
        public ushort InvokeDelay { get; set; }

        /// <summary>
        /// <para>The abs value of this is the delay (in ms) required before commands with IsDelayedEffect will execute.</para>
        /// <para>If positive, the value will be multiplied by (Range/100).</para>
        /// <para>If negative, the absolute value is taken with no further adjustment.</para>
        /// </summary>
        [DataElement]
        public short EffectDelay { get; set; }

        [DataElement]
        public bool IgnoreGlobalCooldown { get; set; } = false;

        [DataElement]
        public bool AffectsDead { get; set; } = false;

        [DataElement]
        public int StealthInteraction { get; set; }

        [DataElement]
        public byte Fragile { get; set; }

        [DataElement]
        public ushort CooldownEntry { get; set; }

        [DataElement]
        public ushort ToggleEntry { get; set; }

        [DataElement]
        public bool IgnoreOwnModifiers { get; set; }

        [DataElement]
        public ushort AIRange { get; set; }

        /// <summary>
        /// if this is set it will ignore any modifications to the cooldown
        /// </summary>
        [DataElement]
        public ushort IgnoreCooldownReduction { get; set; }

        [DataElement]
        public ushort CDcap { get; set; }

        [DataElement(Varchar = 255)]
        public string VFXTarget { get; set; }

        [DataElement]
        public ushort abilityID { get; set; }

        [DataElement]
        public ushort effectID2 { get; set; }

        [DataElement]
        public ushort Time { get; set; }
    }
}
