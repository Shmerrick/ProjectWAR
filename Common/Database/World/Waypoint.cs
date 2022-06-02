using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "waypoints", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Waypoint : DataObject
    {
        public static byte Loop = 0;
        public static byte Random = 2;
        public static byte StartToEnd = 1;
        public Waypoint NextWaypoint;
        private uint _CreatureSpawnGUID;
        private byte _EmoteOnEnd;
        private byte _EmoteOnStart;
        private ushort _EquipOnEnd;
        private ushort _EquipOnStart;
        private uint _GameObjectSpawnGUID;
        private uint _GUID;
        private uint _NextWaypointGUID;
        private uint _O;
        private ushort _Speed = 100;
        private string _TextOnEnd;
        private string _TextOnStart;
        private uint _WaitAtEndMS;
        private uint _X;
        private uint _Y;
        private uint _Z;
        [DataElement(AllowDbNull = false)]
        public uint CreatureSpawnGUID
        {
            get { return _CreatureSpawnGUID; }
            set { _CreatureSpawnGUID = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public byte EmoteOnEnd
        {
            get { return _EmoteOnEnd; }
            set { _EmoteOnEnd = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public byte EmoteOnStart
        {
            get { return _EmoteOnStart; }
            set { _EmoteOnStart = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public ushort EquipOnEnd
        {
            get { return _EquipOnEnd; }
            set { _EquipOnEnd = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public ushort EquipOnStart
        {
            get { return _EquipOnStart; }
            set { _EquipOnStart = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint GameObjectSpawnGUID
        {
            get { return _GameObjectSpawnGUID; }
            set { _GameObjectSpawnGUID = value; Dirty = true; }
        }

        [PrimaryKey(AutoIncrement = true)]
        public uint GUID
        {
            get { return _GUID; }
            set { _GUID = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public uint NextWaypointGUID
        {
            get { return _NextWaypointGUID; }
            set { _NextWaypointGUID = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public uint O
        {
            get { return _O; }
            set { _O = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Speed
        {
            get { return _Speed; }
            set { _Speed = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public string TextOnEnd
        {
            get { return _TextOnEnd; }
            set { _TextOnEnd = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public string TextOnStart
        {
            get { return _TextOnStart; }
            set { _TextOnStart = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public uint WaitAtEndMS
        {
            get { return _WaitAtEndMS; }
            set { _WaitAtEndMS = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint X
        {
            get { return _X; }
            set { _X = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Y
        {
            get { return _Y; }
            set { _Y = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Z
        {
            get { return _Z; }
            set { _Z = value; Dirty = true; }
        }
        public override string ToString()
        {
            return GUID + ":" + CreatureSpawnGUID + ":" + GameObjectSpawnGUID + ":" + X + "," + Y + "," + Z + "," + O + "," + Speed + "," + EmoteOnStart + "," + EmoteOnEnd + "," + WaitAtEndMS + "," + EquipOnStart + "," + EquipOnEnd + "," + TextOnStart + "," + TextOnEnd + "," + NextWaypointGUID + "," + NextWaypoint;
        }
    }
}