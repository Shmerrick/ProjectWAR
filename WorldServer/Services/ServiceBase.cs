using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
