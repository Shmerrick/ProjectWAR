using FrameWork;
using System;

namespace Common.Database.World.GMCommands
{
    [DataTable(PreCache = false, TableName = "gm_commands", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class GMCommands : DataObject
    {
        [DataElement(AllowDbNull = false)]  // Command name
        public string name { get; set; }
		
		[DataElement(AllowDbNull = false)]  // GM level acsess
        public string security { get; set; }

        [DataElement(AllowDbNull = false)]  // Description of each command
        public string help { get; set; }

    }

}
