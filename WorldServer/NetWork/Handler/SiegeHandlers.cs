using FrameWork;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class SiegeHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_UPDATE_SIEGE_LOOK_AT, (int)eClientState.WorldEnter, "onUpdateSiegeLookAt")]
        public static void F_UPDATE_SIEGE_LOOK_AT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            var siegeOid = packet.GetUint16R();
            var heading = packet.GetUint16R();

            var siege = (Creature)cclient.Plr.Region.GetObject(siegeOid);

            if (siege != null && siege.Heading != heading)
            {
                siege.Heading = heading;
                siege.SendState();
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_SIEGE_WEAPON_RESULTS, (int)eClientState.WorldEnter, "onSiegeWeaponResults")]
        public static void F_SIEGE_WEAPON_RESULTS(BaseClient client, PacketIn packet)
        {
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_FIRE_SIEGE_WEAPON, (int)eClientState.WorldEnter, "onFireSiegeWeapon")]
        public static void F_FIRE_SIEGE_WEAPON(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            ushort siegeOid = packet.GetUint16();
            ushort targetID = packet.GetUint16();
            var targetX = packet.GetUint16();
            var targetY = packet.GetUint16();
            var targetZ = packet.GetUint16();
            var targetZoneID = packet.GetUint16();
            var power = packet.GetUint8();   // ram power
            var unk2 = packet.GetUint8();

            var siege = (Creature)cclient.Plr.Region.GetObject(siegeOid);

            if (siege != null)
            {
                if (siege.Spawn.Proto.CreatureSubType == (byte)GameData.CreatureSubTypes.SIEGE_RAM)
                {
                    siege.SiegeInterface.RamSwing(cclient.Plr, targetID, power);
                }
                else
                    siege.SiegeInterface.Fire(cclient.Plr, targetID, targetX, targetY, targetZ, targetZoneID, power);
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_START_SIEGE_MULTIUSER, (int)eClientState.WorldEnter, "onStartSiegeMultiUser")]
        public static void F_START_SIEGE_MULTIUSER(BaseClient client, PacketIn packet)
        {

            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            ushort siegeOid = packet.GetUint16();

            cclient.Plr.CurrentSiege?.SiegeInterface?.SendSiegeMultiuser();
        }
    }
}
