using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "ability_knockback_info", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AbilityKnockbackInfo : DataObject
    {
        [DataElement] public byte Angle { get; set; }
        [PrimaryKey] public ushort Entry { get; set; }
        [DataElement] public byte GravMultiplier { get; set; }
        [PrimaryKey] public ushort Id { get; set; }
        [DataElement] public ushort Power { get; set; }
        [DataElement] public ushort RangeExtension { get; set; }
        [DataElement] public byte Unk { get; set; }
    }
}