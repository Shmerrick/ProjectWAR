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
    [DataTable(PreCache = false, TableName = "creature_items", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Creature_item : DataObject
    {
        private uint _Entry;
        private ushort _SlotId;
        private ushort _ModelId;
        private uint _EffectId;
        private ushort _PrimaryColor;
        private ushort _SecondaryColor;

        [PrimaryKey]
        public uint Entry
        {
            get { return _Entry; }
            set { _Entry = value; Dirty = true; }
        }

        [PrimaryKey]
        public ushort SlotId
        {
            get { return _SlotId; }
            set { _SlotId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort ModelId
        {
            get { return _ModelId; }
            set { _ModelId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint EffectId
        {
            get { return _EffectId; }
            set { _EffectId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort PrimaryColor
        {
            get { return _PrimaryColor; }
            set { _PrimaryColor = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort SecondaryColor
        {
            get { return _SecondaryColor; }
            set { _SecondaryColor = value; Dirty = true; }
        }
    }
}