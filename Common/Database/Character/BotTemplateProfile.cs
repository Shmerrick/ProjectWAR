using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "bot_template_profiles", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BotTemplateProfile : DataObject
    {
        private uint _characterId;
        private byte _variantIndex;

        [PrimaryKey]
        public uint CharacterId
        {
            get { return _characterId; }
            set { _characterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte VariantIndex
        {
            get { return _variantIndex; }
            set { _variantIndex = value; Dirty = true; }
        }
    }
}
