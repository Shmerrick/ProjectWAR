using System;
using System.Collections.Generic;
using System.Text;

namespace FrameWork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleHandlerAttribute : Attribute
    {
        private string _command;
        private int _argcount;
        private string _Description;

        public ConsoleHandlerAttribute(string command,int argcount,string Description)
        {
            _command = command;
            _argcount = argcount;
            _Description = Description;
        }

        public string Command
        {
            get { return _command; }
        }

        public int ArgCount
        {
            get { return _argcount; }
        }

        public string Description
        {
            get { return _Description; }
        }
    }
}
