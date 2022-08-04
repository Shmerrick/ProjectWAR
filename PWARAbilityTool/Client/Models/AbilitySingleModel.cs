namespace PWARAbilityTool.Client.Models
{
    public class AbilitySingleModel : BaseModel
    {
        #region properties
        private int entry;
        public int Entry
        {
            get => entry;
            set
            {
                if (entry != value)
                {
                    Update("Entry");
                    entry = value;
                }
            }
        }

        private int careerLine;
        public int CareerLine
        {
            get => careerLine;
            set
            {
                if (careerLine != value)
                {
                    Update("CareerLine");
                    careerLine = value;
                }
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    Update("Name");
                    name = value;
                }
            }
        }

        private int minRange;
        public int MinRange
        {
            get => minRange;
            set
            {
                if (minRange != value)
                {
                    Update("MinRange");
                    minRange = value;
                }
            }
        }

        private int range;
        public int Range
        {
            get => range;
            set
            {
                if (range != value)
                {
                    Update("Range");
                    range = value;
                }
            }
        }

        private int castTime;
        public int CastTime
        {
            get => castTime;
            set
            {
                if (castTime != value)
                {
                    Update("CastTime");
                    castTime = value;
                }
            }
        }

        private int cd;
        public int Cooldown
        {
            get => cd;
            set
            {
                if (cd != value)
                {
                    Update("Cooldown");
                    cd = value;
                }
            }
        }

        private int apCost;
        public int ApCost
        {
            get => apCost;
            set
            {
                if (apCost != value)
                {
                    Update("ApCost");
                    apCost = value;
                }
            }
        }

        private int specialCost;
        public int SpecialCost
        {
            get => specialCost;
            set
            {
                if (specialCost != value)
                {
                    Update("SpecialCost");
                    specialCost = value;
                }
            }
        }

        private int moveCast;
        public int MoveCast
        {
            get => moveCast;
            set
            {
                if (moveCast != value)
                {
                    Update("MoveCast");
                    moveCast = value;
                }
            }
        }

        private int invokeDelay;
        public int InvokeDelay
        {
            get => invokeDelay;
            set
            {
                if (invokeDelay != value)
                {
                    Update("InvokeDelay");
                    invokeDelay = value;
                }
            }
        }

        private int effectDelay;
        public int EffectDelay
        {
            get => effectDelay;
            set
            {
                if (effectDelay != value)
                {
                    Update("EffectDelay");
                    effectDelay = value;
                }
            }
        }

        private int effectId;
        public int EffectID
        {
            get => effectId;
            set
            {
                if (effectId != value)
                {
                    Update("EffectID");
                    effectId = value;
                }
            }
        }

        private int channelId;
        public int ChannelID
        {
            get => channelId;
            set
            {
                if (channelId != value)
                {
                    Update("ChannelID");
                    channelId = value;
                }
            }
        }

        private int cdEntry;
        public int CooldownEntry
        {
            get => cdEntry;
            set
            {
                if (cdEntry != value)
                {
                    Update("CooldownEntry");
                    cdEntry = value;
                }
            }
        }

        private int toggleEntry;
        public int ToggleEntry
        {
            get => toggleEntry;
            set
            {
                if (toggleEntry != value)
                {
                    Update("ToggleEntry");
                    toggleEntry = value;
                }
            }
        }

        private int castAngle;
        public int CastAngle
        {
            get => castAngle;
            set
            {
                if (castAngle != value)
                {
                    Update("CastAngle");
                    castAngle = value;
                }
            }
        }

        private int abType;
        public int AbilityType
        {
            get => abType;
            set
            {
                if (abType != value)
                {
                    Update("AbilityType");
                    abType = value;
                }
            }
        }

        private int mTree;
        public int MasteryTree
        {
            get => mTree;
            set
            {
                if (mTree != value)
                {
                    Update("MasteryTree");
                    mTree = value;
                }
            }
        }

        private string specLine;
        public string Specline
        {
            get => specLine;
            set
            {
                if (specLine != value)
                {
                    Update("Specline");
                    specLine = value;
                }
            }
        }

        private int weaponNeeded;
        public int WeaponNeeded
        {
            get => weaponNeeded;
            set
            {
                if (weaponNeeded != value)
                {
                    Update("WeaponNeeded");
                    weaponNeeded = value;
                }
            }
        }

        private int affectsDead;
        public int AffectsDead
        {
            get => affectsDead;
            set
            {
                if (affectsDead != value)
                {
                    Update("AffectsDead");
                    affectsDead = value;
                }
            }
        }

        private int ignoreGlobalcd;
        public int IgnoreGlobalCooldown
        {
            get => ignoreGlobalcd;
            set
            {
                if (ignoreGlobalcd != value)
                {
                    Update("IgnoreGlobalCooldown");
                    ignoreGlobalcd = value;
                }
            }
        }

        private int ignoreOwnModifiers;
        public int IgnoreOwnModifiers
        {
            get => ignoreOwnModifiers;
            set
            {
                if (ignoreOwnModifiers != value)
                {
                    Update("IgnoreOwnModifiers");
                    ignoreOwnModifiers = value;
                }
            }
        }

        private int fragile;
        public int Fragile
        {
            get => fragile;
            set
            {
                if (fragile != value)
                {
                    Update("Fragile");
                    fragile = value;
                }
            }
        }

        private int minRank;
        public int MinimumRank
        {
            get => minRank;
            set
            {
                if (minRank != value)
                {
                    Update("MinimumRank");
                    minRank = value;
                }
            }
        }

        private int minimumRenown;
        public int MinimumRenown
        {
            get => minimumRenown;
            set
            {
                if (minimumRenown != value)
                {
                    Update("MinimumRenown");
                    minimumRenown = value;
                }
            }
        }

        private int iconId;
        public int IconId
        {
            get => iconId;
            set
            {
                if (iconId != value)
                {
                    Update("IconId");
                    iconId = value;
                }
            }
        }

        private int category;
        public int Category
        {
            get => category;
            set
            {
                if (category != value)
                {
                    Update("Category");
                    category = value;
                }
            }
        }

        private int flags;
        public int Flags
        {
            get => flags;
            set
            {
                if (flags != value)
                {
                    Update("Flags");
                    flags = value;
                }
            }
        }

        private int pointCost;
        public int PointCost
        {
            get => pointCost;
            set
            {
                if (pointCost != value)
                {
                    Update("PointCost");
                    pointCost = value;
                }
            }
        }

        public int cashCost;
        public int CashCost
        {
            get => cashCost;
            set
            {
                if (cashCost != value)
                {
                    Update("CashCost");
                    cashCost = value;
                }
            }
        }

        private int stealthInteraction;
        public int StealthInteraction
        {
            get => stealthInteraction;
            set
            {
                if (stealthInteraction != value)
                {
                    Update("StealthInteraction");
                    stealthInteraction = value;
                }
            }
        }

        private int aiRange;
        public int AIRange
        {
            get => aiRange;
            set
            {
                if (aiRange != value)
                {
                    Update("AIRange");
                    aiRange = value;
                }
            }
        }

        private int ignorecdReduction;
        public int IgnoreCooldownReduction
        {
            get => ignorecdReduction;
            set
            {
                if (ignorecdReduction != value)
                {
                    Update("IgnoreCooldownReduction");
                    ignorecdReduction = value;
                }
            }
        }

        private int cdCap;
        public int CDcap
        {
            get => cdCap;
            set
            {
                if (cdCap != value)
                {
                    Update("CDcap");
                    cdCap = value;
                }
            }
        }

        private string vfxTarget;
        public string VFXTarget
        {
            get => vfxTarget;
            set
            {
                if (vfxTarget != value)
                {
                    Update("VFXTarget");
                    vfxTarget = value;
                }
            }
        }

        private int abilityId;
        public int abilityID
        {
            get => abilityId;
            set
            {
                if (abilityId != value)
                {
                    Update("abilityID");
                    abilityId = value;
                }
            }
        }

        private int effectId2;
        public int effectID2
        {
            get => effectId2;
            set
            {
                if (effectId2 != value)
                {
                    Update("effectID2");
                    effectId2 = value;
                }
            }
        }

        private int time;
        public int Time
        {
            get => time;
            set
            {
                if (time != value)
                {
                    Update("Time");
                    time = value;
                }
            }
        }
        #endregion

        protected override void CollectErrors()
        {
            Errors.Clear();
            
        }
    }
}
