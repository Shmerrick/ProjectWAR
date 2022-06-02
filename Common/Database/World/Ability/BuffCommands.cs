﻿using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "buff_commands", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class DBBuffCommandInfo : DataObject
    {
        [DataElement(Varchar = 8)]
        public string BuffClassString { get; set; }

        [DataElement]
        public byte BuffLine { get; set; }

        [PrimaryKey]
        public byte CommandID { get; set; }

        [DataElement(Varchar = 255)]
        public string CommandName { get; set; }

        [PrimaryKey]
        public byte CommandSequence { get; set; }

        [DataElement]
        public bool ConsumesStack { get; set; }

        [DataElement]
        public short EffectAngle { get; set; }

        [DataElement]
        public byte EffectRadius { get; set; }

        [DataElement(Varchar = 24)]
        public string EffectSource { get; set; }

        [PrimaryKey]
        public ushort Entry { get; set; }

        [DataElement]
        public byte EventChance { get; set; }

        [DataElement(Varchar = 36)]
        public string EventCheck { get; set; }

        [DataElement]
        public uint EventCheckParam { get; set; }

        [DataElement(Varchar = 36)]
        public string EventIDString { get; set; }

        [DataElement]
        public byte InvokeOn { get; set; }

        [DataElement]
        public byte MaxTargets { get; set; }

        [DataElement(Varchar = 255)]
        public string Name { get; set; }
        [DataElement]
        public bool NoAutoUse { get; set; }

        [DataElement]
        public int PrimaryValue { get; set; }

        [DataElement]
        public ushort RetriggerInterval { get; set; }

        [DataElement]
        public int SecondaryValue { get; set; }

        [DataElement(Varchar = 24)]
        public string Target { get; set; }

        [DataElement]
        public int TertiaryValue { get; set; }
    }
}