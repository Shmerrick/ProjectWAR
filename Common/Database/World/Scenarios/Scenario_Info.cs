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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "scenario_infos", DatabaseName = "World")]
    [Serializable]
    public class Scenario_Info : DataObject
    {
        public List<Scenario_Object> ScenObjects { get; } = new List<Scenario_Object>();

        [PrimaryKey]
        public ushort ScenarioId { get; set; }

        [DataElement(Varchar = 255)]
        public string Name { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MinLevel { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MaxLevel { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MinPlayers { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte MaxPlayers { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Type { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Tier { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort MapId { get; set; }

        [DataElement]
        public byte KillPointScore { get; set; }

        [DataElement(AllowDbNull = false)]
        public float RewardScaler { get; set; }

        [DataElement]
        public bool DeferKills { get; set; }

        private byte _enabled;

        [DataElement(AllowDbNull = false)]
        public byte Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = false)]
        public int QueueType { get; set; }
    }
}
