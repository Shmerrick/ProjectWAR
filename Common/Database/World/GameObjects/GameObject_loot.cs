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
    [DataTable(PreCache = false, TableName = "gameobject_loots", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class GameObject_loot : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [PrimaryKey]
        public uint ItemId { get; set; }

        [DataElement()]
        public float Pct { get; set; }

        public Item_Info Info;
    }
}