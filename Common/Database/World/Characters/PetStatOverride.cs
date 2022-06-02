using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "pet_stat_override", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PetStatOverride : DataObject
    {
        private byte _CareerLine;
        private byte _PrimaryValue;
        private float _SecondaryValue;
        private bool _Active;
        private string _UUID;

        [PrimaryKey]
        public string UUID
        {
            get { return _UUID; }
            set { _UUID = value; Dirty = true; }
        }

        [DataElement]
        public byte CareerLine
        {
            get { return _CareerLine; }
            set { _CareerLine = value; Dirty = true; }
        }

        [DataElement]
        public byte PrimaryValue
        {
            get { return _PrimaryValue; }
            set { _PrimaryValue = value; Dirty = true; }
        }

        [DataElement]
        public float SecondaryValue
        {
            get { return _SecondaryValue; }
            set { _SecondaryValue = value; Dirty = true; }
        }

        [DataElement]
        public bool Active
        {
            get { return _Active; }
            set { _Active = value; Dirty = true; }
        }
    }

    public class DBPetStatOverrideInfo : DataObject
    {
        [PrimaryKey]
        public string UUID { get; set; }

        [DataElement]
        public int CareerLine { get; set; }

        [DataElement(Varchar = 255)]
        public string CommandName { get; set; }

        [DataElement]
        public int PrimaryValue { get; set; }

        [DataElement]
        public float SecondaryValue { get; set; }

        [DataElement]
        public bool Active { get; set; }
    }
}