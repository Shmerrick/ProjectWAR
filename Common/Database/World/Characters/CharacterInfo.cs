﻿/*
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using FrameWork;
using System;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "characterinfo", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CharacterInfo : DataObject
    {
        private byte _CareerLine;
        private byte _Career;
        private string _CareerName;
        private byte _Realm;
        private ushort _Region;
        private ushort _ZoneId;
        private int _WorldX;
        private int _WorldY;
        private int _WorldZ;
        private int _WorldO;
        private ushort _RallyPt;
        private uint _Skills;

        [PrimaryKey]
        public byte CareerLine
        {
            get { return _CareerLine; }
            set { _CareerLine = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Career
        {
            get { return _Career; }
            set { _Career = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public string CareerName
        {
            get { return _CareerName; }
            set { _CareerName = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Realm
        {
            get { return _Realm; }
            set { _Realm = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Region
        {
            get { return _Region; }
            set { _Region = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId
        {
            get { return _ZoneId; }
            set { _ZoneId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldX
        {
            get { return _WorldX; }
            set { _WorldX = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldY
        {
            get { return _WorldY; }
            set { _WorldY = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldZ
        {
            get { return _WorldZ; }
            set { _WorldZ = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldO
        {
            get { return _WorldO; }
            set { _WorldO = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort RallyPt
        {
            get { return _RallyPt; }
            set { _RallyPt = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Skills
        {
            get { return _Skills; }
            set { _Skills = value; Dirty = true; }
        }
    }
}