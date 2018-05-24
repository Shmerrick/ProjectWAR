/*
 * Copyright (C) 2014 WarEmu
 *	http://WarEmu.com
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "keep_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Keep_Info : DataObject
    {
        private byte _KeepId;
        private string _Name;
        private byte _Realm;
        private byte _Race;
        private byte _DoorCount;
        private ushort _ZoneId;
        private ushort _RegionId;
        private ushort _PQuestId;

        [PrimaryKey]
        public byte KeepId
        {
            get { return _KeepId; }
            set { _KeepId = value; Dirty = true; }
        }

        [DataElement(Varchar = 255)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Realm
        {
            get { return _Realm; }
            set { _Realm = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Race
        {
            get { return _Race; }
            set { _Race = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte DoorCount
        {
            get { return _DoorCount; }
            set { _DoorCount = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId
        {
            get { return _ZoneId; }
            set { _ZoneId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort RegionId
        {
            get { return _RegionId; }
            set { _RegionId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort PQuestId
        {
            get { return _PQuestId; }
            set { _PQuestId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int X { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Y { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Z { get; set; }

        [DataElement(AllowDbNull = false)]
        public int O { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilX { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilY { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilO { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilOuterX { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilOuterY { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilOuterZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public int OilOuterO { get; set; }

        // RB   5/15/2016   Fixed Ram locations in DB

        [DataElement(AllowDbNull = false)]
        public int RamX { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RamY { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RamZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RamO { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RamOuterX { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RamOuterY { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RamOuterZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RamOuterO { get; set; }

        public List<Keep_Creature> Creatures;
        public List<Keep_Door> Doors;
        public PQuest_Info PQuest;
    }
}
