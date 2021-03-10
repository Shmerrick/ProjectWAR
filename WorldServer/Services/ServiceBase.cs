using FrameWork;
using WorldServer.Managers;

namespace WorldServer.Services
{
    /// <summary>
    /// TODO work to do here
    /// </summary>
    public abstract class ServiceBase
    {
        protected static IObjectDatabase Database
        {
            get
            {
                return WorldMgr.Database;
            }
        }
    }
}