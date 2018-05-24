using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "character_client_data", DatabaseName = "Characters")]
    [Serializable]
    public class CharacterClientData : DataObject
    {
        private uint _characterId;
        public byte[] Data = new byte[1024];

        [PrimaryKey]
        public uint CharacterId
        {
            get { return _characterId; }
            set { _characterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string ClientDataString
        {
            get { return Convert.ToBase64String(Data); }
            set
            {
                byte[] dataTemp = Convert.FromBase64String(value);
                if (dataTemp.Length > 0)
                    Buffer.BlockCopy(dataTemp, 0, Data, 0, dataTemp.Length - 1);
            }
        }
    }
}
