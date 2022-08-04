namespace PWARAbilityTool.Client.Models
{
    public class AbilityBuffCommandsModel : BaseModel
    {
        private int _entry;
        private string _name;
        private int _commandID;
        private int _commandSequence;
        private string _commandName;
        private int _primaryValue;
        private int _secondaryValue;
        private int _tertiaryValue;
        private int _invokeOn;
        private string _target;
        private string _effectSource;
        private int _effectRadius;
        private int _effectAngle;
        private int _maxTargets;
        private string _eventIDString;
        private string _eventCheck;
        private int _eventCheckParam;
        private int _eventChance;
        private int _consumesStack;
        private int _retriggerInterval;
        private int _buffLine;
        private int _noAutoUse;
        private string _buffClassString;

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

        public int TertiaryValue
        {
            get => _tertiaryValue;
            set
            {
                if (_tertiaryValue != value)
                {
                    Update("TertiaryValue");
                    _tertiaryValue = value;
                }
            }
        }

        public int InvokeOn
        {
            get => _invokeOn;
            set
            {
                if (_invokeOn != value)
                {
                    Update("InvokeOn");
                    _invokeOn = value;
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

        public string EventIDString
        {
            get => _eventIDString;
            set
            {
                if (_eventIDString != value)
                {
                    Update("EventIDString");
                    _eventIDString = value;
                }
            }
        }

        public string EventCheck
        {
            get => _eventCheck;
            set
            {
                if (_eventCheck != value)
                {
                    Update("EventCheck");
                    _eventCheck = value;
                }
            }
        }

        public int EventCheckParam
        {
            get => _eventCheckParam;
            set
            {
                if (_eventCheckParam != value)
                {
                    Update("EventCheckParam");
                    _eventCheckParam = value;
                }
            }
        }

        public int EventChance
        {
            get => _eventChance;
            set
            {
                if (_eventChance != value)
                {
                    Update("EventChance");
                    _eventChance = value;
                }
            }
        }

        public int ConsumesStack
        {
            get => _consumesStack;
            set
            {
                if (_consumesStack != value)
                {
                    Update("ConsumesStack");
                    _consumesStack = value;
                }
            }
        }

        public int RetriggerInterval
        {
            get => _retriggerInterval;
            set
            {
                if (_retriggerInterval != value)
                {
                    Update("RetriggerInterval");
                    _retriggerInterval = value;
                }
            }
        }

        public int BuffLine
        {
            get => _buffLine;
            set
            {
                if (_buffLine != value)
                {
                    Update("BuffLine");
                    _buffLine = value;
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
    }
}
