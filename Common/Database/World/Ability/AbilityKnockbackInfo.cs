using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ability_knockback_info", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AbilityKnockbackInfo : DataObject
    {
        [PrimaryKey] public ushort Entry { get; set; }
        [PrimaryKey] public ushort Id { get; set; }
        [DataElement] public byte Angle { get; set; }
        [DataElement] public ushort Power { get; set; }
        [DataElement] public ushort RangeExtension { get; set; }
        [DataElement] public byte GravMultiplier { get; set; }
        [DataElement] public byte Unk { get; set; }
    }
}