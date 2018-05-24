using System;

namespace FrameWork
{
    /// <summary>
    /// Attribute set on a service class to register it on server startup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        private Type[] _dependencies;

        /// <summary>
        /// Setups the type as service.
        /// </summary>
        /// <param name="dependencies">Services that must be loaded prior to this one.</param>
        public ServiceAttribute(params Type[] dependencies)
        {
            _dependencies = dependencies;
        }

        /// <summary>Services that must be loaded prior to this one.</summary>
        public Type[] Dependencies { get { return _dependencies; } }

    }
}
