using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
