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
using System.Collections.Generic;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = true, TableName = "vendor_items", DatabaseName = "World")]
    [Serializable]
    public class Vendor_items : DataObject
    {
        public ushort _VendorId;
        public Item_Info Info;
        private uint _ItemGuid;
        private uint _ItemId;
        private uint _Price;
        private string _ReqItems;
        private ushort _ReqTokUnlock;
        private byte _ReqGuildlvl;

        [PrimaryKey]
        public uint ItemGuid
        {
            get { return _ItemGuid; }
            set { _ItemGuid = value; Dirty = true; }
        }

        [PrimaryKey]
        public ushort VendorId
        {
            get { return _VendorId; }
            set { _VendorId = value; Dirty = true; }
        }

        [PrimaryKey]
        public uint ItemId
        {
            get { return _ItemId; }
            set { _ItemId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Price
        {
            get { return _Price; }
            set { _Price = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public ushort ReqTokUnlock
        {
            get { return _ReqTokUnlock; }
            set { _ReqTokUnlock = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public byte ReqGuildlvl
        {
            get { return _ReqGuildlvl; }
            set { _ReqGuildlvl = value; Dirty = true; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string ReqItems
        {
            get { return _ReqItems; }
            set
            {
                _ReqItems = value;
                string[] Infos = _ReqItems.Split(')');
                foreach (string Info in Infos)
                {
                    if (Info.Length <= 0)
                        continue;

                    string[] Items = Info.Split(',');
                    if (Items.Length < 2)
                        continue;

                    Items[0] = Items[0].Remove(0, 1);

                    ushort Count = ushort.Parse(Items[0]);
                    uint Entry = uint.Parse(Items[1]);

                    if (!ItemsReq.ContainsKey(Entry))
                        ItemsReq.Add(Entry, Count);
                }
                Dirty = true;
            }
        }

        public Dictionary<uint, ushort> ItemsReq = new Dictionary<uint, ushort>();
    }
}