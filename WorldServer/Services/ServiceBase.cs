using FrameWork;
using WorldServer.Managers;

namespace WorldServer.Services
{
    /// <summary>
    ///     Base class for all world server services providing common utilities
    ///     such as database access and lifecycle hooks for startup and shutdown.
    /// </summary>
    public abstract class ServiceBase
    {
        /// <summary>
        ///     Convenience access to the world database.
        /// </summary>
        protected static IObjectDatabase Database
        {
            get
            {
                return WorldMgr.Database;
            }
        }

        /// <summary>
        ///     Called when the service is starting.
        /// </summary>
        public virtual void Start()
        {
            Log.Info(GetType().Name, "Service starting");
        }

        /// <summary>
        ///     Called when the service is stopping.
        /// </summary>
        public virtual void Stop()
        {
            Log.Info(GetType().Name, "Service stopping");
        }
    }
}