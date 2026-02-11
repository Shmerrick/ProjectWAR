using System;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.Quests
{
    public class BasicQuest : AGeneralScript
    {
        protected Object Creature; // This is creature for scripting. Set ID.
        public Random random = new Random();

        protected Point3D spawnPoint;
    }
}