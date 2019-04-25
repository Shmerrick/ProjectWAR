using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Scripting;
using Object = WorldServer.World.Objects.Object;


namespace WorldServer
{
    // This assigns script AltdorfSewersWing3Boss to creature with ID 33401
    
    public class CommonFunctions : AGeneralScript
    {
        public void RemoveBuffs(Object Obj)
        {
            Creature c = Obj as Creature;
            //c.IsImmovable = false;
            c.IsInvulnerable = false;
        }

    }
}
