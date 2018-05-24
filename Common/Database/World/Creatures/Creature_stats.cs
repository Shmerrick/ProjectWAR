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
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "creature_stats", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Creature_stats : DataObject
    {
        private string _UUID;
        private uint _ProtoEntry;
        private uint _StatId;
        private int _StatValue;
        private string _Comment;

        [DataElement(AllowDbNull = false)]
        public uint ProtoEntry
        {
            get { return _ProtoEntry; }
            set { _ProtoEntry = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint StatId
        {
            get { return _StatId; }
            set { _StatId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int StatValue
        {
            get { return _StatValue; }
            set { _StatValue = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = null; Dirty = true; }
        }

        [PrimaryKey]
        public string UUID
        {
            get { return _UUID; }
            set { _UUID = value; Dirty = true; }
        }
    }
}
