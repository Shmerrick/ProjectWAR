namespace PWARAbilityTool.Client.Models
{
    public class AbilityBuffInfosModel : BaseModel
    {
        private int _entry;
        private string _name;
        private string _buffClassString;
        private string _typeString;
        private int _group;
        private string _auraPropagation;
        private int _maxCopies;
        private int _maxStack;
        private int _userMaxStackAsInitial;
        private int _stackLine;
        private int _stacksFromCaster;
        private int _duration;
        private int _leadInDelay;
        private int _interval;
        private int _persistsOnDeath;
        private int _canRefresh;
        private int _friendlyEffectID;
        private int _enemyEffectID;
        private int _silent;

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

        public string BuffClassString
        {
            get => _buffClassString;
            set
            {
                if (_buffClassString != value)
                {
                    Update("BuffClassString");
                    _buffClassString = value;
                }
            }
        }

        public string TypeString
        {
            get => _typeString;
            set
            {
                if (_typeString != value)
                {
                    Update("TypeString");
                    _typeString = value;
                }
            }
        }

        public int Group
        {
            get => _group;
            set
            {
                if (_group != value)
                {
                    Update("Group");
                    _group = value;
                }
            }
        }

        public string AuraPropagation
        {
            get => _auraPropagation;
            set
            {
                if (_auraPropagation != value)
                {
                    Update("AuraPropagation");
                    _auraPropagation = value;
                }
            }
        }

        public int MaxCopies
        {
            get => _maxCopies;
            set
            {
                if (_maxCopies != value)
                {
                    Update("MaxCopies");
                    _maxCopies = value;
                }
            }
        }

        public int MaxStack
        {
            get => _maxStack;
            set
            {
                if (_maxStack != value)
                {
                    Update("MaxStack");
                    _maxStack = value;
                }
            }
        }

        public int UserMaxStackAsInitial
        {
            get => _userMaxStackAsInitial;
            set
            {
                if (_userMaxStackAsInitial != value)
                {
                    Update("UserMaxStackAsInitial");
                    _userMaxStackAsInitial = value;
                }
            }
        }

        public int StackLine
        {
            get => _stackLine;
            set
            {
                if (_stackLine != value)
                {
                    Update("StackLine");
                    _stackLine = value;
                }
            }
        }

        public int StacksFromCaster
        {
            get => _stacksFromCaster;
            set
            {
                if (_stacksFromCaster != value)
                {
                    Update("StacksFromCaster");
                    _stacksFromCaster = value;
                }
            }
        }

        public int Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    Update("Duration");
                    _duration = value;
                }
            }
        }

        public int LeadInDelay
        {
            get => _leadInDelay;
            set
            {
                if (_leadInDelay != value)
                {
                    Update("LeadInDelay");
                    _leadInDelay = value;
                }
            }
        }

        public int Interval
        {
            get => _interval;
            set
            {
                if (_interval != value)
                {
                    Update("Interval");
                    _interval = value;
                }
            }
        }

        public int PersistsOnDeath
        {
            get => _persistsOnDeath;
            set
            {
                if (_persistsOnDeath != value)
                {
                    Update("PersistsOnDeath");
                    _persistsOnDeath = value;
                }
            }
        }

        public int CanRefresh
        {
            get => _canRefresh;
            set
            {
                if (_canRefresh != value)
                {
                    Update("CanRefresh");
                    _canRefresh = value;
                }
            }
        }

        public int FriendlyEffectID
        {
            get => _friendlyEffectID;
            set
            {
                if (_friendlyEffectID != value)
                {
                    Update("FriendlyEffectID");
                    _friendlyEffectID = value;
                }
            }
        }

        public int EnemyEffectID
        {
            get => _enemyEffectID;
            set
            {
                if (_enemyEffectID != value)
                {
                    Update("EnemyEffectID");
                    _enemyEffectID = value;
                }
            }
        }

        public int Silent
        {
            get => _silent;
            set
            {
                if (_silent != value)
                {
                    Update("Silent");
                    _silent = value;
                }
            }
        }
    }
}
