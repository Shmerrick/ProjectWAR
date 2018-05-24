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

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "rvr_objects", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RvRObjectInfo : DataObject
    {
        [PrimaryKey] public ushort ModelId { get; set; }

        [DataElement(AllowDbNull = false)] public string Name { get; set; }

        [DataElement(AllowDbNull = false)] public byte Race { get; set; }

        [DataElement(AllowDbNull = false)] public uint MaxInteractionDist { get; set; }

        [DataElement(AllowDbNull = false)] public uint MaxHealth { get; set; }

        [DataElement(AllowDbNull = false)] public uint BuildTime { get; set; }

        [DataElement(AllowDbNull = false)] public float ExclusionRadius { get; set; }
    }
}
