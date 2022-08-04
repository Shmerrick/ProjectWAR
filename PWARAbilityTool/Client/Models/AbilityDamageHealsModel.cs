namespace PWARAbilityTool.Client.Models
{
    public class AbilityDamageHealsModel : BaseModel
    {
        private int _entry;
        private int _displayEntry;
        private int _index;
        private string _name;
        private int _minDamage;
        private int _maxDamage;
        private int _damageVariance;
        private string _damageType;
        private int _parentCommandID;
        private int _parentCommandSequence;
        private float _castTimeDamageMult;
        private string _weaponDamageFrom;
        private float _weaponDamageScale;
        private int _noCrits;
        private int _undefendable;
        private int _overrideDefenseEvent;
        private int _statUsed;
        private float _statDamageScale;
        private int _ressourceBuild;
        private int _castPlayerSubID;
        private float _armorResistPenFactor;
        private float _hatredScale;
        private float _healHatredScale;
        private float _priStatMultiplier;

        public int Entry
        {
            get => _entry;
            set
            {
                if (_entry != value)
                {
                    Update("Entry");
                    _entry = value;
                }
            }
        }

        public int DisplayEntry
        {
            get => _displayEntry;
            set
            {
                if (_displayEntry != value)
                {
                    Update("DisplayEntry");
                    _displayEntry = value;
                }
            }
        }

        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    Update("Index");
                    _index = value;
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    Update("Name");
                    _name = value;
                }
            }
        }

        public int MinDamage
        {
            get => _minDamage;
            set
            {
                if (_minDamage != value)
                {
                    Update("MinDamage");
                    _minDamage = value;
                }
            }
        }

        public int MaxDamage
        {
            get => _maxDamage;
            set
            {
                if (_maxDamage != value)
                {
                    Update("MaxDamage");
                    _maxDamage = value;
                }
            }
        }

        public int DamageVariance
        {
            get => _damageVariance;
            set
            {
                if (_damageVariance != value)
                {
                    Update("DamageVariance");
                    _damageVariance = value;
                }
            }
        }

        public string DamageType
        {
            get => _damageType;
            set
            {
                if (_damageType != value)
                {
                    Update("DamageType");
                    _damageType = value;
                }
            }
        }

        public int ParentCommandID
        {
            get => _parentCommandID;
            set
            {
                if (_parentCommandID != value)
                {
                    Update("ParentCommandID");
                    _parentCommandID = value;
                }
            }
        }

        public int ParentCommandSequence
        {
            get => _parentCommandSequence;
            set
            {
                if (_parentCommandSequence != value)
                {
                    Update("ParentCommandSequence");
                    _parentCommandSequence = value;
                }
            }
        }

        public float CastTimeDamageMult
        {
            get => _castTimeDamageMult;
            set
            {
                if (_castTimeDamageMult != value)
                {
                    Update("CastTimeDamageMult");
                    _castTimeDamageMult = value;
                }
            }
        }

        public string WeaponDamageFrom
        {
            get => _weaponDamageFrom;
            set
            {
                if (_weaponDamageFrom != value)
                {
                    Update("WeaponDamageFrom");
                    _weaponDamageFrom = value;
                }
            }
        }

        public float WeaponDamageScale
        {
            get => _weaponDamageScale;
            set
            {
                if (_weaponDamageScale != value)
                {
                    Update("WeaponDamageScale");
                    _weaponDamageScale = value;
                }
            }
        }

        public int NoCrits
        {
            get => _noCrits;
            set
            {
                if (_noCrits != value)
                {
                    Update("NoCrits");
                    _noCrits = value;
                }
            }
        }

        public int Undefendable
        {
            get => _undefendable;
            set
            {
                if (_undefendable != value)
                {
                    Update("Undefendable");
                    _undefendable = value;
                }
            }
        }

        public int OverrideDefenseEvent
        {
            get => _overrideDefenseEvent;
            set
            {
                if (_overrideDefenseEvent != value)
                {
                    Update("OverrideDefenseEvent");
                    _overrideDefenseEvent = value;
                }
            }
        }

        public int StatUsed
        {
            get => _statUsed;
            set
            {
                if (_statUsed != value)
                {
                    Update("StatUsed");
                    _statUsed = value;
                }
            }
        }

        public float StatDamageScale
        {
            get => _statDamageScale;
            set
            {
                if (_statDamageScale != value)
                {
                    Update("StatDamageScale");
                    _statDamageScale = value;
                }
            }
        }

        public int RessourceBuild
        {
            get => _ressourceBuild;
            set
            {
                if (_ressourceBuild != value)
                {
                    Update("RessourceBuild");
                    _ressourceBuild = value;
                }
            }
        }

        public int CastPlayerSubID
        {
            get => _castPlayerSubID;
            set
            {
                if (_castPlayerSubID != value)
                {
                    Update("CastPlayerSubID");
                    _castPlayerSubID = value;
                }
            }
        }

        public float ArmorResistPenFactor
        {
            get => _armorResistPenFactor;
            set
            {
                if (_armorResistPenFactor != value)
                {
                    Update("ArmorResistPenFactor");
                    _armorResistPenFactor = value;
                }
            }
        }

        public float HatredScale
        {
            get => _hatredScale;
            set
            {
                if (_hatredScale != value)
                {
                    Update("HatredScale");
                    _hatredScale = value;
                }
            }
        }

        public float HealHatredScale
        {
            get => _healHatredScale;
            set
            {
                if (_healHatredScale != value)
                {
                    Update("HealHatredScale");
                    _healHatredScale = value;
                }
            }
        }

        public float PriStatMultiplier
        {
            get => _priStatMultiplier;
            set
            {
                if (_priStatMultiplier != value)
                {
                    Update("PriStatMultiplier");
                    _priStatMultiplier = value;
                }
            }
        }
    }
}
