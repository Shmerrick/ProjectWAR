using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "creature_spawns", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Creature_spawn : DataObject
    {
        public Creature_proto Proto;

        private uint _Guid;
        private uint _Entry;
        private ushort _ZoneId;
        public int _WorldX;
        public int _WorldY;
        public int _WorldZ;
        public int _WorldO;
        private string _Bytes;
        private byte _Icone;
        private byte _Emote;
        private byte _Faction;
        private byte _Level;
        private uint _Oid;
        private byte _Enabled;

        [PrimaryKey(AutoIncrement = true)]
        public uint Guid
        {
            get { return _Guid; }
            set { _Guid = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Entry
        {
            get { return _Entry; }
            set { _Entry = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId
        {
            get { return _ZoneId; }
            set { _ZoneId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldX
        {
            get { return _WorldX; }
            set { _WorldX = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldY
        {
            get { return _WorldY; }
            set { _WorldY = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldZ
        {
            get { return _WorldZ; }
            set { _WorldZ = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int WorldO
        {
            get { return _WorldO; }
            set { _WorldO = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Icone
        {
            get { return _Icone; }
            set { _Icone = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Emote
        {
            get { return _Emote; }
            set { _Emote = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort RespawnMinutes { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Faction
        {
            get { return _Faction; }
            set { _Faction = value; Dirty = true; }
        }
        [DataElement()]
        public byte WaypointType { get; set; } = 0; // 0 = Loop Start->End->Start, 1 = Start->End, 2 = Random

        [DataElement(AllowDbNull = false)]
        public byte Level
        {
            get { return _Level; }
            set { _Level = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Oid
        {
            get { return _Oid; }
            set { _Oid = value; Dirty = true; }
        }

        public void BuildFromProto(Creature_proto Proto)
        {
            if (Proto == null)
            {
                return;
            }
            this.Proto = Proto;
            Entry = Proto.Entry;
            Emote = Proto.Emote;
            Icone = Proto.Icone;
        }

        [DataElement(AllowDbNull = false)]
        public byte Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; Dirty = true; }
        }

        public byte NoRespawn = 0;
    }
}
