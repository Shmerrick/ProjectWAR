using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using WorldServer.Services.World;
using WorldServer.World.Positions;

namespace WorldServer.World.Objects
{
    public class SpawnPoint : Point3D
    {
        public ushort ZoneId { get; set; }

        public SpawnPoint(ushort zoneId, int x, int y, int z)
        {
            ZoneId = zoneId;
            X = x;
            Y = y;
            Z = z;
        }

        public SpawnPoint(Zone_Respawn respawn)
        {
            Point3D world;

            if (respawn.InZoneID != 0)
            {
                 world = ZoneService.GetWorldPosition(
                    ZoneService.GetZone_Info(
                        (ushort)respawn.InZoneID), respawn.PinX, respawn.PinY, respawn.PinZ);
                 ZoneId = (ushort) respawn.InZoneID;

            }
            else
            {
                 world = ZoneService.GetWorldPosition(
                    ZoneService.GetZone_Info(
                        (ushort)respawn.ZoneID), respawn.PinX, respawn.PinY, respawn.PinZ);
                 ZoneId = (ushort) respawn.ZoneID;
            }


            
            X = world.X;
            Y = world.Y;
            Z = world.Z;
            
        }

        public  override string ToString()
        {
            return $"RESPAWN : ZoneId={ZoneId},X={X},Y={Y},Z={Z}";
        }

        public Point3D As3DPoint()
        {
            return new Point3D(X,Y,Z);
        }
    }
}
