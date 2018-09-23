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
    [DataTable(PreCache = false, TableName = "gameobject_spawns", DatabaseName = "World")]
    [Serializable]
    public class GameObject_spawn : DataObject
    {
        public GameObject_proto Proto;

        [PrimaryKey(AutoIncrement = true)]
        public uint Guid { get; set; }

        [DataElement()]
        public uint Entry { get; set; }

        [DataElement()]
        public ushort ZoneId { get; set; }

        [DataElement()]
        public int WorldX { get; set; }

        [DataElement()]
        public int WorldY { get; set; }

        [DataElement()]
        public int WorldZ { get; set; }

        [DataElement()]
        public int WorldO { get; set; }

        [DataElement()]
        public uint DisplayID { get; set; }

        [DataElement(AllowDbNull = true)]
        public ushort[] Unks { get; set; } = new ushort[6];

        public ushort GetUnk(int Id)
        {
            if (Id >= Unks.Length)
                return 0;

            return Unks[Id];
        }

        [DataElement()]
        public byte Unk1 { get; set; }

        [DataElement()]
        public byte Unk2 { get; set; }

        [DataElement()]
        public uint Unk3 { get; set; }

        [DataElement()]
        public uint Unk4 { get; set; }

        [DataElement()]
        public uint VfxState { get; set; }

        [DataElement()]
        public uint AllowVfxUpdate { get; set; }

        [DataElement()]
        public uint DoorId { get; set; }

        [DataElement()]
        public string TokUnlock { get; set; }

        [DataElement()]
        public uint SoundId { get; set; }

        [DataElement()]
        public string AlternativeName { get; set; }

        public void BuildFromProto(GameObject_proto Proto)
        {
			if (Proto == null)
			{
				return;
			}
			this.Proto = Proto;
            Entry = Proto.Entry;
            Unks = Proto.Unks;
            DisplayID = Proto.DisplayID;
        }
    }
}
