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
    [DataTable(PreCache = false, TableName = "scenario_objects", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Scenario_Object : DataObject
    {
        [DataElement]
        public ushort ScenarioId { get; set; }

        [DataElement]
        public ushort Identifier { get; set; }

        [DataElement(Varchar=255)]
        public string ObjectiveName { get; set; }

        [DataElement]
        public uint ProtoEntry { get; set; }

        [DataElement(Varchar = 255)]
        public string Type { get; set; }

        [DataElement]
        public byte PointGain { get; set; }

        [DataElement]
        public byte PointOverTimeGain { get; set; }

        [DataElement]
        public int WorldPosX { get; set; }

        [DataElement]
        public int WorldPosY { get; set; }

        [DataElement]
        public ushort PosZ { get; set; }

        [DataElement]
        public ushort Heading { get; set; }

        [DataElement]
        public string CaptureObjectiveText { get; set; }

        [DataElement]
        public string CaptureObjectiveDescription { get; set; }

        [DataElement]
        public string HoldObjectiveText { get; set; }

        [DataElement]
        public string HoldObjectiveDescription { get; set; }

        [DataElement]
        public string CaptureAnnouncement { get; set; }

        [DataElement]
        public byte Realm { get; set; }
    }
}
