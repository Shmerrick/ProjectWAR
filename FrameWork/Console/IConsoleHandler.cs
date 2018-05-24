using System;
using System.Collections.Generic;
using System.Text;

namespace FrameWork
{
    public interface IConsoleHandler
    {
        bool HandleCommand(string command,List<string> args);
    }
}
