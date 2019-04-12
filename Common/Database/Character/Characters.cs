using System;
using System.Collections.Generic;
using System.Text;
using Common.Database.World.Characters;
using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "characters", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character : DataObject
    {
        private uint _characterId;
        private string _name;
        private string _surname;
        private int _realmId;
        private int _accountId;
        private byte _slotId;
        private byte _modelId;
        private byte _career;
        private byte _careerLine;
        private byte _realm;
        private int _heldLeft;
        private byte _race;
        private byte[] _traits;
        private byte _sex;
        public bool FirstConnect;
        private bool _anonymous;
        private bool _hidden;
        private string _oldName;
        private string _petName;
        private ushort _petModel;
        private ushort _honorPoints;
        private ushort _honorRank;


        public uint CareerFlags => CareerLine != 0 ? (uint)1 << (_careerLine - 1) : 0;

        [PrimaryKey]
        public uint CharacterId
        {
            get { return _characterId; }
            set { _characterId = value; Dirty = true; }
        }

        [DataElement(Unique = true,AllowDbNull = false,Varchar=24)]
        public string Name
        {
            get { return _name; }
            set { _name = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 24)]
        public string Surname
        {
            get { return _surname; }
            set { _surname = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int RealmId
        {
            get { return _realmId; }
            set { _realmId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int AccountId
        {
            get { return _accountId; }
            set { _accountId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte SlotId
        {
            get { return _slotId; }
            set { _slotId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte ModelId
        {
            get { return _modelId; }
            set { _modelId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Career
        {
            get { return _career; }
            set { _career = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte CareerLine
        {
            get { return _careerLine; }
            set { _careerLine = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Realm
        {
            get { return _realm; }
            set { _realm = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int HeldLeft
        {
            get { return _heldLeft; }
            set { _heldLeft = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Race
        {
            get { return _race; }
            set { _race = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Traits
        {
            get { return Encoding.UTF8.GetString(_traits); }
            set 
            {
                _traits = Encoding.UTF8.GetBytes(value); Dirty = true; 
            }
        }

        [DataElement(AllowDbNull = false)]
        public byte Sex
        {
            get { return _sex; }
            set { _sex = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool Anonymous
        {
            get { return _anonymous; }
            set { _anonymous = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool Hidden
        {
            get { return _hidden; }
            set { _hidden = value; Dirty = true; }
        }

        public byte[] bTraits
        {
            get { return _traits; }
            set { _traits = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 24)]
        public string OldName
        {
            get { return _oldName; }
            set { _oldName = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 24)]
        public string PetName
        {
            get { return _petName; }
            set { _petName = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort PetModel
        {
            get { return _petModel; }
            set { _petModel = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort HonorPoints
        {
            get { return _honorPoints; }
            set { _honorPoints = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort HonorRank
        {
            get { return _honorRank; }
            set { _honorRank = value; Dirty = true; }
        }

        public List<HonorRewardCooldown> HonorCooldowns { get; set; }

        public Character_value Value;

        public CharacterClientData ClientData;

        public List<Character_social> Socials;

        public List<Character_tok> Toks;

        public List<Character_tok_kills> TokKills;

        public List<Character_quest> Quests;

        public List<Characters_influence> Influences;

        public List<Characters_bag_pools> Bag_Pools;

        public List<Character_mail> Mails;

        public List<CharacterSavedBuff> Buffs;

        public string TempFirstName; //name to be shown to everyone (used for world events)
        public string TempLastName; //(used for world events)
    }
}
