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

        [DataElement(AllowDbNull = false)]
        public ushort RegionId { get; set; }

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
