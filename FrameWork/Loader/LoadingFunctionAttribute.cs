using System;

namespace FrameWork
{
    /// <summary>
    /// Attribute set on a service function to register it on server startup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LoadingFunctionAttribute : Attribute
    {
        private bool _immediate;

        public LoadingFunctionAttribute(bool immediate)
        {
            _immediate = immediate;
        }
        
        public bool Immediate { get { return _immediate; } }
    }
}
