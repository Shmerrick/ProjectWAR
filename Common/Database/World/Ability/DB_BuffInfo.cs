﻿using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "buff_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class DBBuffInfo : DataObject
    {
        /// <summary>Indicates how an aura should propagate.</summary>
        [DataElement(Varchar = 255)]
        public string AuraPropagation { get; set; }

        [DataElement(Varchar = 8)]
        public string BuffClassString { get; set; }

        [DataElement]
        public bool CanRefresh { get; set; }

        [DataElement]
        public uint Duration { get; set; }

        [DataElement]
        public byte EnemyEffectID { get; set; }

        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement]
        public byte FriendlyEffectID { get; set; }

        [DataElement]
        public byte Group { get; set; }

        [DataElement]
        public ushort Interval { get; set; }

        [DataElement]
        public int LeadInDelay { get; set; }

        [DataElement]
        public byte MaxCopies { get; set; }

        [DataElement]
        public byte MaxStack { get; set; }

        [DataElement(Varchar = 255)]
        public string Name { get; set; }
        [DataElement]
        public byte PersistsOnDeath { get; set; }

        [DataElement]
        public byte Silent { get; set; }

        [DataElement]
        public byte StackLine { get; set; }

        [DataElement]
        public bool StacksFromCaster { get; set; }

        [DataElement(Varchar = 11)]
        public string TypeString { get; set; }
        [DataElement]
        public bool UseMaxStackAsInitial { get; set; }
    }
}