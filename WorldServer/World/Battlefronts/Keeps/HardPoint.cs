using NLog;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Battlefronts.Keeps
{
    public class Hardpoint : Point3D
    {
        public readonly SiegeType SiegeType;
        public readonly ushort Heading;
        public Siege CurrentWeapon;
        public KeepMessage SiegeRequirement = KeepMessage.Safe;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public Hardpoint(SiegeType type, int x, int y, int z, int heading)
        {
            SiegeType = type;
            X = x;
            Y = y;
            Z = z;
            Heading = (ushort)heading;
        }
    }
}