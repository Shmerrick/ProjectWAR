using System;
using System.Collections.Generic;

using FrameWork;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "item_infos", DatabaseName = "World")]
    [Serializable]
    public class Item_Info : DataObject
    {
        public static int MaxStats = 12;

        private uint _entry;
        private string _name="";
        private string _description="";
        private byte _type;
        private byte _race;
        private uint _modelId;
        private ushort _slotId;
        private byte _rarity;
        private uint _career;
        private uint _skills;
        private byte _bind;
        private ushort _armor;
        private ushort _spellId;
        private uint _itemSet;
        private ushort _dps;
        private ushort _speed;
        private byte _minRank;
        private byte _minRenown;
        private byte _objectLevel;
        private byte _uniqueEquiped;
        private int _startQuest;
        private uint _sellPrice;
        private byte _talismanSlots;
        private ushort _maxStack;
        private string _scriptName;
        private byte[] _unk27;
        private bool _twoHanded;
        public byte Realm;
        private String _craftresult;
        private bool _dyeAble;
        private bool _salvageable;
        private ushort _baseColor1;
        private ushort _baseColor2;

        public Dictionary<byte, ushort> _Stats = new Dictionary<byte, ushort>();
        public List<ushort> EffectsList = new List<ushort>(); 
        public List<KeyValuePair<byte, ushort>> _Crafts = new List<KeyValuePair<byte, ushort>>();
        public List<KeyValuePair<uint, ushort>> _SellRequiredItems = new List<KeyValuePair<uint, ushort>>();
        public List<KeyValuePair<Item_Info, ushort>> RequiredItems = new List<KeyValuePair<Item_Info, ushort>>();

        [PrimaryKey]
        public uint Entry { get { return _entry; } set { _entry = value; } }
        [DataElement(Varchar = 255)]
        public string Name { get { return _name; } set { _name = value; } }
        [DataElement(Varchar = 255)]
        public string Description { get { return _description; } set { _description = value; } }
        
        [DataElement()]
        public byte Type { get { return _type; } set { _type = value; } }
        [DataElement()]
        public byte Race { get { return _race; } set { _race = value; } }
        [DataElement()]
        public uint ModelId  { get { return _modelId; } set { _modelId = value; } }
        [DataElement()]
        public ushort SlotId { get { return _slotId; } set { _slotId = value; } }
        [DataElement()]
        public byte Rarity { get { return _rarity; } set { _rarity = value; } }
        [DataElement()]
        public uint Career { get { return _career; } set { _career = value; } }
        [DataElement()]
        public uint Skills { get { return _skills; } set {  _skills= value; } }
        [DataElement()]
        public byte Bind { get { return _bind; } set { _bind = value; } }
        [DataElement()]
        public ushort Armor { get { return _armor; } set { _armor = value; } }
        [DataElement()]
        public ushort SpellId { get { return _spellId; } set { _spellId = value; } }
        [DataElement()]
        public uint ItemSet { get { return _itemSet; } set { _itemSet = value; } }
        [DataElement()]
        public ushort Dps { get { return _dps; } set { _dps = value; } }
        [DataElement()]
        public ushort Speed { get { return _speed; } set { _speed = value; } }
        [DataElement()]
        public byte MinRank { get { return _minRank; } set { _minRank = value; } }
        [DataElement()]
        public byte MinRenown { get { return _minRenown; } set { _minRenown = value; } }
        [DataElement()]
        public byte ObjectLevel { get { return _objectLevel; } set { _objectLevel = value; } }
        [DataElement()]
        public byte UniqueEquiped { get { return _uniqueEquiped; } set { _uniqueEquiped = value; } }
        [DataElement()]
        public int StartQuest { get { return _startQuest; } set { _startQuest = value; } }
        [DataElement()]
        public string Stats
        {
            get
            {
                string st = "";
                foreach (KeyValuePair<byte, ushort> stat in _Stats)
                    st += stat.Key + ":" + stat.Value + ";";

                return st;
            }
            set
            {
                string[] st = value.Split(';');
                byte type;
                ushort Value;
                ushort lastValue;
                string[] val;

                try {

                    foreach (string str in st)
                        if (str.Length > 1)
                        {
                            val = str.Split(':');
                            if (val.Length < 2) continue;

                            type = byte.Parse(val[0]);
                            Value = ushort.Parse(val[1]);

                            if (type <= 0 || Value <= 0)
                                continue;

                            lastValue = 0;

                            if (!_Stats.TryGetValue(type, out lastValue))
                                _Stats.Add(type, Value);
                            else
                                _Stats[type] = (ushort)(lastValue + Value);
                        }
                }
                catch(Exception)
                {
                    Log.Error("itemstats broken in item entry ", "" + Entry + " statsstring " + value);

                }
            }
        }

        [DataElement()]
        public string Effects
        {
            get
            {
                string st = "";
                foreach (ushort effect in EffectsList)
                    st += effect + ";";

                return st;
            }
            set
            {
                EffectsList.Clear();

                string[] effectStrArray = value.Split(';');

                foreach (string effectStr in effectStrArray)
                    if (effectStr.Length != 0)
                        EffectsList.Add(ushort.Parse(effectStr));
            }
        }

        [DataElement()]
        public string Crafts
        {
            get
            {
                string st = "";
                foreach (KeyValuePair<byte, ushort> craft in _Crafts)
                    st += craft.Key + ":" + craft.Value + ";";

                return st;
            }
            set
            {
                if (value.Length == 0)
                    return;
                else if (value.Length < 3)
                {
                    _Crafts.Add(new KeyValuePair<byte, ushort>(byte.Parse(value), 0));
                }
                else
                {
                    string[] st = value.Split(';');
                    byte type;
                    ushort Value;
                    string[] val;
                    _Crafts.Clear();

                    foreach (string str in st)
                    {
                        if (str.Length > 1)
                        {
                            val = str.Split(':');
                            if (val.Length < 2) continue;

                            type = byte.Parse(val[0]);
                            Value = ushort.Parse(val[1]);

                            if (type <= 0 || Value <= 0)
                                continue;

                            _Crafts.Add(new KeyValuePair<byte, ushort>(type, Value));
                        }
                    }
                }
            }
        }

        [DataElement()]
        public uint SellPrice { get { return _sellPrice; } set { _sellPrice = value; } }
        [DataElement()]
        public string SellRequiredItems
        {
            get
            {
                string st = "";
                foreach (KeyValuePair<uint, ushort> sri in _SellRequiredItems)
                    st += sri.Key + ":" + sri.Value + ";";

                return st;
            }
            set
            {
                string[] st = value.Split(';');
                uint type;
                ushort Value;
                string[] val;
                _SellRequiredItems.Clear();

                foreach (string str in st)
                {
                    if (str.Length > 1)
                    {
                        val = str.Split(':');
                        if (val.Length < 2) continue;

                        type = uint.Parse(val[0]);
                        Value = ushort.Parse(val[1]);

                        if (type <= 0 || Value <= 0)
                            continue;

                        _SellRequiredItems.Add(new KeyValuePair<uint, ushort>(type, Value));
                    }
                }
            }
        }

        [DataElement()]
        public byte TalismanSlots { get { return _talismanSlots; } set { _talismanSlots = value; } }
        [DataElement()]
        public ushort MaxStack { get { return _maxStack; } set { _maxStack = value; } }
        [DataElement()]
        public byte[] Unk27 
        {
            get
            {
                if (_unk27 == null || _unk27.Length < 27)
                {
                    _unk27 = new byte[27];
                    _unk27[4] = 0x03;
                    _unk27[5] = 0x02;
                }
                return _unk27;
            }
            set
            {
                _unk27 = value;
                if (_unk27 == null || _unk27.Length < 27)
                {
                    _unk27 = new byte[27];
                    _unk27[4] = 0x03;
                    _unk27[5] = 0x02;
                }
            }
        }

        [DataElement(Varchar=255)]
        public string ScriptName { get { return _scriptName; } set { _scriptName = value; } }

        [DataElement()]
        public bool TwoHanded { get { return _twoHanded; } set { _twoHanded = value; } }

        public Dictionary<byte, ushort> GetStats()
        {
            return _Stats;
        }

        [DataElement()]
        public String Craftresult { get { return _craftresult; } set { _craftresult = value; } }

        [DataElement()]
        public bool DyeAble { get { return _dyeAble; } set { _dyeAble = value; } }

        [DataElement()]
        public bool Salvageable { get { return _salvageable; } set { _salvageable = value; } }

        [DataElement()]
        public ushort BaseColor1 { get { return _baseColor1; } set { _baseColor1 = value; } }

        [DataElement()]
        public ushort BaseColor2 { get { return _baseColor2; } set { _baseColor2 = value; } }

        [DataElement(AllowDbNull = false)]
        public ushort TokUnlock { get; set; }

        //This is the unlock value that is awarded when player completes several smaller unlocks
        [DataElement(AllowDbNull = false)]
        public ushort TokUnlock2 { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool IsSiege { get; set; }

    }
}
