using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer
{
    public class BasicQuest : AGeneralScript
    {
        protected Object Obj; // This is creature
        public Random random = new Random();

        protected Point3D spawnPoint;
    }
}
