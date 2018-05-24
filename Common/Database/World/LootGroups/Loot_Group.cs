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
    [DataTable(PreCache = false, TableName = "loot_groups", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Loot_Group : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public byte DropEvent { get; set; }

        [DataElement]
        public uint CreatureID { get; set; }

        [DataElement]
        public ushort CreatureSubType { get; set; }

        [DataElement]
        public float DropChance { get; set; }

        [DataElement]
        public byte DropCount { get; set; }

        [DataElement]
        public bool ReqGroupUsable { get; set; }

        [DataElement]
        public ushort ReqActiveQuest { get; set; }

        [DataElement]
        public ushort SpecificZone { get; set; }

        public List<Loot_Group_Item> LootGroupItems;
    }
}