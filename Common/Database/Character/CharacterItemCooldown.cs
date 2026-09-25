using System;
using FrameWork;

namespace Common
{
    [Serializable]
    [DataTable(PreCache = false, TableName = "character_item_cooldowns", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    public class CharacterItemCooldown : DataObject
    {
        private long _expiresAtMilliseconds;

        [PrimaryKey]
        public uint CharacterId { get; set; }

        // Spell IDs occupy 0..65534; group IDs use 65536 + group ID.
        [PrimaryKey]
        public uint CooldownKey { get; set; }

        [DataElement(AllowDbNull = false)]
        public long ExpiresAtMilliseconds
        {
            get { return _expiresAtMilliseconds; }
            set { _expiresAtMilliseconds = value; Dirty = true; }
        }
    }
}
