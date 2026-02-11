using System.Collections.Generic;

namespace FrameWork
{
    public interface IConsoleHandler
    {
        bool HandleCommand(string command, List<string> args);
    }
}