namespace PWARAbilityTool.Client.Models
{
    public class AbilityModifierChecksModel : BaseModel
    {
        private int _entry;
        private string _sourceAbility;
        private int _affecting;
        private string _affectedAbility;
        private int _preOrPost;
        private int _iD;
        private int _sequence;
        private string _commandName;
        private int _failCode;
        private int _primaryValue;
        private int _secondaryValue;
        private string _ability_modifier_checks_ID;

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

        public string SourceAbility
        {
            get => _sourceAbility;
            set
            {
                if (_sourceAbility != value)
                {
                    Update("SourceAbility");
                    _sourceAbility = value;
                }
            }
        }

        public int Affecting
        {
            get => _affecting;
            set
            {
                if (_affecting != value)
                {
                    Update("Affecting");
                    _affecting = value;
                }
            }
        }

        public string AffectedAbility
        {
            get => _affectedAbility;
            set
            {
                if (_affectedAbility != value)
                {
                    Update("AffectedAbility");
                    _affectedAbility = value;
                }
            }
        }

        public int PreOrPost
        {
            get => _preOrPost;
            set
            {
                if (_preOrPost != value)
                {
                    Update("PreOrPost");
                    _preOrPost = value;
                }
            }
        }

        public int ID
        {
            get => _iD;
            set
            {
                if (_iD != value)
                {
                    Update("ID");
                    _iD = value;
                }
            }
        }

        public int Sequence
        {
            get => _sequence;
            set
            {
                if (_sequence != value)
                {
                    Update("Sequence");
                    _sequence = value;
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

        public int FailCode
        {
            get => _failCode;
            set
            {
                if (_failCode != value)
                {
                    Update("FailCode");
                    _failCode = value;
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

        public string ability_modifier_checks_ID
        {
            get => _ability_modifier_checks_ID;
            set
            {
                if (_ability_modifier_checks_ID != value)
                {
                    Update("ability_modifier_checks_ID");
                    _ability_modifier_checks_ID = value;
                }
            }
        }
    }
}
