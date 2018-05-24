using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public abstract class aConfig
    {
        public bool IConfiguredTheFile=false;
    }
    
    public delegate void ConfigMethod(aConfigAttributes Attributes,aConfig Conf,bool FirstLoad);

    [AttributeUsage(AttributeTargets.Method)]
    public class aConfigMethod : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class aConfigAttributes : Attribute
    {
        protected string _FileName;

        public aConfigAttributes(string FileName)
        {
            _FileName = FileName;
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
        }
    }
}
