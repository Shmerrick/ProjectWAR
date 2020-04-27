using System;
using System.Collections.Generic;
using Common;
using FrameWork;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities.Components
{
    public class AbilityDamageInfo
    {
        public ushort Entry;

        public ushort DisplayEntry;

        public byte Index;

        // Damage
        public ushort MinDamage;
        public ushort DamageVariance;
        public float PrecalcDamage;
        public float Damage;
        public float PrecalcMitigation;
        public float Mitigation;
        public float Absorption;
        public byte DamageEvent;
        public bool IsAoE;

        public byte ParentCommandID, ParentCommandSequence;

        public float CastTimeDamageMult;

        public WeaponDamageContribution WeaponMod;
        public float WeaponDamageScale;

        public byte StatUsed;
        public float StatDamageScale;

        public float DamageBonus = 1;
        public float DamageReduction = 1;

        public byte CriticalHitRate;
        public float CriticalHitDamageBonus;

        /// <summary>
        /// This is set when a damage bonus which should not stack with other damage reductions is applied.
        /// </summary>
        public bool[] ExclusiveBonusApplied = new bool[5];

        /// <summary>
        /// This is set when a damage reduction which should not stack with other damage reductions is applied.
        /// </summary>
        public bool[] ExclusiveReductionApplied = new bool[5];

        public DamageTypes DamageType;

        public SubDamageTypes SubDamageType;

        public bool IsHeal => DamageType == DamageTypes.Healing || DamageType == DamageTypes.RawHealing;

        /// <summary>
        /// If true, includes the mitigated and absorbed damage in the component result for later components to use.
        /// </summary>
        public bool ResultFromRaw;

        public bool NoCrits;
        public bool Undefendable;

        /// <summary>
        /// <para>Used to force a certain DamageEvent when an ability is defended.</para>
        /// <para>Abilities such as Throw, Wrath of Hoeth and Judgment shouldn't appear as "parried", for example.</para>
        /// </summary>
        public byte OverrideDefenseEvent;

        public float ArmorResistPenFactor;

        public ushort MinArmorResistPen;
        public ushort MaxArmorResistPen;

        public float HatredScale = 1.0f;

        public float HealHatredScale = 1.0f;

        public short ResourceBuild;

        public byte CastPlayerSubID;

        public int Defensibility;

        public bool WasLethalDamage;

        // Set if this damage was split by Guard, to ensure that lifetaps still pull the full heal amount.
        public int TransferFactor = 1;

        public byte MasteryTree;

        // Amount that this damage contributes for XP/RP sharing.
        public float ContributoryFactor = 1f;

        // Set if this damage should use a fraction of the Item Stat Total for its stat contribution.
        public bool UseItemStatTotal;

        //If defined, use the Primary Stat defined as a multiplier and override the weapon value to also use this statistic by calculating the DPS value of the primary stat plus the DPS value of the weapons used in conjunction with the base value for the skill.
        //
        public float PriStatMultiplier;

        #region Load

        public AbilityDamageInfo()
        {

        }

        public AbilityDamageInfo(DBAbilityDamageInfo dbObj)
        {
            Entry = dbObj.Entry;
            DisplayEntry = dbObj.DisplayEntry;
            Index = dbObj.Index;
            ParentCommandID = dbObj.ParentCommandID;
            ParentCommandSequence = dbObj.ParentCommandSequence;
            MinDamage = dbObj.MinDamage;
            WeaponDamageScale = dbObj.WeaponDamageScale;
            Undefendable = dbObj.Undefendable;
            NoCrits = dbObj.NoCrits;
            OverrideDefenseEvent = dbObj.OverrideDefenseEvent;
            StatUsed = dbObj.StatUsed;
            StatDamageScale = dbObj.StatDamageScale;
            ArmorResistPenFactor = dbObj.ArmorResistPenFactor;
            HatredScale = dbObj.HatredScale;
            HealHatredScale = dbObj.HealHatredScale;
            ResourceBuild = dbObj.ResourceBuild;
            CastPlayerSubID = dbObj.CastPlayerSubID;
            PriStatMultiplier = dbObj.PriStatMultiplier;


            if (!string.IsNullOrEmpty(dbObj.DamageType))
                DamageType = (DamageTypes)Enum.Parse(typeof(DamageTypes), dbObj.DamageType);
            DamageVariance = dbObj.DamageVariance;
            CastTimeDamageMult = dbObj.CastTimeDamageMult;
            if (!string.IsNullOrEmpty(dbObj.WeaponDamageFrom))
                WeaponMod = (WeaponDamageContribution)Enum.Parse(typeof(WeaponDamageContribution), dbObj.WeaponDamageFrom);
        }

        public static List<AbilityDamageInfo> Convert(List<DBAbilityDamageInfo> dbObjs)
        {
            List<AbilityDamageInfo> dmgInfo = new List<AbilityDamageInfo>();

            foreach (DBAbilityDamageInfo dbObj in dbObjs)
                dmgInfo.Add(new AbilityDamageInfo(dbObj));

            return dmgInfo;
        }

        #endregion

        #region Interface

        public uint GetDamageForLevel(byte level)
        {
            uint damage = (uint)((MinDamage) * ((level - 1) / 39.0f));
            if (DamageVariance == 0)
                return damage;

            float percentageModifier = (StaticRandom.Instance.Next(DamageVariance * 2) - DamageVariance) * 0.01f;
            return (uint)(damage * (1f + percentageModifier));
        }

        public ushort GetArmorPenetrationForLevel(byte level)
        {
            return (ushort)(MinArmorResistPen + (MaxArmorResistPen - MinArmorResistPen) * ((level - 1) / 39.0f));
        }

        public void ApplyDamageModifiers(bool precalculated = false)
        {
            if (Math.Abs(DamageReduction - DamageBonus) > 0)
            {
                if (precalculated)
                {
                    PrecalcDamage *= DamageBonus * DamageReduction;
                    PrecalcMitigation *= DamageBonus * DamageReduction;
                }
                else
                {
                    Damage *= DamageBonus * DamageReduction;
                    Mitigation *= DamageBonus * DamageReduction;
                }
            }

            DamageBonus = 1;
            DamageReduction = 1;

            ExclusiveBonusApplied[0] = false;
            ExclusiveBonusApplied[1] = false;
            ExclusiveBonusApplied[2] = false;
            ExclusiveBonusApplied[3] = false;
            ExclusiveBonusApplied[4] = false;

            ExclusiveReductionApplied[0] = false;
            ExclusiveReductionApplied[1] = false;
            ExclusiveReductionApplied[2] = false;
            ExclusiveReductionApplied[3] = false;
            ExclusiveReductionApplied[4] = false;
        }

        public AbilityDamageInfo Clone()
        {
            AbilityDamageInfo cDmgInfo = (AbilityDamageInfo)MemberwiseClone();

            cDmgInfo.ExclusiveBonusApplied = new bool[5];
            cDmgInfo.ExclusiveReductionApplied = new bool[5];

            return cDmgInfo;
        }

        public AbilityDamageInfo Clone(Unit damageInstigator)
        {
            AbilityDamageInfo cDmgInfo = (AbilityDamageInfo)MemberwiseClone();

            cDmgInfo.ExclusiveBonusApplied = new bool[5];
            cDmgInfo.ExclusiveReductionApplied = new bool[5];

            damageInstigator.ModifyDamageOut(cDmgInfo);

            return cDmgInfo;
        }

        #endregion
    }
}