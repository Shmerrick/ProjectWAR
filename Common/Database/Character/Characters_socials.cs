﻿using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "characters_socials", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character_social : DataObject
    {
        public object Event;
        private uint _CharacterId;

        private uint _DistCharacterId;
        private string _DistName;
        private byte _Friend;
        private byte _Ignore;

        [PrimaryKey]
        public uint CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }

        [PrimaryKey]
        public uint DistCharacterId
        {
            get { return _DistCharacterId; }
            set { _DistCharacterId = value; Dirty = true; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string DistName
        {
            get { return _DistName; }
            set { _DistName = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Friend
        {
            get { return _Friend; }
            set { _Friend = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Ignore
        {
            get { return _Ignore; }
            set { _Ignore = value; Dirty = true; }
        }
        public T GetEvent<T>()
        {
            return (T)Convert.ChangeType(Event, typeof(T));
        }
    }
}