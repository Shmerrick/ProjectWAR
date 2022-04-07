using WorldServer.World.Objects;
using WorldServer.World.Scripting;
using Object = WorldServer.World.Objects.Object;


namespace WorldServer
{
    // This assigns script AltdorfSewersWing3Boss to creature with ID 33401
    
    public class CommonFunctions : AGeneralScript
    {
        public void RemoveBuffs(Object Creature)
        {
            Creature c = Creature as Creature;
            c.IsImmovable = false;
            c.IsInvulnerable = false;
        }

    }
}
