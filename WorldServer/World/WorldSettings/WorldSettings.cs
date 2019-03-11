using Common;
using WorldServer.Managers;

namespace WorldServer.World.WorldSettings
{
    public class WorldSettings
    {
        World_Settings Settings = WorldMgr.Database.SelectObject<World_Settings>("SettingId = 2");
        //public bool FirstConnect = 
    }
}
