using System;
using System.Collections.Generic;
using Common;
using FrameWork;

namespace WorldServer
{
    public class AbilityDamageInfo
    {
        public ushort Entry;

        public ushort DisplayEntry;

        public byte Index;

        // Damage
        public ushort BaseDamage;
        public ushort Multiplier;
        public ushort DamageVariance;
        public float PrecalcDamage;
        public float Damage;
        public float PrecalcMitigation;
        public float Mitigation;
        public float Absorption;
        public byte DamageEvent;
        public bool IsAoE;

        public byte ParentCommandID, ParentCommandSequence;

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
            BaseDamage = dbObj.BaseDamage;
            if (!string.IsNullOrEmpty(dbObj.DamageType))
                DamageType = (DamageTypes)Enum.Parse(typeof(DamageTypes), dbObj.DamageType);
            DamageVariance = dbObj.DamageVariance;
             if (!string.IsNullOrEmpty(dbObj.WeaponDamageFrom))
                WeaponMod = (WeaponDamageContribution)Enum.Parse(typeof(WeaponDamageContribution), dbObj.WeaponDamageFrom);
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
            Multiplier = dbObj.Multiplier;
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
        /* Ability Use Example [SIMPLE!!] - The following are from the client and do not represent WarEmu integration.
        *  ID                  = 8323
        *  AbilityName         = Ravage
        *  AbilityDesc         = You channel the fell powers of the warp into your blade unleashing a devastating strike that does {COM_0_VAL0} Spiritual damage.
        *  ScaleStatMult       = 1.5
        *  ComponentID         = 147
        *  ComponentDesc       = 
        *  ComponentIndex      = 0
        *  Operation           = DAMAGE
        *  ActivationDelay     = 0
        *  Radius              = 0
        *  ComponentDuration   = 0
        *  FlightSpeed         = 0
        *  Trigger             = 1
        *  VfxID               = 0
        *  Values              = 20,0,6,7,0,0,0,0
        *  Values              = [0], [1], etc
        *  Multipliers         = 100,100,68,150,100,100,100,100
        *  Multipliers         = [0], [1], etc
        *  
        *  DAMAGE VALUE CONSTANT    (increasePerLevel) = 0.16666666666666666666666666666667
        *  HEAL VALUE CONSTANT      (increasePerLevel) = 1.0 [NOT CONFIRMED]
        *  MORALE VALUE CONSTANT    (increasePerLevel) = 0.55128205128205128205128205128205
        *  
        * ((((abilityLevel - 1) * increasePerLevel) * baseValue) + Values[0]) * (Multipliers[0] / 100)
        * AbilityLevel  = Player.Level + Buff.SUM(Ability.Specilization)
        * If Player.Level > 25, then Player.Level = 25
        * 
        * Level 1 Chosen
        * (((((1) - 1) * (0.166667)) * (20)) + 20) * ((100) / 100 ) = 20
        * 
        * Level 40 Chosen
        * (((((40) - 1) * (0.166667)) * (20)) + 20) * ((100) / 100 ) = 150
        * 
        * (((((39) * (0.166667)) * (20)) + 20) * (1) = 150
        * 
        * Values[0] = BaseDamage
        *
       */

        /*
         public float DamageConstantDef(float DamageConstant)
         {
             if (damageInfo.DamageType == DamageTypes.RawDamage)
                 DamageConstant = 0.55128205128205128205128205128205f;
             else
                 DamageConstant = 0.16666666666666666666666666666667f;
         }
         */

        const float DamageConstant = 0.16666666666666666666666666666667f;
        public uint GetDamageForLevel(byte level)
        {
            uint damage = (uint)((((level - 1) * (/*DamageConstantDef.DamageConstant*/DamageConstant) * BaseDamage) + BaseDamage) * (Multiplier / 100));
                return damage;
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