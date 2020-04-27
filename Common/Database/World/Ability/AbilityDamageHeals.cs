using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ability_damage_heals", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class DBAbilityDamageInfo : DataObject
    {
        #region Database Elements

        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement]
        public ushort DisplayEntry { get; set; }

        [DataElement(Varchar = 36)]
        public string Name { get; set; }

        [PrimaryKey]
        public byte Index { get; set; }

        [PrimaryKey]
        public byte ParentCommandID { get; set; }

        [PrimaryKey]
        public byte ParentCommandSequence { get; set; }

        [DataElement]
        public ushort MinDamage { get; set; }

        [DataElement(Varchar = 16)]
        public string DamageType { get; set; }

        // Flurry, etc
        [DataElement]
        public ushort DamageVariance { get; set; }

        [DataElement]
        public float CastTimeDamageMult { get; set; }

        [DataElement(Varchar = 16)]
        public string WeaponDamageFrom { get; set; }

        [DataElement]
        public float WeaponDamageScale { get; set; }

        [DataElement]
        public bool NoCrits { get; set; }

        [DataElement]
        public bool Undefendable { get; set; }

        /// <summary>
        /// <para>Used to force a certain DamageEvent when an ability is defended.</para>
        /// <para>Abilities such as Throw, Wrath of Hoeth and Judgment shouldn't appear as "parried", for example.</para>
        /// </summary>
        [DataElement]
        public byte OverrideDefenseEvent { get; set; }

        /// <summary>
        /// The stat used to calculate damage bonus.
        /// </summary>
        [DataElement]
        public byte StatUsed { get; set; }

        [DataElement]
        public float StatDamageScale { get; set; }

        [DataElement]
        public float ArmorResistPenFactor { get; set; }

        [DataElement]
        public float HatredScale { get; set; } = 1.0f;

        [DataElement]
        public float HealHatredScale { get; set; } = 1.0f;

        [DataElement]
        public short ResourceBuild { get; set; }

        [DataElement]
        public byte CastPlayerSubID { get; set; }

        [DataElement]
        public float PriStatMultiplier { get; set; }

        #endregion
    }
}
