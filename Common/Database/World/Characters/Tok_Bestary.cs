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
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "tok_bestary", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Tok_Bestary : DataObject
    {
        [PrimaryKey]
        public ushort Creature_Sub_Type { get; set; }

        [DataElement]
        public ushort Bestary_ID { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill1 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill25 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill100 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill1000 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill10000 { get; set; }

        [DataElement(Varchar = 50)]
        public string Kill100000 { get; set; }
    }
}