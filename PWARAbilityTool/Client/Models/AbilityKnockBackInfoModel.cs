namespace PWARAbilityTool.Client.Models
{
    public class AbilityKnockBackInfoModel : BaseModel
    {
        private int _entry;
        private int _id;
        private int _angle;
        private int _power;
        private int _rangeExtension;
        private int _gravMultiplier;
        private int _unk;

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

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    Update("Id");
                    _id = value;
                }
            }
        }

        public int Angle
        {
            get => _angle;
            set
            {
                if (_angle != value)
                {
                    Update("Angle");
                    _angle = value;
                }
            }
        }

        public int Power
        {
            get => _power;
            set
            {
                if (_power != value)
                {
                    Update("Power");
                    _power = value;
                }
            }
        }

        public int RangeExtension
        {
            get => _rangeExtension;
            set
            {
                if (_rangeExtension != value)
                {
                    Update("RangeExtension");
                    _rangeExtension = value;
                }
            }
        }

        public int GravMultiplier
        {
            get => _gravMultiplier;
            set
            {
                if (_gravMultiplier != value)
                {
                    Update("GravMultiplier");
                    _gravMultiplier = value;
                }
            }
        }

        public int Unk
        {
            get => _unk;
            set
            {
                if (_unk != value)
                {
                    Update("Unk");
                    _unk = value;
                }
            }
        }
    }
}
