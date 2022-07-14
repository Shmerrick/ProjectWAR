using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWARAbilityTool.Client.Messages
{
    public interface IOpenAbilityMessage
    {
        string title { get; set; }
        bool isInsertType { get; set; }
    }
}
