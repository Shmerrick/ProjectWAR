using System;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.Quests
{
    public class BasicQuest : AGeneralScript
    {
        protected Object Obj; // This is creature
        public Random random = new Random();

        protected Point3D spawnPoint;
    }
}
