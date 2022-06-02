using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "pet_mastery_modifiers", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PetMasteryModifiers : DataObject
    {
        private byte _CareerLine;
        private byte _PrimaryValue;
        private byte _MasteryTree;
        private byte _PointStart;
        private byte _PointEnd;
        private float _MasteryModifierPercent;
        private ushort _MasteryModifierAddition;
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
        public byte MasteryTree
        {
            get { return _MasteryTree; }
            set { _MasteryTree = value; Dirty = true; }
        }

        [DataElement]
        public byte PointStart
        {
            get { return _PointStart; }
            set { _PointStart = value; Dirty = true; }
        }

        [DataElement]
        public byte PointEnd
        {
            get { return _PointEnd; }
            set { _PointEnd = value; Dirty = true; }
        }

        [DataElement]
        public float MasteryModifierPercent
        {
            get { return _MasteryModifierPercent; }
            set { _MasteryModifierPercent = value; Dirty = true; }
        }

        [DataElement]
        public ushort MasteryModifierAddition
        {
            get { return _MasteryModifierAddition; }
            set { _MasteryModifierAddition = value; Dirty = true; }
        }

        [DataElement]
        public bool Active
        {
            get { return _Active; }
            set { _Active = value; Dirty = true; }
        }
    }

    public class DBPetMasteryModifiersInfo : DataObject
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
        public int MasteryTree { get; set; }

        [DataElement]
        public int PointStart { get; set; }

        [DataElement]
        public int PointEnd { get; set; }

        [DataElement]
        public float MasteryModifierPercent { get; set; }

        [DataElement]
        public bool Active { get; set; }
    }
}