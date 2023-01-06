using System;

namespace FrameWork
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ReadOnly : Attribute
    {
        public ReadOnly()
        {
        }
    }
}