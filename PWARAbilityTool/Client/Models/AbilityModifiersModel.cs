namespace PWARAbilityTool.Client.Models
{
    public class AbilityModifiersModel : BaseModel
    {
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

        private string sourceAbility;
        public string SourceAbility
        {
            get => sourceAbility;
            set
            {
                if (sourceAbility != value)
                {
                    Update("SourceAbility");
                    sourceAbility = value;
                }
            }
        }

        private int affecting;
        public int Affecting
        {
            get => affecting;
            set
            {
                if (affecting != value)
                {
                    Update("Affecting");
                    affecting = value;
                }
            }
        }

        private string affectedAbility;
        public string AffectedAbility
        {
            get => affectedAbility;
            set
            {
                if (affectedAbility != value)
                {
                    Update("AffectedAbility");
                    affectedAbility = value;
                }
            }
        }

        private int preOrPost;
        public int PreOrPost
        {
            get => preOrPost;
            set
            {
                if (preOrPost != value)
                {
                    Update("PreOrPost");
                    preOrPost = value;
                }
            }
        }

        private int sequence;
        public int Sequence
        {
            get => sequence;
            set
            {
                if (sequence != value)
                {
                    Update("Sequence");
                    sequence = value;
                }
            }
        }

        private string modCommandName;
        public string ModifierCommandName
        {
            get => modCommandName;
            set
            {
                if (modCommandName != value)
                {
                    Update("ModifierCommandName");
                    modCommandName = value;
                }
            }
        }

        private int targetCommandId;
        public int TargetCommandID
        {
            get => targetCommandId;
            set
            {
                if (targetCommandId != value)
                {
                    Update("TargetCommandID");
                    targetCommandId = value;
                }
            }
        }

        private int targetCommandSeq;
        public int TargetCommandSequence
        {
            get => targetCommandSeq;
            set
            {
                if (targetCommandSeq != value)
                {
                    Update("TargetCommandSequence");
                    targetCommandSeq = value;
                }
            }
        }

        private int primaryVal;
        public int PrimaryValue
        {
            get => primaryVal;
            set
            {
                if (primaryVal != value)
                {
                    Update("PrimaryValue");
                    primaryVal = value;
                }
            }
        }

        private int secVal;
        public int SecondaryValue
        {
            get => secVal;
            set
            {
                if (secVal != value)
                {
                    Update("SecondaryValue");
                    secVal = value;
                }
            }
        }

        private int buffLine;
        public int BuffLine
        {
            get => buffLine;
            set
            {
                if (buffLine != value)
                {
                    Update("BuffLine");
                    buffLine = value;
                }
            }
        }

        private string abModId;
        public string ability_modifiers_ID
        {
            get => abModId;
            set
            {
                if (abModId != value)
                {
                    Update("ability_modifiers_ID");
                    abModId = value;
                }
            }
        }
    }
}
