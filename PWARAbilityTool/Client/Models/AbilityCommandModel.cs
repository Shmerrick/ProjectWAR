namespace PWARAbilityTool.Client.Models
{
    public class AbilityCommandModel : BaseModel
    {
        private int _entry;
        private string _abilityName;
        private int _commandID;
        private int _commandSequence;
        private string _commandName;
        private int _primaryValue;
        private int _secondaryValue;
        private string _target;
        private int _effectRadius;
        private int _effectAngle;
        private string _effectSource;
        private int _fromAllTargets;
        private int _maxTargets;
        private int _attackingStat;
        private int _noAutoUse;
        private int _isDelayedEffect;

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

        public string AbilityName
        {
            get => _abilityName;
            set
            {
                if (_abilityName != value)
                {
                    Update("AbilityName");
                    _abilityName = value;
                }
            }
        }

        public int CommandID
        {
            get => _commandID;
            set
            {
                if (_commandID != value)
                {
                    Update("CommandID");
                    _commandID = value;
                }
            }
        }

        public int CommandSequence
        {
            get => _commandSequence;
            set
            {
                if (_commandSequence != value)
                {
                    Update("CommandSequence");
                    _commandSequence = value;
                }
            }
        }

        public string CommandName
        {
            get => _commandName;
            set
            {
                if (_commandName != value)
                {
                    Update("CommandName");
                    _commandName = value;
                }
            }
        }

        public int PrimaryValue
        {
            get => _primaryValue;
            set
            {
                if (_primaryValue != value)
                {
                    Update("PrimaryValue");
                    _primaryValue = value;
                }
            }
        }

        public int SecondaryValue
        {
            get => _secondaryValue;
            set
            {
                if (_secondaryValue != value)
                {
                    Update("SecondaryValue");
                    _secondaryValue = value;
                }
            }
        }

        public string Target
        {
            get => _target;
            set
            {
                if (_target != value)
                {
                    Update("Target");
                    _target = value;
                }
            }
        }

        public int EffectRadius
        {
            get => _effectRadius;
            set
            {
                if (_effectRadius != value)
                {
                    Update("EffectRadius");
                    _effectRadius = value;
                }
            }
        }

        public int EffectAngle
        {
            get => _effectAngle;
            set
            {
                if (_effectAngle != value)
                {
                    Update("EffectAngle");
                    _effectAngle = value;
                }
            }
        }

        public string EffectSource
        {
            get => _effectSource;
            set
            {
                if (_effectSource != value)
                {
                    Update("EffectSource");
                    _effectSource = value;
                }
            }
        }

        public int FromAllTargets
        {
            get => _fromAllTargets;
            set
            {
                if (_fromAllTargets != value)
                {
                    Update("FromAllTargets");
                    _fromAllTargets = value;
                }
            }
        }

        public int MaxTargets
        {
            get => _maxTargets;
            set
            {
                if (_maxTargets != value)
                {
                    Update("MaxTargets");
                    _maxTargets = value;
                }
            }
        }

        public int AttackingStat
        {
            get => _attackingStat;
            set
            {
                if (_attackingStat != value)
                {
                    Update("AttackingStat");
                    _attackingStat = value;
                }
            }
        }

        public int isDelayedEffect
        {
            get => _isDelayedEffect;
            set
            {
                if (_isDelayedEffect != value)
                {
                    Update("isDelayedEffect");
                    _isDelayedEffect = value;
                }
            }
        }

        public int NoAutoUse
        {
            get => _noAutoUse;
            set
            {
                if (_noAutoUse != value)
                {
                    Update("NoAutoUse");
                    _noAutoUse = value;
                }
            }
        }
    }
}
