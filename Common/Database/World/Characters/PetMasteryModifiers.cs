/*
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
        private ushort  _MasteryModifierAddition;
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
