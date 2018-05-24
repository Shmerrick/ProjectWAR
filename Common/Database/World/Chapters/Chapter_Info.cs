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
    [DataTable(PreCache = false, TableName = "chapter_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Chapter_Info : DataObject
    {
        [PrimaryKey()]
        public uint Entry { get; set; }

        [DataElement()]
        public ushort ZoneId { get; set; }

        [DataElement(Varchar=255)]
        public string Name { get; set; }

        [DataElement()]
        public uint CreatureEntry { get; set; }

        [DataElement()]
        public uint InfluenceEntry { get; set; }

        [DataElement(Varchar = 30)]
        public string Race { get; set; }

        [DataElement()]
        public uint ChapterRank { get; set; }

        [DataElement()]
        public ushort PinX { get; set; }

        [DataElement()]
        public ushort PinY { get; set; }

        [DataElement()]
        public ushort TokEntry { get; set; }

        [DataElement()]
        public uint TokExploreEntry { get; set; }

        [DataElement()]
        public uint Tier1InfluenceCount { get; set; }
        
        [DataElement()]
        public uint Tier2InfluenceCount { get; set; }
        
        [DataElement()]
        public uint Tier3InfluenceCount { get; set; }

        public ushort OffX;
        public ushort OffY;
        public uint MaxInflu;
        public List<Chapter_Reward> T1Rewards;
        public List<Chapter_Reward> T2Rewards;
        public List<Chapter_Reward> T3Rewards;

    }
}